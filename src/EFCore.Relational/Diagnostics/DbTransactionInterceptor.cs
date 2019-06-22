// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Abstract base class for <see cref="IDbTransactionInterceptor" /> for use when implementing a subset
    ///         of the interface methods.
    ///     </para>
    /// </summary>
    public abstract class DbTransactionInterceptor : IDbTransactionInterceptor
    {
        /// <summary>
        ///     Called just before EF intends to call <see cref="DbConnection.BeginTransaction(IsolationLevel)" />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="result">
        ///     The current result, or null if no result yet exists.
        ///     This value will be non-null if some previous interceptor suppressed creation by returning a result from
        ///     its implementation of this method.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     If null, then EF will start the transaction as normal.
        ///     If non-null, then transaction creation is suppressed and the value contained in
        ///     the <see cref="InterceptionResult{TResult}" /> we be used by EF instead.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual InterceptionResult<DbTransaction>? TransactionStarting(
            DbConnection connection,
            TransactionStartingEventData eventData,
            InterceptionResult<DbTransaction>? result)
            => result;

        /// <summary>
        ///     <para>
        ///         Called immediately after EF calls <see cref="DbConnection.BeginTransaction(IsolationLevel)" />.
        ///     </para>
        ///     <para>
        ///         This method is still called if an interceptor suppressed creation in <see cref="IDbTransactionInterceptor.TransactionStarting" />.
        ///         In this case, <paramref name="result" /> is the result returned by <see cref="IDbTransactionInterceptor.TransactionStarting" />.
        ///     </para>
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="result">
        ///     The result of the call to <see cref="DbConnection.BeginTransaction(IsolationLevel)" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     The result that EF will use.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual DbTransaction TransactionStarted(
            DbConnection connection,
            TransactionEndEventData eventData,
            DbTransaction result)
            => result;

        /// <summary>
        ///     Called just before EF intends to call <see cref="DbConnection.BeginTransaction(IsolationLevel)" />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="result">
        ///     The current result, or null if no result yet exists.
        ///     This value will be non-null if some previous interceptor suppressed creation by returning a result from
        ///     its implementation of this method.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     If the <see cref="Task" /> result is null, then EF will start the transaction as normal.
        ///     If the <see cref="Task" /> result is non-null value, then transaction creation is suppressed and the value contained in
        ///     the <see cref="InterceptionResult{TResult}" /> we be used by EF instead.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual Task<InterceptionResult<DbTransaction>?> TransactionStartingAsync(
            DbConnection connection, TransactionStartingEventData eventData, InterceptionResult<DbTransaction>? result,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);

        /// <summary>
        ///     <para>
        ///         Called immediately after EF calls <see cref="DbConnection.BeginTransaction(IsolationLevel)" />.
        ///     </para>
        ///     <para>
        ///         This method is still called if an interceptor suppressed creation in <see cref="IDbTransactionInterceptor.TransactionStarting" />.
        ///         In this case, <paramref name="result" /> is the result returned by <see cref="IDbTransactionInterceptor.TransactionStarting" />.
        ///     </para>
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="result">
        ///     The result of the call to <see cref="DbConnection.BeginTransaction(IsolationLevel)" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     A <see cref="Task" /> providing the result that EF will use.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual Task<DbTransaction> TransactionStartedAsync(
            DbConnection connection, TransactionEndEventData eventData, DbTransaction result, CancellationToken cancellationToken = default)
            => Task.FromResult(result);

        /// <summary>
        ///     <para>
        ///         Called immediately after <see cref="RelationalDatabaseFacadeExtensions.UseTransaction" /> is called.
        ///     </para>
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="result">
        ///     The <see cref="DbTransaction" /> that was passed to <see cref="RelationalDatabaseFacadeExtensions.UseTransaction" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     The value that will be used as the effective value passed to <see cref="RelationalDatabaseFacadeExtensions.UseTransaction" />
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual DbTransaction TransactionUsed(
            DbConnection connection,
            TransactionEventData eventData,
            DbTransaction result)
            => result;

        /// <summary>
        ///     <para>
        ///         Called immediately after <see cref="RelationalDatabaseFacadeExtensions.UseTransactionAsync" /> is called.
        ///     </para>
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="result">
        ///     The <see cref="DbTransaction" /> that was passed to <see cref="RelationalDatabaseFacadeExtensions.UseTransactionAsync" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     A <see cref="Task" />  containing the value that will be used as the effective value passed
        ///     to <see cref="RelationalDatabaseFacadeExtensions.UseTransactionAsync" />
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual Task<DbTransaction> TransactionUsedAsync(
            DbConnection connection,
            TransactionEventData eventData,
            DbTransaction result,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);

        /// <summary>
        ///     Called just before EF intends to call <see cref="DbTransaction.Commit" />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="result">
        ///     The current result, or null if no result yet exists.
        ///     This value will be non-null if some previous interceptor suppressed committing by returning a result from
        ///     its implementation of this method.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     If null, then EF will committing the transaction as normal.
        ///     If non-null, then committing the transaction is suppressed.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual InterceptionResult? TransactionCommitting(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult? result)
            => result;

        /// <summary>
        ///     Called immediately after EF calls <see cref="DbTransaction.Commit" />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        public virtual void TransactionCommitted(
            DbTransaction transaction,
            TransactionEndEventData eventData)
        {
        }

        /// <summary>
        ///     Called just before EF intends to call <see cref="DbTransaction.Commit" />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="result">
        ///     The current result, or null if no result yet exists.
        ///     This value will be non-null if some previous interceptor suppressed committing returning a result from
        ///     its implementation of this method.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     If the <see cref="Task" /> result is null, then EF will commit the transaction as normal.
        ///     If the <see cref="Task" /> result is non-null value, committing the transaction is suppressed.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual Task<InterceptionResult?> TransactionCommittingAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult? result,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);

        /// <summary>
        ///     Called immediately after EF calls <see cref="DbTransaction.Commit" />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the asynchronous operation. </returns>
        public virtual Task TransactionCommittedAsync(
            DbTransaction transaction,
            TransactionEndEventData eventData,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        /// <summary>
        ///     Called just before EF intends to call <see cref="DbTransaction.Rollback" />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="result">
        ///     The current result, or null if no result yet exists.
        ///     This value will be non-null if some previous interceptor suppressed rolling back by returning a result from
        ///     its implementation of this method.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     If null, then EF will roll back the transaction as normal.
        ///     If non-null, then rolling back the transaction is suppressed.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual InterceptionResult? TransactionRollingBack(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult? result)
            => result;

        /// <summary>
        ///     Called immediately after EF calls <see cref="DbTransaction.Rollback" />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        public virtual void TransactionRolledBack(
            DbTransaction transaction,
            TransactionEndEventData eventData)
        {
        }

        /// <summary>
        ///     Called just before EF intends to call <see cref="DbTransaction.Rollback" />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="result">
        ///     The current result, or null if no result yet exists.
        ///     This value will be non-null if some previous interceptor suppressed rolling back returning a result from
        ///     its implementation of this method.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     If the <see cref="Task" /> result is null, then EF will roll back the transaction as normal.
        ///     If the <see cref="Task" /> result is non-null value, rolling back the transaction is suppressed.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual Task<InterceptionResult?> TransactionRollingBackAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult? result,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);

        /// <summary>
        ///     Called immediately after EF calls <see cref="DbTransaction.Rollback" />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the asynchronous operation. </returns>
        public virtual Task TransactionRolledBackAsync(
            DbTransaction transaction,
            TransactionEndEventData eventData,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        /// <summary>
        ///     Called when use of a <see cref="DbTransaction"/> has failed with an exception. />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        public virtual void TransactionFailed(
            DbTransaction transaction,
            TransactionErrorEventData eventData)
        {
        }

        /// <summary>
        ///     Called when use of a <see cref="DbTransaction"/> has failed with an exception. />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task"/> representing the asynchronous operation. </returns>
        public virtual Task TransactionFailedAsync(
            DbTransaction transaction,
            TransactionErrorEventData eventData,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
