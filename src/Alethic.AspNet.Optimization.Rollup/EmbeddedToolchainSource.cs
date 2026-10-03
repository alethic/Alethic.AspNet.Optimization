using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

using Alethic.Node;

namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// The toolchain, embedded in this assembly: the module and the WebAssembly binaries its tools read from beside it.
/// Node loads modules from files, so the first time the module is asked for, the resources are written out, to the
/// application's own temporary files where it has them and to the user's temporary directory where it has not.
/// </summary>
/// <remarks>
/// The files are written to a directory named for this build of the assembly, so a newer build never loads an older
/// toolchain. Each file is written beside its final name and moved into place, so a process that finds a file there
/// finds it whole.
/// </remarks>
sealed class EmbeddedToolchainSource : NodeModuleSource
{

    /// <summary>
    /// The prefix of the toolchain's resource names; the rest of each name is the file's.
    /// </summary>
    public const string ResourcePrefix = "Alethic.AspNet.Optimization.Rollup.toolchain.";

    /// <summary>
    /// The module's file name.
    /// </summary>
    public const string ModuleFileName = "toolchain.cjs";

    /// <summary>
    /// The one instance.
    /// </summary>
    public static readonly EmbeddedToolchainSource Instance = new();

    /// <summary>
    /// Writes every toolchain resource out to the directory for this build of the assembly, where not already there,
    /// and returns the module's path.
    /// </summary>
    static string Extract()
    {
        var assembly = typeof(EmbeddedToolchainSource).Assembly;
        var directory = Path.Combine(Root(), "alethic.aspnet.optimization.rollup", assembly.ManifestModule.ModuleVersionId.ToString("N"));
        Directory.CreateDirectory(directory);

        foreach (var name in assembly.GetManifestResourceNames())
            if (name.StartsWith(ResourcePrefix, StringComparison.Ordinal))
                Write(assembly, name, Path.Combine(directory, name.Substring(ResourcePrefix.Length)));

        var module = Path.Combine(directory, ModuleFileName);
        return File.Exists(module) ? module : throw new FileNotFoundException($"The toolchain module is not embedded in '{assembly.FullName}'.", module);
    }

    /// <summary>
    /// Returns the directory the toolchain is written under: the application's temporary files where it is hosted by
    /// ASP.NET, and the user's temporary directory otherwise.
    /// </summary>
    static string Root()
    {
        return HostingEnvironment.IsHosted && string.IsNullOrEmpty(HttpRuntime.CodegenDir) == false ? HttpRuntime.CodegenDir : Path.GetTempPath();
    }

    /// <summary>
    /// Writes a resource to a file, unless the file is already there.
    /// </summary>
    /// <param name="assembly">The assembly the resource is embedded in.</param>
    /// <param name="name">The resource's name.</param>
    /// <param name="path">The file to write.</param>
    static void Write(Assembly assembly, string name, string path)
    {
        if (File.Exists(path))
            return;

        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var source = assembly.GetManifestResourceStream(name)!)
            using (var target = File.Create(temporary))
                source.CopyTo(target);

            File.Move(temporary, path);
        }
        catch (IOException) when (File.Exists(path))
        {
            // another process wrote it first
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }
    }

    readonly Lazy<string> _path = new(Extract, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Name reported in stack traces.
    /// </summary>
    public override string Name => ModuleFileName;

    /// <summary>
    /// Returns the path of the module, writing the toolchain out the first time.
    /// </summary>
    /// <param name="cancellationToken">Unused: writing the toolchain out is not cancelled part way.</param>
    public override ValueTask<string> ResolveAsync(CancellationToken cancellationToken) => new(_path.Value);

}
