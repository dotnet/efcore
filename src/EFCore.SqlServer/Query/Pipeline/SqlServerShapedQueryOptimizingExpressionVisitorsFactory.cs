// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerShapedQueryOptimizerFactory : RelationalShapedQueryOptimizerFactory
    {
        public SqlServerShapedQueryOptimizerFactory(ISqlExpressionFactory sqlExpressionFactory)
            : base(sqlExpressionFactory)
        {
        }

        public override ShapedQueryOptimizer Create(QueryCompilationContext queryCompilationContext)
        {
            return new SqlServerShapedQueryOptimizer(queryCompilationContext, SqlExpressionFactory);
        }
    }
}
