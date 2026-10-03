namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// The files a build reads, named by absolute path. The toolchain reads nothing from disk itself; Rollup and Sass ask
/// for every file through this.
/// </summary>
abstract class ToolchainFiles
{

    /// <summary>
    /// Returns whether the file exists.
    /// </summary>
    /// <param name="path">The file's absolute path.</param>
    public abstract bool Exists(string path);

    /// <summary>
    /// Returns the file's content, or <see langword="null"/> where there is no such file.
    /// </summary>
    /// <param name="path">The file's absolute path.</param>
    public abstract string? Read(string path);

}
