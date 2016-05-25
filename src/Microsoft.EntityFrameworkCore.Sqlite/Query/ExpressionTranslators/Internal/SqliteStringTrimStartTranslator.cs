// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class SqliteStringTrimStartTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _trimStart = typeof(string).GetTypeInfo()
            .GetDeclaredMethod(nameof(string.TrimStart));

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (_trimStart == methodCallExpression.Method)
            {
                var sqlArguments = new List<Expression> { methodCallExpression.Object };
                var charactersToTrim = (methodCallExpression.Arguments[0] as ConstantExpression)?.Value as char[];
                if (charactersToTrim?.Length > 0)
                {
                    sqlArguments.Add(Expression.Constant(new string(charactersToTrim), typeof(string)));
                }
                return new SqlFunctionExpression("ltrim", methodCallExpression.Type, sqlArguments);
            }

            return null;
        }
    }
}
