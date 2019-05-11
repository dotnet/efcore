// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class NegationOptimizingVisitor : ExpressionVisitor
    {
        private readonly Dictionary<ExpressionType, ExpressionType> _expressionTypesNegationMap
            = new Dictionary<ExpressionType, ExpressionType>
            {
                { ExpressionType.AndAlso, ExpressionType.OrElse },
                { ExpressionType.OrElse, ExpressionType.AndAlso },
                { ExpressionType.Equal, ExpressionType.NotEqual },
                { ExpressionType.NotEqual, ExpressionType.Equal },
                { ExpressionType.GreaterThan, ExpressionType.LessThanOrEqual },
                { ExpressionType.GreaterThanOrEqual, ExpressionType.LessThan },
                { ExpressionType.LessThan, ExpressionType.GreaterThanOrEqual },
                { ExpressionType.LessThanOrEqual, ExpressionType.GreaterThan },
            };

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
                                _expressionTypesNegationMap[innerBinary.NodeType],
                                Expression.Not(innerBinary.Left),
                                Expression.Not(innerBinary.Right)));
                    }

                    if (_expressionTypesNegationMap.ContainsKey(innerBinary.NodeType))
                    {
                        return Visit(
                            Expression.MakeBinary(
                                _expressionTypesNegationMap[innerBinary.NodeType],
                                innerBinary.Left,
                                innerBinary.Right));
                    }
                }
            }

            return base.VisitUnary(unaryExpression);
        }
    }
}
