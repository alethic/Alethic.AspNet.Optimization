using System.Web.Optimization;

namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// ES modules, JavaScript or TypeScript, bundled from an entry module: its imports are followed, what nothing uses is
/// left out, and the result is one immediately invoked function, so no module's names become globals.
/// </summary>
public class RollupModuleBundle : RollupEntryBundle
{

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="virtualPath">The bundle's virtual path, such as <c>~/bundle/app.js</c>.</param>
    /// <param name="entryVirtualPath">The virtual path of the entry module, such as <c>~/Scripts/app/main.ts</c>.</param>
    /// <param name="transforms">Transforms applied to the entry module's content before it is built, as to any file a
    /// bundle includes.</param>
    public RollupModuleBundle(string virtualPath, string entryVirtualPath, params IItemTransform[] transforms) :
        base(virtualPath, entryVirtualPath, transforms)
    {

    }

    /// <inheritdoc />
    internal override BundleKind Kind => BundleKind.Module;

}
