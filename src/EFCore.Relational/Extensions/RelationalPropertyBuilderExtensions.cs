// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="PropertyBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalPropertyBuilderExtensions
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
    public static PropertyBuilder HasColumnName(
        this PropertyBuilder propertyBuilder,
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
    public static PropertyBuilder<TProperty> HasColumnName<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        string? name)
        => (PropertyBuilder<TProperty>)HasColumnName((PropertyBuilder)propertyBuilder, name);

    /// <summary>
    ///     Configures the column that the property maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? HasColumnName(
        this IConventionPropertyBuilder propertyBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        if (!propertyBuilder.CanSetColumnName(name, fromDataAnnotation))
        {
            return null;
        }

        propertyBuilder.Metadata.SetColumnName(name, fromDataAnnotation);
        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the column that the property maps to in a particular table-like store object.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the column.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? HasColumnName(
        this IConventionPropertyBuilder propertyBuilder,
        string? name,
        in StoreObjectIdentifier storeObject,
        bool fromDataAnnotation = false)
    {
        if (!propertyBuilder.CanSetColumnName(name, storeObject, fromDataAnnotation))
        {
            return null;
        }

        propertyBuilder.Metadata.SetColumnName(name, storeObject, fromDataAnnotation);
        return propertyBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the given column can be set for the property.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the property can be mapped to the given column.</returns>
    public static bool CanSetColumnName(
        this IConventionPropertyBuilder propertyBuilder,
        string? name,
        bool fromDataAnnotation = false)
        => propertyBuilder.CanSetAnnotation(RelationalAnnotationNames.ColumnName, name, fromDataAnnotation);

    /// <summary>
    ///     Returns a value indicating whether the given column for a particular table-like store object can be set for the property.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the column.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the property can be mapped to the given column.</returns>
    public static bool CanSetColumnName(
        this IConventionPropertyBuilder propertyBuilder,
        string? name,
        in StoreObjectIdentifier storeObject,
        bool fromDataAnnotation = false)
    {
        var overrides = (IConventionRelationalPropertyOverrides?)RelationalPropertyOverrides.Find(
            propertyBuilder.Metadata, storeObject);
        return overrides == null
            || (fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
            .Overrides(overrides.GetColumnNameConfigurationSource())
            || overrides.ColumnName == name;
    }

    /// <summary>
    ///     Configures the order of the column the property is mapped to.
    /// </summary>
    /// <param name="propertyBuilder">The builder of the property being configured.</param>
    /// <param name="order">The column order.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder HasColumnOrder(this PropertyBuilder propertyBuilder, int? order)
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
    public static PropertyBuilder<TProperty> HasColumnOrder<TProperty>(this PropertyBuilder<TProperty> propertyBuilder, int? order)
        => (PropertyBuilder<TProperty>)HasColumnOrder((PropertyBuilder)propertyBuilder, order);

    /// <summary>
    ///     Configures the order of the column the property is mapped to.
    /// </summary>
    /// <param name="propertyBuilder">The builder of the property being configured.</param>
    /// <param name="order">The column order.</param>
    /// <param name="fromDataAnnotation">A value indicating whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance if the configuration was applied, <see langword="null" /> otherwise.</returns>
    public static IConventionPropertyBuilder? HasColumnOrder(
        this IConventionPropertyBuilder propertyBuilder,
        int? order,
        bool fromDataAnnotation = false)
    {
        if (!propertyBuilder.CanSetColumnOrder(order, fromDataAnnotation))
        {
            return null;
        }

        propertyBuilder.Metadata.SetColumnOrder(order, fromDataAnnotation);

        return propertyBuilder;
    }

    /// <summary>
    ///     Gets a value indicating whether the given column order can be set for the property.
    /// </summary>
    /// <param name="propertyBuilder">The builder of the property being configured.</param>
    /// <param name="order">The column order.</param>
    /// <param name="fromDataAnnotation">A value indicating whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the column order can be set for the property.</returns>
    public static bool CanSetColumnOrder(this IConventionPropertyBuilder propertyBuilder, int? order, bool fromDataAnnotation = false)
        => propertyBuilder.CanSetAnnotation(RelationalAnnotationNames.ColumnOrder, order, fromDataAnnotation);

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
    public static PropertyBuilder HasColumnType(
        this PropertyBuilder propertyBuilder,
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
    public static PropertyBuilder<TProperty> HasColumnType<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        string? typeName)
        => (PropertyBuilder<TProperty>)HasColumnType((PropertyBuilder)propertyBuilder, typeName);

    /// <summary>
    ///     Configures the data type of the column that the property maps to when targeting a relational database.
    ///     This should be the complete type name, including precision, scale, length, etc.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="typeName">The name of the data type of the column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? HasColumnType(
        this IConventionPropertyBuilder propertyBuilder,
        string? typeName,
        bool fromDataAnnotation = false)
    {
        if (!propertyBuilder.CanSetColumnType(typeName, fromDataAnnotation))
        {
            return null;
        }

        propertyBuilder.Metadata.SetColumnType(typeName, fromDataAnnotation);
        return propertyBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the given data type can be set for the property.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="typeName">The name of the data type of the column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given data type can be set for the property.</returns>
    public static bool CanSetColumnType(
        this IConventionPropertyBuilder propertyBuilder,
        string? typeName,
        bool fromDataAnnotation = false)
        => propertyBuilder.CanSetAnnotation(RelationalAnnotationNames.ColumnType, typeName, fromDataAnnotation);

    /// <summary>
    ///     Configures the property as capable of storing only fixed-length data, such as strings.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="fixedLength">A value indicating whether the property is constrained to fixed length values.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public static PropertyBuilder IsFixedLength(
        this PropertyBuilder propertyBuilder,
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
    public static PropertyBuilder<TProperty> IsFixedLength<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        bool fixedLength = true)
        => (PropertyBuilder<TProperty>)IsFixedLength((PropertyBuilder)propertyBuilder, fixedLength);

    /// <summary>
    ///     Configures the property as capable of storing only fixed-length data, such as strings.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="fixedLength">A value indicating whether the property is constrained to fixed length values.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? IsFixedLength(
        this IConventionPropertyBuilder propertyBuilder,
        bool? fixedLength,
        bool fromDataAnnotation = false)
    {
        if (!propertyBuilder.CanSetIsFixedLength(fixedLength, fromDataAnnotation))
        {
            return null;
        }

        propertyBuilder.Metadata.SetIsFixedLength(fixedLength, fromDataAnnotation);
        return propertyBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the property can be configured as being fixed length or not.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="fixedLength">A value indicating whether the property is constrained to fixed length values.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the property can be configured as being fixed length or not.</returns>
    public static bool CanSetIsFixedLength(
        this IConventionPropertyBuilder propertyBuilder,
        bool? fixedLength,
        bool fromDataAnnotation = false)
        => propertyBuilder.CanSetAnnotation(RelationalAnnotationNames.IsFixedLength, fixedLength, fromDataAnnotation);

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
    public static PropertyBuilder HasDefaultValueSql(this PropertyBuilder propertyBuilder)
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
    public static PropertyBuilder HasDefaultValueSql(
        this PropertyBuilder propertyBuilder,
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
    public static PropertyBuilder<TProperty> HasDefaultValueSql<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder)
        => (PropertyBuilder<TProperty>)HasDefaultValueSql((PropertyBuilder)propertyBuilder);

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
    public static PropertyBuilder<TProperty> HasDefaultValueSql<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        string? sql)
        => (PropertyBuilder<TProperty>)HasDefaultValueSql((PropertyBuilder)propertyBuilder, sql);

    /// <summary>
    ///     Configures the default value expression for the column that the property maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression for the default value of the column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? HasDefaultValueSql(
        this IConventionPropertyBuilder propertyBuilder,
        string? sql,
        bool fromDataAnnotation = false)
    {
        if (!propertyBuilder.CanSetDefaultValueSql(sql, fromDataAnnotation))
        {
            return null;
        }

        propertyBuilder.Metadata.SetDefaultValueSql(sql, fromDataAnnotation);
        return propertyBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the given default value expression can be set for the column.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression for the default value of the column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given default value expression can be set for the column.</returns>
    public static bool CanSetDefaultValueSql(
        this IConventionPropertyBuilder propertyBuilder,
        string? sql,
        bool fromDataAnnotation = false)
        => propertyBuilder.CanSetAnnotation(
            RelationalAnnotationNames.DefaultValueSql,
            sql,
            fromDataAnnotation);

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
    public static PropertyBuilder HasComputedColumnSql(this PropertyBuilder propertyBuilder)
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
    public static PropertyBuilder HasComputedColumnSql(
        this PropertyBuilder propertyBuilder,
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
    public static PropertyBuilder HasComputedColumnSql(
        this PropertyBuilder propertyBuilder,
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
    public static PropertyBuilder<TProperty> HasComputedColumnSql<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder)
        => (PropertyBuilder<TProperty>)HasComputedColumnSql((PropertyBuilder)propertyBuilder);

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
    public static PropertyBuilder<TProperty> HasComputedColumnSql<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
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
    public static PropertyBuilder<TProperty> HasComputedColumnSql<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        string? sql,
        bool? stored)
        => (PropertyBuilder<TProperty>)HasComputedColumnSql((PropertyBuilder)propertyBuilder, sql, stored);

    /// <summary>
    ///     Configures the property to map to a computed column when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression that computes values for the column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? HasComputedColumnSql(
        this IConventionPropertyBuilder propertyBuilder,
        string? sql,
        bool fromDataAnnotation = false)
    {
        if (!propertyBuilder.CanSetComputedColumnSql(sql, fromDataAnnotation))
        {
            return null;
        }

        propertyBuilder.Metadata.SetComputedColumnSql(sql, fromDataAnnotation);
        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the property to map to a computed column of the given type when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="stored">
    ///     If <see langword="true" />, the computed value is calculated on row modification and stored in the database like a regular column.
    ///     If <see langword="false" />, the value is computed when the value is read, and does not occupy any actual storage.
    ///     <see langword="null" /> selects the database provider default.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied, <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? IsStoredComputedColumn(
        this IConventionPropertyBuilder propertyBuilder,
        bool? stored,
        bool fromDataAnnotation = false)
    {
        if (!propertyBuilder.CanSetIsStoredComputedColumn(stored, fromDataAnnotation))
        {
            return null;
        }

        propertyBuilder.Metadata.SetIsStored(stored, fromDataAnnotation);
        return propertyBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the given computed value SQL expression can be set for the column.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression that computes values for the column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given computed value SQL expression can be set for the column.</returns>
    public static bool CanSetComputedColumnSql(
        this IConventionPropertyBuilder propertyBuilder,
        string? sql,
        bool fromDataAnnotation = false)
        => propertyBuilder.CanSetAnnotation(
            RelationalAnnotationNames.ComputedColumnSql,
            sql,
            fromDataAnnotation);

    /// <summary>
    ///     Returns a value indicating whether the given computed column type can be set for the column.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="stored">
    ///     If <see langword="true" />, the computed value is calculated on row modification and stored in the database like a regular column.
    ///     If <see langword="false" />, the value is computed when the value is read, and does not occupy any actual storage.
    ///     <see langword="null" /> selects the database provider default.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given computed column type can be set for the column.</returns>
    public static bool CanSetIsStoredComputedColumn(
        this IConventionPropertyBuilder propertyBuilder,
        bool? stored,
        bool fromDataAnnotation = false)
        => propertyBuilder.CanSetAnnotation(
            RelationalAnnotationNames.IsStored,
            stored,
            fromDataAnnotation);

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
    public static PropertyBuilder HasDefaultValue(this PropertyBuilder propertyBuilder)
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
    public static PropertyBuilder HasDefaultValue(
        this PropertyBuilder propertyBuilder,
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
    public static PropertyBuilder<TProperty> HasDefaultValue<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder)
        => (PropertyBuilder<TProperty>)HasDefaultValue((PropertyBuilder)propertyBuilder);

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
    public static PropertyBuilder<TProperty> HasDefaultValue<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        object? value)
        => (PropertyBuilder<TProperty>)HasDefaultValue((PropertyBuilder)propertyBuilder, value);

    /// <summary>
    ///     Configures the default value for the column that the property maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="value">The default value of the column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? HasDefaultValue(
        this IConventionPropertyBuilder propertyBuilder,
        object? value,
        bool fromDataAnnotation = false)
    {
        if (!propertyBuilder.CanSetDefaultValue(value, fromDataAnnotation))
        {
            return null;
        }

        propertyBuilder.Metadata.SetDefaultValue(value, fromDataAnnotation);
        return propertyBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as default for the column.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="value">The default value of the column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as default for the column.</returns>
    public static bool CanSetDefaultValue(
        this IConventionPropertyBuilder propertyBuilder,
        object? value,
        bool fromDataAnnotation = false)
        => propertyBuilder.CanSetAnnotation(
            RelationalAnnotationNames.DefaultValue,
            value,
            fromDataAnnotation);

    /// <summary>
    ///     Configures a comment to be applied to the column
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="comment">The comment for the column.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder HasComment(
        this PropertyBuilder propertyBuilder,
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
    public static PropertyBuilder<TProperty> HasComment<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        string? comment)
        => (PropertyBuilder<TProperty>)HasComment((PropertyBuilder)propertyBuilder, comment);

    /// <summary>
    ///     Configures a comment to be applied to the column
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="comment">The comment for the column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? HasComment(
        this IConventionPropertyBuilder propertyBuilder,
        string? comment,
        bool fromDataAnnotation = false)
    {
        if (!propertyBuilder.CanSetComment(comment, fromDataAnnotation))
        {
            return null;
        }

        propertyBuilder.Metadata.SetComment(comment, fromDataAnnotation);
        return propertyBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as comment for the column.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="comment">The comment for the column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as default for the column.</returns>
    public static bool CanSetComment(
        this IConventionPropertyBuilder propertyBuilder,
        string? comment,
        bool fromDataAnnotation = false)
        => propertyBuilder.CanSetAnnotation(
            RelationalAnnotationNames.Comment,
            comment,
            fromDataAnnotation);

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
    public static PropertyBuilder UseCollation(this PropertyBuilder propertyBuilder, string? collation)
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
    public static PropertyBuilder<TProperty> UseCollation<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        string? collation)
        => (PropertyBuilder<TProperty>)UseCollation((PropertyBuilder)propertyBuilder, collation);

    /// <summary>
    ///     Configures the property to use the given collation. The database column will be created with the given
    ///     collation, and it will be used implicitly in all collation-sensitive operations.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-collations">Database collations</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="collation">The collation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? UseCollation(
        this IConventionPropertyBuilder propertyBuilder,
        string? collation,
        bool fromDataAnnotation = false)
    {
        if (propertyBuilder.CanSetCollation(collation, fromDataAnnotation))
        {
            propertyBuilder.Metadata.SetCollation(collation, fromDataAnnotation);

            return propertyBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the collation.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-collations">Database collations</see> for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="collation">The collation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as default for the column.</returns>
    public static bool CanSetCollation(
        this IConventionPropertyBuilder propertyBuilder,
        string? collation,
        bool fromDataAnnotation = false)
        => propertyBuilder.CanSetAnnotation(RelationalAnnotationNames.Collation, collation, fromDataAnnotation);

    /// <summary>
    ///     Configures the property of an entity mapped to a JSON column, mapping the entity property to a specific JSON property,
    ///     rather than using the entity property name.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder HasJsonPropertyName(
        this PropertyBuilder propertyBuilder,
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
    public static PropertyBuilder<TProperty> HasJsonPropertyName<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        string? name)
        => (PropertyBuilder<TProperty>)HasJsonPropertyName((PropertyBuilder)propertyBuilder, name);

    /// <summary>
    ///     Configures the property of an entity mapped to a JSON column, mapping the entity property to a specific JSON property,
    ///     rather than using the entity property name.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? HasJsonPropertyName(
        this IConventionPropertyBuilder propertyBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        if (!propertyBuilder.CanSetJsonPropertyName(name, fromDataAnnotation))
        {
            return null;
        }

        propertyBuilder.Metadata.SetJsonPropertyName(name, fromDataAnnotation);

        return propertyBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be used as a JSON property name for a given entity property.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as JSON property name for this entity property.</returns>
    public static bool CanSetJsonPropertyName(
        this IConventionPropertyBuilder propertyBuilder,
        string? name,
        bool fromDataAnnotation = false)
        => propertyBuilder.CanSetAnnotation(RelationalAnnotationNames.JsonPropertyName, name, fromDataAnnotation);
}
