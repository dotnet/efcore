// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class NullSemanticsRewritingExpressionVisitor : SqlExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        private bool _isNullable;
        private bool _canOptimize;
        private readonly List<ColumnExpression> _nonNullableColumns = new List<ColumnExpression>();

        public NullSemanticsRewritingExpressionVisitor(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _canOptimize = true;
        }

        protected override Expression VisitCase(CaseExpression caseExpression)
        {
            _isNullable = false;
            // if there is no 'else' there is a possibility of null, when none of the conditions are met
            // otherwise the result is nullable if any of the WhenClause results OR ElseResult is nullable
            var isNullable = caseExpression.ElseResult == null;

            var canOptimize = _canOptimize;
            var testIsCondition = caseExpression.Operand == null;
            _canOptimize = false;
            var newOperand = (SqlExpression)Visit(caseExpression.Operand);
            var newWhenClauses = new List<CaseWhenClause>();
            foreach (var whenClause in caseExpression.WhenClauses)
            {
                _canOptimize = testIsCondition;
                var newTest = (SqlExpression)Visit(whenClause.Test);
                _canOptimize = false;
                _isNullable = false;
                var newResult = (SqlExpression)Visit(whenClause.Result);
                isNullable |= _isNullable;
                newWhenClauses.Add(new CaseWhenClause(newTest, newResult));
            }

            _canOptimize = false;
            var newElseResult = (SqlExpression)Visit(caseExpression.ElseResult);
            _isNullable |= isNullable;
            _canOptimize = canOptimize;

            return caseExpression.Update(newOperand, newWhenClauses, newElseResult);
        }

        protected override Expression VisitColumn(ColumnExpression columnExpression)
        {
            _isNullable = !_nonNullableColumns.Contains(columnExpression) && columnExpression.IsNullable;

            return columnExpression;
        }

        protected override Expression VisitCrossApply(CrossApplyExpression crossApplyExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            var table = (TableExpressionBase)Visit(crossApplyExpression.Table);
            _canOptimize = canOptimize;

            return crossApplyExpression.Update(table);
        }

        protected override Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            var table = (TableExpressionBase)Visit(crossJoinExpression.Table);
            _canOptimize = canOptimize;

            return crossJoinExpression.Update(table);
        }

        protected override Expression VisitExcept(ExceptExpression exceptExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            var source1 = (SelectExpression)Visit(exceptExpression.Source1);
            var source2 = (SelectExpression)Visit(exceptExpression.Source2);
            _canOptimize = canOptimize;

            return exceptExpression.Update(source1, source2);
        }

        protected override Expression VisitExists(ExistsExpression existsExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            var newSubquery = (SelectExpression)Visit(existsExpression.Subquery);
            _canOptimize = canOptimize;

            return existsExpression.Update(newSubquery);
        }

        protected override Expression VisitFromSql(FromSqlExpression fromSqlExpression)
            => fromSqlExpression;

        protected override Expression VisitIn(InExpression inExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            _isNullable = false;
            var item = (SqlExpression)Visit(inExpression.Item);
            var isNullable = _isNullable;
            _isNullable = false;
            var subquery = (SelectExpression)Visit(inExpression.Subquery);
            isNullable |= _isNullable;
            _isNullable = false;
            var values = (SqlExpression)Visit(inExpression.Values);
            _isNullable |= isNullable;
            _canOptimize = canOptimize;

            return inExpression.Update(item, values, subquery);
        }

        protected override Expression VisitIntersect(IntersectExpression intersectExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            var source1 = (SelectExpression)Visit(intersectExpression.Source1);
            var source2 = (SelectExpression)Visit(intersectExpression.Source2);
            _canOptimize = canOptimize;

            return intersectExpression.Update(source1, source2);
        }

        protected override Expression VisitLike(LikeExpression likeExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            _isNullable = false;
            var newMatch = (SqlExpression)Visit(likeExpression.Match);
            var isNullable = _isNullable;
            _isNullable = false;
            var newPattern = (SqlExpression)Visit(likeExpression.Pattern);
            isNullable |= _isNullable;
            _isNullable = false;
            var newEscapeChar = (SqlExpression)Visit(likeExpression.EscapeChar);
            _isNullable |= isNullable;
            _canOptimize = canOptimize;

            return likeExpression.Update(newMatch, newPattern, newEscapeChar);
        }

        protected override Expression VisitInnerJoin(InnerJoinExpression innerJoinExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            var newTable = (TableExpressionBase)Visit(innerJoinExpression.Table);
            var newJoinPredicate = VisitJoinPredicate((SqlBinaryExpression)innerJoinExpression.JoinPredicate);
            _canOptimize = canOptimize;

            return innerJoinExpression.Update(newTable, newJoinPredicate);
        }

        protected override Expression VisitLeftJoin(LeftJoinExpression leftJoinExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            var newTable = (TableExpressionBase)Visit(leftJoinExpression.Table);
            var newJoinPredicate = VisitJoinPredicate((SqlBinaryExpression)leftJoinExpression.JoinPredicate);
            _canOptimize = canOptimize;

            return leftJoinExpression.Update(newTable, newJoinPredicate);
        }

        private SqlExpression VisitJoinPredicate(SqlBinaryExpression predicate)
        {
            var canOptimize = _canOptimize;
            _canOptimize = true;

            if (predicate.OperatorType == ExpressionType.Equal)
            {
                var newLeft = (SqlExpression)Visit(predicate.Left);
                var newRight = (SqlExpression)Visit(predicate.Right);
                _canOptimize = canOptimize;

                return predicate.Update(newLeft, newRight);
            }

            if (predicate.OperatorType == ExpressionType.AndAlso)
            {
                var newPredicate = (SqlExpression)VisitSqlBinary(predicate);
                _canOptimize = canOptimize;

                return newPredicate;
            }

            throw new InvalidOperationException("Unexpected join predicate shape: " + predicate);
        }

        protected override Expression VisitOrdering(OrderingExpression orderingExpression)
        {
            var expression = (SqlExpression)Visit(orderingExpression.Expression);

            return orderingExpression.Update(expression);
        }

        protected override Expression VisitOuterApply(OuterApplyExpression outerApplyExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            var table = (TableExpressionBase)Visit(outerApplyExpression.Table);
            _canOptimize = canOptimize;

            return outerApplyExpression.Update(table);
        }

        protected override Expression VisitProjection(ProjectionExpression projectionExpression)
        {
            var expression = (SqlExpression)Visit(projectionExpression.Expression);

            return projectionExpression.Update(expression);
        }

        protected override Expression VisitRowNumber(RowNumberExpression rowNumberExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
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

            _canOptimize = canOptimize;

            return rowNumberExpression.Update(partitions, orderings);
        }

        protected override Expression VisitSelect(SelectExpression selectExpression)
        {
            var changed = false;
            var canOptimize = _canOptimize;
            var projections = new List<ProjectionExpression>();
            _canOptimize = false;
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

            _canOptimize = true;
            var predicate = (SqlExpression)Visit(selectExpression.Predicate);
            changed |= predicate != selectExpression.Predicate;

            var groupBy = new List<SqlExpression>();
            _canOptimize = false;
            foreach (var groupingKey in selectExpression.GroupBy)
            {
                var newGroupingKey = (SqlExpression)Visit(groupingKey);
                changed |= newGroupingKey != groupingKey;
                groupBy.Add(newGroupingKey);
            }

            _canOptimize = true;
            var havingExpression = (SqlExpression)Visit(selectExpression.Having);
            changed |= havingExpression != selectExpression.Having;

            var orderings = new List<OrderingExpression>();
            _canOptimize = false;
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

            _canOptimize = canOptimize;

            // we assume SelectExpression can always be null
            // (e.g. projecting non-nullable column but with predicate that filters out all rows)
            _isNullable = true;

            return changed
                ? selectExpression.Update(
                    projections, tables, predicate, groupBy, havingExpression, orderings, limit, offset, selectExpression.IsDistinct,
                    selectExpression.Alias)
                : selectExpression;
        }

        protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
        {
            _isNullable = false;
            var canOptimize = _canOptimize;

            // for SqlServer we could also allow optimize on children of ExpressionType.Equal
            // because they get converted to CASE blocks anyway, but for other providers it's incorrect
            // once/if null semantics optimizations are provider-specific we can enable it
            _canOptimize = _canOptimize && (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso
                || sqlBinaryExpression.OperatorType == ExpressionType.OrElse);

            var nonNullableColumns = new List<ColumnExpression>();
            if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
            {
                nonNullableColumns = FindNonNullableColumns(sqlBinaryExpression.Left);
            }

            var newLeft = (SqlExpression)Visit(sqlBinaryExpression.Left);
            var leftNullable = _isNullable;

            _isNullable = false;
            if (nonNullableColumns.Count > 0)
            {
                _nonNullableColumns.AddRange(nonNullableColumns);
            }

            var newRight = (SqlExpression)Visit(sqlBinaryExpression.Right);
            var rightNullable = _isNullable;

            foreach (var nonNullableColumn in nonNullableColumns)
            {
                _nonNullableColumns.Remove(nonNullableColumn);
            }

            if (sqlBinaryExpression.OperatorType == ExpressionType.Coalesce)
            {
                _isNullable = leftNullable && rightNullable;
                _canOptimize = canOptimize;

                return sqlBinaryExpression.Update(newLeft, newRight);
            }

            if (sqlBinaryExpression.OperatorType == ExpressionType.Equal
                || sqlBinaryExpression.OperatorType == ExpressionType.NotEqual)
            {
                var leftConstantNull = newLeft is SqlConstantExpression leftConstant && leftConstant.Value == null;
                var rightConstantNull = newRight is SqlConstantExpression rightConstant && rightConstant.Value == null;

                // a == null -> a IS NULL
                // a != null -> a IS NOT NULL
                if (rightConstantNull)
                {
                    _isNullable = false;
                    _canOptimize = canOptimize;

                    return sqlBinaryExpression.OperatorType == ExpressionType.Equal
                        ? _sqlExpressionFactory.IsNull(newLeft)
                        : _sqlExpressionFactory.IsNotNull(newLeft);
                }

                // null == a -> a IS NULL
                // null != a -> a IS NOT NULL
                if (leftConstantNull)
                {
                    _isNullable = false;
                    _canOptimize = canOptimize;

                    return sqlBinaryExpression.OperatorType == ExpressionType.Equal
                        ? _sqlExpressionFactory.IsNull(newRight)
                        : _sqlExpressionFactory.IsNotNull(newRight);
                }

                var leftUnary = newLeft as SqlUnaryExpression;
                var rightUnary = newRight as SqlUnaryExpression;

                var leftNegated = leftUnary?.OperatorType == ExpressionType.Not;
                var rightNegated = rightUnary?.OperatorType == ExpressionType.Not;

                if (leftNegated)
                {
                    newLeft = leftUnary.Operand;
                }

                if (rightNegated)
                {
                    newRight = rightUnary.Operand;
                }

                var leftIsNull = _sqlExpressionFactory.IsNull(newLeft);
                var rightIsNull = _sqlExpressionFactory.IsNull(newRight);

                // optimized expansion which doesn't distinguish between null and false
                if (canOptimize
                    && sqlBinaryExpression.OperatorType == ExpressionType.Equal
                    && !leftNegated
                    && !rightNegated)
                {
                    // when we use optimized form, the result can still be nullable
                    if (leftNullable && rightNullable)
                    {
                        _isNullable = true;
                        _canOptimize = canOptimize;

                        return _sqlExpressionFactory.OrElse(
                            _sqlExpressionFactory.Equal(newLeft, newRight),
                            _sqlExpressionFactory.AndAlso(leftIsNull, rightIsNull));
                    }

                    if ((leftNullable && !rightNullable)
                        || (!leftNullable && rightNullable))
                    {
                        _isNullable = true;
                        _canOptimize = canOptimize;

                        return _sqlExpressionFactory.Equal(newLeft, newRight);
                    }
                }

                // doing a full null semantics rewrite - removing all nulls from truth table
                // this will NOT be correct once we introduce simplified null semantics
                _isNullable = false;
                _canOptimize = canOptimize;

                if (sqlBinaryExpression.OperatorType == ExpressionType.Equal)
                {
                    if (!leftNullable
                        && !rightNullable)
                    {
                        // a == b <=> !a == !b -> a == b
                        // !a == b <=> a == !b -> a != b
                        return leftNegated == rightNegated
                            ? _sqlExpressionFactory.Equal(newLeft, newRight)
                            : _sqlExpressionFactory.NotEqual(newLeft, newRight);
                    }

                    if (leftNullable && rightNullable)
                    {
                        // ?a == ?b <=> !(?a) == !(?b) -> [(a == b) && (a != null && b != null)] || (a == null && b == null))
                        // !(?a) == ?b <=> ?a == !(?b) -> [(a != b) && (a != null && b != null)] || (a == null && b == null)
                        return leftNegated == rightNegated
                            ? ExpandNullableEqualNullable(newLeft, newRight, leftIsNull, rightIsNull)
                            : ExpandNegatedNullableEqualNullable(newLeft, newRight, leftIsNull, rightIsNull);
                    }

                    if (leftNullable && !rightNullable)
                    {
                        // ?a == b <=> !(?a) == !b -> (a == b) && (a != null)
                        // !(?a) == b <=> ?a == !b -> (a != b) && (a != null)
                        return leftNegated == rightNegated
                            ? ExpandNullableEqualNonNullable(newLeft, newRight, leftIsNull)
                            : ExpandNegatedNullableEqualNonNullable(newLeft, newRight, leftIsNull);
                    }

                    if (rightNullable && !leftNullable)
                    {
                        // a == ?b <=> !a == !(?b) -> (a == b) && (b != null)
                        // !a == ?b <=> a == !(?b) -> (a != b) && (b != null)
                        return leftNegated == rightNegated
                            ? ExpandNullableEqualNonNullable(newLeft, newRight, rightIsNull)
                            : ExpandNegatedNullableEqualNonNullable(newLeft, newRight, rightIsNull);
                    }
                }

                if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual)
                {
                    if (!leftNullable
                        && !rightNullable)
                    {
                        // a != b <=> !a != !b -> a != b
                        // !a != b <=> a != !b -> a == b
                        return leftNegated == rightNegated
                            ? _sqlExpressionFactory.NotEqual(newLeft, newRight)
                            : _sqlExpressionFactory.Equal(newLeft, newRight);
                    }

                    if (leftNullable && rightNullable)
                    {
                        // ?a != ?b <=> !(?a) != !(?b) -> [(a != b) || (a == null || b == null)] && (a != null || b != null)
                        // !(?a) != ?b <=> ?a != !(?b) -> [(a == b) || (a == null || b == null)] && (a != null || b != null)
                        return leftNegated == rightNegated
                            ? ExpandNullableNotEqualNullable(newLeft, newRight, leftIsNull, rightIsNull)
                            : ExpandNegatedNullableNotEqualNullable(newLeft, newRight, leftIsNull, rightIsNull);
                    }

                    if (leftNullable)
                    {
                        // ?a != b <=> !(?a) != !b -> (a != b) || (a == null)
                        // !(?a) != b <=> ?a != !b -> (a == b) || (a == null)
                        return leftNegated == rightNegated
                            ? ExpandNullableNotEqualNonNullable(newLeft, newRight, leftIsNull)
                            : ExpandNegatedNullableNotEqualNonNullable(newLeft, newRight, leftIsNull);
                    }

                    if (rightNullable)
                    {
                        // a != ?b <=> !a != !(?b) -> (a != b) || (b == null)
                        // !a != ?b <=> a != !(?b) -> (a == b) || (b == null)
                        return leftNegated == rightNegated
                            ? ExpandNullableNotEqualNonNullable(newLeft, newRight, rightIsNull)
                            : ExpandNegatedNullableNotEqualNonNullable(newLeft, newRight, rightIsNull);
                    }
                }
            }

            _isNullable = leftNullable || rightNullable;
            _canOptimize = canOptimize;

            return sqlBinaryExpression.Update(newLeft, newRight);
        }

        protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
        {
            _isNullable = sqlConstantExpression.Value == null;

            return sqlConstantExpression;
        }

        protected override Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression)
            => sqlFragmentExpression;

        protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;

            var newInstance = (SqlExpression)Visit(sqlFunctionExpression.Instance);
            var newArguments = new SqlExpression[sqlFunctionExpression.Arguments.Count];
            for (var i = 0; i < newArguments.Length; i++)
            {
                newArguments[i] = (SqlExpression)Visit(sqlFunctionExpression.Arguments[i]);
            }

            _canOptimize = canOptimize;

            // TODO: #18555
            _isNullable = true;

            return sqlFunctionExpression.Update(newInstance, newArguments);
        }

        protected override Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
        {
            // at this point we assume every parameter is nullable, we will filter out the non-nullable ones once we know the actual values
            _isNullable = true;

            return sqlParameterExpression;
        }

        protected override Expression VisitSqlUnary(SqlUnaryExpression sqlCastExpression)
        {
            _isNullable = false;

            var canOptimize = _canOptimize;
            _canOptimize = false;

            var newOperand = (SqlExpression)Visit(sqlCastExpression.Operand);

            // result of IsNull/IsNotNull can never be null
            if (sqlCastExpression.OperatorType == ExpressionType.Equal
                || sqlCastExpression.OperatorType == ExpressionType.NotEqual)
            {
                _isNullable = false;
            }

            _canOptimize = canOptimize;

            return sqlCastExpression.Update(newOperand);
        }

        protected override Expression VisitSubSelect(ScalarSubqueryExpression scalarSubqueryExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            var subquery = (SelectExpression)Visit(scalarSubqueryExpression.Subquery);
            _canOptimize = canOptimize;

            return scalarSubqueryExpression.Update(subquery);
        }

        protected override Expression VisitTable(TableExpression tableExpression)
            => tableExpression;

        protected override Expression VisitUnion(UnionExpression unionExpression)
        {
            var canOptimize = _canOptimize;
            _canOptimize = false;
            var source1 = (SelectExpression)Visit(unionExpression.Source1);
            var source2 = (SelectExpression)Visit(unionExpression.Source2);
            _canOptimize = canOptimize;

            return unionExpression.Update(source1, source2);
        }

        private List<ColumnExpression> FindNonNullableColumns(SqlExpression sqlExpression)
        {
            var result = new List<ColumnExpression>();
            if (sqlExpression is SqlBinaryExpression sqlBinaryExpression)
            {
                if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual)
                {
                    if (sqlBinaryExpression.Left is ColumnExpression leftColumn
                        && leftColumn.IsNullable
                        && sqlBinaryExpression.Right is SqlConstantExpression rightConstant
                        && rightConstant.Value == null)
                    {
                        result.Add(leftColumn);
                    }

                    if (sqlBinaryExpression.Right is ColumnExpression rightColumn
                        && rightColumn.IsNullable
                        && sqlBinaryExpression.Left is SqlConstantExpression leftConstant
                        && leftConstant.Value == null)
                    {
                        result.Add(rightColumn);
                    }
                }

                if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                {
                    result.AddRange(FindNonNullableColumns(sqlBinaryExpression.Left));
                    result.AddRange(FindNonNullableColumns(sqlBinaryExpression.Right));
                }
            }

            return result;
        }

        // ?a == ?b -> [(a == b) && (a != null && b != null)] || (a == null && b == null))
        //
        // a | b | F1 = a == b | F2 = (a != null && b != null) | F3 = F1 && F2 |
        //   |   |             |                               |               |
        // 0 | 0 | 1           | 1                             | 1             |
        // 0 | 1 | 0           | 1                             | 0             |
        // 0 | N | N           | 0                             | 0             |
        // 1 | 0 | 0           | 1                             | 0             |
        // 1 | 1 | 1           | 1                             | 1             |
        // 1 | N | N           | 0                             | 0             |
        // N | 0 | N           | 0                             | 0             |
        // N | 1 | N           | 0                             | 0             |
        // N | N | N           | 0                             | 0             |
        //
        // a | b | F4 = (a == null && b == null) | Final = F3 OR F4 |
        //   |   |                               |                  |
        // 0 | 0 | 0                             | 1 OR 0 = 1       |
        // 0 | 1 | 0                             | 0 OR 0 = 0       |
        // 0 | N | 0                             | 0 OR 0 = 0       |
        // 1 | 0 | 0                             | 0 OR 0 = 0       |
        // 1 | 1 | 0                             | 1 OR 0 = 1       |
        // 1 | N | 0                             | 0 OR 0 = 0       |
        // N | 0 | 0                             | 0 OR 0 = 0       |
        // N | 1 | 0                             | 0 OR 0 = 0       |
        // N | N | 1                             | 0 OR 1 = 1       |
        private SqlBinaryExpression ExpandNullableEqualNullable(
            SqlExpression left, SqlExpression right, SqlExpression leftIsNull, SqlExpression rightIsNull)
            => _sqlExpressionFactory.OrElse(
                _sqlExpressionFactory.AndAlso(
                    _sqlExpressionFactory.Equal(left, right),
                    _sqlExpressionFactory.AndAlso(
                        _sqlExpressionFactory.Not(leftIsNull),
                        _sqlExpressionFactory.Not(rightIsNull))),
                _sqlExpressionFactory.AndAlso(
                    leftIsNull,
                    rightIsNull));

        // !(?a) == ?b -> [(a != b) && (a != null && b != null)] || (a == null && b == null)
        //
        // a | b | F1 = a != b | F2 = (a != null && b != null) | F3 = F1 && F2 |
        //   |   |             |                               |               |
        // 0 | 0 | 0           | 1                             | 0             |
        // 0 | 1 | 1           | 1                             | 1             |
        // 0 | N | N           | 0                             | 0             |
        // 1 | 0 | 1           | 1                             | 1             |
        // 1 | 1 | 0           | 1                             | 0             |
        // 1 | N | N           | 0                             | 0             |
        // N | 0 | N           | 0                             | 0             |
        // N | 1 | N           | 0                             | 0             |
        // N | N | N           | 0                             | 0             |
        //
        // a | b | F4 = (a == null && b == null) | Final = F3 OR F4 |
        //   |   |                               |                  |
        // 0 | 0 | 0                             | 0 OR 0 = 0       |
        // 0 | 1 | 0                             | 1 OR 0 = 1       |
        // 0 | N | 0                             | 0 OR 0 = 0       |
        // 1 | 0 | 0                             | 1 OR 0 = 1       |
        // 1 | 1 | 0                             | 0 OR 0 = 0       |
        // 1 | N | 0                             | 0 OR 0 = 0       |
        // N | 0 | 0                             | 0 OR 0 = 0       |
        // N | 1 | 0                             | 0 OR 0 = 0       |
        // N | N | 1                             | 0 OR 1 = 1       |
        private SqlBinaryExpression ExpandNegatedNullableEqualNullable(
            SqlExpression left, SqlExpression right, SqlExpression leftIsNull, SqlExpression rightIsNull)
            => _sqlExpressionFactory.OrElse(
                _sqlExpressionFactory.AndAlso(
                    _sqlExpressionFactory.NotEqual(left, right),
                    _sqlExpressionFactory.AndAlso(
                        _sqlExpressionFactory.Not(leftIsNull),
                        _sqlExpressionFactory.Not(rightIsNull))),
                _sqlExpressionFactory.AndAlso(
                    leftIsNull,
                    rightIsNull));

        // ?a == b -> (a == b) && (a != null)
        //
        // a | b | F1 = a == b | F2 = (a != null) | Final = F1 && F2 |
        //   |   |             |                  |                  |
        // 0 | 0 | 1           | 1                | 1                |
        // 0 | 1 | 0           | 1                | 0                |
        // 1 | 0 | 0           | 1                | 0                |
        // 1 | 1 | 1           | 1                | 1                |
        // N | 0 | N           | 0                | 0                |
        // N | 1 | N           | 0                | 0                |
        private SqlBinaryExpression ExpandNullableEqualNonNullable(
            SqlExpression left, SqlExpression right, SqlExpression leftIsNull)
            => _sqlExpressionFactory.AndAlso(
                _sqlExpressionFactory.Equal(left, right),
                _sqlExpressionFactory.Not(leftIsNull));

        // !(?a) == b -> (a != b) && (a != null)
        //
        // a | b | F1 = a != b | F2 = (a != null) | Final = F1 && F2 |
        //   |   |             |                  |                  |
        // 0 | 0 | 0           | 1                | 0                |
        // 0 | 1 | 1           | 1                | 1                |
        // 1 | 0 | 1           | 1                | 1                |
        // 1 | 1 | 0           | 1                | 0                |
        // N | 0 | N           | 0                | 0                |
        // N | 1 | N           | 0                | 0                |
        private SqlBinaryExpression ExpandNegatedNullableEqualNonNullable(
            SqlExpression left, SqlExpression right, SqlExpression leftIsNull)
            => _sqlExpressionFactory.AndAlso(
                _sqlExpressionFactory.NotEqual(left, right),
                _sqlExpressionFactory.Not(leftIsNull));

        // ?a != ?b -> [(a != b) || (a == null || b == null)] && (a != null || b != null)
        //
        // a | b | F1 = a != b | F2 = (a == null || b == null) | F3 = F1 || F2 |
        //   |   |             |                               |               |
        // 0 | 0 | 0           | 0                             | 0             |
        // 0 | 1 | 1           | 0                             | 1             |
        // 0 | N | N           | 1                             | 1             |
        // 1 | 0 | 1           | 0                             | 1             |
        // 1 | 1 | 0           | 0                             | 0             |
        // 1 | N | N           | 1                             | 1             |
        // N | 0 | N           | 1                             | 1             |
        // N | 1 | N           | 1                             | 1             |
        // N | N | N           | 1                             | 1             |
        //
        // a | b | F4 = (a != null || b != null) | Final = F3 && F4 |
        //   |   |                               |                  |
        // 0 | 0 | 1                             | 0 && 1 = 0       |
        // 0 | 1 | 1                             | 1 && 1 = 1       |
        // 0 | N | 1                             | 1 && 1 = 1       |
        // 1 | 0 | 1                             | 1 && 1 = 1       |
        // 1 | 1 | 1                             | 0 && 1 = 0       |
        // 1 | N | 1                             | 1 && 1 = 1       |
        // N | 0 | 1                             | 1 && 1 = 1       |
        // N | 1 | 1                             | 1 && 1 = 1       |
        // N | N | 0                             | 1 && 0 = 0       |
        private SqlBinaryExpression ExpandNullableNotEqualNullable(
            SqlExpression left, SqlExpression right, SqlExpression leftIsNull, SqlExpression rightIsNull)
            => _sqlExpressionFactory.AndAlso(
                _sqlExpressionFactory.OrElse(
                    _sqlExpressionFactory.NotEqual(left, right),
                    _sqlExpressionFactory.OrElse(
                        leftIsNull,
                        rightIsNull)),
                _sqlExpressionFactory.OrElse(
                    _sqlExpressionFactory.Not(leftIsNull),
                    _sqlExpressionFactory.Not(rightIsNull)));

        // !(?a) != ?b -> [(a == b) || (a == null || b == null)] && (a != null || b != null)
        //
        // a | b | F1 = a == b | F2 = (a == null || b == null) | F3 = F1 || F2 |
        //   |   |             |                               |               |
        // 0 | 0 | 1           | 0                             | 1             |
        // 0 | 1 | 0           | 0                             | 0             |
        // 0 | N | N           | 1                             | 1             |
        // 1 | 0 | 0           | 0                             | 0             |
        // 1 | 1 | 1           | 0                             | 1             |
        // 1 | N | N           | 1                             | 1             |
        // N | 0 | N           | 1                             | 1             |
        // N | 1 | N           | 1                             | 1             |
        // N | N | N           | 1                             | 1             |
        //
        // a | b | F4 = (a != null || b != null) | Final = F3 && F4 |
        //   |   |                               |                  |
        // 0 | 0 | 1                             | 1 && 1 = 1       |
        // 0 | 1 | 1                             | 0 && 1 = 0       |
        // 0 | N | 1                             | 1 && 1 = 1       |
        // 1 | 0 | 1                             | 0 && 1 = 0       |
        // 1 | 1 | 1                             | 1 && 1 = 1       |
        // 1 | N | 1                             | 1 && 1 = 1       |
        // N | 0 | 1                             | 1 && 1 = 1       |
        // N | 1 | 1                             | 1 && 1 = 1       |
        // N | N | 0                             | 1 && 0 = 0       |
        private SqlBinaryExpression ExpandNegatedNullableNotEqualNullable(
            SqlExpression left, SqlExpression right, SqlExpression leftIsNull, SqlExpression rightIsNull)
            => _sqlExpressionFactory.AndAlso(
                _sqlExpressionFactory.OrElse(
                    _sqlExpressionFactory.Equal(left, right),
                    _sqlExpressionFactory.OrElse(
                        leftIsNull,
                        rightIsNull)),
                _sqlExpressionFactory.OrElse(
                    _sqlExpressionFactory.Not(leftIsNull),
                    _sqlExpressionFactory.Not(rightIsNull)));

        // ?a != b -> (a != b) || (a == null)
        //
        // a | b | F1 = a != b | F2 = (a == null) | Final = F1 OR F2 |
        //   |   |             |                  |                  |
        // 0 | 0 | 0           | 0                | 0                |
        // 0 | 1 | 1           | 0                | 1                |
        // 1 | 0 | 1           | 0                | 1                |
        // 1 | 1 | 0           | 0                | 0                |
        // N | 0 | N           | 1                | 1                |
        // N | 1 | N           | 1                | 1                |
        private SqlBinaryExpression ExpandNullableNotEqualNonNullable(
            SqlExpression left, SqlExpression right, SqlExpression leftIsNull)
            => _sqlExpressionFactory.OrElse(
                _sqlExpressionFactory.NotEqual(left, right),
                leftIsNull);

        // !(?a) != b -> (a == b) || (a == null)
        //
        // a | b | F1 = a == b | F2 = (a == null) | F3 = F1 OR F2 |
        //   |   |             |                  |               |
        // 0 | 0 | 1           | 0                | 1             |
        // 0 | 1 | 0           | 0                | 0             |
        // 1 | 0 | 0           | 0                | 0             |
        // 1 | 1 | 1           | 0                | 1             |
        // N | 0 | N           | 1                | 1             |
        // N | 1 | N           | 1                | 1             |
        private SqlBinaryExpression ExpandNegatedNullableNotEqualNonNullable(
            SqlExpression left, SqlExpression right, SqlExpression leftIsNull)
            => _sqlExpressionFactory.OrElse(
                _sqlExpressionFactory.Equal(left, right),
                leftIsNull);
    }
}
