// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <inheritdoc />
    public class RelationalQueryTranslationPostprocessor : QueryTranslationPostprocessor
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="RelationalQueryTranslationPostprocessor" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this class. </param>
        /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this class. </param>
        /// <param name="queryCompilationContext"> The query compilation context object to use. </param>
        public RelationalQueryTranslationPostprocessor(
            [NotNull] QueryTranslationPostprocessorDependencies dependencies,
            [NotNull] RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(dependencies, queryCompilationContext)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     Parameter object containing relational service dependencies.
        /// </summary>
        protected virtual RelationalQueryTranslationPostprocessorDependencies RelationalDependencies { get; }

        /// <inheritdoc />
        public override Expression Process(Expression query)
        {
            query = base.Process(query);
            query = new SelectExpressionProjectionApplyingExpressionVisitor().Visit(query);
            query = new CollectionJoinApplyingExpressionVisitor((RelationalQueryCompilationContext)QueryCompilationContext).Visit(query);
            query = new TableAliasUniquifyingExpressionVisitor().Visit(query);
            query = new SelectExpressionPruningExpressionVisitor().Visit(query);
            query = new SqlExpressionSimplifyingExpressionVisitor(RelationalDependencies.SqlExpressionFactory).Visit(query);
            query = new RelationalValueConverterCompensatingExpressionVisitor(RelationalDependencies.SqlExpressionFactory).Visit(query);

#pragma warning disable 618
            query = OptimizeSqlExpression(query);
#pragma warning restore 618

            return query;
        }

        /// <summary>
        ///     Optimizes the SQL expression.
        /// </summary>
        /// <param name="query"> An expression to optimize. </param>
        /// <returns> An expression which has SQL optimized. </returns>
        [Obsolete(
            "Use 'Optimize' method on "
            + nameof(RelationalParameterBasedSqlProcessor)
            + " instead. If you have a case for optimizations to be performed here, please file an issue on github.com/dotnet/efcore.")]
        protected virtual Expression OptimizeSqlExpression([NotNull] Expression query)
            => query;
    }
}
