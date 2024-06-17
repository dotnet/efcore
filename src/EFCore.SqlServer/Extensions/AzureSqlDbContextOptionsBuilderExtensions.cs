// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Azure SQL specific extension methods for <see cref="DbContextOptionsBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class AzureSqlDbContextOptionsExtensions
{
    /// <summary>
    ///     Configures the context to connect to a Azure SQL database, but without initially setting any
    ///     <see cref="DbConnection" /> or connection string.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The connection or connection string must be set before the <see cref="DbContext" /> is used to connect
    ///         to a database. Set a connection using <see cref="RelationalDatabaseFacadeExtensions.SetDbConnection" />.
    ///         Set a connection string using <see cref="RelationalDatabaseFacadeExtensions.SetConnectionString" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing Azure SQL databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="azureSqlOptionsAction">An optional action to allow additional Azure SQL specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseAzureSql(
        this DbContextOptionsBuilder optionsBuilder,
        Action<AzureSqlDbContextOptionsBuilder>? azureSqlOptionsAction = null)
    {
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(GetOrCreateExtension(optionsBuilder));

        return ApplyConfiguration(optionsBuilder, azureSqlOptionsAction);
    }

    /// <summary>
    ///     Configures the context to connect to a Azure SQL database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connectionString">The connection string of the database to connect to.</param>
    /// <param name="azureSqlOptionsAction">An optional action to allow additional Azure SQL specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseAzureSql(
        this DbContextOptionsBuilder optionsBuilder,
        string? connectionString,
        Action<AzureSqlDbContextOptionsBuilder>? azureSqlOptionsAction = null)
    {
        var extension = (AzureSqlOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnectionString(connectionString);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return ApplyConfiguration(optionsBuilder, azureSqlOptionsAction);
    }

    // Note: Decision made to use DbConnection not SqlConnection: Issue #772
    /// <summary>
    ///     Configures the context to connect to a Azure SQL database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connection">
    ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
    ///     in the open state then EF will not open or close the connection. If the connection is in the closed
    ///     state then EF will open and close the connection as needed. The caller owns the connection and is
    ///     responsible for its disposal.
    /// </param>
    /// <param name="azureSqlOptionsAction">An optional action to allow additional Azure SQL specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseAzureSql(
        this DbContextOptionsBuilder optionsBuilder,
        DbConnection connection,
        Action<AzureSqlDbContextOptionsBuilder>? azureSqlOptionsAction = null)
        => UseAzureSql(optionsBuilder, connection, false, azureSqlOptionsAction);

    // Note: Decision made to use DbConnection not SqlConnection: Issue #772
    /// <summary>
    ///     Configures the context to connect to a Azure SQL database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connection">
    ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
    ///     in the open state then EF will not open or close the connection. If the connection is in the closed
    ///     state then EF will open and close the connection as needed.
    /// </param>
    /// <param name="contextOwnsConnection">
    ///     If <see langword="true" />, then EF will take ownership of the connection and will
    ///     dispose it in the same way it would dispose a connection created by EF. If <see langword="false" />, then the caller still
    ///     owns the connection and is responsible for its disposal.
    /// </param>
    /// <param name="azureSqlOptionsAction">An optional action to allow additional Azure SQL specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseAzureSql(
        this DbContextOptionsBuilder optionsBuilder,
        DbConnection connection,
        bool contextOwnsConnection,
        Action<AzureSqlDbContextOptionsBuilder>? azureSqlOptionsAction = null)
    {
        Check.NotNull(connection, nameof(connection));

        var extension = (AzureSqlOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnection(connection, contextOwnsConnection);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return ApplyConfiguration(optionsBuilder, azureSqlOptionsAction);
    }

    /// <summary>
    ///     Configures the context to connect to a Azure SQL database, but without initially setting any
    ///     <see cref="DbConnection" /> or connection string.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The connection or connection string must be set before the <see cref="DbContext" /> is used to connect
    ///         to a database. Set a connection using <see cref="RelationalDatabaseFacadeExtensions.SetDbConnection" />.
    ///         Set a connection string using <see cref="RelationalDatabaseFacadeExtensions.SetConnectionString" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing Azure SQL databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="azureSqlOptionsAction">An optional action to allow additional Azure SQL specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseAzureSql<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        Action<AzureSqlDbContextOptionsBuilder>? azureSqlOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseAzureSql(
            (DbContextOptionsBuilder)optionsBuilder, azureSqlOptionsAction);

    /// <summary>
    ///     Configures the context to connect to a Azure SQL database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be configured.</typeparam>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connectionString">The connection string of the database to connect to.</param>
    /// <param name="azureSqlOptionsAction">An optional action to allow additional Azure SQL specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseAzureSql<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string? connectionString,
        Action<AzureSqlDbContextOptionsBuilder>? azureSqlOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseAzureSql(
            (DbContextOptionsBuilder)optionsBuilder, connectionString, azureSqlOptionsAction);

    // Note: Decision made to use DbConnection not SqlConnection: Issue #772
    /// <summary>
    ///     Configures the context to connect to a Azure SQL database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be configured.</typeparam>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connection">
    ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
    ///     in the open state then EF will not open or close the connection. If the connection is in the closed
    ///     state then EF will open and close the connection as needed. The caller owns the connection and is
    ///     responsible for its disposal.
    /// </param>
    /// <param name="azureSqlOptionsAction">An optional action to allow additional Azure SQL specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseAzureSql<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        DbConnection connection,
        Action<AzureSqlDbContextOptionsBuilder>? azureSqlOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseAzureSql(
            (DbContextOptionsBuilder)optionsBuilder, connection, azureSqlOptionsAction);

    // Note: Decision made to use DbConnection not SqlConnection: Issue #772
    /// <summary>
    ///     Configures the context to connect to a Azure SQL database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be configured.</typeparam>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connection">
    ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
    ///     in the open state then EF will not open or close the connection. If the connection is in the closed
    ///     state then EF will open and close the connection as needed.
    /// </param>
    /// <param name="contextOwnsConnection">
    ///     If <see langword="true" />, then EF will take ownership of the connection and will
    ///     dispose it in the same way it would dispose a connection created by EF. If <see langword="false" />, then the caller still
    ///     owns the connection and is responsible for its disposal.
    /// </param>
    /// <param name="azureSqlOptionsAction">An optional action to allow additional Azure SQL specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseAzureSql<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        DbConnection connection,
        bool contextOwnsConnection,
        Action<AzureSqlDbContextOptionsBuilder>? azureSqlOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseAzureSql(
            (DbContextOptionsBuilder)optionsBuilder, connection, contextOwnsConnection, azureSqlOptionsAction);

    private static AzureSqlOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.Options.FindExtension<AzureSqlOptionsExtension>()
            ?? new AzureSqlOptionsExtension();

    private static DbContextOptionsBuilder ApplyConfiguration(
        DbContextOptionsBuilder optionsBuilder,
        Action<AzureSqlDbContextOptionsBuilder>? azureSqlOptionsAction)
    {
        ConfigureWarnings(optionsBuilder);

        azureSqlOptionsAction?.Invoke(new AzureSqlDbContextOptionsBuilder(optionsBuilder));

        var extension = (AzureSqlOptionsExtension)GetOrCreateExtension(optionsBuilder).ApplyDefaults(optionsBuilder.Options);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }

    private static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
    {
        var coreOptionsExtension
            = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()
            ?? new CoreOptionsExtension();

        coreOptionsExtension = RelationalOptionsExtension.WithDefaultWarningConfiguration(coreOptionsExtension);

        coreOptionsExtension = coreOptionsExtension.WithWarningsConfiguration(
            coreOptionsExtension.WarningsConfiguration.TryWithExplicit(
                SqlServerEventId.ConflictingValueGenerationStrategiesWarning, WarningBehavior.Throw));

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(coreOptionsExtension);
    }
}
