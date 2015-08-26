// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query.ExpressionTranslators
{
    public class EqualsTranslator : IMethodCallTranslator
    {
        private readonly ILogger _logger;

        public EqualsTranslator([NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger(nameof(EqualsTranslator));
            ;
        }

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (methodCallExpression.Method.Name == nameof(object.Equals)
                && methodCallExpression.Arguments.Count == 1)
            {
                var argument = methodCallExpression.Arguments[0];
                var @object = methodCallExpression.Object;
                if (methodCallExpression.Method.GetParameters()[0].ParameterType == typeof(object)
                    && @object.Type != argument.Type)
                {
                    argument = argument.RemoveConvert();
                    var unwrappedObjectType = @object.Type.UnwrapNullableType();
                    var unwrappedArgumentType = argument.Type.UnwrapNullableType();
                    if (unwrappedObjectType == unwrappedArgumentType)
                    {
                        return Expression.Equal(
                            Expression.Convert(@object, unwrappedObjectType),
                            Expression.Convert(argument, unwrappedArgumentType));
                    }

                    _logger.LogInformation(
                        Strings.PossibleUnintendedUseOfEquals(
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
