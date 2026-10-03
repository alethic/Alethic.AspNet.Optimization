using System.Web.Optimization;

namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// A stylesheet built from an entry stylesheet, Sass or CSS: compiled by Sass, which follows its <c>@use</c> and
/// <c>@import</c> rules, then transformed and minified by Lightning CSS.
/// </summary>
public class RollupStyleBundle : RollupEntryBundle
{

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="virtualPath">The bundle's virtual path, such as <c>~/bundle/site.css</c>.</param>
    /// <param name="entryVirtualPath">The virtual path of the entry stylesheet, such as <c>~/Styles/site.scss</c>.</param>
    /// <param name="transforms">Transforms applied to the entry stylesheet's content before it is built, as to any file a
    /// bundle includes.</param>
    public RollupStyleBundle(string virtualPath, string entryVirtualPath, params IItemTransform[] transforms) :
        base(virtualPath, entryVirtualPath, transforms)
    {

    }

    /// <inheritdoc />
    internal override BundleKind Kind => BundleKind.Style;

}
