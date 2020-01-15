// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IgnoreCheckedExpressionVisitor: ExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var visitedLeft = Visit(node.Left);
            var visitedRight = Visit(node.Right);

            var visitBinary = node.NodeType switch
                              {
                                  ExpressionType.AddChecked      => Expression.Add(visitedLeft, visitedRight),
                                  ExpressionType.SubtractChecked => Expression.Subtract(visitedLeft, visitedRight),
                                  ExpressionType.MultiplyChecked => Expression.Multiply(visitedLeft, visitedRight),
                                  _                              => base.VisitBinary(node)
                              };

            return visitBinary;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            var operand = Visit(node.Operand);

            var visitUnary = node.NodeType switch
                             {
                                 ExpressionType.ConvertChecked => Expression.Convert(operand, node.Type),
                                 _                             => base.VisitUnary(node)
                             };

            return visitUnary;
        }
    }
}
