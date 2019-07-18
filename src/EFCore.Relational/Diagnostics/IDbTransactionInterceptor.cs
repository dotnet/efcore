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
        InterceptionResult<DbTransaction> TransactionStarting(
            [NotNull] DbConnection connection,
            [NotNull] TransactionStartingEventData eventData,
            InterceptionResult<DbTransaction> result);

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
        Task<InterceptionResult<DbTransaction>> TransactionStartingAsync(
            [NotNull] DbConnection connection,
            [NotNull] TransactionStartingEventData eventData,
            InterceptionResult<DbTransaction> result,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     <para>
        ///         Called immediately after EF calls <see cref="DbConnection.BeginTransactionAsync(IsolationLevel, CancellationToken)" />.
        ///     </para>
        ///     <para>
        ///         This method is still called if an interceptor suppressed creation in <see cref="TransactionStarting" />.
        ///         In this case, <paramref name="result" /> is the result returned by <see cref="TransactionStarting" />.
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
        InterceptionResult TransactionCommitting(
            [NotNull] DbTransaction transaction,
            [NotNull] TransactionEventData eventData,
            InterceptionResult result);

        /// <summary>
        ///     Called immediately after EF calls <see cref="DbTransaction.Commit" />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        void TransactionCommitted(
            [NotNull] DbTransaction transaction,
            [NotNull] TransactionEndEventData eventData);

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
        Task<InterceptionResult> TransactionCommittingAsync(
            [NotNull] DbTransaction transaction,
            [NotNull] TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Called immediately after EF calls <see cref="DbTransaction.CommitAsync" />.
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
        InterceptionResult TransactionRollingBack(
            [NotNull] DbTransaction transaction,
            [NotNull] TransactionEventData eventData,
            InterceptionResult result);

        /// <summary>
        ///     Called immediately after EF calls <see cref="DbTransaction.Rollback" />.
        /// </summary>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="eventData"> Contextual information about connection and transaction. </param>
        void TransactionRolledBack(
            [NotNull] DbTransaction transaction,
            [NotNull] TransactionEndEventData eventData);

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
        Task<InterceptionResult> TransactionRollingBackAsync(
            [NotNull] DbTransaction transaction,
            [NotNull] TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Called immediately after EF calls <see cref="DbTransaction.RollbackAsync" />.
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
