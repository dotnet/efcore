// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class SqlServerStringTrimEndTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _trimEnd = typeof(string).GetTypeInfo()
            .GetDeclaredMethod(nameof(string.TrimEnd));

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if ((_trimEnd == methodCallExpression.Method)
                // SqlServer RTRIM does not take arguments
                && (((methodCallExpression.Arguments[0] as ConstantExpression)?.Value as Array)?.Length == 0))
            {
                var sqlArguments = new[] { methodCallExpression.Object };
                return new SqlFunctionExpression("RTRIM", methodCallExpression.Type, sqlArguments);
            }

            return null;
        }
    }
}
