// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SearchConditionConvertingExpressionVisitor : SqlExpressionVisitor
    {
        private bool _isSearchCondition;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SearchConditionConvertingExpressionVisitor(
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        private Expression ApplyConversion(SqlExpression sqlExpression, bool condition)
                => _isSearchCondition
                    ? ConvertToSearchCondition(sqlExpression, condition)
                    : ConvertToValue(sqlExpression, condition);

        private Expression ConvertToSearchCondition(SqlExpression sqlExpression, bool condition)
            => condition
                ? sqlExpression
                : BuildCompareToExpression(sqlExpression);

        private Expression ConvertToValue(SqlExpression sqlExpression, bool condition)
        {
            return condition
                ? _sqlExpressionFactory.Case(new[]
                    {
                        new CaseWhenClause(
                            sqlExpression,
                            _sqlExpressionFactory.ApplyDefaultTypeMapping(_sqlExpressionFactory.Constant(true)))
                    },
                    _sqlExpressionFactory.Constant(false))
                : sqlExpression;
        }

        private SqlExpression BuildCompareToExpression(SqlExpression sqlExpression)
        {
            return _sqlExpressionFactory.Equal(sqlExpression, _sqlExpressionFactory.Constant(true));
        }

        protected override Expression VisitCase(CaseExpression caseExpression)
        {
            var parentSearchCondition = _isSearchCondition;

            var testIsCondition = caseExpression.Operand == null;
            _isSearchCondition = false;
            var operand = (SqlExpression)Visit(caseExpression.Operand);
            var whenClauses = new List<CaseWhenClause>();
            foreach (var whenClause in caseExpression.WhenClauses)
            {
                _isSearchCondition = testIsCondition;
                var test = (SqlExpression)Visit(whenClause.Test);
                _isSearchCondition = false;
                var result = (SqlExpression)Visit(whenClause.Result);
                whenClauses.Add(new CaseWhenClause(test, result));
            }

            _isSearchCondition = false;
            var elseResult = (SqlExpression)Visit(caseExpression.ElseResult);

            _isSearchCondition = parentSearchCondition;

            return ApplyConversion(caseExpression.Update(operand, whenClauses, elseResult), condition: false);
        }

        protected override Expression VisitColumn(ColumnExpression columnExpression)
        {
            return ApplyConversion(columnExpression, condition: false);
        }

        protected override Expression VisitExists(ExistsExpression existsExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var subquery = (SelectExpression)Visit(existsExpression.Subquery);
            _isSearchCondition = parentSearchCondition;

            return ApplyConversion(existsExpression.Update(subquery), condition: true);
        }

        protected override Expression VisitFromSql(FromSqlExpression fromSqlExpression)
            => fromSqlExpression;

        protected override Expression VisitIn(InExpression inExpression)
        {
            var parentSearchCondition = _isSearchCondition;

            _isSearchCondition = false;
            var item = (SqlExpression)Visit(inExpression.Item);
            var subquery = (SelectExpression)Visit(inExpression.Subquery);
            var values = (SqlExpression)Visit(inExpression.Values);
            _isSearchCondition = parentSearchCondition;

            return ApplyConversion(inExpression.Update(item, values, subquery), condition: true);
        }

        protected override Expression VisitLike(LikeExpression likeExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var match = (SqlExpression)Visit(likeExpression.Match);
            var pattern = (SqlExpression)Visit(likeExpression.Pattern);
            var escapeChar = (SqlExpression)Visit(likeExpression.EscapeChar);
            _isSearchCondition = parentSearchCondition;

            return ApplyConversion(likeExpression.Update(match, pattern, escapeChar), condition: true);
        }

        protected override Expression VisitSelect(SelectExpression selectExpression)
        {
            var changed = false;
            var parentSearchCondition = _isSearchCondition;

            var projections = new List<ProjectionExpression>();
            _isSearchCondition = false;
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

            _isSearchCondition = true;
            var predicate = (SqlExpression)Visit(selectExpression.Predicate);
            changed |= predicate != selectExpression.Predicate;

            var groupBy = new List<SqlExpression>();
            _isSearchCondition = false;
            foreach (var groupingKey in selectExpression.GroupBy)
            {
                var newGroupingKey = (SqlExpression)Visit(groupingKey);
                changed |= newGroupingKey != groupingKey;
                groupBy.Add(newGroupingKey);
            }

            _isSearchCondition = true;
            var havingExpression = (SqlExpression)Visit(selectExpression.Having);
            changed |= havingExpression != selectExpression.Having;

            var orderings = new List<OrderingExpression>();
            _isSearchCondition = false;
            foreach (var ordering in selectExpression.Orderings)
            {
                var orderingExpression = (SqlExpression)Visit(ordering.Expression);
                changed |= orderingExpression != ordering.Expression;
                orderings.Add(ordering.Update(orderingExpression));
            }

            var offset = (SqlExpression)Visit(selectExpression.Offset);
            changed |= offset != selectExpression.Offset;

            var limit = (SqlExpression)Visit(selectExpression.Limit);
            changed |= limit != selectExpression.Limit;

            _isSearchCondition = parentSearchCondition;

            return changed
                ? selectExpression.Update(
                    projections, tables, predicate, groupBy, havingExpression, orderings, limit, offset, selectExpression.IsDistinct, selectExpression.Alias)
                : selectExpression;
        }

        protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
        {
            var parentIsSearchCondition = _isSearchCondition;

            switch (sqlBinaryExpression.OperatorType)
            {
                // Only logical operations need conditions on both sides
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    _isSearchCondition = true;
                    break;
                default:
                    _isSearchCondition = false;
                    break;
            }

            var newLeft = (SqlExpression)Visit(sqlBinaryExpression.Left);
            var newRight = (SqlExpression)Visit(sqlBinaryExpression.Right);

            _isSearchCondition = parentIsSearchCondition;

            sqlBinaryExpression = sqlBinaryExpression.Update(newLeft, newRight);
            var condition = sqlBinaryExpression.OperatorType == ExpressionType.AndAlso
                    || sqlBinaryExpression.OperatorType == ExpressionType.OrElse
                    || sqlBinaryExpression.OperatorType == ExpressionType.Equal
                    || sqlBinaryExpression.OperatorType == ExpressionType.NotEqual
                    || sqlBinaryExpression.OperatorType == ExpressionType.GreaterThan
                    || sqlBinaryExpression.OperatorType == ExpressionType.GreaterThanOrEqual
                    || sqlBinaryExpression.OperatorType == ExpressionType.LessThan
                    || sqlBinaryExpression.OperatorType == ExpressionType.LessThanOrEqual;

            return ApplyConversion(sqlBinaryExpression, condition);
        }

        protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            bool resultCondition;
            switch (sqlUnaryExpression.OperatorType)
            {
                case ExpressionType.Not:
                    _isSearchCondition = true;
                    resultCondition = true;
                    break;

                case ExpressionType.Convert:
                case ExpressionType.Negate:
                    _isSearchCondition = false;
                    resultCondition = false;
                    break;

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    _isSearchCondition = false;
                    resultCondition = true;
                    break;

                default:
                    throw new InvalidOperationException("Unknown operator type encountered in SqlUnaryExpression.");
            }

            var operand = (SqlExpression)Visit(sqlUnaryExpression.Operand);
            _isSearchCondition = parentSearchCondition;

            return ApplyConversion(sqlUnaryExpression.Update(operand), condition: resultCondition);
        }

        protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
        {
            return ApplyConversion(sqlConstantExpression, condition: false);
        }

        protected override Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression)
        {
            return sqlFragmentExpression;
        }

        protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var instance = (SqlExpression)Visit(sqlFunctionExpression.Instance);
            var arguments = new SqlExpression[sqlFunctionExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                arguments[i] = (SqlExpression)Visit(sqlFunctionExpression.Arguments[i]);
            }

            _isSearchCondition = parentSearchCondition;
            var newFunction = sqlFunctionExpression.Update(instance, arguments);

            var condition = string.Equals(sqlFunctionExpression.Name, "FREETEXT")
                || string.Equals(sqlFunctionExpression.Name, "CONTAINS");

            return ApplyConversion(newFunction, condition);
        }

        protected override Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
        {
            return ApplyConversion(sqlParameterExpression, condition: false);
        }

        protected override Expression VisitTable(TableExpression tableExpression)
        {
            return tableExpression;
        }

        protected override Expression VisitProjection(ProjectionExpression projectionExpression)
        {
            var expression = (SqlExpression)Visit(projectionExpression.Expression);

            return projectionExpression.Update(expression);
        }

        protected override Expression VisitOrdering(OrderingExpression orderingExpression)
        {
            var expression = (SqlExpression)Visit(orderingExpression.Expression);

            return orderingExpression.Update(expression);
        }

        protected override Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var table = (TableExpressionBase)Visit(crossJoinExpression.Table);
            _isSearchCondition = parentSearchCondition;

            return crossJoinExpression.Update(table);
        }

        protected override Expression VisitCrossApply(CrossApplyExpression crossApplyExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var table = (TableExpressionBase)Visit(crossApplyExpression.Table);
            _isSearchCondition = parentSearchCondition;

            return crossApplyExpression.Update(table);
        }

        protected override Expression VisitOuterApply(OuterApplyExpression outerApplyExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var table = (TableExpressionBase)Visit(outerApplyExpression.Table);
            _isSearchCondition = parentSearchCondition;

            return outerApplyExpression.Update(table);
        }

        protected override Expression VisitInnerJoin(InnerJoinExpression innerJoinExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var table = (TableExpressionBase)Visit(innerJoinExpression.Table);
            _isSearchCondition = true;
            var joinPredicate = (SqlExpression)Visit(innerJoinExpression.JoinPredicate);
            _isSearchCondition = parentSearchCondition;

            return innerJoinExpression.Update(table, joinPredicate);
        }

        protected override Expression VisitLeftJoin(LeftJoinExpression leftJoinExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var table = (TableExpressionBase)Visit(leftJoinExpression.Table);
            _isSearchCondition = true;
            var joinPredicate = (SqlExpression)Visit(leftJoinExpression.JoinPredicate);
            _isSearchCondition = parentSearchCondition;

            return leftJoinExpression.Update(table, joinPredicate);
        }

        protected override Expression VisitSubSelect(ScalarSubqueryExpression scalarSubqueryExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            var subquery = (SelectExpression)Visit(scalarSubqueryExpression.Subquery);
            _isSearchCondition = parentSearchCondition;

            return ApplyConversion(scalarSubqueryExpression.Update(subquery), condition: false);
        }

        protected override Expression VisitRowNumber(RowNumberExpression rowNumberExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var changed = false;
            var partitions = new List<SqlExpression>();
            foreach (var partition in rowNumberExpression.Partitions)
            {
                var newPartition = (SqlExpression)Visit(partition);
                changed |= newPartition != partition;
                partitions.Add(newPartition);
            }

            var orderings = new List<OrderingExpression>();
            foreach (var ordering in rowNumberExpression.Orderings)
            {
                var newOrdering = (OrderingExpression)Visit(ordering);
                changed |= newOrdering != ordering;
                orderings.Add(newOrdering);
            }

            _isSearchCondition = parentSearchCondition;

            return ApplyConversion(rowNumberExpression.Update(partitions, orderings), condition: false);
        }

        protected override Expression VisitExcept(ExceptExpression exceptExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var source1 = (SelectExpression)Visit(exceptExpression.Source1);
            var source2 = (SelectExpression)Visit(exceptExpression.Source2);
            _isSearchCondition = parentSearchCondition;

            return exceptExpression.Update(source1, source2);
        }

        protected override Expression VisitIntersect(IntersectExpression intersectExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var source1 = (SelectExpression)Visit(intersectExpression.Source1);
            var source2 = (SelectExpression)Visit(intersectExpression.Source2);
            _isSearchCondition = parentSearchCondition;

            return intersectExpression.Update(source1, source2);
        }

        protected override Expression VisitUnion(UnionExpression unionExpression)
        {
            var parentSearchCondition = _isSearchCondition;
            _isSearchCondition = false;
            var source1 = (SelectExpression)Visit(unionExpression.Source1);
            var source2 = (SelectExpression)Visit(unionExpression.Source2);
            _isSearchCondition = parentSearchCondition;

            return unionExpression.Update(source1, source2);
        }
    }
}
