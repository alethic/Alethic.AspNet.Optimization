using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web;
using System.Web.Optimization;

namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// Builds a <see cref="RollupBundle"/>'s content with Rollup, in place of the content System.Web.Optimization would
/// join: the files read and transformed, combined, and minified where the bundle says, with the source map inline.
/// </summary>
/// <remarks>
/// The build also says which files it read, imported modules and Sass partials included, which a builder has no way to
/// return; the bundle takes them from <see cref="TakeResult"/> for the response it makes.
/// </remarks>
sealed class RollupBundleBuilder : IBundleBuilder
{

    /// <summary>
    /// The one instance.
    /// </summary>
    public static readonly RollupBundleBuilder Instance = new();

    static readonly ConditionalWeakTable<BundleContext, ToolchainResult> _results = new();

    /// <summary>
    /// Returns, and forgets, the result of the build made in the context, or <see langword="null"/> where there was none.
    /// </summary>
    /// <param name="context">The bundle's context.</param>
    public static ToolchainResult? TakeResult(BundleContext context)
    {
        if (_results.TryGetValue(context, out var result) == false)
            return null;

        _results.Remove(context);
        return result;
    }

    /// <summary>
    /// Returns what the bundle's classic scripts are joined with: its <see cref="Bundle.ConcatenationToken"/>, or a
    /// line break where it has none, as System.Web.Optimization joins files, ending with a line break either way.
    /// </summary>
    /// <param name="bundle">The bundle.</param>
    static string Separator(Bundle bundle)
    {
        var token = string.IsNullOrEmpty(bundle.ConcatenationToken) ? "\n" : bundle.ConcatenationToken.Replace("\r\n", "\n");
        return token.EndsWith("\n", StringComparison.Ordinal) ? token : token + "\n";
    }

    /// <summary>
    /// Appends a source map to the code, inline, its sources made into the URLs the browser loads them from.
    /// </summary>
    /// <param name="kind">The bundle's kind, which says how it writes a comment.</param>
    /// <param name="code">The bundle.</param>
    /// <param name="map">The bundle's source map, whose sources are absolute paths.</param>
    static string AppendSourceMap(BundleKind kind, string code, string map)
    {
        var json = JsonNode.Parse(map)!.AsObject();
        if (json["sources"] is JsonArray sources)
            for (var i = 0; i < sources.Count; i++)
                if (sources[i]?.GetValue<string>() is string source && source.Length > 0)
                    sources[i] = AppPaths.ToUrl(source);

        var data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json.ToJsonString()));
        var url = "data:application/json;charset=utf-8;base64," + data;

        return kind == BundleKind.Style
            ? code + "\n/*# sourceMappingURL=" + url + " */\n"
            : code + "\n//# sourceMappingURL=" + url + "\n";
    }

    /// <summary>
    /// Builds the bundle's content.
    /// </summary>
    /// <param name="bundle">The bundle, which must be a <see cref="RollupBundle"/>.</param>
    /// <param name="context">The bundle's context.</param>
    /// <param name="files">The bundle's files, in order.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"><paramref name="bundle"/> is not a <see cref="RollupBundle"/>.</exception>
    public string BuildBundleContent(Bundle bundle, BundleContext context, IEnumerable<BundleFile> files)
    {
        if (bundle is null)
            throw new ArgumentNullException(nameof(bundle));
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (files is null)
            throw new ArgumentNullException(nameof(files));
        if (bundle is not RollupBundle rollup)
            throw new ArgumentException($"The bundle '{bundle.Path}' is not a {nameof(RollupBundle)}.", nameof(bundle));

        var included = files.ToList();
        var section = RollupSection.Current();
        var minify = rollup.Minify ?? section.MinifyValue ?? (context.EnableOptimizations && context.EnableInstrumentation == false);
        var sourceMap = rollup.SourceMap ?? section.SourceMapValue ?? context.EnableOptimizations == false;

        // the inputs are named by the paths their virtual paths map to, and read through System.Web.Optimization
        var inputs = included.Select(f => AppPaths.ToPhysical(f.IncludedVirtualPath)).ToList();
        var request = new ToolchainRequest(rollup.Kind, inputs, VirtualPathUtility.GetFileName(context.BundleVirtualPath), new BundleToolchainFiles(included))
        {
            Minify = minify,
            SourceMap = sourceMap,
            Separator = Separator(rollup),
            Targets = rollup.Targets ?? section.TargetsValue,
        };

        // off the request's thread, whose synchronization context the build's continuations must not wait for
        var result = Task.Run(() => Toolchain.ForApplication().BuildAsync(request)).GetAwaiter().GetResult();
        _results.Remove(context);
        _results.Add(context, result);

        foreach (var warning in result.Warnings)
            Trace.TraceWarning("{0}: {1}", context.BundleVirtualPath, warning);

        return result.Map is string map ? AppendSourceMap(rollup.Kind, result.Code, map) : result.Code;
    }

}
