// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class QueryCompilationContextFactory2 : IQueryCompilationContextFactory2
    {
        private readonly IQueryOptimizingExpressionVisitorsFactory _queryOptimizingExpressionVisitorsFactory;
        private readonly IEntityQueryableExpressionVisitorsFactory _entityQueryableExpressionVisitorsFactory;
        private readonly IQueryableMethodTranslatingExpressionVisitorFactory _queryableMethodTranslatingExpressionVisitorFactory;
        private readonly IShapedQueryOptimizingExpressionVisitorsFactory _shapedQueryOptimizingExpressionVisitorsFactory;
        private readonly IShapedQueryCompilingExpressionVisitorFactory _shapedQueryCompilingExpressionVisitorFactory;

        public QueryCompilationContextFactory2(
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

        public QueryCompilationContext2 Create(bool async)
        {
            var queryCompilationContext = new QueryCompilationContext2(
                _queryOptimizingExpressionVisitorsFactory,
                _entityQueryableExpressionVisitorsFactory,
                _queryableMethodTranslatingExpressionVisitorFactory,
                _shapedQueryOptimizingExpressionVisitorsFactory,
                _shapedQueryCompilingExpressionVisitorFactory)
            {
                Async = async
            };

            return queryCompilationContext;
        }
    }
}
