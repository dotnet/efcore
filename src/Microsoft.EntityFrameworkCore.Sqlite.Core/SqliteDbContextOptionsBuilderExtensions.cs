// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQLite specific extension methods for <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    public static class SqliteDbContextOptionsBuilderExtensions
    {
        /// <summary>
        ///     Configures the context to connect to a SQLite database.
        /// </summary>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="connectionString"> The connection string of the database to connect to. </param>
        /// <param name="sqliteOptionsAction">An optional action to allow additional SQLite specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseSqlite(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] string connectionString,
            [CanBeNull] Action<SqliteDbContextOptionsBuilder> sqliteOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(connectionString, nameof(connectionString));

            var extension = GetOrCreateExtension(optionsBuilder);
            extension.ConnectionString = connectionString;
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            ConfigureWarnings(optionsBuilder);

            sqliteOptionsAction?.Invoke(new SqliteDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        /// <summary>
        ///     Configures the context to connect to a SQLite database.
        /// </summary>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="connection">
        ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
        ///     in the open state then EF will not open or close the connection. If the connection is in the closed
        ///     state then EF will open and close the connection as needed.
        /// </param>
        /// <param name="sqliteOptionsAction">An optional action to allow additional SQLite specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseSqlite(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] DbConnection connection,
            [CanBeNull] Action<SqliteDbContextOptionsBuilder> sqliteOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(connection, nameof(connection));

            var extension = GetOrCreateExtension(optionsBuilder);
            extension.Connection = connection;
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            ConfigureWarnings(optionsBuilder);

            sqliteOptionsAction?.Invoke(new SqliteDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        /// <summary>
        ///     Configures the context to connect to a SQLite database.
        /// </summary>
        /// <typeparam name="TContext"> The type of context to be configured. </typeparam>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="connectionString"> The connection string of the database to connect to. </param>
        /// <param name="sqliteOptionsAction">An optional action to allow additional SQLite specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder<TContext> UseSqlite<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] string connectionString,
            [CanBeNull] Action<SqliteDbContextOptionsBuilder> sqliteOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseSqlite(
                (DbContextOptionsBuilder)optionsBuilder, connectionString, sqliteOptionsAction);

        /// <summary>
        ///     Configures the context to connect to a SQLite database.
        /// </summary>
        /// <typeparam name="TContext"> The type of context to be configured. </typeparam>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="connection">
        ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
        ///     in the open state then EF will not open or close the connection. If the connection is in the closed
        ///     state then EF will open and close the connection as needed.
        /// </param>
        /// <param name="sqliteOptionsAction">An optional action to allow additional SQLite specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder<TContext> UseSqlite<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] DbConnection connection,
            [CanBeNull] Action<SqliteDbContextOptionsBuilder> sqliteOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseSqlite(
                (DbContextOptionsBuilder)optionsBuilder, connection, sqliteOptionsAction);

        private static SqliteOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder options)
        {
            var existingExtension = options.Options.FindExtension<SqliteOptionsExtension>();

            return existingExtension != null
                ? new SqliteOptionsExtension(existingExtension)
                : new SqliteOptionsExtension();
        }

        private static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
        {
            // Set warnings defaults
            optionsBuilder.ConfigureWarnings(w =>
                {
                    w.Configuration.TryAddExplicit(
                        RelationalEventId.AmbientTransactionWarning, WarningBehavior.Throw);
                });
        }
    }
}
