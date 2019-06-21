// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class CosmosShapedQueryCompilingExpressionVisitorFactory : IShapedQueryCompilingExpressionVisitorFactory
    {
        private readonly IEntityMaterializerSource _entityMaterializerSource;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;

        public CosmosShapedQueryCompilingExpressionVisitorFactory(IEntityMaterializerSource entityMaterializerSource,
            ISqlExpressionFactory sqlExpressionFactory,
            IQuerySqlGeneratorFactory querySqlGeneratorFactory)
        {
            _entityMaterializerSource = entityMaterializerSource;
            _sqlExpressionFactory = sqlExpressionFactory;
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
        }

        public ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
        {
            return new CosmosShapedQueryCompilingExpressionVisitor(
                _entityMaterializerSource,
                _sqlExpressionFactory,
                _querySqlGeneratorFactory,
                queryCompilationContext.ContextType,
                queryCompilationContext.Logger,
                queryCompilationContext.TrackQueryResults,
                queryCompilationContext.Async);
        }
    }
}
