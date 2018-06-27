// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Internal
{
    public static class ValueBufferFactoryFactory
    {
        private static readonly ParameterExpression _jObjectParameter = Expression.Parameter(typeof(JObject), "jObject");

        private static readonly MethodInfo _getItemMethodInfo
            = typeof(JObject).GetTypeInfo().GetRuntimeProperties()
                .Single(pi => pi.Name == "Item" && pi.GetIndexParameters()[0].ParameterType == typeof(string))
                .GetMethod;

        public static Expression<Func<JObject, object[]>> Create(IEntityType entityType)
        {
            return Expression.Lambda<Func<JObject, object[]>>(
                Expression.NewArrayInit(
                    typeof(object),
                    entityType.GetProperties()
                        .Select(
                            p =>
                                CreateGetValueExpression(
                                    _jObjectParameter,
                                    p))),
                                _jObjectParameter);
        }

        private static Expression CreateGetValueExpression(
            Expression jObjectExpression,
            IProperty property)
        {
            // TODO : Converters
            // TODO : TryCatch for invalid values
            return Expression.Convert(
                Expression.Call(
                    jObjectExpression,
                    _getItemMethodInfo,
                    Expression.Constant(property.Name)),
                property.ClrType);
        }
    }
}
