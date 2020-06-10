// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <inheritdoc />
    public partial class RelationalShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
    {
        private readonly Type _contextType;
        private readonly ISet<string> _tags;
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
            _detailedErrorsEnabled = relationalDependencies.CoreSingletonOptions.AreDetailedErrorsEnabled;
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
            selectExpression.ApplyTags(_tags);

            if (selectExpression.IsNonComposedFromSql())
            {
                var dataReaderParameter = Expression.Parameter(typeof(DbDataReader), "dataReader");
                var indexMapParameter = Expression.Parameter(typeof(int[]), "indexMap");

                Expression shaper = Expression.Lambda(
                        shapedQueryExpression.ShaperExpression,
                        QueryCompilationContext.QueryContextParameter,
                        dataReaderParameter,
                        indexMapParameter);
                shaper = InjectEntityMaterializers(shaper);

                shaper = new RelationalProjectionBindingRemovingExpressionVisitor(
                        selectExpression,
                        dataReaderParameter,
                        indexMapParameter,
                        _detailedErrorsEnabled,
                        QueryCompilationContext.IsBuffering)
                    .Visit(shaper, out var projectionColumns);

                var relationalCommandCache = new RelationalCommandCache(
                    Dependencies.MemoryCache,
                    RelationalDependencies.QuerySqlGeneratorFactory,
                    RelationalDependencies.RelationalParameterBasedSqlProcessorFactory,
                    selectExpression,
                    projectionColumns,
                    _useRelationalNulls);

                var shaperLambda = (LambdaExpression)shaper;
                var columnNames = selectExpression.Projection.Select(pe => ((ColumnExpression)pe.Expression).Name).ToList();

                return Expression.New(
                    typeof(FromSqlQueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                    Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                    Expression.Constant(relationalCommandCache),
                    Expression.Constant(columnNames, typeof(IReadOnlyList<string>)),
                    Expression.Constant(shaperLambda.Compile()),
                    Expression.Constant(_contextType),
                    Expression.Constant(QueryCompilationContext.PerformIdentityResolution));
            }
            else
            {
                var dataReaderParameter = Expression.Parameter(typeof(DbDataReader), "dataReader");
                var resultCoordinatorParameter = Expression.Parameter(typeof(ResultCoordinator), "resultCoordinator");

                var shaper = new ShaperExpressionProcessingExpressionVisitor(
                        selectExpression,
                        dataReaderParameter,
                        resultCoordinatorParameter)
                    .Inject(shapedQueryExpression.ShaperExpression);
                shaper = InjectEntityMaterializers(shaper);

                shaper = new RelationalProjectionBindingRemovingExpressionVisitor(
                        selectExpression,
                        dataReaderParameter,
                        indexMapParameter: null,
                        _detailedErrorsEnabled,
                        QueryCompilationContext.IsBuffering)
                    .Visit(shaper, out var projectionColumns);
                shaper = new CustomShaperCompilingExpressionVisitor(dataReaderParameter, resultCoordinatorParameter, QueryCompilationContext.IsTracking)
                    .Visit(shaper);

                var relationalCommandCache = new RelationalCommandCache(
                    Dependencies.MemoryCache,
                    RelationalDependencies.QuerySqlGeneratorFactory,
                    RelationalDependencies.RelationalParameterBasedSqlProcessorFactory,
                    selectExpression,
                    projectionColumns,
                    _useRelationalNulls);

                var shaperLambda = (LambdaExpression)shaper;

                return Expression.New(
                    typeof(QueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                    Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                    Expression.Constant(relationalCommandCache),
                    Expression.Constant(shaperLambda.Compile()),
                    Expression.Constant(_contextType),
                    Expression.Constant(QueryCompilationContext.PerformIdentityResolution));
            }
        }
    }
}
