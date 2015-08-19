// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Query.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class NullSemanticsExpandingVisitor : NullSemanticsExpressionVisitorBase
    {
        protected override Expression VisitBinary(BinaryExpression expression)
        {
            var left = Visit(expression.Left);
            var right = Visit(expression.Right);
            if (expression.NodeType == ExpressionType.Equal
                || expression.NodeType == ExpressionType.NotEqual)
            {
                var leftIsNull = BuildIsNullExpression(left);
                var rightIsNull = BuildIsNullExpression(right);
                var leftNullable = leftIsNull != null;
                var rightNullable = rightIsNull != null;

                Type conversionResultTypeLeft;
                Type conversionResultTypeRight;
                var unwrappedConvertLeft = UnwrapConvertExpression(left, out conversionResultTypeLeft);
                var unwrappedConvertRight = UnwrapConvertExpression(right, out conversionResultTypeRight);

                var leftUnary = unwrappedConvertLeft as UnaryExpression;
                var leftNegated = leftUnary != null && leftUnary.NodeType == ExpressionType.Not;
                var rightUnary = unwrappedConvertRight as UnaryExpression;
                var rightNegated = rightUnary != null && rightUnary.NodeType == ExpressionType.Not;

                var leftOperand = leftNegated
                    ? conversionResultTypeLeft == null
                        ? leftUnary.Operand
                        : Expression.Convert(leftUnary.Operand, conversionResultTypeLeft)
                    : left;

                var rightOperand = rightNegated
                    ? conversionResultTypeRight == null
                        ? rightUnary.Operand
                        : Expression.Convert(rightUnary.Operand, conversionResultTypeRight)
                    : right;

                if (expression.NodeType == ExpressionType.Equal)
                {
                    if (leftNullable && rightNullable)
                    {
                        if (leftNegated == rightNegated)
                        {
                            return ExpandNullableEqualNullable(leftOperand, rightOperand, leftIsNull, rightIsNull);
                        }

                        return ExpandNegatedNullableEqualNullable(leftOperand, rightOperand, leftIsNull, rightIsNull);
                    }

                    if (leftNullable && !rightNullable)
                    {
                        if (leftNegated == rightNegated)
                        {
                            return ExpandNullableEqualNonNullable(leftOperand, rightOperand, leftIsNull);
                        }

                        return ExpandNegatedNullableEqualNonNullable(leftOperand, rightOperand, leftIsNull);
                    }

                    if (!leftNullable && rightNullable)
                    {
                        if (leftNegated == rightNegated)
                        {
                            return ExpandNonNullableEqualNullable(leftOperand, rightOperand, rightIsNull);
                        }

                        return ExpandNegatedNonNullableEqualNullable(leftOperand, rightOperand, rightIsNull);
                    }
                }

                if (expression.NodeType == ExpressionType.NotEqual)
                {
                    if (leftNullable && rightNullable)
                    {
                        if (leftNegated == rightNegated)
                        {
                            return ExpandNullableNotEqualNullable(leftOperand, rightOperand, leftIsNull, rightIsNull);
                        }

                        return ExpandNegatedNullableNotEqualNullable(leftOperand, rightOperand, leftIsNull, rightIsNull);
                    }

                    if (leftNullable && !rightNullable)
                    {
                        if (leftNegated == rightNegated)
                        {
                            return ExpandNullableNotEqualNonNullable(leftOperand, rightOperand, leftIsNull);
                        }

                        return ExpandNegatedNullableNotEqualNonNullable(leftOperand, rightOperand, leftIsNull);
                    }

                    if (!leftNullable && rightNullable)
                    {
                        if (leftNegated == rightNegated)
                        {
                            return ExpandNonNullableNotEqualNullable(leftOperand, rightOperand, rightIsNull);
                        }

                        return ExpandNegatedNonNullableNotEqualNullable(leftOperand, rightOperand, rightIsNull);
                    }
                }
            }

            if (left == expression.Left && right == expression.Right)
            {
                return expression;
            }

            return Expression.MakeBinary(expression.NodeType, left, right, expression.IsLiftedToNull, expression.Method);
        }

        protected override Expression VisitExtension(Expression expression)
        {
            var notNullableExpression = expression as NotNullableExpression;

            return notNullableExpression != null
                ? expression
                : base.VisitExtension(expression);
        }

        private Expression UnwrapConvertExpression(Expression expression, out Type conversionResultType)
        {
            var unary = expression as UnaryExpression;
            if (unary != null
                && unary.NodeType == ExpressionType.Convert)
            {
                conversionResultType = unary.Type;
                return unary.Operand;
            }

            conversionResultType = null;
            return expression;
        }

        private Expression ExpandNullableEqualNullable(
            Expression left,
            Expression right,
            Expression leftIsNull,
            Expression rightIsNull)
        {
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
            return new NotNullableExpression(
                Expression.OrElse(
                    Expression.AndAlso(
                        Expression.Equal(left, right),
                        Expression.AndAlso(
                            Expression.Not(leftIsNull),
                            Expression.Not(rightIsNull)
                            )
                        ),
                    Expression.AndAlso(
                        leftIsNull,
                        rightIsNull
                        )
                    )
                );
        }

        private Expression ExpandNegatedNullableEqualNullable(
            Expression left,
            Expression right,
            Expression leftIsNull,
            Expression rightIsNull)
        {
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
            return new NotNullableExpression(
                Expression.OrElse(
                    Expression.AndAlso(
                        Expression.NotEqual(left, right),
                        Expression.AndAlso(
                            Expression.Not(leftIsNull),
                            Expression.Not(rightIsNull)
                            )
                        ),
                    Expression.AndAlso(
                        leftIsNull,
                        rightIsNull
                        )
                    )
                );
        }

        private Expression ExpandNullableEqualNonNullable(
            Expression left,
            Expression right,
            Expression leftIsNull)
        {
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
            return new NotNullableExpression(
                Expression.AndAlso(
                    Expression.Equal(left, right),
                    Expression.Not(leftIsNull)
                    )
                );
        }

        private Expression ExpandNegatedNullableEqualNonNullable(
            Expression left,
            Expression right,
            Expression leftIsNull)
        {
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
            return new NotNullableExpression(
                Expression.AndAlso(
                    Expression.NotEqual(left, right),
                    Expression.Not(leftIsNull)
                    )
                );
        }

        private Expression ExpandNonNullableEqualNullable(
            Expression left,
            Expression right,
            Expression rightIsNull)
        {
            // a == ?b -> (a == b) && (b != null)
            //
            // a | b | F1 = a == b | F2 = (b != null) | Final = F1 && F2 | 
            //   |   |             |                  |                  |
            // 0 | 0 | 1           | 1                | 1                |
            // 0 | 1 | 0           | 1                | 0                |
            // 0 | N | N           | 0                | 0                |
            // 1 | 0 | 0           | 1                | 0                |
            // 1 | 1 | 1           | 1                | 1                |
            // 1 | N | N           | 0                | 0                |
            return new NotNullableExpression(
                Expression.AndAlso(
                    Expression.Equal(left, right),
                    Expression.Not(rightIsNull)
                    )
                );
        }

        private Expression ExpandNegatedNonNullableEqualNullable(
            Expression left,
            Expression right,
            Expression rightIsNull)
        {
            // !a == ?b -> (a != b) && (b != null)
            //
            // a | b | F1 = a != b | F2 = (b != null) | Final = F1 && F2 | 
            //   |   |             |                  |                  |
            // 0 | 0 | 0           | 1                | 0                |
            // 0 | 1 | 1           | 1                | 1                |
            // 0 | N | N           | 0                | 0                |
            // 1 | 0 | 1           | 1                | 1                |
            // 1 | 1 | 0           | 1                | 0                |
            // 1 | N | N           | 0                | 0                |
            return new NotNullableExpression(
                Expression.AndAlso(
                    Expression.NotEqual(left, right),
                    Expression.Not(rightIsNull)
                    )
                );
        }

        private Expression ExpandNullableNotEqualNullable(
            Expression left,
            Expression right,
            Expression leftIsNull,
            Expression rightIsNull)
        {
            // ?a != ?b -> [(a != b) || (a == null || b == null)] && (a != null || b != null)]
            //
            // a | b | F1 = a != b | F2 = (a == null || b == null) | F3 = F1 && F2 | 
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
            return new NotNullableExpression(
                Expression.AndAlso(
                    Expression.OrElse(
                        Expression.NotEqual(left, right),
                        Expression.OrElse(
                            leftIsNull,
                            rightIsNull
                            )
                        ),
                    Expression.OrElse(
                        Expression.Not(leftIsNull),
                        Expression.Not(rightIsNull)
                        )
                    )
                );
        }

        private Expression ExpandNegatedNullableNotEqualNullable(
            Expression left,
            Expression right,
            Expression leftIsNull,
            Expression rightIsNull)
        {
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
            return new NotNullableExpression(
                Expression.AndAlso(
                    Expression.OrElse(
                        Expression.Equal(left, right),
                        Expression.OrElse(
                            leftIsNull,
                            rightIsNull
                            )
                        ),
                    Expression.OrElse(
                        Expression.Not(leftIsNull),
                        Expression.Not(rightIsNull)
                        )
                    )
                );
        }

        private Expression ExpandNullableNotEqualNonNullable(
            Expression left,
            Expression right,
            Expression leftIsNull)
        {
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
            return new NotNullableExpression(
                Expression.OrElse(
                    Expression.NotEqual(left, right),
                    leftIsNull
                    )
                );
        }

        private Expression ExpandNegatedNullableNotEqualNonNullable(
            Expression left,
            Expression right,
            Expression leftIsNull)
        {
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
            return new NotNullableExpression(
                Expression.OrElse(
                    Expression.Equal(left, right),
                    leftIsNull
                    )
                );
        }

        private Expression ExpandNonNullableNotEqualNullable(
            Expression left,
            Expression right,
            Expression rightIsNull)
        {
            // a != ?b -> (a != b) || (b == null)
            //
            // a | b | F1 = a != b | F2 = (b == null) | F3 = F1 OR F2 | 
            //   |   |             |                  |               |
            // 0 | 0 | 0           | 0                | 0             |
            // 0 | 1 | 1           | 0                | 1             |
            // 0 | N | N           | 1                | 1             |
            // 1 | 0 | 1           | 0                | 1             |
            // 1 | 1 | 0           | 0                | 0             |
            // 1 | N | N           | 1                | 1             |
            return new NotNullableExpression(
                Expression.OrElse(
                    Expression.NotEqual(left, right),
                    rightIsNull
                    )
                );
        }

        private Expression ExpandNegatedNonNullableNotEqualNullable(
            Expression left,
            Expression right,
            Expression rightIsNull)
        {
            // !a != ?b -> (a == b) || (b == null)
            //
            // a | b | F1 = a == b | F2 = (b == null) | F3 = F1 OR F2 | 
            //   |   |             |                  |               |
            // 0 | 0 | 1           | 0                | 1             |
            // 0 | 1 | 0           | 0                | 0             |
            // 0 | N | N           | 1                | 1             |
            // 1 | 0 | 0           | 0                | 0             |
            // 1 | 1 | 1           | 0                | 1             |
            // 1 | N | N           | 1                | 1             |
            return new NotNullableExpression(
                Expression.OrElse(
                    Expression.Equal(left, right),
                    rightIsNull
                    )
                );
        }
    }
}
