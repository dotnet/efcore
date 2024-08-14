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
    {
        // TODO: Remove ToList, see #19710
        var commands = migrationCommands.ToList();
        var userTransaction = connection.CurrentTransaction;
        if (userTransaction is not null
            && (commands.Any(x => x.TransactionSuppressed) || executionStrategy.RetriesOnFailure))
        {
            throw new NotSupportedException(RelationalStrings.TransactionSuppressedMigrationInUserTransaction);
        }

        using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
        {
            var parameters = new ExecuteParameters(commands, connection);
            if (userTransaction is null)
            {
                executionStrategy.Execute(parameters, static (_, p) => Execute(p, beginTransaction: true), verifySucceeded: null);
            }
            else
            {
                Execute(parameters, beginTransaction: false);
            }
        }
    }

    private static bool Execute(ExecuteParameters parameters, bool beginTransaction)
    {
        var migrationCommands = parameters.MigrationCommands;
        var connection = parameters.Connection;
        IDbContextTransaction? transaction = null;
        connection.Open();
        try
        {
            for (var i = parameters.CurrentCommandIndex; i < migrationCommands.Count; i++)
            {
                var command = migrationCommands[i];
                if (transaction == null
                    && !command.TransactionSuppressed
                    && beginTransaction)
                {
                    transaction = connection.BeginTransaction();
                }

                if (transaction != null
                    && command.TransactionSuppressed)
                {
                    transaction.Commit();
                    transaction.Dispose();
                    transaction = null;
                    parameters.CurrentCommandIndex = i;
                }

                command.ExecuteNonQuery(connection);

                if (transaction == null)
                {
                    parameters.CurrentCommandIndex = i + 1;
                }
            }

            transaction?.Commit();
        }
        finally
        {
            transaction?.Dispose();
            connection.Close();
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task ExecuteNonQueryAsync(
        IEnumerable<MigrationCommand> migrationCommands,
        IRelationalConnection connection,
        CancellationToken cancellationToken = default)
    {
        var commands = migrationCommands.ToList();
        var userTransaction = connection.CurrentTransaction;
        if (userTransaction is not null
            && (commands.Any(x => x.TransactionSuppressed) || executionStrategy.RetriesOnFailure))
        {
            throw new NotSupportedException(RelationalStrings.TransactionSuppressedMigrationInUserTransaction);
        }

        using var transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);

        var parameters = new ExecuteParameters(commands, connection);
        if (userTransaction is null)
        {
            await executionStrategy.ExecuteAsync(
                parameters,
                static (_, p, ct) => ExecuteAsync(p, beginTransaction: true, ct),
                verifySucceeded: null,
                cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await ExecuteAsync(parameters, beginTransaction: false, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<bool> ExecuteAsync(ExecuteParameters parameters, bool beginTransaction, CancellationToken cancellationToken)
    {
        var migrationCommands = parameters.MigrationCommands;
        var connection = parameters.Connection;
        IDbContextTransaction? transaction = null;
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            for (var i = parameters.CurrentCommandIndex; i < migrationCommands.Count; i++)
            {
                var command = migrationCommands[i];
                if (transaction == null
                    && !command.TransactionSuppressed
                    && beginTransaction)
                {
                    transaction = await connection.BeginTransactionAsync(cancellationToken)
                        .ConfigureAwait(false);
                }

                if (transaction != null
                    && command.TransactionSuppressed)
                {
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                    await transaction.DisposeAsync().ConfigureAwait(false);
                    transaction = null;
                    parameters.CurrentCommandIndex = i;
                }

                await command.ExecuteNonQueryAsync(connection, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (transaction == null)
                {
                    parameters.CurrentCommandIndex = i + 1;
                }
            }

            if (transaction != null)
            {
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            if (transaction != null)
            {
                await transaction.DisposeAsync().ConfigureAwait(false);
            }

            await connection.CloseAsync().ConfigureAwait(false);
        }

        return true;
    }

    private sealed class ExecuteParameters(List<MigrationCommand> migrationCommands, IRelationalConnection connection)
    {
        public int CurrentCommandIndex;
        public List<MigrationCommand> MigrationCommands { get; } = migrationCommands;
        public IRelationalConnection Connection { get; } = connection;
    }
}
