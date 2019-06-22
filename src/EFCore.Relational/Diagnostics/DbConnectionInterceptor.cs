// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        ///     The current result, or null if no result yet exists.
        ///     This value will be non-null if some previous interceptor suppressed execution by returning a result from
        ///     its implementation of this method.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     If null, then EF will open the connection as normal.
        ///     If non-null, then connection opening is suppressed.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual InterceptionResult? ConnectionOpening(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult? result)
            => result;

        /// <summary>
        ///     Called just before EF intends to call <see cref="DbConnection.OpenAsync()" />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        /// <param name="result">
        ///     The current result, or null if no result yet exists.
        ///     This value will be non-null if some previous interceptor suppressed execution by returning a result from
        ///     its implementation of this method.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     If the <see cref="Task" /> result is null, then EF will open the connection as normal.
        ///     If the <see cref="Task" /> result is non-null value, then connection opening is suppressed.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual Task<InterceptionResult?> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult? result,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);

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
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task"/> representing the asynchronous operation. </returns>
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
        ///     The current result, or null if no result yet exists.
        ///     This value will be non-null if some previous interceptor suppressed execution by returning a result from
        ///     its implementation of this method.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <returns>
        ///     If null, then EF will close the connection as normal.
        ///     If non-null, then connection closing is suppressed.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in.
        /// </returns>
        public virtual InterceptionResult? ConnectionClosing(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult? result)
            => result;

        /// <summary>
        ///     Called just before EF intends to call <see cref="DbConnection.Close()" /> in an async context.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        /// <param name="result">
        ///     The current result, or null if no result yet exists.
        ///     This value will be non-null if some previous interceptor suppressed execution by returning a result from
        ///     its implementation of this method.
        ///     This value is typically used as the return value for the implementation of this method.
        /// </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     If the <see cref="Task" /> result is null, then EF will close the connection as normal.
        ///     If the <see cref="Task" /> result is non-null value, then connection closing is suppressed.
        ///     A normal implementation of this method for any interceptor that is not attempting to change the result
        ///     is to return the <paramref name="result" /> value passed in, often using <see cref="Task.FromResult{TResult}" />
        /// </returns>
        public virtual Task<InterceptionResult?> ConnectionClosingAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult? result,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);

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
        ///     Called just after EF has called <see cref="DbConnection.Close()" />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task"/> representing the asynchronous operation. </returns>
        public virtual Task ConnectionClosedAsync(
            DbConnection connection,
            ConnectionEndEventData eventData,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        /// <summary>
        ///     Called when opening of a connection has failed with an exception. />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        public virtual void ConnectionFailed(
            DbConnection connection,
            ConnectionErrorEventData eventData)
        {
        }

        /// <summary>
        ///     Called when opening of a connection has failed with an exception. />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task"/> representing the asynchronous operation. </returns>
        public virtual Task ConnectionFailedAsync(
            DbConnection connection,
            ConnectionErrorEventData eventData,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
