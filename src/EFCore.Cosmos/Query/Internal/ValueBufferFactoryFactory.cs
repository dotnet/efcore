// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using Remotion.Linq.Parsing.ExpressionVisitors;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    public static class ValueBufferFactoryFactory
    {
        private static readonly ParameterExpression _jObjectParameter = Expression.Parameter(typeof(JObject), "jObject");

        private static readonly MethodInfo _getItemMethodInfo
            = typeof(JObject).GetTypeInfo().GetRuntimeProperties()
                .Single(pi => pi.Name == "Item" && pi.GetIndexParameters()[0].ParameterType == typeof(string))
                .GetMethod;

        private static readonly MethodInfo _toObjectMethodInfo
            = typeof(ValueBufferFactoryFactory).GetTypeInfo().GetRuntimeMethods()
                .Single(mi => mi.Name == nameof(SafeToObject));

        private static readonly MethodInfo _isNullMethodInfo
            = typeof(ValueBufferFactoryFactory).GetTypeInfo().GetRuntimeMethods()
                .Single(mi => mi.Name == nameof(IsNull));

        public static Expression<Func<JObject, object[]>> Create(List<IProperty> usedProperties)
            => Expression.Lambda<Func<JObject, object[]>>(
                Expression.NewArrayInit(
                    typeof(object),
                    usedProperties
                        .Select(
                            p =>
                                CreateGetValueExpression(
                                    _jObjectParameter,
                                    p))),
                _jObjectParameter);

        private static Expression CreateGetValueExpression(
            Expression jObjectExpression,
            IProperty property)
        {
            if (property.Name == StoreKeyConvention.JObjectPropertyName)
            {
                return jObjectExpression;
            }

            var storeName = property.Cosmos().PropertyName;
            if (storeName.Length == 0)
            {
                var type = property.FindMapping()?.Converter?.ProviderClrType
                           ?? property.ClrType;

                Expression calculatedExpression = Expression.Default(type);
                return type.IsValueType
                    ? Expression.Convert(calculatedExpression, typeof(object))
                    : calculatedExpression;
            }

            var valueExpression = CreateGetStoreValueExpression(jObjectExpression, property, storeName);
            if (valueExpression.Type.IsValueType)
            {
                valueExpression = Expression.Convert(valueExpression, typeof(object));
            }

            return valueExpression;
        }

        public static Expression CreateGetStoreValueExpression(Expression jObjectExpression, IProperty property, string storeName)
        {
            Expression valueExpression = Expression.Call(
                                jObjectExpression,
                                _getItemMethodInfo,
                                Expression.Constant(storeName));

            var modelClrType = property.ClrType;
            var converter = property.FindMapping().Converter;
            if (converter != null)
            {
                var nullableExpression = valueExpression;

                if (valueExpression.Type != converter.ProviderClrType)
                {
                    valueExpression =
                        Expression.Call(
                            _toObjectMethodInfo.MakeGenericMethod(converter.ProviderClrType),
                            valueExpression);

                    if (converter.ProviderClrType.IsNullableType())
                    {
                        nullableExpression = valueExpression;
                    }
                }

                valueExpression = ReplacingExpressionVisitor.Replace(
                    converter.ConvertFromProviderExpression.Parameters.Single(),
                    valueExpression,
                    converter.ConvertFromProviderExpression.Body);

                if (valueExpression.Type != modelClrType)
                {
                    valueExpression = Expression.Convert(valueExpression, modelClrType);
                }

                if (modelClrType.IsNullableType())
                {
                    valueExpression =
                        Expression.Condition(
                            nullableExpression.Type == typeof(JToken)
                                ? (Expression)Expression.Call(_isNullMethodInfo, nullableExpression)
                                : Expression.Equal(nullableExpression, Expression.Constant(null, nullableExpression.Type)),
                            Expression.Constant(null, modelClrType),
                            valueExpression);
                }
            }
            else if (valueExpression.Type != modelClrType)
            {
                valueExpression =
                    Expression.Call(
                        _toObjectMethodInfo.MakeGenericMethod(modelClrType),
                        valueExpression);
            }

            return valueExpression;
        }

        private static T SafeToObject<T>(JToken token)
            => token == null ? default : token.ToObject<T>();

        private static bool IsNull(JToken token)
            => token == null || token.Type == JTokenType.Null;
    }
}
