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
/// Its builder runs the whole build, minification included, so a bundle needs no transforms; any added to
/// <see cref="Bundle.Transforms"/> run after it, on the built content, as they would on any bundle's. One that moves
/// content around leaves the inline source map pointing at the wrong places.
/// </para>
/// <para>
/// Whether the output is minified and carries a source map follows <see cref="BundleTable.EnableOptimizations"/>
/// unless <see cref="Minify"/> and <see cref="SourceMap"/> say otherwise.
/// </para>
/// </remarks>
public abstract class RollupBundle : Bundle
{

    /// <summary>
    /// Returns the files a build read, as the bundle files System.Web.Optimization makes its cache dependency from.
    /// Files outside the application are left out: no virtual path names them.
    /// </summary>
    /// <param name="watchFiles">Absolute paths of the files the build read.</param>
    static List<BundleFile> ToBundleFiles(IReadOnlyList<string> watchFiles)
    {
        var provider = BundleTable.VirtualPathProvider;
        var files = new List<BundleFile>();
        foreach (var watchFile in watchFiles)
            if (AppPaths.ToVirtual(watchFile) is string virtualPath && provider.FileExists(virtualPath))
                files.Add(new BundleFile(virtualPath, provider.GetFile(virtualPath)));

        return files;
    }

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="virtualPath">The bundle's virtual path, such as <c>~/bundle/site.js</c>.</param>
    private protected RollupBundle(string virtualPath) :
        base(virtualPath)
    {
        Builder = RollupBundleBuilder.Instance;
        Orderer = IncludedOrderBundleOrderer.Instance;
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
    /// <remarks>
    /// The response's files are every file the build read, the imported modules and Sass partials included, so the
    /// cached bundle is invalidated by a change to any of them and not only to the files the bundle includes.
    /// </remarks>
    /// <param name="context">The bundle's context.</param>
    /// <param name="bundleContent">The content the builder built.</param>
    /// <param name="bundleFiles">The files the bundle includes.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public override BundleResponse ApplyTransforms(BundleContext context, string bundleContent, IEnumerable<BundleFile> bundleFiles)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        var response = new BundleResponse(bundleContent, bundleFiles)
        {
            ContentType = Kind == BundleKind.Style ? "text/css" : "text/javascript",
        };

        if (RollupBundleBuilder.TakeResult(context) is ToolchainResult result)
            response.Files = ToBundleFiles(result.WatchFiles);

        // a bundle served for debugging changes with every edit, so no browser keeps it
        if (context.EnableOptimizations == false)
            response.Cacheability = HttpCacheability.NoCache;

        foreach (var transform in Transforms)
            transform.Process(context, response);

        return response;
    }

}
