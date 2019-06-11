// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public partial class RelationalShapedQueryCompilingExpressionVisitor
    {
        private class AsyncQueryingEnumerable<T> : IAsyncEnumerable<T>
        {
            private readonly RelationalQueryContext _relationalQueryContext;
            private readonly SelectExpression _selectExpression;
            private readonly Func<QueryContext, DbDataReader, int[], ResultCoordinator, Task<T>> _shaper;
            private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
            private readonly Type _contextType;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
            private readonly ISqlExpressionFactory _sqlExpressionFactory;
            private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;

            public AsyncQueryingEnumerable(
                RelationalQueryContext relationalQueryContext,
                IQuerySqlGeneratorFactory querySqlGeneratorFactory,
                ISqlExpressionFactory sqlExpressionFactory,
                IParameterNameGeneratorFactory parameterNameGeneratorFactory,
                SelectExpression selectExpression,
                Func<QueryContext, DbDataReader, int[], ResultCoordinator, Task<T>> shaper,
                Type contextType,
                IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            {
                _relationalQueryContext = relationalQueryContext;
                _querySqlGeneratorFactory = querySqlGeneratorFactory;
                _sqlExpressionFactory = sqlExpressionFactory;
                _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
                _selectExpression = selectExpression;
                _shaper = shaper;
                _contextType = contextType;
                _logger = logger;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new AsyncEnumerator(this, cancellationToken);

            private sealed class AsyncEnumerator : IAsyncEnumerator<T>
            {
                private RelationalDataReader _dataReader;
                private int[] _indexMap;
                private ResultCoordinator _resultCoordinator;
                private readonly RelationalQueryContext _relationalQueryContext;
                private readonly SelectExpression _selectExpression;
                private readonly Func<QueryContext, DbDataReader, int[], ResultCoordinator, Task<T>> _shaper;
                private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
                private readonly Type _contextType;
                private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
                private readonly ISqlExpressionFactory _sqlExpressionFactory;
                private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;
                private readonly CancellationToken _cancellationToken;

                public AsyncEnumerator(
                    AsyncQueryingEnumerable<T> queryingEnumerable,
                    CancellationToken cancellationToken)
                {
                    _relationalQueryContext = queryingEnumerable._relationalQueryContext;
                    _shaper = queryingEnumerable._shaper;
                    _selectExpression = queryingEnumerable._selectExpression;
                    _querySqlGeneratorFactory = queryingEnumerable._querySqlGeneratorFactory;
                    _contextType = queryingEnumerable._contextType;
                    _logger = queryingEnumerable._logger;
                    _sqlExpressionFactory = queryingEnumerable._sqlExpressionFactory;
                    _parameterNameGeneratorFactory = queryingEnumerable._parameterNameGeneratorFactory;
                    _cancellationToken = cancellationToken;
                }

                public T Current { get; private set; }

                public async ValueTask<bool> MoveNextAsync()
                {
                    try
                    {
                        if (_dataReader == null)
                        {
                            var selectExpression = new ParameterValueBasedSelectExpressionOptimizer(
                                _sqlExpressionFactory,
                                _parameterNameGeneratorFactory)
                                .Optimize(_selectExpression, _relationalQueryContext.ParameterValues);

                            var relationalCommand = _querySqlGeneratorFactory.Create().GetCommand(selectExpression);

                            _dataReader
                                = await relationalCommand.ExecuteReaderAsync(
                                    _relationalQueryContext.Connection,
                                    _relationalQueryContext.ParameterValues,
                                    _relationalQueryContext.CommandLogger,
                                    _cancellationToken);

                            if (selectExpression.IsNonComposedFromSql())
                            {
                                var projection = _selectExpression.Projection.ToList();
                                var readerColumns = Enumerable.Range(0, _dataReader.DbDataReader.FieldCount)
                                    .ToDictionary(i => _dataReader.DbDataReader.GetName(i), i => i, StringComparer.OrdinalIgnoreCase);

                                _indexMap = new int[projection.Count];
                                for (var i = 0; i < projection.Count; i++)
                                {
                                    if (projection[i].Expression is ColumnExpression columnExpression)
                                    {
                                        var columnName = columnExpression.Name;
                                        if (columnName != null)
                                        {
                                            if (!readerColumns.TryGetValue(columnName, out var ordinal))
                                            {
                                                throw new InvalidOperationException(RelationalStrings.FromSqlMissingColumn(columnName));
                                            }

                                            _indexMap[i] = ordinal;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _indexMap = null;
                            }

                            _resultCoordinator = new ResultCoordinator();
                        }

                        var hasNext = _resultCoordinator.HasNext ?? await _dataReader.ReadAsync(_cancellationToken);
                        _resultCoordinator.HasNext = null;

                        Current
                            = hasNext
                                ? await _shaper(_relationalQueryContext, _dataReader.DbDataReader, _indexMap, _resultCoordinator)
                                : default;

                        return hasNext;
                    }
                    catch (Exception exception)
                    {
                        _logger.QueryIterationFailed(_contextType, exception);

                        throw;
                    }
                }

                public ValueTask DisposeAsync()
                {
                    _dataReader?.Dispose();
                    _dataReader = null;

                    return default;
                }
            }
        }
    }
}
