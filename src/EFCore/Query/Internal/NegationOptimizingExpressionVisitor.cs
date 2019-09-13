// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class NegationOptimizingExpressionVisitor : ExpressionVisitor
    {
        private static bool TryNegate(ExpressionType expressionType, out ExpressionType result)
        {
            var negated = expressionType switch
            {
                ExpressionType.AndAlso => ExpressionType.OrElse,
                ExpressionType.OrElse => ExpressionType.AndAlso,
                ExpressionType.Equal => ExpressionType.NotEqual,
                ExpressionType.NotEqual => ExpressionType.Equal,
                ExpressionType.GreaterThan => ExpressionType.LessThanOrEqual,
                ExpressionType.GreaterThanOrEqual => ExpressionType.LessThan,
                ExpressionType.LessThan => ExpressionType.GreaterThanOrEqual,
                ExpressionType.LessThanOrEqual => ExpressionType.GreaterThan,
                _ => (ExpressionType?)null
            };

            result = negated ?? default;
            return negated.HasValue;
        }

        private static ExpressionType Negate(ExpressionType expressionType)
            => TryNegate(expressionType, out var result) ? result : throw new KeyNotFoundException();

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            if (unaryExpression.NodeType == ExpressionType.Not)
            {
                if (unaryExpression.Operand is ConstantExpression innerConstant
                    && innerConstant.Value is bool value)
                {
                    // !(true) -> false
                    // !(false) -> true
                    return Expression.Constant(!value);
                }

                if (unaryExpression.Operand is UnaryExpression innerUnary
                    && innerUnary.NodeType == ExpressionType.Not)
                {
                    // !(!a) -> a
                    return Visit(innerUnary.Operand);
                }

                if (unaryExpression.Operand is BinaryExpression innerBinary)
                {
                    // De Morgan's
                    if (innerBinary.NodeType == ExpressionType.AndAlso
                        || innerBinary.NodeType == ExpressionType.OrElse)
                    {
                        return Visit(
                            Expression.MakeBinary(
                                Negate(innerBinary.NodeType),
                                Expression.Not(innerBinary.Left),
                                Expression.Not(innerBinary.Right)));
                    }

                    if (TryNegate(innerBinary.NodeType, out var negated))
                    {
                        return Visit(
                            Expression.MakeBinary(
                                negated,
                                innerBinary.Left,
                                innerBinary.Right));
                    }
                }
            }

            return base.VisitUnary(unaryExpression);
        }
    }
}
