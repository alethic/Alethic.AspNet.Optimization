using System;

namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// A bundle built from one entry file, which names everything else it needs: <see cref="RollupModuleBundle"/> and
/// <see cref="RollupStyleBundle"/>. Its other files are found by following the entry; a bundle given more fails to
/// build.
/// </summary>
public abstract class RollupEntryBundle : RollupBundle
{

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="virtualPath">The bundle's virtual path.</param>
    /// <param name="entryVirtualPath">The virtual path of the entry file.</param>
    private protected RollupEntryBundle(string virtualPath, string entryVirtualPath) :
        base(virtualPath)
    {
        if (string.IsNullOrEmpty(entryVirtualPath))
            throw new ArgumentException("The entry's virtual path is required.", nameof(entryVirtualPath));

        EntryVirtualPath = entryVirtualPath;
        Include(entryVirtualPath);
    }

    /// <summary>
    /// The virtual path of the entry file.
    /// </summary>
    public string EntryVirtualPath { get; }

}
