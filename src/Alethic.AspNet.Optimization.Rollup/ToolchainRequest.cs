using System.Collections.Generic;

namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// One bundle for the toolchain to build.
/// </summary>
sealed class ToolchainRequest
{

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="kind">How the inputs combine.</param>
    /// <param name="inputs">Absolute paths of the inputs, in bundle order.</param>
    /// <param name="fileName">The output's file name, which names it in its source map.</param>
    /// <param name="files">The files the build reads, the inputs and everything they import.</param>
    public ToolchainRequest(BundleKind kind, IReadOnlyList<string> inputs, string fileName, ToolchainFiles files)
    {
        Kind = kind;
        Inputs = inputs;
        FileName = fileName;
        Files = files;
    }

    /// <summary>
    /// How the inputs combine.
    /// </summary>
    public BundleKind Kind { get; }

    /// <summary>
    /// Absolute paths of the inputs, in bundle order.
    /// </summary>
    public IReadOnlyList<string> Inputs { get; }

    /// <summary>
    /// The output's file name, which names it in its source map.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// The files the build reads, the inputs and everything they import.
    /// </summary>
    public ToolchainFiles Files { get; }

    /// <summary>
    /// What classic scripts are joined with; it ends with a line break, so every script starts a line.
    /// </summary>
    public string Separator { get; set; } = ";\n";

    /// <summary>
    /// Whether to minify the output.
    /// </summary>
    public bool Minify { get; set; }

    /// <summary>
    /// Whether to produce a source map.
    /// </summary>
    public bool SourceMap { get; set; }

}
