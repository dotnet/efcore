// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerShapedQueryOptimizerFactory : IShapedQueryOptimizerFactory
    {
        private readonly SqlExpressionVisitorDependencies _sqlExpressionVisitorDependencies;
        private readonly ShapedQueryOptimizerDependencies _dependencies;
        private readonly RelationalShapedQueryOptimizerDependencies _relationalDependencies;

        public SqlServerShapedQueryOptimizerFactory(
            SqlExpressionVisitorDependencies sqlExpressionVisitorDependencies,
            ShapedQueryOptimizerDependencies dependencies,
            RelationalShapedQueryOptimizerDependencies relationalDependencies)
        {
            _sqlExpressionVisitorDependencies = sqlExpressionVisitorDependencies;
            _dependencies = dependencies;
            _relationalDependencies = relationalDependencies;
        }

        public virtual ShapedQueryOptimizer Create(QueryCompilationContext queryCompilationContext)
            => new SqlServerShapedQueryOptimizer(
                _dependencies,
                _relationalDependencies,
                _sqlExpressionVisitorDependencies,
                queryCompilationContext);
    }
}
