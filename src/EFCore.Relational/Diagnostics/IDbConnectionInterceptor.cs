// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Allows interception of operations on <see cref="DbConnection" />.
/// </summary>
/// <remarks>
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
///     <para>
///         See <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see> for more information and examples.
///     </para>
/// </remarks>
public interface IDbConnectionInterceptor : IInterceptor
{
    /// <summary>
    ///     Called just before EF creates a <see cref="DbConnection" />. This event is not triggered if the application provides the
    ///     connection to use.
    /// </summary>
    /// <param name="eventData">Contextual information about the connection.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult{DbConnection}.HasResult" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult{DbConnection}.SuppressWithResult" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult{DbConnection}.HasResult" /> is <see langword="false" />, then EF will continue as normal.
    ///     If <see cref="InterceptionResult{DbConnection}.HasResult" /> is <see langword="true" />, then EF will suppress the operation it
    ///     was about to perform and use <see cref="InterceptionResult{DbConnection}.Result" /> instead.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     should return the <paramref name="result" /> value passed in.
    /// </returns>
    InterceptionResult<DbConnection> ConnectionCreating(ConnectionCreatingEventData eventData, InterceptionResult<DbConnection> result)
        => result;

    /// <summary>
    ///     Called just after EF creates a <see cref="DbConnection" />. This event is not triggered if the application provides the
    ///     connection to use.
    /// </summary>
    /// <param name="eventData">Contextual information about the connection.</param>
    /// <param name="result">
    ///     The connection that has been created.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     The result that EF will use.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     is to return the <paramref name="result" /> value passed in.
    /// </returns>
    DbConnection ConnectionCreated(ConnectionCreatedEventData eventData, DbConnection result)
        => result;

    /// <summary>
    ///     Called just before EF intends to call <see cref="DbConnection.Open()" />.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about the connection.</param>
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
    InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        => result;

    /// <summary>
    ///     Called just before EF intends to call <see cref="DbConnection.OpenAsync()" />.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about the connection.</param>
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
    ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <summary>
    ///     Called just after EF has called <see cref="DbConnection.Open()" />.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about the connection.</param>
    void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
    }

    /// <summary>
    ///     Called just after EF has called <see cref="DbConnection.OpenAsync()" />.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about the connection.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    ///     Called just before EF intends to call <see cref="DbConnection.Close()" />.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about the connection.</param>
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
    InterceptionResult ConnectionClosing(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        => result;

    /// <summary>
    ///     Called just before EF intends to call <see cref="DbConnection.CloseAsync()" /> in an async context.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about the connection.</param>
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
    ValueTask<InterceptionResult> ConnectionClosingAsync(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        => new(result);

    /// <summary>
    ///     Called just after EF has called <see cref="DbConnection.Close()" /> in an async context.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about the connection.</param>
    void ConnectionClosed(DbConnection connection, ConnectionEndEventData eventData)
    {
    }

    /// <summary>
    ///     Called just after EF has called <see cref="DbConnection.CloseAsync()" />.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about the connection.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task ConnectionClosedAsync(DbConnection connection, ConnectionEndEventData eventData)
        => Task.CompletedTask;

    /// <summary>
    ///     Called just before EF intends to call <see cref="Component.Dispose()" /> for the <see cref="DbConnection" />.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about the connection.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="false" />, the EF will continue as normal.
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="true" />, then EF will suppress the operation
    ///     it was about to perform.
    ///     An implementation of this method for any interceptor that is not attempting to suppress
    ///     the operation is to return the <paramref name="result" /> value passed in.
    /// </returns>
    InterceptionResult ConnectionDisposing(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        => result;

    /// <summary>
    ///     Called just before EF intends to call <see cref="DbConnection.DisposeAsync()" /> in an async context.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about the connection.</param>
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
    ValueTask<InterceptionResult> ConnectionDisposingAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result)
        => new(result);

    /// <summary>
    ///     Called just after EF has called <see cref="Component.Dispose()" /> in an async context.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about the connection.</param>
    void ConnectionDisposed(DbConnection connection, ConnectionEndEventData eventData)
    {
    }

    /// <summary>
    ///     Called just after EF has called <see cref="DbConnection.DisposeAsync()" />.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about the connection.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task ConnectionDisposedAsync(DbConnection connection, ConnectionEndEventData eventData)
        => Task.CompletedTask;

    /// <summary>
    ///     Called when closing of a connection has failed with an exception.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about the connection.</param>
    void ConnectionFailed(DbConnection connection, ConnectionErrorEventData eventData)
    {
    }

    /// <summary>
    ///     Called when closing of a connection has failed with an exception.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="eventData">Contextual information about the connection.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task ConnectionFailedAsync(DbConnection connection, ConnectionErrorEventData eventData, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
