// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.Framework.Logging;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Query.Methods
{
    public class EqualsTranslator : IMethodCallTranslator
    {
        private ILogger _logger;

        public EqualsTranslator([NotNull]ILogger logger)
        {
            _logger = logger;
        }

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.Name == "Equals"
                && methodCallExpression.Arguments.Count == 1)
            {
                var argument = methodCallExpression.Arguments[0];
                var @object = methodCallExpression.Object;
                if (methodCallExpression.Method.GetParameters()[0].ParameterType == typeof(object)
                    && @object.Type != argument.Type)
                {
                    var unaryArgument = argument as UnaryExpression;
                    if (unaryArgument != null && argument.NodeType == ExpressionType.Convert)
                    {
                        argument = unaryArgument.Operand;
                    }

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
