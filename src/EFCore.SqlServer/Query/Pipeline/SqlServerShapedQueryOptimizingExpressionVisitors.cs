// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerShapedQueryOptimizer : RelationalShapedQueryOptimizer
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerShapedQueryOptimizer(
            QueryCompilationContext2 queryCompilationContext,
            ISqlExpressionFactory sqlExpressionFactory)
            : base(queryCompilationContext)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public override Expression Visit(Expression query)
        {
            query = base.Visit(query);
            query = new SearchConditionConvertingExpressionVisitor(_sqlExpressionFactory).Visit(query);

            return query;
        }
    }
}
