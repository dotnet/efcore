// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal
{
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

            public CompositeDbConnectionInterceptor([NotNull] IEnumerable<IDbConnectionInterceptor> interceptors)
            {
                _interceptors = interceptors.ToArray();
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

            public async Task<InterceptionResult> ConnectionOpeningAsync(
                DbConnection connection,
                ConnectionEventData eventData,
                InterceptionResult result,
                CancellationToken cancellationToken = default)
            {
                for (var i = 0; i < _interceptors.Length; i++)
                {
                    result = await _interceptors[i].ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
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
                    await _interceptors[i].ConnectionOpenedAsync(connection, eventData, cancellationToken);
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

            public async Task<InterceptionResult> ConnectionClosingAsync(
                DbConnection connection,
                ConnectionEventData eventData,
                InterceptionResult result)
            {
                for (var i = 0; i < _interceptors.Length; i++)
                {
                    result = await _interceptors[i].ConnectionClosingAsync(connection, eventData, result);
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
                    await _interceptors[i].ConnectionClosedAsync(connection, eventData);
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
                    await _interceptors[i].ConnectionFailedAsync(connection, eventData, cancellationToken);
                }
            }
        }
    }
}
