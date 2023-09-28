// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Creates and manages the current transaction.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
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
///         and <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
///     </para>
/// </remarks>
public interface IDbContextTransactionManager : IResettableService
{
    /// <summary>
    ///     Begins a new transaction.
    /// </summary>
    /// <returns>The newly created transaction.</returns>
    IDbContextTransaction BeginTransaction();

    /// <summary>
    ///     Asynchronously begins a new transaction.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the newly created transaction.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Commits all changes made to the database in the current transaction.
    /// </summary>
    void CommitTransaction();

    /// <summary>
    ///     Commits all changes made to the database in the current transaction.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Discards all changes made to the database in the current transaction.
    /// </summary>
    void RollbackTransaction();

    /// <summary>
    ///     Discards all changes made to the database in the current transaction.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the current transaction.
    /// </summary>
    IDbContextTransaction? CurrentTransaction { get; }
}
