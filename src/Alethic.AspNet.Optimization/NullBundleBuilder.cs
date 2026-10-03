using System.Collections.Generic;
using System.Web.Optimization;

namespace Alethic.AspNet.Optimization;

/// <summary>
/// Builds no content: a <see cref="RollupBundle"/>'s transform reads its files itself.
/// </summary>
sealed class NullBundleBuilder : IBundleBuilder
{

    /// <summary>
    /// The one instance.
    /// </summary>
    public static readonly NullBundleBuilder Instance = new();

    /// <summary>
    /// Returns the empty string.
    /// </summary>
    /// <param name="bundle">The bundle.</param>
    /// <param name="context">The bundle's context.</param>
    /// <param name="files">The bundle's files.</param>
    public string BuildBundleContent(Bundle bundle, BundleContext context, IEnumerable<BundleFile> files) => string.Empty;

}
