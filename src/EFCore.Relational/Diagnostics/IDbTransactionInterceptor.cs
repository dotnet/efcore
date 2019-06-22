// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Allows interception of operations related to a <see cref="DbTransaction"/>.
    ///     </para>
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
    /// </summary>
    public interface IDbTransactionInterceptor : IInterceptor
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
        InterceptionResult<DbTransaction>? TransactionStarting(
            [NotNull] DbConnection connection,
            [NotNull] TransactionStartingEventData eventData,
            InterceptionResult<DbTransaction>? result);

        /// <summary>
        ///     <para>
        ///         Called immediately after EF calls <see cref="DbConnection.BeginTransaction(IsolationLevel)" />.
        ///     </para>
        ///     <para>
        ///         This method is still called if an interceptor suppressed creation in <see cref="TransactionStarting" />.
        ///         In this case, <paramref name="result" /> is the result returned by <see cref="TransactionStarting" />.
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
        DbTransaction TransactionStarted(
            [NotNull] DbConnection connection,
            [NotNull] TransactionEndEventData eventData,
            [CanBeNull] DbTransaction result);

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
        Task<InterceptionResult<DbTransaction>?> TransactionStartingAsync(
            [NotNull] DbConnection connection,
            [NotNull] TransactionStartingEventData eventData,
            InterceptionResult<DbTransaction>? result,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     <para>
        ///         Called immediately after EF calls <see cref="DbConnection.BeginTransaction(IsolationLevel)" />.
        ///     </para>
        ///     <para>
        ///         This method is still called if an interceptor suppressed creation in <see cref="TransactionStarting" />.
        ///         In this case, <paramref name="result" /> is the result returned by <see cref="TransactionStarting" />.
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
        ///     A <see cref="Task"/> providing the result that EF will use.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        Task<DbTransaction> TransactionStartedAsync(
            [NotNull] DbConnection connection,
            [NotNull] TransactionEndEventData eventData,
            [CanBeNull] DbTransaction result,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     <para>
        ///         Called immediately after <see cref="RelationalDatabaseFacadeExtensions.UseTransaction" /> is called.
        ///     </para>
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="result">
        ///     The <see cref="DbTransaction"/> that was passed to <see cref="RelationalDatabaseFacadeExtensions.UseTransaction" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     The value that will be used as the effective value passed to <see cref="RelationalDatabaseFacadeExtensions.UseTransaction" />
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in.
        /// </returns>
        DbTransaction TransactionUsed(
            [NotNull] DbConnection connection,
            [NotNull] TransactionEventData eventData,
            [CanBeNull] DbTransaction result);

        /// <summary>
        ///     <para>
        ///         Called immediately after <see cref="RelationalDatabaseFacadeExtensions.UseTransactionAsync" /> is called.
        ///     </para>
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="result">
        ///     The <see cref="DbTransaction"/> that was passed to <see cref="RelationalDatabaseFacadeExtensions.UseTransactionAsync" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     A <see cref="Task"/>  containing the value that will be used as the effective value passed
        ///     to <see cref="RelationalDatabaseFacadeExtensions.UseTransactionAsync" />
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        Task<DbTransaction> TransactionUsedAsync(
            [NotNull] DbConnection connection,
            [NotNull] TransactionEventData eventData,
            [CanBeNull] DbTransaction result,
            CancellationToken cancellationToken = default);

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
        InterceptionResult? TransactionCommitting(
            [NotNull] DbTransaction transaction,
            [NotNull] TransactionEventData eventData,
            InterceptionResult? result);

        /// <summary>
        ///     Called immediately after EF calls <see cref="DbTransaction.Commit" />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        void TransactionCommitted(
            [NotNull] DbTransaction transaction,
            [NotNull] TransactionEndEventData eventData);

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
        Task<InterceptionResult?> TransactionCommittingAsync(
            [NotNull] DbTransaction transaction,
            [NotNull] TransactionEventData eventData,
            InterceptionResult? result,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Called immediately after EF calls <see cref="DbTransaction.Commit" />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task"/> representing the asynchronous operation. </returns>
        Task TransactionCommittedAsync(
            [NotNull] DbTransaction transaction,
            [NotNull] TransactionEndEventData eventData,
            CancellationToken cancellationToken = default);

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
        InterceptionResult? TransactionRollingBack(
            [NotNull] DbTransaction transaction,
            [NotNull] TransactionEventData eventData,
            InterceptionResult? result);

        /// <summary>
        ///     Called immediately after EF calls <see cref="DbTransaction.Rollback" />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        void TransactionRolledBack(
            [NotNull] DbTransaction transaction,
            [NotNull] TransactionEndEventData eventData);

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
        Task<InterceptionResult?> TransactionRollingBackAsync(
            [NotNull] DbTransaction transaction,
            [NotNull] TransactionEventData eventData,
            InterceptionResult? result,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Called immediately after EF calls <see cref="DbTransaction.Rollback" />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task"/> representing the asynchronous operation. </returns>
        Task TransactionRolledBackAsync(
            [NotNull] DbTransaction transaction,
            [NotNull] TransactionEndEventData eventData,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Called when use of a <see cref="DbTransaction"/> has failed with an exception. />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        void TransactionFailed(
            [NotNull] DbTransaction transaction,
            [NotNull] TransactionErrorEventData eventData);

        /// <summary>
        ///     Called when use of a <see cref="DbTransaction"/> has failed with an exception. />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task"/> representing the asynchronous operation. </returns>
        Task TransactionFailedAsync(
            [NotNull] DbTransaction transaction,
            [NotNull] TransactionErrorEventData eventData,
            CancellationToken cancellationToken = default);
    }
}
