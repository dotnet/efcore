// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerMathRoundTranslator : IMethodCallTranslator
    {
        private static readonly IEnumerable<MethodInfo> _methodInfos = typeof(Math).GetTypeInfo().GetDeclaredMethods(nameof(Math.Round))
            .Where(m => (m.GetParameters().Length == 1)
                        || ((m.GetParameters().Length == 2) && (m.GetParameters()[1].ParameterType == typeof(int))));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
            => _methodInfos.Contains(methodCallExpression.Method)
                ? new SqlFunctionExpression(
                    "ROUND",
                    methodCallExpression.Type,
                    methodCallExpression.Arguments.Count == 1
                        ? new[] { methodCallExpression.Arguments[0], Expression.Constant(0) }
                        : new[] { methodCallExpression.Arguments[1], methodCallExpression.Arguments[1] })
                : null;
    }
}
