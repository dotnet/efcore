// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     A service for executing migration commands against a database.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
///     </para>
/// </remarks>
public interface IMigrationCommandExecutor
{
    /// <summary>
    ///     Executes the given commands using the given database connection.
    /// </summary>
    /// <param name="migrationCommands">The commands to execute.</param>
    /// <param name="connection">The connection to use.</param>
    void ExecuteNonQuery(
        IEnumerable<MigrationCommand> migrationCommands,
        IRelationalConnection connection);

    /// <summary>
    ///     Executes the given commands using the given database connection.
    /// </summary>
    /// <param name="migrationCommands">The commands to execute.</param>
    /// <param name="connection">The connection to use.</param>
    /// <param name="executionState">The state of the current migration execution.</param>
    /// <param name="commitTransaction">
    ///     Indicates whether the transaction started by this call should be commited.
    ///     If <see langword="false" />, the transaction will be made available in <paramref name="executionState"/>.
    /// </param>
    /// <param name="isolationLevel">The isolation level for the transaction.</param>
    int ExecuteNonQuery(
        IReadOnlyList<MigrationCommand> migrationCommands,
        IRelationalConnection connection,
        MigrationExecutionState executionState,
        bool commitTransaction,
        IsolationLevel? isolationLevel = null);

    /// <summary>
    ///     Executes the given commands using the given database connection.
    /// </summary>
    /// <param name="migrationCommands">The commands to execute.</param>
    /// <param name="connection">The connection to use.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task ExecuteNonQueryAsync(
        IEnumerable<MigrationCommand> migrationCommands,
        IRelationalConnection connection,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes the given commands using the given database connection.
    /// </summary>
    /// <param name="migrationCommands">The commands to execute.</param>
    /// <param name="connection">The connection to use.</param>
    /// <param name="executionState">The state of the current migration execution.</param>
    /// <param name="commitTransaction">
    ///     Indicates whether the transaction started by this call should be commited.
    ///     If <see langword="false" />, the transaction will be made available in <paramref name="executionState"/>.
    /// </param>
    /// <param name="isolationLevel">The isolation level for the transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task<int> ExecuteNonQueryAsync(
        IReadOnlyList<MigrationCommand> migrationCommands,
        IRelationalConnection connection,
        MigrationExecutionState executionState,
        bool commitTransaction,
        IsolationLevel? isolationLevel = null,
        CancellationToken cancellationToken = default);
}
