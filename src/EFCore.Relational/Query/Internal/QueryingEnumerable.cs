// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
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

            private RelationalDataReader _dataReader;
            private Queue<ValueBuffer> _buffer;
            private DbDataReader _dbDataReader;
            private IRelationalValueBufferFactory _valueBufferFactory;

            private bool _disposed;

            public Enumerator(QueryingEnumerable<T> queryingEnumerable)
            {
                _shaperCommandContext = queryingEnumerable._shaperCommandContext;
                _valueBufferFactory = _shaperCommandContext.ValueBufferFactory;
                _relationalQueryContext = queryingEnumerable._relationalQueryContext;
                _shaper = queryingEnumerable._shaper;
            }

            public bool MoveNext()
            {
                if (_buffer == null)
                {
                    var executionStrategy = _relationalQueryContext.ExecutionStrategyFactory.Create();

                    return executionStrategy.Execute(BufferlessMoveNext, executionStrategy.RetriesOnFailure);
                }

                if (_buffer.Count > 0)
                {
                    Current = _shaper.Shape(_relationalQueryContext, _buffer.Dequeue());

                    return true;
                }

                return false;
            }

            private bool BufferlessMoveNext(bool buffer)
            {
                try
                {
                    if (_dataReader == null)
                    {
                        _relationalQueryContext.Connection.Open();

                        var relationalCommand
                            = _shaperCommandContext
                                .GetRelationalCommand(_relationalQueryContext.ParameterValues);

                        _relationalQueryContext.Connection.RegisterBufferable(this);

                        _dataReader
                            = relationalCommand.ExecuteReader(
                                _relationalQueryContext.Connection,
                                _relationalQueryContext.ParameterValues);

                        _dbDataReader = _dataReader.DbDataReader;
                        _shaperCommandContext.NotifyReaderCreated(_dbDataReader);
                        _valueBufferFactory = _shaperCommandContext.ValueBufferFactory;
                    }

                    var hasNext = _dbDataReader.Read();

                    Current
                        = hasNext
                            ? _shaper.Shape(_relationalQueryContext, _valueBufferFactory.Create(_dbDataReader))
                            : default(T);

                    if (buffer)
                    {
                        BufferAll();
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

                    _disposed = true;
                }
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}
