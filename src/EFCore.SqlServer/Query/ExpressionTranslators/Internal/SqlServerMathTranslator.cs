// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerMathTranslator : IMethodCallTranslator
    {
        private static readonly Dictionary<string, string> _supportedMethodTranslations = new Dictionary<string, string>
        {
            { nameof(Math.Abs), "ABS" },
            { nameof(Math.Ceiling), "CEILING" },
            { nameof(Math.Floor), "FLOOR" },
            { nameof(Math.Pow), "POWER" },
            { nameof(Math.Exp), "EXP" },
            { nameof(Math.Log10), "LOG10" },
            { nameof(Math.Log), "LOG" },
            { nameof(Math.Sqrt), "SQRT" },
            { nameof(Math.Acos), "ACOS" },
            { nameof(Math.Asin), "ASIN" },
            { nameof(Math.Atan), "ATAN" },
            { nameof(Math.Atan2), "ATN2" },
            { nameof(Math.Cos), "COS" },
            { nameof(Math.Sin), "SIN" },
            { nameof(Math.Tan), "TAN" },
            { nameof(Math.Sign), "SIGN" }
        };

        private static readonly IEnumerable<MethodInfo> _roundMethodInfos = typeof(Math).GetTypeInfo().GetDeclaredMethods(nameof(Math.Round))
            .Where(m => m.GetParameters().Length == 1
                        || m.GetParameters().Length == 2 && m.GetParameters()[1].ParameterType == typeof(int));

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            var method = methodCallExpression.Method;
            if (method.DeclaringType == typeof(Math))
            {
                if (_supportedMethodTranslations.TryGetValue(method.Name, out string sqlFunctionName))
                {
                    return new SqlFunctionExpression(
                        sqlFunctionName,
                        methodCallExpression.Type,
                        methodCallExpression.Arguments);
                }

                if (method.Name == nameof(Math.Truncate))
                {
                    return new SqlFunctionExpression(
                        "ROUND",
                        methodCallExpression.Type,
                        new[] { methodCallExpression.Arguments[0], Expression.Constant(0), Expression.Constant(1) });
                }

                if (_roundMethodInfos.Contains(method))
                {
                    return new SqlFunctionExpression(
                        "ROUND",
                        methodCallExpression.Type,
                        methodCallExpression.Arguments.Count == 1
                            ? new[] { methodCallExpression.Arguments[0], Expression.Constant(0) }
                            : new[] { methodCallExpression.Arguments[0], methodCallExpression.Arguments[1] });
                }
            }

            return null;
        }
    }
}
