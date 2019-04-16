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
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for the <see cref="DatabaseFacade" /> returned from <see cref="DbContext.Database" />
    ///     that can be used only with relational database providers.
    /// </summary>
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
            => Check.NotNull(databaseFacade, nameof(databaseFacade)).GetRelationalService<IMigrator>().Migrate();

        /// <summary>
        ///     Gets all the migrations that are defined in the configured migrations assembly.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context.</param>
        /// <returns>The list of migrations.</returns>
        public static IEnumerable<string> GetMigrations([NotNull] this DatabaseFacade databaseFacade)
            => Check.NotNull(databaseFacade, nameof(databaseFacade)).GetRelationalService<IMigrationsAssembly>().Migrations.Keys;

        /// <summary>
        ///     Gets all migrations that have been applied to the target database.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context.</param>
        /// <returns> The list of migrations. </returns>
        public static IEnumerable<string> GetAppliedMigrations([NotNull] this DatabaseFacade databaseFacade)
            => Check.NotNull(databaseFacade, nameof(databaseFacade)).GetRelationalService<IHistoryRepository>()
                .GetAppliedMigrations().Select(hr => hr.MigrationId);

        /// <summary>
        ///     Asynchronously gets all migrations that have been applied to the target database.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context.</param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public static async Task<IEnumerable<string>> GetAppliedMigrationsAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            CancellationToken cancellationToken = default)
            => (await Check.NotNull(databaseFacade, nameof(databaseFacade)).GetRelationalService<IHistoryRepository>()
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
            CancellationToken cancellationToken = default)
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
            CancellationToken cancellationToken = default)
            => Check.NotNull(databaseFacade, nameof(databaseFacade)).GetRelationalService<IMigrator>()
                .MigrateAsync(cancellationToken: cancellationToken);

        /// <summary>
        ///     <para>
        ///         Executes the given SQL against the database and returns the number of rows affected.
        ///     </para>
        ///     <para>
        ///         Note that this method does not start a transaction. To use this method with
        ///         a transaction, first call <see cref="BeginTransaction" /> or <see cref="UseTransaction" />.
        ///     </para>
        ///     <para>
        ///         Note that the current <see cref="ExecutionStrategy" /> is not used by this method
        ///         since the SQL may not be idempotent and does not run in a transaction. An ExecutionStrategy
        ///         can be used explicitly, making sure to also use a transaction if the SQL is not
        ///         idempotent.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The SQL to execute. </param>
        /// <param name="parameters"> Parameters to use with the SQL. </param>
        /// <returns> The number of rows affected. </returns>
        public static int ExecuteSqlCommand(
            [NotNull] this DatabaseFacade databaseFacade,
            RawSqlString sql,
            [NotNull] params object[] parameters)
            => ExecuteSqlCommand(databaseFacade, sql, (IEnumerable<object>)parameters);

        /// <summary>
        ///     <para>
        ///         Executes the given SQL against the database and returns the number of rows affected.
        ///     </para>
        ///     <para>
        ///         Note that this method does not start a transaction. To use this method with
        ///         a transaction, first call <see cref="BeginTransaction" /> or <see cref="UseTransaction" />.
        ///     </para>
        ///     <para>
        ///         Note that the current <see cref="ExecutionStrategy" /> is not used by this method
        ///         since the SQL may not be idempotent and does not run in a transaction. An ExecutionStrategy
        ///         can be used explicitly, making sure to also use a transaction if the SQL is not
        ///         idempotent.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The interpolated string representing a SQL query with parameters. </param>
        /// <returns> The number of rows affected. </returns>
        public static int ExecuteSqlCommand(
            [NotNull] this DatabaseFacade databaseFacade,
            [NotNull] FormattableString sql)
            => ExecuteSqlCommand(databaseFacade, sql.Format, sql.GetArguments());

        /// <summary>
        ///     <para>
        ///         Executes the given SQL against the database and returns the number of rows affected.
        ///     </para>
        ///     <para>
        ///         Note that this method does not start a transaction. To use this method with
        ///         a transaction, first call <see cref="BeginTransaction" /> or <see cref="UseTransaction" />.
        ///     </para>
        ///     <para>
        ///         Note that the current <see cref="ExecutionStrategy" /> is not used by this method
        ///         since the SQL may not be idempotent and does not run in a transaction. An ExecutionStrategy
        ///         can be used explicitly, making sure to also use a transaction if the SQL is not
        ///         idempotent.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The SQL to execute. </param>
        /// <param name="parameters"> Parameters to use with the SQL. </param>
        /// <returns> The number of rows affected. </returns>
        public static int ExecuteSqlCommand(
            [NotNull] this DatabaseFacade databaseFacade,
            RawSqlString sql,
            [NotNull] IEnumerable<object> parameters)
        {
            Check.NotNull(databaseFacade, nameof(databaseFacade));
            Check.NotNull(sql, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            var concurrencyDetector = databaseFacade.GetService<IConcurrencyDetector>();

            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = databaseFacade
                    .GetRelationalService<IRawSqlCommandBuilder>()
                    .Build(sql.Format, parameters);

                return rawSqlCommand
                    .RelationalCommand
                    .ExecuteNonQuery(
                        databaseFacade.GetRelationalService<IRelationalConnection>(),
                        rawSqlCommand.ParameterValues);
            }
        }

        /// <summary>
        ///     <para>
        ///         Executes the given SQL against the database and returns the number of rows affected.
        ///     </para>
        ///     <para>
        ///         Note that this method does not start a transaction. To use this method with
        ///         a transaction, first call <see cref="BeginTransaction" /> or <see cref="UseTransaction" />.
        ///     </para>
        ///     <para>
        ///         Note that the current <see cref="ExecutionStrategy" /> is not used by this method
        ///         since the SQL may not be idempotent and does not run in a transaction. An ExecutionStrategy
        ///         can be used explicitly, making sure to also use a transaction if the SQL is not
        ///         idempotent.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The interpolated string representing a SQL query with parameters. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result is the number of rows affected.
        /// </returns>
        public static Task<int> ExecuteSqlCommandAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            [NotNull] FormattableString sql,
            CancellationToken cancellationToken = default)
            => ExecuteSqlCommandAsync(databaseFacade, sql.Format, sql.GetArguments(), cancellationToken);

        /// <summary>
        ///     <para>
        ///         Executes the given SQL against the database and returns the number of rows affected.
        ///     </para>
        ///     <para>
        ///         Note that this method does not start a transaction. To use this method with
        ///         a transaction, first call <see cref="BeginTransaction" /> or <see cref="UseTransaction" />.
        ///     </para>
        ///     <para>
        ///         Note that the current <see cref="ExecutionStrategy" /> is not used by this method
        ///         since the SQL may not be idempotent and does not run in a transaction. An ExecutionStrategy
        ///         can be used explicitly, making sure to also use a transaction if the SQL is not
        ///         idempotent.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The SQL to execute. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result is the number of rows affected.
        /// </returns>
        public static Task<int> ExecuteSqlCommandAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            RawSqlString sql,
            CancellationToken cancellationToken = default)
            => ExecuteSqlCommandAsync(databaseFacade, sql, Enumerable.Empty<object>(), cancellationToken);

        /// <summary>
        ///     <para>
        ///         Executes the given SQL against the database and returns the number of rows affected.
        ///     </para>
        ///     <para>
        ///         Note that this method does not start a transaction. To use this method with
        ///         a transaction, first call <see cref="BeginTransaction" /> or <see cref="UseTransaction" />.
        ///     </para>
        ///     <para>
        ///         Note that the current <see cref="ExecutionStrategy" /> is not used by this method
        ///         since the SQL may not be idempotent and does not run in a transaction. An ExecutionStrategy
        ///         can be used explicitly, making sure to also use a transaction if the SQL is not
        ///         idempotent.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The SQL to execute. </param>
        /// <param name="parameters"> Parameters to use with the SQL. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result is the number of rows affected.
        /// </returns>
        public static Task<int> ExecuteSqlCommandAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            RawSqlString sql,
            [NotNull] params object[] parameters)
            => ExecuteSqlCommandAsync(databaseFacade, sql, (IEnumerable<object>)parameters);

        /// <summary>
        ///     <para>
        ///         Executes the given SQL against the database and returns the number of rows affected.
        ///     </para>
        ///     <para>
        ///         Note that this method does not start a transaction. To use this method with
        ///         a transaction, first call <see cref="BeginTransaction" /> or <see cref="UseTransaction" />.
        ///     </para>
        ///     <para>
        ///         Note that the current <see cref="ExecutionStrategy" /> is not used by this method
        ///         since the SQL may not be idempotent and does not run in a transaction. An ExecutionStrategy
        ///         can be used explicitly, making sure to also use a transaction if the SQL is not
        ///         idempotent.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The SQL to execute. </param>
        /// <param name="parameters"> Parameters to use with the SQL. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result is the number of rows affected.
        /// </returns>
        public static async Task<int> ExecuteSqlCommandAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            RawSqlString sql,
            [NotNull] IEnumerable<object> parameters,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(databaseFacade, nameof(databaseFacade));
            Check.NotNull(sql, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            var concurrencyDetector = databaseFacade.GetService<IConcurrencyDetector>();

            using (await concurrencyDetector.EnterCriticalSectionAsync(cancellationToken))
            {
                var rawSqlCommand = databaseFacade
                    .GetRelationalService<IRawSqlCommandBuilder>()
                    .Build(sql.Format, parameters);

                return await rawSqlCommand
                    .RelationalCommand
                    .ExecuteNonQueryAsync(
                        databaseFacade.GetRelationalService<IRelationalConnection>(),
                        rawSqlCommand.ParameterValues,
                        cancellationToken);
            }
        }

        /// <summary>
        ///     Gets the underlying ADO.NET <see cref="DbConnection" /> for this <see cref="DbContext" />.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <returns> The <see cref="DbConnection" /> </returns>
        public static DbConnection GetDbConnection([NotNull] this DatabaseFacade databaseFacade)
            => databaseFacade.GetRelationalService<IRelationalConnection>().DbConnection;

        /// <summary>
        ///     Opens the underlying <see cref="DbConnection" />.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        public static void OpenConnection([NotNull] this DatabaseFacade databaseFacade)
            => databaseFacade.CreateExecutionStrategy().Execute(
                databaseFacade, database
                    => database.GetRelationalService<IRelationalConnection>().Open());

        /// <summary>
        ///     Opens the underlying <see cref="DbConnection" />.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        public static Task OpenConnectionAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            CancellationToken cancellationToken = default)
            => databaseFacade.CreateExecutionStrategy().ExecuteAsync(
                databaseFacade, (database, ct) =>
                    database.GetRelationalService<IRelationalConnection>().OpenAsync(cancellationToken), cancellationToken);

        /// <summary>
        ///     Closes the underlying <see cref="DbConnection" />.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        public static void CloseConnection([NotNull] this DatabaseFacade databaseFacade)
            => databaseFacade.GetRelationalService<IRelationalConnection>().Close();

        /// <summary>
        ///     Starts a new transaction with a given <see cref="IsolationLevel" />.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="isolationLevel"> The <see cref="IsolationLevel" /> to use. </param>
        /// <returns> A <see cref="IDbContextTransaction" /> that represents the started transaction. </returns>
        public static IDbContextTransaction BeginTransaction([NotNull] this DatabaseFacade databaseFacade, IsolationLevel isolationLevel)
            => databaseFacade.CreateExecutionStrategy().Execute(
                databaseFacade, database =>
                {
                    var transactionManager = database.GetTransactionManager();

                    return transactionManager is IRelationalTransactionManager relationalTransactionManager
                        ? relationalTransactionManager.BeginTransaction(isolationLevel)
                        : transactionManager.BeginTransaction();
                });

        /// <summary>
        ///     Asynchronously starts a new transaction with a given <see cref="IsolationLevel" />.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="isolationLevel"> The <see cref="IsolationLevel" /> to use. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous transaction initialization. The task result contains a <see cref="IDbContextTransaction" />
        ///     that represents the started transaction.
        /// </returns>
        public static Task<IDbContextTransaction> BeginTransactionAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default)
            => databaseFacade.CreateExecutionStrategy().ExecuteAsync(
                databaseFacade, (database, ct) =>
                {
                    var transactionManager = database.GetTransactionManager();

                    return transactionManager is IRelationalTransactionManager relationalTransactionManager
                        ? relationalTransactionManager.BeginTransactionAsync(isolationLevel, ct)
                        : transactionManager.BeginTransactionAsync(ct);
                }, cancellationToken);

        /// <summary>
        ///     Sets the <see cref="DbTransaction" /> to be used by database operations on the <see cref="DbContext" />.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="transaction"> The <see cref="DbTransaction" /> to use. </param>
        /// <returns> A <see cref="IDbContextTransaction" /> that encapsulates the given transaction. </returns>
        public static IDbContextTransaction UseTransaction(
            [NotNull] this DatabaseFacade databaseFacade, [CanBeNull] DbTransaction transaction)
        {
            var transactionManager = GetTransactionManager(databaseFacade);

            if (!(transactionManager is IRelationalTransactionManager relationalTransactionManager))
            {
                throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);
            }

            return relationalTransactionManager.UseTransaction(transaction);
        }

        /// <summary>
        ///     <para>
        ///         Sets the timeout (in seconds) to use for commands executed with this <see cref="DbContext" />.
        ///     </para>
        ///     <para>
        ///         Note that the command timeout is distinct from the connection timeout, which is commonly
        ///         set on the database connection string.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="timeout"> The timeout to use, in seconds. </param>
        public static void SetCommandTimeout([NotNull] this DatabaseFacade databaseFacade, int? timeout)
            => databaseFacade.GetRelationalService<IRelationalConnection>().CommandTimeout = timeout;

        /// <summary>
        ///     <para>
        ///         Sets the timeout to use for commands executed with this <see cref="DbContext" />.
        ///     </para>
        ///     <para>
        ///         Note that the command timeout is distinct from the connection timeout, which is commonly
        ///         set on the database connection string.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="timeout"> The timeout to use. </param>
        public static void SetCommandTimeout([NotNull] this DatabaseFacade databaseFacade, TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw new ArgumentException(RelationalStrings.TimeoutTooSmall(timeout.TotalSeconds));
            }

            if (timeout.TotalSeconds > int.MaxValue)
            {
                throw new ArgumentException(RelationalStrings.TimeoutTooBig(timeout.TotalSeconds));
            }

            SetCommandTimeout(databaseFacade, Convert.ToInt32(timeout.TotalSeconds));
        }

        /// <summary>
        ///     <para>
        ///         Returns the timeout (in seconds) set for commands executed with this <see cref="DbContext" />.
        ///     </para>
        ///     <para>
        ///         Note that the command timeout is distinct from the connection timeout, which is commonly
        ///         set on the database connection string.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <returns> The timeout, in seconds, or null if no timeout has been set. </returns>
        public static int? GetCommandTimeout([NotNull] this DatabaseFacade databaseFacade)
            => databaseFacade.GetRelationalService<IRelationalConnection>().CommandTimeout;

        /// <summary>
        ///     Generates a script to create all tables for the current model.
        /// </summary>
        /// <returns>
        ///     A SQL script.
        /// </returns>
        public static string GenerateCreateScript([NotNull] this DatabaseFacade databaseFacade)
            => databaseFacade.GetRelationalService<IRelationalDatabaseCreator>().GenerateCreateScript();

        private static TService GetRelationalService<TService>(this IInfrastructure<IServiceProvider> databaseFacade)
        {
            Check.NotNull(databaseFacade, nameof(databaseFacade));

            var service = databaseFacade.Instance.GetService<TService>();
            if (service == null)
            {
                throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);
            }

            return service;
        }

        private static IDbContextTransactionManager GetTransactionManager([NotNull] this DatabaseFacade databaseFacade)
            => Check.NotNull(databaseFacade, nameof(databaseFacade)).GetService<IDbContextTransactionManager>();
    }
}
