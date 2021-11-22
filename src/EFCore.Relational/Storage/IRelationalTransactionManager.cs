// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Creates and manages the current transaction for a relational database.
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
public interface IRelationalTransactionManager : IDbContextTransactionManager
{
    /// <summary>
    ///     Begins a new transaction.
    /// </summary>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
    /// <returns>The newly created transaction.</returns>
    IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel);

    /// <summary>
    ///     Asynchronously begins a new transaction.
    /// </summary>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the newly created transaction.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Specifies an existing <see cref="DbTransaction" /> to be used for database operations.
    /// </summary>
    /// <param name="transaction">The transaction to be used.</param>
    /// <returns>An instance of <see cref="IDbTransaction" /> that wraps the provided transaction.</returns>
    IDbContextTransaction? UseTransaction(DbTransaction? transaction);

    /// <summary>
    ///     Specifies an existing <see cref="DbTransaction" /> to be used for database operations.
    /// </summary>
    /// <param name="transaction">The transaction to be used.</param>
    /// <param name="transactionId">The unique identifier for the transaction.</param>
    /// <returns>An instance of <see cref="IDbTransaction" /> that wraps the provided transaction.</returns>
    IDbContextTransaction? UseTransaction(DbTransaction? transaction, Guid transactionId);

    /// <summary>
    ///     Specifies an existing <see cref="DbTransaction" /> to be used for database operations.
    /// </summary>
    /// <param name="transaction">The transaction to be used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="IDbTransaction" /> that wraps the provided transaction.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task<IDbContextTransaction?> UseTransactionAsync(
        DbTransaction? transaction,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Specifies an existing <see cref="DbTransaction" /> to be used for database operations.
    /// </summary>
    /// <param name="transaction">The transaction to be used.</param>
    /// <param name="transactionId">The unique identifier for the transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="IDbTransaction" /> that wraps the provided transaction.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task<IDbContextTransaction?> UseTransactionAsync(
        DbTransaction? transaction,
        Guid transactionId,
        CancellationToken cancellationToken = default);
}
