// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class RelationalQueryTranslationPostprocessor : QueryTranslationPostprocessor
    {
        private readonly SqlExpressionOptimizingExpressionVisitor _sqlExpressionOptimizingExpressionVisitor;

        public RelationalQueryTranslationPostprocessor(
            [NotNull] QueryTranslationPostprocessorDependencies dependencies,
            [NotNull] RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(dependencies)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            RelationalDependencies = relationalDependencies;
            UseRelationalNulls = RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).UseRelationalNulls;
            SqlExpressionFactory = relationalDependencies.SqlExpressionFactory;
            _sqlExpressionOptimizingExpressionVisitor
                = new SqlExpressionOptimizingExpressionVisitor(SqlExpressionFactory, UseRelationalNulls);
        }

        protected virtual RelationalQueryTranslationPostprocessorDependencies RelationalDependencies { get; }

        protected virtual ISqlExpressionFactory SqlExpressionFactory { get; }

        protected virtual bool UseRelationalNulls { get; }

        public override Expression Process(Expression query)
        {
            query = base.Process(query);
            query = new SelectExpressionProjectionApplyingExpressionVisitor().Visit(query);
            query = new CollectionJoinApplyingExpressionVisitor().Visit(query);
            query = new TableAliasUniquifyingExpressionVisitor().Visit(query);
            query = new CaseWhenFlatteningExpressionVisitor(SqlExpressionFactory).Visit(query);

            if (!UseRelationalNulls)
            {
                query = new NullSemanticsRewritingExpressionVisitor(SqlExpressionFactory).Visit(query);
            }

            query = OptimizeSqlExpression(query);

            return query;
        }

        protected virtual Expression OptimizeSqlExpression([NotNull] Expression query)
        {
            Check.NotNull(query, nameof(query));

            return _sqlExpressionOptimizingExpressionVisitor.Visit(query);
        }
    }
}
