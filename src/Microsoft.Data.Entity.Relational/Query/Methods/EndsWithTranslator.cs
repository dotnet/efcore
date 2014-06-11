// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Entity.Relational.Query.Expressions;

namespace Microsoft.Data.Entity.Relational.Query.Methods
{
    public class EndsWithTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(string).GetRuntimeMethod("EndsWith", new[] { typeof(string) });

        private static readonly MethodInfo _concat
            = typeof(string).GetRuntimeMethod("Concat", new[] { typeof(string), typeof(string) });

        public virtual Expression Translate(MethodCallExpression expression)
        {
            if (ReferenceEquals(expression.Method, _methodInfo))
            {
                return new LikeExpression(
                    expression.Object,
                    Expression.Add(new LiteralExpression("%"), expression.Arguments[0], _concat));
            }

            return null;
        }
    }
}
