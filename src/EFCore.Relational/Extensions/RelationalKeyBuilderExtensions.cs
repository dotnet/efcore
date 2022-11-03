// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="KeyBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-keys">Keys</see> for more information and examples.
/// </remarks>
public static class RelationalKeyBuilderExtensions
{
    /// <summary>
    ///     Configures the name of the key constraint in the database when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-keys">Keys</see> for more information and examples.
    /// </remarks>
    /// <param name="keyBuilder">The builder for the key being configured.</param>
    /// <param name="name">The name of the key.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static KeyBuilder HasName(this KeyBuilder keyBuilder, string? name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        keyBuilder.Metadata.SetName(name);

        return keyBuilder;
    }

    /// <summary>
    ///     Configures the name of the key constraint in the database when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-keys">Keys</see> for more information and examples.
    /// </remarks>
    /// <param name="keyBuilder">The builder for the key being configured.</param>
    /// <param name="name">The name of the key.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static KeyBuilder<TEntity> HasName<TEntity>(
        this KeyBuilder<TEntity> keyBuilder,
        string? name)
        => (KeyBuilder<TEntity>)HasName((KeyBuilder)keyBuilder, name);

    /// <summary>
    ///     Configures the name of the key constraint in the database when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-keys">Keys</see> for more information and examples.
    /// </remarks>
    /// <param name="keyBuilder">The builder for the key being configured.</param>
    /// <param name="name">The name of the key.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionKeyBuilder? HasName(
        this IConventionKeyBuilder keyBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        if (keyBuilder.CanSetName(name, fromDataAnnotation))
        {
            keyBuilder.Metadata.SetName(name, fromDataAnnotation);
            return keyBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given name can be set for the key constraint.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-keys">Keys</see> for more information and examples.
    /// </remarks>
    /// <param name="keyBuilder">The builder for the key being configured.</param>
    /// <param name="name">The name of the index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given name can be set for the key constraint.</returns>
    public static bool CanSetName(
        this IConventionKeyBuilder keyBuilder,
        string? name,
        bool fromDataAnnotation = false)
        => keyBuilder.CanSetAnnotation(RelationalAnnotationNames.Name, name, fromDataAnnotation);
}
