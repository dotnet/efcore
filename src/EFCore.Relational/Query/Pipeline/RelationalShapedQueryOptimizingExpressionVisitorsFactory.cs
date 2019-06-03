// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalShapedQueryOptimizerFactory : ShapedQueryOptimizerFactory
    {
        protected ISqlExpressionFactory SqlExpressionFactory { get; private set; }

        public RelationalShapedQueryOptimizerFactory(ISqlExpressionFactory sqlExpressionFactory)
        {
            SqlExpressionFactory = sqlExpressionFactory;
        }

        public override ShapedQueryOptimizer Create(QueryCompilationContext queryCompilationContext)
        {
            return new RelationalShapedQueryOptimizer(queryCompilationContext, SqlExpressionFactory);
        }
    }
}
