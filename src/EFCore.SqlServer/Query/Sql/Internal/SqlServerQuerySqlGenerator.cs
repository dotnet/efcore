// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Sql.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerQuerySqlGenerator : DefaultQuerySqlGenerator, ISqlServerExpressionVisitor
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerQuerySqlGenerator(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] SelectExpression selectExpression,
            bool rowNumberPagingEnabled)
            : base(dependencies, selectExpression)
        {
            if (rowNumberPagingEnabled)
            {
                var rowNumberPagingExpressionVisitor = new RowNumberPagingExpressionVisitor();
                rowNumberPagingExpressionVisitor.Visit(selectExpression);
            }
        }

        /// <summary>
        ///     Visit a BinaryExpression.
        /// </summary>
        /// <param name="binaryExpression"> The binary expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.Left is SqlFunctionExpression sqlFunctionExpression
                && sqlFunctionExpression.FunctionName == "FREETEXT")
            {
                Visit(binaryExpression.Left);

                return binaryExpression;
            }

            return base.VisitBinary(binaryExpression);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression VisitCrossJoinLateral(CrossJoinLateralExpression crossJoinLateralExpression)
        {
            Check.NotNull(crossJoinLateralExpression, nameof(crossJoinLateralExpression));

            Sql.Append("CROSS APPLY ");

            Visit(crossJoinLateralExpression.TableExpression);

            return crossJoinLateralExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void GenerateLimitOffset(SelectExpression selectExpression)
        {
            if (selectExpression.Offset != null
                && !selectExpression.OrderBy.Any())
            {
                Sql.AppendLine().Append("ORDER BY (SELECT 1)");
            }

            base.GenerateLimitOffset(selectExpression);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression VisitRowNumber(RowNumberExpression rowNumberExpression)
        {
            Check.NotNull(rowNumberExpression, nameof(rowNumberExpression));

            Sql.Append("ROW_NUMBER() OVER(");
            GenerateOrderBy(rowNumberExpression.Orderings);
            Sql.Append(")");

            return rowNumberExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            if (sqlFunctionExpression.FunctionName.StartsWith("@@", StringComparison.Ordinal))
            {
                Sql.Append(sqlFunctionExpression.FunctionName);

                return sqlFunctionExpression;
            }

            if (sqlFunctionExpression.FunctionName == "COUNT"
                && sqlFunctionExpression.Type == typeof(long))
            {
                Visit(new SqlFunctionExpression("COUNT_BIG", typeof(long), sqlFunctionExpression.Arguments));

                return sqlFunctionExpression;
            }

            return base.VisitSqlFunction(sqlFunctionExpression);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression ApplyExplicitCastToBoolInProjectionOptimization(Expression expression)
        {
            var aliasedProjection = expression as AliasExpression;
            var expressionToProcess = aliasedProjection?.Expression ?? expression;

            var updatedExpression = ExplicitCastToBool(expressionToProcess);

            return aliasedProjection != null
                ? new AliasExpression(aliasedProjection.Alias, updatedExpression)
                : updatedExpression;
        }

        private static Expression ExplicitCastToBool(Expression expression)
            => ((expression as BinaryExpression)?.NodeType == ExpressionType.Coalesce || expression.NodeType == ExpressionType.Constant)
                && expression.Type.UnwrapNullableType() == typeof(bool)
                ? new ExplicitCastExpression(expression, expression.Type)
                : expression;

        private class RowNumberPagingExpressionVisitor : ExpressionVisitorBase
        {
            private const string RowNumberColumnName = "__RowNumber__";
            private int _counter;

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
                    RowNumberColumnName + (_counter != 0 ? $"{_counter}" : ""),
                    new RowNumberExpression(
                        subQuery.OrderBy
                            .Select(
                                o => new Ordering(
                                    o.Expression is AliasExpression ae ? ae.Expression : o.Expression,
                                    o.OrderingDirection))
                            .ToList()));

                _counter++;

                subQuery.ClearOrderBy();
                subQuery.AddToProjection(innerRowNumberExpression, resetProjectStar: false);

                var rowNumberReferenceExpression = new ColumnReferenceExpression(innerRowNumberExpression, subQuery);

                var offset = subQuery.Offset ?? Expression.Constant(0);

                if (subQuery.Offset != null)
                {
                    selectExpression.AddToPredicate(
                        Expression.GreaterThan(
                            rowNumberReferenceExpression,
                            ApplyConversion(offset, rowNumberReferenceExpression.Type)));

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
                        Expression.LessThanOrEqual(
                            rowNumberReferenceExpression,
                            ApplyConversion(limitExpression, rowNumberReferenceExpression.Type)));

                    subQuery.Limit = null;
                }

                return selectExpression;
            }

            private static Expression ApplyConversion(Expression expression, Type type)
            {
                return expression.Type != type ? Expression.Convert(expression, type) : expression;
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
