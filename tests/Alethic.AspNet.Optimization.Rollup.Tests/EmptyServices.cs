using System;

namespace Alethic.AspNet.Optimization.Rollup.Tests;

/// <summary>
/// A service provider with no services, for a pool made without a container.
/// </summary>
sealed class EmptyServices : IServiceProvider
{

    /// <summary>
    /// The one instance.
    /// </summary>
    public static readonly EmptyServices Instance = new();

    /// <summary>
    /// Returns <see langword="null"/>: there are no services.
    /// </summary>
    /// <param name="serviceType">The service asked for.</param>
    public object? GetService(Type serviceType) => null;

}
