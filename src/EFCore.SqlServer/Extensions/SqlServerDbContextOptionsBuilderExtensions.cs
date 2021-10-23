// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQL Server specific extension methods for <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and SQL Azure databases with EF Core</see>
    ///     for more information.
    /// </remarks>
    public static class SqlServerDbContextOptionsExtensions
    {
        /// <summary>
        ///     Configures the context to connect to a Microsoft SQL Server database, but without initially setting any
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
        ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and SQL Azure databases with EF Core</see>
        ///         for more information.
        ///     </para>
        /// </remarks>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="sqlServerOptionsAction">An optional action to allow additional SQL Server specific configuration.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder UseSqlServer(
            this DbContextOptionsBuilder optionsBuilder,
            Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(GetOrCreateExtension(optionsBuilder));

            ConfigureWarnings(optionsBuilder);

            sqlServerOptionsAction?.Invoke(new SqlServerDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        /// <summary>
        ///     Configures the context to connect to a Microsoft SQL Server database.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
        ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and SQL Azure databases with EF Core</see>
        ///     for more information.
        /// </remarks>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="connectionString">The connection string of the database to connect to.</param>
        /// <param name="sqlServerOptionsAction">An optional action to allow additional SQL Server specific configuration.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder UseSqlServer(
            this DbContextOptionsBuilder optionsBuilder,
            string connectionString,
            Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(connectionString, nameof(connectionString));

            var extension = (SqlServerOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnectionString(connectionString);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            ConfigureWarnings(optionsBuilder);

            sqlServerOptionsAction?.Invoke(new SqlServerDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        // Note: Decision made to use DbConnection not SqlConnection: Issue #772
        /// <summary>
        ///     Configures the context to connect to a Microsoft SQL Server database.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
        ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and SQL Azure databases with EF Core</see>
        ///     for more information.
        /// </remarks>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="connection">
        ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
        ///     in the open state then EF will not open or close the connection. If the connection is in the closed
        ///     state then EF will open and close the connection as needed.
        /// </param>
        /// <param name="sqlServerOptionsAction">An optional action to allow additional SQL Server specific configuration.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder UseSqlServer(
            this DbContextOptionsBuilder optionsBuilder,
            DbConnection connection,
            Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(connection, nameof(connection));

            var extension = (SqlServerOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnection(connection);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            ConfigureWarnings(optionsBuilder);

            sqlServerOptionsAction?.Invoke(new SqlServerDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        /// <summary>
        ///     Configures the context to connect to a Microsoft SQL Server database, but without initially setting any
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
        ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and SQL Azure databases with EF Core</see>
        ///         for more information.
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
        ///     Configures the context to connect to a Microsoft SQL Server database.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
        ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and SQL Azure databases with EF Core</see>
        ///     for more information.
        /// </remarks>
        /// <typeparam name="TContext">The type of context to be configured.</typeparam>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="connectionString">The connection string of the database to connect to.</param>
        /// <param name="sqlServerOptionsAction">An optional action to allow additional SQL Server specific configuration.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder<TContext> UseSqlServer<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            string connectionString,
            Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseSqlServer(
                (DbContextOptionsBuilder)optionsBuilder, connectionString, sqlServerOptionsAction);

        // Note: Decision made to use DbConnection not SqlConnection: Issue #772
        /// <summary>
        ///     Configures the context to connect to a Microsoft SQL Server database.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
        ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and SQL Azure databases with EF Core</see>
        ///     for more information.
        /// </remarks>
        /// <typeparam name="TContext">The type of context to be configured.</typeparam>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="connection">
        ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
        ///     in the open state then EF will not open or close the connection. If the connection is in the closed
        ///     state then EF will open and close the connection as needed.
        /// </param>
        /// <param name="sqlServerOptionsAction">An optional action to allow additional SQL Server specific configuration.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder<TContext> UseSqlServer<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            DbConnection connection,
            Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseSqlServer(
                (DbContextOptionsBuilder)optionsBuilder, connection, sqlServerOptionsAction);

        private static SqlServerOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.Options.FindExtension<SqlServerOptionsExtension>()
                ?? new SqlServerOptionsExtension();

        private static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
        {
            var coreOptionsExtension
                = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()
                ?? new CoreOptionsExtension();

            coreOptionsExtension = RelationalOptionsExtension.WithDefaultWarningConfiguration(coreOptionsExtension)
                .WithWarningsConfiguration(
                    coreOptionsExtension.WarningsConfiguration.TryWithExplicit(
                        SqlServerEventId.ConflictingValueGenerationStrategiesWarning, WarningBehavior.Throw));

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(coreOptionsExtension);
        }
    }
}
