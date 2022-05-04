// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     An <see cref="IDiagnosticsLogger{ConnectionCategory}" /> with some extra functionality suited for high-performance logging.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public interface IRelationalConnectionDiagnosticsLogger : IDiagnosticsLogger<DbLoggerCategory.Database.Connection>
{
    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ConnectionCreating" /> event.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <returns>The result of execution, which may have been modified by an interceptor.</returns>
    InterceptionResult<DbConnection> ConnectionCreating(
        IRelationalConnection connection,
        DateTimeOffset startTime);

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ConnectionCreated" /> event.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="duration">The amount of time it took to create the connection.</param>
    DbConnection ConnectionCreated(
        IRelationalConnection connection,
        DateTimeOffset startTime,
        TimeSpan duration);

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ConnectionDisposing" /> event.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <returns>The result of execution, which may have been modified by an interceptor.</returns>
    InterceptionResult ConnectionDisposing(
        IRelationalConnection connection,
        DateTimeOffset startTime);

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ConnectionDisposing" /> event.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    ValueTask<InterceptionResult> ConnectionDisposingAsync(
        IRelationalConnection connection,
        DateTimeOffset startTime);

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ConnectionDisposed" /> event.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="duration">The amount of time it took to dispose the connection.</param>
    void ConnectionDisposed(
        IRelationalConnection connection,
        DateTimeOffset startTime,
        TimeSpan duration);

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ConnectionDisposed" /> event.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="duration">The amount of time it took to dispose the connection.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    Task ConnectionDisposedAsync(
        IRelationalConnection connection,
        DateTimeOffset startTime,
        TimeSpan duration);

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ConnectionOpening" /> event.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <returns>The result of execution, which may have been modified by an interceptor.</returns>
    InterceptionResult ConnectionOpening(
        IRelationalConnection connection,
        DateTimeOffset startTime);

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ConnectionOpening" /> event.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    ValueTask<InterceptionResult> ConnectionOpeningAsync(
        IRelationalConnection connection,
        DateTimeOffset startTime,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ConnectionOpened" /> event.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="duration">The amount of time before the connection was opened.</param>
    void ConnectionOpened(
        IRelationalConnection connection,
        DateTimeOffset startTime,
        TimeSpan duration);

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ConnectionOpened" /> event.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="duration">The amount of time before the connection was opened.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task ConnectionOpenedAsync(
        IRelationalConnection connection,
        DateTimeOffset startTime,
        TimeSpan duration,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ConnectionClosing" /> event.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <returns>The result of execution, which may have been modified by an interceptor.</returns>
    InterceptionResult ConnectionClosing(
        IRelationalConnection connection,
        DateTimeOffset startTime);

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ConnectionClosing" /> event.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    ValueTask<InterceptionResult> ConnectionClosingAsync(
        IRelationalConnection connection,
        DateTimeOffset startTime);

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ConnectionClosed" /> event.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="duration">The amount of time before the connection was closed.</param>
    void ConnectionClosed(
        IRelationalConnection connection,
        DateTimeOffset startTime,
        TimeSpan duration);

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ConnectionClosed" /> event.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="duration">The amount of time before the connection was closed.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    Task ConnectionClosedAsync(
        IRelationalConnection connection,
        DateTimeOffset startTime,
        TimeSpan duration);

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ConnectionError" /> event.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="exception">The exception representing the error.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="duration">The elapsed time before the operation failed.</param>
    /// <param name="logErrorAsDebug">A flag indicating the exception is being handled and so it should be logged at Debug level.</param>
    void ConnectionError(
        IRelationalConnection connection,
        Exception exception,
        DateTimeOffset startTime,
        TimeSpan duration,
        bool logErrorAsDebug);

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ConnectionError" /> event.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="exception">The exception representing the error.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="duration">The elapsed time before the operation failed.</param>
    /// <param name="logErrorAsDebug">A flag indicating the exception is being handled and so it should be logged at Debug level.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task ConnectionErrorAsync(
        IRelationalConnection connection,
        Exception exception,
        DateTimeOffset startTime,
        TimeSpan duration,
        bool logErrorAsDebug,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Whether <see cref="RelationalEventId.ConnectionCreating" /> or <see cref="RelationalEventId.ConnectionCreated" /> need
    ///     to be logged.
    /// </summary>
    bool ShouldLogConnectionCreate(DateTimeOffset now);

    /// <summary>
    ///     Whether <see cref="RelationalEventId.ConnectionDisposing" /> or <see cref="RelationalEventId.ConnectionDisposed" /> need
    ///     to be logged.
    /// </summary>
    bool ShouldLogConnectionDispose(DateTimeOffset now);

    /// <summary>
    ///     Whether <see cref="RelationalEventId.ConnectionOpening" /> or <see cref="RelationalEventId.ConnectionOpened" /> need
    ///     to be logged.
    /// </summary>
    bool ShouldLogConnectionOpen(DateTimeOffset now);

    /// <summary>
    ///     Whether <see cref="RelationalEventId.ConnectionClosing" /> or <see cref="RelationalEventId.ConnectionClosed" /> need
    ///     to be logged.
    /// </summary>
    bool ShouldLogConnectionClose(DateTimeOffset now);
}
