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
            Check.NotNull(relationalQueryContext, nameof(relationalQueryContext));
            Check.NotNull(commandBuilder, nameof(commandBuilder));
            Check.NotNull(shaper, nameof(shaper));
            Check.NotNull(logger, nameof(logger));

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

            private DbDataReader _reader;

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

                Current = !hasNext ? default(T) : _enumerable._shaper(_reader);

                return hasNext;
            }

            private async Task<bool> InitializeAndReadAsync(CancellationToken cancellationToken)
            {
                await _enumerable._relationalQueryContext.Connection
                    .OpenAsync(cancellationToken)
                    .WithCurrentCulture();

                using (var command
                    = _enumerable._commandBuilder
                        .Build(
                            _enumerable._relationalQueryContext.Connection,
                            _enumerable._relationalQueryContext.ParameterValues))
                {
                    _enumerable._logger.LogCommand(command);

                    _reader = await command.ExecuteReaderAsync(cancellationToken).WithCurrentCulture();
                }

                _enumerable._relationalQueryContext.RegisterDataReader(_reader);

                return await _reader.ReadAsync(cancellationToken).WithCurrentCulture();
            }

            public T Current { get; private set; }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;

                    if (_reader != null)
                    {
                        _reader.Dispose();
                        _enumerable._relationalQueryContext.Connection?.Close();
                    }
                }
            }
        }
    }
}
