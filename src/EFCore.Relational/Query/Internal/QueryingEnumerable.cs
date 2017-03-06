// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
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
    public class QueryingEnumerable : IEnumerable<ValueBuffer>
    {
        private readonly RelationalQueryContext _relationalQueryContext;
        private readonly ShaperCommandContext _shaperCommandContext;
        private readonly int? _queryIndex;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryingEnumerable(
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
        public virtual IEnumerator<ValueBuffer> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private sealed class Enumerator : IEnumerator<ValueBuffer>, IValueBufferCursor
        {
            private readonly QueryingEnumerable _queryingEnumerable;

            private RelationalDataReader _dataReader;
            private Queue<ValueBuffer> _buffer;

            private bool _disposed;

            private DbDataReader _dbDataReader;
            private IRelationalValueBufferFactory _valueBufferFactory;

            private ValueBuffer _current;

            public Enumerator(QueryingEnumerable queryingEnumerable)
            {
                _queryingEnumerable = queryingEnumerable;
            }

            public bool MoveNext()
            {
                if (_buffer == null)
                {
                    var executionStrategy = _queryingEnumerable._relationalQueryContext.ExecutionStrategyFactory.Create();
                    return executionStrategy.Execute(BufferlessMoveNext, executionStrategy.RetriesOnFailure);
                }

                if (_buffer.Count > 0)
                {
                    _current = _buffer.Dequeue();

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
                        _queryingEnumerable._relationalQueryContext.Connection.Open();

                        var relationalCommand
                            = _queryingEnumerable._shaperCommandContext
                                .GetRelationalCommand(_queryingEnumerable._relationalQueryContext.ParameterValues);

                        _queryingEnumerable._relationalQueryContext
                            .RegisterValueBufferCursor(this, _queryingEnumerable._queryIndex);

                        _dataReader
                            = relationalCommand.ExecuteReader(
                                _queryingEnumerable._relationalQueryContext.Connection,
                                _queryingEnumerable._relationalQueryContext.ParameterValues);

                        _dbDataReader = _dataReader.DbDataReader;
                        _queryingEnumerable._shaperCommandContext.NotifyReaderCreated(_dbDataReader);
                        _valueBufferFactory = _queryingEnumerable._shaperCommandContext.ValueBufferFactory;
                    }

                    var hasNext = _dbDataReader.Read();

                    _current
                        = hasNext
                            ? _valueBufferFactory.Create(_dbDataReader)
                            : default(ValueBuffer);

                    if (buffer)
                    {
                        BufferAll();
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

            public void BufferAll()
            {
                if (_buffer == null)
                {
                    _buffer = new Queue<ValueBuffer>();

                    using (_dataReader)
                    {
                        while (_dbDataReader.Read())
                        {
                            _buffer.Enqueue(_valueBufferFactory.Create(_dbDataReader));
                        }
                    }

                    _queryingEnumerable._relationalQueryContext.Connection?.Close();
                    _dataReader = null;
                    _dbDataReader = null;
                }
            }

            public Task BufferAllAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                if (!_disposed)
                {
                    _queryingEnumerable._relationalQueryContext.DeregisterValueBufferCursor(this);
                    if (_dataReader != null)
                    {
                        _dataReader.Dispose();
                        _queryingEnumerable._relationalQueryContext.Connection?.Close();
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
