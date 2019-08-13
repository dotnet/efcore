// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class NullSemanticsRewritingExpressionVisitor : ExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        private bool _isNullable = false;

        public NullSemanticsRewritingExpressionVisitor(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case SqlConstantExpression sqlConstantExpression:
                    return VisitSqlConstantExpression(sqlConstantExpression);

                case ColumnExpression columnExpression:
                    return VisitColumnExpression(columnExpression);

                case SqlParameterExpression sqlParameterExpression:
                    return VisitSqlParameterExpression(sqlParameterExpression);

                case SqlUnaryExpression sqlUnaryExpression:
                    return VisitSqlUnaryExpression(sqlUnaryExpression);

                case LikeExpression likeExpression:
                    return VisitLikeExpression(likeExpression);

                case SqlFunctionExpression sqlFunctionExpression:
                    return VisitSqlFunctionExpression(sqlFunctionExpression);

                case SqlBinaryExpression sqlBinaryExpression:
                    return VisitSqlBinaryExpression(sqlBinaryExpression);

                case CaseExpression caseExpression:
                    return VisitCaseExpression(caseExpression);

                case InnerJoinExpression innerJoinExpression:
                    return VisitInnerJoinExpression(innerJoinExpression);

                case LeftJoinExpression leftJoinExpression:
                    return VisitLeftJoinExpression(leftJoinExpression);

                case ScalarSubqueryExpression subSelectExpression:
                    var result = base.VisitExtension(subSelectExpression);
                    _isNullable = true;

                    return result;

                default:
                    return base.VisitExtension(extensionExpression);
            }
        }

        private SqlConstantExpression VisitSqlConstantExpression(SqlConstantExpression sqlConstantExpression)
        {
            _isNullable = sqlConstantExpression.Value == null;

            return sqlConstantExpression;
        }

        private ColumnExpression VisitColumnExpression(ColumnExpression columnExpression)
        {
            _isNullable = columnExpression.IsNullable;

            return columnExpression;
        }

        private SqlParameterExpression VisitSqlParameterExpression(SqlParameterExpression sqlParameterExpression)
        {
            // at this point we assume every parameter is nullable, we will filter out the non-nullable ones once we know the actual values
            _isNullable = true;

            return sqlParameterExpression;
        }

        private SqlUnaryExpression VisitSqlUnaryExpression(SqlUnaryExpression sqlUnaryExpression)
        {
            _isNullable = false;
            var newOperand = (SqlExpression)Visit(sqlUnaryExpression.Operand);

            // IsNull/IsNotNull
            if (sqlUnaryExpression.OperatorType == ExpressionType.Equal
                || sqlUnaryExpression.OperatorType == ExpressionType.NotEqual)
            {
                _isNullable = false;
            }

            return sqlUnaryExpression.Update(newOperand);
        }

        private LikeExpression VisitLikeExpression(LikeExpression likeExpression)
        {
            _isNullable = false;
            var newMatch = (SqlExpression)Visit(likeExpression.Match);
            var isNullable = _isNullable;
            var newPattern = (SqlExpression)Visit(likeExpression.Pattern);
            isNullable |= _isNullable;
            var newEscapeChar = (SqlExpression)Visit(likeExpression.EscapeChar);
            _isNullable |= isNullable;

            return likeExpression.Update(newMatch, newPattern, newEscapeChar);
        }

        private InnerJoinExpression VisitInnerJoinExpression(InnerJoinExpression innerJoinExpression)
        {
            var newTable = (TableExpressionBase)Visit(innerJoinExpression.Table);
            var newJoinPredicate = VisitJoinPredicate((SqlBinaryExpression)innerJoinExpression.JoinPredicate);

            return innerJoinExpression.Update(newTable, newJoinPredicate);
        }

        private LeftJoinExpression VisitLeftJoinExpression(LeftJoinExpression leftJoinExpression)
        {
            var newTable = (TableExpressionBase)Visit(leftJoinExpression.Table);
            var newJoinPredicate = VisitJoinPredicate((SqlBinaryExpression)leftJoinExpression.JoinPredicate);

            return leftJoinExpression.Update(newTable, newJoinPredicate);
        }

        private SqlExpression VisitJoinPredicate(SqlBinaryExpression predicate)
        {
            if (predicate.OperatorType == ExpressionType.Equal)
            {
                var newLeft = (SqlExpression)Visit(predicate.Left);
                var newRight = (SqlExpression)Visit(predicate.Right);

                return predicate.Update(newLeft, newRight);
            }

            if (predicate.OperatorType == ExpressionType.AndAlso)
            {
                return VisitSqlBinaryExpression(predicate);
            }

            throw new InvalidOperationException("Unexpected join predicate shape: " + predicate);
        }

        private CaseExpression VisitCaseExpression(CaseExpression caseExpression)
        {
            _isNullable = false;
            // if there is no 'else' there is a possibility of null, when none of the conditions are met
            // otherwise the result is nullable if any of the WhenClause results OR ElseResult is nullable
            var isNullable = caseExpression.ElseResult == null;

            var newOperand = (SqlExpression)Visit(caseExpression.Operand);
            var newWhenClauses = new List<CaseWhenClause>();
            foreach (var whenClause in caseExpression.WhenClauses)
            {
                var newTest = (SqlExpression)Visit(whenClause.Test);
                var newResult = (SqlExpression)Visit(whenClause.Result);
                isNullable |= _isNullable;
                newWhenClauses.Add(new CaseWhenClause(newTest, newResult));
            }

            var newElseResult = (SqlExpression)Visit(caseExpression.ElseResult);
            _isNullable |= isNullable;

            return caseExpression.Update(newOperand, newWhenClauses, newElseResult);
        }

        private SqlFunctionExpression VisitSqlFunctionExpression(SqlFunctionExpression sqlFunctionExpression)
        {
            _isNullable = false;
            var newInstance = (SqlExpression)Visit(sqlFunctionExpression.Instance);
            var isNullable = _isNullable;
            var newArguments = new SqlExpression[sqlFunctionExpression.Arguments.Count];
            for (var i = 0; i < newArguments.Length; i++)
            {
                newArguments[i] = (SqlExpression)Visit(sqlFunctionExpression.Arguments[i]);
                isNullable |= _isNullable;
            }

            _isNullable = isNullable;

            return sqlFunctionExpression.Update(newInstance, newArguments);
        }

        private SqlBinaryExpression VisitSqlBinaryExpression(SqlBinaryExpression sqlBinaryExpression)
        {
            _isNullable = false;
            var newLeft = (SqlExpression)Visit(sqlBinaryExpression.Left);
            var leftNullable = _isNullable;

            _isNullable = false;
            var newRight = (SqlExpression)Visit(sqlBinaryExpression.Right);
            var rightNullable = _isNullable;

            if (sqlBinaryExpression.OperatorType == ExpressionType.Coalesce)
            {
                _isNullable = leftNullable && rightNullable;

                return sqlBinaryExpression.Update(newLeft, newRight);
            }

            if (sqlBinaryExpression.OperatorType == ExpressionType.Equal
                || sqlBinaryExpression.OperatorType == ExpressionType.NotEqual)
            {
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

                // TODO: optimize this by looking at subcomponents, e.g. f(a, b) == null <=> a == null || b == null
                var leftIsNull = _sqlExpressionFactory.IsNull(newLeft);
                var rightIsNull = _sqlExpressionFactory.IsNull(newRight);

                // doing a full null semantics rewrite - removing all nulls from truth table
                _isNullable = false;

                if (sqlBinaryExpression.OperatorType == ExpressionType.Equal)
                {
                    if (!leftNullable && !rightNullable)
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
                    if (!leftNullable && !rightNullable)
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

            return sqlBinaryExpression.Update(newLeft, newRight);
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
