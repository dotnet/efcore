// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class RelationalShapedQueryOptimizer : ShapedQueryOptimizer
    {
        public RelationalShapedQueryOptimizer(
            ShapedQueryOptimizerDependencies dependencies,
            RelationalShapedQueryOptimizerDependencies relationalDependencies,
            QueryCompilationContext queryCompilationContext)
            : base(dependencies)
        {
            RelationalDependencies = relationalDependencies;
            UseRelationalNulls = RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).UseRelationalNulls;
            SqlExpressionFactory = relationalDependencies.SqlExpressionFactory;
        }

        protected virtual RelationalShapedQueryOptimizerDependencies RelationalDependencies { get; }

        protected virtual ISqlExpressionFactory SqlExpressionFactory { get; }

        protected virtual bool UseRelationalNulls { get; }

        public override Expression Visit(Expression query)
        {
            query = base.Visit(query);
            query = new SelectExpressionProjectionApplyingExpressionVisitor().Visit(query);
            query = new CollectionJoinApplyingExpressionVisitor().Visit(query);
            query = new SelectExpressionTableAliasUniquifyingExpressionVisitor().Visit(query);

            if (!UseRelationalNulls)
            {
                query = new NullSemanticsRewritingVisitor(SqlExpressionFactory).Visit(query);
            }

            query = new SqlExpressionOptimizingVisitor(SqlExpressionFactory, UseRelationalNulls).Visit(query);
            query = new NullComparisonTransformingExpressionVisitor().Visit(query);

            return query;
        }
    }
}
