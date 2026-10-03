namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// Classic scripts, JavaScript or TypeScript, joined in the order they are included into one script. Their top-level
/// names are globals, as they would be if each file were loaded by a <c>&lt;script&gt;</c> tag of its own, so a file
/// uses what the files before it declare.
/// </summary>
public class RollupScriptBundle : RollupBundle
{

    /// <summary>
    /// Initializes a new instance. Add the scripts with <c>Include</c>, in the order they run.
    /// </summary>
    /// <param name="virtualPath">The bundle's virtual path, such as <c>~/bundle/site.js</c>.</param>
    public RollupScriptBundle(string virtualPath) :
        base(virtualPath)
    {

    }

    /// <inheritdoc />
    internal override BundleKind Kind => BundleKind.Script;

}
