using System;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// Converts between the application's virtual paths and the files they name on disk.
/// </summary>
static class AppPaths
{

    /// <summary>
    /// Returns the file a virtual path names.
    /// </summary>
    /// <param name="virtualPath">An application-relative or absolute virtual path.</param>
    /// <exception cref="InvalidOperationException">The path names no file on disk.</exception>
    public static string ToPhysical(string virtualPath)
    {
        return HostingEnvironment.MapPath(virtualPath) ?? throw new InvalidOperationException($"'{virtualPath}' names no file on disk. Bundles built by the toolchain read their files from disk.");
    }

    /// <summary>
    /// Returns the application-relative virtual path of a file, or <see langword="null"/> where the file is outside the
    /// application.
    /// </summary>
    /// <param name="physicalPath">An absolute path.</param>
    public static string? ToVirtual(string physicalPath)
    {
        var root = HttpRuntime.AppDomainAppPath;
        if (string.IsNullOrEmpty(root))
            return null;

        var full = Path.GetFullPath(physicalPath);
        if (full.StartsWith(root, StringComparison.OrdinalIgnoreCase) == false)
            return null;

        return "~/" + full.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar).Replace(Path.DirectorySeparatorChar, '/');
    }

    /// <summary>
    /// Returns the URL a browser loads a file from: its absolute virtual path where the file is in the application, a
    /// <c>file:</c> URL where it is elsewhere on disk, and the path as it is where it is not absolute.
    /// </summary>
    /// <param name="path">A path.</param>
    public static string ToUrl(string path)
    {
        if (Path.IsPathRooted(path) == false)
            return path;

        return ToVirtual(path) is string virtualPath ? VirtualPathUtility.ToAbsolute(virtualPath) : new Uri(path).AbsoluteUri;
    }

}
