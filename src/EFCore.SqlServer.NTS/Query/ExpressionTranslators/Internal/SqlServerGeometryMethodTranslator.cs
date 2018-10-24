// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Storage;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerGeometryMethodTranslator : IMethodCallTranslator
    {
        private static readonly IDictionary<MethodInfo, string> _methodToFunctionName = new Dictionary<MethodInfo, string>
        {
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.AsBinary), Type.EmptyTypes), "STAsBinary" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.AsText), Type.EmptyTypes), "AsTextZM" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Buffer), new[] { typeof(double) }), "STBuffer" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Contains), new[] { typeof(IGeometry) }), "STContains" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.ConvexHull), Type.EmptyTypes), "STConvexHull" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Difference), new[] { typeof(IGeometry) }), "STDifference" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Disjoint), new[] { typeof(IGeometry) }), "STDisjoint" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Distance), new[] { typeof(IGeometry) }), "STDistance" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.EqualsTopologically), new[] { typeof(IGeometry) }), "STEquals" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Intersection), new[] { typeof(IGeometry) }), "STIntersection" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Intersects), new[] { typeof(IGeometry) }), "STIntersects" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Overlaps), new[] { typeof(IGeometry) }), "STOverlaps" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.SymmetricDifference), new[] { typeof(IGeometry) }), "STSymDifference" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.ToBinary), Type.EmptyTypes), "STAsBinary" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.ToText), Type.EmptyTypes), "AsTextZM" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Union), new[] { typeof(IGeometry) }), "STUnion" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Within), new[] { typeof(IGeometry) }), "STWithin" }
        };

        private static readonly IDictionary<MethodInfo, string> _geometryMethodToFunctionName = new Dictionary<MethodInfo, string>
        {
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Crosses), new[] { typeof(IGeometry) }), "STCrosses" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Relate), new[] { typeof(IGeometry), typeof(string) }), "STRelate" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Touches), new[] { typeof(IGeometry) }), "STTouches" }
        };

        private static readonly MethodInfo _getGeometryN = typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.GetGeometryN), new[] { typeof(int) });
        private static readonly MethodInfo _isWithinDistance = typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.IsWithinDistance), new[] { typeof(IGeometry), typeof(double) });

        private readonly IRelationalTypeMappingSource _typeMappingSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerGeometryMethodTranslator(IRelationalTypeMappingSource typeMappingSource)
            => _typeMappingSource = typeMappingSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (!typeof(IGeometry).IsAssignableFrom(methodCallExpression.Method.DeclaringType))
            {
                return null;
            }

            var storeType = methodCallExpression.FindSpatialStoreType();
            var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

            var method = methodCallExpression.Method.OnInterface(typeof(IGeometry));
            if (_methodToFunctionName.TryGetValue(method, out var functionName)
                || (!isGeography && _geometryMethodToFunctionName.TryGetValue(method, out functionName)))
            {
                var anyGeometryArguments = false;
                var argumentTypeMappings = new RelationalTypeMapping[methodCallExpression.Arguments.Count];
                for (var i = 0; i < methodCallExpression.Arguments.Count; i++)
                {
                    var type = methodCallExpression.Arguments[i].Type;
                    if (typeof(IGeometry).IsAssignableFrom(type))
                    {
                        anyGeometryArguments = true;
                        argumentTypeMappings[i] = _typeMappingSource.FindMapping(type, storeType);
                    }
                    else
                    {
                        argumentTypeMappings[i] = _typeMappingSource.FindMapping(type);
                    }
                }
                if (!anyGeometryArguments)
                {
                    argumentTypeMappings = null;
                }

                RelationalTypeMapping resultTypeMapping = null;
                if (typeof(IGeometry).IsAssignableFrom(methodCallExpression.Type))
                {
                    resultTypeMapping = _typeMappingSource.FindMapping(methodCallExpression.Type, storeType);
                }

                return new SqlFunctionExpression(
                    methodCallExpression.Object,
                    functionName,
                    methodCallExpression.Type,
                    Simplify(methodCallExpression.Arguments, isGeography),
                    resultTypeMapping,
                    _typeMappingSource.FindMapping(methodCallExpression.Object.Type, storeType),
                    argumentTypeMappings);
            }
            if (Equals(method, _getGeometryN))
            {
                return new SqlFunctionExpression(
                    methodCallExpression.Object,
                    "STGeometryN",
                    methodCallExpression.Type,
                    new[] { Expression.Add(methodCallExpression.Arguments[0], Expression.Constant(1)) },
                    _typeMappingSource.FindMapping(typeof(IGeometry), storeType));
            }
            if (Equals(method, _isWithinDistance))
            {
                return Expression.LessThanOrEqual(
                    Expression.Convert(
                        new SqlFunctionExpression(
                            methodCallExpression.Object,
                            "STDistance",
                            typeof(double),
                            Simplify(new[] { methodCallExpression.Arguments[0] }, isGeography),
                            resultTypeMapping: null,
                            _typeMappingSource.FindMapping(methodCallExpression.Object.Type, storeType),
                            new[] { _typeMappingSource.FindMapping(typeof(IGeometry), storeType) }),
                        typeof(double?)),
                    Expression.Convert(methodCallExpression.Arguments[1], typeof(double?)));
            }

            return null;
        }

        private static IEnumerable<Expression> Simplify(IEnumerable<Expression> arguments, bool isGeography)
        {
            foreach (var argument in arguments)
            {
                if (argument is ConstantExpression constant
                    && constant.Value is IGeometry geometry
                    && geometry.SRID == (isGeography ? 4326 : 0))
                {
                    yield return new SqlFragmentExpression("'" + geometry.AsText() + "'");
                    continue;
                }

                yield return argument;
            }
        }
    }
}
