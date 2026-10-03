# Alethic.AspNet.Optimization

Packages that extend System.Web.Optimization, for ASP.NET on .NET Framework.

| Package | |
|---|---|
| [Alethic.AspNet.Optimization.Rollup](src/Alethic.AspNet.Optimization.Rollup/README.md) | Bundles of TypeScript, ES modules and Sass, built in the site by Rollup on embedded Node. |

## Building

```bash
dotnet build Alethic.AspNet.Optimization.slnx
```

Building needs Node and npm on the path: the Rollup package's JavaScript tools are an npm project, under
`src/Alethic.AspNet.Optimization.Rollup/toolchain`, that its build installs and bundles into the assembly.

## Testing

The tests build real bundles on a real Node engine. They are a .NET Framework executable:

```bash
tests/Alethic.AspNet.Optimization.Rollup.Tests/bin/Debug/net48/Alethic.AspNet.Optimization.Rollup.Tests.exe
```

## Sample

`samples/Alethic.AspNet.Optimization.Sample` is a Web Forms site with one bundle of each kind; each panel on its page
turns green when its bundle has done its job. `dotnet run` it to serve it with IIS Express on port 8091.
