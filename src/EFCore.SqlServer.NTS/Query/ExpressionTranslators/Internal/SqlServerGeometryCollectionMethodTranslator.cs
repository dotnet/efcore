// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    public class SqlServerGeometryCollectionMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _item = typeof(IGeometryCollection).GetRuntimeProperty("Item").GetMethod;

        private readonly IRelationalTypeMappingSource _typeMappingSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerGeometryCollectionMethodTranslator(IRelationalTypeMappingSource typeMappingSource)
            => _typeMappingSource = typeMappingSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (!typeof(IGeometryCollection).IsAssignableFrom(methodCallExpression.Method.DeclaringType))
            {
                return null;
            }

            var storeType = methodCallExpression.FindSpatialStoreType();

            var method = methodCallExpression.Method.OnInterface(typeof(IGeometryCollection));
            if (Equals(method, _item))
            {
                return new SqlFunctionExpression(
                    methodCallExpression.Object,
                    "STGeometryN",
                    methodCallExpression.Type,
                    new[] { Expression.Add(methodCallExpression.Arguments[0], Expression.Constant(1)) },
                    _typeMappingSource.FindMapping(typeof(IGeometry), storeType));
            }

            return null;
        }
    }
}
