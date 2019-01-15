// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
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
        private readonly RelationalTypeMapping _intTypeMapping;

        public SqlServerPolygonMemberTranslator(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
            _intTypeMapping = typeMappingSource.FindMapping(typeof(int));
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
                    var constantExpression = new SqlConstantExpression(Expression.Constant(1), _intTypeMapping);
                    if (Equals(_exteriorRing, member))
                    {
                        return new SqlFunctionExpression(
                            instance,
                            "RingN",
                            new[] { constantExpression },
                            returnType,
                            _typeMappingSource.FindMapping(returnType, storeType),
                            false);
                    }

                    if (Equals(_numInteriorRings, member))
                    {
                        return new SqlBinaryExpression(
                            ExpressionType.Subtract,
                            new SqlFunctionExpression(
                                instance,
                                "NumRings",
                                null,
                                returnType,
                                _intTypeMapping,
                                false),
                            constantExpression,
                            returnType,
                            _intTypeMapping);
                    }
                }

                if (_geometryMemberToFunctionName.TryGetValue(member, out var functionName))
                {
                    var resultTypeMapping = typeof(IGeometry).IsAssignableFrom(returnType)
                        ? _typeMappingSource.FindMapping(returnType, storeType)
                        : _typeMappingSource.FindMapping(returnType);

                    return new SqlFunctionExpression(
                        instance,
                        functionName,
                        null,
                        returnType,
                        resultTypeMapping,
                        false);
                }
            }

            return null;
        }
    }
}
