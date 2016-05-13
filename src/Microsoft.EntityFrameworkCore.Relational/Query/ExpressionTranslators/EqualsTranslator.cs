// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators
{
    public class EqualsTranslator : IMethodCallTranslator
    {
        private readonly ILogger _logger;

        public EqualsTranslator([NotNull] ILogger logger)
        {
            Check.NotNull(logger, nameof(logger));

            _logger = logger;
        }

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if ((methodCallExpression.Method.Name == nameof(object.Equals))
                && (methodCallExpression.Arguments.Count == 1)
                && (methodCallExpression.Object != null))
            {
                var argument = methodCallExpression.Arguments[0];

                if ((methodCallExpression.Method.GetParameters()[0].ParameterType == typeof(object))
                    && (methodCallExpression.Object.Type != argument.Type))
                {
                    argument = argument.RemoveConvert();

                    var unwrappedObjectType = methodCallExpression.Object.Type.UnwrapNullableType();
                    var unwrappedArgumentType = argument.Type.UnwrapNullableType();

                    if (unwrappedObjectType == unwrappedArgumentType)
                    {
                        return Expression.Equal(
                            Expression.Convert(methodCallExpression.Object, unwrappedObjectType),
                            Expression.Convert(argument, unwrappedArgumentType));
                    }

                    _logger.LogWarning(
                        RelationalLoggingEventId.PossibleUnintendedUseOfEquals,
                        () => RelationalStrings.PossibleUnintendedUseOfEquals(
                            methodCallExpression.Object.ToString(),
                            argument.ToString()));

                    // Equals(object) always returns false if when comparing objects of different types
                    return Expression.Constant(false);
                }

                return Expression.Equal(methodCallExpression.Object, argument);
            }

            return null;
        }
    }
}
