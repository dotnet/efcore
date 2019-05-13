// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class EnumerableToQueryableReMappingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(Enumerable))
            {
                var arguments = VisitAndConvert(methodCallExpression.Arguments, nameof(VisitMethodCall));
                var enumerableMethod = methodCallExpression.Method;
                var enumerableParameters = enumerableMethod.GetParameters();
                Type[] genericArguments = null;
                if (enumerableMethod.Name == nameof(Enumerable.Min)
                    || enumerableMethod.Name == nameof(Enumerable.Max))
                {
                    genericArguments = new Type[methodCallExpression.Arguments.Count];

                    if (!enumerableMethod.IsGenericMethod)
                    {
                        genericArguments[0] = enumerableMethod.ReturnType;
                    }
                    else
                    {
                        var argumentTypes = enumerableMethod.GetGenericArguments();
                        if (argumentTypes.Length == genericArguments.Length)
                        {
                            genericArguments = argumentTypes;
                        }
                        else
                        {
                            genericArguments[0] = argumentTypes[0];
                            genericArguments[1] = enumerableMethod.ReturnType;
                        }
                    }
                }
                else if (enumerableMethod.IsGenericMethod)
                {
                    genericArguments = enumerableMethod.GetGenericArguments();
                }

                foreach (var method in typeof(Queryable).GetTypeInfo().GetDeclaredMethods(methodCallExpression.Method.Name))
                {
                    var queryableMethod = method;
                    if (queryableMethod.IsGenericMethod)
                    {
                        if (queryableMethod.GetGenericArguments().Length == genericArguments.Length)
                        {
                            queryableMethod = queryableMethod.MakeGenericMethod(genericArguments);
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

                        if (CanConvertEnumerableToQueryable(enumerableParameterType, queryableParameterType, arguments[i].Type))
                        {
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

            return base.VisitMethodCall(methodCallExpression);
        }

        private static bool CanConvertEnumerableToQueryable(Type enumerableType, Type queryableType, Type argumentType)
        {
            if (!enumerableType.IsGenericType
                || !queryableType.IsGenericType
                || argumentType.TryGetElementType(typeof(IQueryable<>)) == null)
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
