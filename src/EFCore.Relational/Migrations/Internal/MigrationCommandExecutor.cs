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
public class MigrationCommandExecutor(IExecutionStrategy executionStrategy) : IMigrationCommandExecutor
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
            migrationCommands.ToList(), connection, new MigrationExecutionState(), commitTransaction: true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int ExecuteNonQuery(
        IReadOnlyList<MigrationCommand> migrationCommands,
        IRelationalConnection connection,
        MigrationExecutionState executionState,
        bool commitTransaction,
        System.Data.IsolationLevel? isolationLevel = null)
    {
        var inUserTransaction = connection.CurrentTransaction is not null && executionState.Transaction == null;
        if (inUserTransaction
            && (migrationCommands.Any(x => x.TransactionSuppressed) || executionStrategy.RetriesOnFailure))
        {
            throw new NotSupportedException(RelationalStrings.TransactionSuppressedMigrationInUserTransaction);
        }

        using var transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);

        return executionStrategy.Execute(
            (migrationCommands, connection, inUserTransaction, executionState, commitTransaction, isolationLevel),
            static (_, s) => Execute(
                s.migrationCommands,
                s.connection,
                s.executionState,
                beginTransaction: !s.inUserTransaction,
                commitTransaction: !s.inUserTransaction && s.commitTransaction,
                s.isolationLevel),
            verifySucceeded: null);
    }

    private static int Execute(
        IReadOnlyList<MigrationCommand> migrationCommands,
        IRelationalConnection connection,
        MigrationExecutionState executionState,
        bool beginTransaction,
        bool commitTransaction,
        System.Data.IsolationLevel? isolationLevel)
    {
        var result = 0;
        var connectionOpened = connection.Open();
        Check.DebugAssert(!connectionOpened || executionState.Transaction == null,
            "executionState.Transaction should be null");

        try
        {
            for (var i = executionState.LastCommittedCommandIndex; i < migrationCommands.Count; i++)
            {
                var command = migrationCommands[i];
                if (executionState.Transaction == null
                    && !command.TransactionSuppressed
                    && beginTransaction)
                {
                    executionState.Transaction = isolationLevel == null
                        ? connection.BeginTransaction()
                        : connection.BeginTransaction(isolationLevel.Value);
                    if (executionState.DatabaseLock != null)
                    {
                        executionState.DatabaseLock = executionState.DatabaseLock.ReacquireIfNeeded(
                            connectionOpened, transactionRestarted: true);
                        connectionOpened = false;
                    }
                }

                if (executionState.Transaction != null
                    && command.TransactionSuppressed)
                {
                    executionState.Transaction.Commit();
                    executionState.Transaction.Dispose();
                    executionState.Transaction = null;
                    executionState.LastCommittedCommandIndex = i;
                    executionState.AnyOperationPerformed = true;

                    if (executionState.DatabaseLock != null)
                    {
                        executionState.DatabaseLock = executionState.DatabaseLock.ReacquireIfNeeded(
                            connectionOpened, transactionRestarted: null);
                        connectionOpened = false;
                    }
                }

                result = command.ExecuteNonQuery(connection);

                if (executionState.Transaction == null)
                {
                    executionState.LastCommittedCommandIndex = i + 1;
                    executionState.AnyOperationPerformed = true;
                }
            }

            if (commitTransaction
                && executionState.Transaction != null)
            {
                executionState.Transaction.Commit();
                executionState.Transaction.Dispose();
                executionState.Transaction = null;
            }
        }
        catch
        {
            executionState.Transaction?.Dispose();
            executionState.Transaction = null;
            connection.Close();
            throw;
        }

        connection.Close();
        return result;
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
            migrationCommands.ToList(), connection, new MigrationExecutionState(), commitTransaction: true, System.Data.IsolationLevel.Unspecified, cancellationToken);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task<int> ExecuteNonQueryAsync(
        IReadOnlyList<MigrationCommand> migrationCommands,
        IRelationalConnection connection,
        MigrationExecutionState executionState,
        bool commitTransaction,
        System.Data.IsolationLevel? isolationLevel = null,
        CancellationToken cancellationToken = default)
    {
        var inUserTransaction = connection.CurrentTransaction is not null && executionState.Transaction == null;
        if (inUserTransaction
            && (migrationCommands.Any(x => x.TransactionSuppressed) || executionStrategy.RetriesOnFailure))
        {
            throw new NotSupportedException(RelationalStrings.TransactionSuppressedMigrationInUserTransaction);
        }

        using var transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);

        return await executionStrategy.ExecuteAsync(
            (migrationCommands, connection, inUserTransaction, executionState, commitTransaction, isolationLevel),
            static (_, s, ct) => ExecuteAsync(
                s.migrationCommands,
                s.connection,
                s.executionState,
                beginTransaction: !s.inUserTransaction,
                commitTransaction: !s.inUserTransaction && s.commitTransaction,
                s.isolationLevel,
                ct),
            verifySucceeded: null,
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task<int> ExecuteAsync(
        IReadOnlyList<MigrationCommand> migrationCommands,
        IRelationalConnection connection,
        MigrationExecutionState executionState,
        bool beginTransaction,
        bool commitTransaction,
        System.Data.IsolationLevel? isolationLevel,
        CancellationToken cancellationToken)
    {
        var result = 0;
        var connectionOpened = await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        Check.DebugAssert(!connectionOpened || executionState.Transaction == null,
            "executionState.Transaction should be null");

        try
        {
            for (var i = executionState.LastCommittedCommandIndex; i < migrationCommands.Count; i++)
            {
                var lockReacquired = false;
                var command = migrationCommands[i];
                if (executionState.Transaction == null
                    && !command.TransactionSuppressed
                    && beginTransaction)
                {
                    executionState.Transaction = await (isolationLevel == null
                        ? connection.BeginTransactionAsync(cancellationToken)
                        : connection.BeginTransactionAsync(isolationLevel.Value, cancellationToken))
                        .ConfigureAwait(false);

                    if (executionState.DatabaseLock != null)
                    {
                        executionState.DatabaseLock = await executionState.DatabaseLock.ReacquireIfNeededAsync(
                            connectionOpened, transactionRestarted: true, cancellationToken)
                            .ConfigureAwait(false);
                        lockReacquired = true;
                    }
                }

                if (executionState.Transaction != null
                    && command.TransactionSuppressed)
                {
                    await executionState.Transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                    await executionState.Transaction.DisposeAsync().ConfigureAwait(false);
                    executionState.Transaction = null;
                    executionState.LastCommittedCommandIndex = i;
                    executionState.AnyOperationPerformed = true;

                    if (executionState.DatabaseLock != null
                        && !lockReacquired)
                    {
                        executionState.DatabaseLock = await executionState.DatabaseLock.ReacquireIfNeededAsync(
                            connectionOpened, transactionRestarted: null, cancellationToken)
                            .ConfigureAwait(false);
                    }
                }

                result = await command.ExecuteNonQueryAsync(connection, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (executionState.Transaction == null)
                {
                    executionState.LastCommittedCommandIndex = i + 1;
                    executionState.AnyOperationPerformed = true;
                }
            }

            if (commitTransaction
                && executionState.Transaction != null)
            {
                await executionState.Transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                await executionState.Transaction.DisposeAsync().ConfigureAwait(false);
                executionState.Transaction = null;
            }
        }
        catch
        {
            if (executionState.Transaction != null)
            {
                await executionState.Transaction.DisposeAsync().ConfigureAwait(false);
                executionState.Transaction = null;
            }
            await connection.CloseAsync().ConfigureAwait(false);
            throw;
        }

        await connection.CloseAsync().ConfigureAwait(false);
        return result;
    }
}
