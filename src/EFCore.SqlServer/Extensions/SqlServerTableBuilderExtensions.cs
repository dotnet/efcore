// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQL Server specific extension methods for <see cref="TableBuilder" />.
/// </summary>
public static class SqlServerTableBuilderExtensions
{
    #region IsTemporal

    /// <summary>
    ///     Configures the table as temporal.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="temporal">A value indicating whether the table is temporal.</param>
    /// <returns>An object that can be used to configure the temporal table.</returns>
    public static TemporalTableBuilder IsTemporal(
        this TableBuilder tableBuilder,
        bool temporal = true)
    {
        tableBuilder.Metadata.SetIsTemporal(temporal);

        return new TemporalTableBuilder(tableBuilder.GetInfrastructure());
    }

    /// <summary>
    ///     Configures the table as temporal.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="buildAction">An action that performs configuration of the temporal table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static TableBuilder IsTemporal(
        this TableBuilder tableBuilder,
        Action<TemporalTableBuilder> buildAction)
    {
        tableBuilder.Metadata.SetIsTemporal(true);

        buildAction(new TemporalTableBuilder(tableBuilder.GetInfrastructure()));

        return tableBuilder;
    }

    /// <summary>
    ///     Configures the table as temporal.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="temporal">A value indicating whether the table is temporal.</param>
    /// <returns>An object that can be used to configure the temporal table.</returns>
    public static TemporalTableBuilder<TEntity> IsTemporal<TEntity>(
        this TableBuilder<TEntity> tableBuilder,
        bool temporal = true)
        where TEntity : class
    {
        tableBuilder.Metadata.SetIsTemporal(temporal);

        return new TemporalTableBuilder<TEntity>(tableBuilder.GetInfrastructure<EntityTypeBuilder<TEntity>>());
    }

    /// <summary>
    ///     Configures the table as temporal.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="buildAction">An action that performs configuration of the temporal table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static TableBuilder<TEntity> IsTemporal<TEntity>(
        this TableBuilder<TEntity> tableBuilder,
        Action<TemporalTableBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        tableBuilder.Metadata.SetIsTemporal(true);
        buildAction(new TemporalTableBuilder<TEntity>(tableBuilder.GetInfrastructure<EntityTypeBuilder<TEntity>>()));

        return tableBuilder;
    }

    /// <summary>
    ///     Configures the table as temporal.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information.
    /// </remarks>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="temporal">A value indicating whether the table is temporal.</param>
    /// <returns>An object that can be used to configure the temporal table.</returns>
    public static OwnedNavigationTemporalTableBuilder IsTemporal(
        this OwnedNavigationTableBuilder tableBuilder,
        bool temporal = true)
    {
        tableBuilder.Metadata.SetIsTemporal(temporal);

        return new OwnedNavigationTemporalTableBuilder(tableBuilder.GetInfrastructure());
    }

    /// <summary>
    ///     Configures the table as temporal.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information.
    /// </remarks>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="buildAction">An action that performs configuration of the temporal table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationTableBuilder IsTemporal(
        this OwnedNavigationTableBuilder tableBuilder,
        Action<OwnedNavigationTemporalTableBuilder> buildAction)
    {
        tableBuilder.Metadata.SetIsTemporal(true);

        buildAction(new OwnedNavigationTemporalTableBuilder(tableBuilder.GetInfrastructure()));

        return tableBuilder;
    }

    /// <summary>
    ///     Configures the table as temporal.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="temporal">A value indicating whether the table is temporal.</param>
    /// <returns>An object that can be used to configure the temporal table.</returns>
    public static OwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity> IsTemporal<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> tableBuilder,
        bool temporal = true)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        tableBuilder.Metadata.SetIsTemporal(temporal);

        return new OwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>(
            tableBuilder.GetInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>>());
    }

    /// <summary>
    ///     Configures the table as temporal.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="buildAction">An action that performs configuration of the temporal table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> IsTemporal<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> tableBuilder,
        Action<OwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        tableBuilder.Metadata.SetIsTemporal(true);
        buildAction(
            new OwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>(
                tableBuilder.GetInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>>()));

        return tableBuilder;
    }

    #endregion IsTemporal

    #region IsMemoryOptimized

    /// <summary>
    ///     Configures the table that the entity maps to when targeting SQL Server as memory-optimized.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-memory-optimized">Using SQL Server memory-optimized tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="memoryOptimized">A value indicating whether the table is memory-optimized.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static TableBuilder IsMemoryOptimized(
        this TableBuilder tableBuilder,
        bool memoryOptimized = true)
    {
        tableBuilder.Metadata.SetIsMemoryOptimized(memoryOptimized);

        return tableBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity maps to when targeting SQL Server as memory-optimized.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-memory-optimized">Using SQL Server memory-optimized tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="memoryOptimized">A value indicating whether the table is memory-optimized.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static TableBuilder<TEntity> IsMemoryOptimized<TEntity>(
        this TableBuilder<TEntity> tableBuilder,
        bool memoryOptimized = true)
        where TEntity : class
    {
        tableBuilder.Metadata.SetIsMemoryOptimized(memoryOptimized);

        return tableBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity maps to when targeting SQL Server as memory-optimized.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-memory-optimized">Using SQL Server memory-optimized tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="memoryOptimized">A value indicating whether the table is memory-optimized.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationTableBuilder IsMemoryOptimized(
        this OwnedNavigationTableBuilder tableBuilder,
        bool memoryOptimized = true)
    {
        tableBuilder.Metadata.SetIsMemoryOptimized(memoryOptimized);

        return tableBuilder;
    }

    /// <summary>
    ///     Configures the table that the entity maps to when targeting SQL Server as memory-optimized.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-memory-optimized">Using SQL Server memory-optimized tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="memoryOptimized">A value indicating whether the table is memory-optimized.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> IsMemoryOptimized<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> tableBuilder,
        bool memoryOptimized = true)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        tableBuilder.Metadata.SetIsMemoryOptimized(memoryOptimized);

        return tableBuilder;
    }

    #endregion IsMemoryOptimized

    #region UseSqlOutputClause

    /// <summary>
    ///     Configures whether to use the SQL OUTPUT clause when saving changes to the table.
    ///     The OUTPUT clause is incompatible with certain SQL Server features, such as tables with triggers.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sqlserver-save-changes-and-output-clause">Using the SQL OUTPUT clause with SQL Server</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="useSqlOutputClause">A value indicating whether to use the OUTPUT clause when saving changes to the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static TableBuilder UseSqlOutputClause(
        this TableBuilder tableBuilder,
        bool useSqlOutputClause = true)
    {
        UseSqlOutputClause(tableBuilder.Metadata, tableBuilder.Name, tableBuilder.Schema, useSqlOutputClause);

        return tableBuilder;
    }

    /// <summary>
    ///     Configures whether to use the SQL OUTPUT clause when saving changes to the table.
    ///     The OUTPUT clause is incompatible with certain SQL Server features, such as tables with triggers.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sqlserver-save-changes-and-output-clause">Using the SQL OUTPUT clause with SQL Server</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="useSqlOutputClause">A value indicating whether to use the OUTPUT clause when saving changes to the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static TableBuilder<TEntity> UseSqlOutputClause<TEntity>(
        this TableBuilder<TEntity> tableBuilder,
        bool useSqlOutputClause = true)
        where TEntity : class
        => (TableBuilder<TEntity>)((TableBuilder)tableBuilder).UseSqlOutputClause(useSqlOutputClause);

    /// <summary>
    ///     Configures whether to use the SQL OUTPUT clause when saving changes to the table.
    ///     The OUTPUT clause is incompatible with certain SQL Server features, such as tables with triggers.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sqlserver-save-changes-and-output-clause">Using the SQL OUTPUT clause with SQL Server</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="useSqlOutputClause">A value indicating whether to use the OUTPUT clause when saving changes to the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static SplitTableBuilder UseSqlOutputClause(
        this SplitTableBuilder tableBuilder,
        bool useSqlOutputClause = true)
    {
        UseSqlOutputClause(tableBuilder.Metadata, tableBuilder.Name, tableBuilder.Schema, useSqlOutputClause);

        return tableBuilder;
    }

    /// <summary>
    ///     Configures whether to use the SQL OUTPUT clause when saving changes to the table.
    ///     The OUTPUT clause is incompatible with certain SQL Server features, such as tables with triggers.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sqlserver-save-changes-and-output-clause">Using the SQL OUTPUT clause with SQL Server</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="useSqlOutputClause">A value indicating whether to use the OUTPUT clause when saving changes to the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static SplitTableBuilder<TEntity> UseSqlOutputClause<TEntity>(
        this SplitTableBuilder<TEntity> tableBuilder,
        bool useSqlOutputClause = true)
        where TEntity : class
        => (SplitTableBuilder<TEntity>)((SplitTableBuilder)tableBuilder).UseSqlOutputClause(useSqlOutputClause);

    /// <summary>
    ///     Configures whether to use the SQL OUTPUT clause when saving changes to the table.
    ///     The OUTPUT clause is incompatible with certain SQL Server features, such as tables with triggers.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sqlserver-save-changes-and-output-clause">Using the SQL OUTPUT clause with SQL Server</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="useSqlOutputClause">A value indicating whether to use the OUTPUT clause when saving changes to the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationTableBuilder UseSqlOutputClause(
        this OwnedNavigationTableBuilder tableBuilder,
        bool useSqlOutputClause = true)
    {
        UseSqlOutputClause(tableBuilder.Metadata, tableBuilder.Name, tableBuilder.Schema, useSqlOutputClause);

        return tableBuilder;
    }

    /// <summary>
    ///     Configures whether to use the SQL OUTPUT clause when saving changes to the table.
    ///     The OUTPUT clause is incompatible with certain SQL Server features, such as tables with triggers.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sqlserver-save-changes-and-output-clause">Using the SQL OUTPUT clause with SQL Server</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
    /// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
    /// <param name="tableBuilder">The builder for the table being configured.</param>
    /// <param name="useSqlOutputClause">A value indicating whether to use the OUTPUT clause when saving changes to the table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> UseSqlOutputClause<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> tableBuilder,
        bool useSqlOutputClause = true)
        where TOwnerEntity : class
        where TDependentEntity : class
        => (OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>)
            ((OwnedNavigationTableBuilder)tableBuilder).UseSqlOutputClause(useSqlOutputClause);

    private static void UseSqlOutputClause(IMutableEntityType entityType, string? tableName, string? tableSchema, bool useSqlOutputClause)
    {
        if (tableName is null)
        {
            entityType.UseSqlOutputClause(useSqlOutputClause);
        }
        else
        {
            entityType.UseSqlOutputClause(
                useSqlOutputClause,
                StoreObjectIdentifier.Table(tableName, tableSchema));
        }
    }

    #endregion UseSqlOutputClause
}
