// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
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
    public class QueryingEnumerable<T> : IEnumerable<T>
    {
        private readonly RelationalQueryContext _relationalQueryContext;
        private readonly ShaperCommandContext _shaperCommandContext;
        private readonly IShaper<T> _shaper;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryingEnumerable(
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
        public virtual IEnumerator<T> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private sealed class Enumerator : IEnumerator<T>, IBufferable
        {
            private readonly RelationalQueryContext _relationalQueryContext;
            private readonly ShaperCommandContext _shaperCommandContext;
            private readonly IShaper<T> _shaper;
            private readonly Func<DbContext, bool, bool> _bufferlessMoveNext;

            private RelationalDataReader _dataReader;
            private Queue<ValueBuffer> _buffer;
            private DbDataReader _dbDataReader;
            private IRelationalValueBufferFactory _valueBufferFactory;
            private IExecutionStrategy _executionStrategy;

            private bool _disposed;

            public Enumerator(QueryingEnumerable<T> queryingEnumerable)
            {
                _shaperCommandContext = queryingEnumerable._shaperCommandContext;
                _valueBufferFactory = _shaperCommandContext.ValueBufferFactory;
                _relationalQueryContext = queryingEnumerable._relationalQueryContext;
                _shaper = queryingEnumerable._shaper;
                _bufferlessMoveNext = BufferlessMoveNext;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (_buffer == null)
                {
                    if (_executionStrategy == null)
                    {
                        _executionStrategy = _relationalQueryContext.ExecutionStrategyFactory.Create();
                    }

                    return _executionStrategy.Execute(_executionStrategy.RetriesOnFailure, _bufferlessMoveNext, null);
                }

                if (_buffer.Count > 0)
                {
                    Current = _shaper.Shape(_relationalQueryContext, _buffer.Dequeue());

                    return true;
                }

                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool BufferlessMoveNext(DbContext _, bool buffer)
            {
                if (_dataReader == null)
                {
                    _relationalQueryContext.Connection.Open();

                    try
                    {
                        var relationalCommand
                            = _shaperCommandContext
                                .GetRelationalCommand(_relationalQueryContext.ParameterValues);

                        _relationalQueryContext.Connection.RegisterBufferable(this);

                        _dataReader
                            = relationalCommand.ExecuteReader(
                                _relationalQueryContext.Connection,
                                _relationalQueryContext.ParameterValues);
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

                var hasNext = _dataReader.Read();

                Current
                    = hasNext
                        ? _shaper.Shape(_relationalQueryContext, _valueBufferFactory.Create(_dbDataReader))
                        : default;

                if (buffer)
                {
                    BufferAll();
                }

                return hasNext;
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get;
                private set;
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

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                if (!_disposed)
                {
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
            }

            public void Reset() => throw new NotImplementedException();
        }
    }
}
