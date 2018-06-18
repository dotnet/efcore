// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EqualsTranslator : IMethodCallTranslator
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EqualsTranslator([NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(logger, nameof(logger));

            _logger = logger;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (methodCallExpression.Method.Name == nameof(object.Equals)
                && methodCallExpression.Arguments.Count == 1
                && methodCallExpression.Object != null)
            {
                var argument = methodCallExpression.Arguments[0];

                return methodCallExpression.Method.GetParameters()[0].ParameterType == typeof(object)
                    && methodCallExpression.Object.Type != argument.Type
                    ? TranslateEquals(methodCallExpression.Object, argument.RemoveConvert(), methodCallExpression)
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
                    ? TranslateEquals(left, right, methodCallExpression)
                    : Expression.Equal(left, right);
            }

            return null;
        }

        private Expression TranslateEquals(Expression left, Expression right, MethodCallExpression methodCallExpression)
        {
            var unwrappedLeftType = left.Type.UnwrapNullableType();
            var unwrappedRightType = right.Type.UnwrapNullableType();

            if (unwrappedLeftType == unwrappedRightType)
            {
                return Expression.Equal(
                    Expression.Convert(left, unwrappedLeftType),
                    Expression.Convert(right, unwrappedRightType));
            }

            _logger.QueryPossibleUnintendedUseOfEqualsWarning(methodCallExpression);

            // Equals(object) always returns false if when comparing objects of different types
            return Expression.Constant(false);
        }
    }
}
