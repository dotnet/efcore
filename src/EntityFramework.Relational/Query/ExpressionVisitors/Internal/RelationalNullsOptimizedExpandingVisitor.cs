// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class RelationalNullsOptimizedExpandingVisitor : RelationalNullsExpressionVisitorBase
    {
        public virtual bool IsOptimalExpansion { get; private set; } = true;

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var newLeft = Visit(binaryExpression.Left);
            var newRight = Visit(binaryExpression.Right);

            if (!IsOptimalExpansion)
            {
                return binaryExpression;
            }

            if (binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                var leftIsNull = BuildIsNullExpression(newLeft);
                var rightIsNull = BuildIsNullExpression(newRight);

                var leftNullable = leftIsNull != null;
                var rightNullable = rightIsNull != null;

                if (binaryExpression.NodeType == ExpressionType.Equal
                    && leftNullable
                    && rightNullable)
                {
                    return Expression.OrElse(
                        Expression.Equal(newLeft, newRight),
                        Expression.AndAlso(leftIsNull, rightIsNull));
                }

                if (binaryExpression.NodeType == ExpressionType.NotEqual
                    && (leftNullable || rightNullable))
                {
                    IsOptimalExpansion = false;
                }
            }

            return binaryExpression.Update(newLeft, binaryExpression.Conversion, newRight);
        }

        protected override Expression VisitUnary(UnaryExpression expression)
        {
            var operand = Visit(expression.Operand);

            if (!IsOptimalExpansion)
            {
                return expression;
            }

            if (expression.NodeType == ExpressionType.Not)
            {
                IsOptimalExpansion = false;
            }

            return expression.Update(operand);
        }
    }
}
