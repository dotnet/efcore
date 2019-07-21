// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerShapedQueryOptimizerFactory : IShapedQueryOptimizerFactory
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerShapedQueryOptimizerFactory(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual ShapedQueryOptimizer Create(QueryCompilationContext queryCompilationContext)
        {
            return new SqlServerShapedQueryOptimizer(queryCompilationContext, _sqlExpressionFactory);
        }
    }
}
