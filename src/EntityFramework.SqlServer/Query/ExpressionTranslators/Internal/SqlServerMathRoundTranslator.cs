// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Entity.Query.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionTranslators.Internal
{
    public class SqlServerMathRoundTranslator : IMethodCallTranslator
    {
        private static readonly IEnumerable<MethodInfo> _methodInfos = typeof(Math).GetTypeInfo().GetDeclaredMethods(nameof(Math.Round))
            .Where(m => m.GetParameters().Count() == 1
                        || (m.GetParameters().Count() == 2 && m.GetParameters()[1].ParameterType == typeof(int)));

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
