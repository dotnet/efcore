// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class RelationalShapedQueryOptimizer : ShapedQueryOptimizer
    {
        private readonly SqlExpressionOptimizingExpressionVisitor _sqlExpressionOptimizingExpressionVisitor;

        public RelationalShapedQueryOptimizer(
            ShapedQueryOptimizerDependencies dependencies,
            RelationalShapedQueryOptimizerDependencies relationalDependencies,
            QueryCompilationContext queryCompilationContext)
            : base(dependencies)
        {
            RelationalDependencies = relationalDependencies;
            UseRelationalNulls = RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).UseRelationalNulls;
            SqlExpressionFactory = relationalDependencies.SqlExpressionFactory;
            _sqlExpressionOptimizingExpressionVisitor
                = new SqlExpressionOptimizingExpressionVisitor(SqlExpressionFactory, UseRelationalNulls);
        }

        protected virtual RelationalShapedQueryOptimizerDependencies RelationalDependencies { get; }

        protected virtual ISqlExpressionFactory SqlExpressionFactory { get; }

        protected virtual bool UseRelationalNulls { get; }

        public override Expression Visit(Expression query)
        {
            query = base.Visit(query);
            query = new SelectExpressionProjectionApplyingExpressionVisitor().Visit(query);
            query = new CollectionJoinApplyingExpressionVisitor().Visit(query);
            query = new TableAliasUniquifyingExpressionVisitor().Visit(query);

            if (!UseRelationalNulls)
            {
                query = new NullSemanticsRewritingExpressionVisitor(SqlExpressionFactory).Visit(query);
            }

            query = OptimizeSqlExpression(query);
            query = new NullComparisonTransformingExpressionVisitor().Visit(query);

            return query;
        }

        protected virtual Expression OptimizeSqlExpression(Expression query) => _sqlExpressionOptimizingExpressionVisitor.Visit(query);
    }
}
