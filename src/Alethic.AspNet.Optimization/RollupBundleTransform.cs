using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web;
using System.Web.Optimization;

namespace Alethic.AspNet.Optimization;

/// <summary>
/// Builds a <see cref="RollupBundle"/> with the toolchain, in place of the content System.Web.Optimization would join.
/// </summary>
/// <remarks>
/// The response's files become every file the build read, the imported modules and Sass partials included, so the
/// cached bundle is invalidated by a change to any of them and not only to the files the bundle names.
/// </remarks>
sealed class RollupBundleTransform : IBundleTransform
{

    readonly RollupBundle _bundle;
    readonly Toolchain? _toolchain;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="bundle">The bundle this builds.</param>
    /// <param name="toolchain">The toolchain to build with, or <see langword="null"/> for <see cref="Toolchain.Default"/>.</param>
    public RollupBundleTransform(RollupBundle bundle, Toolchain? toolchain)
    {
        _bundle = bundle;
        _toolchain = toolchain;
    }

    /// <summary>
    /// Builds the bundle into the response.
    /// </summary>
    /// <param name="context">The bundle's context.</param>
    /// <param name="response">The response, whose files are the bundle's, in order.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Process(BundleContext context, BundleResponse response)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (response is null)
            throw new ArgumentNullException(nameof(response));

        var minify = _bundle.Minify ?? (context.EnableOptimizations && context.EnableInstrumentation == false);
        var sourceMap = _bundle.SourceMap ?? context.EnableOptimizations == false;

        var inputs = response.Files.Select(f => AppPaths.ToPhysical(f.IncludedVirtualPath)).ToList();
        var request = new ToolchainRequest(_bundle.Kind, inputs, VirtualPathUtility.GetFileName(context.BundleVirtualPath)) { Minify = minify, SourceMap = sourceMap };

        // off the request's thread, whose synchronization context the build's continuations must not wait for
        var toolchain = _toolchain ?? Toolchain.Default;
        var result = Task.Run(() => toolchain.BuildAsync(request)).GetAwaiter().GetResult();

        foreach (var warning in result.Warnings)
            Trace.TraceWarning("{0}: {1}", context.BundleVirtualPath, warning);

        response.Content = result.Map is string map ? AppendSourceMap(result.Code, map) : result.Code;
        response.ContentType = _bundle.Kind == BundleKind.Style ? "text/css" : "text/javascript";
        response.Files = ToBundleFiles(context, result.WatchFiles);

        // a bundle served for debugging changes with every edit, so no browser keeps it
        if (context.EnableOptimizations == false)
            response.Cacheability = HttpCacheability.NoCache;
    }

    /// <summary>
    /// Appends a source map to the code, inline, its sources made into the URLs the browser loads them from.
    /// </summary>
    /// <param name="code">The bundle.</param>
    /// <param name="map">The bundle's source map, whose sources are absolute paths.</param>
    string AppendSourceMap(string code, string map)
    {
        var json = JsonNode.Parse(map)!.AsObject();
        if (json["sources"] is JsonArray sources)
            for (var i = 0; i < sources.Count; i++)
                if (sources[i]?.GetValue<string>() is string source && source.Length > 0)
                    sources[i] = AppPaths.ToUrl(source);

        var data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json.ToJsonString()));
        var url = "data:application/json;charset=utf-8;base64," + data;

        return _bundle.Kind == BundleKind.Style
            ? code + "\n/*# sourceMappingURL=" + url + " */\n"
            : code + "\n//# sourceMappingURL=" + url + "\n";
    }

    /// <summary>
    /// Returns the files the build read, as the bundle files System.Web.Optimization makes its cache dependency from.
    /// Files outside the application are left out: no virtual path names them.
    /// </summary>
    /// <param name="context">The bundle's context.</param>
    /// <param name="watchFiles">Absolute paths of the files the build read.</param>
    static List<BundleFile> ToBundleFiles(BundleContext context, IReadOnlyList<string> watchFiles)
    {
        var provider = BundleTable.VirtualPathProvider;
        var files = new List<BundleFile>();
        foreach (var watchFile in watchFiles)
            if (AppPaths.ToVirtual(watchFile) is string virtualPath && provider.FileExists(virtualPath))
                files.Add(new BundleFile(virtualPath, provider.GetFile(virtualPath)));

        return files;
    }

}
