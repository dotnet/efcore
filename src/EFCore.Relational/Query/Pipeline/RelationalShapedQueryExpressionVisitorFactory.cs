// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalShapedQueryCompilingExpressionVisitorFactory : IShapedQueryCompilingExpressionVisitorFactory
    {
        private readonly IEntityMaterializerSource _entityMaterializerSource;
        private readonly IQuerySqlGeneratorFactory2 _querySqlGeneratorFactory;

        public RelationalShapedQueryCompilingExpressionVisitorFactory(IEntityMaterializerSource entityMaterializerSource,
            IQuerySqlGeneratorFactory2 querySqlGeneratorFactory)
        {
            _entityMaterializerSource = entityMaterializerSource;
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
        }

        public ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext2 queryCompilationContext)
        {
            return new RelationalShapedQueryCompilingExpressionVisitor(
                _entityMaterializerSource,
                _querySqlGeneratorFactory,
                queryCompilationContext.TrackQueryResults,
                queryCompilationContext.Async);
        }
    }
}
