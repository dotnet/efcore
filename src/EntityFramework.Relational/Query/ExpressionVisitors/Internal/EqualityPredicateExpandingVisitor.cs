// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class EqualityPredicateExpandingVisitor : RelinqExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var newLeft = Visit(binaryExpression.Left);
            var newRight = Visit(binaryExpression.Right);

            if (((binaryExpression.NodeType == ExpressionType.Equal)
                 || (binaryExpression.NodeType == ExpressionType.NotEqual))
                && (binaryExpression.Left.Type == typeof(bool))
                && (binaryExpression.Right.Type == typeof(bool)))
            {
                var simpleLeft = binaryExpression.Left.IsSimpleExpression();
                var simpleRight = binaryExpression.Right.IsSimpleExpression();

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

                    return binaryExpression.NodeType == ExpressionType.Equal
                        ? Expression.Equal(leftOperand, rightOperand)
                        : Expression.NotEqual(leftOperand, rightOperand);
                }
            }

            return binaryExpression.Update(newLeft, binaryExpression.Conversion, newRight);
        }
    }
}
