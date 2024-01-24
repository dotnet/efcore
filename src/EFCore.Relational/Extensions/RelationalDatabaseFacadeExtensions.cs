// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Extension methods for the <see cref="DatabaseFacade" /> returned from <see cref="DbContext.Database" />
///     that can be used only with relational database providers.
/// </summary>
public static class RelationalDatabaseFacadeExtensions
{
    /// <summary>
    ///     Gets all the migrations that are defined in the configured migrations assembly.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <returns>The list of migrations.</returns>
    [RequiresDynamicCode(
        "Migrations operations are not supported with NativeAOT"
        + " Use a migration bundle or an alternate way of executing migration operations.")]
    public static IEnumerable<string> GetMigrations(this DatabaseFacade databaseFacade)
        => databaseFacade.GetRelationalService<IMigrationsAssembly>().Migrations.Keys;

    /// <summary>
    ///     Gets all migrations that have been applied to the target database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <returns>The list of migrations.</returns>
    [RequiresDynamicCode(
        "Migrations operations are not supported with NativeAOT"
        + " Use a migration bundle or an alternate way of executing migration operations.")]
    public static IEnumerable<string> GetAppliedMigrations(this DatabaseFacade databaseFacade)
        => databaseFacade.GetRelationalService<IHistoryRepository>()
            .GetAppliedMigrations().Select(hr => hr.MigrationId);

    /// <summary>
    ///     Asynchronously gets all migrations that have been applied to the target database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    [RequiresDynamicCode(
        "Migrations operations are not supported with NativeAOT"
        + " Use a migration bundle or an alternate way of executing migration operations.")]
    public static async Task<IEnumerable<string>> GetAppliedMigrationsAsync(
        this DatabaseFacade databaseFacade,
        CancellationToken cancellationToken = default)
        => (await databaseFacade.GetRelationalService<IHistoryRepository>()
            .GetAppliedMigrationsAsync(cancellationToken).ConfigureAwait(false)).Select(hr => hr.MigrationId);

    /// <summary>
    ///     Gets all migrations that are defined in the assembly but haven't been applied to the target database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <returns>The list of migrations.</returns>
    [RequiresDynamicCode(
        "Migrations operations are not supported with NativeAOT"
        + " Use a migration bundle or an alternate way of executing migration operations.")]
    public static IEnumerable<string> GetPendingMigrations(this DatabaseFacade databaseFacade)
        => GetMigrations(databaseFacade).Except(GetAppliedMigrations(databaseFacade));

    /// <summary>
    ///     Asynchronously gets all migrations that are defined in the assembly but haven't been applied to the target database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    [RequiresDynamicCode(
        "Migrations operations are not supported with NativeAOT"
        + " Use a migration bundle or an alternate way of executing migration operations.")]
    public static async Task<IEnumerable<string>> GetPendingMigrationsAsync(
        this DatabaseFacade databaseFacade,
        CancellationToken cancellationToken = default)
        => GetMigrations(databaseFacade).Except(
            await GetAppliedMigrationsAsync(databaseFacade, cancellationToken).ConfigureAwait(false));

    /// <summary>
    ///     Applies any pending migrations for the context to the database. Will create the database
    ///     if it does not already exist.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this API is mutually exclusive with <see cref="DatabaseFacade.EnsureCreated" />. EnsureCreated does not use migrations
    ///         to create the database and therefore the database that is created cannot be later updated using migrations.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    [RequiresDynamicCode(
        "Migrations operations are not supported with NativeAOT"
        + " Use a migration bundle or an alternate way of executing migration operations.")]
    public static void Migrate(this DatabaseFacade databaseFacade)
        => databaseFacade.GetRelationalService<IMigrator>().Migrate();

    /// <summary>
    ///     Asynchronously applies any pending migrations for the context to the database. Will create the database
    ///     if it does not already exist.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this API is mutually exclusive with <see cref="DatabaseFacade.EnsureCreated" />.
    ///         <see cref="DatabaseFacade.EnsureCreated" /> does not use migrations to create the database and therefore the database
    ///         that is created cannot be later updated using migrations.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous migration operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    [RequiresDynamicCode(
        "Migrations operations are not supported with NativeAOT"
        + " Use a migration bundle or an alternate way of executing migration operations.")]
    public static Task MigrateAsync(
        this DatabaseFacade databaseFacade,
        CancellationToken cancellationToken = default)
        => databaseFacade.GetRelationalService<IMigrator>().MigrateAsync(cancellationToken: cancellationToken);

    /// <summary>
    ///     Executes the given SQL against the database and returns the number of rows affected.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this method does not start a transaction. To use this method with
    ///         a transaction, first call <see cref="BeginTransaction" /> or <see cref="O:UseTransaction" />.
    ///     </para>
    ///     <para>
    ///         Note that the current <see cref="ExecutionStrategy" /> is not used by this method
    ///         since the SQL may not be idempotent and does not run in a transaction. An <see cref="ExecutionStrategy" />
    ///         can be used explicitly, making sure to also use a transaction if the SQL is not
    ///         idempotent.
    ///     </para>
    ///     <para>
    ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
    ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
    ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter.
    ///     </para>
    ///     <para>
    ///         However, <b>never</b> pass a concatenated or interpolated string (<c>$""</c>) with non-validated user-provided values
    ///         into this method. Doing so may expose your application to SQL injection attacks. To use the interpolated string syntax,
    ///         consider using <see cref="ExecuteSql" /> to create parameters.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-raw-sql">Executing raw SQL commands with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="sql">The SQL to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The number of rows affected.</returns>
    public static int ExecuteSqlRaw(
        this DatabaseFacade databaseFacade,
        string sql,
        params object[] parameters)
        => ExecuteSqlRaw(databaseFacade, sql, (IEnumerable<object>)parameters);

    /// <summary>
    ///     Executes the given SQL against the database and returns the number of rows affected.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this method does not start a transaction. To use this method with
    ///         a transaction, first call <see cref="BeginTransaction" /> or <see cref="O:UseTransaction" />.
    ///     </para>
    ///     <para>
    ///         Note that the current <see cref="ExecutionStrategy" /> is not used by this method
    ///         since the SQL may not be idempotent and does not run in a transaction. An <see cref="ExecutionStrategy" />
    ///         can be used explicitly, making sure to also use a transaction if the SQL is not
    ///         idempotent.
    ///     </para>
    ///     <para>
    ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
    ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
    ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-raw-sql">Executing raw SQL commands with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="sql">The interpolated string representing a SQL query with parameters.</param>
    /// <returns>The number of rows affected.</returns>
    public static int ExecuteSqlInterpolated(
        this DatabaseFacade databaseFacade,
        FormattableString sql)
        => ExecuteSqlRaw(databaseFacade, sql.Format, sql.GetArguments()!);

    /// <summary>
    ///     Executes the given SQL against the database and returns the number of rows affected.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this method does not start a transaction. To use this method with
    ///         a transaction, first call <see cref="BeginTransaction" /> or <see cref="O:UseTransaction" />.
    ///     </para>
    ///     <para>
    ///         Note that the current <see cref="ExecutionStrategy" /> is not used by this method
    ///         since the SQL may not be idempotent and does not run in a transaction. An <see cref="ExecutionStrategy" />
    ///         can be used explicitly, making sure to also use a transaction if the SQL is not
    ///         idempotent.
    ///     </para>
    ///     <para>
    ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
    ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
    ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-raw-sql">Executing raw SQL commands with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="sql">The interpolated string representing a SQL query with parameters.</param>
    /// <returns>The number of rows affected.</returns>
    public static int ExecuteSql(
        this DatabaseFacade databaseFacade,
        FormattableString sql)
        => ExecuteSqlRaw(databaseFacade, sql.Format, sql.GetArguments()!);

    /// <summary>
    ///     Executes the given SQL against the database and returns the number of rows affected.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this method does not start a transaction. To use this method with
    ///         a transaction, first call <see cref="BeginTransaction" /> or <see cref="O:UseTransaction" />.
    ///     </para>
    ///     <para>
    ///         Note that the current <see cref="ExecutionStrategy" /> is not used by this method
    ///         since the SQL may not be idempotent and does not run in a transaction. An <see cref="ExecutionStrategy" />
    ///         can be used explicitly, making sure to also use a transaction if the SQL is not
    ///         idempotent.
    ///     </para>
    ///     <para>
    ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
    ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
    ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter.
    ///     </para>
    ///     <para>
    ///         However, <b>never</b> pass a concatenated or interpolated string (<c>$""</c>) with non-validated user-provided values
    ///         into this method. Doing so may expose your application to SQL injection attacks. To use the interpolated string syntax,
    ///         consider using <see cref="ExecuteSql" /> to create parameters.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-raw-sql">Executing raw SQL commands with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="sql">The SQL to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The number of rows affected.</returns>
    public static int ExecuteSqlRaw(
        this DatabaseFacade databaseFacade,
        string sql,
        IEnumerable<object> parameters)
    {
        Check.NotNull(sql, nameof(sql));
        Check.NotNull(parameters, nameof(parameters));

        var facadeDependencies = GetFacadeDependencies(databaseFacade);
        var concurrencyDetector = facadeDependencies.CoreOptions.AreThreadSafetyChecksEnabled
            ? facadeDependencies.ConcurrencyDetector
            : null;
        var logger = facadeDependencies.CommandLogger;

        concurrencyDetector?.EnterCriticalSection();

        try
        {
            var rawSqlCommand = facadeDependencies.RawSqlCommandBuilder
                .Build(sql, parameters, databaseFacade.GetService<IModel>());

            return rawSqlCommand
                .RelationalCommand
                .ExecuteNonQuery(
                    new RelationalCommandParameterObject(
                        facadeDependencies.RelationalConnection,
                        rawSqlCommand.ParameterValues,
                        null,
                        ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Context,
                        logger, CommandSource.ExecuteSqlRaw));
        }
        finally
        {
            concurrencyDetector?.ExitCriticalSection();
        }
    }

    /// <summary>
    ///     Creates a LINQ query based on a raw SQL query, which returns a result set of a scalar type natively supported by the database
    ///     provider.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         To use this method with a return type that isn't natively supported by the database provider, use the
    ///         <see cref="ModelConfigurationBuilder.DefaultTypeMapping{TScalar}(Action{TypeMappingConfigurationBuilder{TScalar}})" />
    ///         method.
    ///     </para>
    ///     <para>
    ///         The returned <see cref="IQueryable{TResult}" /> can be composed over using LINQ to build more complex queries.
    ///     </para>
    ///     <para>
    ///         Note that this method does not start a transaction. To use this method with a transaction, first call
    ///         <see cref="BeginTransaction" /> or <see cref="O:UseTransaction" />.
    ///     </para>
    ///     <para>
    ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
    ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
    ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter.
    ///     </para>
    ///     <para>
    ///         However, <b>never</b> pass a concatenated or interpolated string (<c>$""</c>) with non-validated user-provided values
    ///         into this method. Doing so may expose your application to SQL injection attacks. To use the interpolated string syntax,
    ///         consider using <see cref="SqlQuery{TResult}(DatabaseFacade, FormattableString)" /> to create parameters.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-raw-sql">Executing raw SQL commands with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="sql">The raw SQL query.</param>
    /// <param name="parameters">The values to be assigned to parameters.</param>
    /// <returns>An <see cref="IQueryable{T}" /> representing the raw SQL query.</returns>
    [StringFormatMethod("sql")]
    public static IQueryable<TResult> SqlQueryRaw<TResult>(
        this DatabaseFacade databaseFacade,
        [NotParameterized] string sql,
        params object[] parameters)
    {
        Check.NotNull(sql, nameof(sql));
        Check.NotNull(parameters, nameof(parameters));

        var facadeDependencies = GetFacadeDependencies(databaseFacade);
        var queryProvider = facadeDependencies.QueryProvider;
        var argumentsExpression = Expression.Constant(parameters);

        return queryProvider.CreateQuery<TResult>(
            facadeDependencies.TypeMappingSource.FindMapping(typeof(TResult)) != null
                ? new SqlQueryRootExpression(queryProvider, typeof(TResult), sql, argumentsExpression)
                : new FromSqlQueryRootExpression(
                    queryProvider, facadeDependencies.AdHocMapper.GetOrAddEntityType(typeof(TResult)), sql, argumentsExpression));
    }

    /// <summary>
    ///     Creates a LINQ query based on a raw SQL query, which returns a result set of a scalar type natively supported by the database
    ///     provider.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         To use this method with a return type that isn't natively supported by the database provider, use the
    ///         <see cref="ModelConfigurationBuilder.DefaultTypeMapping{TScalar}(Action{TypeMappingConfigurationBuilder{TScalar}})" />
    ///         method.
    ///     </para>
    ///     <para>
    ///         The returned <see cref="IQueryable{TResult}" /> can be composed over using LINQ to build more complex queries.
    ///     </para>
    ///     <para>
    ///         Note that this method does not start a transaction. To use this method with a transaction, first call
    ///         <see cref="BeginTransaction" /> or <see cref="O:UseTransaction" />.
    ///     </para>
    ///     <para>
    ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
    ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
    ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-raw-sql">Executing raw SQL commands with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="sql">The interpolated string representing a SQL query with parameters.</param>
    /// <returns>An <see cref="IQueryable{T}" /> representing the interpolated string SQL query.</returns>
    public static IQueryable<TResult> SqlQuery<TResult>(
        this DatabaseFacade databaseFacade,
        [NotParameterized] FormattableString sql)
        => SqlQueryRaw<TResult>(databaseFacade, sql.Format, sql.GetArguments()!);

    /// <summary>
    ///     Executes the given SQL against the database and returns the number of rows affected.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this method does not start a transaction. To use this method with
    ///         a transaction, first call <see cref="BeginTransaction" /> or <see cref="O:UseTransaction" />.
    ///     </para>
    ///     <para>
    ///         Note that the current <see cref="ExecutionStrategy" /> is not used by this method
    ///         since the SQL may not be idempotent and does not run in a transaction. An <see cref="ExecutionStrategy" />
    ///         can be used explicitly, making sure to also use a transaction if the SQL is not
    ///         idempotent.
    ///     </para>
    ///     <para>
    ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
    ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
    ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-raw-sql">Executing raw SQL commands with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="sql">The interpolated string representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is the number of rows affected.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task<int> ExecuteSqlInterpolatedAsync(
        this DatabaseFacade databaseFacade,
        FormattableString sql,
        CancellationToken cancellationToken = default)
        => ExecuteSqlRawAsync(databaseFacade, sql.Format, sql.GetArguments()!, cancellationToken);

    /// <summary>
    ///     Executes the given SQL against the database and returns the number of rows affected.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this method does not start a transaction. To use this method with
    ///         a transaction, first call <see cref="BeginTransaction" /> or <see cref="O:UseTransaction" />.
    ///     </para>
    ///     <para>
    ///         Note that the current <see cref="ExecutionStrategy" /> is not used by this method
    ///         since the SQL may not be idempotent and does not run in a transaction. An <see cref="ExecutionStrategy" />
    ///         can be used explicitly, making sure to also use a transaction if the SQL is not
    ///         idempotent.
    ///     </para>
    ///     <para>
    ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
    ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
    ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-raw-sql">Executing raw SQL commands with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="sql">The interpolated string representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is the number of rows affected.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task<int> ExecuteSqlAsync(
        this DatabaseFacade databaseFacade,
        FormattableString sql,
        CancellationToken cancellationToken = default)
        => ExecuteSqlRawAsync(databaseFacade, sql.Format, sql.GetArguments()!, cancellationToken);

    /// <summary>
    ///     Executes the given SQL against the database and returns the number of rows affected.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this method does not start a transaction. To use this method with
    ///         a transaction, first call <see cref="BeginTransaction" /> or <see cref="O:UseTransaction" />.
    ///     </para>
    ///     <para>
    ///         Note that the current <see cref="ExecutionStrategy" /> is not used by this method
    ///         since the SQL may not be idempotent and does not run in a transaction. An <see cref="ExecutionStrategy" />
    ///         can be used explicitly, making sure to also use a transaction if the SQL is not idempotent.
    ///     </para>
    ///     <para>
    ///         <b>Never</b> pass a concatenated or interpolated string (<c>$""</c>) with non-validated user-provided values
    ///         into this method. Doing so may expose your application to SQL injection attacks.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-raw-sql">Executing raw SQL commands with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="sql">The SQL to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is the number of rows affected.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task<int> ExecuteSqlRawAsync(
        this DatabaseFacade databaseFacade,
        string sql,
        CancellationToken cancellationToken = default)
        => ExecuteSqlRawAsync(databaseFacade, sql, Enumerable.Empty<object>(), cancellationToken);

    /// <summary>
    ///     Executes the given SQL against the database and returns the number of rows affected.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this method does not start a transaction. To use this method with
    ///         a transaction, first call <see cref="BeginTransaction" /> or <see cref="O:UseTransaction" />.
    ///     </para>
    ///     <para>
    ///         Note that the current <see cref="ExecutionStrategy" /> is not used by this method
    ///         since the SQL may not be idempotent and does not run in a transaction. An <see cref="ExecutionStrategy" />
    ///         can be used explicitly, making sure to also use a transaction if the SQL is not
    ///         idempotent.
    ///     </para>
    ///     <para>
    ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
    ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
    ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter.
    ///     </para>
    ///     <para>
    ///         However, <b>never</b> pass a concatenated or interpolated string (<c>$""</c>) with non-validated user-provided values
    ///         into this method. Doing so may expose your application to SQL injection attacks. To use the interpolated string syntax,
    ///         consider using <see cref="ExecuteSqlAsync" /> to create parameters.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-raw-sql">Executing raw SQL commands with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="sql">The SQL to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is the number of rows affected.
    /// </returns>
    public static Task<int> ExecuteSqlRawAsync(
        this DatabaseFacade databaseFacade,
        string sql,
        params object[] parameters)
        => ExecuteSqlRawAsync(databaseFacade, sql, (IEnumerable<object>)parameters);

    /// <summary>
    ///     Executes the given SQL against the database and returns the number of rows affected.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this method does not start a transaction. To use this method with
    ///         a transaction, first call <see cref="BeginTransaction" /> or <see cref="O:UseTransaction" />.
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
    ///     </para>
    ///     <para>
    ///         However, <b>never</b> pass a concatenated or interpolated string (<c>$""</c>) with non-validated user-provided values
    ///         into this method. Doing so may expose your application to SQL injection attacks. To use the interpolated string syntax,
    ///         consider using <see cref="ExecuteSqlAsync" /> to create parameters.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-raw-sql">Executing raw SQL commands with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="sql">The SQL to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is the number of rows affected.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static async Task<int> ExecuteSqlRawAsync(
        this DatabaseFacade databaseFacade,
        string sql,
        IEnumerable<object> parameters,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(sql, nameof(sql));
        Check.NotNull(parameters, nameof(parameters));

        var facadeDependencies = GetFacadeDependencies(databaseFacade);
        var concurrencyDetector = facadeDependencies.CoreOptions.AreThreadSafetyChecksEnabled
            ? facadeDependencies.ConcurrencyDetector
            : null;
        var logger = facadeDependencies.CommandLogger;

        concurrencyDetector?.EnterCriticalSection();

        try
        {
            var rawSqlCommand = facadeDependencies.RawSqlCommandBuilder
                .Build(sql, parameters, databaseFacade.GetService<IModel>());

            return await rawSqlCommand
                .RelationalCommand
                .ExecuteNonQueryAsync(
                    new RelationalCommandParameterObject(
                        facadeDependencies.RelationalConnection,
                        rawSqlCommand.ParameterValues,
                        null,
                        ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Context,
                        logger, CommandSource.ExecuteSqlRaw),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            concurrencyDetector?.ExitCriticalSection();
        }
    }

    /// <summary>
    ///     Gets the underlying ADO.NET <see cref="DbConnection" /> for this <see cref="DbContext" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This connection should not be disposed if it was created by Entity Framework. Connections are created by
    ///         Entity Framework when a connection string rather than a DbConnection object is passed to the 'UseMyProvider'
    ///         method for the database provider in use. Conversely, the application is responsible for disposing a DbConnection
    ///         passed to Entity Framework in 'UseMyProvider'.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-connections">Connections and connection strings</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <returns>The <see cref="DbConnection" /></returns>
    public static DbConnection GetDbConnection(this DatabaseFacade databaseFacade)
        => GetFacadeDependencies(databaseFacade).RelationalConnection.DbConnection;

    /// <summary>
    ///     Sets the underlying ADO.NET <see cref="DbConnection" /> for this <see cref="DbContext" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The connection can only be set when the existing connection, if any, is not open.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-connections">Connections and connection strings</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="contextOwnsConnection">
    ///     If <see langword="true" />, then EF will take ownership of the connection and will
    ///     dispose it in the same way it would dispose a connection created by EF. If <see langword="false" />, then the caller still
    ///     owns the connection and is responsible for its disposal. The default value is <see langword="false" />.
    /// </param>
    public static void SetDbConnection(this DatabaseFacade databaseFacade, DbConnection? connection, bool contextOwnsConnection = false)
        => GetFacadeDependencies(databaseFacade).RelationalConnection.SetDbConnection(connection, contextOwnsConnection);

    /// <summary>
    ///     Gets the underlying connection string configured for this <see cref="DbContext" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connections">Connections and connection strings</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <returns>The connection string.</returns>
    public static string? GetConnectionString(this DatabaseFacade databaseFacade)
        => GetFacadeDependencies(databaseFacade).RelationalConnection.ConnectionString;

    /// <summary>
    ///     Sets the underlying connection string configured for this <see cref="DbContext" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         It may not be possible to change the connection string if existing connection, if any, is open.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-connections">Connections and connection strings</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="connectionString">The connection string.</param>
    public static void SetConnectionString(this DatabaseFacade databaseFacade, string? connectionString)
        => GetFacadeDependencies(databaseFacade).RelationalConnection.ConnectionString = connectionString;

    /// <summary>
    ///     Opens the underlying <see cref="DbConnection" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connections">Connections and connection strings</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    public static void OpenConnection(this DatabaseFacade databaseFacade)
        => ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Dependencies.ExecutionStrategy
            .Execute(databaseFacade, database => GetFacadeDependencies(database).RelationalConnection.Open(), null);

    /// <summary>
    ///     Opens the underlying <see cref="DbConnection" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connections">Connections and connection strings</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task OpenConnectionAsync(
        this DatabaseFacade databaseFacade,
        CancellationToken cancellationToken = default)
        => ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Dependencies.ExecutionStrategy
            .ExecuteAsync(
                databaseFacade, (database, ct) => GetFacadeDependencies(database).RelationalConnection.OpenAsync(ct), null,
                cancellationToken);

    /// <summary>
    ///     Closes the underlying <see cref="DbConnection" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connections">Connections and connection strings</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    public static void CloseConnection(this DatabaseFacade databaseFacade)
        => GetFacadeDependencies(databaseFacade).RelationalConnection.Close();

    /// <summary>
    ///     Closes the underlying <see cref="DbConnection" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connections">Connections and connection strings</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task CloseConnectionAsync(this DatabaseFacade databaseFacade)
        => GetFacadeDependencies(databaseFacade).RelationalConnection.CloseAsync();

    /// <summary>
    ///     Starts a new transaction with a given <see cref="IsolationLevel" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="isolationLevel">The <see cref="IsolationLevel" /> to use.</param>
    /// <returns>A <see cref="IDbContextTransaction" /> that represents the started transaction.</returns>
    public static IDbContextTransaction BeginTransaction(this DatabaseFacade databaseFacade, IsolationLevel isolationLevel)
        => ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Dependencies.ExecutionStrategy.Execute(
            databaseFacade, database =>
            {
                var transactionManager = database.GetTransactionManager();

                return transactionManager is IRelationalTransactionManager relationalTransactionManager
                    ? relationalTransactionManager.BeginTransaction(isolationLevel)
                    : transactionManager.BeginTransaction();
            },
            null);

    /// <summary>
    ///     Asynchronously starts a new transaction with a given <see cref="IsolationLevel" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="isolationLevel">The <see cref="IsolationLevel" /> to use.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous transaction initialization. The task result contains a <see cref="IDbContextTransaction" />
    ///     that represents the started transaction.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task<IDbContextTransaction> BeginTransactionAsync(
        this DatabaseFacade databaseFacade,
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
        => ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Dependencies.ExecutionStrategy.ExecuteAsync(
            databaseFacade, (database, ct) =>
            {
                var transactionManager = database.GetTransactionManager();

                return transactionManager is IRelationalTransactionManager relationalTransactionManager
                    ? relationalTransactionManager.BeginTransactionAsync(isolationLevel, ct)
                    : transactionManager.BeginTransactionAsync(ct);
            }, null, cancellationToken);

    /// <summary>
    ///     Sets the <see cref="DbTransaction" /> to be used by database operations on the <see cref="DbContext" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="transaction">The <see cref="DbTransaction" /> to use.</param>
    /// <returns>A <see cref="IDbContextTransaction" /> that encapsulates the given transaction.</returns>
    public static IDbContextTransaction? UseTransaction(
        this DatabaseFacade databaseFacade,
        DbTransaction? transaction)
        => databaseFacade.UseTransaction(transaction, Guid.NewGuid());

    /// <summary>
    ///     Sets the <see cref="DbTransaction" /> to be used by database operations on the <see cref="DbContext" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="transaction">The <see cref="DbTransaction" /> to use.</param>
    /// <param name="transactionId">The unique identifier for the transaction.</param>
    /// <returns>A <see cref="IDbContextTransaction" /> that encapsulates the given transaction.</returns>
    public static IDbContextTransaction? UseTransaction(
        this DatabaseFacade databaseFacade,
        DbTransaction? transaction,
        Guid transactionId)
        => GetTransactionManager(databaseFacade) is IRelationalTransactionManager relationalTransactionManager
            ? relationalTransactionManager.UseTransaction(transaction, transactionId)
            : throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);

    /// <summary>
    ///     Sets the <see cref="DbTransaction" /> to be used by database operations on the <see cref="DbContext" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="transaction">The <see cref="DbTransaction" /> to use.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> containing the <see cref="IDbContextTransaction" /> for the given transaction.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task<IDbContextTransaction?> UseTransactionAsync(
        this DatabaseFacade databaseFacade,
        DbTransaction? transaction,
        CancellationToken cancellationToken = default)
        => databaseFacade.UseTransactionAsync(transaction, Guid.NewGuid(), cancellationToken);

    /// <summary>
    ///     Sets the <see cref="DbTransaction" /> to be used by database operations on the <see cref="DbContext" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="transaction">The <see cref="DbTransaction" /> to use.</param>
    /// <param name="transactionId">The unique identifier for the transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> containing the <see cref="IDbContextTransaction" /> for the given transaction.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task<IDbContextTransaction?> UseTransactionAsync(
        this DatabaseFacade databaseFacade,
        DbTransaction? transaction,
        Guid transactionId,
        CancellationToken cancellationToken = default)
        => GetTransactionManager(databaseFacade) is IRelationalTransactionManager relationalTransactionManager
            ? relationalTransactionManager.UseTransactionAsync(transaction, transactionId, cancellationToken)
            : throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);

    /// <summary>
    ///     Sets the timeout (in seconds) to use for commands executed with this <see cref="DbContext" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If this value is set, then it is used to set <see cref="DbCommand.CommandTimeout" /> whenever Entity Framework creates a
    ///         <see cref="DbCommand" /> to execute a query.
    ///     </para>
    ///     <para>
    ///         If this value is not set, then the default value used is defined by the underlying ADO.NET data provider.
    ///         Consult the documentation for the implementation of <see cref="DbCommand" /> in the ADO.NET data provider for details of
    ///         default values, etc.
    ///     </para>
    ///     <para>
    ///         Note that the command timeout is distinct from the connection timeout. Connection timeouts are usually
    ///         configured in the connection string. More recently, some ADO.NET data providers are adding the capability
    ///         to also set a command timeout in the connection string. A value set with this API for the command timeout
    ///         will override any value set in the connection string.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-connections">Connections and connection strings</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="timeout">The timeout to use, in seconds.</param>
    public static void SetCommandTimeout(this DatabaseFacade databaseFacade, int? timeout)
        => GetFacadeDependencies(databaseFacade).RelationalConnection.CommandTimeout = timeout;

    /// <summary>
    ///     Sets the timeout to use for commands executed with this <see cref="DbContext" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is a sugar method allowing a <see cref="TimeSpan" /> to be used to set the value. It delegates to
    ///         <see cref="SetCommandTimeout(DatabaseFacade,int?)" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-connections">Connections and connection strings</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="timeout">The timeout to use.</param>
    public static void SetCommandTimeout(this DatabaseFacade databaseFacade, TimeSpan timeout)
    {
        if (timeout == Timeout.InfiniteTimeSpan)
        {
            SetCommandTimeout(databaseFacade, 0);
            return;
        }

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
    ///     Returns the timeout (in seconds) set for commands executed with this <see cref="DbContext" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that the command timeout is distinct from the connection timeout, which is commonly
    ///         set on the database connection string.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-connections">Connections and connection strings</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <returns>The timeout, in seconds, or null if no timeout has been set.</returns>
    public static int? GetCommandTimeout(this DatabaseFacade databaseFacade)
        => GetFacadeDependencies(databaseFacade).RelationalConnection.CommandTimeout;

    /// <summary>
    ///     Generates a script to create all tables for the current model.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <returns>
    ///     A SQL script.
    /// </returns>
    [RequiresDynamicCode(
        "Migrations operations are not supported with NativeAOT"
        + " Use a migration bundle or an alternate way of executing migration operations.")]
    public static string GenerateCreateScript(this DatabaseFacade databaseFacade)
        => databaseFacade.GetRelationalService<IRelationalDatabaseCreator>().GenerateCreateScript();

    /// <summary>
    ///     Returns <see langword="true" /> if the database provider currently in use is a relational database.
    /// </summary>
    /// <param name="databaseFacade">The facade from <see cref="DbContext.Database" />.</param>
    /// <returns>
    ///     <see langword="true" /> if a relational database provider is being used;
    ///     <see langword="false" /> otherwise.
    /// </returns>
    public static bool IsRelational(this DatabaseFacade databaseFacade)
        => ((IDatabaseFacadeDependenciesAccessor)databaseFacade)
            .Context.GetService<IDbContextOptions>().Extensions.OfType<RelationalOptionsExtension>().Any();

    /// <summary>
    ///     Returns <see langword="true" /> if the model has pending changes to be applied.
    /// </summary>
    /// <param name="databaseFacade">The facade from <see cref="DbContext.Database" />.</param>
    /// <returns>
    ///     <see langword="true" /> if the database model has pending changes
    ///     and a new migration has to be added.
    /// </returns>
    [RequiresDynamicCode(
        "Migrations operations are not supported with NativeAOT"
        + " Use a migration bundle or an alternate way of executing migration operations.")]
    public static bool HasPendingModelChanges(this DatabaseFacade databaseFacade)
    {
        var modelDiffer = databaseFacade.GetRelationalService<IMigrationsModelDiffer>();
        var migrationsAssembly = databaseFacade.GetRelationalService<IMigrationsAssembly>();

        var modelInitializer = databaseFacade.GetRelationalService<IModelRuntimeInitializer>();

        var snapshotModel = migrationsAssembly.ModelSnapshot?.Model;
        if (snapshotModel is IMutableModel mutableModel)
        {
            snapshotModel = mutableModel.FinalizeModel();
        }

        if (snapshotModel is not null)
        {
            snapshotModel = modelInitializer.Initialize(snapshotModel);
        }

        var designTimeModel = databaseFacade.GetRelationalService<IDesignTimeModel>();

        return modelDiffer.HasDifferences(
            snapshotModel?.GetRelationalModel(),
            designTimeModel.Model.GetRelationalModel());
    }

    private static IRelationalDatabaseFacadeDependencies GetFacadeDependencies(DatabaseFacade databaseFacade)
    {
        var dependencies = ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Dependencies;

        return dependencies is IRelationalDatabaseFacadeDependencies relationalDependencies
            ? relationalDependencies
            : throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);
    }

    private static TService GetRelationalService<TService>(this IInfrastructure<IServiceProvider> databaseFacade)
    {
        var service = databaseFacade.Instance.GetService<TService>();
        return service == null
            ? throw new InvalidOperationException(RelationalStrings.RelationalNotInUse)
            : service;
    }

    private static IDbContextTransactionManager GetTransactionManager(this DatabaseFacade databaseFacade)
        => ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Dependencies.TransactionManager;
}
