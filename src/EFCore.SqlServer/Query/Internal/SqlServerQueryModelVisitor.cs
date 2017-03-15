// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerQueryModelVisitor : RelationalQueryModelVisitor
    {
        private const string RowNumberColumnName = "__RowNumber__";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerQueryModelVisitor(
            [NotNull] EntityQueryModelVisitorDependencies dependencies,
            [NotNull] RelationalQueryModelVisitorDependencies relationalDependencies,
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            // ReSharper disable once SuggestBaseTypeForParameter
            [CanBeNull] SqlServerQueryModelVisitor parentQueryModelVisitor)
            : base(dependencies, relationalDependencies, queryCompilationContext, parentQueryModelVisitor)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitQueryModel(QueryModel queryModel)
        {
            base.VisitQueryModel(queryModel);

            if (ContextOptions.FindExtension<SqlServerOptionsExtension>()?.RowNumberPaging == true)
            {
                var visitor = new RowNumberPagingExpressionVisitor();

                if (QueriesBySource.TryGetValue(queryModel.MainFromClause, out SelectExpression mainSelectExpression))
                {
                    visitor.Visit(mainSelectExpression);
                }

                foreach (var additionalSource in queryModel.BodyClauses.OfType<IQuerySource>())
                {
                    if (QueriesBySource.TryGetValue(additionalSource, out SelectExpression additionalFromExpression))
                    {
                        visitor.Visit(additionalFromExpression);
                    }
                }
            }
        }

        private class RowNumberPagingExpressionVisitor : ExpressionVisitorBase
        {
            public override Expression Visit(Expression expression)
            {
                if (expression is ExistsExpression existsExpression)
                {
                    return VisitExistExpression(existsExpression);
                }

                return expression is SelectExpression selectExpression
                    ? VisitSelectExpression(selectExpression)
                    : base.Visit(expression);
            }

            private static bool RequiresRowNumberPaging(SelectExpression selectExpression)
                => selectExpression.Offset != null
                   && !selectExpression.Projection.Any(p => p is RowNumberExpression);

            private Expression VisitSelectExpression(SelectExpression selectExpression)
            {
                base.Visit(selectExpression);

                if (!RequiresRowNumberPaging(selectExpression))
                {
                    return selectExpression;
                }

                var subQuery = selectExpression.PushDownSubquery();

                foreach (var projection in subQuery.Projection)
                {
                    selectExpression.AddToProjection(projection.LiftExpressionFromSubquery(subQuery));
                }

                if (subQuery.OrderBy.Count == 0)
                {
                    subQuery.AddToOrderBy(
                        new Ordering(new SqlFunctionExpression("@@RowCount", typeof(int)), OrderingDirection.Asc));
                }

                var innerRowNumberExpression = new AliasExpression(
                    RowNumberColumnName,
                    new RowNumberExpression(subQuery.OrderBy));

                subQuery.ClearOrderBy();
                subQuery.AddToProjection(innerRowNumberExpression, resetProjectStar: false);

                var rowNumberReferenceExpression = new ColumnReferenceExpression(innerRowNumberExpression, subQuery);

                var offset = subQuery.Offset ?? Expression.Constant(0);

                if (subQuery.Offset != null)
                {
                    selectExpression.AddToPredicate
                        (Expression.GreaterThan(rowNumberReferenceExpression, offset));

                    subQuery.Offset = null;
                }

                if (subQuery.Limit != null)
                {
                    var constantValue = (subQuery.Limit as ConstantExpression)?.Value;
                    var offsetValue = (offset as ConstantExpression)?.Value;

                    var limitExpression
                        = constantValue != null
                          && offsetValue != null
                            ? (Expression)Expression.Constant((int)offsetValue + (int)constantValue)
                            : Expression.Add(offset, subQuery.Limit);

                    selectExpression.AddToPredicate(
                        Expression.LessThanOrEqual(rowNumberReferenceExpression, limitExpression));

                    subQuery.Limit = null;
                }

                if (selectExpression.Alias != null)
                {
                    selectExpression.ClearOrderBy();
                }

                return selectExpression;
            }

            private Expression VisitExistExpression(ExistsExpression existsExpression)
            {
                var newSubquery = (SelectExpression)Visit(existsExpression.Subquery);

                if (newSubquery.Limit == null
                    && newSubquery.Offset == null)
                {
                    newSubquery.ClearOrderBy();
                }

                return new ExistsExpression(newSubquery);
            }
        }
    }
}
