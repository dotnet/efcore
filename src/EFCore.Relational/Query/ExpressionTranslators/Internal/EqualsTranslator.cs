// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class EqualsTranslator : IMethodCallTranslator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression Translate(
            MethodCallExpression methodCallExpression,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));
            Check.NotNull(logger, nameof(logger));

            if (methodCallExpression.Method.Name == nameof(object.Equals)
                && methodCallExpression.Arguments.Count == 1
                && methodCallExpression.Object != null)
            {
                var argument = methodCallExpression.Arguments[0];

                return methodCallExpression.Method.GetParameters()[0].ParameterType == typeof(object)
                       && methodCallExpression.Object.Type != argument.Type
                    ? TranslateEquals(methodCallExpression.Object, argument.RemoveConvert(), methodCallExpression, logger)
                    : Expression.Equal(methodCallExpression.Object, argument);
            }

            if (methodCallExpression.Method.Name == nameof(object.Equals)
                && methodCallExpression.Arguments.Count == 2
                && methodCallExpression.Arguments[0].Type == methodCallExpression.Arguments[1].Type)
            {
                var left = methodCallExpression.Arguments[0].RemoveConvert();
                var right = methodCallExpression.Arguments[1].RemoveConvert();
                return methodCallExpression.Method.GetParameters()[0].ParameterType == typeof(object)
                       && left.Type != right.Type
                    ? TranslateEquals(left, right, methodCallExpression, logger)
                    : Expression.Equal(left, right);
            }

            return null;
        }

        private Expression TranslateEquals(
            Expression left,
            Expression right,
            MethodCallExpression methodCallExpression,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            var unwrappedLeftType = left.Type.UnwrapNullableType();
            var unwrappedRightType = right.Type.UnwrapNullableType();

            if (unwrappedLeftType == unwrappedRightType)
            {
                return Expression.Equal(
                    Expression.Convert(left, unwrappedLeftType),
                    Expression.Convert(right, unwrappedRightType));
            }

            logger.QueryPossibleUnintendedUseOfEqualsWarning(methodCallExpression);

            // Equals(object) always returns false if when comparing objects of different types
            return Expression.Constant(false);
        }
    }
}
