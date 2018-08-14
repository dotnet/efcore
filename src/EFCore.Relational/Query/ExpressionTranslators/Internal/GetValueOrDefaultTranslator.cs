// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class GetValueOrDefaultTranslator : IMethodCallTranslator
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.Name == nameof(Nullable<int>.GetValueOrDefault)
                && methodCallExpression.Type.IsNumeric())
            {
                if (methodCallExpression.Arguments.Count == 0)
                {
                    return Expression.Coalesce(
                        methodCallExpression.Object,
                        methodCallExpression.Type.GenerateDefaultValueConstantExpression());
                }
                else if (methodCallExpression.Arguments.Count == 1)
                {
                    return Expression.Coalesce(
                        methodCallExpression.Object,
                        methodCallExpression.Arguments[0]);
                }
            }

            return null;
        }
    }
}
