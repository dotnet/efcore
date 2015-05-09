// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class AsyncQueryingEnumerable : IAsyncEnumerable<ValueBuffer>
    {
        private readonly RelationalQueryContext _relationalQueryContext;
        private readonly CommandBuilder _commandBuilder;
        private readonly ILogger _logger;

        public AsyncQueryingEnumerable(
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

        public virtual IAsyncEnumerator<ValueBuffer> GetEnumerator()
        {
            return new AsyncEnumerator(this);
        }

        private sealed class AsyncEnumerator : IAsyncEnumerator<ValueBuffer>, IValueBufferCursor
        {
            private readonly AsyncQueryingEnumerable _queryingEnumerable;

            private DbDataReader _dataReader;

            private bool _disposed;

            public AsyncEnumerator(AsyncQueryingEnumerable queryingEnumerable)
            {
                _queryingEnumerable = queryingEnumerable;
            }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                Debug.Assert(!_disposed);

                cancellationToken.ThrowIfCancellationRequested();

                if (_dataReader == null)
                {
                    await _queryingEnumerable._relationalQueryContext.Connection
                        .OpenAsync(cancellationToken);

                    using (var command
                        = _queryingEnumerable._commandBuilder
                            .Build(
                                _queryingEnumerable._relationalQueryContext.Connection,
                                _queryingEnumerable._relationalQueryContext.ParameterValues))
                    {
                        _queryingEnumerable._logger.LogCommand(command);

                        _dataReader = await command.ExecuteReaderAsync(cancellationToken);

                        _queryingEnumerable._commandBuilder.NotifyReaderCreated(_dataReader);
                    }

                    _queryingEnumerable._relationalQueryContext.RegisterActiveQuery(this);
                }

                var hasNext = await _dataReader.ReadAsync(cancellationToken);

                Current
                    = hasNext
                        ? _queryingEnumerable._commandBuilder.ValueBufferFactory
                            .CreateValueBuffer(_dataReader)
                        : default(ValueBuffer);

                return hasNext;
            }

            public ValueBuffer Current { get; private set; }

            private readonly object _gate = new object();

            public void Dispose()
            {
                // TODO: Undiagnosed IX-Async re-entrancy here.
                // https://github.com/Reactive-Extensions/Rx.NET/issues/93
                lock (_gate)
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
            }
        }
    }
}
