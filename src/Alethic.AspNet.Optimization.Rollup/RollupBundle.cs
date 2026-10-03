using System.Web.Optimization;

namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// A bundle the toolchain builds, with Rollup and the tools it runs: <see cref="RollupScriptBundle"/>,
/// <see cref="RollupModuleBundle"/> or <see cref="RollupStyleBundle"/>.
/// </summary>
/// <remarks>
/// Files are read from disk, so each must be one a virtual path maps to. Whether the output is minified and carries a
/// source map follows <see cref="BundleTable.EnableOptimizations"/> unless <see cref="Minify"/> and
/// <see cref="SourceMap"/> say otherwise.
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
        Builder = NullBundleBuilder.Instance;
        Orderer = IncludedOrderBundleOrderer.Instance;
        Transforms.Add(new RollupBundleTransform(this));
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

}
