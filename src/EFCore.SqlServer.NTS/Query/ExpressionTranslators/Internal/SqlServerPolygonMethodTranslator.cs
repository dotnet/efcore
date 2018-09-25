// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerPolygonMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _getInteriorRingN = typeof(IPolygon).GetRuntimeMethod(nameof(IPolygon.GetInteriorRingN), new[] { typeof(int) });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            var instance = methodCallExpression.Object;
            var isGeography = string.Equals(
                instance.FindProperty(instance.Type)?.Relational().ColumnType,
                "geography",
                StringComparison.OrdinalIgnoreCase);

            var method = methodCallExpression.Method.OnInterface(typeof(IPolygon));
            if (isGeography)
            {
                if (Equals(method, _getInteriorRingN))
                {
                    return new SqlFunctionExpression(
                        instance,
                        "RingN",
                        methodCallExpression.Type,
                        new[] { Expression.Add(methodCallExpression.Arguments[0], Expression.Constant(2)) });
                }
            }
            else if (Equals(method, _getInteriorRingN))
            {
                return new SqlFunctionExpression(
                    instance,
                    "STInteriorRingN",
                    methodCallExpression.Type,
                    new[] { Expression.Add(methodCallExpression.Arguments[0], Expression.Constant(1)) });
            }

            return null;
        }
    }
}
