using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Alethic.Node;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JavaScript.NodeApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Alethic.AspNet.Optimization.Rollup.Tests;

/// <summary>
/// Builds real bundles through the real toolchain on a real Node engine, and runs what comes out.
/// </summary>
[TestClass]
public class ToolchainTests
{

    static NodeEnginePool? _pool;
    static Toolchain? _toolchain;

    /// <summary>
    /// Starts one engine for the class, and the toolchain embedded in the library, as a site loads it.
    /// </summary>
    /// <param name="context"></param>
    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        _pool = new NodeEnginePool(new NodeEnginePoolOptions() { EngineCount = 1 }, NullLoggerFactory.Instance, EmptyServices.Instance);
        _toolchain = new Toolchain(_pool, EmbeddedToolchainSource.Instance);
    }

    /// <summary>
    /// Stops the engine.
    /// </summary>
    [ClassCleanup]
    public static async Task Cleanup()
    {
        if (_pool is not null)
            await _pool.DisposeAsync();
    }

    /// <summary>
    /// Returns the absolute path of a fixture.
    /// </summary>
    /// <param name="path">The fixture's path below <c>Fixtures</c>.</param>
    static string Fixture(string path) => Path.Combine(AppContext.BaseDirectory, "Fixtures", path.Replace('/', Path.DirectorySeparatorChar));

    /// <summary>
    /// Runs a script in a fresh context on the engine and returns a global it left, as a string.
    /// </summary>
    /// <param name="code">The script.</param>
    /// <param name="name">The global to read.</param>
    static Task<string> RunAndRead(string code, string name) => _pool!.RunAsync(() =>
    {
        var vm = JSValue.Global["require"].Call(JSValue.Undefined, "node:vm");
        var context = vm.CallMethod("createContext", new JSObject());
        vm.CallMethod("runInContext", code, context);
        return Task.FromResult((string)context[name].CoerceToString());
    });

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task Classic_scripts_share_one_global_scope(bool minify)
    {
        var result = await _toolchain!.BuildAsync(new ToolchainRequest(BundleKind.Script, [Fixture("classic/first.js"), Fixture("classic/second.ts")], "classic.js", new DiskToolchainFiles()) { Minify = minify });

        // second.ts calls a function and reads a variable first.js declared at its top level, and its own top-level
        // `this` is the global object, as it is for a script
        Assert.AreEqual("HELLO!", await RunAndRead(result.Code, "shouted"));
        Assert.AreEqual("hello", await RunAndRead(result.Code, "greeting"));
    }

    [TestMethod]
    public async Task Classic_scripts_map_back_to_their_files()
    {
        var result = await _toolchain!.BuildAsync(new ToolchainRequest(BundleKind.Script, [Fixture("classic/first.js"), Fixture("classic/second.ts")], "classic.js", new DiskToolchainFiles()) { Minify = true, SourceMap = true });

        Assert.IsNotNull(result.Map);
        StringAssert.Contains(result.Map, "first.js");
        StringAssert.Contains(result.Map, "second.ts");
        CollectionAssert.IsSubsetOf(new[] { Fixture("classic/first.js"), Fixture("classic/second.ts") }, result.WatchFiles.ToArray());
    }

    [TestMethod]
    public async Task Modules_are_bundled_without_leaking_their_names()
    {
        var result = await _toolchain!.BuildAsync(new ToolchainRequest(BundleKind.Module, [Fixture("modules/main.js")], "main.js", new DiskToolchainFiles()) { Minify = true });

        Assert.AreEqual("42", await RunAndRead(result.Code, "answer"));
        Assert.AreEqual("undefined", await RunAndRead(result.Code, "local"));
        Assert.IsFalse(result.Code.Contains("unused"), "tree-shaking removes what nothing imports");
    }

    [TestMethod]
    public async Task Sass_compiles_and_reports_its_partials()
    {
        var result = await _toolchain!.BuildAsync(new ToolchainRequest(BundleKind.Style, [Fixture("styles/site.scss")], "site.css", new DiskToolchainFiles()) { Minify = true, SourceMap = true });

        StringAssert.Contains(result.Code, ".banner .title{font-weight:700}");
        StringAssert.Contains(result.Code, "#369");
        CollectionAssert.Contains(result.WatchFiles.ToArray(), Fixture("styles/_palette.scss"));
        Assert.IsNotNull(result.Map);
        StringAssert.Contains(result.Map, "site.scss");
    }

    [TestMethod]
    [DataRow("Module", "modules/main.js", "modules/math.ts")]
    [DataRow("Style", "styles/site.scss", "styles/_palette.scss")]
    public async Task A_bundle_built_from_an_entry_takes_one(string kind, string first, string second)
    {
        var exception = await Assert.ThrowsExactlyAsync<JSException>(() => _toolchain!.BuildAsync(new ToolchainRequest((BundleKind)Enum.Parse(typeof(BundleKind), kind), [Fixture(first), Fixture(second)], "out", new DiskToolchainFiles())));
        StringAssert.Contains(exception.Message, "built from one entry");
    }

    [TestMethod]
    public async Task Every_file_is_read_through_the_host()
    {
        var files = new DiskToolchainFiles();
        await _toolchain!.BuildAsync(new ToolchainRequest(BundleKind.Style, [Fixture("styles/site.scss")], "site.css", files));

        CollectionAssert.AreEquivalent(new[] { Fixture("styles/site.scss"), Fixture("styles/_palette.scss") }, files.Reads);
    }

    [TestMethod]
    public async Task Files_need_not_be_on_disk()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var files = new MemoryToolchainFiles
        {
            [Path.Combine(root, "main.ts")] = "import { answer } from './answer';\nglobalThis.answer = answer;",
            [Path.Combine(root, "answer.ts")] = "export const answer: number = 42;",
            [Path.Combine(root, "site.scss")] = "@use 'colors';\n.x { color: colors.$accent; }",
            [Path.Combine(root, "_colors.scss")] = "$accent: red;",
        };

        var script = await _toolchain!.BuildAsync(new ToolchainRequest(BundleKind.Module, [Path.Combine(root, "main.ts")], "main.js", files));
        Assert.AreEqual("42", await RunAndRead(script.Code, "answer"));

        var style = await _toolchain!.BuildAsync(new ToolchainRequest(BundleKind.Style, [Path.Combine(root, "site.scss")], "site.css", files) { Minify = true });
        Assert.AreEqual(".x{color:red}", style.Code.Trim());
        CollectionAssert.Contains(style.WatchFiles.ToArray(), Path.Combine(root, "_colors.scss"));
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task Targets_rewrite_scripts_for_older_browsers(bool minify)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var files = new MemoryToolchainFiles
        {
            [Path.Combine(root, "counter.ts")] = "enum Start { Zero }\nclass Counter { #count: number = Start.Zero; next(): number { return ++this.#count; } }\nvar counted = new Counter().next() ?? 0;",
        };

        var result = await _toolchain!.BuildAsync(new ToolchainRequest(BundleKind.Script, [Path.Combine(root, "counter.ts")], "counter.js", files) { Targets = "ie 11", Minify = minify });

        Assert.IsFalse(result.Code.Contains("class Counter"), "classes are rewritten as functions");
        Assert.IsFalse(result.Code.Contains("??"), "nullish coalescing is rewritten");
        Assert.AreEqual("1", await RunAndRead(result.Code, "counted"));
    }

    [TestMethod]
    public async Task Without_targets_scripts_keep_their_language_level()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var files = new MemoryToolchainFiles
        {
            [Path.Combine(root, "counter.js")] = "class Counter { next() { return 1; } }\nvar counted = new Counter().next() ?? 0;",
        };

        var result = await _toolchain!.BuildAsync(new ToolchainRequest(BundleKind.Script, [Path.Combine(root, "counter.js")], "counter.js", files));

        StringAssert.Contains(result.Code, "class Counter");
        StringAssert.Contains(result.Code, "??");
    }

    [TestMethod]
    public async Task Targets_lower_and_prefix_stylesheets()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var files = new MemoryToolchainFiles
        {
            [Path.Combine(root, "site.css")] = ".a { user-select: none; .b { color: red } }",
        };

        var result = await _toolchain!.BuildAsync(new ToolchainRequest(BundleKind.Style, [Path.Combine(root, "site.css")], "site.css", files) { Targets = "ie 11", Minify = true });

        StringAssert.Contains(result.Code, "-ms-user-select:none");
        StringAssert.Contains(result.Code, ".a .b{color:red}");
    }

    [TestMethod]
    public async Task A_syntax_error_fails_the_build()
    {
        var broken = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".js");
        File.WriteAllText(broken, "var = ;");

        try
        {
            await Assert.ThrowsExactlyAsync<JSException>(() => _toolchain!.BuildAsync(new ToolchainRequest(BundleKind.Script, [broken], "broken.js", new DiskToolchainFiles())));
        }
        finally
        {
            File.Delete(broken);
        }
    }

}
