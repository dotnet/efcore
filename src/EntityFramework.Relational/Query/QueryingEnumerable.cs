// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class QueryingEnumerable<T> : IEnumerable<T>
    {
        private readonly RelationalQueryContext _relationalQueryContext;
        private readonly CommandBuilder _commandBuilder;
        private readonly Func<DbDataReader, T> _shaper;
        private readonly ILogger _logger;

        public QueryingEnumerable(
            [NotNull] RelationalQueryContext relationalQueryContext,
            [NotNull] CommandBuilder commandBuilder,
            [NotNull] Func<DbDataReader, T> shaper,
            [NotNull] ILogger logger)
        {
            Check.NotNull(relationalQueryContext, "relationalQueryContext");
            Check.NotNull(relationalQueryContext, "commandBuilder");
            Check.NotNull(relationalQueryContext, "shaper");
            Check.NotNull(relationalQueryContext, "logger");

            _relationalQueryContext = relationalQueryContext;
            _commandBuilder = commandBuilder;
            _shaper = shaper;
            _logger = logger;
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private sealed class Enumerator : IEnumerator<T>
        {
            private readonly QueryingEnumerable<T> _enumerable;

            private DbCommand _command;
            private DbDataReader _reader;
            private bool _disposed;

            public Enumerator(QueryingEnumerable<T> enumerable)
            {
                _enumerable = enumerable;
            }

            public bool MoveNext()
            {
                if (_reader == null)
                {
                    _enumerable._relationalQueryContext.Connection.Open();

                    _command
                        = _enumerable._commandBuilder
                            .Build(_enumerable._relationalQueryContext.Connection);

                    _enumerable._logger.WriteSql(_command.CommandText);

                    _reader = _command.ExecuteReader();

                    _enumerable._relationalQueryContext.RegisterDataReader(_reader);
                }

                var hasNext = _reader.Read();

                Current = hasNext ? _enumerable._shaper(_reader) : default(T);

                return hasNext;
            }

            public T Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                if (!_disposed)
                {
                    if (_reader != null)
                    {
                        _enumerable._relationalQueryContext.Connection?.Close();
                        _reader.Dispose();
                    }

                    _command?.Dispose();

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
