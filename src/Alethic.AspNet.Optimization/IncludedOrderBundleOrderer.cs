using System.Collections.Generic;
using System.Web.Optimization;

namespace Alethic.AspNet.Optimization;

/// <summary>
/// Keeps a bundle's files in the order they were included, where the default orderer moves files it recognizes, such
/// as jQuery, to the front.
/// </summary>
sealed class IncludedOrderBundleOrderer : IBundleOrderer
{

    /// <summary>
    /// The one instance.
    /// </summary>
    public static readonly IncludedOrderBundleOrderer Instance = new();

    /// <summary>
    /// Returns the files as they are.
    /// </summary>
    /// <param name="context">The bundle's context.</param>
    /// <param name="files">The bundle's files, in the order they were included.</param>
    public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files) => files;

}
