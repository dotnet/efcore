// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class QueryCompilationContextFactory : IQueryCompilationContextFactory
    {
        private readonly IModel _model;
        private readonly IQueryOptimizerFactory _queryOptimizerFactory;
        private readonly IQueryableMethodTranslatingExpressionVisitorFactory _queryableMethodTranslatingExpressionVisitorFactory;
        private readonly IShapedQueryOptimizerFactory _shapedQueryOptimizerFactory;
        private readonly IShapedQueryCompilingExpressionVisitorFactory _shapedQueryCompilingExpressionVisitorFactory;
        private readonly ICurrentDbContext _currentContext;
        private readonly IDbContextOptions _contextOptions;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

        public QueryCompilationContextFactory(
            IModel model,
            IQueryOptimizerFactory queryOptimizerFactory,
            IQueryableMethodTranslatingExpressionVisitorFactory queryableMethodTranslatingExpressionVisitorFactory,
            IShapedQueryOptimizerFactory shapedQueryOptimizerFactory,
            IShapedQueryCompilingExpressionVisitorFactory shapedQueryCompilingExpressionVisitorFactory,
            ICurrentDbContext currentContext,
            IDbContextOptions contextOptions,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            _model = model;
            _queryOptimizerFactory = queryOptimizerFactory;
            _queryableMethodTranslatingExpressionVisitorFactory = queryableMethodTranslatingExpressionVisitorFactory;
            _shapedQueryOptimizerFactory = shapedQueryOptimizerFactory;
            _shapedQueryCompilingExpressionVisitorFactory = shapedQueryCompilingExpressionVisitorFactory;
            _currentContext = currentContext;
            _contextOptions = contextOptions;
            _logger = logger;
        }

        public QueryCompilationContext Create(bool async)
            => new QueryCompilationContext(
                _model,
                _queryOptimizerFactory,
                _queryableMethodTranslatingExpressionVisitorFactory,
                _shapedQueryOptimizerFactory,
                _shapedQueryCompilingExpressionVisitorFactory,
                _currentContext,
                _contextOptions,
                _logger,
                async);
    }
}
