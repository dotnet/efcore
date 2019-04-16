// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerStringConcatMethodCallTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _stringConcatMethodInfo
            = typeof(string).GetRuntimeMethod(
                nameof(string.Concat),
                new[] { typeof(string), typeof(string) });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
            => _stringConcatMethodInfo.Equals(methodCallExpression.Method)
            ? Expression.Add(methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], _stringConcatMethodInfo)
            : null;
    }
}
