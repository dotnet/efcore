// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.Oracle.Query.ExpressionTranslators.Internal
{
    public class OracleContainsOptimizedTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) });

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (Equals(methodCallExpression.Method, _methodInfo))
            {
                var patternExpression = methodCallExpression.Arguments[0];

                if (patternExpression is ConstantExpression patternConstantExpression
                    && ((string)patternConstantExpression.Value)?.Length == 0)
                {
                    return Expression.Constant(true);
                }

                // TODO: Use EmptyStringCompensatingExpression

                return
                    Expression.GreaterThan(
                        new SqlFunctionExpression(
                            "INSTR",
                            typeof(int),
                            new[]
                            {
                                methodCallExpression.Object,
                                patternExpression
                            }),
                        Expression.Constant(0));
            }

            return null;
        }
    }
}
