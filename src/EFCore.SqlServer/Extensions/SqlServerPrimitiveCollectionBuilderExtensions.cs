// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQL Server specific extension methods for <see cref="PrimitiveCollectionBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SqlServerPrimitiveCollectionBuilderExtensions
{
    /// <summary>
    ///     Configures whether the property's column is created as sparse when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples. Also see
    ///     <see href="https://docs.microsoft.com/sql/relational-databases/tables/use-sparse-columns">Sparse columns</see> for
    ///     general information on SQL Server sparse columns.
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="sparse">A value indicating whether the property's column is created as sparse.</param>
    /// <returns>A builder to further configure the property.</returns>
    public static PrimitiveCollectionBuilder IsSparse(this PrimitiveCollectionBuilder primitiveCollectionBuilder, bool sparse = true)
    {
        primitiveCollectionBuilder.Metadata.SetIsSparse(sparse);

        return primitiveCollectionBuilder;
    }

    /// <summary>
    ///     Configures whether the property's column is created as sparse when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples. Also see
    ///     <see href="https://docs.microsoft.com/sql/relational-databases/tables/use-sparse-columns">Sparse columns</see> for
    ///     general information on SQL Server sparse columns.
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="sparse">A value indicating whether the property's column is created as sparse.</param>
    /// <returns>A builder to further configure the property.</returns>
    public static PrimitiveCollectionBuilder<TProperty> IsSparse<TProperty>(
        this PrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder,
        bool sparse = true)
        => (PrimitiveCollectionBuilder<TProperty>)IsSparse((PrimitiveCollectionBuilder)primitiveCollectionBuilder, sparse);

    /// <summary>
    ///     Configures the default value for the column that the property maps
    ///     to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="value">The default value of the column.</param>
    /// <param name="defaultConstraintName">The default constraint name.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PrimitiveCollectionBuilder HasDefaultValue(
        this PrimitiveCollectionBuilder primitiveCollectionBuilder,
        object? value,
        string defaultConstraintName)
    {
        primitiveCollectionBuilder.Metadata.SetDefaultValue(value);
        primitiveCollectionBuilder.Metadata.SetDefaultConstraintName(defaultConstraintName);

        return primitiveCollectionBuilder;
    }

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
    /// <param name="defaultConstraintName">The default constraint name.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PrimitiveCollectionBuilder<TProperty> HasDefaultValue<TProperty>(
        this PrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder,
        object? value,
        string defaultConstraintName)
        => (PrimitiveCollectionBuilder<TProperty>)HasDefaultValue((PrimitiveCollectionBuilder)primitiveCollectionBuilder, value, defaultConstraintName);

    /// <summary>
    ///     Configures the default value expression for the column that the property maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression for the default value of the column.</param>
    /// <param name="defaultConstraintName">The default constraint name.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PrimitiveCollectionBuilder HasDefaultValueSql(
        this PrimitiveCollectionBuilder primitiveCollectionBuilder,
        string? sql,
        string defaultConstraintName)
    {
        Check.NullButNotEmpty(sql, nameof(sql));

        primitiveCollectionBuilder.Metadata.SetDefaultValueSql(sql);
        primitiveCollectionBuilder.Metadata.SetDefaultConstraintName(defaultConstraintName);

        return primitiveCollectionBuilder;
    }

    /// <summary>
    ///     Configures the default value expression for the column that the property maps to when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-default-values">Database default values</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="primitiveCollectionBuilder">The builder for the property being configured.</param>
    /// <param name="sql">The SQL expression for the default value of the column.</param>
    /// <param name="defaultConstraintName">The default constraint name.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PrimitiveCollectionBuilder<TProperty> HasDefaultValueSql<TProperty>(
        this PrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder,
        string? sql,
        string defaultConstraintName)
        => (PrimitiveCollectionBuilder<TProperty>)HasDefaultValueSql((PrimitiveCollectionBuilder)primitiveCollectionBuilder, sql, defaultConstraintName);
}
