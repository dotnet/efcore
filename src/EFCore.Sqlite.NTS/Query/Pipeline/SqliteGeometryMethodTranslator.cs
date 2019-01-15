// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public class SqliteGeometryMethodTranslator : IMethodCallTranslator
    {
        private static readonly IDictionary<MethodInfo, string> _methodToFunctionName = new Dictionary<MethodInfo, string>
        {
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.AsBinary), Type.EmptyTypes), "AsBinary" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.AsText), Type.EmptyTypes), "AsText" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Buffer), new[] { typeof(double) }), "Buffer" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Buffer), new[] { typeof(double), typeof(int) }), "Buffer" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Contains), new[] { typeof(IGeometry) }), "Contains" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.ConvexHull), Type.EmptyTypes), "ConvexHull" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Crosses), new[] { typeof(IGeometry) }), "Crosses" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.CoveredBy), new[] { typeof(IGeometry) }), "CoveredBy" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Covers), new[] { typeof(IGeometry) }), "Covers" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Difference), new[] { typeof(IGeometry) }), "Difference" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Disjoint), new[] { typeof(IGeometry) }), "Disjoint" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Distance), new[] { typeof(IGeometry) }), "Distance" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.EqualsTopologically), new[] { typeof(IGeometry) }), "Equals" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Intersection), new[] { typeof(IGeometry) }), "Intersection" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Intersects), new[] { typeof(IGeometry) }), "Intersects" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Overlaps), new[] { typeof(IGeometry) }), "Overlaps" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Relate), new[] { typeof(IGeometry), typeof(string) }), "Relate" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Reverse), Type.EmptyTypes), "ST_Reverse" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.SymmetricDifference), new[] { typeof(IGeometry) }), "SymDifference" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.ToBinary), Type.EmptyTypes), "AsBinary" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.ToText), Type.EmptyTypes), "AsText" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Touches), new[] { typeof(IGeometry) }), "Touches" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Union), Type.EmptyTypes), "UnaryUnion" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Union), new[] { typeof(IGeometry) }), "GUnion" },
            { typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.Within), new[] { typeof(IGeometry) }), "Within" }
        };

        private static readonly MethodInfo _getGeometryN = typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.GetGeometryN), new[] { typeof(int) });
        private static readonly MethodInfo _isWithinDistance = typeof(IGeometry).GetRuntimeMethod(nameof(IGeometry.IsWithinDistance), new[] { typeof(IGeometry), typeof(double) });

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqliteGeometryMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            method = method.OnInterface(typeof(IGeometry));
            if (_methodToFunctionName.TryGetValue(method, out var functionName))
            {
                SqlExpression translation = _sqlExpressionFactory.Function(
                    functionName,
                    new[] { instance }.Concat(arguments),
                    method.ReturnType);

                if (method.ReturnType == typeof(bool))
                {
                    translation = _sqlExpressionFactory.Case(
                        new[]
                        {
                            new CaseWhenClause(_sqlExpressionFactory.IsNotNull(instance), translation)
                        },
                        null);
                }

                return translation;
            }

            if (Equals(method, _getGeometryN))
            {
                return _sqlExpressionFactory.Function(
                    "GeometryN",
                    new[] {
                        instance,
                        _sqlExpressionFactory.Add(
                            arguments[0],
                            _sqlExpressionFactory.Constant(1))
                    },
                    method.ReturnType);
            }

            if (Equals(method, _isWithinDistance))
            {
                return _sqlExpressionFactory.LessThanOrEqual(
                    _sqlExpressionFactory.Function(
                        "Distance",
                        new[] { instance, arguments[0] },
                        typeof(double)),
                    arguments[1]);
            }

            return null;
        }
    }
}
