// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    public static class ValueBufferFactoryFactory
    {
        private static readonly ParameterExpression _jObjectParameter = Expression.Parameter(typeof(JObject), "jObject");
        private static readonly ParameterExpression _ordinalParameter = Expression.Parameter(typeof(int), "ordinal");

        private static readonly MethodInfo _getItemMethodInfo
            = typeof(JObject).GetTypeInfo().GetRuntimeProperties()
                .Single(pi => pi.Name == "Item" && pi.GetIndexParameters()[0].ParameterType == typeof(string))
                .GetMethod;

        public static Expression<Func<JObject, int, object[]>> Create(List<IProperty> usedProperties)
            => Expression.Lambda<Func<JObject, int, object[]>>(
                Expression.NewArrayInit(
                    typeof(object),
                    usedProperties
                        .Select(p =>
                                CreateGetValueExpression(
                                    _jObjectParameter,
                                    _ordinalParameter,
                                    p))),
                _jObjectParameter,
                _ordinalParameter);

        private static Expression CreateGetValueExpression(
            Expression jObjectExpression,
            Expression ordinalParameter,
            IProperty property)
        {
            if (property.Name == StoreKeyConvention.JObjectPropertyName)
            {
                return jObjectExpression;
            }

            var storeName = property.Cosmos().PropertyName;
            if (storeName.Length == 0)
            {
                var type = property.FindMapping()?.ClrType
                    ?? property.GetValueConverter()?.ProviderClrType
                    ?? property.ClrType;

                Expression calculatedExpression = Expression.Default(type);

                var entityType = property.DeclaringEntityType;
                var ownership = entityType.FindOwnership();

                if (ownership != null
                    && !entityType.IsDocumentRoot()
                    && property.IsPrimaryKey()
                    && !property.IsForeignKey())
                {
                    calculatedExpression = ordinalParameter;
                }

                return type.IsValueType
                    ? Expression.Convert(calculatedExpression, typeof(object))
                    : calculatedExpression;
            }

            var expression = Expression.Convert(
                Expression.Call(
                    jObjectExpression,
                    _getItemMethodInfo,
                    Expression.Constant(storeName)),
                property.ClrType);

            return property.ClrType.IsValueType
                ? Expression.Convert(expression, typeof(object))
                : expression;
        }
    }
}
