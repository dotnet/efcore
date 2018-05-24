// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteStringTrimStartTranslator : IMethodCallTranslator
    {
        // Method defined in netcoreapp2.0 only
        private static readonly MethodInfo _methodInfoWithoutArgs
            = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), Array.Empty<Type>());

        // Method defined in netcoreapp2.0 only
        private static readonly MethodInfo _methodInfoWithCharArg
            = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new[] { typeof(char) });

        // Method defined in netstandard2.0
        private static readonly MethodInfo _methodInfoWithCharArrayArg
            = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new[] { typeof(char[]) });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            var methodInfo = methodCallExpression.Method;

            if (_methodInfoWithoutArgs?.Equals(methodInfo) == true
                || _methodInfoWithCharArg?.Equals(methodInfo) == true
                || _methodInfoWithCharArrayArg.Equals(methodInfo))
            {
                var sqlArguments = new List<Expression>
                {
                    methodCallExpression.Object
                };

                if (methodCallExpression.Arguments.Count == 1)
                {
                    var constantValue = (methodCallExpression.Arguments[0] as ConstantExpression)?.Value;
                    var charactersToTrim = new List<char>();

                    if (constantValue is char singleChar)
                    {
                        charactersToTrim.Add(singleChar);
                    }
                    else if (constantValue is char[] charArray)
                    {
                        charactersToTrim.AddRange(charArray);
                    }

                    if (charactersToTrim.Count > 0)
                    {
                        sqlArguments.Add(Expression.Constant(new string(charactersToTrim.ToArray()), typeof(string)));
                    }
                }

                return new SqlFunctionExpression("ltrim", methodCallExpression.Type, sqlArguments);
            }

            return null;
        }
    }
}
