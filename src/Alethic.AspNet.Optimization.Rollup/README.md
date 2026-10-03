# Alethic.AspNet.Optimization.Rollup

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
  - TypeScript is compiled by SWC. Type checking stays with the compiler, in the editor and the build.
  - Scripts and modules are minified by Terser.
  - Stylesheets are compiled by Sass, then transformed and minified by Lightning CSS.
- **Classic scripts stay classic.** A `RollupScriptBundle` joins its files in order into one scope, so a top-level
  `var` or `function` in one file is a global the next can use, exactly as when the files are joined as text.
- **Every file the build read keeps the bundle current.** An edit to any of them, imported modules and Sass partials
  included, rebuilds the bundle on its next request. This needs no file change notifications, which a site running
  Node turns off.
- **It is System.Web.Optimization underneath.** Rendering, versioned URLs, caching, ignore lists, `.min` swapping,
  ordering, item transforms and CDN paths all work as for any bundle. Files are read through the bundle's
  `BundleFile`s and the site's virtual path provider. A bundle's `Transforms` start empty, for the site's own, which
  run on the built content.
- **Browser targets.** Set `Targets` to a browserslist query, such as `defaults`, and SWC rewrites newer script
  syntax, and Lightning CSS lowers and prefixes stylesheets, for those browsers. Built-ins the browsers lack are not
  added. Without targets, the output keeps the language level of the sources.
- **Source maps.** Set `SourceMap = true`, or leave it to follow optimizations, to append an inline source map that
  shows the browser's tools each file as it was written, TypeScript and Sass included.

Where optimizations are disabled, System.Web.Optimization renders a bundle as a tag per file. TypeScript and Sass files
cannot yet be served on their own, so a bundle that includes them needs optimizations enabled.

## Site-wide settings

What every bundle does that does not say otherwise comes from the `alethic.optimization.rollup` section of
`web.config`. A bundle's own `Targets`, `Minify` and `SourceMap` win over it; where neither sets one, a bundle is
minified, and carries no source map, where optimizations are enabled, and the reverse where they are not.

```xml
<configSections>
  <section name="alethic.optimization.rollup" type="Alethic.AspNet.Optimization.Rollup.RollupSection, Alethic.AspNet.Optimization.Rollup" />
</configSections>

<alethic.optimization.rollup targets="defaults" minify="true" sourceMap="false" />
```

## Requirements

- .NET Framework 4.8, 64-bit.
- libnode: reference `Microsoft.JavaScript.LibNode.win-x64` from the web project.
- The application's Node engines come from `Alethic.Node.AspNet`: the `NodeEnginePool` the site's
  `HttpRuntime.WebObjectActivator` supplies, where it supplies one, and otherwise a default pool configured by the
  `alethic.node` section of `web.config`. Bundles are built on whichever it is at the time.
- Node starts once per process, and ASP.NET restarts an application in a new AppDomain of the same process. Keep it
  from doing so, for example with `<httpRuntime fcnMode="Disabled" />`, and recycle the application pool instead. See
  Alethic.Node.AspNet.
