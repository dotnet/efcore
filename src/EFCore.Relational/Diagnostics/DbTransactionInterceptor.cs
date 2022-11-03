// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Abstract base class for <see cref="IDbTransactionInterceptor" /> for use when implementing a subset
///     of the interface methods.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see> for more information and examples.
/// </remarks>
public abstract class DbTransactionInterceptor : IDbTransactionInterceptor
{
    /// <inheritdoc />
    public virtual InterceptionResult<DbTransaction> TransactionStarting(
        DbConnection connection,
        TransactionStartingEventData eventData,
        InterceptionResult<DbTransaction> result)
        => result;

    /// <inheritdoc />
    public virtual DbTransaction TransactionStarted(DbConnection connection, TransactionEndEventData eventData, DbTransaction result)
        => result;

    /// <inheritdoc />
    public virtual ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(
        DbConnection connection,
        TransactionStartingEventData eventData,
        InterceptionResult<DbTransaction> result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <inheritdoc />
    public virtual ValueTask<DbTransaction> TransactionStartedAsync(
        DbConnection connection,
        TransactionEndEventData eventData,
        DbTransaction result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <inheritdoc />
    public virtual DbTransaction TransactionUsed(DbConnection connection, TransactionEventData eventData, DbTransaction result)
        => result;

    /// <inheritdoc />
    public virtual ValueTask<DbTransaction> TransactionUsedAsync(
        DbConnection connection,
        TransactionEventData eventData,
        DbTransaction result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <inheritdoc />
    public virtual InterceptionResult TransactionCommitting(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result)
        => result;

    /// <inheritdoc />
    public virtual void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
    {
    }

    /// <inheritdoc />
    public virtual ValueTask<InterceptionResult> TransactionCommittingAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <inheritdoc />
    public virtual Task TransactionCommittedAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual InterceptionResult TransactionRollingBack(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result)
        => result;

    /// <inheritdoc />
    public virtual void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData)
    {
    }

    /// <inheritdoc />
    public virtual ValueTask<InterceptionResult> TransactionRollingBackAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <inheritdoc />
    public virtual Task TransactionRolledBackAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual InterceptionResult CreatingSavepoint(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result)
        => result;

    /// <inheritdoc />
    public virtual void CreatedSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
    }

    /// <inheritdoc />
    public virtual ValueTask<InterceptionResult> CreatingSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <inheritdoc />
    public virtual Task CreatedSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual InterceptionResult RollingBackToSavepoint(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result)
        => result;

    /// <inheritdoc />
    public virtual void RolledBackToSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
    }

    /// <inheritdoc />
    public virtual ValueTask<InterceptionResult> RollingBackToSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <inheritdoc />
    public virtual Task RolledBackToSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual InterceptionResult ReleasingSavepoint(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result)
        => result;

    /// <inheritdoc />
    public virtual void ReleasedSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
    }

    /// <inheritdoc />
    public virtual ValueTask<InterceptionResult> ReleasingSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <inheritdoc />
    public virtual Task ReleasedSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual void TransactionFailed(DbTransaction transaction, TransactionErrorEventData eventData)
    {
    }

    /// <inheritdoc />
    public virtual Task TransactionFailedAsync(
        DbTransaction transaction,
        TransactionErrorEventData eventData,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
