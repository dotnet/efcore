// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerFreeTextMethodCallTranslator : IMethodCallTranslator
    {
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

            if(Equals(methodCallExpression.Method, _methodInfo))
            {
                return new SqlFunctionExpression(
                    "FREETEXT",
                    typeof(bool),
                    new Expression[]
                    {
                        methodCallExpression.Arguments[1],
                        methodCallExpression.Arguments[2]
                    }
                );
            }

            return Equals(methodCallExpression.Method, _methodInfoWithLanguage) ?
                new SqlFunctionExpression(
                    "FREETEXT",
                    typeof(bool),
                    new Expression[]
                    {
                        methodCallExpression.Arguments[1],
                        methodCallExpression.Arguments[2],
                        new SqlFragmentExpression($"LANGUAGE {((ConstantExpression)methodCallExpression.Arguments[3]).Value}")
                    }
                ): null;
        }
    }
}
