// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Transactions;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class MigrationCommandExecutor : IMigrationCommandExecutor
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ExecuteNonQuery(
        IEnumerable<MigrationCommand> migrationCommands,
        IRelationalConnection connection)
        => ExecuteNonQuery(
            migrationCommands.ToList(), connection, new MigrationExecutionState(), executeInTransaction: true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ExecuteNonQuery(
        IReadOnlyList<MigrationCommand> migrationCommands,
        IRelationalConnection connection,
        MigrationExecutionState executionState,
        bool executeInTransaction)
    {
        var userTransaction = connection.CurrentTransaction;
        if (userTransaction is not null
            && migrationCommands.Any(x => x.TransactionSuppressed))
        {
            throw new NotSupportedException(RelationalStrings.TransactionSuppressedMigrationInUserTransaction);
        }

        using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
        {
            if (userTransaction is null)
            {
                Execute(migrationCommands, connection, executionState, beginTransaction: true, commitTransaction: !executeInTransaction);
            }
            else
            {
                Execute(migrationCommands, connection, executionState, beginTransaction: false, commitTransaction: false);
            }
        }
    }

    private static IDbContextTransaction? Execute(
        IReadOnlyList<MigrationCommand> migrationCommands,
        IRelationalConnection connection,
        MigrationExecutionState executionState,
        bool beginTransaction,
        bool commitTransaction)
    {
        var transaction = executionState.Transaction;
        connection.Open();
        try
        {
            for (var i = executionState.LastCommittedCommandIndex; i < migrationCommands.Count; i++)
            {
                var command = migrationCommands[i];
                if (transaction == null
                    && !command.TransactionSuppressed
                    && beginTransaction)
                {
                    transaction = connection.BeginTransaction();
                    executionState.Transaction = transaction;

                    if (executionState.DatabaseLock != null)
                    {
                        executionState.DatabaseLock = executionState.DatabaseLock.Reacquire(transaction);
                    }
                }

                if (transaction != null
                    && command.TransactionSuppressed)
                {
                    transaction.Commit();
                    transaction.Dispose();
                    transaction = null;
                    executionState.Transaction = null;
                    executionState.LastCommittedCommandIndex = i;
                }

                command.ExecuteNonQuery(connection);

                if (transaction == null)
                {
                    executionState.LastCommittedCommandIndex = i + 1;
                }
            }

            if (commitTransaction)
            {
                transaction?.Commit();
            }
        }
        catch
        {
            transaction?.Dispose();
            connection.Close();
            executionState.Transaction = null;
            throw;
        }

        if (commitTransaction)
        {
            transaction?.Dispose();
            transaction = null;
            executionState.Transaction = null;
        }

        connection.Close();
        return transaction;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task ExecuteNonQueryAsync(
        IEnumerable<MigrationCommand> migrationCommands,
        IRelationalConnection connection,
        CancellationToken cancellationToken = default)
        => ExecuteNonQueryAsync(
            migrationCommands.ToList(), connection, new MigrationExecutionState(), executeInTransaction: true, cancellationToken);
    
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task ExecuteNonQueryAsync(
        IReadOnlyList<MigrationCommand> migrationCommands,
        IRelationalConnection connection,
        MigrationExecutionState executionState,
        bool executeInTransaction,
        CancellationToken cancellationToken = default)
    {
        var userTransaction = connection.CurrentTransaction;
        if (userTransaction is not null
            && (migrationCommands.Any(x => x.TransactionSuppressed)))
        {
            throw new NotSupportedException(RelationalStrings.TransactionSuppressedMigrationInUserTransaction);
        }

        using var transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);

        if (userTransaction is null)
        {
            await ExecuteAsync(migrationCommands, connection, executionState, beginTransaction: false, commitTransaction: !executeInTransaction, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await ExecuteAsync(migrationCommands, connection, executionState, beginTransaction: false, commitTransaction: false, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<IDbContextTransaction?> ExecuteAsync(
        IReadOnlyList<MigrationCommand> migrationCommands,
        IRelationalConnection connection,
        MigrationExecutionState executionState,
        bool beginTransaction,
        bool commitTransaction,
        CancellationToken cancellationToken)
    {
        var transaction = executionState.Transaction;
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            for (var i = executionState.LastCommittedCommandIndex; i < migrationCommands.Count; i++)
            {
                var command = migrationCommands[i];
                if (transaction == null
                    && !command.TransactionSuppressed
                    && beginTransaction)
                {
                    transaction = await connection.BeginTransactionAsync(cancellationToken)
                        .ConfigureAwait(false);
                    executionState.Transaction = transaction;

                    if (executionState.DatabaseLock != null)
                    {
                        executionState.DatabaseLock = await executionState.DatabaseLock.ReacquireAsync(transaction, cancellationToken)
                            .ConfigureAwait(false);
                    }
                }

                if (transaction != null
                    && command.TransactionSuppressed)
                {
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                    await transaction.DisposeAsync().ConfigureAwait(false);
                    transaction = null;
                    executionState.Transaction = null;
                    executionState.LastCommittedCommandIndex = i;
                }

                await command.ExecuteNonQueryAsync(connection, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (transaction == null)
                {
                    executionState.LastCommittedCommandIndex = i + 1;
                }
            }

            if (transaction != null)
            {
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch
        {
            if (transaction != null)
            {
                await transaction.DisposeAsync().ConfigureAwait(false);
            }
            await connection.CloseAsync().ConfigureAwait(false);
            executionState.Transaction = null;
            throw;
        }

        if (commitTransaction
            && transaction != null)
        {
            await transaction.DisposeAsync().ConfigureAwait(false);
            transaction = null;
            executionState.Transaction = null;
        }

        await connection.CloseAsync().ConfigureAwait(false);
        return transaction;
    }
}
