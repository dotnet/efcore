// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerShapedQueryOptimizer : RelationalShapedQueryOptimizer
    {
        private readonly SqlExpressionVisitorDependencies _sqlExpressionVisitorDependencies;

        public SqlServerShapedQueryOptimizer(
            ShapedQueryOptimizerDependencies dependencies,
            RelationalShapedQueryOptimizerDependencies relationalDependencies,
            SqlExpressionVisitorDependencies sqlExpressionVisitorDependencies,
            QueryCompilationContext queryCompilationContext)
            : base(dependencies, relationalDependencies, queryCompilationContext)
        {
            _sqlExpressionVisitorDependencies = sqlExpressionVisitorDependencies;
        }

        public override Expression Visit(Expression query)
        {
            query = base.Visit(query);
            query = new SearchConditionConvertingExpressionVisitor(_sqlExpressionVisitorDependencies, SqlExpressionFactory).Visit(query);
            query = new SqlExpressionOptimizingVisitor(SqlExpressionFactory, UseRelationalNulls).Visit(query);

            return query;
        }
    }
}
