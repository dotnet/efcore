// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="ComplexPropertyBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalComplexPropertyBuilderExtensions
{
    /// <summary>
    ///     Configures the complex property to be stored as a JSON column.
    /// </summary>
    /// <param name="complexPropertyBuilder">The builder for the complex property being configured.</param>
    /// <param name="jsonColumnName">The name of the JSON column. If not specified, the complex property name is used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexPropertyBuilder ToJson(
        this ComplexPropertyBuilder complexPropertyBuilder,
        string? jsonColumnName = null)
    {
        Check.NullButNotEmpty(jsonColumnName);

        var complexProperty = complexPropertyBuilder.Metadata;

        complexProperty.ComplexType.SetContainerColumnName(jsonColumnName ?? complexProperty.Name);

        return complexPropertyBuilder;
    }

    /// <summary>
    ///     Configures the complex property to be stored as a JSON column.
    /// </summary>
    /// <param name="complexPropertyBuilder">The builder for the complex property being configured.</param>
    /// <param name="jsonColumnName">The name of the JSON column. If not specified, the complex property name is used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexPropertyBuilder<TComplex> ToJson<TComplex>(
        this ComplexPropertyBuilder<TComplex> complexPropertyBuilder,
        string? jsonColumnName = null)
        where TComplex : class
        => (ComplexPropertyBuilder<TComplex>)ToJson((ComplexPropertyBuilder)complexPropertyBuilder, jsonColumnName);

    /// <summary>
    ///     Configures the complex property of an entity mapped to a JSON column, mapping the complex property to a specific JSON property,
    ///     rather than using the complex property name.
    /// </summary>
    /// <param name="complexPropertyBuilder">The builder for the complex property being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexPropertyBuilder HasJsonPropertyName(
        this ComplexPropertyBuilder complexPropertyBuilder,
        string? name)
    {
        Check.NullButNotEmpty(name);

        complexPropertyBuilder.Metadata.SetJsonPropertyName(name);

        return complexPropertyBuilder;
    }

    /// <summary>
    ///     Configures the complex property of an entity mapped to a JSON column, mapping the complex property to a specific JSON property,
    ///     rather than using the complex property name.
    /// </summary>
    /// <param name="complexPropertyBuilder">The builder for the complex property being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexPropertyBuilder<TComplex> HasJsonPropertyName<TComplex>(
        this ComplexPropertyBuilder<TComplex> complexPropertyBuilder,
        string? name)
        where TComplex : notnull
        => (ComplexPropertyBuilder<TComplex>)HasJsonPropertyName((ComplexPropertyBuilder)complexPropertyBuilder, name);

    /// <summary>
    ///     Configures the column type for the JSON column that stores the complex property.
    /// </summary>
    /// <param name="complexPropertyBuilder">The builder for the complex property being configured.</param>
    /// <param name="columnType">The database type for the column, or <see langword="null" /> to use the database default.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexPropertyBuilder HasColumnType(
        this ComplexPropertyBuilder complexPropertyBuilder,
        string? columnType)
    {
        complexPropertyBuilder.Metadata.ComplexType.SetContainerColumnType(columnType);

        return complexPropertyBuilder;
    }

    /// <summary>
    ///     Configures the column type for the JSON column that stores the complex property.
    /// </summary>
    /// <param name="complexPropertyBuilder">The builder for the complex property being configured.</param>
    /// <param name="columnType">The database type for the column, or <see langword="null" /> to use the database default.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexPropertyBuilder<TComplex> HasColumnType<TComplex>(
        this ComplexPropertyBuilder<TComplex> complexPropertyBuilder,
        string? columnType)
        where TComplex : notnull
        => (ComplexPropertyBuilder<TComplex>)HasColumnType((ComplexPropertyBuilder)complexPropertyBuilder, columnType);
}
