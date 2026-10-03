using System.Web;

[assembly: PreApplicationStartMethod(typeof(Alethic.AspNet.Optimization.Rollup.PreApplicationStart), nameof(Alethic.AspNet.Optimization.Rollup.PreApplicationStart.Start))]

namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// Runs as the application starts, before <c>Application_Start</c>.
/// </summary>
public static class PreApplicationStart
{

    /// <summary>
    /// Installs the <see cref="RollupBundleResolver"/>.
    /// </summary>
    public static void Start()
    {
        RollupBundleResolver.Install();
    }

}
