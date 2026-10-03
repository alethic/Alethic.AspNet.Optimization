using System.Collections.Generic;

namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// A bundle the toolchain built.
/// </summary>
sealed class ToolchainResult
{

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="code">The bundle.</param>
    /// <param name="map">The bundle's source map, whose sources are absolute paths; <see langword="null"/> if none was asked for.</param>
    /// <param name="watchFiles">Absolute paths of every file the build read, the inputs and what they imported.</param>
    /// <param name="warnings">What the tools warned of.</param>
    public ToolchainResult(string code, string? map, IReadOnlyList<string> watchFiles, IReadOnlyList<string> warnings)
    {
        Code = code;
        Map = map;
        WatchFiles = watchFiles;
        Warnings = warnings;
    }

    /// <summary>
    /// The bundle.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// The bundle's source map, whose sources are absolute paths; <see langword="null"/> if none was asked for.
    /// </summary>
    public string? Map { get; }

    /// <summary>
    /// Absolute paths of every file the build read: the inputs, and the modules and stylesheets they imported.
    /// </summary>
    public IReadOnlyList<string> WatchFiles { get; }

    /// <summary>
    /// What the tools warned of.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; }

}
