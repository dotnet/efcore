// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="IndexBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-indexes">Indexes</see> for more information and examples.
/// </remarks>
public static class RelationalIndexBuilderExtensions
{
    /// <summary>
    ///     Configures the name of the index in the database when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-indexes">Indexes</see> for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="name">The name of the index.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder HasDatabaseName(this IndexBuilder indexBuilder, string? name)
    {
        indexBuilder.Metadata.SetDatabaseName(name);

        return indexBuilder;
    }

    /// <summary>
    ///     Configures the name of the index in the database when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-indexes">Indexes</see> for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="name">The name of the index.</param>
    /// <returns>A builder to further configure the index.</returns>
    [Obsolete("Use HasDatabaseName() instead.")] // DO NOT REMOVE
    // Used in model snapshot. See issue#18557
    public static IndexBuilder HasName(this IndexBuilder indexBuilder, string? name)
        => HasDatabaseName(indexBuilder, name);

    /// <summary>
    ///     Configures the name of the index in the database when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-indexes">Indexes</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="name">The name of the index.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> HasDatabaseName<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        string? name)
    {
        indexBuilder.Metadata.SetDatabaseName(name);

        return indexBuilder;
    }

    /// <summary>
    ///     Configures the name of the index in the database when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-indexes">Indexes</see> for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="name">The name of the index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionIndexBuilder? HasDatabaseName(
        this IConventionIndexBuilder indexBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        if (indexBuilder.CanSetDatabaseName(name, fromDataAnnotation))
        {
            indexBuilder.Metadata.SetDatabaseName(name, fromDataAnnotation);
            return indexBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given name can be set for the index.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-indexes">Indexes</see> for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="name">The name of the index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given name can be set for the index.</returns>
    public static bool CanSetDatabaseName(
        this IConventionIndexBuilder indexBuilder,
        string? name,
        bool fromDataAnnotation = false)
        => indexBuilder.CanSetAnnotation(RelationalAnnotationNames.Name, name, fromDataAnnotation);

    /// <summary>
    ///     Configures the filter expression for the index.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-indexes">Indexes</see> for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="sql">The filter expression for the index.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder HasFilter(this IndexBuilder indexBuilder, string? sql)
    {
        Check.NullButNotEmpty(sql, nameof(sql));

        indexBuilder.Metadata.SetFilter(sql);

        return indexBuilder;
    }

    /// <summary>
    ///     Configures the filter expression for the index.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-indexes">Indexes</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="sql">The filter expression for the index.</param>
    /// <returns>A builder to further configure the index.</returns>
    public static IndexBuilder<TEntity> HasFilter<TEntity>(this IndexBuilder<TEntity> indexBuilder, string? sql)
        => (IndexBuilder<TEntity>)HasFilter((IndexBuilder)indexBuilder, sql);

    /// <summary>
    ///     Configures the filter expression for the index.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-indexes">Indexes</see> for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="sql">The filter expression for the index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionIndexBuilder? HasFilter(
        this IConventionIndexBuilder indexBuilder,
        string? sql,
        bool fromDataAnnotation = false)
    {
        if (indexBuilder.CanSetFilter(sql, fromDataAnnotation))
        {
            indexBuilder.Metadata.SetFilter(sql, fromDataAnnotation);
            return indexBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given expression can be set as the filter for the index.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-indexes">Indexes</see> for more information and examples.
    /// </remarks>
    /// <param name="indexBuilder">The builder for the index being configured.</param>
    /// <param name="sql">The filter expression for the index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given name can be set for the index.</returns>
    public static bool CanSetFilter(
        this IConventionIndexBuilder indexBuilder,
        string? sql,
        bool fromDataAnnotation = false)
        => indexBuilder.CanSetAnnotation(RelationalAnnotationNames.Filter, sql, fromDataAnnotation);
}
