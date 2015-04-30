// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class EqualityPredicateExpandingVisitor : ExpressionTreeVisitor
    {
        protected override Expression VisitBinaryExpression(
            [NotNull] BinaryExpression expression)
        {
            var left = VisitExpression(expression.Left);
            var right = VisitExpression(expression.Right);

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
                    var leftOperand = simpleLeft ? left : new CaseExpression(left, typeof(bool));
                    var rightOperand = simpleRight ? right : new CaseExpression(right, typeof(bool));

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
            return Expression.MakeBinary(expression.NodeType, left, right);
        }
    }
}
