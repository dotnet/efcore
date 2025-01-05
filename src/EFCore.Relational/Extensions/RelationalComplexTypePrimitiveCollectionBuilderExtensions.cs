// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="ComplexTypePrimitiveCollectionBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalComplexTypePrimitiveCollectionBuilderExtensions
{
    /// <summary>
    ///     Configures the column that the property maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder HasColumnName(
        this ComplexTypePrimitiveCollectionBuilder primitiveCollectionBuilder,
        string? name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        primitiveCollectionBuilder.Metadata.SetColumnName(name);

        return primitiveCollectionBuilder;
    }

    /// <summary>
    ///     Configures the column that the property maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder<TProperty> HasColumnName<TProperty>(
        this ComplexTypePrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder,
        string? name)
        => (ComplexTypePrimitiveCollectionBuilder<TProperty>)HasColumnName(
            (ComplexTypePrimitiveCollectionBuilder)primitiveCollectionBuilder, name);

    /// <summary>
    ///     Configures the order of the column the property is mapped to.
    /// </summary>
    /// <param name="primitiveCollectionBuilder">The builder of the property being configured.</param>
    /// <param name="order">The column order.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder HasColumnOrder(
        this ComplexTypePrimitiveCollectionBuilder primitiveCollectionBuilder,
        int? order)
    {
        primitiveCollectionBuilder.Metadata.SetColumnOrder(order);

        return primitiveCollectionBuilder;
    }

    /// <summary>
    ///     Configures the order of the column the property is mapped to.
    /// </summary>
    /// <param name="primitiveCollectionBuilder">The builder of the property being configured.</param>
    /// <param name="order">The column order.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder<TProperty> HasColumnOrder<TProperty>(
        this ComplexTypePrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder,
        int? order)
        => (ComplexTypePrimitiveCollectionBuilder<TProperty>)HasColumnOrder(
            (ComplexTypePrimitiveCollectionBuilder)primitiveCollectionBuilder, order);

    /// <summary>
    ///     Configures the data type of the column that the property maps to when targeting a relational database.
    ///     This should be the complete type name, including precision, scale, length, etc.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="typeName">The name of the data type of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder HasColumnType(
        this ComplexTypePrimitiveCollectionBuilder primitiveCollectionBuilder,
        string? typeName)
    {
        Check.NullButNotEmpty(typeName, nameof(typeName));

        primitiveCollectionBuilder.Metadata.SetColumnType(typeName);

        return primitiveCollectionBuilder;
    }

    /// <summary>
    ///     Configures the data type of the column that the property maps to when targeting a relational database.
    ///     This should be the complete type name, including precision, scale, length, etc.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="typeName">The name of the data type of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder<TProperty> HasColumnType<TProperty>(
        this ComplexTypePrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder,
        string? typeName)
        => (ComplexTypePrimitiveCollectionBuilder<TProperty>)HasColumnType(
            (ComplexTypePrimitiveCollectionBuilder)primitiveCollectionBuilder, typeName);

    /// <summary>
    ///     Configures the property as capable of storing only fixed-length data, such as strings.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="fixedLength">A value indicating whether the property is constrained to fixed length values.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder IsFixedLength(
        this ComplexTypePrimitiveCollectionBuilder primitiveCollectionBuilder,
        bool fixedLength = true)
    {
        primitiveCollectionBuilder.Metadata.SetIsFixedLength(fixedLength);

        return primitiveCollectionBuilder;
    }

    /// <summary>
    ///     Configures the property as capable of storing only fixed-length data, such as strings.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="fixedLength">A value indicating whether the property is constrained to fixed length values.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder<TProperty> IsFixedLength<TProperty>(
        this ComplexTypePrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder,
        bool fixedLength = true)
        => (ComplexTypePrimitiveCollectionBuilder<TProperty>)IsFixedLength(
            (ComplexTypePrimitiveCollectionBuilder)primitiveCollectionBuilder, fixedLength);

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
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder HasDefaultValueSql(
        this ComplexTypePrimitiveCollectionBuilder primitiveCollectionBuilder)
    {
        primitiveCollectionBuilder.Metadata.SetDefaultValueSql(string.Empty);

        return primitiveCollectionBuilder;
    }

    /// <summary>
    ///     Configures the default value expression for the column that the property maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression for the default value of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder HasDefaultValueSql(
        this ComplexTypePrimitiveCollectionBuilder primitiveCollectionBuilder,
        string? sql)
    {
        Check.NullButNotEmpty(sql, nameof(sql));

        primitiveCollectionBuilder.Metadata.SetDefaultValueSql(sql);

        return primitiveCollectionBuilder;
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
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder<TProperty> HasDefaultValueSql<TProperty>(
        this ComplexTypePrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder)
        => (ComplexTypePrimitiveCollectionBuilder<TProperty>)HasDefaultValueSql(
            (ComplexTypePrimitiveCollectionBuilder)primitiveCollectionBuilder);

    /// <summary>
    ///     Configures the default value expression for the column that the property maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression for the default value of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder<TProperty> HasDefaultValueSql<TProperty>(
        this ComplexTypePrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder,
        string? sql)
        => (ComplexTypePrimitiveCollectionBuilder<TProperty>)HasDefaultValueSql(
            (ComplexTypePrimitiveCollectionBuilder)primitiveCollectionBuilder, sql);

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
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder HasComputedColumnSql(
        this ComplexTypePrimitiveCollectionBuilder primitiveCollectionBuilder)
    {
        primitiveCollectionBuilder.Metadata.SetComputedColumnSql(string.Empty);

        return primitiveCollectionBuilder;
    }

    /// <summary>
    ///     Configures the property to map to a computed column when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression that computes values for the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder HasComputedColumnSql(
        this ComplexTypePrimitiveCollectionBuilder primitiveCollectionBuilder,
        string? sql)
        => HasComputedColumnSql(primitiveCollectionBuilder, sql, null);

    /// <summary>
    ///     Configures the property to map to a computed column when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression that computes values for the column.</param>
    /// <param name="stored">
    ///     If <see langword="true" />, the computed value is calculated on row modification and stored in the database like a regular column.
    ///     If <see langword="false" />, the value is computed when the value is read, and does not occupy any actual storage.
    ///     <see langword="null" /> selects the database provider default.
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder HasComputedColumnSql(
        this ComplexTypePrimitiveCollectionBuilder primitiveCollectionBuilder,
        string? sql,
        bool? stored)
    {
        Check.NullButNotEmpty(sql, nameof(sql));

        primitiveCollectionBuilder.Metadata.SetComputedColumnSql(sql);

        if (stored != null)
        {
            primitiveCollectionBuilder.Metadata.SetIsStored(stored);
        }

        return primitiveCollectionBuilder;
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
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder<TProperty> HasComputedColumnSql<TProperty>(
        this ComplexTypePrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder)
        => (ComplexTypePrimitiveCollectionBuilder<TProperty>)HasComputedColumnSql(
            (ComplexTypePrimitiveCollectionBuilder)primitiveCollectionBuilder);

    /// <summary>
    ///     Configures the property to map to a computed column when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression that computes values for the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder<TProperty> HasComputedColumnSql<TProperty>(
        this ComplexTypePrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder,
        string? sql)
        => HasComputedColumnSql(primitiveCollectionBuilder, sql, null);

    /// <summary>
    ///     Configures the property to map to a computed column when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression that computes values for the column.</param>
    /// <param name="stored">
    ///     If <see langword="true" />, the computed value is calculated on row modification and stored in the database like a regular column.
    ///     If <see langword="false" />, the value is computed when the value is read, and does not occupy any actual storage.
    ///     <see langword="null" /> selects the database provider default.
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder<TProperty> HasComputedColumnSql<TProperty>(
        this ComplexTypePrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder,
        string? sql,
        bool? stored)
        => (ComplexTypePrimitiveCollectionBuilder<TProperty>)HasComputedColumnSql(
            (ComplexTypePrimitiveCollectionBuilder)primitiveCollectionBuilder, sql, stored);

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
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder HasDefaultValue(
        this ComplexTypePrimitiveCollectionBuilder primitiveCollectionBuilder)
    {
        primitiveCollectionBuilder.Metadata.SetDefaultValue(DBNull.Value);

        return primitiveCollectionBuilder;
    }

    /// <summary>
    ///     Configures the default value for the column that the property maps
    ///     to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="value">The default value of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder HasDefaultValue(
        this ComplexTypePrimitiveCollectionBuilder primitiveCollectionBuilder,
        object? value)
    {
        primitiveCollectionBuilder.Metadata.SetDefaultValue(value);

        return primitiveCollectionBuilder;
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
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder<TProperty> HasDefaultValue<TProperty>(
        this ComplexTypePrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder)
        => (ComplexTypePrimitiveCollectionBuilder<TProperty>)HasDefaultValue(
            (ComplexTypePrimitiveCollectionBuilder)primitiveCollectionBuilder);

    /// <summary>
    ///     Configures the default value for the column that the property maps
    ///     to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="value">The default value of the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder<TProperty> HasDefaultValue<TProperty>(
        this ComplexTypePrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder,
        object? value)
        => (ComplexTypePrimitiveCollectionBuilder<TProperty>)HasDefaultValue(
            (ComplexTypePrimitiveCollectionBuilder)primitiveCollectionBuilder, value);

    /// <summary>
    ///     Configures a comment to be applied to the column
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="comment">The comment for the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder HasComment(
        this ComplexTypePrimitiveCollectionBuilder primitiveCollectionBuilder,
        string? comment)
    {
        primitiveCollectionBuilder.Metadata.SetComment(comment);

        return primitiveCollectionBuilder;
    }

    /// <summary>
    ///     Configures a comment to be applied to the column
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="comment">The comment for the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder<TProperty> HasComment<TProperty>(
        this ComplexTypePrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder,
        string? comment)
        => (ComplexTypePrimitiveCollectionBuilder<TProperty>)HasComment(
            (ComplexTypePrimitiveCollectionBuilder)primitiveCollectionBuilder, comment);

    /// <summary>
    ///     Configures the property to use the given collation. The database column will be created with the given
    ///     collation, and it will be used implicitly in all collation-sensitive operations.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-collations">Database collations</see> for more information and examples.
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="collation">The collation for the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder UseCollation(
        this ComplexTypePrimitiveCollectionBuilder primitiveCollectionBuilder,
        string? collation)
    {
        Check.NullButNotEmpty(collation, nameof(collation));

        primitiveCollectionBuilder.Metadata.SetCollation(collation);

        return primitiveCollectionBuilder;
    }

    /// <summary>
    ///     Configures the property to use the given collation. The database column will be created with the given
    ///     collation, and it will be used implicitly in all collation-sensitive operations.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-collations">Database collations</see> for more information and examples.
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="collation">The collation for the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder<TProperty> UseCollation<TProperty>(
        this ComplexTypePrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder,
        string? collation)
        => (ComplexTypePrimitiveCollectionBuilder<TProperty>)UseCollation(
            (ComplexTypePrimitiveCollectionBuilder)primitiveCollectionBuilder, collation);

    /// <summary>
    ///     Configures the property of an entity mapped to a JSON column, mapping the entity property to a specific JSON property,
    ///     rather than using the entity property name.
    /// </summary>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder HasJsonPropertyName(
        this ComplexTypePrimitiveCollectionBuilder primitiveCollectionBuilder,
        string? name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        primitiveCollectionBuilder.Metadata.SetJsonPropertyName(name);

        return primitiveCollectionBuilder;
    }

    /// <summary>
    ///     Configures the property of an entity mapped to a JSON column, mapping the entity property to a specific JSON property,
    ///     rather than using the entity property name.
    /// </summary>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ComplexTypePrimitiveCollectionBuilder<TProperty> HasJsonPropertyName<TProperty>(
        this ComplexTypePrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder,
        string? name)
        => (ComplexTypePrimitiveCollectionBuilder<TProperty>)HasJsonPropertyName(
            (ComplexTypePrimitiveCollectionBuilder)primitiveCollectionBuilder, name);
}
