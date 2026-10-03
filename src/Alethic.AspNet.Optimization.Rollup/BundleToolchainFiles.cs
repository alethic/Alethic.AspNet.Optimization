using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using System.Web.Optimization;

namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// A bundle's files as System.Web.Optimization provides them: a file the bundle includes is its <see cref="BundleFile"/>,
/// with its item transforms applied, and any other, such as a module or partial one imports, comes from
/// <see cref="BundleTable.VirtualPathProvider"/>.
/// </summary>
/// <remarks>
/// The toolchain names files by the path a virtual path maps to, which need not exist on disk: a provider may serve the
/// file from anywhere.
/// </remarks>
sealed class BundleToolchainFiles : ToolchainFiles
{

    readonly Dictionary<string, BundleFile> _included = new(StringComparer.OrdinalIgnoreCase);
    readonly VirtualPathProvider _provider;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="included">The files the bundle includes.</param>
    public BundleToolchainFiles(IEnumerable<BundleFile> included)
    {
        foreach (var file in included)
            _included[AppPaths.ToPhysical(file.IncludedVirtualPath)] = file;

        _provider = BundleTable.VirtualPathProvider;
    }

    /// <summary>
    /// Returns whether the file exists.
    /// </summary>
    /// <param name="path">The file's absolute path.</param>
    public override bool Exists(string path)
    {
        return _included.ContainsKey(path) || AppPaths.ToVirtual(path) is string virtualPath && _provider.FileExists(virtualPath);
    }

    /// <summary>
    /// Returns the file's content, or <see langword="null"/> where there is no such file.
    /// </summary>
    /// <param name="path">The file's absolute path.</param>
    public override string? Read(string path)
    {
        if (_included.TryGetValue(path, out var file))
            return file.ApplyTransforms();

        if (AppPaths.ToVirtual(path) is not string virtualPath || _provider.FileExists(virtualPath) == false)
            return null;

        using var stream = _provider.GetFile(virtualPath).Open();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

}
