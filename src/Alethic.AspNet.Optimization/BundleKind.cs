namespace Alethic.AspNet.Optimization;

/// <summary>
/// How a bundle's files combine.
/// </summary>
public enum BundleKind
{

    /// <summary>
    /// Classic scripts, joined in order into one script whose top-level names are globals, as the files' would be if
    /// each were loaded by a script tag of its own.
    /// </summary>
    Script,

    /// <summary>
    /// ES modules, imported in order for their effects and bundled into one immediately invoked function.
    /// </summary>
    Module,

    /// <summary>
    /// Stylesheets, CSS or Sass, compiled and joined in order.
    /// </summary>
    Style,

}
