// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerShapedQueryOptimizerFactory : RelationalShapedQueryOptimizerFactory
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerShapedQueryOptimizerFactory(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public override ShapedQueryOptimizer Create(QueryCompilationContext2 queryCompilationContext)
        {
            return new SqlServerShapedQueryOptimizer(queryCompilationContext, _sqlExpressionFactory);
        }
    }
}
