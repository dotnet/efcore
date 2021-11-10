// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class RelationalValueConverterCompensatingExpressionVisitor : ExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public RelationalValueConverterCompensatingExpressionVisitor(
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression switch
            {
                ShapedQueryExpression shapedQueryExpression => VisitShapedQueryExpression(shapedQueryExpression),
                CaseExpression caseExpression => VisitCase(caseExpression),
                SelectExpression selectExpression => VisitSelect(selectExpression),
                InnerJoinExpression innerJoinExpression => VisitInnerJoin(innerJoinExpression),
                LeftJoinExpression leftJoinExpression => VisitLeftJoin(leftJoinExpression),
                _ => base.VisitExtension(extensionExpression),
            };

        private Expression VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression)
        {
            if (AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue26428", out var enabled)
                && enabled)
            {
                return shapedQueryExpression.Update(
                    Visit(shapedQueryExpression.QueryExpression), shapedQueryExpression.ShaperExpression);
            }

            var selectExpression = shapedQueryExpression.QueryExpression;
            var updatedSelectExpression = Visit(selectExpression);
            return updatedSelectExpression != selectExpression
                ? shapedQueryExpression.Update(updatedSelectExpression,
                    ReplacingExpressionVisitor.Replace(
                        selectExpression, updatedSelectExpression, shapedQueryExpression.ShaperExpression))
                : shapedQueryExpression;
        }

        private Expression VisitCase(CaseExpression caseExpression)
        {
            var testIsCondition = caseExpression.Operand == null;
            var operand = (SqlExpression?)Visit(caseExpression.Operand);
            var whenClauses = new List<CaseWhenClause>();
            foreach (var whenClause in caseExpression.WhenClauses)
            {
                var test = (SqlExpression)Visit(whenClause.Test);
                if (testIsCondition)
                {
                    test = TryCompensateForBoolWithValueConverter(test);
                }

                var result = (SqlExpression)Visit(whenClause.Result);
                whenClauses.Add(new CaseWhenClause(test, result));
            }

            var elseResult = (SqlExpression?)Visit(caseExpression.ElseResult);

            return caseExpression.Update(operand, whenClauses, elseResult);
        }

        private Expression VisitSelect(SelectExpression selectExpression)
        {
            var changed = false;
            var projections = new List<ProjectionExpression>();
            foreach (var item in selectExpression.Projection)
            {
                var updatedProjection = (ProjectionExpression)Visit(item);
                projections.Add(updatedProjection);
                changed |= updatedProjection != item;
            }

            var tables = new List<TableExpressionBase>();
            foreach (var table in selectExpression.Tables)
            {
                var newTable = (TableExpressionBase)Visit(table);
                changed |= newTable != table;
                tables.Add(newTable);
            }

            var predicate = TryCompensateForBoolWithValueConverter((SqlExpression?)Visit(selectExpression.Predicate));
            changed |= predicate != selectExpression.Predicate;

            var groupBy = new List<SqlExpression>();
            foreach (var groupingKey in selectExpression.GroupBy)
            {
                var newGroupingKey = (SqlExpression)Visit(groupingKey);
                changed |= newGroupingKey != groupingKey;
                groupBy.Add(newGroupingKey);
            }

            var having = TryCompensateForBoolWithValueConverter((SqlExpression?)Visit(selectExpression.Having));
            changed |= having != selectExpression.Having;

            var orderings = new List<OrderingExpression>();
            foreach (var ordering in selectExpression.Orderings)
            {
                var orderingExpression = (SqlExpression)Visit(ordering.Expression);
                changed |= orderingExpression != ordering.Expression;
                orderings.Add(ordering.Update(orderingExpression));
            }

            var offset = (SqlExpression?)Visit(selectExpression.Offset);
            changed |= offset != selectExpression.Offset;

            var limit = (SqlExpression?)Visit(selectExpression.Limit);
            changed |= limit != selectExpression.Limit;

            return changed
                ? selectExpression.Update(
                    projections, tables, predicate, groupBy, having, orderings, limit, offset)
                : selectExpression;
        }

        private Expression VisitInnerJoin(InnerJoinExpression innerJoinExpression)
        {
            var table = (TableExpressionBase)Visit(innerJoinExpression.Table);
            var joinPredicate = TryCompensateForBoolWithValueConverter((SqlExpression)Visit(innerJoinExpression.JoinPredicate));

            return innerJoinExpression.Update(table, joinPredicate);
        }

        private Expression VisitLeftJoin(LeftJoinExpression leftJoinExpression)
        {
            var table = (TableExpressionBase)Visit(leftJoinExpression.Table);
            var joinPredicate = TryCompensateForBoolWithValueConverter((SqlExpression)Visit(leftJoinExpression.JoinPredicate));

            return leftJoinExpression.Update(table, joinPredicate);
        }

        [return: NotNullIfNotNull("sqlExpression")]
        private SqlExpression? TryCompensateForBoolWithValueConverter(SqlExpression? sqlExpression)
        {
            if (sqlExpression is ColumnExpression columnExpression
                && columnExpression.TypeMapping!.ClrType == typeof(bool)
                && columnExpression.TypeMapping.Converter != null)
            {
                return _sqlExpressionFactory.Equal(
                    sqlExpression,
                    _sqlExpressionFactory.Constant(true, sqlExpression.TypeMapping));
            }

            if (sqlExpression is SqlUnaryExpression sqlUnaryExpression)
            {
                return sqlUnaryExpression.Update(
                    TryCompensateForBoolWithValueConverter(sqlUnaryExpression.Operand));
            }

            if (sqlExpression is SqlBinaryExpression sqlBinaryExpression
                && (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso
                    || sqlBinaryExpression.OperatorType == ExpressionType.OrElse))
            {
                return sqlBinaryExpression.Update(
                    TryCompensateForBoolWithValueConverter(sqlBinaryExpression.Left),
                    TryCompensateForBoolWithValueConverter(sqlBinaryExpression.Right));
            }

            return sqlExpression;
        }
    }
}
