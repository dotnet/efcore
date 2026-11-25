// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="ComplexCollectionBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-complex-types">Complex types</see> for more information and examples.
/// </remarks>
public static class RelationalComplexCollectionBuilderExtensions
{
    /// <summary>
    ///     Configures the complex collection to be stored as a JSON column.
    /// </summary>
    /// <param name="complexCollectionBuilder">The builder for the complex collection being configured.</param>
    /// <param name="jsonColumnName">The name of the JSON column. If not specified, the complex collection name is used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    /// <remarks>
    ///     <para>
    ///         If the complex collection is declared on an owned type that shares a table or view with its owner and the column name
    ///         is not specified, the name is prefixed in that table or view with the names of the ownership navigations.
    ///         For example, a complex collection <c>Locations</c> on the owned <c>Order.ShippingAddress</c> is mapped to the
    ///         <c>ShippingAddress_Locations</c> column.
    ///     </para>
    ///     <para>
    ///         If the complex collection is declared on a complex type that isn't mapped to JSON and the column name is not specified,
    ///         the name is prefixed with the names of the containing complex properties. For example, a complex collection
    ///         <c>Parcels</c> on the complex property <c>Order.Shipping</c> is mapped to the <c>Shipping_Parcels</c> column.
    ///         A number is appended to the name if it's already used by another column.
    ///     </para>
    /// </remarks>
    public static ComplexCollectionBuilder ToJson(
        this ComplexCollectionBuilder complexCollectionBuilder,
        string? jsonColumnName = null)
    {
        Check.NullButNotEmpty(jsonColumnName);

        var complexProperty = complexCollectionBuilder.Metadata;

        // An empty name stands for the default one, see GetContainerColumnName
        complexProperty.ComplexType.SetContainerColumnName(jsonColumnName ?? string.Empty);

        return complexCollectionBuilder;
    }

    /// <summary>
    ///     Configures the complex collection to be stored as a JSON column.
    /// </summary>
    /// <param name="complexCollectionBuilder">The builder for the complex collection being configured.</param>
    /// <param name="jsonColumnName">The name of the JSON column. If not specified, the complex collection name is used.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    /// <remarks>
    ///     <para>
    ///         If the complex collection is declared on an owned type that shares a table or view with its owner and the column name
    ///         is not specified, the name is prefixed in that table or view with the names of the ownership navigations.
    ///         For example, a complex collection <c>Locations</c> on the owned <c>Order.ShippingAddress</c> is mapped to the
    ///         <c>ShippingAddress_Locations</c> column.
    ///     </para>
    ///     <para>
    ///         If the complex collection is declared on a complex type that isn't mapped to JSON and the column name is not specified,
    ///         the name is prefixed with the names of the containing complex properties. For example, a complex collection
    ///         <c>Parcels</c> on the complex property <c>Order.Shipping</c> is mapped to the <c>Shipping_Parcels</c> column.
    ///         A number is appended to the name if it's already used by another column.
    ///     </para>
    /// </remarks>
    public static ComplexCollectionBuilder<TComplex> ToJson<TComplex>(
        this ComplexCollectionBuilder<TComplex> complexCollectionBuilder,
        string? jsonColumnName = null)
        where TComplex : notnull
        => (ComplexCollectionBuilder<TComplex>)((ComplexCollectionBuilder)complexCollectionBuilder).ToJson(jsonColumnName);

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
        => (ComplexCollectionBuilder<TComplex>)((ComplexCollectionBuilder)complexCollectionBuilder).HasJsonPropertyName(name);

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
        => (ComplexCollectionBuilder<TComplex>)((ComplexCollectionBuilder)complexCollectionBuilder).HasColumnType(columnType);
}
