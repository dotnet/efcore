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
    public class SqlServerGeometryMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Area)), "STArea" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Dimension)), "STDimension" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.GeometryType)), "STGeometryType" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.IsEmpty)), "STIsEmpty" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.IsValid)), "STIsValid" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Length)), "STLength" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.NumGeometries)), "STNumGeometries" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.NumPoints)), "STNumPoints" }
        };

        private static readonly IDictionary<MemberInfo, string> _geometryMemberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Boundary)), "STBoundary" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Centroid)), "STCentroid" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Envelope)), "STEnvelope" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.InteriorPoint)), "STPointOnSurface" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.IsSimple)), "STIsSimple" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.PointOnSurface)), "STPointOnSurface" }
        };

        private static readonly MemberInfo _ogcGeometryType = typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.OgcGeometryType));
        private static readonly MemberInfo _srid = typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.SRID));

        private readonly IRelationalTypeMappingSource _typeMappingSource;

        public SqlServerGeometryMemberTranslator(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            if (typeof(IGeometry).IsAssignableFrom(member.DeclaringType))
            {
                Debug.Assert(instance.TypeMapping != null, "Instance must have typeMapping assigned.");
                var storeType = instance.TypeMapping.StoreType;
                var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

                member = member.OnInterface(typeof(IGeometry));
                if (_memberToFunctionName.TryGetValue(member, out var functionName)
                    || (!isGeography && _geometryMemberToFunctionName.TryGetValue(member, out functionName)))
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

                if (Equals(member, _ogcGeometryType))
                {
                    var stringTypeMapping = _typeMappingSource.FindMapping(typeof(string));
                    var resultTypeMapping = _typeMappingSource.FindMapping(returnType);

                    var whenClauses = new List<CaseWhenClause>
                    {
                        new CaseWhenClause(MakeSqlConstant("Point", stringTypeMapping), MakeSqlConstant(OgcGeometryType.Point, resultTypeMapping)),
                        new CaseWhenClause(MakeSqlConstant("LineString", stringTypeMapping), MakeSqlConstant(OgcGeometryType.LineString, resultTypeMapping)),
                        new CaseWhenClause(MakeSqlConstant("Polygon", stringTypeMapping), MakeSqlConstant(OgcGeometryType.Polygon, resultTypeMapping)),
                        new CaseWhenClause(MakeSqlConstant("MultiPoint", stringTypeMapping), MakeSqlConstant(OgcGeometryType.MultiPoint, resultTypeMapping)),
                        new CaseWhenClause(MakeSqlConstant("MultiLineString", stringTypeMapping), MakeSqlConstant(OgcGeometryType.MultiLineString, resultTypeMapping)),
                        new CaseWhenClause(MakeSqlConstant("MultiPolygon", stringTypeMapping), MakeSqlConstant(OgcGeometryType.MultiPolygon, resultTypeMapping)),
                        new CaseWhenClause(MakeSqlConstant("GeometryCollection", stringTypeMapping), MakeSqlConstant(OgcGeometryType.GeometryCollection, resultTypeMapping)),
                        new CaseWhenClause(MakeSqlConstant("CircularString", stringTypeMapping), MakeSqlConstant(OgcGeometryType.CircularString, resultTypeMapping)),
                        new CaseWhenClause(MakeSqlConstant("CompoundCurve", stringTypeMapping), MakeSqlConstant(OgcGeometryType.CompoundCurve, resultTypeMapping)),
                        new CaseWhenClause(MakeSqlConstant("CurvePolygon", stringTypeMapping), MakeSqlConstant(OgcGeometryType.CurvePolygon, resultTypeMapping))
                    };

                    if (isGeography)
                    {
                        whenClauses.Add(new CaseWhenClause(MakeSqlConstant("FullGlobe", stringTypeMapping), MakeSqlConstant((OgcGeometryType)126, resultTypeMapping)));
                    }

                    return new CaseExpression(
                        new SqlFunctionExpression(
                            instance,
                            "STGeometryType",
                            null,
                            returnType,
                            resultTypeMapping,
                            false),
                        whenClauses.ToArray());
                }

                if (Equals(member, _srid))
                {
                    return new SqlFunctionExpression(
                        instance,
                        "STSrid",
                        niladic: true,
                        returnType,
                        _typeMappingSource.FindMapping(returnType),
                        false);
                }
            }

            return null;
        }

        private SqlConstantExpression MakeSqlConstant(object value, RelationalTypeMapping typeMapping)
        {
            return new SqlConstantExpression(Expression.Constant(value), typeMapping);
        }
    }
}
