// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public partial class CosmosShapedQueryCompilingExpressionVisitor
    {
        private sealed class QueryingEnumerable<T> : IEnumerable<T>, IAsyncEnumerable<T>, IQueryingEnumerable
        {
            private readonly CosmosQueryContext _cosmosQueryContext;
            private readonly ISqlExpressionFactory _sqlExpressionFactory;
            private readonly SelectExpression _selectExpression;
            private readonly Func<QueryContext, JObject, T> _shaper;
            private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
            private readonly Type _contextType;
            private readonly string _partitionKey;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

            public QueryingEnumerable(
                CosmosQueryContext cosmosQueryContext,
                ISqlExpressionFactory sqlExpressionFactory,
                IQuerySqlGeneratorFactory querySqlGeneratorFactory,
                SelectExpression selectExpression,
                Func<QueryContext, JObject, T> shaper,
                Type contextType,
                string partitionKey,
                IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            {
                _cosmosQueryContext = cosmosQueryContext;
                _sqlExpressionFactory = sqlExpressionFactory;
                _querySqlGeneratorFactory = querySqlGeneratorFactory;
                _selectExpression = selectExpression;
                _shaper = shaper;
                _contextType = contextType;
                _partitionKey = partitionKey;
                _logger = logger;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new AsyncEnumerator(this, cancellationToken);

            public IEnumerator<T> GetEnumerator() => new Enumerator(this);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private CosmosSqlQuery GenerateQuery()
                => _querySqlGeneratorFactory.Create().GetSqlQuery(
                    (SelectExpression)new InExpressionValuesExpandingExpressionVisitor(
                            _sqlExpressionFactory,
                            _cosmosQueryContext.ParameterValues)
                        .Visit(_selectExpression),
                    _cosmosQueryContext.ParameterValues);

            public string ToQueryString()
            {
                var sqlQuery = GenerateQuery();
                if (sqlQuery.Parameters.Count == 0)
                {
                    return sqlQuery.Query;
                }

                var builder = new StringBuilder();
                foreach (var parameter in sqlQuery.Parameters)
                {
                    builder
                        .Append("-- ")
                        .Append(parameter.Name)
                        .Append("='")
                        .Append(parameter.Value)
                        .AppendLine("'");
                }

                return builder.Append(sqlQuery.Query).ToString();
            }

            private sealed class Enumerator : IEnumerator<T>
            {
                private readonly QueryingEnumerable<T> _queryingEnumerable;
                private IEnumerator<JObject> _enumerator;

                public Enumerator(QueryingEnumerable<T> queryingEnumerable)
                {
                    _queryingEnumerable = queryingEnumerable;
                }

                public T Current { get; private set; }

                object IEnumerator.Current => Current;

                public bool MoveNext()
                {
                    try
                    {
                        using (_queryingEnumerable._cosmosQueryContext.ConcurrencyDetector.EnterCriticalSection())
                        {
                            if (_enumerator == null)
                            {
                                var sqlQuery = _queryingEnumerable.GenerateQuery();

                                _enumerator = _queryingEnumerable._cosmosQueryContext.CosmosClient
                                    .ExecuteSqlQuery(
                                        _queryingEnumerable._selectExpression.Container,
                                        _queryingEnumerable._partitionKey,
                                        sqlQuery)
                                    .GetEnumerator();
                            }

                            var hasNext = _enumerator.MoveNext();

                            Current
                                = hasNext
                                    ? _queryingEnumerable._shaper(_queryingEnumerable._cosmosQueryContext, _enumerator.Current)
                                    : default;

                            return hasNext;
                        }
                    }
                    catch (Exception exception)
                    {
                        _queryingEnumerable._logger.QueryIterationFailed(_queryingEnumerable._contextType, exception);

                        throw;
                    }
                }

                public void Dispose()
                {
                    _enumerator?.Dispose();
                    _enumerator = null;
                }

                public void Reset() => throw new NotImplementedException();
            }

            private sealed class AsyncEnumerator : IAsyncEnumerator<T>
            {
                private IAsyncEnumerator<JObject> _enumerator;
                private readonly CosmosQueryContext _cosmosQueryContext;
                private readonly SelectExpression _selectExpression;
                private readonly Func<QueryContext, JObject, T> _shaper;
                private readonly ISqlExpressionFactory _sqlExpressionFactory;
                private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
                private readonly Type _contextType;
                private readonly string _partitionKey;
                private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
                private readonly CancellationToken _cancellationToken;

                public AsyncEnumerator(QueryingEnumerable<T> queryingEnumerable, CancellationToken cancellationToken)
                {
                    _cosmosQueryContext = queryingEnumerable._cosmosQueryContext;
                    _shaper = queryingEnumerable._shaper;
                    _selectExpression = queryingEnumerable._selectExpression;
                    _sqlExpressionFactory = queryingEnumerable._sqlExpressionFactory;
                    _querySqlGeneratorFactory = queryingEnumerable._querySqlGeneratorFactory;
                    _contextType = queryingEnumerable._contextType;
                    _partitionKey = queryingEnumerable._partitionKey;
                    _logger = queryingEnumerable._logger;
                    _cancellationToken = cancellationToken;
                }

                public T Current { get; private set; }

                public async ValueTask<bool> MoveNextAsync()
                {
                    try
                    {
                        using (_cosmosQueryContext.ConcurrencyDetector.EnterCriticalSection())
                        {
                            if (_enumerator == null)
                            {
                                var selectExpression = (SelectExpression)new InExpressionValuesExpandingExpressionVisitor(
                                    _sqlExpressionFactory, _cosmosQueryContext.ParameterValues).Visit(_selectExpression);

                                _enumerator = _cosmosQueryContext.CosmosClient
                                    .ExecuteSqlQueryAsync(
                                        _selectExpression.Container,
                                        _partitionKey,
                                        _querySqlGeneratorFactory.Create().GetSqlQuery(
                                            selectExpression, _cosmosQueryContext.ParameterValues))
                                    .GetAsyncEnumerator(_cancellationToken);
                            }

                            var hasNext = await _enumerator.MoveNextAsync();

                            Current
                                = hasNext
                                    ? _shaper(_cosmosQueryContext, _enumerator.Current)
                                    : default;

                            return hasNext;
                        }
                    }
                    catch (Exception exception)
                    {
                        _logger.QueryIterationFailed(_contextType, exception);

                        throw;
                    }
                }

                public ValueTask DisposeAsync()
                {
                    _enumerator?.DisposeAsync();
                    _enumerator = null;

                    return default;
                }
            }
        }
    }
}
