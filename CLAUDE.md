# Alethic.AspNet.Optimization

System.Web.Optimization bundles built by a JavaScript toolchain (Rollup, Sass, SWC, Terser, Lightning CSS) on Node
engines embedded in the process through Alethic.Node. Targets `net48`: bundling inside the application is a System.Web
concern, and there is no ASP.NET Core build.

## Layout

- `src/Alethic.AspNet.Optimization` - the library.
  - `toolchain/` - the npm project Rollup builds into `dist/toolchain.cjs`, with the WebAssembly binaries the tools
    read from beside it. The csproj's `BuildToolchain` target runs `npm ci` and `npx rollup -c`; the files are copied
    to `alethic.aspnet.optimization\` in the output and packed under `build\`, where
    `build/Alethic.AspNet.Optimization.targets` copies them into a consumer's output.
  - `toolchain/src/index.js` - `build(request)`, the one entry point. Every bundle is one Rollup build; the plugins in
    `toolchain/src/plugins` turn a bundle's ordered inputs into Rollup's entry (`classicScripts`, `moduleEntry`,
    `styles`) and transform or minify (`typescript`, `minifyScript`).
  - `Toolchain` - calls `build` on an engine from the pool. `RollupBundle`, `RollupBundleTransform` and
    `RollupBundleResolver` are the System.Web.Optimization side.
- `tests/Alethic.AspNet.Optimization.Tests` - MSTest, `net48`, x64. Builds fixtures through the real toolchain on a
  real libnode engine and runs the output.
- `src/dist-nuget`, `src/dist-tests` - NoTargets projects that pack the library and publish the test project.
  `Alethic.AspNet.Optimization.dist.msbuildproj` drives them and writes to `dist/`.

## Commands

```bash
dotnet build Alethic.AspNet.Optimization.slnx
```

The test project is a Microsoft.Testing.Platform executable:

```bash
tests/Alethic.AspNet.Optimization.Tests/bin/Debug/net48/Alethic.AspNet.Optimization.Tests.exe
```

Working on the toolchain alone, its sources run under Node directly, without the 50-second Rollup build:

```bash
cd src/Alethic.AspNet.Optimization/toolchain && node -e "import('./src/index.js').then(t => t.build({...}))"
```

## Toolchain rules

- Alethic.Node loads CommonJS only, with no dynamic `import()`: `toolchain.cjs` is one static bundle.
- The tools are the builds that need nothing native: `@rollup/wasm-node`, `lightningcss-wasm`,
  `@swc/wasm-typescript`, `sass`, `terser`. They run on any platform libnode does.
- Rollup leaves modules whose id starts with `\0` out of source maps. An entry whose code is the inputs' own, as the
  classic-scripts entry's is, takes an id without it.
- Package versions in `toolchain/package.json` are exact.

## Conventions

- Namespaces, projects, directories and the solution are prefixed `Alethic.AspNet.Optimization`.
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
