// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQL Server specific extension methods for <see cref="DbContextOptionsBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SqlServerDbContextOptionsExtensions
{
    /// <summary>
    ///     Configures the context to connect to a SQL Server database, but without initially setting any
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
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="sqlServerOptionsAction">An optional action to allow additional SQL Server specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseSqlServer(
        this DbContextOptionsBuilder optionsBuilder,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
    {
        var extension = GetOrCreateExtension<SqlServerOptionsExtension>(optionsBuilder);
        extension = extension
            .WithEngineType(SqlServerEngineType.SqlServer);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
        return ApplyConfiguration(optionsBuilder, sqlServerOptionsAction);
    }

    /// <summary>
    ///     Configures the context to connect to a SQL Server database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connectionString">The connection string of the database to connect to.</param>
    /// <param name="sqlServerOptionsAction">An optional action to allow additional SQL Server specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseSqlServer(
        this DbContextOptionsBuilder optionsBuilder,
        string? connectionString,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
    {
        var extension = GetOrCreateExtension<SqlServerOptionsExtension>(optionsBuilder);
        extension = (SqlServerOptionsExtension)extension
            .WithEngineType(SqlServerEngineType.SqlServer)
            .WithConnectionString(connectionString);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
        return ApplyConfiguration(optionsBuilder, sqlServerOptionsAction);
    }

    // Note: Decision made to use DbConnection not SqlConnection: Issue #772
    /// <summary>
    ///     Configures the context to connect to a SQL Server database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connection">
    ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
    ///     in the open state then EF will not open or close the connection. If the connection is in the closed
    ///     state then EF will open and close the connection as needed. The caller owns the connection and is
    ///     responsible for its disposal.
    /// </param>
    /// <param name="sqlServerOptionsAction">An optional action to allow additional SQL Server specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseSqlServer(
        this DbContextOptionsBuilder optionsBuilder,
        DbConnection connection,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
        => UseSqlServer(optionsBuilder, connection, false, sqlServerOptionsAction);

    // Note: Decision made to use DbConnection not SqlConnection: Issue #772
    /// <summary>
    ///     Configures the context to connect to a SQL Server database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
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
    /// <param name="sqlServerOptionsAction">An optional action to allow additional SQL Server specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseSqlServer(
        this DbContextOptionsBuilder optionsBuilder,
        DbConnection connection,
        bool contextOwnsConnection,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
    {
        Check.NotNull(connection, nameof(connection));

        var extension = GetOrCreateExtension<SqlServerOptionsExtension>(optionsBuilder);
        extension = (SqlServerOptionsExtension)extension
            .WithEngineType(SqlServerEngineType.SqlServer)
            .WithConnection(connection, contextOwnsConnection);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
        return ApplyConfiguration(optionsBuilder, sqlServerOptionsAction);
    }

    /// <summary>
    ///     Configures the context to connect to a SQL Server database, but without initially setting any
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
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="sqlServerOptionsAction">An optional action to allow additional SQL Server specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseSqlServer<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseSqlServer(
            (DbContextOptionsBuilder)optionsBuilder, sqlServerOptionsAction);

    /// <summary>
    ///     Configures the context to connect to a SQL Server database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be configured.</typeparam>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connectionString">The connection string of the database to connect to.</param>
    /// <param name="sqlServerOptionsAction">An optional action to allow additional SQL Server specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseSqlServer<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string? connectionString,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseSqlServer(
            (DbContextOptionsBuilder)optionsBuilder, connectionString, sqlServerOptionsAction);

    // Note: Decision made to use DbConnection not SqlConnection: Issue #772
    /// <summary>
    ///     Configures the context to connect to a SQL Server database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
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
    /// <param name="sqlServerOptionsAction">An optional action to allow additional SQL Server Synapse specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseSqlServer<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        DbConnection connection,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseSqlServer(
            (DbContextOptionsBuilder)optionsBuilder, connection, sqlServerOptionsAction);

    // Note: Decision made to use DbConnection not SqlConnection: Issue #772
    /// <summary>
    ///     Configures the context to connect to a SQL Server database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
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
    /// <param name="sqlServerOptionsAction">An optional action to allow additional SQL Server Synapse specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseSqlServer<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        DbConnection connection,
        bool contextOwnsConnection,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseSqlServer(
            (DbContextOptionsBuilder)optionsBuilder, connection, contextOwnsConnection, sqlServerOptionsAction);

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
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
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
        var extension = GetOrCreateExtension<SqlServerOptionsExtension>(optionsBuilder);
        extension = extension
            .WithEngineType(SqlServerEngineType.AzureSql);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
        return ApplyConfiguration(optionsBuilder, azureSqlOptionsAction);
    }

    /// <summary>
    ///     Configures the context to connect to a Azure SQL database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
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
        var extension = GetOrCreateExtension<SqlServerOptionsExtension>(optionsBuilder);
        extension = (SqlServerOptionsExtension)extension
            .WithEngineType(SqlServerEngineType.AzureSql)
            .WithConnectionString(connectionString);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
        return ApplyConfiguration(optionsBuilder, azureSqlOptionsAction);
    }

    // Note: Decision made to use DbConnection not SqlConnection: Issue #772
    /// <summary>
    ///     Configures the context to connect to a Azure SQL database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
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
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
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

        var extension = GetOrCreateExtension<SqlServerOptionsExtension>(optionsBuilder);
        extension = (SqlServerOptionsExtension)extension
            .WithEngineType(SqlServerEngineType.AzureSql)
            .WithConnection(connection, contextOwnsConnection);
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
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
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
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
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
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
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
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
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

    /// <summary>
    ///     Configures the context to connect to a Azure Synapse database, but without initially setting any
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
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="azureSynapseOptionsAction">An optional action to allow additional Azure Synapse specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseAzureSynapse(
        this DbContextOptionsBuilder optionsBuilder,
        Action<AzureSynapseDbContextOptionsBuilder>? azureSynapseOptionsAction = null)
    {
        var extension = GetOrCreateExtension<SqlServerOptionsExtension>(optionsBuilder);
        extension = extension
            .WithEngineType(SqlServerEngineType.AzureSynapse);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
        return ApplyConfiguration(optionsBuilder, azureSynapseOptionsAction);
    }

    /// <summary>
    ///     Configures the context to connect to a Azure Synapse database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connectionString">The connection string of the database to connect to.</param>
    /// <param name="azureSynapseOptionsAction">An optional action to allow additional Azure Synapse specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseAzureSynapse(
        this DbContextOptionsBuilder optionsBuilder,
        string? connectionString,
        Action<AzureSynapseDbContextOptionsBuilder>? azureSynapseOptionsAction = null)
    {
        var extension = GetOrCreateExtension<SqlServerOptionsExtension>(optionsBuilder);
        extension = (SqlServerOptionsExtension)extension
            .WithEngineType(SqlServerEngineType.AzureSynapse)
            .WithConnectionString(connectionString);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
        return ApplyConfiguration(optionsBuilder, azureSynapseOptionsAction);
    }

    // Note: Decision made to use DbConnection not SqlConnection: Issue #772
    /// <summary>
    ///     Configures the context to connect to a Azure Synapse database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connection">
    ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
    ///     in the open state then EF will not open or close the connection. If the connection is in the closed
    ///     state then EF will open and close the connection as needed. The caller owns the connection and is
    ///     responsible for its disposal.
    /// </param>
    /// <param name="azureSynapseOptionsAction">An optional action to allow additional Azure Synapse specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseAzureSynapse(
        this DbContextOptionsBuilder optionsBuilder,
        DbConnection connection,
        Action<AzureSynapseDbContextOptionsBuilder>? azureSynapseOptionsAction = null)
        => UseAzureSynapse(optionsBuilder, connection, false, azureSynapseOptionsAction);

    // Note: Decision made to use DbConnection not SqlConnection: Issue #772
    /// <summary>
    ///     Configures the context to connect to a Azure Synapse database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
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
    /// <param name="azureSynapseOptionsAction">An optional action to allow additional Azure Synapse specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseAzureSynapse(
        this DbContextOptionsBuilder optionsBuilder,
        DbConnection connection,
        bool contextOwnsConnection,
        Action<AzureSynapseDbContextOptionsBuilder>? azureSynapseOptionsAction = null)
    {
        Check.NotNull(connection, nameof(connection));

        var extension = GetOrCreateExtension<SqlServerOptionsExtension>(optionsBuilder);
        extension = (SqlServerOptionsExtension)extension
            .WithEngineType(SqlServerEngineType.AzureSynapse)
            .WithConnection(connection, contextOwnsConnection);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
        return ApplyConfiguration(optionsBuilder, azureSynapseOptionsAction);
    }

    /// <summary>
    ///     Configures the context to connect to a Azure Synapse database, but without initially setting any
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
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="azureSynapseOptionsAction">An optional action to allow additional Azure Synapse specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseAzureSynapse<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        Action<AzureSynapseDbContextOptionsBuilder>? azureSynapseOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseAzureSynapse(
            (DbContextOptionsBuilder)optionsBuilder, azureSynapseOptionsAction);

    /// <summary>
    ///     Configures the context to connect to a Azure Synapse database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be configured.</typeparam>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connectionString">The connection string of the database to connect to.</param>
    /// <param name="azureSynapseOptionsAction">An optional action to allow additional Azure Synapse specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseAzureSynapse<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string? connectionString,
        Action<AzureSynapseDbContextOptionsBuilder>? azureSynapseOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseAzureSynapse(
            (DbContextOptionsBuilder)optionsBuilder, connectionString, azureSynapseOptionsAction);

    // Note: Decision made to use DbConnection not SqlConnection: Issue #772
    /// <summary>
    ///     Configures the context to connect to a Azure Synapse database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
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
    /// <param name="azureSynapseOptionsAction">An optional action to allow additional Azure Synapse specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseAzureSynapse<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        DbConnection connection,
        Action<AzureSynapseDbContextOptionsBuilder>? azureSynapseOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseAzureSynapse(
            (DbContextOptionsBuilder)optionsBuilder, connection, azureSynapseOptionsAction);

    // Note: Decision made to use DbConnection not SqlConnection: Issue #772
    /// <summary>
    ///     Configures the context to connect to a Azure Synapse database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
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
    /// <param name="azureSynapseOptionsAction">An optional action to allow additional Azure Synapse specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseAzureSynapse<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        DbConnection connection,
        bool contextOwnsConnection,
        Action<AzureSynapseDbContextOptionsBuilder>? azureSynapseOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseAzureSynapse(
            (DbContextOptionsBuilder)optionsBuilder, connection, contextOwnsConnection, azureSynapseOptionsAction);

    /// <summary>
    ///     Configures the context to connect to any of SQL Server, Azure SQL, Azure Synapse databases, but without initially setting any
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
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="sqlEngineOptionsAction">An optional action to allow additional SQL Server, Azure SQL, Azure Synapse configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder ConfigureSqlEngine(
        this DbContextOptionsBuilder optionsBuilder,
        Action<SqlEngineDbContextOptionsBuilder>? sqlEngineOptionsAction = null)
    {
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(
            GetOrCreateExtension<SqlServerOptionsExtension>(optionsBuilder));
        return ApplyConfiguration(optionsBuilder, sqlEngineOptionsAction);
    }

    /// <summary>
    ///     Configures the context to connect to any of SQL Server, Azure SQL, Azure Synapse databases, but without initially setting any
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
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server, Azure SQL, Azure Synapse databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="sqlEngineOptionsAction">An optional action to allow additional SQL Server, Azure SQL, Azure Synapse configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> ConfigureSqlEngine<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        Action<SqlEngineDbContextOptionsBuilder>? sqlEngineOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)ConfigureSqlEngine(
            (DbContextOptionsBuilder)optionsBuilder, sqlEngineOptionsAction);

    private static T GetOrCreateExtension<T>(DbContextOptionsBuilder optionsBuilder)
        where T : RelationalOptionsExtension, new()
        => optionsBuilder.Options.FindExtension<T>()
            ?? new T();

    private static DbContextOptionsBuilder ApplyConfiguration(
        DbContextOptionsBuilder optionsBuilder,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction)
    {
        ConfigureWarnings(optionsBuilder);

        sqlServerOptionsAction?.Invoke(new SqlServerDbContextOptionsBuilder(optionsBuilder));

        var extension = GetOrCreateExtension<SqlServerOptionsExtension>(optionsBuilder).ApplyDefaults(optionsBuilder.Options);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }

    private static DbContextOptionsBuilder ApplyConfiguration(
        DbContextOptionsBuilder optionsBuilder,
        Action<AzureSqlDbContextOptionsBuilder>? azureSqlOptionsAction)
    {
        ConfigureWarnings(optionsBuilder);

        azureSqlOptionsAction?.Invoke(new AzureSqlDbContextOptionsBuilder(optionsBuilder));

        var extension = GetOrCreateExtension<SqlServerOptionsExtension>(optionsBuilder).ApplyDefaults(optionsBuilder.Options);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }

    private static DbContextOptionsBuilder ApplyConfiguration(
        DbContextOptionsBuilder optionsBuilder,
        Action<AzureSynapseDbContextOptionsBuilder>? azureSynapseOptionsAction)
    {
        ConfigureWarnings(optionsBuilder);

        azureSynapseOptionsAction?.Invoke(new AzureSynapseDbContextOptionsBuilder(optionsBuilder));

        var extension = GetOrCreateExtension<SqlServerOptionsExtension>(optionsBuilder).ApplyDefaults(optionsBuilder.Options);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }

    private static DbContextOptionsBuilder ApplyConfiguration(
        DbContextOptionsBuilder optionsBuilder,
        Action<SqlEngineDbContextOptionsBuilder>? sqlEngineOptionsAction)
    {
        ConfigureWarnings(optionsBuilder);

        sqlEngineOptionsAction?.Invoke(new SqlEngineDbContextOptionsBuilder(optionsBuilder));

        var extension = GetOrCreateExtension<SqlServerOptionsExtension>(optionsBuilder).ApplyDefaults(optionsBuilder.Options);
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
