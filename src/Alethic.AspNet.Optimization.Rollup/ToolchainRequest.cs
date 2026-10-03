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
    public ToolchainRequest(BundleKind kind, IReadOnlyList<string> inputs, string fileName)
    {
        Kind = kind;
        Inputs = inputs;
        FileName = fileName;
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
    /// Whether to minify the output.
    /// </summary>
    public bool Minify { get; set; }

    /// <summary>
    /// Whether to produce a source map.
    /// </summary>
    public bool SourceMap { get; set; }

}
