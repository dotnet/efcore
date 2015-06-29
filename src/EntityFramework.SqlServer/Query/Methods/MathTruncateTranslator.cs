// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.Methods;

namespace Microsoft.Data.Entity.SqlServer.Query.Methods
{
    public class MathTruncateTranslator : IMethodCallTranslator
    {
        public virtual Expression Translate([NotNull] MethodCallExpression methodCallExpression)
        {
            var methodInfos = typeof(Math).GetTypeInfo().GetDeclaredMethods("Truncate");
            if (methodInfos.Contains(methodCallExpression.Method))
            {
                var arguments = new[] { methodCallExpression.Arguments[0], Expression.Constant(0), Expression.Constant(1) };
                return new SqlFunctionExpression("ROUND", arguments, methodCallExpression.Type);
            }

            return null;
        }
    }
}
