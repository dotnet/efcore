// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class SqliteStringTrimEndTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _trimEnd = typeof(string).GetTypeInfo()
            .GetDeclaredMethod(nameof(string.TrimEnd));

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (_trimEnd == methodCallExpression.Method)
            {
                var sqlArguments = new List<Expression> { methodCallExpression.Object };
                var charactersToTrim = (methodCallExpression.Arguments[0] as ConstantExpression)?.Value as char[];
                if (charactersToTrim?.Length > 0)
                {
                    sqlArguments.Add(Expression.Constant(new string(charactersToTrim), typeof(string)));
                }
                return new SqlFunctionExpression("rtrim", methodCallExpression.Type, sqlArguments);
            }

            return null;
        }
    }
}
