// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class AsyncQueryingEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly RelationalConnection _connection;
        private readonly CommandBuilder _commandBuilder;
        private readonly Func<DbDataReader, T> _shaper;
        private readonly ILogger _logger;

        public AsyncQueryingEnumerable(
            [NotNull] RelationalConnection connection,
            [NotNull] CommandBuilder commandBuilder,
            [NotNull] Func<DbDataReader, T> shaper,
            [NotNull] ILogger logger)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(connection, "commandBuilder");
            Check.NotNull(connection, "shaper");
            Check.NotNull(connection, "logger");

            _connection = connection;
            _commandBuilder = commandBuilder;
            _shaper = shaper;
            _logger = logger;
        }

        public virtual IAsyncEnumerator<T> GetEnumerator()
        {
            return new AsyncEnumerator(this);
        }

        private sealed class AsyncEnumerator : IAsyncEnumerator<T>
        {
            private readonly AsyncQueryingEnumerable<T> _enumerable;

            private DbCommand _command;
            private DbDataReader _reader;

            private T _current;

            private bool _disposed;

            public AsyncEnumerator(AsyncQueryingEnumerable<T> enumerable)
            {
                _enumerable = enumerable;
            }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var hasNext
                    = await (_reader == null
                        ? InitializeAndReadAsync(cancellationToken)
                        : _reader.ReadAsync(cancellationToken))
                        .WithCurrentCulture();

                if (!hasNext)
                {
                    // H.A.C.K.: Workaround https://github.com/Reactive-Extensions/Rx.NET/issues/5
                    Dispose();

                    _current = default(T);
                }
                else
                {
                    _current = _enumerable._shaper(_reader);
                }

                return hasNext;
            }

            private async Task<bool> InitializeAndReadAsync(CancellationToken cancellationToken)
            {
                await _enumerable._connection
                    .OpenAsync(cancellationToken)
                    .WithCurrentCulture();

                _command = _enumerable._commandBuilder.Build(_enumerable._connection);

                _enumerable._logger.WriteSql(_command.CommandText);

                _reader = await _command.ExecuteReaderAsync(cancellationToken).WithCurrentCulture();

                return await _reader.ReadAsync(cancellationToken).WithCurrentCulture();
            }

            public T Current
            {
                get { return _current; }
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;

                    if (_reader != null)
                    {
                        _reader.Dispose();
                    }

                    if (_command != null)
                    {
                        _command.Dispose();
                    }

                    if (_enumerable._connection != null)
                    {
                        _enumerable._connection.Close();
                    }
                }
            }
        }
    }
}
