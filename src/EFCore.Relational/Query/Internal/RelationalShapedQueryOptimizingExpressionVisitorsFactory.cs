// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class RelationalShapedQueryOptimizerFactory : IShapedQueryOptimizerFactory
    {
        private readonly ShapedQueryOptimizerDependencies _dependencies;
        private readonly RelationalShapedQueryOptimizerDependencies _relationalDependencies;

        public RelationalShapedQueryOptimizerFactory(
            ShapedQueryOptimizerDependencies dependencies,
            RelationalShapedQueryOptimizerDependencies relationalDependencies,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _dependencies = dependencies;
            _relationalDependencies = relationalDependencies;
            SqlExpressionFactory = sqlExpressionFactory;
        }

        protected virtual ISqlExpressionFactory SqlExpressionFactory { get; }

        public virtual ShapedQueryOptimizer Create(QueryCompilationContext queryCompilationContext)
        {
            return new RelationalShapedQueryOptimizer(
                _dependencies,
                _relationalDependencies,
                queryCompilationContext);
        }
    }
}
