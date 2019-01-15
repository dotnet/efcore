// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
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

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ITypeMappingApplyingExpressionVisitor _typeMappingApplyingExpressionVisitor;

        public SqliteGeometryMethodTranslator(
            IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor)
        {
            _typeMappingSource = typeMappingSource;
            _typeMappingApplyingExpressionVisitor = typeMappingApplyingExpressionVisitor;
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            method = method.OnInterface(typeof(IGeometry));
            if (_methodToFunctionName.TryGetValue(method, out var functionName))
            {
                instance = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(
                    instance, _typeMappingSource.FindMapping(instance.Type));

                SqlExpression translation = new SqlFunctionExpression(
                    functionName,
                    new[] { instance }.Concat(
                        arguments.Select(e => _typeMappingApplyingExpressionVisitor
                            .ApplyTypeMapping(e, _typeMappingSource.FindMapping(e.Type)))),
                    method.ReturnType,
                    _typeMappingSource.FindMapping(method.ReturnType),
                    false);

                if (method.ReturnType == typeof(bool))
                {
                    translation = new CaseExpression(
                        new[]
                        {
                            new CaseWhenClause(
                                new SqlNullExpression(instance, true, _typeMappingSource.FindMapping(typeof(bool))),
                                translation)
                        },
                        null);
                }

                return translation;
            }

            if (Equals(method, _getGeometryN))
            {
                instance = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(
                    instance, _typeMappingSource.FindMapping(instance.Type));

                return new SqlFunctionExpression(
                    "GeometryN",
                    new[] {
                        instance,
                        _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(
                            new SqlBinaryExpression(
                                ExpressionType.Add,
                                arguments[0],
                                new SqlConstantExpression(Expression.Constant(1), null),
                                typeof(int),
                                null),
                            _typeMappingSource.FindMapping(typeof(int)))
                    },
                    method.ReturnType,
                    _typeMappingSource.FindMapping(method.ReturnType),
                    false);
            }

            if (Equals(method, _isWithinDistance))
            {
                instance = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(
                    instance, _typeMappingSource.FindMapping(instance.Type));

                var updatedArguments = arguments.Select(e => _typeMappingApplyingExpressionVisitor
                            .ApplyTypeMapping(e, _typeMappingSource.FindMapping(e.Type)))
                            .ToList();

                return new SqlBinaryExpression(
                    ExpressionType.LessThanOrEqual,
                    new SqlFunctionExpression(
                        "Distance",
                        new[] { instance, updatedArguments[0] },
                        typeof(double),
                        _typeMappingSource.FindMapping(typeof(double)),
                        false),
                    updatedArguments[1],
                    typeof(bool),
                    _typeMappingSource.FindMapping(typeof(bool)));
            }

            return null;
        }
    }
}
