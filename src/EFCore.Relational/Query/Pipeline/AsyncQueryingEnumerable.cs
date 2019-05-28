// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
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
            private readonly Func<QueryContext, DbDataReader, ResultCoordinator, Task<T>> _shaper;
            private readonly IQuerySqlGeneratorFactory2 _querySqlGeneratorFactory;
            private readonly Type _contextType;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

            public AsyncQueryingEnumerable(
                RelationalQueryContext relationalQueryContext,
                IQuerySqlGeneratorFactory2 querySqlGeneratorFactory,
                SelectExpression selectExpression,
                Func<QueryContext, DbDataReader, ResultCoordinator, Task<T>> shaper,
                Type contextType,
                IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            {
                _relationalQueryContext = relationalQueryContext;
                _querySqlGeneratorFactory = querySqlGeneratorFactory;
                _selectExpression = selectExpression;
                _shaper = shaper;
                _contextType = contextType;
                _logger = logger;
            }

            public IAsyncEnumerator<T> GetEnumerator()
            {
                return new AsyncEnumerator(this);
            }

            private sealed class AsyncEnumerator : IAsyncEnumerator<T>
            {
                private RelationalDataReader _dataReader;
                private ResultCoordinator _resultCoordinator;
                private readonly RelationalQueryContext _relationalQueryContext;
                private readonly SelectExpression _selectExpression;
                private readonly Func<QueryContext, DbDataReader, ResultCoordinator, Task<T>> _shaper;
                private readonly IQuerySqlGeneratorFactory2 _querySqlGeneratorFactory;
                private readonly Type _contextType;
                private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

                public AsyncEnumerator(AsyncQueryingEnumerable<T> queryingEnumerable)
                {
                    _relationalQueryContext = queryingEnumerable._relationalQueryContext;
                    _shaper = queryingEnumerable._shaper;
                    _selectExpression = queryingEnumerable._selectExpression;
                    _querySqlGeneratorFactory = queryingEnumerable._querySqlGeneratorFactory;
                    _contextType = queryingEnumerable._contextType;
                    _logger = queryingEnumerable._logger;
                }

                public T Current { get; private set; }

                public void Dispose()
                {
                    _dataReader?.Dispose();
                    _dataReader = null;
                    _relationalQueryContext.Connection.Close();
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    try
                    {
                        if (_dataReader == null)
                        {
                            await _relationalQueryContext.Connection.OpenAsync(cancellationToken);

                            try
                            {
                                var relationalCommand = _querySqlGeneratorFactory.Create()
                                    .GetCommand(
                                        _selectExpression,
                                        _relationalQueryContext.ParameterValues,
                                        _relationalQueryContext.CommandLogger);

                                _dataReader
                                    = await relationalCommand.ExecuteReaderAsync(
                                        _relationalQueryContext.Connection,
                                        _relationalQueryContext.ParameterValues,
                                        _relationalQueryContext.CommandLogger,
                                        cancellationToken);

                                _resultCoordinator = new ResultCoordinator();
                            }
                            catch (Exception)
                            {
                                // If failure happens creating the data reader, then it won't be available to
                                // handle closing the connection, so do it explicitly here to preserve ref counting.
                                _relationalQueryContext.Connection.Close();

                                throw;
                            }
                        }

                        var hasNext = _resultCoordinator.HasNext ?? await _dataReader.ReadAsync(cancellationToken);
                        _resultCoordinator.HasNext = null;

                        Current
                            = hasNext
                                ? await _shaper(_relationalQueryContext, _dataReader.DbDataReader, _resultCoordinator)
                                : default;

                        return hasNext;
                    }
                    catch (Exception exception)
                    {
                        _logger.QueryIterationFailed(_contextType, exception);

                        throw;
                    }
                }
            }
        }
    }
}
