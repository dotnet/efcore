// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteGeometryCollectionMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _item = typeof(IGeometryCollection).GetRuntimeProperty("Item").GetMethod;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            var method = methodCallExpression.Method.OnInterface(typeof(IGeometryCollection));
            if (Equals(method, _item))
            {
                return new SqlFunctionExpression(
                    "GeometryN",
                    methodCallExpression.Type,
                    new[]
                    {
                        methodCallExpression.Object,
                        Expression.Add(methodCallExpression.Arguments[0], Expression.Constant(1))
                    });
            }

            return null;
        }
    }
}
