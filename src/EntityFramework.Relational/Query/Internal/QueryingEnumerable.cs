// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class QueryingEnumerable : IEnumerable<ValueBuffer>
    {
        private readonly RelationalQueryContext _relationalQueryContext;
        private readonly CommandBuilder _commandBuilder;
        private readonly int? _queryIndex;

        public QueryingEnumerable(
            [NotNull] RelationalQueryContext relationalQueryContext,
            [NotNull] CommandBuilder commandBuilder,
            int? queryIndex)
        {
            _relationalQueryContext = relationalQueryContext;
            _commandBuilder = commandBuilder;
            _queryIndex = queryIndex;
        }

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
                    if (_dataReader == null)
                    {
                        _queryingEnumerable._relationalQueryContext.Connection.Open();

                        var command
                            = _queryingEnumerable._commandBuilder
                                .Build(_queryingEnumerable._relationalQueryContext.ParameterValues);

                        _queryingEnumerable._relationalQueryContext
                            .RegisterValueBufferCursor(this, _queryingEnumerable._queryIndex);

                        _dataReader 
                            = command.ExecuteReader(
                                _queryingEnumerable._relationalQueryContext.Connection, 
                                manageConnection: false);

                        _dbDataReader = _dataReader.DbDataReader;
                        _queryingEnumerable._commandBuilder.NotifyReaderCreated(_dbDataReader);
                        _valueBufferFactory = _queryingEnumerable._commandBuilder.ValueBufferFactory;
                    }

                    var hasNext = _dbDataReader.Read();

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

                    _dataReader = null;
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
                    _dataReader?.Dispose();
                    _queryingEnumerable._relationalQueryContext.DeregisterValueBufferCursor(this);
                    _queryingEnumerable._relationalQueryContext.Connection?.Close();

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
