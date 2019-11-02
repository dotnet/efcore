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
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
        ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter -
        ///         <code>context.Database.ExecuteSqlCommand("SELECT * FROM [dbo].[SearchBlogs]({0})", userSuppliedSearchTerm)</code>.
        ///         You can also consider using ExecuteSqlInterpolated to use interpolated string syntax to create parameters.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The SQL to execute. </param>
        /// <param name="parameters"> Parameters to use with the SQL. </param>
        /// <returns> The number of rows affected. </returns>
        [Obsolete(
            "For the execution of SQL queries using plain strings, use ExecuteSqlRaw instead. For the execution of SQL queries using interpolated string syntax to create parameters, use ExecuteSqlInterpolated instead.")]
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
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
        ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter -
        ///         <code>context.Database.ExecuteSqlCommand($"SELECT * FROM [dbo].[SearchBlogs]({userSuppliedSearchTerm})")</code>.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The interpolated string representing a SQL query with parameters. </param>
        /// <returns> The number of rows affected. </returns>
        [Obsolete(
            "For the execution of SQL queries using plain strings, use ExecuteSqlRaw instead. For the execution of SQL queries using interpolated string syntax to create parameters, use ExecuteSqlInterpolated instead.")]
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
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
        ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        ///         You can also consider using ExecuteSqlInterpolated to use interpolated string syntax to create parameters.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The SQL to execute. </param>
        /// <param name="parameters"> Parameters to use with the SQL. </param>
        /// <returns> The number of rows affected. </returns>
        [Obsolete(
            "For the execution of SQL queries using plain strings, use ExecuteSqlRaw instead. For the execution of SQL queries using interpolated string syntax to create parameters, use ExecuteSqlInterpolated instead.")]
        public static int ExecuteSqlCommand(
            [NotNull] this DatabaseFacade databaseFacade,
            RawSqlString sql,
            [NotNull] IEnumerable<object> parameters)
        {
            Check.NotNull(databaseFacade, nameof(databaseFacade));
            Check.NotNull(sql, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            var concurrencyDetector = GetFacadeDependencies(databaseFacade).ConcurrencyDetector;
            var logger = GetFacadeDependencies(databaseFacade).CommandLogger;

            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = GetFacadeDependencies(databaseFacade).RawSqlCommandBuilder
                    .Build(sql.Format, parameters);

                return rawSqlCommand
                    .RelationalCommand
                    .ExecuteNonQuery(
                        new RelationalCommandParameterObject(
                            GetFacadeDependencies(databaseFacade).RelationalConnection,
                            rawSqlCommand.ParameterValues,
                            null,
                            ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Context,
                            logger));
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
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
        ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter -
        ///         <code>context.Database.ExecuteSqlCommandAsync($"SELECT * FROM [dbo].[SearchBlogs]({userSuppliedSearchTerm})")</code>.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The interpolated string representing a SQL query with parameters. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result is the number of rows affected.
        /// </returns>
        [Obsolete(
            "For the async execution of SQL queries using plain strings, use ExecuteSqlRawAsync instead. For the async execution of SQL queries using interpolated string syntax to create parameters, use ExecuteSqlInterpolatedAsync instead.")]
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
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
        ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter -
        ///         <code>context.Database.ExecuteSqlCommandAsync("SELECT * FROM [dbo].[SearchBlogs]({0})", userSuppliedSearchTerm)</code>.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The SQL to execute. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result is the number of rows affected.
        /// </returns>
        [Obsolete(
            "For the async execution of SQL queries using plain strings, use ExecuteSqlRawAsync instead. For the async execution of SQL queries using interpolated string syntax to create parameters, use ExecuteSqlInterpolatedAsync instead.")]
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
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
        ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter -
        ///         <code>context.Database.ExecuteSqlCommandAsync("SELECT * FROM [dbo].[SearchBlogs]({0})", userSuppliedSearchTerm)</code>.
        ///         You can also consider using ExecuteSqlInterpolated to use interpolated string syntax to create parameters.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The SQL to execute. </param>
        /// <param name="parameters"> Parameters to use with the SQL. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result is the number of rows affected.
        /// </returns>
        [Obsolete(
            "For the async execution of SQL queries using plain strings, use ExecuteSqlRawAsync instead. For the async execution of SQL queries using interpolated string syntax to create parameters, use ExecuteSqlInterpolatedAsync instead.")]
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
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
        ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        ///         You can also consider using ExecuteSqlInterpolated to use interpolated string syntax to create parameters.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The SQL to execute. </param>
        /// <param name="parameters"> Parameters to use with the SQL. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result is the number of rows affected.
        /// </returns>
        [Obsolete(
            "For the async execution of SQL queries using plain strings, use ExecuteSqlRawAsync instead. For the async execution of SQL queries using interpolated string syntax to create parameters, use ExecuteSqlInterpolatedAsync instead.")]
        public static async Task<int> ExecuteSqlCommandAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            RawSqlString sql,
            [NotNull] IEnumerable<object> parameters,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(databaseFacade, nameof(databaseFacade));
            Check.NotNull(sql, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            var facadeDependencies = GetFacadeDependencies(databaseFacade);
            var concurrencyDetector = facadeDependencies.ConcurrencyDetector;
            var logger = facadeDependencies.CommandLogger;

            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = GetFacadeDependencies(databaseFacade).RawSqlCommandBuilder
                    .Build(sql.Format, parameters);

                return await rawSqlCommand
                    .RelationalCommand
                    .ExecuteNonQueryAsync(
                        new RelationalCommandParameterObject(
                            facadeDependencies.RelationalConnection,
                            rawSqlCommand.ParameterValues,
                            null,
                            ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Context,
                            logger),
                        cancellationToken);
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
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
        ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter -
        ///         <code>context.Database.ExecuteSqlRaw("SELECT * FROM [dbo].[SearchBlogs]({0})", userSuppliedSearchTerm)</code>.
        ///         You can also consider using ExecuteSqlInterpolated to use interpolated string syntax to create parameters.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The SQL to execute. </param>
        /// <param name="parameters"> Parameters to use with the SQL. </param>
        /// <returns> The number of rows affected. </returns>
        public static int ExecuteSqlRaw(
            [NotNull] this DatabaseFacade databaseFacade,
            [NotNull] string sql,
            [NotNull] params object[] parameters)
            => ExecuteSqlRaw(databaseFacade, sql, (IEnumerable<object>)parameters);

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
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
        ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter -
        ///         <code>context.Database.ExecuteSqlInterpolated($"SELECT * FROM [dbo].[SearchBlogs]({userSuppliedSearchTerm})")</code>.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The interpolated string representing a SQL query with parameters. </param>
        /// <returns> The number of rows affected. </returns>
        public static int ExecuteSqlInterpolated(
            [NotNull] this DatabaseFacade databaseFacade,
            [NotNull] FormattableString sql)
            => ExecuteSqlRaw(databaseFacade, sql.Format, sql.GetArguments());

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
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
        ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        ///         You can also consider using ExecuteSqlInterpolated to use interpolated string syntax to create parameters.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The SQL to execute. </param>
        /// <param name="parameters"> Parameters to use with the SQL. </param>
        /// <returns> The number of rows affected. </returns>
        public static int ExecuteSqlRaw(
            [NotNull] this DatabaseFacade databaseFacade,
            [NotNull] string sql,
            [NotNull] IEnumerable<object> parameters)
        {
            Check.NotNull(databaseFacade, nameof(databaseFacade));
            Check.NotNull(sql, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            var facadeDependencies = GetFacadeDependencies(databaseFacade);
            var concurrencyDetector = facadeDependencies.ConcurrencyDetector;
            var logger = facadeDependencies.CommandLogger;

            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = GetFacadeDependencies(databaseFacade).RawSqlCommandBuilder
                    .Build(sql, parameters);

                return rawSqlCommand
                    .RelationalCommand
                    .ExecuteNonQuery(
                        new RelationalCommandParameterObject(
                            facadeDependencies.RelationalConnection,
                            rawSqlCommand.ParameterValues,
                            null,
                            ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Context,
                            logger));
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
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
        ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter -
        ///         <code>context.Database.ExecuteSqlInterpolatedAsync($"SELECT * FROM [dbo].[SearchBlogs]({userSuppliedSearchTerm})")</code>.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The interpolated string representing a SQL query with parameters. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result is the number of rows affected.
        /// </returns>
        public static Task<int> ExecuteSqlInterpolatedAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            [NotNull] FormattableString sql,
            CancellationToken cancellationToken = default)
            => ExecuteSqlRawAsync(databaseFacade, sql.Format, sql.GetArguments(), cancellationToken);

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
        public static Task<int> ExecuteSqlRawAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            [NotNull] string sql,
            CancellationToken cancellationToken = default)
            => ExecuteSqlRawAsync(databaseFacade, sql, Enumerable.Empty<object>(), cancellationToken);

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
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
        ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter -
        ///         <code>context.Database.ExecuteSqlRawAsync("SELECT * FROM [dbo].[SearchBlogs]({0})", userSuppliedSearchTerm)</code>.
        ///         You can also consider using ExecuteSqlInterpolated to use interpolated string syntax to create parameters.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The SQL to execute. </param>
        /// <param name="parameters"> Parameters to use with the SQL. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result is the number of rows affected.
        /// </returns>
        public static Task<int> ExecuteSqlRawAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            [NotNull] string sql,
            [NotNull] params object[] parameters)
            => ExecuteSqlRawAsync(databaseFacade, sql, (IEnumerable<object>)parameters);

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
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
        ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter.
        ///         You can also consider using ExecuteSqlInterpolated to use interpolated string syntax to create parameters.
        ///     </para>
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="sql"> The SQL to execute. </param>
        /// <param name="parameters"> Parameters to use with the SQL. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result is the number of rows affected.
        /// </returns>
        public static async Task<int> ExecuteSqlRawAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            [NotNull] string sql,
            [NotNull] IEnumerable<object> parameters,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(databaseFacade, nameof(databaseFacade));
            Check.NotNull(sql, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            var facadeDependencies = GetFacadeDependencies(databaseFacade);
            var concurrencyDetector = facadeDependencies.ConcurrencyDetector;
            var logger = facadeDependencies.CommandLogger;

            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = GetFacadeDependencies(databaseFacade).RawSqlCommandBuilder
                    .Build(sql, parameters);

                return await rawSqlCommand
                    .RelationalCommand
                    .ExecuteNonQueryAsync(
                        new RelationalCommandParameterObject(
                            facadeDependencies.RelationalConnection,
                            rawSqlCommand.ParameterValues,
                            null,
                            ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Context,
                            logger),
                        cancellationToken);
            }
        }

        /// <summary>
        ///     Gets the underlying ADO.NET <see cref="DbConnection" /> for this <see cref="DbContext" />.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <returns> The <see cref="DbConnection" /> </returns>
        public static DbConnection GetDbConnection([NotNull] this DatabaseFacade databaseFacade)
            => GetFacadeDependencies(databaseFacade).RelationalConnection.DbConnection;

        /// <summary>
        ///     Opens the underlying <see cref="DbConnection" />.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        public static void OpenConnection([NotNull] this DatabaseFacade databaseFacade)
            => databaseFacade.CreateExecutionStrategy().Execute(
                databaseFacade, database
                    => GetFacadeDependencies(database).RelationalConnection.Open());

        /// <summary>
        ///     Opens the underlying <see cref="DbConnection" />.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public static Task OpenConnectionAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            CancellationToken cancellationToken = default)
            => databaseFacade.CreateExecutionStrategy().ExecuteAsync(
                databaseFacade, (database, ct) =>
                    GetFacadeDependencies(database).RelationalConnection.OpenAsync(cancellationToken), cancellationToken);

        /// <summary>
        ///     Closes the underlying <see cref="DbConnection" />.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        public static void CloseConnection([NotNull] this DatabaseFacade databaseFacade)
            => GetFacadeDependencies(databaseFacade).RelationalConnection.Close();

        /// <summary>
        ///     Closes the underlying <see cref="DbConnection" />.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public static Task CloseConnectionAsync([NotNull] this DatabaseFacade databaseFacade)
            => GetFacadeDependencies(databaseFacade).RelationalConnection.CloseAsync();

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
        ///     Sets the <see cref="DbTransaction" /> to be used by database operations on the <see cref="DbContext" />.
        /// </summary>
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context. </param>
        /// <param name="transaction"> The <see cref="DbTransaction" /> to use. </param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns> A <see cref="Task" /> containing the <see cref="IDbContextTransaction" /> for the given transaction. </returns>
        public static Task<IDbContextTransaction> UseTransactionAsync(
            [NotNull] this DatabaseFacade databaseFacade,
            [CanBeNull] DbTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            var transactionManager = GetTransactionManager(databaseFacade);

            if (!(transactionManager is IRelationalTransactionManager relationalTransactionManager))
            {
                throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);
            }

            return relationalTransactionManager.UseTransactionAsync(transaction, cancellationToken);
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
            => GetFacadeDependencies(databaseFacade).RelationalConnection.CommandTimeout = timeout;

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
            => GetFacadeDependencies(databaseFacade).RelationalConnection.CommandTimeout;

        /// <summary>
        ///     Generates a script to create all tables for the current model.
        /// </summary>
        /// <returns>
        ///     A SQL script.
        /// </returns>
        public static string GenerateCreateScript([NotNull] this DatabaseFacade databaseFacade)
            => databaseFacade.GetRelationalService<IRelationalDatabaseCreator>().GenerateCreateScript();

        private static IRelationalDatabaseFacadeDependencies GetFacadeDependencies(DatabaseFacade databaseFacade)
        {
            var dependencies = ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Dependencies;

            if (dependencies is IRelationalDatabaseFacadeDependencies relationalDependencies)
            {
                return relationalDependencies;
            }

            throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);
        }

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
            => ((IDatabaseFacadeDependenciesAccessor)Check.NotNull(databaseFacade, nameof(databaseFacade))).Dependencies.TransactionManager;
    }
}
