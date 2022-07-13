// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Allows interception of operations related to a <see cref="DbTransaction" />.
/// </summary>
/// <remarks>
///     <para>
///         Transaction interceptors can be used to view, change, or suppress operations on <see cref="DbTransaction" />, and
///         to modify the result before it is returned to EF.
///     </para>
///     <para>
///         Consider inheriting from <see cref="DbTransactionInterceptor" /> if not implementing all methods.
///     </para>
///     <para>
///         Use <see cref="DbContextOptionsBuilder.AddInterceptors(Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor[])" />
///         to register application interceptors.
///     </para>
///     <para>
///         Extensions can also register interceptors in the internal service provider.
///         If both injected and application interceptors are found, then the injected interceptors are run in the
///         order that they are resolved from the service provider, and then the application interceptors are run last.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see> for more information and examples.
///     </para>
/// </remarks>
public interface IDbTransactionInterceptor : IInterceptor
{
    /// <summary>
    ///     Called just before EF intends to call <see cref="DbConnection.BeginTransaction(IsolationLevel)" />.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult{DbTransaction}.HasResult" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult{DbTransaction}.SuppressWithResult" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult{DbTransaction}.HasResult" /> is false, the EF will continue as normal.
    ///     If <see cref="InterceptionResult{DbTransaction}.HasResult" /> is true, then EF will suppress the operation it
    ///     was about to perform and use <see cref="InterceptionResult{DbTransaction}.Result" /> instead.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
    /// </returns>
    InterceptionResult<DbTransaction> TransactionStarting(
        DbConnection connection,
        TransactionStartingEventData eventData,
        InterceptionResult<DbTransaction> result)
        => result;

    /// <summary>
    ///     Called immediately after EF calls <see cref="DbConnection.BeginTransaction(IsolationLevel)" />.
    /// </summary>
    /// <remarks>
    ///     This method is still called if an interceptor suppressed creation in <see cref="TransactionStarting" />.
    ///     In this case, <paramref name="result" /> is the result returned by <see cref="TransactionStarting" />.
    /// </remarks>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="result">
    ///     The result of the call to <see cref="DbConnection.BeginTransaction(IsolationLevel)" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     The result that EF will use.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in.
    /// </returns>
    DbTransaction TransactionStarted(DbConnection connection, TransactionEndEventData eventData, DbTransaction result)
        => result;

    /// <summary>
    ///     Called just before EF intends to call
    ///     <see cref="DbConnection.BeginTransactionAsync(IsolationLevel,CancellationToken)" />.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult{DbTransaction}.HasResult" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult{DbTransaction}.SuppressWithResult" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     If <see cref="InterceptionResult{DbTransaction}.HasResult" /> is false, the EF will continue as normal.
    ///     If <see cref="InterceptionResult{DbTransaction}.HasResult" /> is true, then EF will suppress the operation it
    ///     was about to perform and use <see cref="InterceptionResult{DbTransaction}.Result" /> instead.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(
        DbConnection connection,
        TransactionStartingEventData eventData,
        InterceptionResult<DbTransaction> result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <summary>
    ///     Called immediately after EF calls <see cref="DbConnection.BeginTransactionAsync(IsolationLevel,CancellationToken)" />.
    /// </summary>
    /// <remarks>
    ///     This method is still called if an interceptor suppressed creation in <see cref="TransactionStarting" />.
    ///     In this case, <paramref name="result" /> is the result returned by <see cref="TransactionStarting" />.
    /// </remarks>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="result">
    ///     The result of the call to
    ///     <see cref="DbConnection.BeginTransactionAsync(IsolationLevel,CancellationToken)" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A <see cref="Task" /> providing the result that EF will use.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<DbTransaction> TransactionStartedAsync(
        DbConnection connection,
        TransactionEndEventData eventData,
        DbTransaction result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <summary>
    ///     Called immediately after <see cref="O:RelationalDatabaseFacadeExtensions.UseTransaction" /> is called.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="result">
    ///     The <see cref="DbTransaction" /> that was passed to <see cref="O:RelationalDatabaseFacadeExtensions.UseTransaction" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     The value that will be used as the effective value passed to <see cref="O:RelationalDatabaseFacadeExtensions.UseTransaction" />
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in.
    /// </returns>
    DbTransaction TransactionUsed(DbConnection connection, TransactionEventData eventData, DbTransaction result)
        => result;

    /// <summary>
    ///     Called immediately after <see cref="O:RelationalDatabaseFacadeExtensions.UseTransactionAsync" /> is called.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="result">
    ///     The <see cref="DbTransaction" /> that was passed to <see cref="O:RelationalDatabaseFacadeExtensions.UseTransactionAsync" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A <see cref="Task" />  containing the value that will be used as the effective value passed
    ///     to <see cref="O:RelationalDatabaseFacadeExtensions.UseTransactionAsync" />
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<DbTransaction> TransactionUsedAsync(
        DbConnection connection,
        TransactionEventData eventData,
        DbTransaction result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <summary>
    ///     Called just before EF intends to call <see cref="DbTransaction.Commit" />.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="false" />, then EF will continue as normal.
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="true" />, then EF will suppress the operation
    ///     it was about to perform.
    ///     An implementation of this method for any interceptor that is not attempting to suppress
    ///     the operation is to return the <paramref name="result" /> value passed in.
    /// </returns>
    InterceptionResult TransactionCommitting(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result)
        => result;

    /// <summary>
    ///     Called immediately after EF calls <see cref="DbTransaction.Commit" />.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
    {
    }

    /// <summary>
    ///     Called just before EF intends to call
    ///     <see cref="DbTransaction.CommitAsync(CancellationToken)" />.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="false" />, then EF will continue as normal.
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="true" />, then EF will suppress the operation
    ///     it was about to perform.
    ///     An implementation of this method for any interceptor that is not attempting to suppress
    ///     the operation is to return the <paramref name="result" /> value passed in.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<InterceptionResult> TransactionCommittingAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <summary>
    ///     Called immediately after EF calls <see cref="DbTransaction.CommitAsync" />.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task TransactionCommittedAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    ///     Called just before EF intends to call <see cref="DbTransaction.Rollback()" />.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="false" />, then EF will continue as normal.
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="true" />, then EF will suppress the operation
    ///     it was about to perform.
    ///     An implementation of this method for any interceptor that is not attempting to suppress
    ///     the operation is to return the <paramref name="result" /> value passed in.
    /// </returns>
    InterceptionResult TransactionRollingBack(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result)
        => result;

    /// <summary>
    ///     Called immediately after EF calls <see cref="DbTransaction.Rollback()" />.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData)
    {
    }

    /// <summary>
    ///     Called just before EF intends to call
    ///     <see cref="DbTransaction.RollbackAsync(CancellationToken)" />.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="false" />, then EF will continue as normal.
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="true" />, then EF will suppress the operation
    ///     it was about to perform.
    ///     An implementation of this method for any interceptor that is not attempting to suppress
    ///     the operation is to return the <paramref name="result" /> value passed in.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<InterceptionResult> TransactionRollingBackAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <summary>
    ///     Called immediately after EF calls <see cref="DbTransaction.RollbackAsync(CancellationToken)" />.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task TransactionRolledBackAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    ///     Called just before EF intends to create a transaction savepoint.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="false" />, then EF will continue as normal.
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="true" />, then EF will suppress the operation
    ///     it was about to perform.
    ///     An implementation of this method for any interceptor that is not attempting to suppress
    ///     the operation is to return the <paramref name="result" /> value passed in.
    /// </returns>
    InterceptionResult CreatingSavepoint(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result)
        => result;

    /// <summary>
    ///     Called immediately after EF creates a transaction savepoint.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    void CreatedSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
    }

    /// <summary>
    ///     Called just before EF intends to create a transaction savepoint.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="false" />, then EF will continue as normal.
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="true" />, then EF will suppress the operation
    ///     it was about to perform.
    ///     An implementation of this method for any interceptor that is not attempting to suppress
    ///     the operation is to return the <paramref name="result" /> value passed in.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<InterceptionResult> CreatingSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <summary>
    ///     Called immediately after EF calls <see cref="DbTransaction.CommitAsync" />.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task CreatedSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    ///     Called just before EF intends to roll back to a transaction savepoint.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="false" />, then EF will continue as normal.
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="true" />, then EF will suppress the operation
    ///     it was about to perform.
    ///     An implementation of this method for any interceptor that is not attempting to suppress
    ///     the operation is to return the <paramref name="result" /> value passed in.
    /// </returns>
    InterceptionResult RollingBackToSavepoint(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result)
        => result;

    /// <summary>
    ///     Called immediately after EF rolls back to a transaction savepoint.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    void RolledBackToSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
    }

    /// <summary>
    ///     Called just before EF intends to roll back to a transaction savepoint.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="false" />, then EF will continue as normal.
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="true" />, then EF will suppress the operation
    ///     it was about to perform.
    ///     An implementation of this method for any interceptor that is not attempting to suppress
    ///     the operation is to return the <paramref name="result" /> value passed in.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<InterceptionResult> RollingBackToSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <summary>
    ///     Called immediately after EF rolls back to a transaction savepoint.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task RolledBackToSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    ///     Called just before EF intends to release a transaction savepoint.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="false" />, then EF will continue as normal.
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="true" />, then EF will suppress the operation
    ///     it was about to perform.
    ///     An implementation of this method for any interceptor that is not attempting to suppress
    ///     the operation is to return the <paramref name="result" /> value passed in.
    /// </returns>
    InterceptionResult ReleasingSavepoint(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result)
        => result;

    /// <summary>
    ///     Called immediately after EF releases a transaction savepoint.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    void ReleasedSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
    }

    /// <summary>
    ///     Called just before EF intends to release a transaction savepoint.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="false" />, then EF will continue as normal.
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="true" />, then EF will suppress the operation
    ///     it was about to perform.
    ///     An implementation of this method for any interceptor that is not attempting to suppress
    ///     the operation is to return the <paramref name="result" /> value passed in.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<InterceptionResult> ReleasingSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <summary>
    ///     Called immediately after EF releases a transaction savepoint.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task ReleasedSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    ///     Called when use of a <see cref="DbTransaction" /> has failed with an exception.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    void TransactionFailed(DbTransaction transaction, TransactionErrorEventData eventData)
    {
    }

    /// <summary>
    ///     Called when use of a <see cref="DbTransaction" /> has failed with an exception.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="eventData">Contextual information about connection and transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task TransactionFailedAsync(
        DbTransaction transaction,
        TransactionErrorEventData eventData,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
