// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class RelationalNullsOptimizedExpandingVisitor : RelationalNullsExpressionVisitorBase
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsOptimalExpansion { get; private set; } = true;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var newLeft = Visit(node.Left);
            var newRight = Visit(node.Right);

            if (!IsOptimalExpansion)
            {
                return node;
            }

            if (node.NodeType == ExpressionType.Equal
                || node.NodeType == ExpressionType.NotEqual)
            {
                var leftIsNull = BuildIsNullExpression(newLeft);
                var rightIsNull = BuildIsNullExpression(newRight);

                var leftNullable = leftIsNull != null;
                var rightNullable = rightIsNull != null;

                if (node.NodeType == ExpressionType.Equal
                    && leftNullable
                    && rightNullable)
                {
                    return Expression.OrElse(
                        Expression.Equal(newLeft, newRight),
                        Expression.AndAlso(leftIsNull, rightIsNull));
                }

                if (node.NodeType == ExpressionType.NotEqual
                    && (leftNullable || rightNullable))
                {
                    IsOptimalExpansion = false;
                }
            }

            return node.Update(newLeft, node.Conversion, newRight);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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
