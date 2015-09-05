// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class AsyncQueryingEnumerable : IAsyncEnumerable<ValueBuffer>
    {
        private readonly RelationalQueryContext _relationalQueryContext;
        private readonly CommandBuilder _commandBuilder;
        private readonly int? _queryIndex;
        private readonly ILogger _logger;

        public AsyncQueryingEnumerable(
            [NotNull] RelationalQueryContext relationalQueryContext,
            [NotNull] CommandBuilder commandBuilder,
            int? queryIndex,
            [NotNull] ILogger logger)
        {
            _relationalQueryContext = relationalQueryContext;
            _commandBuilder = commandBuilder;
            _queryIndex = queryIndex;
            _logger = logger;
        }

        public virtual IAsyncEnumerator<ValueBuffer> GetEnumerator() => new AsyncEnumerator(this);

        private sealed class AsyncEnumerator : IAsyncEnumerator<ValueBuffer>, IValueBufferCursor
        {
            private readonly AsyncQueryingEnumerable _queryingEnumerable;

            private DbDataReader _dataReader;
            private Queue<ValueBuffer> _buffer;

            private bool _disposed;

            public AsyncEnumerator(AsyncQueryingEnumerable queryingEnumerable)
            {
                _queryingEnumerable = queryingEnumerable;
            }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                Debug.Assert(!_disposed);

                cancellationToken.ThrowIfCancellationRequested();

                if (_buffer == null)
                {
                    if (_dataReader == null)
                    {
                        await _queryingEnumerable._relationalQueryContext.Connection
                            .OpenAsync(cancellationToken);

                        using (var command
                            = _queryingEnumerable._commandBuilder
                                .Build(
                                    _queryingEnumerable._relationalQueryContext.Connection,
                                    _queryingEnumerable._relationalQueryContext.ParameterValues))
                        {
                            _queryingEnumerable._logger.LogCommand(command);

                            await _queryingEnumerable._relationalQueryContext
                                .RegisterValueBufferCursorAsync(this, _queryingEnumerable._queryIndex, cancellationToken);

                            _dataReader = await command.ExecuteReaderAsync(cancellationToken);

                            _queryingEnumerable._commandBuilder.NotifyReaderCreated(_dataReader);
                        }
                    }

                    var hasNext = await _dataReader.ReadAsync(cancellationToken);

                    Current
                        = hasNext
                            ? _queryingEnumerable._commandBuilder.ValueBufferFactory
                                .Create(_dataReader)
                            : default(ValueBuffer);

                    return hasNext;
                }

                if (_buffer.Count > 0)
                {
                    Current = _buffer.Dequeue();

                    return true;
                }

                return false;
            }

            public ValueBuffer Current { get; private set; }

            public async Task BufferAllAsync(CancellationToken cancellationToken)
            {
                if (_buffer == null)
                {
                    _buffer = new Queue<ValueBuffer>();

                    using (_dataReader)
                    {
                        while (await _dataReader.ReadAsync(cancellationToken))
                        {
                            _buffer.Enqueue(
                                _queryingEnumerable._commandBuilder.ValueBufferFactory
                                    .Create(_dataReader));
                        }
                    }

                    _dataReader = null;
                }
            }

            public void BufferAll()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _dataReader?.Dispose();
                    _queryingEnumerable._relationalQueryContext.DeregisterValueBufferCursor(this);
                    _queryingEnumerable._relationalQueryContext.Connection?.Close();

                    _disposed = true;
                }
            }
        }
    }
}
