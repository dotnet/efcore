// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    public class PredicateReductionExpressionOptimizer : RelinqExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.IsLogicalOperation())
            {
                var newLeft = Visit(node.Left);
                var newRight = Visit(node.Right);
                var constantLeft = newLeft as ConstantExpression;
                var constantRight = newRight as ConstantExpression;

                if (node.NodeType == ExpressionType.AndAlso)
                {
                    if ((constantLeft != null)
                        && (constantLeft.Type == typeof(bool)))
                    {
                        // true && a => a
                        // false && a => false
                        return (bool)constantLeft.Value ? newRight : newLeft;
                    }

                    if ((constantRight != null)
                       && (constantRight.Type == typeof(bool)))
                    {
                        // a && true => a
                        // a && false => false
                        return (bool)constantRight.Value ? newLeft : newRight;
                    }
                }

                if (node.NodeType == ExpressionType.OrElse)
                {
                    if ((constantLeft != null)
                        && (constantLeft.Type == typeof(bool)))
                    {
                        // true || a => true
                        // false || a => a
                        return (bool)constantLeft.Value ? newLeft : newRight;
                    }

                    if ((constantRight != null)
                       && (constantRight.Type == typeof(bool)))
                    {
                        // a || true => true
                        // a || false => a
                        return (bool)constantRight.Value ? newRight : newLeft;
                    }
                }
            }

            return base.VisitBinary(node);
        }
    }
}
