// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class QueryingEnumerable<T> : IEnumerable<T>, IAsyncEnumerable<T>
    {
        private readonly RelationalQueryContext _relationalQueryContext;
        private readonly RelationalCommandCache _relationalCommandCache;
        private readonly IReadOnlyList<string> _columnNames;
        private readonly IReadOnlyList<ReaderColumn> _readerColumns;
        private readonly Func<QueryContext, DbDataReader, ResultContext, int[], ResultCoordinator, T> _shaper;
        private readonly Type _contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

        public QueryingEnumerable(
            RelationalQueryContext relationalQueryContext,
            RelationalCommandCache relationalCommandCache,
            IReadOnlyList<string> columnNames,
            IReadOnlyList<ReaderColumn> readerColumns,
            Func<QueryContext, DbDataReader, ResultContext, int[], ResultCoordinator, T> shaper,
            Type contextType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            _relationalQueryContext = relationalQueryContext;
            _relationalCommandCache = relationalCommandCache;
            _columnNames = columnNames;
            _readerColumns = readerColumns;
            _shaper = shaper;
            _contextType = contextType;
            _logger = logger;
        }

        public virtual IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new AsyncEnumerator(this, cancellationToken);

        public virtual IEnumerator<T> GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static int[] BuildIndexMap(IReadOnlyList<string> columnNames, DbDataReader dataReader)
        {
            if (columnNames == null)
            {
                return null;
            }

            // Non-Composed FromSql
            var readerColumns = Enumerable.Range(0, dataReader.FieldCount)
                .ToDictionary(dataReader.GetName, i => i, StringComparer.OrdinalIgnoreCase);

            var indexMap = new int[columnNames.Count];
            for (var i = 0; i < columnNames.Count; i++)
            {
                var columnName = columnNames[i];
                if (!readerColumns.TryGetValue(columnName, out var ordinal))
                {
                    throw new InvalidOperationException(RelationalStrings.FromSqlMissingColumn(columnName));
                }

                indexMap[i] = ordinal;
            }

            return indexMap;
        }

        private sealed class Enumerator : IEnumerator<T>
        {
            private readonly RelationalQueryContext _relationalQueryContext;
            private readonly RelationalCommandCache _relationalCommandCache;
            private readonly IReadOnlyList<string> _columnNames;
            private readonly IReadOnlyList<ReaderColumn> _readerColumns;
            private readonly Func<QueryContext, DbDataReader, ResultContext, int[], ResultCoordinator, T> _shaper;
            private readonly Type _contextType;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

            private RelationalDataReader _dataReader;
            private int[] _indexMap;
            private ResultCoordinator _resultCoordinator;
            private IExecutionStrategy _executionStrategy;

            public Enumerator(QueryingEnumerable<T> queryingEnumerable)
            {
                _relationalQueryContext = queryingEnumerable._relationalQueryContext;
                _relationalCommandCache = queryingEnumerable._relationalCommandCache;
                _columnNames = queryingEnumerable._columnNames;
                _readerColumns = queryingEnumerable._readerColumns;
                _shaper = queryingEnumerable._shaper;
                _contextType = queryingEnumerable._contextType;
                _logger = queryingEnumerable._logger;
            }

            public T Current { get; private set; }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                try
                {
                    using (_relationalQueryContext.ConcurrencyDetector.EnterCriticalSection())
                    {
                        if (_dataReader == null)
                        {
                            if (_executionStrategy == null)
                            {
                                _executionStrategy = _relationalQueryContext.ExecutionStrategyFactory.Create();
                            }

                            _executionStrategy.Execute(true, InitializeReader, null);
                        }

                        var hasNext = _resultCoordinator.HasNext ?? _dataReader.Read();
                        Current = default;

                        if (hasNext)
                        {
                            while (true)
                            {
                                _resultCoordinator.ResultReady = true;
                                _resultCoordinator.HasNext = null;
                                Current = _shaper(
                                    _relationalQueryContext, _dataReader.DbDataReader,
                                    _resultCoordinator.ResultContext, _indexMap, _resultCoordinator);
                                if (_resultCoordinator.ResultReady)
                                {
                                    // We generated a result so null out previously stored values
                                    _resultCoordinator.ResultContext.Values = null;
                                    break;
                                }

                                if (!_dataReader.Read())
                                {
                                    _resultCoordinator.HasNext = false;
                                    // Enumeration has ended, materialize last element
                                    _resultCoordinator.ResultReady = true;
                                    Current = _shaper(
                                        _relationalQueryContext, _dataReader.DbDataReader,
                                        _resultCoordinator.ResultContext, _indexMap, _resultCoordinator);

                                    break;
                                }
                            }
                        }

                        return hasNext;
                    }
                }
                catch (Exception exception)
                {
                    _logger.QueryIterationFailed(_contextType, exception);

                    throw;
                }
            }

            private bool InitializeReader(DbContext _, bool result)
            {
                var relationalCommand = _relationalCommandCache.GetRelationalCommand(_relationalQueryContext.ParameterValues);

                _dataReader
                    = relationalCommand.ExecuteReader(
                        new RelationalCommandParameterObject(
                            _relationalQueryContext.Connection,
                            _relationalQueryContext.ParameterValues,
                            _readerColumns,
                            _relationalQueryContext.Context,
                            _relationalQueryContext.CommandLogger));

                _indexMap = BuildIndexMap(_columnNames, _dataReader.DbDataReader);

                _resultCoordinator = new ResultCoordinator();

                return result;
            }

            public void Dispose()
            {
                _dataReader?.Dispose();
                _dataReader = null;
            }

            public void Reset() => throw new NotImplementedException();
        }

        private sealed class AsyncEnumerator : IAsyncEnumerator<T>
        {
            private readonly RelationalQueryContext _relationalQueryContext;
            private readonly RelationalCommandCache _relationalCommandCache;
            private readonly IReadOnlyList<string> _columnNames;
            private readonly IReadOnlyList<ReaderColumn> _readerColumns;
            private readonly Func<QueryContext, DbDataReader, ResultContext, int[], ResultCoordinator, T> _shaper;
            private readonly Type _contextType;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
            private readonly CancellationToken _cancellationToken;

            private RelationalDataReader _dataReader;
            private int[] _indexMap;
            private ResultCoordinator _resultCoordinator;
            private IExecutionStrategy _executionStrategy;

            public AsyncEnumerator(
                QueryingEnumerable<T> queryingEnumerable,
                CancellationToken cancellationToken)
            {
                _relationalQueryContext = queryingEnumerable._relationalQueryContext;
                _relationalCommandCache = queryingEnumerable._relationalCommandCache;
                _columnNames = queryingEnumerable._columnNames;
                _readerColumns = queryingEnumerable._readerColumns;
                _shaper = queryingEnumerable._shaper;
                _contextType = queryingEnumerable._contextType;
                _logger = queryingEnumerable._logger;
                _cancellationToken = cancellationToken;
            }

            public T Current { get; private set; }

            public async ValueTask<bool> MoveNextAsync()
            {
                try
                {
                    using (_relationalQueryContext.ConcurrencyDetector.EnterCriticalSection())
                    {
                        if (_dataReader == null)
                        {
                            if (_executionStrategy == null)
                            {
                                _executionStrategy = _relationalQueryContext.ExecutionStrategyFactory.Create();
                            }

                            await _executionStrategy.ExecuteAsync(true, InitializeReaderAsync, null, _cancellationToken);
                        }

                        var hasNext = _resultCoordinator.HasNext ?? await _dataReader.ReadAsync(_cancellationToken);
                        Current = default;

                        if (hasNext)
                        {
                            while (true)
                            {
                                _resultCoordinator.ResultReady = true;
                                _resultCoordinator.HasNext = null;
                                Current = _shaper(
                                    _relationalQueryContext, _dataReader.DbDataReader,
                                    _resultCoordinator.ResultContext, _indexMap, _resultCoordinator);
                                if (_resultCoordinator.ResultReady)
                                {
                                    // We generated a result so null out previously stored values
                                    _resultCoordinator.ResultContext.Values = null;
                                    break;
                                }

                                if (!await _dataReader.ReadAsync(_cancellationToken))
                                {
                                    _resultCoordinator.HasNext = false;
                                    // Enumeration has ended, materialize last element
                                    _resultCoordinator.ResultReady = true;
                                    Current = _shaper(
                                        _relationalQueryContext, _dataReader.DbDataReader,
                                        _resultCoordinator.ResultContext, _indexMap, _resultCoordinator);

                                    break;
                                }
                            }
                        }

                        return hasNext;
                    }
                }
                catch (Exception exception)
                {
                    _logger.QueryIterationFailed(_contextType, exception);

                    throw;
                }
            }

            private async Task<bool> InitializeReaderAsync(DbContext _, bool result, CancellationToken cancellationToken)
            {
                var relationalCommand = _relationalCommandCache.GetRelationalCommand(
                    _relationalQueryContext.ParameterValues);

                _dataReader
                    = await relationalCommand.ExecuteReaderAsync(
                        new RelationalCommandParameterObject(
                            _relationalQueryContext.Connection,
                            _relationalQueryContext.ParameterValues,
                            _readerColumns,
                            _relationalQueryContext.Context,
                            _relationalQueryContext.CommandLogger),
                        cancellationToken);

                _indexMap = BuildIndexMap(_columnNames, _dataReader.DbDataReader);

                _resultCoordinator = new ResultCoordinator();

                return result;
            }

            public ValueTask DisposeAsync()
            {
                if (_dataReader != null)
                {
                    var dataReader = _dataReader;
                    _dataReader = null;

                    return dataReader.DisposeAsync();
                }

                return default;
            }
        }
    }
}
