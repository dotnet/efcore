// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="ComplexCollectionBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalComplexCollectionBuilderExtensions
{
    /// <summary>
    ///     Configures the complex collection to be stored as a JSON column.
    /// </summary>
    /// <param name="complexCollectionBuilder">The builder for the complex collection being configured.</param>
    /// <param name="jsonColumnName">The name of the JSON column. If not specified, the complex collection name is used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexCollectionBuilder ToJson(
        this ComplexCollectionBuilder complexCollectionBuilder,
        string? jsonColumnName = null)
    {
        Check.NullButNotEmpty(jsonColumnName);

        var complexProperty = complexCollectionBuilder.Metadata;
        complexProperty.ComplexType.SetContainerColumnName(jsonColumnName ?? complexProperty.Name);

        return complexCollectionBuilder;
    }

    /// <summary>
    ///     Configures the complex collection to be stored as a JSON column.
    /// </summary>
    /// <param name="complexCollectionBuilder">The builder for the complex collection being configured.</param>
    /// <param name="jsonColumnName">The name of the JSON column. If not specified, the complex collection name is used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexCollectionBuilder<TComplex> ToJson<TComplex>(
        this ComplexCollectionBuilder<TComplex> complexCollectionBuilder,
        string? jsonColumnName = null)
        where TComplex : notnull
        => (ComplexCollectionBuilder<TComplex>)ToJson((ComplexCollectionBuilder)complexCollectionBuilder, jsonColumnName);

    /// <summary>
    ///     Configures the complex property contained in a JSON column to map to a specific JSON property,
    ///     rather than using the property name.
    /// </summary>
    /// <param name="complexCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexCollectionBuilder HasJsonPropertyName(
        this ComplexCollectionBuilder complexCollectionBuilder,
        string? name)
    {
        Check.NullButNotEmpty(name);

        complexCollectionBuilder.Metadata.SetJsonPropertyName(name);

        return complexCollectionBuilder;
    }

    /// <summary>
    ///     Configures the complex property contained in a JSON column to map to a specific JSON property,
    ///     rather than using the property name.
    /// </summary>
    /// <param name="complexCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexCollectionBuilder<TComplex> HasJsonPropertyName<TComplex>(
        this ComplexCollectionBuilder<TComplex> complexCollectionBuilder,
        string? name)
        where TComplex : notnull
        => (ComplexCollectionBuilder<TComplex>)HasJsonPropertyName((ComplexCollectionBuilder)complexCollectionBuilder, name);

    /// <summary>
    ///     Configures the column type for the JSON column that stores the complex collection.
    /// </summary>
    /// <param name="complexCollectionBuilder">The builder for the complex collection being configured.</param>
    /// <param name="columnType">The database type for the column, or <see langword="null" /> to use the database default.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexCollectionBuilder HasColumnType(
        this ComplexCollectionBuilder complexCollectionBuilder,
        string? columnType)
    {
        complexCollectionBuilder.Metadata.ComplexType.SetContainerColumnType(columnType);

        return complexCollectionBuilder;
    }

    /// <summary>
    ///     Configures the column type for the JSON column that stores the complex collection.
    /// </summary>
    /// <param name="complexCollectionBuilder">The builder for the complex collection being configured.</param>
    /// <param name="columnType">The database type for the column, or <see langword="null" /> to use the database default.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexCollectionBuilder<TComplex> HasColumnType<TComplex>(
        this ComplexCollectionBuilder<TComplex> complexCollectionBuilder,
        string? columnType)
        where TComplex : notnull
        => (ComplexCollectionBuilder<TComplex>)HasColumnType((ComplexCollectionBuilder)complexCollectionBuilder, columnType);
}
