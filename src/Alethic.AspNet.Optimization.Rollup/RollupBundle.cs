using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Optimization;

namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// A bundle Rollup builds, with the tools it runs: <see cref="RollupScriptBundle"/>, <see cref="RollupModuleBundle"/>
/// or <see cref="RollupStyleBundle"/>.
/// </summary>
/// <remarks>
/// <para>
/// It is a System.Web.Optimization bundle in every way but its builder, which runs the whole Rollup build,
/// minification included. So a bundle needs no transforms; any added to <see cref="Bundle.Transforms"/> run after it,
/// on the built content, as they would on any bundle's. One that moves content around leaves the inline source map
/// pointing at the wrong places.
/// </para>
/// <para>
/// Whether the output is minified and carries a source map follows <see cref="BundleTable.EnableOptimizations"/>
/// unless <see cref="Minify"/> and <see cref="SourceMap"/> say otherwise.
/// </para>
/// </remarks>
public abstract class RollupBundle : Bundle
{

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="virtualPath">The bundle's virtual path, such as <c>~/bundle/site.js</c>.</param>
    private protected RollupBundle(string virtualPath) :
        base(virtualPath)
    {
        Builder = RollupBundleBuilder.Instance;
    }

    /// <summary>
    /// How the bundle's files combine.
    /// </summary>
    internal abstract BundleKind Kind { get; }

    /// <summary>
    /// Whether to minify the bundle; <see langword="null"/>, the default, to minify where optimizations are enabled.
    /// </summary>
    public bool? Minify { get; set; }

    /// <summary>
    /// Whether to append the bundle's source map to it, inline; <see langword="null"/>, the default, to do so where
    /// optimizations are disabled.
    /// </summary>
    public bool? SourceMap { get; set; }

    /// <summary>
    /// Makes the response for the built content, then runs the bundle's transforms on it.
    /// </summary>
    /// <param name="context">The bundle's context.</param>
    /// <param name="bundleContent">The content the builder built.</param>
    /// <param name="bundleFiles">The files the bundle includes.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public override BundleResponse ApplyTransforms(BundleContext context, string bundleContent, IEnumerable<BundleFile> bundleFiles)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        var dependencies = new List<string>();
        if (RollupBundleBuilder.TakeResult(context) is ToolchainResult result)
            foreach (var watchFile in result.WatchFiles)
                if (AppPaths.ToVirtual(watchFile) is string virtualPath)
                    dependencies.Add(virtualPath);

        var response = new RollupBundleResponse(bundleContent, bundleFiles, dependencies)
        {
            ContentType = Kind == BundleKind.Style ? "text/css" : "text/javascript",
        };

        // a bundle served for debugging changes with every edit, so no browser keeps it
        if (context.EnableOptimizations == false)
            response.Cacheability = HttpCacheability.NoCache;

        foreach (var transform in Transforms)
            transform.Process(context, response);

        return response;
    }

    /// <summary>
    /// Returns the cached response, unless a file its build read has changed since.
    /// </summary>
    /// <param name="context">The bundle's context.</param>
    public override BundleResponse? CacheLookup(BundleContext context)
    {
        var response = base.CacheLookup(context);
        return response is RollupBundleResponse built && built.IsCurrent(BundleTable.VirtualPathProvider) == false ? null : response;
    }

}
