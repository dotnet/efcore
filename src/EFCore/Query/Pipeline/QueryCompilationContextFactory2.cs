// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class QueryCompilationContextFactory2 : IQueryCompilationContextFactory2
    {
        private readonly IModel _model;
        private readonly IQueryOptimizerFactory _queryOptimizerFactory;
        private readonly IEntityQueryableTranslatorFactory _entityQueryableTranslatorFactory;
        private readonly IQueryableMethodTranslatingExpressionVisitorFactory _queryableMethodTranslatingExpressionVisitorFactory;
        private readonly IShapedQueryOptimizerFactory _shapedQueryOptimizerFactory;
        private readonly IShapedQueryCompilingExpressionVisitorFactory _shapedQueryCompilingExpressionVisitorFactory;
        private readonly ICurrentDbContext _currentDbContext;

        public QueryCompilationContextFactory2(
            IModel model,
            IQueryOptimizerFactory queryOptimizerFactory,
            IEntityQueryableTranslatorFactory entityQueryableTranslatorFactory,
            IQueryableMethodTranslatingExpressionVisitorFactory queryableMethodTranslatingExpressionVisitorFactory,
            IShapedQueryOptimizerFactory shapedQueryOptimizerFactory,
            IShapedQueryCompilingExpressionVisitorFactory shapedQueryCompilingExpressionVisitorFactory,
            ICurrentDbContext currentDbContext)
        {
            _model = model;
            _queryOptimizerFactory = queryOptimizerFactory;
            _entityQueryableTranslatorFactory = entityQueryableTranslatorFactory;
            _queryableMethodTranslatingExpressionVisitorFactory = queryableMethodTranslatingExpressionVisitorFactory;
            _shapedQueryOptimizerFactory = shapedQueryOptimizerFactory;
            _shapedQueryCompilingExpressionVisitorFactory = shapedQueryCompilingExpressionVisitorFactory;
            _currentDbContext = currentDbContext;
        }

        public QueryCompilationContext2 Create(bool async)
        {
            var queryCompilationContext = new QueryCompilationContext2(
                _model,
                _queryOptimizerFactory,
                _entityQueryableTranslatorFactory,
                _queryableMethodTranslatingExpressionVisitorFactory,
                _shapedQueryOptimizerFactory,
                _shapedQueryCompilingExpressionVisitorFactory,
                _currentDbContext,
                async);

            return queryCompilationContext;
        }
    }
}
