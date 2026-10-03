using System.Web.Optimization;

namespace Alethic.AspNet.Optimization.Sample;

/// <summary>
/// Appends a comment to a bundle: a transform a site adds itself, which runs on the content Rollup built.
/// </summary>
public class FooterTransform : IBundleTransform
{

    /// <summary>
    /// Appends the comment.
    /// </summary>
    /// <param name="context">The bundle's context.</param>
    /// <param name="response">The built bundle.</param>
    public void Process(BundleContext context, BundleResponse response)
    {
        response.Content += "\n/* built by Rollup, then transformed by the site */\n";
    }

}
