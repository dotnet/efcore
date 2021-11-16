// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.AsBinary), Type.EmptyTypes), "AsBinary" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.AsText), Type.EmptyTypes), "AsText" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.Buffer), typeof(double)), "Buffer" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.Buffer), typeof(double), typeof(int)), "Buffer" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.Contains), typeof(Geometry)), "Contains" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.ConvexHull), Type.EmptyTypes), "ConvexHull" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.Crosses), typeof(Geometry)), "Crosses" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.CoveredBy), typeof(Geometry)), "CoveredBy" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.Covers), typeof(Geometry)), "Covers" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.Difference), typeof(Geometry)), "Difference" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.Disjoint), typeof(Geometry)), "Disjoint" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.Distance), typeof(Geometry)), "Distance" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.EqualsTopologically), typeof(Geometry)), "Equals" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.Intersection), typeof(Geometry)), "Intersection" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.Intersects), typeof(Geometry)), "Intersects" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.Overlaps), typeof(Geometry)), "Overlaps" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.Relate), typeof(Geometry), typeof(string)), "Relate" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.Reverse), Type.EmptyTypes), "ST_Reverse" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.SymmetricDifference), typeof(Geometry)), "SymDifference" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.ToBinary), Type.EmptyTypes), "AsBinary" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.ToText), Type.EmptyTypes), "AsText" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.Touches), typeof(Geometry)), "Touches" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.Union), Type.EmptyTypes), "UnaryUnion" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.Union), typeof(Geometry)), "GUnion" },
            { typeof(Geometry).GetRequiredRuntimeMethod(nameof(Geometry.Within), typeof(Geometry)), "Within" }
        };

        private static readonly MethodInfo _getGeometryN = typeof(Geometry).GetRequiredRuntimeMethod(
            nameof(Geometry.GetGeometryN), typeof(int));

        private static readonly MethodInfo _isWithinDistance = typeof(Geometry).GetRequiredRuntimeMethod(
            nameof(Geometry.IsWithinDistance), typeof(Geometry), typeof(double));

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteGeometryMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression? Translate(
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (instance != null)
            {
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
            }

            return null;
        }
    }
}
