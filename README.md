# Alethic.AspNet.Optimization

## Alethic.AspNet.Optimization.Rollup

System.Web.Optimization bundles built by a real JavaScript toolchain: Rollup orchestrating Sass, SWC, Terser and
Lightning CSS, running in the site's own Node engines through [Alethic.Node](https://www.nuget.org/packages/Alethic.Node).
For ASP.NET on .NET Framework, Web Forms and MVC alike.

```csharp
using Alethic.AspNet.Optimization.Rollup;

public static void RegisterBundles(BundleCollection bundles)
{
    // classic scripts, in the order they run: one global scope, as if each file had its own <script> tag
    bundles.Add(new RollupScriptBundle("~/bundle/cart.js")
        .Include("~/scripts/currency.js")
        .Include("~/scripts/cart.ts"));

    // ES modules from their entry: imports followed and tree-shaken, bundled into one immediately invoked function
    bundles.Add(new RollupModuleBundle("~/bundle/app.js", "~/scripts/app/main.ts"));

    // a stylesheet from its entry: Sass compiled, then transformed and minified by Lightning CSS
    bundles.Add(new RollupStyleBundle("~/bundle/site.css", "~/styles/site.scss"));
}
```

`Scripts.Render` and `Styles.Render` render them as they render any bundle.

## What it does

- **Every bundle is one Rollup build.** Rollup reads the bundle's files, runs each through its transform plugins, and
  renders the output.
  - TypeScript has its types stripped by SWC. Type checking stays with the compiler, in the editor and the build.
  - Scripts and modules are minified by Terser.
  - Stylesheets are compiled by Sass, then transformed and minified by Lightning CSS.
- **Classic scripts stay classic.** A `RollupScriptBundle` joins its files in order into one scope, so a
  top-level `var` or `function` in one file is a global the next can use, exactly as when the files are joined as text.
- **Dependencies invalidate the cache.** The bundle's cache dependency is every file the build read: imported modules
  and Sass partials as well as the files the bundle names.
- **Debugging shows the source.** Where optimizations are disabled, a bundle is rendered as itself rather than as a tag
  per file, unminified, with an inline source map. The browser's tools show each file as it was written, TypeScript
  and Sass included.

## Requirements

- .NET Framework 4.8, 64-bit.
- libnode: reference `Microsoft.JavaScript.LibNode.win-x64` from the web project.
- The application's Node engines come from `Alethic.Node.AspNet`, whose `alethic.node` section in `web.config`
  configures them.
- Node starts once per process, and ASP.NET restarts an application in a new AppDomain of the same process. Keep it
  from doing so, for example with `<httpRuntime fcnMode="Disabled" />`, and recycle the application pool instead. See
  Alethic.Node.AspNet.

## Building

The toolchain is an npm project under `src/Alethic.AspNet.Optimization.Rollup/toolchain`, which the project's build installs
and bundles into `toolchain.cjs`. Building needs Node and npm on the path.

```bash
dotnet build Alethic.AspNet.Optimization.slnx
```

## Sample

`samples/Alethic.AspNet.Optimization.Sample` is a Web Forms site with one bundle of each kind; each panel on its page
turns green when its bundle has done its job. `dotnet run` it, which serves it with IIS Express on port 8091.
