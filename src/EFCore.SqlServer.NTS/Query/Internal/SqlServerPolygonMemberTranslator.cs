// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
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
    public class SqlServerPolygonMemberTranslator : IMemberTranslator
    {
        private static readonly MemberInfo _exteriorRing = typeof(Polygon).GetRequiredRuntimeProperty(nameof(Polygon.ExteriorRing));
        private static readonly MemberInfo _numInteriorRings = typeof(Polygon).GetRequiredRuntimeProperty(nameof(Polygon.NumInteriorRings));

        private static readonly IDictionary<MemberInfo, string> _geometryMemberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { _exteriorRing, "STExteriorRing" }, { _numInteriorRings, "STNumInteriorRing" }
        };

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerPolygonMemberTranslator(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory)
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
        public virtual SqlExpression? Translate(
            SqlExpression? instance,
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));
            Check.NotNull(logger, nameof(logger));

            if (typeof(Polygon).IsAssignableFrom(member.DeclaringType))
            {
                Check.DebugAssert(instance!.TypeMapping != null, "Instance must have typeMapping assigned.");
                var storeType = instance.TypeMapping.StoreType;
                var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

                if (isGeography)
                {
                    if (Equals(_exteriorRing, member))
                    {
                        return _sqlExpressionFactory.Function(
                            instance,
                            "RingN",
                            new[] { _sqlExpressionFactory.Constant(1) },
                            nullable: true,
                            instancePropagatesNullability: true,
                            argumentsPropagateNullability: new[] { false },
                            returnType,
                            _typeMappingSource.FindMapping(returnType, storeType));
                    }

                    if (Equals(_numInteriorRings, member))
                    {
                        return _sqlExpressionFactory.Subtract(
                            _sqlExpressionFactory.Function(
                                instance,
                                "NumRings",
                                Array.Empty<SqlExpression>(),
                                nullable: true,
                                instancePropagatesNullability: true,
                                argumentsPropagateNullability: Array.Empty<bool>(),
                                returnType),
                            _sqlExpressionFactory.Constant(1));
                    }
                }

                if (_geometryMemberToFunctionName.TryGetValue(member, out var functionName))
                {
                    var resultTypeMapping = typeof(Geometry).IsAssignableFrom(returnType)
                        ? _typeMappingSource.FindMapping(returnType, storeType)
                        : _typeMappingSource.FindMapping(returnType);

                    return _sqlExpressionFactory.Function(
                        instance,
                        functionName,
                        Array.Empty<SqlExpression>(),
                        nullable: true,
                        instancePropagatesNullability: true,
                        argumentsPropagateNullability: Array.Empty<bool>(),
                        returnType,
                        resultTypeMapping);
                }
            }

            return null;
        }
    }
}
