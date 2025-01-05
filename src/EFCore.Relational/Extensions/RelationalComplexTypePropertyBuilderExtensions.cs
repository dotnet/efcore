// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="ComplexTypePropertyBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalComplexTypePropertyBuilderExtensions
{
    /// <summary>
    ///     Configures the column that the property maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder HasColumnName(
        this ComplexTypePropertyBuilder propertyBuilder,
        string? name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        propertyBuilder.Metadata.SetColumnName(name);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the column that the property maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder<TProperty> HasColumnName<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder,
        string? name)
        => (ComplexTypePropertyBuilder<TProperty>)HasColumnName((ComplexTypePropertyBuilder)propertyBuilder, name);

    /// <summary>
    ///     Configures the order of the column the property is mapped to.
    /// </summary>
    /// <param name="propertyBuilder">The builder of the property being configured.</param>
    /// <param name="order">The column order.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder HasColumnOrder(this ComplexTypePropertyBuilder propertyBuilder, int? order)
    {
        propertyBuilder.Metadata.SetColumnOrder(order);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the order of the column the property is mapped to.
    /// </summary>
    /// <param name="propertyBuilder">The builder of the property being configured.</param>
    /// <param name="order">The column order.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder<TProperty> HasColumnOrder<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder,
        int? order)
        => (ComplexTypePropertyBuilder<TProperty>)HasColumnOrder((ComplexTypePropertyBuilder)propertyBuilder, order);

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
    public static ComplexTypePropertyBuilder HasColumnType(
        this ComplexTypePropertyBuilder propertyBuilder,
        string? typeName)
    {
        Check.NullButNotEmpty(typeName, nameof(typeName));

        propertyBuilder.Metadata.SetColumnType(typeName);

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
    public static ComplexTypePropertyBuilder<TProperty> HasColumnType<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder,
        string? typeName)
        => (ComplexTypePropertyBuilder<TProperty>)HasColumnType((ComplexTypePropertyBuilder)propertyBuilder, typeName);

    /// <summary>
    ///     Configures the property as capable of storing only fixed-length data, such as strings.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="fixedLength">A value indicating whether the property is constrained to fixed length values.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public static ComplexTypePropertyBuilder IsFixedLength(
        this ComplexTypePropertyBuilder propertyBuilder,
        bool fixedLength = true)
    {
        propertyBuilder.Metadata.SetIsFixedLength(fixedLength);

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
    public static ComplexTypePropertyBuilder<TProperty> IsFixedLength<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder,
        bool fixedLength = true)
        => (ComplexTypePropertyBuilder<TProperty>)IsFixedLength((ComplexTypePropertyBuilder)propertyBuilder, fixedLength);

    /// <summary>
    ///     Configures the default value expression for the column that the property maps to when targeting a
    ///     relational database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When called with no argument, this method tells EF that a column has a default value constraint of
    ///         some sort without needing to specify exactly what it is. This can be useful when mapping EF to an
    ///         existing database.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder HasDefaultValueSql(this ComplexTypePropertyBuilder propertyBuilder)
    {
        propertyBuilder.Metadata.SetDefaultValueSql(string.Empty);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the default value expression for the column that the property maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression for the default value of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder HasDefaultValueSql(
        this ComplexTypePropertyBuilder propertyBuilder,
        string? sql)
    {
        Check.NullButNotEmpty(sql, nameof(sql));

        propertyBuilder.Metadata.SetDefaultValueSql(sql);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the default value expression for the column that the property maps to when targeting a
    ///     relational database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When called with no argument, this method tells EF that a column has a default value constraint of
    ///         some sort without needing to specify exactly what it is. This can be useful when mapping EF to an
    ///         existing database.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder<TProperty> HasDefaultValueSql<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder)
        => (ComplexTypePropertyBuilder<TProperty>)HasDefaultValueSql((ComplexTypePropertyBuilder)propertyBuilder);

    /// <summary>
    ///     Configures the default value expression for the column that the property maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression for the default value of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder<TProperty> HasDefaultValueSql<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder,
        string? sql)
        => (ComplexTypePropertyBuilder<TProperty>)HasDefaultValueSql((ComplexTypePropertyBuilder)propertyBuilder, sql);

    /// <summary>
    ///     Configures the property to map to a computed column when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When called with no arguments, this method tells EF that a column is computed without needing to
    ///         specify the actual SQL used to computed it. This can be useful when mapping EF to an existing
    ///         database.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder HasComputedColumnSql(this ComplexTypePropertyBuilder propertyBuilder)
    {
        propertyBuilder.Metadata.SetComputedColumnSql(string.Empty);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the property to map to a computed column when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression that computes values for the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder HasComputedColumnSql(
        this ComplexTypePropertyBuilder propertyBuilder,
        string? sql)
        => HasComputedColumnSql(propertyBuilder, sql, null);

    /// <summary>
    ///     Configures the property to map to a computed column when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression that computes values for the column.</param>
    /// <param name="stored">
    ///     If <see langword="true" />, the computed value is calculated on row modification and stored in the database like a regular column.
    ///     If <see langword="false" />, the value is computed when the value is read, and does not occupy any actual storage.
    ///     <see langword="null" /> selects the database provider default.
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder HasComputedColumnSql(
        this ComplexTypePropertyBuilder propertyBuilder,
        string? sql,
        bool? stored)
    {
        Check.NullButNotEmpty(sql, nameof(sql));

        propertyBuilder.Metadata.SetComputedColumnSql(sql);

        if (stored != null)
        {
            propertyBuilder.Metadata.SetIsStored(stored);
        }

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the property to map to a computed column when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When called with no arguments, this method tells EF that a column is computed without needing to
    ///         specify the actual SQL used to computed it. This can be useful when mapping EF to an existing
    ///         database.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder<TProperty> HasComputedColumnSql<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder)
        => (ComplexTypePropertyBuilder<TProperty>)HasComputedColumnSql((ComplexTypePropertyBuilder)propertyBuilder);

    /// <summary>
    ///     Configures the property to map to a computed column when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression that computes values for the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder<TProperty> HasComputedColumnSql<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder,
        string? sql)
        => HasComputedColumnSql(propertyBuilder, sql, null);

    /// <summary>
    ///     Configures the property to map to a computed column when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression that computes values for the column.</param>
    /// <param name="stored">
    ///     If <see langword="true" />, the computed value is calculated on row modification and stored in the database like a regular column.
    ///     If <see langword="false" />, the value is computed when the value is read, and does not occupy any actual storage.
    ///     <see langword="null" /> selects the database provider default.
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder<TProperty> HasComputedColumnSql<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder,
        string? sql,
        bool? stored)
        => (ComplexTypePropertyBuilder<TProperty>)HasComputedColumnSql((ComplexTypePropertyBuilder)propertyBuilder, sql, stored);

    /// <summary>
    ///     Configures the default value for the column that the property maps
    ///     to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When called with no argument, this method tells EF that a column has a default
    ///         value constraint of some sort without needing to specify exactly what it is.
    ///         This can be useful when mapping EF to an existing database.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder HasDefaultValue(this ComplexTypePropertyBuilder propertyBuilder)
    {
        propertyBuilder.Metadata.SetDefaultValue(DBNull.Value);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the default value for the column that the property maps
    ///     to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="value">The default value of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder HasDefaultValue(
        this ComplexTypePropertyBuilder propertyBuilder,
        object? value)
    {
        propertyBuilder.Metadata.SetDefaultValue(value);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the default value for the column that the property maps
    ///     to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When called with no argument, this method tells EF that a column has a default
    ///         value constraint of some sort without needing to specify exactly what it is.
    ///         This can be useful when mapping EF to an existing database.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder<TProperty> HasDefaultValue<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder)
        => (ComplexTypePropertyBuilder<TProperty>)HasDefaultValue((ComplexTypePropertyBuilder)propertyBuilder);

    /// <summary>
    ///     Configures the default value for the column that the property maps
    ///     to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="value">The default value of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder<TProperty> HasDefaultValue<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder,
        object? value)
        => (ComplexTypePropertyBuilder<TProperty>)HasDefaultValue((ComplexTypePropertyBuilder)propertyBuilder, value);

    /// <summary>
    ///     Configures a comment to be applied to the column
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="comment">The comment for the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder HasComment(
        this ComplexTypePropertyBuilder propertyBuilder,
        string? comment)
    {
        propertyBuilder.Metadata.SetComment(comment);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures a comment to be applied to the column
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="comment">The comment for the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder<TProperty> HasComment<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder,
        string? comment)
        => (ComplexTypePropertyBuilder<TProperty>)HasComment((ComplexTypePropertyBuilder)propertyBuilder, comment);

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
    public static ComplexTypePropertyBuilder UseCollation(this ComplexTypePropertyBuilder propertyBuilder, string? collation)
    {
        Check.NullButNotEmpty(collation, nameof(collation));

        propertyBuilder.Metadata.SetCollation(collation);

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
    public static ComplexTypePropertyBuilder<TProperty> UseCollation<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder,
        string? collation)
        => (ComplexTypePropertyBuilder<TProperty>)UseCollation((ComplexTypePropertyBuilder)propertyBuilder, collation);

    /// <summary>
    ///     Configures the property of an entity mapped to a JSON column, mapping the entity property to a specific JSON property,
    ///     rather than using the entity property name.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder HasJsonPropertyName(
        this ComplexTypePropertyBuilder propertyBuilder,
        string? name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        propertyBuilder.Metadata.SetJsonPropertyName(name);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the property of an entity mapped to a JSON column, mapping the entity property to a specific JSON property,
    ///     rather than using the entity property name.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePropertyBuilder<TProperty> HasJsonPropertyName<TProperty>(
        this ComplexTypePropertyBuilder<TProperty> propertyBuilder,
        string? name)
        => (ComplexTypePropertyBuilder<TProperty>)HasJsonPropertyName((ComplexTypePropertyBuilder)propertyBuilder, name);
}
