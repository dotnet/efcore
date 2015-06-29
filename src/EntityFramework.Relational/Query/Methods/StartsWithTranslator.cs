// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Methods
{
    public class StartsWithTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(string).GetRuntimeMethod("StartsWith", new[] { typeof(string) });

        private static readonly MethodInfo _concat
            = typeof(string).GetRuntimeMethod("Concat", new[] { typeof(string), typeof(string) });

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            return ReferenceEquals(methodCallExpression.Method, _methodInfo)
                ? new LikeExpression(
                    methodCallExpression.Object,
                    Expression.Add(methodCallExpression.Arguments[0], new LiteralExpression("%"), _concat))
                : null;
        }
    }
}
