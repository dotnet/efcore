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
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerGeometryMethodTranslator : IMethodCallTranslator
    {
        private static readonly IDictionary<MethodInfo, string> _methodToFunctionName = new Dictionary<MethodInfo, string>
        {
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.AsBinary), Type.EmptyTypes), "STAsBinary" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.AsText), Type.EmptyTypes), "AsTextZM" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Buffer), new[] { typeof(double) }), "STBuffer" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Contains), new[] { typeof(Geometry) }), "STContains" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.ConvexHull), Type.EmptyTypes), "STConvexHull" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Difference), new[] { typeof(Geometry) }), "STDifference" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Disjoint), new[] { typeof(Geometry) }), "STDisjoint" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Distance), new[] { typeof(Geometry) }), "STDistance" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.EqualsTopologically), new[] { typeof(Geometry) }), "STEquals" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Intersection), new[] { typeof(Geometry) }), "STIntersection" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Intersects), new[] { typeof(Geometry) }), "STIntersects" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Overlaps), new[] { typeof(Geometry) }), "STOverlaps" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.SymmetricDifference), new[] { typeof(Geometry) }), "STSymDifference" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.ToBinary), Type.EmptyTypes), "STAsBinary" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.ToText), Type.EmptyTypes), "AsTextZM" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Union), new[] { typeof(Geometry) }), "STUnion" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Within), new[] { typeof(Geometry) }), "STWithin" }
        };

        private static readonly IDictionary<MethodInfo, string> _geometryMethodToFunctionName = new Dictionary<MethodInfo, string>
        {
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Crosses), new[] { typeof(Geometry) }), "STCrosses" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Relate), new[] { typeof(Geometry), typeof(string) }), "STRelate" },
            { typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Touches), new[] { typeof(Geometry) }), "STTouches" }
        };

        private static readonly MethodInfo _getGeometryN = typeof(Geometry).GetRuntimeMethod(
            nameof(Geometry.GetGeometryN), new[] { typeof(int) });

        private static readonly MethodInfo _isWithinDistance = typeof(Geometry).GetRuntimeMethod(
            nameof(Geometry.IsWithinDistance), new[] { typeof(Geometry), typeof(double) });

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerGeometryMethodTranslator(
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _typeMappingSource = typeMappingSource;
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

            if (typeof(Geometry).IsAssignableFrom(method.DeclaringType))
            {
                var geometryExpressions = new[] { instance }.Concat(
                    arguments.Where(e => typeof(Geometry).IsAssignableFrom(e.Type)));
                var typeMapping = ExpressionExtensions.InferTypeMapping(geometryExpressions.ToArray());

                Check.DebugAssert(typeMapping != null, "At least one argument must have typeMapping.");
                var storeType = typeMapping.StoreType;
                var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

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
                                typeof(Geometry).IsAssignableFrom(argument.Type)
                                    ? _typeMappingSource.FindMapping(argument.Type, storeType)
                                    : _typeMappingSource.FindMapping(argument.Type)));
                    }

                    var resultTypeMapping = typeof(Geometry).IsAssignableFrom(method.ReturnType)
                        ? _typeMappingSource.FindMapping(method.ReturnType, storeType)
                        : _typeMappingSource.FindMapping(method.ReturnType);

                    var finalArguments = Simplify(typeMappedArguments, isGeography);

                    var argumentsPropagateNullability = functionName == "STBuffer"
                        ? new[] { false }
                        : functionName == "STRelate"
                            ? new[] { true, false }
                            : finalArguments.Select(a => true).ToArray();

                    return _sqlExpressionFactory.Function(
                        instance,
                        functionName,
                        finalArguments,
                        nullable: true,
                        instancePropagatesNullability: true,
                        argumentsPropagateNullability,
                        method.ReturnType,
                        resultTypeMapping);
                }

                if (Equals(method, _getGeometryN))
                {
                    return _sqlExpressionFactory.Function(
                        instance,
                        "STGeometryN",
                        new[]
                        {
                            _sqlExpressionFactory.Add(
                                arguments[0],
                                _sqlExpressionFactory.Constant(1))
                        },
                        nullable: true,
                        instancePropagatesNullability: true,
                        argumentsPropagateNullability: new[] { false },
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
                                typeof(Geometry).IsAssignableFrom(argument.Type)
                                    ? _typeMappingSource.FindMapping(argument.Type, storeType)
                                    : _typeMappingSource.FindMapping(argument.Type)));
                    }

                    var finalArguments = Simplify(new[] { typeMappedArguments[0] }, isGeography);

                    return _sqlExpressionFactory.LessThanOrEqual(
                        _sqlExpressionFactory.Function(
                            instance,
                            "STDistance",
                            finalArguments,
                            nullable: true,
                            instancePropagatesNullability: true,
                            argumentsPropagateNullability: finalArguments.Select(a => true),
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
                    && constant.Value is Geometry geometry
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
