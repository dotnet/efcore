// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using System.Transactions;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Performs database/schema creation, and other related operations.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public abstract class RelationalDatabaseCreator : IRelationalDatabaseCreator
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationalDatabaseCreator" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    protected RelationalDatabaseCreator(RelationalDatabaseCreatorDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalDatabaseCreatorDependencies Dependencies { get; }

    /// <summary>
    ///     Determines whether the physical database exists. No attempt is made to determine if the database
    ///     contains the schema for the current model.
    /// </summary>
    /// <returns>
    ///     <see langword="true" /> if the database exists; otherwise <see langword="false" />.
    /// </returns>
    public abstract bool Exists();

    /// <summary>
    ///     Asynchronously determines whether the physical database exists. No attempt is made to determine if
    ///     the database contains the schema for the current model.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains
    ///     <see langword="true" /> if the database exists; otherwise <see langword="false" />.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(Exists());
    }

    /// <summary>
    ///     Creates the physical database. Does not attempt to populate it with any schema.
    /// </summary>
    public abstract void Create();

    /// <summary>
    ///     Asynchronously creates the physical database. Does not attempt to populate it with any schema.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual Task CreateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Create();

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Deletes the physical database.
    /// </summary>
    public abstract void Delete();

    /// <summary>
    ///     Asynchronously deletes the physical database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Delete();

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Creates all tables for the current model in the database. No attempt is made
    ///     to incrementally update the schema. It is assumed that none of the tables exist in the database.
    /// </summary>
    public virtual void CreateTables()
        => Dependencies.MigrationCommandExecutor.ExecuteNonQuery(GetCreateTablesCommands(), Dependencies.Connection);

    /// <summary>
    ///     Asynchronously creates all tables for the current model in the database. No attempt is made
    ///     to incrementally update the schema. It is assumed that none of the tables exist in the database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual Task CreateTablesAsync(CancellationToken cancellationToken = default)
        => Dependencies.MigrationCommandExecutor.ExecuteNonQueryAsync(
            GetCreateTablesCommands(), Dependencies.Connection, cancellationToken);

    /// <summary>
    ///     Gets the commands that will create all tables from the model.
    /// </summary>
    /// <param name="options">The options to use when generating commands.</param>
    /// <returns>The generated commands.</returns>
    protected virtual IReadOnlyList<MigrationCommand> GetCreateTablesCommands(
        MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
    {
        var model = Dependencies.CurrentContext.Context.GetService<IDesignTimeModel>().Model;
        return Dependencies.MigrationsSqlGenerator.Generate(
            Dependencies.ModelDiffer.GetDifferences(null, model.GetRelationalModel()),
            model,
            options);
    }

    /// <summary>
    ///     Determines whether the database contains any tables. No attempt is made to determine if
    ///     tables belong to the current model or not.
    /// </summary>
    /// <returns>A value indicating whether any tables are present in the database.</returns>
    public abstract bool HasTables();

    /// <summary>
    ///     Asynchronously determines whether the database contains any tables. No attempt is made to determine if
    ///     tables belong to the current model or not.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains
    ///     a value indicating whether any tables are present in the database.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual Task<bool> HasTablesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(HasTables());
    }

    /// <summary>
    ///     <para>
    ///         Ensures that the database for the context does not exist. If it does not exist, no action is taken. If it does
    ///         exist then the database is deleted.
    ///     </para>
    ///     <para>
    ///         Warning: The entire database is deleted and no effort is made to remove just the database objects that are used by
    ///         the model for this context.
    ///     </para>
    /// </summary>
    /// <returns>
    ///     <see langword="true" /> if the database is deleted, <see langword="false" /> if it did not exist.
    /// </returns>
    public virtual bool EnsureDeleted()
    {
        if (Exists())
        {
            Delete();
            return true;
        }

        return false;
    }

    /// <summary>
    ///     <para>
    ///         Asynchronously ensures that the database for the context does not exist. If it does not exist, no action is taken. If it does
    ///         exist then the database is deleted.
    ///     </para>
    ///     <para>
    ///         Warning: The entire database is deleted and no effort is made to remove just the database objects that are used by
    ///         the model for this context.
    ///     </para>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous save operation. The task result contains <see langword="true" />
    ///     if the database is deleted, <see langword="false" /> if it did not exist.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual async Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
    {
        if (await ExistsAsync(cancellationToken).ConfigureAwait(false))
        {
            await DeleteAsync(cancellationToken).ConfigureAwait(false);

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Ensures that the database for the context exists. If it exists, no action is taken. If it does not
    ///     exist then the database and all its schema are created. If the database exists, then no effort is made
    ///     to ensure it is compatible with the model for this context.
    /// </summary>
    /// <returns>
    ///     <see langword="true" /> if the database is created, <see langword="false" /> if it already existed.
    /// </returns>
    public virtual bool EnsureCreated()
    {
        using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
        {
            if (!Exists())
            {
                Create();
                CreateTables();
                return true;
            }

            if (!HasTables())
            {
                CreateTables();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Asynchronously ensures that the database for the context exists. If it exists, no action is taken. If it does not
    ///     exist then the database and all its schema are created. If the database exists, then no effort is made
    ///     to ensure it is compatible with the model for this context.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous save operation. The task result contains <see langword="true" />
    ///     if the database is created, <see langword="false" /> if it already existed.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual async Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        var transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            if (!await ExistsAsync(cancellationToken).ConfigureAwait(false))
            {
                await CreateAsync(cancellationToken).ConfigureAwait(false);
                await CreateTablesAsync(cancellationToken).ConfigureAwait(false);

                return true;
            }

            if (!await HasTablesAsync(cancellationToken).ConfigureAwait(false))
            {
                await CreateTablesAsync(cancellationToken).ConfigureAwait(false);

                return true;
            }
        }
        finally
        {
            await transactionScope.DisposeAsyncIfAvailable().ConfigureAwait(false);
        }

        return false;
    }

    /// <summary>
    ///     Generates a script to create all tables for the current model.
    /// </summary>
    /// <returns>
    ///     A SQL script.
    /// </returns>
    public virtual string GenerateCreateScript()
    {
        var commands = GetCreateTablesCommands(MigrationsSqlGenerationOptions.Script);
        var builder = new StringBuilder();
        foreach (var command in commands)
        {
            builder
                .Append(command.CommandText)
                .AppendLine(Dependencies.SqlGenerationHelper.BatchTerminator);
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Determines whether or not the database is available and can be connected to.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Any exceptions thrown when attempting to connect are caught and not propagated to the application.
    ///     </para>
    ///     <para>
    ///         The configured connection string is used to create the connection in the normal way, so all
    ///         configured options such as timeouts are honored.
    ///     </para>
    ///     <para>
    ///         Note that being able to connect to the database does not mean that it is
    ///         up-to-date with regard to schema creation, etc.
    ///     </para>
    /// </remarks>
    /// <returns><see langword="true" /> if the database is available; <see langword="false" /> otherwise.</returns>
    public virtual bool CanConnect()
    {
        try
        {
            return Exists();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Determines whether or not the database is available and can be connected to.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Any exceptions thrown when attempting to connect are caught and not propagated to the application.
    ///     </para>
    ///     <para>
    ///         The configured connection string is used to create the connection in the normal way, so all
    ///         configured options such as timeouts are honored.
    ///     </para>
    ///     <para>
    ///         Note that being able to connect to the database does not mean that it is
    ///         up-to-date with regard to schema creation, etc.
    ///     </para>
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns><see langword="true" /> if the database is available; <see langword="false" /> otherwise.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await ExistsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return false;
        }
    }
}
