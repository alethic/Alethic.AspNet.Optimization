using System;
using System.Collections.Generic;

namespace Alethic.AspNet.Optimization.Rollup.Tests;

/// <summary>
/// Files held in memory, as a virtual path provider might serve files that are on no disk.
/// </summary>
sealed class MemoryToolchainFiles : ToolchainFiles
{

    readonly Dictionary<string, string> _files = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Sets a file's content.
    /// </summary>
    /// <param name="path">The file's absolute path.</param>
    public string this[string path]
    {
        set => _files[path] = value;
    }

    /// <summary>
    /// Returns whether the file exists.
    /// </summary>
    /// <param name="path">The file's absolute path.</param>
    public override bool Exists(string path) => _files.ContainsKey(path);

    /// <summary>
    /// Returns the file's content, or <see langword="null"/> where there is no such file.
    /// </summary>
    /// <param name="path">The file's absolute path.</param>
    public override string? Read(string path) => _files.TryGetValue(path, out var content) ? content : null;

}
