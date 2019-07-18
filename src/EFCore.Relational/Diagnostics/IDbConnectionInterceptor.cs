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
        InterceptionResult ConnectionOpening(
            [NotNull] DbConnection connection,
            [NotNull] ConnectionEventData eventData,
            InterceptionResult result);

        /// <summary>
        ///     Called just before EF intends to call <see cref="DbConnection.OpenAsync()" />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
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
        Task<InterceptionResult> ConnectionOpeningAsync(
            [NotNull] DbConnection connection,
            [NotNull] ConnectionEventData eventData,
            InterceptionResult result,
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
        ///     Called just before EF intends to call <see cref="DbConnection.CloseAsync()" />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
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
        InterceptionResult ConnectionClosing(
            [NotNull] DbConnection connection,
            [NotNull] ConnectionEventData eventData,
            InterceptionResult result);

        /// <summary>
        ///     Called just before EF intends to call <see cref="DbConnection.Close()" /> in an async context.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
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
        Task<InterceptionResult> ConnectionClosingAsync(
            [NotNull] DbConnection connection,
            [NotNull] ConnectionEventData eventData,
            InterceptionResult result);

        /// <summary>
        ///     Called just after EF has called <see cref="DbConnection.Close()" /> in an async context.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        void ConnectionClosed(
            [NotNull] DbConnection connection,
            [NotNull] ConnectionEndEventData eventData);

        /// <summary>
        ///     Called just after EF has called <see cref="DbConnection.CloseAsync()" />.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="eventData"> Contextual information about the connection. </param>
        /// <returns> A <see cref="Task"/> representing the asynchronous operation. </returns>
        Task ConnectionClosedAsync(
            [NotNull] DbConnection connection,
            [NotNull] ConnectionEndEventData eventData);

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
