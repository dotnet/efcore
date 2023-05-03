// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="TypeMappingConfigurationBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalTypeMappingConfigurationBuilderExtensions
{
    /// <summary>
    ///     Configures the data type of the column that the scalar maps to when targeting a relational database.
    ///     This should be the complete type name, including precision, scale, length, etc.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="scalarBuilder">The builder for the scalar being configured.</param>
    /// <param name="typeName">The name of the data type of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static TypeMappingConfigurationBuilder HasColumnType(
        this TypeMappingConfigurationBuilder scalarBuilder,
        string typeName)
    {
        Check.NotEmpty(typeName, nameof(typeName));

        scalarBuilder.HasAnnotation(RelationalAnnotationNames.ColumnType, typeName);

        return scalarBuilder;
    }

    /// <summary>
    ///     Configures the data type of the column that the scalar maps to when targeting a relational database.
    ///     This should be the complete type name, including precision, scale, length, etc.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TScalar">The type of the scalar being configured.</typeparam>
    /// <param name="scalarBuilder">The builder for the scalar being configured.</param>
    /// <param name="typeName">The name of the data type of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static TypeMappingConfigurationBuilder<TScalar> HasColumnType<TScalar>(
        this TypeMappingConfigurationBuilder<TScalar> scalarBuilder,
        string typeName)
        => (TypeMappingConfigurationBuilder<TScalar>)HasColumnType((TypeMappingConfigurationBuilder)scalarBuilder, typeName);

    /// <summary>
    ///     Configures the scalar as capable of storing only fixed-length data, such as strings.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="scalarBuilder">The builder for the scalar being configured.</param>
    /// <param name="fixedLength">A value indicating whether the scalar is constrained to fixed length values.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public static TypeMappingConfigurationBuilder IsFixedLength(
        this TypeMappingConfigurationBuilder scalarBuilder,
        bool fixedLength = true)
    {
        scalarBuilder.HasAnnotation(RelationalAnnotationNames.IsFixedLength, fixedLength);

        return scalarBuilder;
    }

    /// <summary>
    ///     Configures the scalar as capable of storing only fixed-length data, such as strings.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TScalar">The type of the scalar being configured.</typeparam>
    /// <param name="scalarBuilder">The builder for the scalar being configured.</param>
    /// <param name="fixedLength">A value indicating whether the scalar is constrained to fixed length values.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public static TypeMappingConfigurationBuilder<TScalar> IsFixedLength<TScalar>(
        this TypeMappingConfigurationBuilder<TScalar> scalarBuilder,
        bool fixedLength = true)
        => (TypeMappingConfigurationBuilder<TScalar>)IsFixedLength((TypeMappingConfigurationBuilder)scalarBuilder, fixedLength);
}
