// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class RelationalShapedQueryOptimizerFactory : IShapedQueryOptimizerFactory
    {
        protected virtual ISqlExpressionFactory SqlExpressionFactory { get; private set; }

        public RelationalShapedQueryOptimizerFactory(ISqlExpressionFactory sqlExpressionFactory)
        {
            SqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual ShapedQueryOptimizer Create(QueryCompilationContext queryCompilationContext)
        {
            return new RelationalShapedQueryOptimizer(queryCompilationContext, SqlExpressionFactory);
        }
    }
}
