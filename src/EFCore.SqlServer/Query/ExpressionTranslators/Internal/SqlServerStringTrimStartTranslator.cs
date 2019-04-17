// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerStringTrimStartTranslator : IMethodCallTranslator
    {
        // Method defined in netcoreapp2.0 only
        private static readonly MethodInfo _methodInfoWithoutArgs
            = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), Array.Empty<Type>());

        // Method defined in netstandard2.0
        private static readonly MethodInfo _methodInfoWithCharArrayArg
            = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new[] { typeof(char[]) });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression Translate(
            MethodCallExpression methodCallExpression,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (_methodInfoWithoutArgs?.Equals(methodCallExpression.Method) == true
                || _methodInfoWithCharArrayArg.Equals(methodCallExpression.Method)
                // SqlServer LTRIM does not take arguments
                && ((methodCallExpression.Arguments[0] as ConstantExpression)?.Value as Array)?.Length == 0)
            {
                var sqlArguments = new[] { methodCallExpression.Object };

                return new SqlFunctionExpression("LTRIM", methodCallExpression.Type, sqlArguments);
            }

            return null;
        }
    }
}
