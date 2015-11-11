// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class AsyncQueryingEnumerable : IAsyncEnumerable<ValueBuffer>
    {
        private readonly RelationalQueryContext _relationalQueryContext;
        private readonly ShaperCommandContext _shaperCommandContext;
        private readonly int? _queryIndex;

        public AsyncQueryingEnumerable(
            [NotNull] RelationalQueryContext relationalQueryContext,
            [NotNull] ShaperCommandContext shaperCommandContext,
            int? queryIndex)
        {
            _relationalQueryContext = relationalQueryContext;
            _shaperCommandContext = shaperCommandContext;
            _queryIndex = queryIndex;
        }

        public virtual IAsyncEnumerator<ValueBuffer> GetEnumerator() => new AsyncEnumerator(this);

        private sealed class AsyncEnumerator : IAsyncEnumerator<ValueBuffer>, IValueBufferCursor
        {
            private readonly AsyncQueryingEnumerable _queryingEnumerable;

            private RelationalDataReader _dataReader;
            private Queue<ValueBuffer> _buffer;

            private bool _disposed;

            private DbDataReader _dbDataReader;
            private IRelationalValueBufferFactory _valueBufferFactory;

            private ValueBuffer _current;

            public AsyncEnumerator(AsyncQueryingEnumerable queryingEnumerable)
            {
                _queryingEnumerable = queryingEnumerable;
                _valueBufferFactory = _queryingEnumerable._shaperCommandContext.ValueBufferFactory;
            }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_buffer == null)
                {
                    if (_dataReader == null)
                    {
                        await _queryingEnumerable._relationalQueryContext.Connection
                            .OpenAsync(cancellationToken);

                        var relationalCommand
                            = _queryingEnumerable._shaperCommandContext
                                .GetRelationalCommand(_queryingEnumerable._relationalQueryContext.ParameterValues);

                        await _queryingEnumerable._relationalQueryContext
                            .RegisterValueBufferCursorAsync(this, _queryingEnumerable._queryIndex, cancellationToken);

                        _dataReader
                            = await relationalCommand.ExecuteReaderAsync(
                                _queryingEnumerable._relationalQueryContext.Connection,
                                cancellationToken,
                                manageConnection: false,
                                parameters: _queryingEnumerable._relationalQueryContext.ParameterValues);

                        _dbDataReader = _dataReader.DbDataReader;
                        _queryingEnumerable._shaperCommandContext.NotifyReaderCreated(_dbDataReader);
                        _valueBufferFactory = _queryingEnumerable._shaperCommandContext.ValueBufferFactory;
                    }

                    var hasNext = await _dbDataReader.ReadAsync(cancellationToken);

                    _current
                        = hasNext
                            ? _valueBufferFactory.Create(_dbDataReader)
                            : default(ValueBuffer);

                    return hasNext;
                }

                if (_buffer.Count > 0)
                {
                    _current = _buffer.Dequeue();

                    return true;
                }

                return false;
            }

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public ValueBuffer Current => _current;

            public async Task BufferAllAsync(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_buffer == null)
                {
                    _buffer = new Queue<ValueBuffer>();

                    using (_dataReader)
                    {
                        while (await _dbDataReader.ReadAsync(cancellationToken))
                        {
                            _buffer.Enqueue(_valueBufferFactory.Create(_dbDataReader));
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
