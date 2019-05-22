// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class EqualityPredicateExpandingVisitor : RelinqExpressionVisitor
    {
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

            if ((node.NodeType == ExpressionType.Equal
                 || node.NodeType == ExpressionType.NotEqual)
                && node.Left.Type == typeof(bool)
                && node.Right.Type == typeof(bool))
            {
                var simpleLeft = node.Left.IsSimpleExpression();
                var simpleRight = node.Right.IsSimpleExpression();

                if (!simpleLeft
                    || !simpleRight)
                {
                    var leftOperand = simpleLeft
                        ? newLeft
                        : Expression.Condition(
                            newLeft,
                            Expression.Constant(true),
                            Expression.Constant(false),
                            typeof(bool));

                    var rightOperand = simpleRight
                        ? newRight
                        : Expression.Condition(
                            newRight,
                            Expression.Constant(true),
                            Expression.Constant(false),
                            typeof(bool));

                    return node.NodeType == ExpressionType.Equal
                        ? Expression.Equal(leftOperand, rightOperand)
                        : Expression.NotEqual(leftOperand, rightOperand);
                }
            }

            return node.Update(newLeft, node.Conversion, newRight);
        }
    }
}
