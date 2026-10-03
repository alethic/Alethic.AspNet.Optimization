using System;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Hosting;

namespace Alethic.AspNet.Optimization.Rollup;

/// <summary>
/// The <c>alethic.optimization.rollup</c> section of <c>web.config</c>: what every <see cref="RollupBundle"/> does that
/// does not say otherwise.
/// </summary>
/// <remarks>
/// <code language="xml"><![CDATA[
/// <configSections>
///   <section name="alethic.optimization.rollup" type="Alethic.AspNet.Optimization.Rollup.RollupSection, Alethic.AspNet.Optimization.Rollup" />
/// </configSections>
///
/// <alethic.optimization.rollup targets="defaults" minify="true" sourceMap="false" />
/// ]]></code>
///
/// A bundle's own <see cref="RollupBundle.Targets"/>, <see cref="RollupBundle.Minify"/> and
/// <see cref="RollupBundle.SourceMap"/> win over the section. Read from the application's root, whatever folder a
/// request is for; a site without the section gets every default.
/// </remarks>
public sealed class RollupSection : ConfigurationSection
{

    /// <summary>
    /// The section's name in <c>web.config</c>.
    /// </summary>
    public const string SectionName = "alethic.optimization.rollup";

    /// <summary>
    /// Returns the application's section, or one with every default where it has none.
    /// </summary>
    internal static RollupSection Current()
    {
        var section = HostingEnvironment.IsHosted
            ? WebConfigurationManager.GetSection(SectionName, HostingEnvironment.ApplicationVirtualPath)
            : ConfigurationManager.GetSection(SectionName);

        return section as RollupSection ?? new RollupSection();
    }

    /// <summary>
    /// Returns a switch's value: <see langword="null"/> where not set, so that the bundle's built-in behavior applies.
    /// </summary>
    /// <param name="name">The attribute's name.</param>
    /// <param name="value">The attribute's value.</param>
    /// <exception cref="ConfigurationErrorsException">The value is neither <c>true</c> nor <c>false</c>.</exception>
    static bool? ParseSwitch(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return bool.TryParse(value, out var result)
            ? result
            : throw new ConfigurationErrorsException($"The {SectionName} section's '{name}' must be 'true' or 'false', not '{value}'.");
    }

    /// <summary>
    /// The browsers bundles are built for, as a browserslist query, such as <c>defaults</c>: see
    /// <see cref="RollupBundle.Targets"/>. Where not set, bundles keep the language level of their sources.
    /// </summary>
    [ConfigurationProperty("targets", DefaultValue = "")]
    public string Targets
    {
        get => (string)this["targets"];
        set => this["targets"] = value;
    }

    /// <summary>
    /// Whether bundles are minified, <c>true</c> or <c>false</c>: see <see cref="RollupBundle.Minify"/>. Where not set,
    /// they are minified where optimizations are enabled.
    /// </summary>
    [ConfigurationProperty("minify", DefaultValue = "")]
    public string Minify
    {
        get => (string)this["minify"];
        set => this["minify"] = value;
    }

    /// <summary>
    /// Whether bundles carry an inline source map, <c>true</c> or <c>false</c>: see <see cref="RollupBundle.SourceMap"/>.
    /// Where not set, they do where optimizations are disabled.
    /// </summary>
    [ConfigurationProperty("sourceMap", DefaultValue = "")]
    public string SourceMap
    {
        get => (string)this["sourceMap"];
        set => this["sourceMap"] = value;
    }

    /// <summary>
    /// <see cref="Targets"/>, or <see langword="null"/> where not set.
    /// </summary>
    internal string? TargetsValue => string.IsNullOrWhiteSpace(Targets) ? null : Targets;

    /// <summary>
    /// <see cref="Minify"/>, or <see langword="null"/> where not set.
    /// </summary>
    internal bool? MinifyValue => ParseSwitch("minify", Minify);

    /// <summary>
    /// <see cref="SourceMap"/>, or <see langword="null"/> where not set.
    /// </summary>
    internal bool? SourceMapValue => ParseSwitch("sourceMap", SourceMap);

}
