// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
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
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerGeometryMethodTranslator(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _typeMappingSource = typeMappingSource;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (typeof(IGeometry).IsAssignableFrom(method.DeclaringType))
            {
                var geometryExpressions = new[] { instance }.Concat(
                    arguments.Where(e => typeof(IGeometry).IsAssignableFrom(e.Type)));
                var typeMapping = ExpressionExtensions.InferTypeMapping(geometryExpressions.ToArray());

                Debug.Assert(typeMapping != null, "At least one argument must have typeMapping.");
                var storeType = typeMapping.StoreType;
                var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

                method = method.OnInterface(typeof(IGeometry));
                if (_methodToFunctionName.TryGetValue(method, out var functionName)
                    || (!isGeography && _geometryMethodToFunctionName.TryGetValue(method, out functionName)))
                {
                    instance = _sqlExpressionFactory.ApplyTypeMapping(
                        instance, _typeMappingSource.FindMapping(instance.Type, storeType));

                    var typeMappedArguments = new List<SqlExpression>();
                    foreach (var argument in arguments)
                    {
                        typeMappedArguments.Add(
                            _sqlExpressionFactory.ApplyTypeMapping(
                                argument,
                                typeof(IGeometry).IsAssignableFrom(argument.Type)
                                    ? _typeMappingSource.FindMapping(argument.Type, storeType)
                                    : _typeMappingSource.FindMapping(argument.Type)));
                    }

                    var resultTypeMapping = typeof(IGeometry).IsAssignableFrom(method.ReturnType)
                        ? _typeMappingSource.FindMapping(method.ReturnType, storeType)
                        : _typeMappingSource.FindMapping(method.ReturnType);

                    return _sqlExpressionFactory.Function(
                        instance,
                        functionName,
                        Simplify(typeMappedArguments, isGeography),
                        method.ReturnType,
                        resultTypeMapping);
                }

                if (Equals(method, _getGeometryN))
                {
                    return _sqlExpressionFactory.Function(
                        instance,
                        "STGeometryN",
                        new[] {
                            _sqlExpressionFactory.Add(
                                arguments[0],
                                _sqlExpressionFactory.Constant(1))
                        },
                        method.ReturnType,
                        _typeMappingSource.FindMapping(method.ReturnType, storeType));
                }

                if (Equals(method, _isWithinDistance))
                {
                    instance = _sqlExpressionFactory.ApplyTypeMapping(
                        instance, _typeMappingSource.FindMapping(instance.Type, storeType));

                    var typeMappedArguments = new List<SqlExpression>();
                    foreach (var argument in arguments)
                    {
                        typeMappedArguments.Add(
                            _sqlExpressionFactory.ApplyTypeMapping(
                                argument,
                                typeof(IGeometry).IsAssignableFrom(argument.Type)
                                    ? _typeMappingSource.FindMapping(argument.Type, storeType)
                                    : _typeMappingSource.FindMapping(argument.Type)));
                    }

                    return _sqlExpressionFactory.LessThanOrEqual(
                        _sqlExpressionFactory.Function(
                            instance,
                            "STDistance",
                            Simplify(new[] { typeMappedArguments[0] }, isGeography),
                            typeof(double)),
                        typeMappedArguments[1]);
                }
            }

            return null;
        }

        private IEnumerable<SqlExpression> Simplify(IEnumerable<SqlExpression> arguments, bool isGeography)
        {
            foreach (var argument in arguments)
            {
                if (argument is SqlConstantExpression constant
                    && constant.Value is IGeometry geometry
                    && geometry.SRID == (isGeography ? 4326 : 0))
                {
                    yield return _sqlExpressionFactory.Fragment("'" + geometry.AsText() + "'");
                    continue;
                }

                yield return argument;
            }
        }
    }
}
