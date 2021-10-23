// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQLite specific extension methods for <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information.
    /// </remarks>
    public static class SqliteDbContextOptionsBuilderExtensions
    {
        /// <summary>
        ///     Configures the context to connect to a SQLite database, but without initially setting any
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
        ///         <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information.
        ///     </para>
        /// </remarks>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="sqliteOptionsAction">An optional action to allow additional SQLite specific configuration.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder UseSqlite(
            this DbContextOptionsBuilder optionsBuilder,
            Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(GetOrCreateExtension(optionsBuilder));

            ConfigureWarnings(optionsBuilder);

            sqliteOptionsAction?.Invoke(new SqliteDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        /// <summary>
        ///     Configures the context to connect to a SQLite database.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
        ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information.
        /// </remarks>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="connectionString">The connection string of the database to connect to.</param>
        /// <param name="sqliteOptionsAction">An optional action to allow additional SQLite specific configuration.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder UseSqlite(
            this DbContextOptionsBuilder optionsBuilder,
            string connectionString,
            Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(connectionString, nameof(connectionString));

            var extension = (SqliteOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnectionString(connectionString);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            ConfigureWarnings(optionsBuilder);

            sqliteOptionsAction?.Invoke(new SqliteDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        /// <summary>
        ///     Configures the context to connect to a SQLite database.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
        ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information.
        /// </remarks>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="connection">
        ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
        ///     in the open state then EF will not open or close the connection. If the connection is in the closed
        ///     state then EF will open and close the connection as needed.
        /// </param>
        /// <param name="sqliteOptionsAction">An optional action to allow additional SQLite specific configuration.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder UseSqlite(
            this DbContextOptionsBuilder optionsBuilder,
            DbConnection connection,
            Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(connection, nameof(connection));

            var extension = (SqliteOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnection(connection);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            ConfigureWarnings(optionsBuilder);

            sqliteOptionsAction?.Invoke(new SqliteDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        /// <summary>
        ///     Configures the context to connect to a SQLite database, but without initially setting any
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
        ///         <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information.
        ///     </para>
        /// </remarks>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="sqliteOptionsAction">An optional action to allow additional SQLite specific configuration.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder<TContext> UseSqlite<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseSqlite(
                (DbContextOptionsBuilder)optionsBuilder, sqliteOptionsAction);

        /// <summary>
        ///     Configures the context to connect to a SQLite database.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
        ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information.
        /// </remarks>
        /// <typeparam name="TContext">The type of context to be configured.</typeparam>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="connectionString">The connection string of the database to connect to.</param>
        /// <param name="sqliteOptionsAction">An optional action to allow additional SQLite specific configuration.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder<TContext> UseSqlite<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            string connectionString,
            Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseSqlite(
                (DbContextOptionsBuilder)optionsBuilder, connectionString, sqliteOptionsAction);

        /// <summary>
        ///     Configures the context to connect to a SQLite database.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
        ///     <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information.
        /// </remarks>
        /// <typeparam name="TContext">The type of context to be configured.</typeparam>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="connection">
        ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
        ///     in the open state then EF will not open or close the connection. If the connection is in the closed
        ///     state then EF will open and close the connection as needed.
        /// </param>
        /// <param name="sqliteOptionsAction">An optional action to allow additional SQLite specific configuration.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder<TContext> UseSqlite<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            DbConnection connection,
            Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseSqlite(
                (DbContextOptionsBuilder)optionsBuilder, connection, sqliteOptionsAction);

        private static SqliteOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder options)
            => options.Options.FindExtension<SqliteOptionsExtension>()
                ?? new SqliteOptionsExtension();

        private static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
        {
            var coreOptionsExtension
                = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()
                ?? new CoreOptionsExtension();

            coreOptionsExtension = RelationalOptionsExtension.WithDefaultWarningConfiguration(coreOptionsExtension);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(coreOptionsExtension);
        }
    }
}
