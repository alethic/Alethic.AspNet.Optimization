using System.Web.Optimization;

namespace Alethic.AspNet.Optimization;

/// <summary>
/// A bundle the toolchain builds: its files in the order they were included, combined as <see cref="Kind"/> says, by
/// Rollup and the tools it runs.
/// </summary>
/// <remarks>
/// Files are read from disk, so each must be one a virtual path maps to. Whether the output is minified and carries a
/// source map follows <see cref="BundleTable.EnableOptimizations"/> unless <see cref="Minify"/> and
/// <see cref="SourceMap"/> say otherwise.
/// </remarks>
public class RollupBundle : Bundle
{

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="virtualPath">The bundle's virtual path, such as <c>~/bundle/site.js</c>.</param>
    /// <param name="kind">How the bundle's files combine.</param>
    /// <remarks>It builds with <see cref="Toolchain.Default"/>, looked up the first time it is built.</remarks>
    public RollupBundle(string virtualPath, BundleKind kind) :
        base(virtualPath)
    {
        Kind = kind;
        Builder = NullBundleBuilder.Instance;
        Orderer = IncludedOrderBundleOrderer.Instance;
        Transforms.Add(new RollupBundleTransform(this, null));
    }

    /// <summary>
    /// Initializes a new instance that builds with the given toolchain.
    /// </summary>
    /// <param name="virtualPath">The bundle's virtual path, such as <c>~/bundle/site.js</c>.</param>
    /// <param name="kind">How the bundle's files combine.</param>
    /// <param name="toolchain">The toolchain to build with.</param>
    public RollupBundle(string virtualPath, BundleKind kind, Toolchain toolchain) :
        this(virtualPath, kind)
    {
        Transforms.Clear();
        Transforms.Add(new RollupBundleTransform(this, toolchain ?? throw new System.ArgumentNullException(nameof(toolchain))));
    }

    /// <summary>
    /// How the bundle's files combine.
    /// </summary>
    public BundleKind Kind { get; }

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
