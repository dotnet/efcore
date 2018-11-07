// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EnumHasFlagTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(Enum).GetRuntimeMethod(nameof(Enum.HasFlag), new[] { typeof(Enum) });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (Equals(methodCallExpression.Method, _methodInfo))
            {
                var argument = methodCallExpression.Arguments[0];
                argument = argument.RemoveConvert();

                // ReSharper disable once PossibleNullReferenceException
                var objectEnumType = methodCallExpression.Object.Type.UnwrapNullableType();
                var argumentEnumType = argument.Type.UnwrapNullableType();

                if (argument is ConstantExpression constantExpression)
                {
                    if (constantExpression.Value == null)
                    {
                        return null;
                    }

                    argumentEnumType = constantExpression.Value.GetType();
                    argument = Expression.Constant(constantExpression.Value, argumentEnumType);
                }

                if (objectEnumType != argumentEnumType)
                {
                    return null;
                }

                var objectType = objectEnumType.UnwrapEnumType();

                var convertedObjectExpression = Expression.Convert(methodCallExpression.Object, objectType);
                var convertedArgumentExpression = Expression.Convert(argument, objectType);

                return Expression.Equal(
                    Expression.And(
                        convertedObjectExpression,
                        convertedArgumentExpression),
                    convertedArgumentExpression);
            }

            return null;
        }
    }
}
