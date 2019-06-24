// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class QueryCompilationContextFactory : IQueryCompilationContextFactory
    {
        private readonly IModel _model;
        private readonly IQueryOptimizerFactory _queryOptimizerFactory;
        private readonly IQueryableMethodTranslatingExpressionVisitorFactory _queryableMethodTranslatingExpressionVisitorFactory;
        private readonly IShapedQueryOptimizerFactory _shapedQueryOptimizerFactory;
        private readonly IShapedQueryCompilingExpressionVisitorFactory _shapedQueryCompilingExpressionVisitorFactory;
        private readonly ICurrentDbContext _currentDbContext;
        private readonly IDbContextOptions _contextOptions;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
        private readonly IEvaluatableExpressionFilter _evaluatableExpressionFilter;

        public QueryCompilationContextFactory(
            IModel model,
            IQueryOptimizerFactory queryOptimizerFactory,
            IQueryableMethodTranslatingExpressionVisitorFactory queryableMethodTranslatingExpressionVisitorFactory,
            IShapedQueryOptimizerFactory shapedQueryOptimizerFactory,
            IShapedQueryCompilingExpressionVisitorFactory shapedQueryCompilingExpressionVisitorFactory,
            ICurrentDbContext currentDbContext,
            IDbContextOptions contextOptions,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            IEvaluatableExpressionFilter evaluatableExpressionFilter)
        {
            _model = model;
            _queryOptimizerFactory = queryOptimizerFactory;
            _queryableMethodTranslatingExpressionVisitorFactory = queryableMethodTranslatingExpressionVisitorFactory;
            _shapedQueryOptimizerFactory = shapedQueryOptimizerFactory;
            _shapedQueryCompilingExpressionVisitorFactory = shapedQueryCompilingExpressionVisitorFactory;
            _currentDbContext = currentDbContext;
            _contextOptions = contextOptions;
            _logger = logger;
            _evaluatableExpressionFilter = evaluatableExpressionFilter;
        }

        public QueryCompilationContext Create(bool async)
        {
            var queryCompilationContext = new QueryCompilationContext(
                _model,
                _queryOptimizerFactory,
                _queryableMethodTranslatingExpressionVisitorFactory,
                _shapedQueryOptimizerFactory,
                _shapedQueryCompilingExpressionVisitorFactory,
                _currentDbContext,
                _contextOptions,
                _logger,
                _evaluatableExpressionFilter,
                async);

            return queryCompilationContext;
        }
    }
}
