// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ConditionalRemovingExpressionVisitor : ExpressionVisitorBase
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Expression Visit(Expression node)
        {
            if (node is SelectExpression selectExpression
                && selectExpression.Projection.Count == 1)
            {
                var conditionalExpression = selectExpression.Projection.First() as ConditionalExpression;

                if (conditionalExpression?.Type == typeof(bool)
                    && conditionalExpression.IfTrue.NodeType == ExpressionType.Constant
                    && conditionalExpression.IfFalse.NodeType == ExpressionType.Constant
                    && (bool)((ConstantExpression)conditionalExpression.IfTrue).Value
                    && !(bool)((ConstantExpression)conditionalExpression.IfFalse).Value)
                {
                    return Visit(conditionalExpression.Test);
                }
            }

            return base.Visit(node);
        }
    }
}
