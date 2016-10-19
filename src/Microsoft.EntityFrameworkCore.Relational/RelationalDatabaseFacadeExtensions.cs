// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class RelationalDatabaseFacadeExtensions
    {
        /// <summary>
        ///     <para>
        ///         Applies any pending migrations for the context to the database. Will create the database
        ///         if it does not already exist.
        ///     </para>
        ///     <para>
        ///         Note that this API is mutually exclusive with DbContext.Database.EnsureCreated(). EnsureCreated does not use migrations
        ///         to create the database and therefore the database that is created cannot be later updated using migrations.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        public static void Migrate([NotNull] this DatabaseFacade databaseFacade)
            => Check.NotNull(databaseFacade, nameof(databaseFacade)).GetService<IMigrator>().Migrate();

        /// <summary>
        ///     Gets all the migrations that are defined in the configured migrations assembly.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context.</param>
        /// <returns>The list of migrations.</returns>
        public static IEnumerable<string> GetMigrations([NotNull] this DatabaseFacade databaseFacade)
            => Check.NotNull(databaseFacade, nameof(databaseFacade)).GetService<IMigrationsAssembly>().Migrations.Keys;

        /// <summary>
        ///     Gets all migrations that have been applied to the target database.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context.</param>
        /// <returns> The list of migrations. </returns>
        public static IEnumerable<string> GetAppliedMigrations([NotNull] this DatabaseFacade databaseFacade)
            => Check.NotNull(databaseFacade, nameof(databaseFacade)).GetService<IHistoryRepository>()
                .GetAppliedMigrations().Select(hr => hr.MigrationId);

        /// <summary>
        ///     Asynchronously gets all migrations that have been applied to the target database.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context.</param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public static async Task<IEnumerable<string>> GetAppliedMigrationsAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            CancellationToken cancellationToken = default(CancellationToken))
            => (await Check.NotNull(databaseFacade, nameof(databaseFacade)).GetService<IHistoryRepository>()
                .GetAppliedMigrationsAsync(cancellationToken)).Select(hr => hr.MigrationId);

        /// <summary>
        ///     Gets all migrations that are defined in the assembly but haven't been applied to the target database.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context.</param>
        /// <returns> The list of migrations. </returns>
        public static IEnumerable<string> GetPendingMigrations([NotNull] this DatabaseFacade databaseFacade)
            => GetMigrations(databaseFacade).Except(GetAppliedMigrations(databaseFacade));

        /// <summary>
        ///     Asynchronously gets all migrations that are defined in the assembly but haven't been applied to the target database.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context.</param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public static async Task<IEnumerable<string>> GetPendingMigrationsAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            CancellationToken cancellationToken = default(CancellationToken))
            => GetMigrations(databaseFacade).Except(await GetAppliedMigrationsAsync(databaseFacade, cancellationToken));

        /// <summary>
        ///     <para>
        ///         Asynchronously applies any pending migrations for the context to the database. Will create the database
        ///         if it does not already exist.
        ///     </para>
        ///     <para>
        ///         Note that this API is mutually exclusive with DbContext.Database.EnsureCreated(). EnsureCreated does not use migrations
        ///         to create the database and therefore the database that is created cannot be later updated using migrations.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> A task that represents the asynchronous migration operation. </returns>
        public static Task MigrateAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            CancellationToken cancellationToken = default(CancellationToken))
            => Check.NotNull(databaseFacade, nameof(databaseFacade)).GetService<IMigrator>()
                .MigrateAsync(cancellationToken: cancellationToken);

        // Note that this method doesn't start a transaction hence it doesn't use ExecutionStrategy
        public static int ExecuteSqlCommand(
            [NotNull] this DatabaseFacade databaseFacade,
            [NotNull] string sql,
            [NotNull] params object[] parameters)
        {
            Check.NotNull(databaseFacade, nameof(databaseFacade));

            var concurrencyDetector = databaseFacade.GetService<IConcurrencyDetector>();

            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = databaseFacade
                    .GetService<IRawSqlCommandBuilder>()
                    .Build(sql, parameters);

                return rawSqlCommand
                    .RelationalCommand
                    .ExecuteNonQuery(
                        GetRelationalConnection(databaseFacade),
                        parameterValues: rawSqlCommand.ParameterValues);
            }
        }

        // Note that this method doesn't start a transaction hence it doesn't use ExecutionStrategy
        public static async Task<int> ExecuteSqlCommandAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            [NotNull] string sql,
            CancellationToken cancellationToken = default(CancellationToken),
            [NotNull] params object[] parameters)
        {
            Check.NotNull(databaseFacade, nameof(databaseFacade));

            var concurrencyDetector = databaseFacade.GetService<IConcurrencyDetector>();

            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = databaseFacade
                    .GetService<IRawSqlCommandBuilder>()
                    .Build(sql, parameters);

                return await rawSqlCommand
                    .RelationalCommand
                    .ExecuteNonQueryAsync(
                        databaseFacade.GetRelationalConnection(),
                        rawSqlCommand.ParameterValues,
                        cancellationToken: cancellationToken);
            }
        }

        public static DbConnection GetDbConnection([NotNull] this DatabaseFacade databaseFacade)
            => GetRelationalConnection(databaseFacade).DbConnection;

        public static void OpenConnection([NotNull] this DatabaseFacade databaseFacade)
            => databaseFacade.CreateExecutionStrategy().Execute(
                database => database.GetRelationalConnection().Open(), databaseFacade);

        public static Task OpenConnectionAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            CancellationToken cancellationToken = default(CancellationToken))
            => databaseFacade.CreateExecutionStrategy().ExecuteAsync((database, ct) =>
                database.GetRelationalConnection().OpenAsync(cancellationToken), databaseFacade, cancellationToken);

        public static void CloseConnection([NotNull] this DatabaseFacade databaseFacade)
            => GetRelationalConnection(databaseFacade).Close();

        public static IDbContextTransaction BeginTransaction([NotNull] this DatabaseFacade databaseFacade, IsolationLevel isolationLevel)
            => databaseFacade.CreateExecutionStrategy().Execute(database =>
                {
                    var transactionManager = database.GetTransactionManager();

                    var relationalTransactionManager = transactionManager as IRelationalTransactionManager;

                    return relationalTransactionManager != null
                        ? relationalTransactionManager.BeginTransaction(isolationLevel)
                        : transactionManager.BeginTransaction();
                }, databaseFacade);

        public static Task<IDbContextTransaction> BeginTransactionAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default(CancellationToken))
            => databaseFacade.CreateExecutionStrategy().ExecuteAsync((database, ct) =>
                {
                    var transactionManager = database.GetTransactionManager();

                    var relationalTransactionManager = transactionManager as IRelationalTransactionManager;

                    return relationalTransactionManager != null
                        ? relationalTransactionManager.BeginTransactionAsync(isolationLevel, ct)
                        : transactionManager.BeginTransactionAsync(ct);
                }, databaseFacade, cancellationToken);

        public static IDbContextTransaction UseTransaction(
            [NotNull] this DatabaseFacade databaseFacade, [CanBeNull] DbTransaction transaction)
        {
            var transactionManager = GetTransactionManager(databaseFacade);

            var relationalTransactionManager = transactionManager as IRelationalTransactionManager;

            if (relationalTransactionManager == null)
            {
                throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);
            }

            return relationalTransactionManager.UseTransaction(transaction);
        }

        public static void SetCommandTimeout([NotNull] this DatabaseFacade databaseFacade, int? timeout)
            => GetRelationalConnection(databaseFacade).CommandTimeout = timeout;
        public static void SetCommandTimeout([NotNull] this DatabaseFacade databaseFacade, TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw new ArgumentException($"Timeout must be greater than or equal to zero.  Provided: {timeout.TotalSeconds} seconds.");
            }
            if (timeout.TotalSeconds > Int32.MaxValue)
            {
                throw new ArgumentException($"Timeout must be less than or equal to Int32.MaxValue (2147483647) seconds.  Provided: {timeout.Seconds} seconds.");
            }
            SetCommandTimeout(databaseFacade, Convert.ToInt32(timeout.TotalSeconds));
        }

        public static int? GetCommandTimeout([NotNull] this DatabaseFacade databaseFacade)
            => GetRelationalConnection(databaseFacade).CommandTimeout;

        private static IRelationalConnection GetRelationalConnection([NotNull] this DatabaseFacade databaseFacade)
            => Check.NotNull(databaseFacade, nameof(databaseFacade)).GetService<IRelationalConnection>();

        private static IDbContextTransactionManager GetTransactionManager([NotNull] this DatabaseFacade databaseFacade)
            => Check.NotNull(databaseFacade, nameof(databaseFacade)).GetService<IDbContextTransactionManager>();
    }
}
