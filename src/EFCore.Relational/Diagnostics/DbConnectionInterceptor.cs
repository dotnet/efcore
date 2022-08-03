// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Abstract base class for <see cref="IDbConnectionInterceptor" /> for use when implementing a subset
///     of the interface methods.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see> for more information and examples.
/// </remarks>
public abstract class DbConnectionInterceptor : IDbConnectionInterceptor
{
    /// <inheritdoc />
    public virtual InterceptionResult<DbConnection> ConnectionCreating(
        ConnectionCreatingEventData eventData,
        InterceptionResult<DbConnection> result)
        => result;

    /// <inheritdoc />
    public virtual DbConnection ConnectionCreated(ConnectionCreatedEventData eventData, DbConnection result)
        => result;

    /// <inheritdoc />
    public virtual InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        => result;

    /// <inheritdoc />
    public virtual ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <inheritdoc />
    public virtual void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
    }

    /// <inheritdoc />
    public virtual Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual InterceptionResult ConnectionClosing(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        => result;

    /// <inheritdoc />
    public virtual ValueTask<InterceptionResult> ConnectionClosingAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result)
        => new(result);

    /// <inheritdoc />
    public virtual void ConnectionClosed(DbConnection connection, ConnectionEndEventData eventData)
    {
    }

    /// <inheritdoc />
    public virtual Task ConnectionClosedAsync(DbConnection connection, ConnectionEndEventData eventData)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual InterceptionResult ConnectionDisposing(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        => result;

    /// <inheritdoc />
    public virtual ValueTask<InterceptionResult> ConnectionDisposingAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result)
        => new(result);

    /// <inheritdoc />
    public virtual void ConnectionDisposed(DbConnection connection, ConnectionEndEventData eventData)
    {
    }

    /// <inheritdoc />
    public virtual Task ConnectionDisposedAsync(DbConnection connection, ConnectionEndEventData eventData)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual void ConnectionFailed(DbConnection connection, ConnectionErrorEventData eventData)
    {
    }

    /// <inheritdoc />
    public virtual Task ConnectionFailedAsync(
        DbConnection connection,
        ConnectionErrorEventData eventData,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
