// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Abstract base class for <see cref="IDbConnectionInterceptor" /> for use when implementing a subset
    ///         of the interface methods.
    ///     </para>
    /// </summary>
    public abstract class DbConnectionInterceptor : IDbConnectionInterceptor
    {
        /// <summary>
        ///     Called just before EF intends to call <see cref="DbConnection.Open()" />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        /// <param name="result">
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     If <see cref="InterceptionResult.IsSuppressed" /> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult.IsSuppressed" /> is true, then EF will suppress the operation
        ///     it was about to perform.
        ///     A normal implementation of this method for any interceptor that is not attempting to suppress
        ///     the operation is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual InterceptionResult ConnectionOpening(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
            => result;

        /// <summary>
        ///     Called just before EF intends to call <see cref="DbConnection.OpenAsync()" />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        /// <param name="result">
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        ///     If <see cref="InterceptionResult.IsSuppressed" /> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult.IsSuppressed" /> is true, then EF will suppress the operation
        ///     it was about to perform.
        ///     A normal implementation of this method for any interceptor that is not attempting to suppress
        ///     the operation is to return the <paramref name="result" /> value passed in.
        /// </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        public virtual ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
            => new(result);

        /// <summary>
        ///     Called just after EF has called <see cref="DbConnection.Open()" />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        public virtual void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
        }

        /// <summary>
        ///     Called just after EF has called <see cref="DbConnection.OpenAsync()" />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> A <see cref="Task" /> representing the asynchronous operation. </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        public virtual Task ConnectionOpenedAsync(
            DbConnection connection,
            ConnectionEndEventData eventData,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        /// <summary>
        ///     Called just before EF intends to call <see cref="DbConnection.Close()" />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        /// <param name="result">
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     If <see cref="InterceptionResult.IsSuppressed" /> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult.IsSuppressed" /> is true, then EF will suppress the operation
        ///     it was about to perform.
        ///     A normal implementation of this method for any interceptor that is not attempting to suppress
        ///     the operation is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual InterceptionResult ConnectionClosing(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
            => result;

        /// <summary>
        ///     Called just before EF intends to call <see cref="DbConnection.CloseAsync()" /> in an async context.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        /// <param name="result">
        ///     Represents the current result if one exists.
        ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
        ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     If <see cref="InterceptionResult.IsSuppressed" /> is false, the EF will continue as normal.
        ///     If <see cref="InterceptionResult.IsSuppressed" /> is true, then EF will suppress the operation
        ///     it was about to perform.
        ///     A normal implementation of this method for any interceptor that is not attempting to suppress
        ///     the operation is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual ValueTask<InterceptionResult> ConnectionClosingAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
            => new(result);

        /// <summary>
        ///     Called just after EF has called <see cref="DbConnection.Close()" /> in an async context.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        public virtual void ConnectionClosed(
            DbConnection connection,
            ConnectionEndEventData eventData)
        {
        }

        /// <summary>
        ///     Called just after EF has called <see cref="DbConnection.CloseAsync()" />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        /// <returns> A <see cref="Task" /> representing the asynchronous operation. </returns>
        public virtual Task ConnectionClosedAsync(
            DbConnection connection,
            ConnectionEndEventData eventData)
            => Task.CompletedTask;

        /// <summary>
        ///     Called when opening of a connection has failed with an exception.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        public virtual void ConnectionFailed(
            DbConnection connection,
            ConnectionErrorEventData eventData)
        {
        }

        /// <summary>
        ///     Called when opening of a connection has failed with an exception.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> A <see cref="Task" /> representing the asynchronous operation. </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        public virtual Task ConnectionFailedAsync(
            DbConnection connection,
            ConnectionErrorEventData eventData,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
