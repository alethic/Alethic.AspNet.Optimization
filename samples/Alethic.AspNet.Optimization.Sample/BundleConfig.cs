using System.Web.Optimization;

using Alethic.AspNet.Optimization.Rollup;

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
        // classic scripts, in the order they run: status.ts uses the function greeting.js declares at its top level
        var classic = new RollupScriptBundle("~/bundle/classic.js")
            .Include("~/Scripts/classic/greeting.js")
            .Include("~/Scripts/classic/status.ts");

        // the site's own transforms run after Rollup's build, as on any bundle
        classic.Transforms.Add(new FooterTransform());
        bundles.Add(classic);

        // ES modules from their entry: main.ts imports counter.ts, and neither's names reach the page's globals
        bundles.Add(new RollupModuleBundle("~/bundle/app.js", "~/Scripts/app/main.ts"));

        // a stylesheet from its entry: site.scss uses the variables and mixins of its partials
        bundles.Add(new RollupStyleBundle("~/bundle/site.css", "~/Styles/site.scss"));
    }

}
