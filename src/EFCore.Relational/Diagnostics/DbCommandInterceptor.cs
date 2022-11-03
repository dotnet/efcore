// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Abstract base class for <see cref="IDbCommandInterceptor" /> for use when implementing a subset
///     of the interface methods.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see> for more information and examples.
/// </remarks>
public abstract class DbCommandInterceptor : IDbCommandInterceptor
{
    /// <inheritdoc />
    public virtual InterceptionResult<DbCommand> CommandCreating(CommandCorrelatedEventData eventData, InterceptionResult<DbCommand> result)
        => result;

    /// <inheritdoc />
    public virtual DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result)
        => result;

    /// <inheritdoc />
    public virtual DbCommand CommandInitialized(CommandEndEventData eventData, DbCommand result)
        => result;

    /// <inheritdoc />
    public virtual InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
        => result;

    /// <inheritdoc />
    public virtual InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
        => result;

    /// <inheritdoc />
    public virtual InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        => result;

    /// <inheritdoc />
    public virtual ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <inheritdoc />
    public virtual ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <inheritdoc />
    public virtual ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <inheritdoc />
    public virtual DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
        => result;

    /// <inheritdoc />
    public virtual object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
        => result;

    /// <inheritdoc />
    public virtual int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
        => result;

    /// <inheritdoc />
    public virtual ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <inheritdoc />
    public virtual ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <inheritdoc />
    public virtual ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
        => new(result);

    /// <inheritdoc />
    public virtual void CommandCanceled(DbCommand command, CommandEndEventData eventData)
    {
    }

    /// <inheritdoc />
    public virtual Task CommandCanceledAsync(
        DbCommand command,
        CommandEndEventData eventData,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
    }

    /// <inheritdoc />
    public virtual Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual InterceptionResult DataReaderClosing(DbCommand command, DataReaderClosingEventData eventData, InterceptionResult result)
        => result;

    /// <inheritdoc />
    public virtual ValueTask<InterceptionResult> DataReaderClosingAsync(
        DbCommand command,
        DataReaderClosingEventData eventData,
        InterceptionResult result)
        => new(result);

    /// <inheritdoc />
    public virtual InterceptionResult DataReaderDisposing(
        DbCommand command,
        DataReaderDisposingEventData eventData,
        InterceptionResult result)
        => result;
}
