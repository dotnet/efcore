// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class EqualityPredicateExpandingVisitor : RelinqExpressionVisitor
    {
        protected override Expression VisitBinary(
            [NotNull] BinaryExpression expression)
        {
            var left = Visit(expression.Left);
            var right = Visit(expression.Right);

            if ((expression.NodeType == ExpressionType.Equal
                 || expression.NodeType == ExpressionType.NotEqual)
                && expression.Left.Type == typeof(bool)
                && expression.Right.Type == typeof(bool))
            {
                var simpleLeft = expression.Left.IsSimpleExpression();
                var simpleRight = expression.Right.IsSimpleExpression();

                if (!simpleLeft
                    || !simpleRight)
                {
                    var leftOperand = simpleLeft
                        ? left
                        : Expression.Condition(
                            left,
                            Expression.Constant(true),
                            Expression.Constant(false),
                            typeof(bool));

                    var rightOperand = simpleRight
                        ? right
                        : Expression.Condition(
                            right,
                            Expression.Constant(true),
                            Expression.Constant(false),
                            typeof(bool));

                    return expression.NodeType == ExpressionType.Equal
                        ? Expression.Equal(leftOperand, rightOperand)
                        : Expression.NotEqual(leftOperand, rightOperand);
                }
            }

            if (left == expression.Left
                && right == expression.Right)
            {
                return expression;
            }

            return Expression.MakeBinary(
                expression.NodeType, 
                left, 
                right, 
                expression.IsLiftedToNull, 
                expression.Method);
        }
    }
}
