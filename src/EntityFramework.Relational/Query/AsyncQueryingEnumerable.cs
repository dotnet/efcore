// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class AsyncQueryingEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly RelationalQueryContext _relationalQueryContext;
        private readonly CommandBuilder _commandBuilder;
        private readonly Func<DbDataReader, T> _shaper;
        private readonly ILogger _logger;

        public AsyncQueryingEnumerable(
            [NotNull] RelationalQueryContext relationalQueryContext,
            [NotNull] CommandBuilder commandBuilder,
            [NotNull] Func<DbDataReader, T> shaper,
            [NotNull] ILogger logger)
        {
            Check.NotNull(relationalQueryContext, "relationalQueryContext");
            Check.NotNull(commandBuilder, "commandBuilder");
            Check.NotNull(shaper, "shaper");
            Check.NotNull(logger, "logger");

            _relationalQueryContext = relationalQueryContext;
            _commandBuilder = commandBuilder;
            _shaper = shaper;
            _logger = logger;
        }

        public virtual IAsyncEnumerator<T> GetEnumerator()
        {
            return new AsyncEnumerator(this);
        }

        private sealed class AsyncEnumerator : IAsyncEnumerator<T>
        {
            private readonly AsyncQueryingEnumerable<T> _enumerable;

            private DbCommand _command;
            private DbDataReader _reader;

            private T _current;

            private bool _disposed;

            public AsyncEnumerator(AsyncQueryingEnumerable<T> enumerable)
            {
                _enumerable = enumerable;
            }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var hasNext
                    = await (_reader == null
                        ? InitializeAndReadAsync(cancellationToken)
                        : _reader.ReadAsync(cancellationToken))
                        .WithCurrentCulture();

                _current = !hasNext ? default(T) : _enumerable._shaper(_reader);

                return hasNext;
            }

            private async Task<bool> InitializeAndReadAsync(CancellationToken cancellationToken)
            {
                await _enumerable._relationalQueryContext.Connection
                    .OpenAsync(cancellationToken)
                    .WithCurrentCulture();

                _command = _enumerable._commandBuilder.Build(_enumerable._relationalQueryContext.Connection);

                _enumerable._logger.WriteSql(_command.CommandText);

                _reader = await _command.ExecuteReaderAsync(cancellationToken).WithCurrentCulture();

                _enumerable._relationalQueryContext.RegisterDataReader(_reader);

                return await _reader.ReadAsync(cancellationToken).WithCurrentCulture();
            }

            public T Current => _current;

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
        }
    }
}
