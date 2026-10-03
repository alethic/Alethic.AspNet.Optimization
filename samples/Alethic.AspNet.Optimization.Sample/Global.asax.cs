using System;
using System.Configuration;
using System.Web;
using System.Web.Optimization;

namespace Alethic.AspNet.Optimization.Sample;

/// <summary>
/// The site.
/// </summary>
public class Global : HttpApplication
{

    /// <summary>
    /// Registers the bundles, and whether they are optimized where <c>web.config</c> says.
    /// </summary>
    /// <param name="sender">The application.</param>
    /// <param name="e">The event's arguments.</param>
    protected void Application_Start(object sender, EventArgs e)
    {
        if (bool.TryParse(ConfigurationManager.AppSettings["sample:enableOptimizations"], out var enableOptimizations))
            BundleTable.EnableOptimizations = enableOptimizations;

        BundleConfig.RegisterBundles(BundleTable.Bundles);
    }

}
