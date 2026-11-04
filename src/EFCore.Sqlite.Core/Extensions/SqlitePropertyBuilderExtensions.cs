// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQLite-specific extension methods for <see cref="PropertyBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
/// </remarks>
public static class SqlitePropertyBuilderExtensions
{
    /// <summary>
    ///     Configures the property to use SQLite AUTOINCREMENT feature to generate values for new entities,
    ///     when targeting SQLite. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     AUTOINCREMENT can only be used on integer primary key columns in SQLite.
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder UseAutoincrement(this PropertyBuilder propertyBuilder)
    {
        propertyBuilder.Metadata.SetValueGenerationStrategy(SqliteValueGenerationStrategy.Autoincrement);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the property to use SQLite AUTOINCREMENT feature to generate values for new entities,
    ///     when targeting SQLite. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     AUTOINCREMENT can only be used on integer primary key columns in SQLite.
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder<TProperty> UseAutoincrement<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder)
        => (PropertyBuilder<TProperty>)UseAutoincrement((PropertyBuilder)propertyBuilder);

    /// <summary>
    ///     Configures the property to use SQLite AUTOINCREMENT feature to generate values for new entities,
    ///     when targeting SQLite. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     AUTOINCREMENT can only be used on integer primary key columns in SQLite.
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="columnBuilder">The builder for the column being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ColumnBuilder UseAutoincrement(
        this ColumnBuilder columnBuilder)
    {
        columnBuilder.Overrides.SetValueGenerationStrategy(SqliteValueGenerationStrategy.Autoincrement);

        return columnBuilder;
    }

    /// <summary>
    ///     Configures the value generation strategy for the property when targeting SQLite.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="strategy">The strategy to use.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? HasValueGenerationStrategy(
        this IConventionPropertyBuilder propertyBuilder,
        SqliteValueGenerationStrategy? strategy,
        bool fromDataAnnotation = false)
    {
        if (propertyBuilder.CanSetValueGenerationStrategy(strategy, fromDataAnnotation))
        {
            propertyBuilder.Metadata.SetValueGenerationStrategy(strategy, fromDataAnnotation);
            return propertyBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value generation strategy can be set for the property.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="strategy">The strategy.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value generation strategy can be set for the property.</returns>
    public static bool CanSetValueGenerationStrategy(
        this IConventionPropertyBuilder propertyBuilder,
        SqliteValueGenerationStrategy? strategy,
        bool fromDataAnnotation = false)
        => propertyBuilder.CanSetAnnotation(
            SqliteAnnotationNames.ValueGenerationStrategy, strategy, fromDataAnnotation);

    /// <summary>
    ///     Configures the SRID of the column that the property maps to when targeting SQLite.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-spatial">Spatial data</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="srid">The SRID.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder HasSrid(this PropertyBuilder propertyBuilder, int srid)
    {
        propertyBuilder.Metadata.SetSrid(srid);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the SRID of the column that the property maps to when targeting SQLite.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-spatial">Spatial data</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="srid">The SRID.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder<TProperty> HasSrid<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        int srid)
        => (PropertyBuilder<TProperty>)HasSrid((PropertyBuilder)propertyBuilder, srid);

    /// <summary>
    ///     Configures the SRID of the column that the property maps to when targeting SQLite.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-spatial">Spatial data</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="srid">The SRID.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? HasSrid(
        this IConventionPropertyBuilder propertyBuilder,
        int? srid,
        bool fromDataAnnotation = false)
    {
        if (propertyBuilder.CanSetSrid(srid, fromDataAnnotation))
        {
            propertyBuilder.Metadata.SetSrid(srid, fromDataAnnotation);

            return propertyBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the SRID for the column.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-spatial">Spatial data</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="srid">The SRID.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as the SRID for the column.</returns>
    public static bool CanSetSrid(
        this IConventionPropertyBuilder propertyBuilder,
        int? srid,
        bool fromDataAnnotation = false)
        => propertyBuilder.CanSetAnnotation(
            SqliteAnnotationNames.Srid,
            srid,
            fromDataAnnotation);
}
