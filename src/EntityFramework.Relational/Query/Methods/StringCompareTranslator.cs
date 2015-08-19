// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Entity.Query.Expressions;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query.Methods
{
    public abstract class StringCompareTranslator : IMethodCallTranslator
    {
        public static string StringCompareMethodName = "StringCompare";

        private static readonly MethodInfo _methodInfo = typeof(string).GetTypeInfo().GetDeclaredMethods(nameof(string.Compare))
            .Where(m => m.GetParameters().Count() == 2)
            .Single();

        public virtual Expression Translate([NotNull] MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method == _methodInfo)
            {
                var arguments = methodCallExpression.Arguments.ToList();
                var leftString = arguments[0];
                var rightString = arguments[1];

                return new SqlFunctionExpression(
                    StringCompareMethodName,
                    new[] { leftString, rightString },
                    typeof(int));
            }

            return null;
        }
    }
}
