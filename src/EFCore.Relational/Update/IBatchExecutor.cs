// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         A service for executing one or more batches of insert/update/delete commands against a database.
///     </para>
///     <para>
///         This type is typically used by database providers; it is generally not used in application code.
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
public interface IBatchExecutor
{
    /// <summary>
    ///     Executes the commands in the batches against the given database connection.
    /// </summary>
    /// <param name="commandBatches">The batches to execute.</param>
    /// <param name="connection">The database connection to use.</param>
    /// <returns>The total number of rows affected.</returns>
    int Execute(IEnumerable<ModificationCommandBatch> commandBatches, IRelationalConnection connection);

    /// <summary>
    ///     Executes the commands in the batches against the given database connection.
    /// </summary>
    /// <param name="commandBatches">The batches to execute.</param>
    /// <param name="connection">The database connection to use.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous save operation. The task result contains the
    ///     total number of rows affected.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task<int> ExecuteAsync(
        IEnumerable<ModificationCommandBatch> commandBatches,
        IRelationalConnection connection,
        CancellationToken cancellationToken = default);
}
