// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IgnoreCheckedExpressionVisitor: ExpressionVisitor
    {
        public static Expression Ignore([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return new IgnoreCheckedExpressionVisitor().Visit(expression);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var visitedLeft = Visit(node.Left);
            var visitedRight = Visit(node.Right);

            return node.NodeType switch
                   {
                       ExpressionType.AddChecked => Expression.Add(visitedLeft, visitedRight),
                       ExpressionType.SubtractChecked => Expression.Subtract(visitedLeft, visitedRight),
                       ExpressionType.MultiplyChecked => Expression.Multiply(visitedLeft, visitedRight),
                       _ => node.Update(visitedLeft, node.Conversion, visitedRight)
                   };
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            var operand = Visit(node.Operand);

            return node.NodeType switch
                   {
                       ExpressionType.ConvertChecked => Expression.Convert(operand, node.Type),
                       _ => node.Update(operand)
                   };
        }
    }
}
