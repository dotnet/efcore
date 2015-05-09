// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class QueryingEnumerable : IEnumerable<ValueBuffer>
    {
        private readonly RelationalQueryContext _relationalQueryContext;
        private readonly CommandBuilder _commandBuilder;
        private readonly ILogger _logger;

        public QueryingEnumerable(
            [NotNull] RelationalQueryContext relationalQueryContext,
            [NotNull] CommandBuilder commandBuilder,
            [NotNull] ILogger logger)
        {
            Check.NotNull(relationalQueryContext, nameof(relationalQueryContext));
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotNull(logger, nameof(logger));

            _relationalQueryContext = relationalQueryContext;
            _commandBuilder = commandBuilder;
            _logger = logger;
        }

        public virtual IEnumerator<ValueBuffer> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private sealed class Enumerator : IEnumerator<ValueBuffer>, IValueBufferCursor
        {
            private readonly QueryingEnumerable _queryingEnumerable;

            private DbDataReader _dataReader;

            private bool _disposed;

            public Enumerator(QueryingEnumerable queryingEnumerable)
            {
                _queryingEnumerable = queryingEnumerable;
            }

            public bool MoveNext()
            {
                if (_dataReader == null)
                {
                    _queryingEnumerable._relationalQueryContext.Connection.Open();

                    using (var command
                        = _queryingEnumerable._commandBuilder
                            .Build(
                                _queryingEnumerable._relationalQueryContext.Connection,
                                _queryingEnumerable._relationalQueryContext.ParameterValues))
                    {
                        _queryingEnumerable._logger.LogCommand(command);

                        _dataReader = command.ExecuteReader();

                        _queryingEnumerable._commandBuilder.NotifyReaderCreated(_dataReader);
                    }

                    _queryingEnumerable._relationalQueryContext.RegisterActiveQuery(this);
                }

                var hasNext = _dataReader.Read();

                Current
                    = hasNext
                        ? _queryingEnumerable._commandBuilder.ValueBufferFactory
                            .CreateValueBuffer(_dataReader)
                        : default(ValueBuffer);

                return hasNext;
            }

            public ValueBuffer Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                if (!_disposed)
                {
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
