// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.Data.Entity.Relational.Query.Methods
{
    public class EqualsTranslator : IMethodCallTranslator
    {
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.Name == "Equals" && methodCallExpression.Arguments.Count == 1)
            {
                bool isBooleanOperand = methodCallExpression.Object.Type == typeof(bool);

                var operand = isBooleanOperand 
                    ? TranslateBooleanEqualsArgument(methodCallExpression.Object) 
                    : methodCallExpression.Object;

                var argument = isBooleanOperand
                    ? TranslateBooleanEqualsArgument(methodCallExpression.Arguments[0])
                    : methodCallExpression.Arguments[0];

                return Expression.Equal(operand, argument);
            }

            return null;
        }

        private Expression TranslateBooleanEqualsArgument(Expression expression)
        {
            var binaryExpression = expression as BinaryExpression;
            if (binaryExpression != null && binaryExpression.NodeType == ExpressionType.Equal)
            {
                var rightConstant = binaryExpression.Right as ConstantExpression;
                if (rightConstant != null 
                    && rightConstant.Type == typeof(bool) 
                    && (bool)rightConstant.Value)
                {
                    return binaryExpression.Left;
                }
            }

            return expression;
        }
    }
}
