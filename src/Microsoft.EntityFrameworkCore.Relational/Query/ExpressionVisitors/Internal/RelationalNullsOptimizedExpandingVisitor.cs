// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    public class RelationalNullsOptimizedExpandingVisitor : RelationalNullsExpressionVisitorBase
    {
        public virtual bool IsOptimalExpansion { get; private set; } = true;

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var newLeft = Visit(node.Left);
            var newRight = Visit(node.Right);

            if (!IsOptimalExpansion)
            {
                return node;
            }

            if ((node.NodeType == ExpressionType.Equal)
                || (node.NodeType == ExpressionType.NotEqual))
            {
                var leftIsNull = BuildIsNullExpression(newLeft);
                var rightIsNull = BuildIsNullExpression(newRight);

                var leftNullable = leftIsNull != null;
                var rightNullable = rightIsNull != null;

                if ((node.NodeType == ExpressionType.Equal)
                    && leftNullable
                    && rightNullable)
                {
                    return Expression.OrElse(
                        Expression.Equal(newLeft, newRight),
                        Expression.AndAlso(leftIsNull, rightIsNull));
                }

                if ((node.NodeType == ExpressionType.NotEqual)
                    && (leftNullable || rightNullable))
                {
                    IsOptimalExpansion = false;
                }
            }

            return node.Update(newLeft, node.Conversion, newRight);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            var operand = Visit(node.Operand);

            if (!IsOptimalExpansion)
            {
                return node;
            }

            if (node.NodeType == ExpressionType.Not)
            {
                IsOptimalExpansion = false;
            }

            return node.Update(operand);
        }
    }
}
