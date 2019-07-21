// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class RelationalShapedQueryCompilingExpressionVisitorFactory : IShapedQueryCompilingExpressionVisitorFactory
    {
        private readonly IEntityMaterializerSource _entityMaterializerSource;
        private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;

        public RelationalShapedQueryCompilingExpressionVisitorFactory(
            IEntityMaterializerSource entityMaterializerSource,
            IQuerySqlGeneratorFactory querySqlGeneratorFactory,
            ISqlExpressionFactory sqlExpressionFactory,
            IParameterNameGeneratorFactory parameterNameGeneratorFactory)
        {
            _entityMaterializerSource = entityMaterializerSource;
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
            _sqlExpressionFactory = sqlExpressionFactory;
            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
        }

        public virtual ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
        {
            return new RelationalShapedQueryCompilingExpressionVisitor(
                queryCompilationContext,
                _entityMaterializerSource,
                _querySqlGeneratorFactory,
                _sqlExpressionFactory,
                _parameterNameGeneratorFactory);
        }
    }
}
