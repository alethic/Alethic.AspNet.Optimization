using System;
using System.Collections.Generic;
using System.Web.Optimization;

namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// Lists a <see cref="RollupBundle"/>'s contents as the bundle itself, and every other bundle's as the resolver it
/// wraps does.
/// </summary>
/// <remarks>
/// Where optimizations are disabled, <c>Scripts.Render</c> and <c>Styles.Render</c> render a tag for each of a
/// bundle's files rather than one for the bundle. A browser cannot run a TypeScript file or a Sass stylesheet, nor
/// scripts that import modules by their source paths, so a <see cref="RollupBundle"/> is rendered as itself instead:
/// built without minification, and with its source map, which shows the browser's tools each file as it was written.
/// </remarks>
public sealed class RollupBundleResolver : IBundleResolver
{

    /// <summary>
    /// Makes this the current resolver, wrapping the one there is, unless it already is.
    /// </summary>
    public static void Install()
    {
        if (BundleResolver.Current is not RollupBundleResolver)
            BundleResolver.Current = new RollupBundleResolver(BundleResolver.Current);
    }

    readonly IBundleResolver _inner;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="inner">The resolver for every bundle that is not a <see cref="RollupBundle"/>.</param>
    public RollupBundleResolver(IBundleResolver inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    /// <summary>
    /// Returns whether the virtual path names a bundle.
    /// </summary>
    /// <param name="virtualPath">The virtual path.</param>
    public bool IsBundleVirtualPath(string virtualPath) => _inner.IsBundleVirtualPath(virtualPath);

    /// <summary>
    /// Returns the virtual paths to render for a bundle where optimizations are disabled: the bundle's own, for a
    /// <see cref="RollupBundle"/>.
    /// </summary>
    /// <param name="virtualPath">The bundle's virtual path.</param>
    public IEnumerable<string> GetBundleContents(string virtualPath)
    {
        return BundleTable.Bundles.GetBundleFor(virtualPath) is RollupBundle bundle ? [bundle.Path] : _inner.GetBundleContents(virtualPath);
    }

    /// <summary>
    /// Returns the URL of a bundle, versioned by its content.
    /// </summary>
    /// <param name="virtualPath">The bundle's virtual path.</param>
    public string GetBundleUrl(string virtualPath) => _inner.GetBundleUrl(virtualPath);

}
