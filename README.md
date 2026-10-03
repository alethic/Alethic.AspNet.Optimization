# Alethic.AspNet.Optimization

Packages that extend System.Web.Optimization, for ASP.NET on .NET Framework.

- **[Alethic.AspNet.Optimization.Rollup](src/Alethic.AspNet.Optimization.Rollup/README.md)** - bundles built by
  Rollup, with Sass, SWC, Terser and Lightning CSS, running in the site's own Node engines through Alethic.Node:
  classic scripts, ES modules and stylesheets, TypeScript and Sass included.

## Building

```bash
dotnet build Alethic.AspNet.Optimization.slnx
```

The Rollup package's toolchain is an npm project under `src/Alethic.AspNet.Optimization.Rollup/toolchain`, which the
project's build installs and bundles into `toolchain.cjs`, so building needs Node and npm on the path.

## Sample

`samples/Alethic.AspNet.Optimization.Sample` is a Web Forms site with one bundle of each kind; each panel on its page
turns green when its bundle has done its job. `dotnet run` it, which serves it with IIS Express on port 8091.
