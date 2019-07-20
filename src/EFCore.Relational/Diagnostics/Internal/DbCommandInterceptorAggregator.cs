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

            public CompositeDbCommandInterceptor([NotNull] IEnumerable<IDbCommandInterceptor> interceptors)
            {
                _interceptors = interceptors.ToArray();
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

            public async Task<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result,
                CancellationToken cancellationToken = default)
            {
                for (var i = 0; i < _interceptors.Length; i++)
                {
                    result = await _interceptors[i].ReaderExecutingAsync(command, eventData, result, cancellationToken);
                }

                return result;
            }

            public async Task<InterceptionResult<object>> ScalarExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object> result,
                CancellationToken cancellationToken = default)
            {
                for (var i = 0; i < _interceptors.Length; i++)
                {
                    result = await _interceptors[i].ScalarExecutingAsync(command, eventData, result, cancellationToken);
                }

                return result;
            }

            public async Task<InterceptionResult<int>> NonQueryExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int> result,
                CancellationToken cancellationToken = default)
            {
                for (var i = 0; i < _interceptors.Length; i++)
                {
                    result = await _interceptors[i].NonQueryExecutingAsync(command, eventData, result, cancellationToken);
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

            public object ScalarExecuted(
                DbCommand command,
                CommandExecutedEventData eventData,
                object result)
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

            public async Task<DbDataReader> ReaderExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                DbDataReader result,
                CancellationToken cancellationToken = default)
            {
                for (var i = 0; i < _interceptors.Length; i++)
                {
                    result = await _interceptors[i].ReaderExecutedAsync(command, eventData, result, cancellationToken);
                }

                return result;
            }

            public async Task<object> ScalarExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                object result,
                CancellationToken cancellationToken = default)
            {
                for (var i = 0; i < _interceptors.Length; i++)
                {
                    result = await _interceptors[i].ScalarExecutedAsync(command, eventData, result, cancellationToken);
                }

                return result;
            }

            public async Task<int> NonQueryExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                int result,
                CancellationToken cancellationToken = default)
            {
                for (var i = 0; i < _interceptors.Length; i++)
                {
                    result = await _interceptors[i].NonQueryExecutedAsync(command, eventData, result, cancellationToken);
                }

                return result;
            }

            public void CommandFailed(DbCommand command, CommandErrorEventData eventData)
            {
                for (var i = 0; i < _interceptors.Length; i++)
                {
                    _interceptors[i].CommandFailed(command, eventData);
                }
            }

            public async Task CommandFailedAsync(
                DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default)
            {
                for (var i = 0; i < _interceptors.Length; i++)
                {
                    await _interceptors[i].CommandFailedAsync(command, eventData, cancellationToken);
                }
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
}
