// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteMathTranslator : IMethodCallTranslator
    {
        private static readonly Dictionary<MethodInfo, string> _supportedMethods = new Dictionary<MethodInfo, string>
        {
            { typeof(Math).GetMethod(nameof(Math.Round), new[]{ typeof(double) }), "round" },
            { typeof(Math).GetMethod(nameof(Math.Round), new[]{ typeof(double), typeof(int) }), "round" },
            { typeof(Math).GetMethod(nameof(Math.Round), new[]{ typeof(decimal) }), "round" },
            { typeof(Math).GetMethod(nameof(Math.Round), new[]{ typeof(decimal), typeof(int) }), "round" }
        };

        static SqliteMathTranslator()
        {
            AddSupportedTranslation(typeof(Math), nameof(Math.Abs), "abs");
            AddSupportedTranslation(typeof(Math), nameof(Math.Max), "max");
            AddSupportedTranslation(typeof(Math), nameof(Math.Min), "min");
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
            => _supportedMethods.TryGetValue(methodCallExpression.Method, out var sqlFunctionName)
                ? new SqlFunctionExpression(sqlFunctionName, methodCallExpression.Type, methodCallExpression.Arguments)
                : null;

        private static void AddSupportedTranslation(Type declaringType, string clrMethodName, string sqlFunctionName)
        {
            var methods = declaringType.GetTypeInfo().GetDeclaredMethods(clrMethodName);
            foreach (var methodInfo in methods)
            {
                _supportedMethods.Add(methodInfo, sqlFunctionName);
            }
        }
    }
}
