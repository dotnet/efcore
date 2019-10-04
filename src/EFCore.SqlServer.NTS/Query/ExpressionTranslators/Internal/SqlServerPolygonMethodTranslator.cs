// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerPolygonMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _getInteriorRingN = typeof(IPolygon).GetRuntimeMethod(nameof(IPolygon.GetInteriorRingN), new[] { typeof(int) });

        private readonly IRelationalTypeMappingSource _typeMappingSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerPolygonMethodTranslator(IRelationalTypeMappingSource typeMappingSource)
            => _typeMappingSource = typeMappingSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (!typeof(IPolygon).IsAssignableFrom(methodCallExpression.Method.DeclaringType))
            {
                return null;
            }

            var storeType = methodCallExpression.FindSpatialStoreType();
            var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

            var method = methodCallExpression.Method.OnInterface(typeof(IPolygon));
            if (isGeography)
            {
                if (Equals(method, _getInteriorRingN))
                {
                    return new SqlFunctionExpression(
                        methodCallExpression.Object,
                        "RingN",
                        methodCallExpression.Type,
                        new[] { Expression.Add(methodCallExpression.Arguments[0], Expression.Constant(2)) },
                        _typeMappingSource.FindMapping(typeof(ILineString), storeType));
                }
            }
            else if (Equals(method, _getInteriorRingN))
            {
                return new SqlFunctionExpression(
                    methodCallExpression.Object,
                    "STInteriorRingN",
                    methodCallExpression.Type,
                    new[] { Expression.Add(methodCallExpression.Arguments[0], Expression.Constant(1)) },
                    _typeMappingSource.FindMapping(typeof(ILineString), storeType));
            }

            return null;
        }
    }
}
