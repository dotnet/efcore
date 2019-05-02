// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerPolygonMemberTranslator : IMemberTranslator
    {
        private static readonly MemberInfo _exteriorRing = typeof(IPolygon).GetRuntimeProperty(nameof(IPolygon.ExteriorRing));
        private static readonly MemberInfo _numInteriorRings = typeof(IPolygon).GetRuntimeProperty(nameof(IPolygon.NumInteriorRings));

        private static readonly IDictionary<MemberInfo, string> _geometryMemberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { _exteriorRing, "STExteriorRing" },
            { _numInteriorRings, "STNumInteriorRing" }
        };

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerPolygonMemberTranslator(IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _typeMappingSource = typeMappingSource;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            if (typeof(IPolygon).IsAssignableFrom(member.DeclaringType))
            {
                member = member.OnInterface(typeof(IPolygon));
                Debug.Assert(instance.TypeMapping != null, "Instance must have typeMapping assigned.");
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
                            returnType,
                            _typeMappingSource.FindMapping(returnType, storeType));
                    }

                    if (Equals(_numInteriorRings, member))
                    {
                        return _sqlExpressionFactory.Subtract(
                            _sqlExpressionFactory.Function(
                                instance,
                                "NumRings",
                                false,
                                returnType),
                            _sqlExpressionFactory.Constant(1));
                    }
                }

                if (_geometryMemberToFunctionName.TryGetValue(member, out var functionName))
                {
                    var resultTypeMapping = typeof(IGeometry).IsAssignableFrom(returnType)
                        ? _typeMappingSource.FindMapping(returnType, storeType)
                        : _typeMappingSource.FindMapping(returnType);

                    return _sqlExpressionFactory.Function(
                        instance,
                        functionName,
                        false,
                        returnType,
                        resultTypeMapping);
                }
            }

            return null;
        }
    }
}
