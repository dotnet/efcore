// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerFreeTextMethodCallTranslator : IMethodCallTranslator
    {
        private const string FunctionName = "FREETEXT";

        private static readonly MethodInfo _methodInfo
            = typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.FreeText),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        private static readonly MethodInfo _methodInfoWithLanguage
            = typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.FreeText),
                new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(int) });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            return Equals(methodCallExpression.Method, _methodInfo)
                ? new SqlFunctionExpression(
                    FunctionName,
                    typeof(bool),
                    new Expression[]
                    {
                        methodCallExpression.Arguments[1],
                        methodCallExpression.Arguments[2]
                    })
                : Equals(methodCallExpression.Method, _methodInfoWithLanguage)
                    ? new SqlFunctionExpression(
                        FunctionName,
                        typeof(bool),
                        new Expression[]
                        {
                            methodCallExpression.Arguments[1],
                            methodCallExpression.Arguments[2],
                            new SqlFragmentExpression(
                                $"LANGUAGE {((ConstantExpression)methodCallExpression.Arguments[3]).Value}")
                        })
                    : null;
        }
    }
}
