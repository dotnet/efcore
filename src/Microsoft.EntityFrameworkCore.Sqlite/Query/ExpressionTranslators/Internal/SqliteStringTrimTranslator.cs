// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class SqliteStringTrimTranslator : IMethodCallTranslator
    {
        private static readonly IEnumerable<MethodInfo> _trims = typeof(string).GetTypeInfo()
            .GetDeclaredMethods(nameof(string.Trim));

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (_trims.Contains(methodCallExpression.Method))
            {
                var sqlArguments = new List<Expression> { methodCallExpression.Object };
                var charactersToTrim = methodCallExpression.Arguments.Count == 1
                    ? (methodCallExpression.Arguments[0] as ConstantExpression)?.Value as char[]
                    : null;
                if (charactersToTrim?.Length > 0)
                {
                    sqlArguments.Add(Expression.Constant(new string(charactersToTrim), typeof(string)));
                }
                return new SqlFunctionExpression("trim", methodCallExpression.Type, sqlArguments);
            }

            return null;
        }
    }
}
