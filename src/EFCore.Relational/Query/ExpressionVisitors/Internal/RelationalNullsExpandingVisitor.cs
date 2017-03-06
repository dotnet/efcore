// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RelationalNullsExpandingVisitor : RelationalNullsExpressionVisitorBase
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var newLeft = Visit(node.Left);
            var newRight = Visit(node.Right);

            if ((node.NodeType == ExpressionType.Equal)
                || (node.NodeType == ExpressionType.NotEqual))
            {
                var leftIsNull = BuildIsNullExpression(newLeft);
                var leftNullable = leftIsNull != null;

                var rightIsNull = BuildIsNullExpression(newRight);
                var rightNullable = rightIsNull != null;

                Type conversionResultTypeLeft;
                Type conversionResultTypeRight;

                var unwrappedConvertLeft = UnwrapConvertExpression(newLeft, out conversionResultTypeLeft);
                var unwrappedConvertRight = UnwrapConvertExpression(newRight, out conversionResultTypeRight);

                var leftUnary = unwrappedConvertLeft as UnaryExpression;
                var leftNegated = (leftUnary != null) && (leftUnary.NodeType == ExpressionType.Not);

                var rightUnary = unwrappedConvertRight as UnaryExpression;
                var rightNegated = (rightUnary != null) && (rightUnary.NodeType == ExpressionType.Not);

                var leftOperand
                    = leftNegated
                        ? conversionResultTypeLeft == null
                            ? leftUnary.Operand
                            : Expression.Convert(leftUnary.Operand, conversionResultTypeLeft)
                        : newLeft;

                var rightOperand
                    = rightNegated
                        ? conversionResultTypeRight == null
                            ? rightUnary.Operand
                            : Expression.Convert(rightUnary.Operand, conversionResultTypeRight)
                        : newRight;

                if (node.NodeType == ExpressionType.Equal)
                {
                    if (leftNullable && rightNullable)
                    {
                        return leftNegated == rightNegated
                            ? ExpandNullableEqualNullable(leftOperand, rightOperand, leftIsNull, rightIsNull)
                            : ExpandNegatedNullableEqualNullable(leftOperand, rightOperand, leftIsNull, rightIsNull);
                    }

                    if (leftNullable)
                    {
                        return leftNegated == rightNegated
                            ? ExpandNullableEqualNonNullable(leftOperand, rightOperand, leftIsNull)
                            : ExpandNegatedNullableEqualNonNullable(leftOperand, rightOperand, leftIsNull);
                    }

                    if (rightNullable)
                    {
                        return leftNegated == rightNegated
                            ? ExpandNonNullableEqualNullable(leftOperand, rightOperand, rightIsNull)
                            : ExpandNegatedNonNullableEqualNullable(leftOperand, rightOperand, rightIsNull);
                    }
                }

                if (node.NodeType == ExpressionType.NotEqual)
                {
                    if (leftNullable && rightNullable)
                    {
                        return leftNegated == rightNegated
                            ? ExpandNullableNotEqualNullable(leftOperand, rightOperand, leftIsNull, rightIsNull)
                            : ExpandNegatedNullableNotEqualNullable(leftOperand, rightOperand, leftIsNull, rightIsNull);
                    }

                    if (leftNullable)
                    {
                        return leftNegated == rightNegated
                            ? ExpandNullableNotEqualNonNullable(leftOperand, rightOperand, leftIsNull)
                            : ExpandNegatedNullableNotEqualNonNullable(leftOperand, rightOperand, leftIsNull);
                    }

                    if (rightNullable)
                    {
                        return leftNegated == rightNegated
                            ? ExpandNonNullableNotEqualNullable(leftOperand, rightOperand, rightIsNull)
                            : ExpandNegatedNonNullableNotEqualNullable(leftOperand, rightOperand, rightIsNull);
                    }
                }
            }

            return node.Update(newLeft, node.Conversion, newRight);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitExtension(Expression node)
            => node is NotNullableExpression
                ? node
                : base.VisitExtension(node);

        private static Expression UnwrapConvertExpression(Expression expression, out Type conversionResultType)
        {
            var unary = expression as UnaryExpression;

            if ((unary != null)
                && (unary.NodeType == ExpressionType.Convert))
            {
                conversionResultType = unary.Type;

                return unary.Operand;
            }

            conversionResultType = null;

            return expression;
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
        private static Expression ExpandNullableEqualNullable(Expression left, Expression right, Expression leftIsNull, Expression rightIsNull)
            => new NotNullableExpression(
                Expression.OrElse(
                    Expression.AndAlso(
                        Expression.Equal(left, right),
                        Expression.AndAlso(
                            Expression.Not(leftIsNull),
                            Expression.Not(rightIsNull))),
                    Expression.AndAlso(
                        leftIsNull,
                        rightIsNull)));

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
        private static Expression ExpandNegatedNullableEqualNullable(
            Expression left, Expression right, Expression leftIsNull, Expression rightIsNull)
            => new NotNullableExpression(
                Expression.OrElse(
                    Expression.AndAlso(
                        Expression.NotEqual(left, right),
                        Expression.AndAlso(
                            Expression.Not(leftIsNull),
                            Expression.Not(rightIsNull))),
                    Expression.AndAlso(
                        leftIsNull,
                        rightIsNull)));

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
        private static Expression ExpandNullableEqualNonNullable(
            Expression left, Expression right, Expression leftIsNull)
            => new NotNullableExpression(
                Expression.AndAlso(
                    Expression.Equal(left, right),
                    Expression.Not(leftIsNull)));

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
        private static Expression ExpandNegatedNullableEqualNonNullable(
            Expression left, Expression right, Expression leftIsNull)
            => new NotNullableExpression(
                Expression.AndAlso(
                    Expression.NotEqual(left, right),
                    Expression.Not(leftIsNull)));

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
        private static Expression ExpandNonNullableEqualNullable(
            Expression left, Expression right, Expression rightIsNull)
            => new NotNullableExpression(
                Expression.AndAlso(
                    Expression.Equal(left, right),
                    Expression.Not(rightIsNull)));

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
        private static Expression ExpandNegatedNonNullableEqualNullable(
            Expression left, Expression right, Expression rightIsNull)
            => new NotNullableExpression(
                Expression.AndAlso(
                    Expression.NotEqual(left, right),
                    Expression.Not(rightIsNull)));

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
        private static Expression ExpandNullableNotEqualNullable(
            Expression left, Expression right, Expression leftIsNull, Expression rightIsNull)
            => new NotNullableExpression(
                Expression.AndAlso(
                    Expression.OrElse(
                        Expression.NotEqual(left, right),
                        Expression.OrElse(
                            leftIsNull,
                            rightIsNull)),
                    Expression.OrElse(
                        Expression.Not(leftIsNull),
                        Expression.Not(rightIsNull))));

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
        private static Expression ExpandNegatedNullableNotEqualNullable(
            Expression left, Expression right, Expression leftIsNull, Expression rightIsNull)
            => new NotNullableExpression(
                Expression.AndAlso(
                    Expression.OrElse(
                        Expression.Equal(left, right),
                        Expression.OrElse(
                            leftIsNull,
                            rightIsNull)),
                    Expression.OrElse(
                        Expression.Not(leftIsNull),
                        Expression.Not(rightIsNull))));

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
        private static Expression ExpandNullableNotEqualNonNullable(
            Expression left, Expression right, Expression leftIsNull)
            => new NotNullableExpression(
                Expression.OrElse(
                    Expression.NotEqual(left, right),
                    leftIsNull));

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
        private static Expression ExpandNegatedNullableNotEqualNonNullable(
            Expression left, Expression right, Expression leftIsNull)
            => new NotNullableExpression(
                Expression.OrElse(
                    Expression.Equal(left, right),
                    leftIsNull));

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
        private static Expression ExpandNonNullableNotEqualNullable(
            Expression left, Expression right, Expression rightIsNull)
            => new NotNullableExpression(
                Expression.OrElse(
                    Expression.NotEqual(left, right),
                    rightIsNull));

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
        private static Expression ExpandNegatedNonNullableNotEqualNullable(
            Expression left, Expression right, Expression rightIsNull)
            => new NotNullableExpression(
                Expression.OrElse(
                    Expression.Equal(left, right),
                    rightIsNull));
    }
}
