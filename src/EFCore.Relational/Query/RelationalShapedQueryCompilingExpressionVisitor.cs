// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <inheritdoc />
    public partial class RelationalShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
    {
        private readonly Type _contextType;
        private readonly ISet<string> _tags;
        private readonly bool _concurrencyDetectionEnabled;
        private readonly bool _detailedErrorsEnabled;
        private readonly bool _useRelationalNulls;

        /// <summary>
        ///     Creates a new instance of the <see cref="ShapedQueryCompilingExpressionVisitor" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this class. </param>
        /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this class. </param>
        /// <param name="queryCompilationContext"> The query compilation context object to use. </param>
        public RelationalShapedQueryCompilingExpressionVisitor(
            [NotNull] ShapedQueryCompilingExpressionVisitorDependencies dependencies,
            [NotNull] RelationalShapedQueryCompilingExpressionVisitorDependencies relationalDependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(dependencies, queryCompilationContext)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            RelationalDependencies = relationalDependencies;

            _contextType = queryCompilationContext.ContextType;
            _tags = queryCompilationContext.Tags;
            _concurrencyDetectionEnabled = dependencies.CoreSingletonOptions.IsConcurrencyDetectionEnabled;
            _detailedErrorsEnabled = dependencies.CoreSingletonOptions.AreDetailedErrorsEnabled;
            _useRelationalNulls = RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).UseRelationalNulls;
        }

        /// <summary>
        ///     Parameter object containing relational service dependencies.
        /// </summary>
        protected virtual RelationalShapedQueryCompilingExpressionVisitorDependencies RelationalDependencies { get; }

        /// <inheritdoc />
        protected override Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
        {
            Check.NotNull(shapedQueryExpression, nameof(shapedQueryExpression));

            var selectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;

            VerifyNoClientConstant(shapedQueryExpression.ShaperExpression);
            var nonComposedFromSql = selectExpression.IsNonComposedFromSql();
            var splitQuery = ((RelationalQueryCompilationContext)QueryCompilationContext).QuerySplittingBehavior
                == QuerySplittingBehavior.SplitQuery;
            var shaper = new ShaperProcessingExpressionVisitor(this, selectExpression, _tags, splitQuery, nonComposedFromSql).ProcessShaper(
                shapedQueryExpression.ShaperExpression, out var relationalCommandCache, out var relatedDataLoaders);

            if (nonComposedFromSql)
            {
                return Expression.New(
                    typeof(FromSqlQueryingEnumerable<>).MakeGenericType(shaper.ReturnType).GetConstructors()[0],
                    Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                    Expression.Constant(relationalCommandCache),
                    Expression.Constant(
                        selectExpression.Projection.Select(pe => ((ColumnExpression)pe.Expression).Name).ToList(),
                        typeof(IReadOnlyList<string>)),
                    Expression.Constant(shaper.Compile()),
                    Expression.Constant(_contextType),
                    Expression.Constant(
                        QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                    Expression.Constant(_detailedErrorsEnabled),
                    Expression.Constant(_concurrencyDetectionEnabled));
            }

            if (splitQuery)
            {
                var relatedDataLoadersParameter = Expression.Constant(
                    QueryCompilationContext.IsAsync ? null : relatedDataLoaders?.Compile(),
                    typeof(Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>));

                var relatedDataLoadersAsyncParameter = Expression.Constant(
                    QueryCompilationContext.IsAsync ? relatedDataLoaders?.Compile() : null,
                    typeof(Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task>));

                return Expression.New(
                    typeof(SplitQueryingEnumerable<>).MakeGenericType(shaper.ReturnType).GetConstructors().Single(),
                    Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                    Expression.Constant(relationalCommandCache),
                    Expression.Constant(shaper.Compile()),
                    relatedDataLoadersParameter,
                    relatedDataLoadersAsyncParameter,
                    Expression.Constant(_contextType),
                    Expression.Constant(
                        QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                    Expression.Constant(_detailedErrorsEnabled),
                    Expression.Constant(_concurrencyDetectionEnabled));
            }

            return Expression.New(
                typeof(SingleQueryingEnumerable<>).MakeGenericType(shaper.ReturnType).GetConstructors()[0],
                Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                Expression.Constant(relationalCommandCache),
                Expression.Constant(shaper.Compile()),
                Expression.Constant(_contextType),
                Expression.Constant(
                    QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                Expression.Constant(_detailedErrorsEnabled),
                Expression.Constant(_concurrencyDetectionEnabled));
        }
    }
}
