// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class NullSemanticsExpandingVisitor : NullSemanticsExpressionVisitorBase
    {
        protected override Expression VisitBinaryExpression(BinaryExpression expression)
        {
            var left = VisitExpression(expression.Left);
            var right = VisitExpression(expression.Right);
            if (expression.NodeType == ExpressionType.Equal || expression.NodeType == ExpressionType.NotEqual)
            {
                var leftNullables = ExtractNullableExpressions(left);
                var rightNullables = ExtractNullableExpressions(right);
                var leftNullable = leftNullables.Count > 0;
                var rightNullable = rightNullables.Count > 0;

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
                            return ExpandNullableEqualNullable(leftOperand, rightOperand, leftNullables, rightNullables);
                        }

                        return ExpandNegatedNullableEqualNullable(leftOperand, rightOperand, leftNullables, rightNullables);
                    }

                    if (leftNullable && !rightNullable)
                    {
                        if (leftNegated == rightNegated)
                        {
                            return ExpandNullableEqualNonNullable(leftOperand, rightOperand, leftNullables);
                        }

                        return ExpandNegatedNullableEqualNonNullable(leftOperand, rightOperand, leftNullables);
                    }

                    if (!leftNullable && rightNullable)
                    {
                        if (leftNegated == rightNegated)
                        {
                            return ExpandNonNullableEqualNullable(leftOperand, rightOperand, rightNullables);
                        }

                        return ExpandNegatedNonNullableEqualNullable(leftOperand, rightOperand, rightNullables);
                    }
                }

                if (expression.NodeType == ExpressionType.NotEqual)
                {
                    if (leftNullable && rightNullable)
                    {
                        if (leftNegated == rightNegated)
                        {
                            return ExpandNullableNotEqualNullable(leftOperand, rightOperand, leftNullables, rightNullables);
                        }

                        return ExpandNegatedNullableNotEqualNullable(leftOperand, rightOperand, leftNullables, rightNullables);
                    }

                    if (leftNullable && !rightNullable)
                    {
                        if (leftNegated == rightNegated)
                        {
                            return ExpandNullableNotEqualNonNullable(leftOperand, rightOperand, leftNullables);
                        }

                        return ExpandNegatedNullableNotEqualNonNullable(leftOperand, rightOperand, leftNullables);
                    }

                    if (!leftNullable && rightNullable)
                    {
                        if (leftNegated == rightNegated)
                        {
                            return ExpandNonNullableNotEqualNullable(leftOperand, rightOperand, rightNullables);
                        }

                        return ExpandNegatedNonNullableNotEqualNullable(leftOperand, rightOperand, rightNullables);
                    }
                }
            }

            if (left == expression.Left && right == expression.Right)
            {
                return expression;
            }
            else
            {
                return Expression.MakeBinary(expression.NodeType, left, right);
            }
        }

        protected override Expression VisitExtensionExpression(ExtensionExpression expression)
        {
            var notNullableExpression = expression as NotNullableExpression;
            if (notNullableExpression != null)
            {
                return expression;
            }

            return base.VisitExtensionExpression(expression);
        }

        private Expression UnwrapConvertExpression(Expression expression, out Type conversionResultType)
        {
            var unary = expression as UnaryExpression;
            if (unary != null && unary.NodeType == ExpressionType.Convert)
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
            List<Expression> leftNullables,
            List<Expression> rightNullables)
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
                            BuildIsNotNullExpression(leftNullables),
                            BuildIsNotNullExpression(rightNullables)
                        )
                    ),
                    Expression.AndAlso(
                        BuildIsNullExpression(leftNullables),
                        BuildIsNullExpression(rightNullables)
                    )
                )
            );
        }

        private Expression ExpandNegatedNullableEqualNullable(
            Expression left,
            Expression right,
            List<Expression> leftNullables,
            List<Expression> rightNullables)
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
                            BuildIsNotNullExpression(leftNullables),
                            BuildIsNotNullExpression(rightNullables)
                        )
                    ),
                    Expression.AndAlso(
                        BuildIsNullExpression(leftNullables),
                        BuildIsNullExpression(rightNullables)
                    )
                )
            );
        }

        private Expression ExpandNullableEqualNonNullable(
            Expression left,
            Expression right,
            List<Expression> leftNullables)
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
                    BuildIsNotNullExpression(leftNullables)
                )
            );
        }

        private Expression ExpandNegatedNullableEqualNonNullable(
            Expression left,
            Expression right,
            List<Expression> leftNullables)
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
                    BuildIsNotNullExpression(leftNullables)
                )
            );
        }

        private Expression ExpandNonNullableEqualNullable(
            Expression left,
            Expression right,
            List<Expression> rightNullables)
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
                    BuildIsNotNullExpression(rightNullables)
                )
            );
        }

        private Expression ExpandNegatedNonNullableEqualNullable(
            Expression left,
            Expression right,
            List<Expression> rightNullables)
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
                    BuildIsNotNullExpression(rightNullables)
                )
            );
        }

        private Expression ExpandNullableNotEqualNullable(
            Expression left,
            Expression right,
            List<Expression> leftNullables,
            List<Expression> rightNullables)
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
                            BuildIsNullExpression(leftNullables),
                            BuildIsNullExpression(rightNullables)
                        )
                    ),
                    Expression.OrElse(
                        BuildIsNotNullExpression(leftNullables),
                        BuildIsNotNullExpression(rightNullables)
                    )
                )
            );
        }

        private Expression ExpandNegatedNullableNotEqualNullable(
            Expression left,
            Expression right,
            List<Expression> leftNullables,
            List<Expression> rightNullables)
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
                            BuildIsNullExpression(leftNullables),
                            BuildIsNullExpression(rightNullables)
                        )
                    ),
                    Expression.OrElse(
                        BuildIsNotNullExpression(leftNullables),
                        BuildIsNotNullExpression(rightNullables)
                    )
                )
            );
        }

        private Expression ExpandNullableNotEqualNonNullable(
            Expression left,
            Expression right,
            List<Expression> leftNullables)
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
                    BuildIsNullExpression(leftNullables)
                )
            );
        }

        private Expression ExpandNegatedNullableNotEqualNonNullable(
            Expression left,
            Expression right,
            List<Expression> leftNullables)
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
                    BuildIsNullExpression(leftNullables)
                )
            );
        }

        private Expression ExpandNonNullableNotEqualNullable(
            Expression left,
            Expression right,
            List<Expression> rightNullables)
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
                    BuildIsNullExpression(rightNullables)
                )
            );
        }

        private Expression ExpandNegatedNonNullableNotEqualNullable(
            Expression left,
            Expression right,
            List<Expression> rightNullables)
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
                    BuildIsNullExpression(rightNullables)
                )
            );
        }
    }
}
