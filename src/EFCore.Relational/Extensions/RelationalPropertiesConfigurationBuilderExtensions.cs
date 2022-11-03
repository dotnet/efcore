// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="PropertiesConfigurationBuilder" />.
/// </summary>
public static class RelationalPropertiesConfigurationBuilderExtensions
{
    /// <summary>
    ///     Configures the data type of the column that the property maps to when targeting a relational database.
    ///     This should be the complete type name, including precision, scale, length, etc.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="typeName">The name of the data type of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertiesConfigurationBuilder HaveColumnType(
        this PropertiesConfigurationBuilder propertyBuilder,
        string typeName)
    {
        Check.NotEmpty(typeName, nameof(typeName));

        propertyBuilder.HaveAnnotation(RelationalAnnotationNames.ColumnType, typeName);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the data type of the column that the property maps to when targeting a relational database.
    ///     This should be the complete type name, including precision, scale, length, etc.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="typeName">The name of the data type of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertiesConfigurationBuilder<TProperty> HaveColumnType<TProperty>(
        this PropertiesConfigurationBuilder<TProperty> propertyBuilder,
        string typeName)
        => (PropertiesConfigurationBuilder<TProperty>)HaveColumnType((PropertiesConfigurationBuilder)propertyBuilder, typeName);

    /// <summary>
    ///     Configures the property as capable of storing only fixed-length data, such as strings.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="fixedLength">A value indicating whether the property is constrained to fixed length values.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public static PropertiesConfigurationBuilder AreFixedLength(
        this PropertiesConfigurationBuilder propertyBuilder,
        bool fixedLength = true)
    {
        propertyBuilder.HaveAnnotation(RelationalAnnotationNames.IsFixedLength, fixedLength);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the property as capable of storing only fixed-length data, such as strings.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="fixedLength">A value indicating whether the property is constrained to fixed length values.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public static PropertiesConfigurationBuilder<TProperty> AreFixedLength<TProperty>(
        this PropertiesConfigurationBuilder<TProperty> propertyBuilder,
        bool fixedLength = true)
        => (PropertiesConfigurationBuilder<TProperty>)AreFixedLength((PropertiesConfigurationBuilder)propertyBuilder, fixedLength);

    /// <summary>
    ///     Configures the property to use the given collation. The database column will be created with the given
    ///     collation, and it will be used implicitly in all collation-sensitive operations.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-collations">Database collations</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="collation">The collation for the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertiesConfigurationBuilder UseCollation(this PropertiesConfigurationBuilder propertyBuilder, string collation)
    {
        Check.NotEmpty(collation, nameof(collation));

        propertyBuilder.HaveAnnotation(RelationalAnnotationNames.Collation, collation);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the property to use the given collation. The database column will be created with the given
    ///     collation, and it will be used implicitly in all collation-sensitive operations.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-collations">Database collations</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="collation">The collation for the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertiesConfigurationBuilder<TProperty> UseCollation<TProperty>(
        this PropertiesConfigurationBuilder<TProperty> propertyBuilder,
        string collation)
        => (PropertiesConfigurationBuilder<TProperty>)UseCollation((PropertiesConfigurationBuilder)propertyBuilder, collation);
}
