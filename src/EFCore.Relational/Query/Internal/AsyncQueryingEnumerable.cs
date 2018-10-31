// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class AsyncQueryingEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly RelationalQueryContext _relationalQueryContext;
        private readonly ShaperCommandContext _shaperCommandContext;
        private readonly IShaper<T> _shaper;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public AsyncQueryingEnumerable(
            [NotNull] RelationalQueryContext relationalQueryContext,
            [NotNull] ShaperCommandContext shaperCommandContext,
            [NotNull] IShaper<T> shaper)
        {
            _relationalQueryContext = relationalQueryContext;
            _shaperCommandContext = shaperCommandContext;
            _shaper = shaper;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IAsyncEnumerator<T> GetEnumerator() => new AsyncEnumerator(this);

        private sealed class AsyncEnumerator : IAsyncEnumerator<T>, IBufferable
        {
            private readonly RelationalQueryContext _relationalQueryContext;
            private readonly ShaperCommandContext _shaperCommandContext;
            private readonly IShaper<T> _shaper;
            private readonly Func<DbContext, bool, CancellationToken, Task<bool>> _bufferlessMoveNext;

            private RelationalDataReader _dataReader;
            private Queue<ValueBuffer> _buffer;
            private DbDataReader _dbDataReader;
            private IRelationalValueBufferFactory _valueBufferFactory;
            private IExecutionStrategy _executionStrategy;

            private bool _disposed;

            public AsyncEnumerator(AsyncQueryingEnumerable<T> queryingEnumerable)
            {
                _shaperCommandContext = queryingEnumerable._shaperCommandContext;
                _valueBufferFactory = _shaperCommandContext.ValueBufferFactory;
                _relationalQueryContext = queryingEnumerable._relationalQueryContext;
                _shaper = queryingEnumerable._shaper;
                _bufferlessMoveNext = BufferlessMoveNext;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await _relationalQueryContext.Connection.Semaphore.WaitAsync(cancellationToken);

                    if (_buffer == null)
                    {
                        if (_executionStrategy == null)
                        {
                            _executionStrategy = _relationalQueryContext.ExecutionStrategyFactory.Create();
                        }

                        return await _executionStrategy
                            .ExecuteAsync(_executionStrategy.RetriesOnFailure, _bufferlessMoveNext, null, cancellationToken);
                    }

                    if (_buffer.Count > 0)
                    {
                        Current = _shaper.Shape(_relationalQueryContext, _buffer.Dequeue());

                        return true;
                    }

                    return false;
                }
                finally
                {
                    _relationalQueryContext.Connection.Semaphore.Release();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private async Task<bool> BufferlessMoveNext(DbContext _, bool buffer, CancellationToken cancellationToken)
            {
                if (_dataReader == null)
                {
                    await _relationalQueryContext.Connection.OpenAsync(cancellationToken);

                    try
                    {
                        var relationalCommand
                            = _shaperCommandContext
                                .GetRelationalCommand(_relationalQueryContext.ParameterValues);

                        await _relationalQueryContext.Connection
                            .RegisterBufferableAsync(this, cancellationToken);

                        _dataReader
                            = await relationalCommand.ExecuteReaderAsync(
                                _relationalQueryContext.Connection,
                                _relationalQueryContext.ParameterValues,
                                cancellationToken);
                    }
                    catch
                    {
                        // If failure happens creating the data reader, then it won't be available to
                        // handle closing the connection, so do it explicitly here to preserve ref counting.
                        _relationalQueryContext.Connection.Close();

                        throw;
                    }

                    _dbDataReader = _dataReader.DbDataReader;
                    _shaperCommandContext.NotifyReaderCreated(_dbDataReader);
                    _valueBufferFactory = _shaperCommandContext.ValueBufferFactory;
                }

                var hasNext = await _dataReader.ReadAsync(cancellationToken);

                Current
                    = hasNext
                        ? _shaper.Shape(_relationalQueryContext, _valueBufferFactory.Create(_dbDataReader))
                        : default;

                if (buffer)
                {
                    await BufferAllAsync(cancellationToken);
                }

                return hasNext;
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get;
                private set;
            }

            public async Task BufferAllAsync(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_buffer == null
                    && _dataReader != null)
                {
                    _buffer = new Queue<ValueBuffer>();

                    using (_dataReader)
                    {
                        while (await _dataReader.ReadAsync(cancellationToken))
                        {
                            _buffer.Enqueue(_valueBufferFactory.Create(_dbDataReader));
                        }
                    }

                    _relationalQueryContext.Connection?.Close();
                    _dataReader = null;
                    _dbDataReader = null;
                }
            }

            public void BufferAll()
            {
                if (_buffer == null
                    && _dataReader != null)
                {
                    _buffer = new Queue<ValueBuffer>();

                    using (_dataReader)
                    {
                        while (_dataReader.Read())
                        {
                            _buffer.Enqueue(_valueBufferFactory.Create(_dbDataReader));
                        }
                    }

                    _relationalQueryContext.Connection?.Close();

                    _dataReader = null;
                    _dbDataReader = null;
                }
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    try
                    {
                        _relationalQueryContext.Connection.Semaphore.Wait();

                        if (_dataReader != null)
                        {
                            _dataReader.Dispose();
                            _dataReader = null;
                            _dbDataReader = null;
                            _buffer = null;

                            _relationalQueryContext.Connection?.Close();
                        }
                        _relationalQueryContext.Connection?.UnregisterBufferable(this);

                        _disposed = true;
                    }
                    finally
                    {
                        _relationalQueryContext.Connection.Semaphore.Release();
                    }
                }
            }
        }
    }
}
