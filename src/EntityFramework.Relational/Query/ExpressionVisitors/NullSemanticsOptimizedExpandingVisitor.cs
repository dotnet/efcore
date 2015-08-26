// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class NullSemanticsOptimizedExpandingVisitor : NullSemanticsExpressionVisitorBase
    {
        public virtual bool OptimizedExpansionPossible { get; set; }

        public NullSemanticsOptimizedExpandingVisitor()
        {
            OptimizedExpansionPossible = true;
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            var left = Visit(expression.Left);
            var right = Visit(expression.Right);

            if (!OptimizedExpansionPossible)
            {
                return expression;
            }

            if (expression.NodeType == ExpressionType.Equal
                || expression.NodeType == ExpressionType.NotEqual)
            {
                var leftIsNull = BuildIsNullExpression(left);
                var rightIsNull = BuildIsNullExpression(right);
                var leftNullable = leftIsNull != null;
                var rightNullable = rightIsNull != null;

                if (expression.NodeType == ExpressionType.Equal
                    && leftNullable
                    && rightNullable)
                {
                    return Expression.OrElse(
                        Expression.Equal(left, right),
                        Expression.AndAlso(
                            leftIsNull,
                            rightIsNull
                            )
                        );
                }

                if (expression.NodeType == ExpressionType.NotEqual
                    && (leftNullable || rightNullable))
                {
                    OptimizedExpansionPossible = false;
                }
            }

            if (left == expression.Left && right == expression.Right)
            {
                return expression;
            }

            return Expression.MakeBinary(expression.NodeType, left, right, expression.IsLiftedToNull, expression.Method);
        }

        protected override Expression VisitUnary(UnaryExpression expression)
        {
            var operand = Visit(expression.Operand);
            if (!OptimizedExpansionPossible)
            {
                return expression;
            }

            if (expression.NodeType == ExpressionType.Not)
            {
                OptimizedExpansionPossible = false;
            }

            if (operand == expression.Operand)
            {
                return expression;
            }
            return Expression.MakeUnary(expression.NodeType, operand, expression.Type);
        }
    }
}
