using System.Web.Optimization;

namespace Alethic.AspNet.Optimization.Sample;

/// <summary>
/// The site's bundles, one of each kind.
/// </summary>
public static class BundleConfig
{

    /// <summary>
    /// Adds the site's bundles to the collection.
    /// </summary>
    /// <param name="bundles">The application's bundles.</param>
    public static void RegisterBundles(BundleCollection bundles)
    {
        // classic scripts: status.ts uses the function greeting.js declares at its top level
        bundles.Add(new RollupBundle("~/bundle/classic.js", BundleKind.Script)
            .Include("~/Scripts/classic/greeting.js")
            .Include("~/Scripts/classic/status.ts"));

        // ES modules: main.ts imports counter.ts, and neither's names reach the page's globals
        bundles.Add(new RollupBundle("~/bundle/app.js", BundleKind.Module)
            .Include("~/Scripts/app/main.ts"));

        // a stylesheet: site.scss uses the variables and mixins of its partials
        bundles.Add(new RollupBundle("~/bundle/site.css", BundleKind.Style)
            .Include("~/Styles/site.scss"));
    }

}
