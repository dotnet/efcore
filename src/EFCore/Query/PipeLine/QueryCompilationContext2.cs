// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class QueryCompilationContext2
    {
        public static readonly ParameterExpression QueryContextParameter = Expression.Parameter(typeof(QueryContext), "queryContext");

        private readonly IQueryOptimizingExpressionVisitorsFactory _queryOptimizingExpressionVisitorsFactory;
        private readonly IEntityQueryableExpressionVisitorsFactory _entityQueryableExpressionVisitorsFactory;
        private readonly IQueryableMethodTranslatingExpressionVisitorFactory _queryableMethodTranslatingExpressionVisitorFactory;
        private readonly IShapedQueryOptimizingExpressionVisitorsFactory _shapedQueryOptimizingExpressionVisitorsFactory;
        private readonly IShapedQueryCompilingExpressionVisitorFactory _shapedQueryCompilingExpressionVisitorFactory;

        public QueryCompilationContext2(
            IQueryOptimizingExpressionVisitorsFactory queryOptimizingExpressionVisitorsFactory,
            IEntityQueryableExpressionVisitorsFactory entityQueryableExpressionVisitorsFactory,
            IQueryableMethodTranslatingExpressionVisitorFactory queryableMethodTranslatingExpressionVisitorFactory,
            IShapedQueryOptimizingExpressionVisitorsFactory shapedQueryOptimizingExpressionVisitorsFactory,
            IShapedQueryCompilingExpressionVisitorFactory shapedQueryCompilingExpressionVisitorFactory)
        {
            _queryOptimizingExpressionVisitorsFactory = queryOptimizingExpressionVisitorsFactory;
            _entityQueryableExpressionVisitorsFactory = entityQueryableExpressionVisitorsFactory;
            _queryableMethodTranslatingExpressionVisitorFactory = queryableMethodTranslatingExpressionVisitorFactory;
            _shapedQueryOptimizingExpressionVisitorsFactory = shapedQueryOptimizingExpressionVisitorsFactory;
            _shapedQueryCompilingExpressionVisitorFactory = shapedQueryCompilingExpressionVisitorFactory;
        }

        public bool Async { get; internal set; }

        public virtual Func<QueryContext, TResult> CreateQueryExecutor<TResult>(Expression query)
        {
            foreach (var visitor in _queryOptimizingExpressionVisitorsFactory.Create(this).GetVisitors())
            {
                query = visitor.Visit(query);
            }

            // Convert EntityQueryable to ShapedQueryExpression
            foreach (var visitor in _entityQueryableExpressionVisitorsFactory.Create(this).GetVisitors())
            {
                query = visitor.Visit(query);
            }

            query = _queryableMethodTranslatingExpressionVisitorFactory.Create(this).Visit(query);

            foreach (var visitor in _shapedQueryOptimizingExpressionVisitorsFactory.Create(this).GetVisitors())
            {
                query = visitor.Visit(query);
            }

            // Inject actual entity materializer
            // Inject tracking
            query = _shapedQueryCompilingExpressionVisitorFactory.Create(this).Visit(query);

            return Expression.Lambda<Func<QueryContext, TResult>>(
                query,
                QueryContextParameter)
                .Compile();
        }
    }
}
