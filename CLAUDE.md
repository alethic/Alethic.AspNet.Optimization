# Alethic.AspNet.Optimization

Packages that extend System.Web.Optimization. Targets `net48`: bundling inside the application is a System.Web concern,
and there is no ASP.NET Core build.

`Alethic.AspNet.Optimization.Rollup` builds bundles with a JavaScript toolchain driven by Rollup (with Sass, SWC,
Terser and Lightning CSS) on Node engines embedded in the process through Alethic.Node.

## Layout

- `src/Alethic.AspNet.Optimization.Rollup` - the library.
  - `toolchain/` - the npm project Rollup builds into `dist/toolchain.cjs`, with the WebAssembly binaries the tools
    read from beside it. The csproj's `BuildToolchain` target runs `npm ci` and `npx rollup -c`; the files are copied
    to `alethic.aspnet.optimization.rollup\` in the output and packed under `build\`, where
    `build/Alethic.AspNet.Optimization.Rollup.targets` copies them into a consumer's output.
  - `toolchain/src/index.js` - `build(request)`, the one entry point. Every bundle is one Rollup build. A module
    bundle's entry is its entry module; `classicScripts` and `styles` in `toolchain/src/plugins` make Rollup's entry
    from classic scripts and from a stylesheet; `typescript` and `minifyScript` transform and minify.
  - The public API is the bundles: `RollupScriptBundle` (classic scripts, included in order), and the
    `RollupEntryBundle`s, `RollupModuleBundle` and `RollupStyleBundle`, built from one entry file. `RollupBundle` is
    their base. Everything else is internal: `Toolchain` calls `build` on an engine from `AspNetNode.Pool`.
  - The System.Web.Optimization side: `RollupBundleBuilder` is each bundle's `IBundleBuilder` and runs the whole
    Rollup build, minification included; `RollupBundle.ApplyTransforms` makes the response, its files being every file
    the build read, then runs whatever transforms the site added. `Transforms` is empty by default. Files are read
    through `BundleToolchainFiles`: included files from their `BundleFile`, item transforms applied, everything else
    from `BundleTable.VirtualPathProvider`. `RollupBundleResolver` renders a bundle as itself where optimizations are
    off.
- `tests/Alethic.AspNet.Optimization.Rollup.Tests` - MSTest, `net48`, x64. Builds fixtures through the real toolchain on a
  real libnode engine and runs the output.
- `samples/Alethic.AspNet.Optimization.Sample` - a Web Forms site with one bundle of each kind, served by IIS Express.
- `src/dist-nuget`, `src/dist-tests` - NoTargets projects that pack the library and publish the test project.
  `Alethic.AspNet.Optimization.dist.msbuildproj` drives them and writes to `dist/`.

## Commands

```bash
dotnet build Alethic.AspNet.Optimization.slnx
```

The test project is a Microsoft.Testing.Platform executable:

```bash
tests/Alethic.AspNet.Optimization.Rollup.Tests/bin/Debug/net48/Alethic.AspNet.Optimization.Rollup.Tests.exe
```

Working on the toolchain alone, its sources run under Node directly, without the 50-second Rollup build:

```bash
cd src/Alethic.AspNet.Optimization.Rollup/toolchain && node -e "import('./src/index.js').then(t => t.build({...}))"
```

## Toolchain rules

- Alethic.Node loads CommonJS only, with no dynamic `import()`: `toolchain.cjs` is one static bundle.
- The tools are the builds that need nothing native: `@rollup/wasm-node`, `lightningcss-wasm`,
  `@swc/wasm-typescript`, `sass`, `terser`. They run on any platform libnode does.
- Rollup leaves modules whose id starts with `\0` out of source maps. An entry whose code is the inputs' own, as the
  classic-scripts entry's is, takes an id without it.
- Package versions in `toolchain/package.json` are exact.

## Conventions

- Namespaces, projects, directories and the solution are prefixed `Alethic.AspNet.Optimization`. Public types of the
  Rollup package are prefixed `Rollup`.
- One type per file, file named after the type.
- A single blank line after a type's opening brace and before its closing brace.
- File-scoped namespaces. Explicit `using` directives; `ImplicitUsings` is off. System namespaces sort first.
- Omit `private`; it is the default. Fields are `_camelCase`, static ones included. Constants are PascalCase.
- Member order inside a type: nested types first, then static members, then instance members.
- No braces around single-line `if` bodies. Braces for anything multi-line.
- Every method has an XML doc. `<summary>` open and close tags on their own lines. Say what a caller needs; no history
  or design rationale.
- Lines, including comments and XML docs, wrap at 140 columns.
- csproj files use four-space indentation. The `.slnx` is tool-managed; leave its formatting alone.
- JSON goes through System.Text.Json.
