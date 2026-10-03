using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Alethic.Node;
using Alethic.Node.AspNet;

using Microsoft.JavaScript.NodeApi;

namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// Builds bundles with the JavaScript toolchain: Rollup orchestrating Sass, SWC, Terser and Lightning CSS, on a pool of
/// Node engines.
/// </summary>
sealed class Toolchain
{

    /// <summary>
    /// Returns the application's toolchain: the one embedded in this assembly, on the application's pool of engines as
    /// it is now, which is the one the site's <see cref="System.Web.HttpRuntime.WebObjectActivator"/> supplies where it
    /// supplies one.
    /// </summary>
    public static Toolchain ForApplication() => new(AspNetNode.Pool, EmbeddedToolchainSource.Instance);

    readonly NodeEnginePool _pool;
    readonly NodeModuleSource _module;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="pool">The engines to build on.</param>
    /// <param name="module">The toolchain module.</param>
    public Toolchain(NodeEnginePool pool, NodeModuleSource module)
    {
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        _module = module ?? throw new ArgumentNullException(nameof(module));
    }

    /// <summary>
    /// Builds a bundle.
    /// </summary>
    /// <param name="request">The bundle to build.</param>
    /// <param name="cancellationToken">Cancels the wait for an engine.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="JSException">A tool failed, such as on a syntax error in an input.</exception>
    public Task<ToolchainResult> BuildAsync(ToolchainRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        return _pool.RunAsync(_module, async exports =>
        {
            var inputs = new JSArray();
            foreach (var input in request.Inputs)
                inputs.Add(input);

            var options = new JSObject
            {
                ["kind"] = request.Kind switch
                {
                    BundleKind.Script => "script",
                    BundleKind.Module => "module",
                    BundleKind.Style => "style",
                    _ => throw new ArgumentOutOfRangeException(nameof(request), request.Kind, "Unknown bundle kind."),
                },
                ["inputs"] = inputs,
                ["fileName"] = request.FileName,
                ["minify"] = request.Minify,
                ["sourceMap"] = request.SourceMap,
                ["separator"] = request.Separator,
                ["targets"] = request.Targets is string targets ? targets : JSValue.Undefined,
                ["files"] = new JSObject
                {
                    ["exists"] = new JSFunction(path => request.Files.Exists((string)path)),
                    ["read"] = new JSFunction(path => request.Files.Read((string)path) is string content ? content : JSValue.Null),
                },
            };

            var result = await ((JSPromise)exports.CallMethod("build", options)).AsTask();
            return new ToolchainResult(
                (string)result["code"],
                result["map"].IsNullOrUndefined() ? null : (string)result["map"],
                ToStrings(result["watchFiles"]),
                ToStrings(result["warnings"]));
        }, cancellationToken);
    }

    /// <summary>
    /// Copies a JavaScript array of strings out of the engine.
    /// </summary>
    /// <param name="value">The array.</param>
    static IReadOnlyList<string> ToStrings(JSValue value)
    {
        var list = new List<string>();
        foreach (var item in (JSArray)value)
            list.Add((string)item);

        return list;
    }

}
