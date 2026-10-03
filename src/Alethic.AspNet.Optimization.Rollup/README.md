# Alethic.AspNet.Optimization.Rollup

Modern bundling for ASP.NET on .NET Framework. Write TypeScript, ES modules and Sass, and serve them through the
System.Web.Optimization bundles you already use: `BundleConfig`, `Scripts.Render` and `Styles.Render` work as they
always have.

The bundles are built inside your site by Rollup, SWC, Terser, Sass and Lightning CSS. Node is embedded in the
process, so nothing needs installing on the server and there is no separate front-end build.

## Install

```
dotnet add package Alethic.AspNet.Optimization.Rollup
dotnet add package Microsoft.JavaScript.LibNode.win-x64
```

The second package is Node itself. The site's application pool must run 64-bit.

Then, in `web.config`:

```xml
<system.web>
  <!-- Node starts once per process: have ASP.NET recycle the application pool rather than restart the site in place -->
  <httpRuntime targetFramework="4.8" fcnMode="Disabled" />
</system.web>

<system.webServer>
  <modules>
    <!-- so bundle URLs ending in .js and .css are served as bundles -->
    <remove name="BundleModule" />
    <add name="BundleModule" type="System.Web.Optimization.BundleModule" />
  </modules>
</system.webServer>
```

With `fcnMode="Disabled"`, a change to `web.config` or `bin` takes effect when the application pool recycles. Changes
to scripts and stylesheets are picked up straight away.

## Bundles

There are three kinds, all added in `BundleConfig` like any other bundle:

```csharp
using Alethic.AspNet.Optimization.Rollup;

public static void RegisterBundles(BundleCollection bundles)
{
    // Scripts, JavaScript or TypeScript, run in the order they are included, sharing one global scope: what one
    // declares at its top level, the next can use, as if each had its own <script> tag.
    bundles.Add(new RollupScriptBundle("~/bundle/cart.js")
        .Include("~/Scripts/currency.js")
        .Include("~/Scripts/cart.ts"));

    // ES modules, from their entry module: its imports are followed, unused code is left out, and nothing it declares
    // becomes a global.
    bundles.Add(new RollupModuleBundle("~/bundle/app.js", "~/Scripts/app/main.ts"));

    // A stylesheet, Sass or CSS, from its entry: its @use and @import rules are followed.
    bundles.Add(new RollupStyleBundle("~/bundle/site.css", "~/Styles/site.scss"));
}
```

Render them as usual:

```aspx
<%: Styles.Render("~/bundle/site.css") %>
<%: Scripts.Render("~/bundle/cart.js") %>
<%: Scripts.Render("~/bundle/app.js") %>
```

Use `RollupScriptBundle` for existing scripts that share globals, and `RollupModuleBundle` for code written with
`import` and `export`. A site can move scripts from one to the other a bundle at a time.

Everything else System.Web.Optimization offers works as for any bundle: versioned URLs, CDN paths, `.min` files,
item transforms such as `CssRewriteUrlTransform`, and transforms of your own added to a bundle's `Transforms`, which
run on the built bundle.

## What a bundle does

- TypeScript is compiled to JavaScript. Types are not checked; check them with `tsc` in your editor or build.
- Where optimizations are enabled, scripts and stylesheets are minified.
- A change to any file a bundle uses, an imported module or a Sass partial included, rebuilds it on its next request.
- With browser targets set, newer script syntax is rewritten and stylesheets are prefixed for older browsers.
  Missing built-in objects and methods are not added.
- A bundle can carry an inline source map, so the browser's developer tools show each file as you wrote it.

## Settings

Each bundle has three settings:

| Property | Meaning | When not set |
|---|---|---|
| `Targets` | The browsers to build for, as a [browserslist](https://browsersl.ist) query, such as `defaults` | Output keeps the language level of the sources |
| `Minify` | Whether to minify | Minify where optimizations are enabled |
| `SourceMap` | Whether to add an inline source map | Add one where optimizations are disabled |

Set them for the whole site in `web.config`; a bundle's own setting wins:

```xml
<configSections>
  <section name="alethic.optimization.rollup" type="Alethic.AspNet.Optimization.Rollup.RollupSection, Alethic.AspNet.Optimization.Rollup" />
</configSections>

<alethic.optimization.rollup targets="defaults" />
```

The Node engines that build the bundles are set up by the `alethic.node` section, or supplied by your dependency
injection container through `HttpRuntime.WebObjectActivator`. See
[Alethic.Node.AspNet](https://www.nuget.org/packages/Alethic.Node.AspNet).

## Optimizations disabled

With optimizations disabled, as under `<compilation debug="true">`, `Scripts.Render` and `Styles.Render` write a tag
for each file in a bundle instead of one for the bundle. TypeScript and Sass files cannot yet be served on their own,
so keep optimizations enabled for bundles that include them, for example with `BundleTable.EnableOptimizations =
true`, and set `SourceMap = true` to debug them.
