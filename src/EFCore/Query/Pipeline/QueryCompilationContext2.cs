// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class QueryCompilationContext
    {
        public static readonly ParameterExpression QueryContextParameter = Expression.Parameter(typeof(QueryContext), "queryContext");

        private readonly IQueryOptimizerFactory _queryOptimizerFactory;
        private readonly IEntityQueryableTranslatorFactory _entityQueryableTranslatorFactory;
        private readonly IQueryableMethodTranslatingExpressionVisitorFactory _queryableMethodTranslatingExpressionVisitorFactory;
        private readonly IShapedQueryOptimizerFactory _shapedQueryOptimizerFactory;
        private readonly IShapedQueryCompilingExpressionVisitorFactory _shapedQueryCompilingExpressionVisitorFactory;

        public QueryCompilationContext(
            IModel model,
            IQueryOptimizerFactory queryOptimizerFactory,
            IEntityQueryableTranslatorFactory entityQuerableTranslatorFactory,
            IQueryableMethodTranslatingExpressionVisitorFactory queryableMethodTranslatingExpressionVisitorFactory,
            IShapedQueryOptimizerFactory shapedQueryOptimizerFactory,
            IShapedQueryCompilingExpressionVisitorFactory shapedQueryCompilingExpressionVisitorFactory,
            ICurrentDbContext currentDbContext,
            IDbContextOptions contextOptions,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            bool async)
        {
            Async = async;
            TrackQueryResults = currentDbContext.Context.ChangeTracker.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll;
            Model = model;
            ContextOptions = contextOptions;
            ContextType = currentDbContext.Context.GetType();
            Logger = logger;

            _queryOptimizerFactory = queryOptimizerFactory;
            _entityQueryableTranslatorFactory = entityQuerableTranslatorFactory;
            _queryableMethodTranslatingExpressionVisitorFactory = queryableMethodTranslatingExpressionVisitorFactory;
            _shapedQueryOptimizerFactory = shapedQueryOptimizerFactory;
            _shapedQueryCompilingExpressionVisitorFactory = shapedQueryCompilingExpressionVisitorFactory;

        }

        public bool Async { get; }
        public IModel Model { get; }
        public IDbContextOptions ContextOptions { get; }
        public bool TrackQueryResults { get; internal set; }
        public virtual IDiagnosticsLogger<DbLoggerCategory.Query> Logger { get; }
        public virtual Type ContextType { get; }

        public virtual Func<QueryContext, TResult> CreateQueryExecutor<TResult>(Expression query)
        {
            query = _queryOptimizerFactory.Create(this).Visit(query);
            // Convert EntityQueryable to ShapedQueryExpression
            query = _entityQueryableTranslatorFactory.Create(this).Visit(query);
            query = _queryableMethodTranslatingExpressionVisitorFactory.Create(Model).Visit(query);
            query = _shapedQueryOptimizerFactory.Create(this).Visit(query);

            // Inject actual entity materializer
            // Inject tracking
            query = _shapedQueryCompilingExpressionVisitorFactory.Create(this).Visit(query);

            var queryExecutorExpression = Expression.Lambda<Func<QueryContext, TResult>>(
                query,
                QueryContextParameter);

            try
            {
                return queryExecutorExpression.Compile();
            }
            finally
            {
                Logger.QueryExecutionPlanned(new ExpressionPrinter(), queryExecutorExpression);
            }
        }
    }
}
