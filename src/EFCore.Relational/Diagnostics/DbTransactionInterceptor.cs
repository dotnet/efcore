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
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult{DbTransaction}.HasResult"/> set to true if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult{DbTransaction}.SuppressWithResult"/>.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     If <see cref="InterceptionResult{DbTransaction}.HasResult"/> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult{DbTransaction}.HasResult"/> is true, then EF will suppress the operation it
        ///     was about to perform and use <see cref="InterceptionResult{DbTransaction}.Result"/> instead.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual InterceptionResult<DbTransaction> TransactionStarting(
            DbConnection connection,
            TransactionStartingEventData eventData,
            InterceptionResult<DbTransaction> result)
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
        ///     Called just before EF intends to call <see cref="DbConnection.BeginTransactionAsync(IsolationLevel, CancellationToken)" />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="result">
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult{DbTransaction}.HasResult"/> set to true if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult{DbTransaction}.SuppressWithResult"/>.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     If <see cref="InterceptionResult{DbTransaction}.HasResult"/> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult{DbTransaction}.HasResult"/> is true, then EF will suppress the operation it
        ///     was about to perform and use <see cref="InterceptionResult{DbTransaction}.Result"/> instead.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual Task<InterceptionResult<DbTransaction>> TransactionStartingAsync(
            DbConnection connection, TransactionStartingEventData eventData, InterceptionResult<DbTransaction> result,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);

        /// <summary>
        ///     <para>
        ///         Called immediately after EF calls <see cref="DbConnection.BeginTransactionAsync(IsolationLevel, CancellationToken)" />.
        ///     </para>
        ///     <para>
        ///         This method is still called if an interceptor suppressed creation in <see cref="IDbTransactionInterceptor.TransactionStarting" />.
        ///         In this case, <paramref name="result" /> is the result returned by <see cref="IDbTransactionInterceptor.TransactionStarting" />.
        ///     </para>
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="result">
        ///     The result of the call to <see cref="DbConnection.BeginTransactionAsync(IsolationLevel, CancellationToken)" />.
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
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult.IsSuppressed"/> set to true if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress"/>.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     If <see cref="InterceptionResult.IsSuppressed"/> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult.IsSuppressed"/> is true, then EF will suppress the operation
        ///     it was about to perform.
        ///     A normal implementation of this method for any interceptor that is not attempting to suppress
        ///     the operation is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual InterceptionResult TransactionCommitting(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result)
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
        ///     Called just before EF intends to call <see cref="DbTransaction.CommitAsync" />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="result">
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult.IsSuppressed"/> set to true if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress"/>.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     If <see cref="InterceptionResult.IsSuppressed"/> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult.IsSuppressed"/> is true, then EF will suppress the operation
        ///     it was about to perform.
        ///     A normal implementation of this method for any interceptor that is not attempting to suppress
        ///     the operation is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual Task<InterceptionResult> TransactionCommittingAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);

        /// <summary>
        ///     Called immediately after EF calls <see cref="DbTransaction.CommitAsync" />.
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
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult.IsSuppressed"/> set to true if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress"/>.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     If <see cref="InterceptionResult.IsSuppressed"/> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult.IsSuppressed"/> is true, then EF will suppress the operation
        ///     it was about to perform.
        ///     A normal implementation of this method for any interceptor that is not attempting to suppress
        ///     the operation is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual InterceptionResult TransactionRollingBack(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result)
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
        ///     Called just before EF intends to call <see cref="DbTransaction.RollbackAsync" />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="result">
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult.IsSuppressed"/> set to true if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress"/>.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     If <see cref="InterceptionResult.IsSuppressed"/> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult.IsSuppressed"/> is true, then EF will suppress the operation
        ///     it was about to perform.
        ///     A normal implementation of this method for any interceptor that is not attempting to suppress
        ///     the operation is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual Task<InterceptionResult> TransactionRollingBackAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);

        /// <summary>
        ///     Called immediately after EF calls <see cref="DbTransaction.RollbackAsync" />.
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
