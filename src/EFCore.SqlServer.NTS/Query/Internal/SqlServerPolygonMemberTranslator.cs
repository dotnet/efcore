// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerPolygonMemberTranslator : IMemberTranslator
    {
        private static readonly MemberInfo _exteriorRing = typeof(Polygon).GetRuntimeProperty(nameof(Polygon.ExteriorRing));
        private static readonly MemberInfo _numInteriorRings = typeof(Polygon).GetRuntimeProperty(nameof(Polygon.NumInteriorRings));

        private static readonly IDictionary<MemberInfo, string> _geometryMemberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { _exteriorRing, "STExteriorRing" }, { _numInteriorRings, "STNumInteriorRing" }
        };

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerPolygonMemberTranslator(
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _typeMappingSource = typeMappingSource;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));

            if (typeof(Polygon).IsAssignableFrom(member.DeclaringType))
            {
                Check.DebugAssert(instance.TypeMapping != null, "Instance must have typeMapping assigned.");
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
                            nullResultAllowed: true,
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
                                nullResultAllowed: true,
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
                        nullResultAllowed: true,
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
