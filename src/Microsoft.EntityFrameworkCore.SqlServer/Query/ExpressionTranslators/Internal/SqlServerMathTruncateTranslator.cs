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
    public class SqlServerMathTruncateTranslator : IMethodCallTranslator
    {
        private static readonly IEnumerable<MethodInfo> _methodInfos = typeof(Math).GetTypeInfo().GetDeclaredMethods(nameof(Math.Truncate));

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
            => _methodInfos.Contains(methodCallExpression.Method)
                ? new SqlFunctionExpression(
                    "ROUND",
                    methodCallExpression.Type,
                    new[] { methodCallExpression.Arguments[0], Expression.Constant(0), Expression.Constant(1) })
                : null;
    }
}
