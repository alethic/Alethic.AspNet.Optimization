using System.Collections.Generic;
using System.IO;

namespace Alethic.AspNet.Optimization.Rollup.Tests;

/// <summary>
/// Files read from disk, recording every file read.
/// </summary>
sealed class DiskToolchainFiles : ToolchainFiles
{

    /// <summary>
    /// Every path read, in order.
    /// </summary>
    public List<string> Reads { get; } = [];

    /// <summary>
    /// Returns whether the file exists.
    /// </summary>
    /// <param name="path">The file's absolute path.</param>
    public override bool Exists(string path) => File.Exists(path);

    /// <summary>
    /// Returns the file's content, or <see langword="null"/> where there is no such file.
    /// </summary>
    /// <param name="path">The file's absolute path.</param>
    public override string? Read(string path)
    {
        Reads.Add(path);
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }

}
