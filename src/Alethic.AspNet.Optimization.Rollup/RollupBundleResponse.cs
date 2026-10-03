using System.Collections.Generic;
using System.Web.Hosting;
using System.Web.Optimization;

namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// A built <see cref="RollupBundle"/>: a <see cref="BundleResponse"/> that also knows every file the build read and
/// what they were when it read them, so a cached copy can tell whether it is still current.
/// </summary>
/// <remarks>
/// System.Web.Optimization's own cache dependency covers the files the bundle includes, and relies on ASP.NET's file
/// change notifications, which a site running Node turns off. The dependencies here include the modules and
/// partials those files import, and are checked by the virtual path provider's file hash, which needs no
/// notifications.
/// </remarks>
sealed class RollupBundleResponse : BundleResponse
{

    /// <summary>
    /// Returns the provider's hash of the files, or <see langword="null"/> where it computes none.
    /// </summary>
    /// <param name="provider">The virtual path provider.</param>
    /// <param name="dependencies">Virtual paths of the files.</param>
    static string? Hash(VirtualPathProvider provider, IReadOnlyList<string> dependencies)
    {
        return dependencies.Count == 0 ? null : provider.GetFileHash(dependencies[0], dependencies);
    }

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="content">The built content.</param>
    /// <param name="files">The files the bundle includes.</param>
    /// <param name="dependencies">Virtual paths of every file the build read.</param>
    public RollupBundleResponse(string content, IEnumerable<BundleFile> files, IReadOnlyList<string> dependencies) :
        base(content, files)
    {
        Dependencies = dependencies;
        DependencyHash = Hash(BundleTable.VirtualPathProvider, dependencies);
    }

    /// <summary>
    /// Virtual paths of every file the build read.
    /// </summary>
    public IReadOnlyList<string> Dependencies { get; }

    /// <summary>
    /// The virtual path provider's hash of the dependencies when they were read, or <see langword="null"/> where the
    /// provider computes none.
    /// </summary>
    public string? DependencyHash { get; }

    /// <summary>
    /// Returns whether every dependency is as it was when the build read it. Where the provider computes no hash, that
    /// cannot be told, and the response is taken to be current.
    /// </summary>
    /// <param name="provider">The virtual path provider the dependencies come from.</param>
    public bool IsCurrent(VirtualPathProvider provider)
    {
        return DependencyHash is null || DependencyHash == Hash(provider, Dependencies);
    }

}
