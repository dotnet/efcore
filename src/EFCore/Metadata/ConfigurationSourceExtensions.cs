// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Provides extension methods for <see cref="ConfigurationSource" />
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public static class ConfigurationSourceExtensions
{
    /// <summary>
    ///     Returns a value indicating whether the new configuration source can override configuration set with the old configuration source.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
    /// </remarks>
    /// <param name="newConfigurationSource">The new configuration source.</param>
    /// <param name="oldConfigurationSource">The old configuration source.</param>
    /// <returns><see langword="true" /> if the new configuration source can override configuration set with the old configuration source.</returns>
    [ContractAnnotation("oldConfigurationSource:null => true")]
    public static bool Overrides(this ConfigurationSource newConfigurationSource, ConfigurationSource? oldConfigurationSource)
    {
        if (oldConfigurationSource == null)
        {
            return true;
        }

        if (newConfigurationSource == ConfigurationSource.Explicit)
        {
            return true;
        }

        if (oldConfigurationSource == ConfigurationSource.Explicit)
        {
            return false;
        }

        if (newConfigurationSource == ConfigurationSource.DataAnnotation)
        {
            return true;
        }

        return oldConfigurationSource != ConfigurationSource.DataAnnotation;
    }

    /// <summary>
    ///     Returns a value indicating whether the new configuration source can override configuration set with the old configuration source.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
    /// </remarks>
    /// <param name="newConfigurationSource">The new configuration source.</param>
    /// <param name="oldConfigurationSource">The old configuration source.</param>
    /// <returns><see langword="true" /> if the new configuration source can override configuration set with the old configuration source.</returns>
    public static bool Overrides(this ConfigurationSource? newConfigurationSource, ConfigurationSource? oldConfigurationSource)
        => newConfigurationSource?.Overrides(oldConfigurationSource) ?? oldConfigurationSource == null;

    /// <summary>
    ///     Returns a value indicating whether the configuration source always takes precedence over the other configuration source.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
    /// </remarks>
    /// <param name="newConfigurationSource">The new configuration source.</param>
    /// <param name="oldConfigurationSource">The old configuration source.</param>
    /// <returns><see langword="true" /> if the configuration source always takes precedence over the other configuration source.</returns>
    public static bool OverridesStrictly(this ConfigurationSource newConfigurationSource, ConfigurationSource? oldConfigurationSource)
        => newConfigurationSource.Overrides(oldConfigurationSource) && newConfigurationSource != oldConfigurationSource;

    /// <summary>
    ///     Returns a value indicating whether the configuration source always takes precedence over the other configuration source.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
    /// </remarks>
    /// <param name="newConfigurationSource">The new configuration source.</param>
    /// <param name="oldConfigurationSource">The old configuration source.</param>
    /// <returns><see langword="true" /> if the configuration source always takes precedence over the other configuration source.</returns>
    public static bool OverridesStrictly(this ConfigurationSource? newConfigurationSource, ConfigurationSource? oldConfigurationSource)
        => newConfigurationSource.HasValue && newConfigurationSource.Value.OverridesStrictly(oldConfigurationSource);

    /// <summary>
    ///     Returns the configuration source which has higher priority.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
    /// </remarks>
    /// <param name="left">The left configuration source.</param>
    /// <param name="right">The right configuration source.</param>
    /// <returns>The configuration source with higher priority.</returns>
    [ContractAnnotation("left:notnull => notnull;right:notnull => notnull")]
    public static ConfigurationSource? Max(this ConfigurationSource? left, ConfigurationSource? right)
        => left.Overrides(right)
            ? left
            : right;

    /// <summary>
    ///     Returns the configuration source which has higher priority.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
    /// </remarks>
    /// <param name="left">The left configuration source.</param>
    /// <param name="right">The right configuration source.</param>
    /// <returns>The configuration source with higher priority.</returns>
    public static ConfigurationSource Max(this ConfigurationSource left, ConfigurationSource? right)
        => left.Overrides(right)
            ? left
            : right!.Value;
}
