// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    public class SqliteGeometryMethodTranslator : IMethodCallTranslator
    {
        private static readonly IDictionary<MethodInfo, string> _methodToFunctionName = new Dictionary<MethodInfo, string>
        {
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.AsBinary), Type.EmptyTypes), "AsBinary" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.AsText), Type.EmptyTypes), "AsText" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Buffer), new[] { typeof(double) }), "Buffer" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Buffer), new[] { typeof(double), typeof(int) }), "Buffer" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Contains), new[] { typeof(Geometry) }), "Contains" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.ConvexHull), Type.EmptyTypes), "ConvexHull" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Crosses), new[] { typeof(Geometry) }), "Crosses" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.CoveredBy), new[] { typeof(Geometry) }), "CoveredBy" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Covers), new[] { typeof(Geometry) }), "Covers" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Difference), new[] { typeof(Geometry) }), "Difference" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Disjoint), new[] { typeof(Geometry) }), "Disjoint" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Distance), new[] { typeof(Geometry) }), "Distance" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.EqualsTopologically), new[] { typeof(Geometry) }), "Equals" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Intersection), new[] { typeof(Geometry) }), "Intersection" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Intersects), new[] { typeof(Geometry) }), "Intersects" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Overlaps), new[] { typeof(Geometry) }), "Overlaps" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Relate), new[] { typeof(Geometry), typeof(string) }), "Relate" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Reverse), Type.EmptyTypes), "ST_Reverse" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.SymmetricDifference), new[] { typeof(Geometry) }), "SymDifference" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.ToBinary), Type.EmptyTypes), "AsBinary" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.ToText), Type.EmptyTypes), "AsText" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Touches), new[] { typeof(Geometry) }), "Touches" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Union), Type.EmptyTypes), "UnaryUnion" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Union), new[] { typeof(Geometry) }), "GUnion" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Within), new[] { typeof(Geometry) }), "Within" }
        };

        private static readonly MethodInfo _getGeometryN = typeof(Geometry).GetRuntimeMethod(
            nameof(Geometry.GetGeometryN), new[] { typeof(int) });

        private static readonly MethodInfo _isWithinDistance = typeof(Geometry).GetRuntimeMethod(
            nameof(Geometry.IsWithinDistance), new[] { typeof(Geometry), typeof(double) });

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqliteGeometryMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (_methodToFunctionName.TryGetValue(method, out var functionName))
            {
                SqlExpression translation = _sqlExpressionFactory.Function(
                    functionName,
                    new[] { instance }.Concat(arguments),
                    method.ReturnType);

                if (method.ReturnType == typeof(bool))
                {
                    translation = _sqlExpressionFactory.Case(
                        new[] { new CaseWhenClause(_sqlExpressionFactory.IsNotNull(instance), translation) },
                        null);
                }

                return translation;
            }

            if (Equals(method, _getGeometryN))
            {
                return _sqlExpressionFactory.Function(
                    "GeometryN",
                    new[]
                    {
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
