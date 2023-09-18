// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Sqlite-specific extension methods for <see cref="TableBuilder" />.
/// </summary>
public static class SqliteTableBuilderExtensions
{
    /// <summary>
    ///     Configures whether to use the SQL RETURNING clause when saving changes to the table.
    ///     The RETURNING clause is incompatible with certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sqlite-returning-clause">Using the SQL RETURNING clause with Sqlite</see> for more
    ///     information and examples.
    /// </remarks>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="useSqlReturningClause">A value indicating whether to use the RETURNING clause when saving changes to the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static TableBuilder UseSqlReturningClause(
        this TableBuilder tableBuilder,
        bool useSqlReturningClause = true)
    {
        UseSqlReturningClause(tableBuilder.Metadata, tableBuilder.Name, tableBuilder.Schema, useSqlReturningClause);

        return tableBuilder;
    }

    /// <summary>
    ///     Configures whether to use the SQL RETURNING clause when saving changes to the table.
    ///     The RETURNING clause is incompatible with certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sqlite-returning-clause">Using the SQL RETURNING clause with Sqlite</see> for more
    ///     information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="useSqlReturningClause">A value indicating whether to use the RETURNING clause when saving changes to the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static TableBuilder<TEntity> UseSqlReturningClause<TEntity>(
        this TableBuilder<TEntity> tableBuilder,
        bool useSqlReturningClause = true)
        where TEntity : class
        => (TableBuilder<TEntity>)((TableBuilder)tableBuilder).UseSqlReturningClause(useSqlReturningClause);

    /// <summary>
    ///     Configures whether to use the SQL RETURNING clause when saving changes to the table.
    ///     The RETURNING clause is incompatible with certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sqlite-returning-clause">Using the SQL RETURNING clause with Sqlite</see> for more
    ///     information and examples.
    /// </remarks>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="useSqlReturningClause">A value indicating whether to use the RETURNING clause when saving changes to the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static SplitTableBuilder UseSqlReturningClause(
        this SplitTableBuilder tableBuilder,
        bool useSqlReturningClause = true)
    {
        UseSqlReturningClause(tableBuilder.Metadata, tableBuilder.Name, tableBuilder.Schema, useSqlReturningClause);

        return tableBuilder;
    }

    /// <summary>
    ///     Configures whether to use the SQL RETURNING clause when saving changes to the table.
    ///     The RETURNING clause is incompatible with certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sqlite-returning-clause">Using the SQL RETURNING clause with Sqlite</see> for more
    ///     information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="useSqlReturningClause">A value indicating whether to use the RETURNING clause when saving changes to the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static SplitTableBuilder<TEntity> UseSqlReturningClause<TEntity>(
        this SplitTableBuilder<TEntity> tableBuilder,
        bool useSqlReturningClause = true)
        where TEntity : class
        => (SplitTableBuilder<TEntity>)((SplitTableBuilder)tableBuilder).UseSqlReturningClause(useSqlReturningClause);

    /// <summary>
    ///     Configures whether to use the SQL RETURNING clause when saving changes to the table.
    ///     The RETURNING clause is incompatible with  certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sqlite-returning-clause">Using the SQL RETURNING clause with Sqlite</see> for more
    ///     information and examples.
    /// </remarks>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="useSqlReturningClause">A value indicating whether to use the RETURNING clause when saving changes to the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationTableBuilder UseSqlReturningClause(
        this OwnedNavigationTableBuilder tableBuilder,
        bool useSqlReturningClause = true)
    {
        UseSqlReturningClause(tableBuilder.Metadata, tableBuilder.Name, tableBuilder.Schema, useSqlReturningClause);

        return tableBuilder;
    }

    /// <summary>
    ///     Configures whether to use the SQL RETURNING clause when saving changes to the table. The RETURNING clause is incompatible with
    ///     certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sqlite-returning-clause">Using the SQL RETURNING clause with Sqlite</see> for more
    ///     information and examples.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="useSqlReturningClause">A value indicating whether to use the RETURNING clause when saving changes to the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> UseSqlReturningClause<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> tableBuilder,
        bool useSqlReturningClause = true)
        where TOwnerEntity : class
        where TDependentEntity : class
        => (OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>)
            ((OwnedNavigationTableBuilder)tableBuilder).UseSqlReturningClause(useSqlReturningClause);

    private static void UseSqlReturningClause(
        IMutableEntityType entityType,
        string? tableName,
        string? tableSchema,
        bool useSqlReturningClause)
    {
        if (tableName is null)
        {
            entityType.UseSqlReturningClause(useSqlReturningClause);
        }
        else
        {
            entityType.UseSqlReturningClause(
                useSqlReturningClause,
                StoreObjectIdentifier.Table(tableName, tableSchema));
        }
    }
}
