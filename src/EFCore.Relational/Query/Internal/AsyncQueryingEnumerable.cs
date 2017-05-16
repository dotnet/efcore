// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
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

            private RelationalDataReader _dataReader;
            private Queue<ValueBuffer> _buffer;
            private DbDataReader _dbDataReader;
            private IRelationalValueBufferFactory _valueBufferFactory;

            private bool _disposed;

            public AsyncEnumerator(AsyncQueryingEnumerable<T> queryingEnumerable)
            {
                _shaperCommandContext = queryingEnumerable._shaperCommandContext;
                _valueBufferFactory = _shaperCommandContext.ValueBufferFactory;
                _relationalQueryContext = queryingEnumerable._relationalQueryContext;
                _shaper = queryingEnumerable._shaper;
            }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await _relationalQueryContext.Connection.Semaphore.WaitAsync(cancellationToken);

                    if (_buffer == null)
                    {
                        var executionStrategy = _relationalQueryContext.ExecutionStrategyFactory.Create();

                        return await executionStrategy
                            .ExecuteAsync(BufferlessMoveNext, executionStrategy.RetriesOnFailure, cancellationToken);
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

            private async Task<bool> BufferlessMoveNext(bool buffer, CancellationToken cancellationToken)
            {
                try
                {
                    if (_dataReader == null)
                    {
                        await _relationalQueryContext.Connection.OpenAsync(cancellationToken);

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

                        _dbDataReader = _dataReader.DbDataReader;
                        _shaperCommandContext.NotifyReaderCreated(_dbDataReader);
                        _valueBufferFactory = _shaperCommandContext.ValueBufferFactory;
                    }

                    var hasNext = await _dbDataReader.ReadAsync(cancellationToken);

                    Current
                        = hasNext
                            ? _shaper.Shape(_relationalQueryContext, _valueBufferFactory.Create(_dbDataReader))
                            : default(T);

                    if (buffer)
                    {
                        await BufferAllAsync(cancellationToken);
                    }

                    return hasNext;
                }
                catch (Exception)
                {
                    _dataReader = null;
                    _dbDataReader = null;

                    throw;
                }
            }

            public T Current { get; private set; }

            public async Task BufferAllAsync(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_buffer == null
                    && _dataReader != null)
                {
                    _buffer = new Queue<ValueBuffer>();

                    using (_dataReader)
                    {
                        while (await _dbDataReader.ReadAsync(cancellationToken))
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
                        while (_dbDataReader.Read())
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
