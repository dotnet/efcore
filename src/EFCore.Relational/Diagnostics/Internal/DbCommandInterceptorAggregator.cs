// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DbCommandInterceptorAggregator : InterceptorAggregator<IDbCommandInterceptor>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override IDbCommandInterceptor CreateChain(IEnumerable<IDbCommandInterceptor> interceptors)
        => new CompositeDbCommandInterceptor(interceptors);

    private sealed class CompositeDbCommandInterceptor : IDbCommandInterceptor
    {
        private readonly IDbCommandInterceptor[] _interceptors;

        public CompositeDbCommandInterceptor(IEnumerable<IDbCommandInterceptor> interceptors)
        {
            _interceptors = interceptors.ToArray();
        }

        public InterceptionResult<DbCommand> CommandCreating(
            CommandCorrelatedEventData eventData,
            InterceptionResult<DbCommand> result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].CommandCreating(eventData, result);
            }

            return result;
        }

        public DbCommand CommandCreated(
            CommandEndEventData eventData,
            DbCommand result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].CommandCreated(eventData, result);
            }

            return result;
        }

        public DbCommand CommandInitialized(
            CommandEndEventData eventData,
            DbCommand result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].CommandInitialized(eventData, result);
            }

            return result;
        }

        public InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].ReaderExecuting(command, eventData, result);
            }

            return result;
        }

        public InterceptionResult<object> ScalarExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].ScalarExecuting(command, eventData, result);
            }

            return result;
        }

        public InterceptionResult<int> NonQueryExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].NonQueryExecuting(command, eventData, result);
            }

            return result;
        }

        public async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].ReaderExecutingAsync(command, eventData, result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].ScalarExecutingAsync(command, eventData, result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].NonQueryExecutingAsync(command, eventData, result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].ReaderExecuted(command, eventData, result);
            }

            return result;
        }

        public object? ScalarExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            object? result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].ScalarExecuted(command, eventData, result);
            }

            return result;
        }

        public int NonQueryExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].NonQueryExecuted(command, eventData, result);
            }

            return result;
        }

        public async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].ReaderExecutedAsync(command, eventData, result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public async ValueTask<object?> ScalarExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            object? result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].ScalarExecutedAsync(command, eventData, result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public async ValueTask<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].NonQueryExecutedAsync(command, eventData, result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public void CommandCanceled(DbCommand command, CommandEndEventData eventData)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                _interceptors[i].CommandCanceled(command, eventData);
            }
        }

        public async Task CommandCanceledAsync(
            DbCommand command,
            CommandEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                await _interceptors[i].CommandCanceledAsync(command, eventData, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public void CommandFailed(DbCommand command, CommandErrorEventData eventData)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                _interceptors[i].CommandFailed(command, eventData);
            }
        }

        public async Task CommandFailedAsync(
            DbCommand command,
            CommandErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                await _interceptors[i].CommandFailedAsync(command, eventData, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public InterceptionResult DataReaderClosing(DbCommand command, DataReaderClosingEventData eventData, InterceptionResult result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].DataReaderClosing(command, eventData, result);
            }

            return result;
        }

        public async ValueTask<InterceptionResult> DataReaderClosingAsync(
            DbCommand command,
            DataReaderClosingEventData eventData,
            InterceptionResult result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].DataReaderClosingAsync(command, eventData, result)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public InterceptionResult DataReaderDisposing(
            DbCommand command,
            DataReaderDisposingEventData eventData,
            InterceptionResult result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].DataReaderDisposing(command, eventData, result);
            }

            return result;
        }
    }
}
