// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class AsyncQueryingEnumerable : IAsyncEnumerable<ValueBuffer>
    {
        private readonly RelationalQueryContext _relationalQueryContext;
        private readonly ShaperCommandContext _shaperCommandContext;
        private readonly int? _queryIndex;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public AsyncQueryingEnumerable(
            [NotNull] RelationalQueryContext relationalQueryContext,
            [NotNull] ShaperCommandContext shaperCommandContext,
            int? queryIndex)
        {
            _relationalQueryContext = relationalQueryContext;
            _shaperCommandContext = shaperCommandContext;
            _queryIndex = queryIndex;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

                try
                {
                    await _queryingEnumerable._relationalQueryContext.Semaphore.WaitAsync(cancellationToken);

                    if (_buffer == null)
                    {
                        var executionStrategy = _queryingEnumerable._relationalQueryContext.ExecutionStrategyFactory.Create();
                        return await executionStrategy.ExecuteAsync(BufferlessMoveNext, executionStrategy.RetriesOnFailure, cancellationToken);
                    }

                    if (_buffer.Count > 0)
                    {
                        _current = _buffer.Dequeue();

                        return true;
                    }

                    return false;
                }
                finally
                {
                    _queryingEnumerable._relationalQueryContext.Semaphore.Release();
                }
            }

            private async Task<bool> BufferlessMoveNext(bool buffer, CancellationToken cancellationToken)
            {
                try
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
                                _queryingEnumerable._relationalQueryContext.ParameterValues,
                                cancellationToken);

                        _dbDataReader = _dataReader.DbDataReader;
                        _queryingEnumerable._shaperCommandContext.NotifyReaderCreated(_dbDataReader);
                        _valueBufferFactory = _queryingEnumerable._shaperCommandContext.ValueBufferFactory;
                    }

                    var hasNext = await _dbDataReader.ReadAsync(cancellationToken);

                    _current
                        = hasNext
                            ? _valueBufferFactory.Create(_dbDataReader)
                            : default(ValueBuffer);

                    if (buffer)
                    {
                        await BufferAllAsync(cancellationToken);
                    }

                    return hasNext;
                }
                catch (Exception)
                {
                    _queryingEnumerable._relationalQueryContext.DeregisterValueBufferCursor(this);
                    _dataReader = null;
                    _dbDataReader = null;

                    throw;
                }
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

                    _queryingEnumerable._relationalQueryContext.Connection?.Close();
                    _dataReader = null;
                    _dbDataReader = null;
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
                    lock (_queryingEnumerable._relationalQueryContext)
                    {
                        _queryingEnumerable._relationalQueryContext.DeregisterValueBufferCursor(this);
                        if (_dataReader != null)
                        {
                            _dataReader.Dispose();
                            _queryingEnumerable._relationalQueryContext.Connection?.Close();
                        }
                    }

                    _disposed = true;
                }
            }
        }
    }
}
