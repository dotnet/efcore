// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public class SqlServerQueryModelVisitor : RelationalQueryModelVisitor
    {
        private const string RowNumberColumnName = "__RowNumber__";

        private readonly bool _useRowNumberPaging;

        public SqlServerQueryModelVisitor(
            [NotNull] IModel model,
            [NotNull] IQueryOptimizer queryOptimizer,
            [NotNull] INavigationRewritingExpressionVisitorFactory navigationRewritingExpressionVisitorFactory,
            [NotNull] ISubQueryMemberPushDownExpressionVisitor subQueryMemberPushDownExpressionVisitor,
            [NotNull] IQuerySourceTracingExpressionVisitorFactory querySourceTracingExpressionVisitorFactory,
            [NotNull] IEntityResultFindingExpressionVisitorFactory entityResultFindingExpressionVisitorFactory,
            [NotNull] ITaskBlockingExpressionVisitor taskBlockingExpressionVisitor,
            [NotNull] IMemberAccessBindingExpressionVisitorFactory memberAccessBindingExpressionVisitorFactory,
            [NotNull] IOrderingExpressionVisitorFactory orderingExpressionVisitorFactory,
            [NotNull] IProjectionExpressionVisitorFactory projectionExpressionVisitorFactory,
            [NotNull] IEntityQueryableExpressionVisitorFactory entityQueryableExpressionVisitorFactory,
            [NotNull] IQueryAnnotationExtractor queryAnnotationExtractor,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] IExpressionPrinter expressionPrinter,
            [NotNull] IRelationalMetadataExtensionProvider relationalMetadataExtensionProvider,
            [NotNull] IIncludeExpressionVisitorFactory includeExpressionVisitorFactory,
            [NotNull] ISqlTranslatingExpressionVisitorFactory sqlTranslatingExpressionVisitorFactory,
            [NotNull] ICompositePredicateExpressionVisitorFactory compositePredicateExpressionVisitorFactory,
            [NotNull] IQueryFlatteningExpressionVisitorFactory queryFlatteningExpressionVisitorFactory,
            [NotNull] IShapedQueryFindingExpressionVisitorFactory shapedQueryFindingExpressionVisitorFactory,
            [NotNull] IDbContextOptions options,
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            // ReSharper disable once SuggestBaseTypeForParameter
            [CanBeNull] SqlServerQueryModelVisitor parentQueryModelVisitor)
            : base(model,
                queryOptimizer,
                navigationRewritingExpressionVisitorFactory,
                subQueryMemberPushDownExpressionVisitor,
                querySourceTracingExpressionVisitorFactory,
                entityResultFindingExpressionVisitorFactory,
                taskBlockingExpressionVisitor,
                memberAccessBindingExpressionVisitorFactory,
                orderingExpressionVisitorFactory,
                projectionExpressionVisitorFactory,
                entityQueryableExpressionVisitorFactory,
                queryAnnotationExtractor,
                resultOperatorHandler,
                entityMaterializerSource,
                expressionPrinter,
                relationalMetadataExtensionProvider,
                includeExpressionVisitorFactory,
                sqlTranslatingExpressionVisitorFactory,
                compositePredicateExpressionVisitorFactory,
                queryFlatteningExpressionVisitorFactory,
                shapedQueryFindingExpressionVisitorFactory,
                queryCompilationContext,
                parentQueryModelVisitor)
        {
            Check.NotNull(options, nameof(options));

            var extension = options.FindExtension<SqlServerOptionsExtension>();

            _useRowNumberPaging
                = extension?.RowNumberPaging != null
                  && extension.RowNumberPaging.Value;
        }

        public override void VisitQueryModel(QueryModel queryModel)
        {
            base.VisitQueryModel(queryModel);

            if (_useRowNumberPaging)
            {
                var visitor = new RowNumberPagingExpressionVisitor();

                SelectExpression mainSelectExpression;
                if (QueriesBySource.TryGetValue(queryModel.MainFromClause, out mainSelectExpression))
                {
                    visitor.Visit(mainSelectExpression);
                }

                foreach (var additionalSource in queryModel.BodyClauses.OfType<IQuerySource>())
                {
                    SelectExpression additionalFromExpression;
                    if (QueriesBySource.TryGetValue(additionalSource, out additionalFromExpression))
                    {
                        visitor.Visit(mainSelectExpression);
                    }
                }
            }
        }

        private class RowNumberPagingExpressionVisitor : ExpressionVisitorBase
        {
            public override Expression Visit(Expression node)
            {
                var expression = node as SelectExpression;

                return expression != null ? VisitSelectExpression(expression) : base.Visit(node);
            }

            private static bool RequiresRowNumberPaging(SelectExpression selectExpression)
                => selectExpression.Offset.HasValue
                   && selectExpression.Offset != 0
                   && !selectExpression.Projection.Any(p => p is RowNumberExpression);

            private Expression VisitSelectExpression(SelectExpression selectExpression)
            {
                base.Visit(selectExpression);

                if (!RequiresRowNumberPaging(selectExpression))
                {
                    return selectExpression;
                }

                var projections = new List<Expression>();
                projections.AddRange(selectExpression.Projection);

                var subQuery = selectExpression.PushDownSubquery();

                foreach (var projection in projections)
                {
                    var alias = projection as AliasExpression;
                    var column = projection as ColumnExpression;

                    if (column != null)
                    {
                        column = new ColumnExpression(column.Name, column.Property, subQuery);
                        selectExpression.AddToProjection(column);
                        continue;
                    }

                    column = alias?.TryGetColumnExpression();

                    if (column != null)
                    {
                        column = new ColumnExpression(column.Name, column.Property, subQuery);
                        alias = new AliasExpression(alias.Alias, column);
                        selectExpression.AddToProjection(alias);
                    }
                    else
                    {
                        selectExpression.AddToProjection(projection);
                    }
                }

                if (subQuery.OrderBy.Count == 0)
                {
                    subQuery.AddToOrderBy(
                        new Ordering(new SqlFunctionExpression("@@RowCount", typeof(int)), OrderingDirection.Asc));
                }

                var columnExpression = new ColumnExpression(RowNumberColumnName, typeof(int), subQuery);
                var rowNumber = new RowNumberExpression(columnExpression, subQuery.OrderBy);

                subQuery.ClearOrderBy();
                subQuery.ClearProjection();
                subQuery.AddToProjection(rowNumber);
                subQuery.IsProjectStar = true;

                Expression predicate = null;

                var offset = subQuery.Offset ?? 0;

                if (subQuery.Offset.HasValue)
                {
                    predicate = Expression.GreaterThan(columnExpression, Expression.Constant(offset));
                }

                if (subQuery.Limit.HasValue)
                {
                    var exp = Expression.LessThanOrEqual(columnExpression, Expression.Constant(offset + subQuery.Limit.Value));
                    if (predicate != null)
                    {
                        exp = Expression.AndAlso(predicate, exp);
                    }
                    predicate = exp;
                }

                selectExpression.Predicate = predicate;

                return selectExpression;
            }
        }
    }
}
