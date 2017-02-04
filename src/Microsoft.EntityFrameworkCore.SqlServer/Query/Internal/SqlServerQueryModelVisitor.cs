// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
                var existsExpression = node as ExistsExpression;
                if (existsExpression != null)
                {
                    return VisitExistExpression(existsExpression);
                }

                var selectExpression = node as SelectExpression;

                return selectExpression != null ? VisitSelectExpression(selectExpression) : base.Visit(node);
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
                        column = new ColumnExpression(alias.Alias ?? column.Name, column.Property, subQuery);
                        alias = new AliasExpression(alias.Alias, column);
                        selectExpression.AddToProjection(alias);
                    }
                    else
                    {
                        column = new ColumnExpression(alias?.Alias, alias.Expression.Type, subQuery);
                        selectExpression.AddToProjection(column);
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
                subQuery.AddToProjection(rowNumber, false);

                Expression predicate = null;

                var offset = subQuery.Offset ?? Expression.Constant(0);

                if (subQuery.Offset != null)
                {
                    predicate = Expression.GreaterThan(columnExpression, offset);
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

                    var expression = Expression.LessThanOrEqual(columnExpression, limitExpression);

                    if (predicate != null)
                    {
                        expression = Expression.AndAlso(predicate, expression);
                    }

                    predicate = expression;
                }

                selectExpression.Predicate = predicate;

                if (selectExpression.Alias != null)
                {
                    selectExpression.ClearOrderBy();
                }

                return selectExpression;
            }

            private Expression VisitExistExpression(ExistsExpression existsExpression)
            {
                var newExpression = Visit(existsExpression.Expression);
                var subSelectExpression = newExpression as SelectExpression;
                if (subSelectExpression != null
                    && subSelectExpression.Limit == null
                    && subSelectExpression.Offset == null)
                {
                    subSelectExpression.ClearOrderBy();
                }
                return new ExistsExpression(newExpression);
            }
        }
    }
}
