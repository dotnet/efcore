// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.Methods;

namespace Microsoft.Data.Entity.SqlServer.Query.Methods
{
    public class StringSubstringTranslator : IMethodCallTranslator
    {
        public virtual Expression Translate([NotNull] MethodCallExpression methodCallExpression)
        {
            var methodInfo = typeof(string).GetTypeInfo().GetDeclaredMethods("Substring")
                .Where(m => m.GetParameters().Count() == 2)
                .Single();

            if (methodCallExpression.Method == methodInfo)
            {
                var sqlArguments = new[] { methodCallExpression.Object }.Concat(methodCallExpression.Arguments);
                return new SqlFunctionExpression("SUBSTRING", sqlArguments, methodCallExpression.Type);
            }

            return null;
        }
    }
}
