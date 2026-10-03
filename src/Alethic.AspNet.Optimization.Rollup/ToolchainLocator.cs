using System;
using System.IO;

namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// Says where the toolchain is: the <c>alethic.aspnet.optimization.rollup</c> folder the build copies into the output.
/// </summary>
/// <remarks>
/// An ASP.NET site's base directory is the site, while its output is the AppDomain's private <c>bin</c>, so that is
/// looked in first; anywhere else the output is the base directory.
/// </remarks>
static class ToolchainLocator
{

    /// <summary>
    /// The folder's name in the output.
    /// </summary>
    public const string FolderName = "alethic.aspnet.optimization.rollup";

    /// <summary>
    /// The toolchain module's file name within the folder.
    /// </summary>
    public const string FileName = "toolchain.cjs";

    /// <summary>
    /// Returns the path of the toolchain module.
    /// </summary>
    /// <exception cref="FileNotFoundException"></exception>
    public static string Locate()
    {
        var domain = AppDomain.CurrentDomain;
        var bin = domain.RelativeSearchPath;

        var candidates = string.IsNullOrEmpty(bin)
            ? new[] { Path.Combine(domain.BaseDirectory, FolderName, FileName) }
            : new[] { Path.Combine(domain.BaseDirectory, bin, FolderName, FileName), Path.Combine(domain.BaseDirectory, FolderName, FileName) };

        foreach (var candidate in candidates)
            if (File.Exists(candidate))
                return candidate;

        throw new FileNotFoundException($"The toolchain was not found at '{candidates[0]}'. The build copies it there from the Alethic.AspNet.Optimization.Rollup package.", candidates[0]);
    }

}
