// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalShapedQueryOptimizer : ShapedQueryOptimizer
    {
        private readonly QueryCompilationContext2 _queryCompilationContext;

        public RelationalShapedQueryOptimizer(
            QueryCompilationContext2 queryCompilationContext,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _queryCompilationContext = queryCompilationContext;
            SqlExpressionFactory = sqlExpressionFactory;
        }

        protected ISqlExpressionFactory SqlExpressionFactory { get; private set; }

        public override Expression Visit(Expression query)
        {
            query = base.Visit(query);
            query = new SelectExpressionProjectionApplyingExpressionVisitor().Visit(query);
            query = new SelectExpressionTableAliasUniquifyingExpressionVisitor().Visit(query);

            if (!RelationalOptionsExtension.Extract(_queryCompilationContext.ContextOptions).UseRelationalNulls)
            {
                query = new NullSemanticsRewritingVisitor(SqlExpressionFactory).Visit(query);
            }

            query = new SqlExpressionOptimizingVisitor(SqlExpressionFactory).Visit(query);
            query = new NullComparisonTransformingExpressionVisitor().Visit(query);

            if (query is ShapedQueryExpression shapedQueryExpression)
            {
                shapedQueryExpression.ShaperExpression
                    = new ShaperExpressionProcessingExpressionVisitor((SelectExpression)shapedQueryExpression.QueryExpression)
                        .Inject(shapedQueryExpression.ShaperExpression);
            }

            return query;
        }
    }
}
