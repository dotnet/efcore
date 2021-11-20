// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     A transaction against the database.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are typically obtained from <see cref="DatabaseFacade.BeginTransaction" /> and it is not designed
///         to be directly constructed in your application code.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
///     </para>
/// </remarks>
public interface IDbContextTransaction : IDisposable, IAsyncDisposable
{
    /// <summary>
    ///     Gets the transaction identifier.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    Guid TransactionId { get; }

    /// <summary>
    ///     Commits all changes made to the database in the current transaction.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    void Commit();

    /// <summary>
    ///     Commits all changes made to the database in the current transaction asynchronously.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Discards all changes made to the database in the current transaction.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    void Rollback();

    /// <summary>
    ///     Discards all changes made to the database in the current transaction asynchronously.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a savepoint in the transaction. This allows all commands that are executed after the savepoint
    ///     was established to be rolled back, restoring the transaction state to what it was at the time of the
    ///     savepoint.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the savepoint to be created.</param>
    void CreateSavepoint(string name)
        => throw new NotSupportedException(CoreStrings.SavepointsNotSupported);

    /// <summary>
    ///     Creates a savepoint in the transaction. This allows all commands that are executed after the savepoint
    ///     was established to be rolled back, restoring the transaction state to what it was at the time of the
    ///     savepoint.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the savepoint to be created.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task CreateSavepointAsync(string name, CancellationToken cancellationToken = default)
        => throw new NotSupportedException(CoreStrings.SavepointsNotSupported);

    /// <summary>
    ///     Rolls back all commands that were executed after the specified savepoint was established.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the savepoint to roll back to.</param>
    void RollbackToSavepoint(string name)
        => throw new NotSupportedException(CoreStrings.SavepointsNotSupported);

    /// <summary>
    ///     Rolls back all commands that were executed after the specified savepoint was established.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the savepoint to roll back to.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task RollbackToSavepointAsync(string name, CancellationToken cancellationToken = default)
        => throw new NotSupportedException(CoreStrings.SavepointsNotSupported);

    /// <summary>
    ///     Destroys a savepoint previously defined in the current transaction. This allows the system to
    ///     reclaim some resources before the transaction ends.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If savepoint release isn't supported, <see cref="ReleaseSavepoint " /> and <see cref="ReleaseSavepointAsync " /> should
    ///         do nothing rather than throw. This is the default behavior.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="name">The name of the savepoint to release.</param>
    void ReleaseSavepoint(string name)
    {
    }

    /// <summary>
    ///     Destroys a savepoint previously defined in the current transaction. This allows the system to
    ///     reclaim some resources before the transaction ends.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If savepoint release isn't supported, <see cref="ReleaseSavepoint " /> and <see cref="ReleaseSavepointAsync " /> should
    ///         do nothing rather than throw. This is the default behavior.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="name">The name of the savepoint to release.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task ReleaseSavepointAsync(string name, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    ///     Gets a value that indicates whether this <see cref="IDbContextTransaction" /> instance supports
    ///     database savepoints. If <see langword="false" />, the methods <see cref="CreateSavepointAsync" />,
    ///     <see cref="RollbackToSavepointAsync" />
    ///     and <see cref="ReleaseSavepointAsync" /> as well as their synchronous counterparts are expected to throw
    ///     <see cref="NotSupportedException" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    /// <returns>
    ///     <see langword="true" /> if this <see cref="IDbContextTransaction" /> instance supports database savepoints;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    bool SupportsSavepoints
        => false;
}
