// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Allows interception of operations on <see cref="DbConnection"/>.
    ///     </para>
    ///     <para>
    ///         Connection interceptors can be used to view, change, or suppress the operation on <see cref="DbConnection" />, and
    ///         to modify the result before it is returned to EF.
    ///     </para>
    ///     <para>
    ///         Consider inheriting from <see cref="DbConnectionInterceptor" /> if not implementing all methods.
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
    public interface IDbConnectionInterceptor : IInterceptor
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
        InterceptionResult? ConnectionOpening(
            [NotNull] DbConnection connection,
            [NotNull] ConnectionEventData eventData,
            InterceptionResult? result);

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
        Task<InterceptionResult?> ConnectionOpeningAsync(
            [NotNull] DbConnection connection,
            [NotNull] ConnectionEventData eventData,
            InterceptionResult? result,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Called just after EF has called <see cref="DbConnection.Open()" />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        void ConnectionOpened(
            [NotNull] DbConnection connection,
            [NotNull] ConnectionEndEventData eventData);

        /// <summary>
        ///     Called just after EF has called <see cref="DbConnection.OpenAsync()" />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task"/> representing the asynchronous operation. </returns>
        Task ConnectionOpenedAsync(
            [NotNull] DbConnection connection,
            [NotNull] ConnectionEndEventData eventData,
            CancellationToken cancellationToken = default);

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
        InterceptionResult? ConnectionClosing(
            [NotNull] DbConnection connection,
            [NotNull] ConnectionEventData eventData,
            InterceptionResult? result);

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
        Task<InterceptionResult?> ConnectionClosingAsync(
            [NotNull] DbConnection connection,
            [NotNull] ConnectionEventData eventData,
            InterceptionResult? result,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Called just after EF has called <see cref="DbConnection.Close()" /> in an async context.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        void ConnectionClosed(
            [NotNull] DbConnection connection,
            [NotNull] ConnectionEndEventData eventData);

        /// <summary>
        ///     Called just after EF has called <see cref="DbConnection.Close()" />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task"/> representing the asynchronous operation. </returns>
        Task ConnectionClosedAsync(
            [NotNull] DbConnection connection,
            [NotNull] ConnectionEndEventData eventData,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Called when closing of a connection has failed with an exception. />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        void ConnectionFailed(
            [NotNull] DbConnection connection,
            [NotNull] ConnectionErrorEventData eventData);

        /// <summary>
        ///     Called when closing of a connection has failed with an exception. />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task"/> representing the asynchronous operation. </returns>
        Task ConnectionFailedAsync(
            [NotNull] DbConnection connection,
            [NotNull] ConnectionErrorEventData eventData,
            CancellationToken cancellationToken = default);
    }
}
