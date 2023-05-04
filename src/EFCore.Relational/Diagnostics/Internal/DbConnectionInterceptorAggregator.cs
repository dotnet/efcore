// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DbConnectionInterceptorAggregator : InterceptorAggregator<IDbConnectionInterceptor>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override IDbConnectionInterceptor CreateChain(IEnumerable<IDbConnectionInterceptor> interceptors)
        => new CompositeDbConnectionInterceptor(interceptors);

    private sealed class CompositeDbConnectionInterceptor : IDbConnectionInterceptor
    {
        private readonly IDbConnectionInterceptor[] _interceptors;

        public CompositeDbConnectionInterceptor(IEnumerable<IDbConnectionInterceptor> interceptors)
        {
            _interceptors = interceptors.ToArray();
        }

        public InterceptionResult<DbConnection> ConnectionCreating(
            ConnectionCreatingEventData eventData,
            InterceptionResult<DbConnection> result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].ConnectionCreating(eventData, result);
            }

            return result;
        }

        public DbConnection ConnectionCreated(
            ConnectionCreatedEventData eventData,
            DbConnection result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].ConnectionCreated(eventData, result);
            }

            return result;
        }

        public InterceptionResult ConnectionOpening(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].ConnectionOpening(connection, eventData, result);
            }

            return result;
        }

        public async ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].ConnectionOpeningAsync(connection, eventData, result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public void ConnectionOpened(
            DbConnection connection,
            ConnectionEndEventData eventData)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                _interceptors[i].ConnectionOpened(connection, eventData);
            }
        }

        public async Task ConnectionOpenedAsync(
            DbConnection connection,
            ConnectionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                await _interceptors[i].ConnectionOpenedAsync(connection, eventData, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public InterceptionResult ConnectionClosing(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].ConnectionClosing(connection, eventData, result);
            }

            return result;
        }

        public async ValueTask<InterceptionResult> ConnectionClosingAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].ConnectionClosingAsync(connection, eventData, result)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public void ConnectionClosed(
            DbConnection connection,
            ConnectionEndEventData eventData)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                _interceptors[i].ConnectionClosed(connection, eventData);
            }
        }

        public async Task ConnectionClosedAsync(
            DbConnection connection,
            ConnectionEndEventData eventData)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                await _interceptors[i].ConnectionClosedAsync(connection, eventData)
                    .ConfigureAwait(false);
            }
        }

        public InterceptionResult ConnectionDisposing(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = _interceptors[i].ConnectionDisposing(connection, eventData, result);
            }

            return result;
        }

        public async ValueTask<InterceptionResult> ConnectionDisposingAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                result = await _interceptors[i].ConnectionDisposingAsync(connection, eventData, result)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public void ConnectionDisposed(
            DbConnection connection,
            ConnectionEndEventData eventData)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                _interceptors[i].ConnectionDisposed(connection, eventData);
            }
        }

        public async Task ConnectionDisposedAsync(
            DbConnection connection,
            ConnectionEndEventData eventData)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                await _interceptors[i].ConnectionDisposedAsync(connection, eventData)
                    .ConfigureAwait(false);
            }
        }

        public void ConnectionFailed(
            DbConnection connection,
            ConnectionErrorEventData eventData)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                _interceptors[i].ConnectionFailed(connection, eventData);
            }
        }

        public async Task ConnectionFailedAsync(
            DbConnection connection,
            ConnectionErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < _interceptors.Length; i++)
            {
                await _interceptors[i].ConnectionFailedAsync(connection, eventData, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
