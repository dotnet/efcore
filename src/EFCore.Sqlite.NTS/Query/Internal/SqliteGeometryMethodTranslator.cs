// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteGeometryMethodTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(logger, nameof(logger));

            if (_methodToFunctionName.TryGetValue(method, out var functionName))
            {
                var finalArguments = new[] { instance }.Concat(arguments);

                if (method.ReturnType == typeof(bool))
                {
                    var nullCheck = (SqlExpression)_sqlExpressionFactory.IsNotNull(instance);
                    foreach (var argument in arguments)
                    {
                        nullCheck = _sqlExpressionFactory.AndAlso(
                            nullCheck,
                            _sqlExpressionFactory.IsNotNull(argument));
                    }

                    return _sqlExpressionFactory.Case(
                        new[]
                        {
                            new CaseWhenClause(
                                nullCheck,
                                _sqlExpressionFactory.Function(
                                    functionName,
                                    finalArguments,
                                    nullable: false,
                                    finalArguments.Select(a => false),
                                    method.ReturnType))
                        },
                        null);
                }

                return _sqlExpressionFactory.Function(
                    functionName,
                    finalArguments,
                    nullable: true,
                    finalArguments.Select(a => true),
                    method.ReturnType);
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
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true },
                    method.ReturnType);
            }

            if (Equals(method, _isWithinDistance))
            {
                return _sqlExpressionFactory.LessThanOrEqual(
                    _sqlExpressionFactory.Function(
                        "Distance",
                        new[] { instance, arguments[0] },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, true },
                        typeof(double)),
                    arguments[1]);
            }

            return null;
        }
    }
}
