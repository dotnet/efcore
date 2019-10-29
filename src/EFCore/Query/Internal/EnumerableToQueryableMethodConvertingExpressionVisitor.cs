// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class EnumerableToQueryableMethodConvertingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(Enumerable))
            {
                if (methodCallExpression.Method.Name == nameof(Enumerable.SequenceEqual))
                {
                    // Skip SequenceEqual over enumerable since it could be over byte[] or other array properties
                    // Ideally we could make check in nav expansion about it (since it can bind to property)
                    // But since we don't translate SequenceEqual anyway, this is fine for now.
                    return base.VisitMethodCall(methodCallExpression);
                }

                if (methodCallExpression.Arguments.Count > 0
                    && ClientSource(methodCallExpression.Arguments[0]))
                {
                    // this is methodCall over closure variable or constant
                    return base.VisitMethodCall(methodCallExpression);
                }

                var arguments = VisitAndConvert(methodCallExpression.Arguments, nameof(VisitMethodCall)).ToArray();

                var enumerableMethod = methodCallExpression.Method;
                var enumerableParameters = enumerableMethod.GetParameters();
                Type[] genericTypeArguments = null;
                if (enumerableMethod.Name == nameof(Enumerable.Min)
                    || enumerableMethod.Name == nameof(Enumerable.Max))
                {
                    genericTypeArguments = new Type[methodCallExpression.Arguments.Count];

                    if (!enumerableMethod.IsGenericMethod)
                    {
                        genericTypeArguments[0] = enumerableMethod.ReturnType;
                    }
                    else
                    {
                        var argumentTypes = enumerableMethod.GetGenericArguments();
                        if (argumentTypes.Length == genericTypeArguments.Length)
                        {
                            genericTypeArguments = argumentTypes;
                        }
                        else
                        {
                            genericTypeArguments[0] = argumentTypes[0];
                            genericTypeArguments[1] = enumerableMethod.ReturnType;
                        }
                    }
                }
                else if (enumerableMethod.IsGenericMethod)
                {
                    genericTypeArguments = enumerableMethod.GetGenericArguments();
                }

                foreach (var method in typeof(Queryable).GetTypeInfo().GetDeclaredMethods(methodCallExpression.Method.Name))
                {
                    var queryableMethod = method;
                    if (queryableMethod.IsGenericMethod)
                    {
                        if (genericTypeArguments != null
                            && queryableMethod.GetGenericArguments().Length == genericTypeArguments.Length)
                        {
                            queryableMethod = queryableMethod.MakeGenericMethod(genericTypeArguments);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    var queryableParameters = queryableMethod.GetParameters();
                    if (enumerableParameters.Length != queryableParameters.Length)
                    {
                        continue;
                    }

                    var validMapping = true;
                    for (var i = 0; i < enumerableParameters.Length; i++)
                    {
                        var enumerableParameterType = enumerableParameters[i].ParameterType;
                        var queryableParameterType = queryableParameters[i].ParameterType;

                        if (enumerableParameterType == queryableParameterType)
                        {
                            continue;
                        }

                        if (CanConvertEnumerableToQueryable(enumerableParameterType, queryableParameterType))
                        {
                            var innerArgument = arguments[i];
                            var genericType = innerArgument.Type.TryGetSequenceType();

                            // If innerArgument has ToList applied to it then unwrap it.
                            // Also preserve generic argument of ToList is applied to different type
                            if (arguments[i].Type.TryGetElementType(typeof(List<>)) != null
                                && arguments[i] is MethodCallExpression toListMethodCallExpression
                                && toListMethodCallExpression.Method.IsGenericMethod
                                && toListMethodCallExpression.Method.GetGenericMethodDefinition() == EnumerableMethods.ToList)
                            {
                                genericType = toListMethodCallExpression.Method.GetGenericArguments()[0];
                                innerArgument = toListMethodCallExpression.Arguments[0];
                            }

                            var innerQueryableElementType = innerArgument.Type.TryGetElementType(typeof(IQueryable<>));
                            if (innerQueryableElementType == null
                                || innerQueryableElementType != genericType)
                            {
                                arguments[i] = Expression.Call(
                                    QueryableMethods.AsQueryable.MakeGenericMethod(genericType),
                                    innerArgument);
                            }

                            continue;
                        }

                        if (queryableParameterType.IsGenericType
                            && queryableParameterType.GetGenericTypeDefinition() == typeof(Expression<>)
                            && queryableParameterType.GetGenericArguments()[0] == enumerableParameterType)
                        {
                            continue;
                        }

                        validMapping = false;
                        break;
                    }

                    if (validMapping)
                    {
                        return Expression.Call(
                            queryableMethod,
                            arguments.Select(
                                arg => arg is LambdaExpression lambda ? Expression.Quote(lambda) : arg));
                    }
                }

                return methodCallExpression.Update(Visit(methodCallExpression.Object), arguments);
            }

            if (methodCallExpression.Method.DeclaringType.IsGenericType
                && methodCallExpression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(List<>)
                && string.Equals(nameof(List<int>.Contains), methodCallExpression.Method.Name))
            {
                if (ClientSource(methodCallExpression.Object))
                {
                    // this is methodCall over closure variable or constant
                    return base.VisitMethodCall(methodCallExpression);
                }

                var sourceType = methodCallExpression.Method.DeclaringType.GetGenericArguments()[0];

                return Expression.Call(
                    QueryableMethods.Contains.MakeGenericMethod(sourceType),
                    Expression.Call(
                        QueryableMethods.AsQueryable.MakeGenericMethod(sourceType),
                        methodCallExpression.Object),
                    methodCallExpression.Arguments[0]);
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private static bool ClientSource(Expression expression)
            => expression is ConstantExpression
                || expression is MemberInitExpression
                || expression is NewExpression
                || expression is ParameterExpression;

        private static bool CanConvertEnumerableToQueryable(Type enumerableType, Type queryableType)
        {
            if (enumerableType == typeof(IEnumerable)
                && queryableType == typeof(IQueryable))
            {
                return true;
            }

            if (!enumerableType.IsGenericType
                || !queryableType.IsGenericType
                || !enumerableType.GetGenericArguments().SequenceEqual(queryableType.GetGenericArguments()))
            {
                return false;
            }

            enumerableType = enumerableType.GetGenericTypeDefinition();
            queryableType = queryableType.GetGenericTypeDefinition();

            if (enumerableType == typeof(IEnumerable<>)
                && queryableType == typeof(IQueryable<>))
            {
                return true;
            }

            if (enumerableType == typeof(IOrderedEnumerable<>)
                && queryableType == typeof(IOrderedQueryable<>))
            {
                return true;
            }

            return false;
        }
    }
}
