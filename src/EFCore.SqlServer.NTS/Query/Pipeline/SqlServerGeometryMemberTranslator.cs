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
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerGeometryMemberTranslator(IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _typeMappingSource = typeMappingSource;
            _sqlExpressionFactory = sqlExpressionFactory;
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

                    return _sqlExpressionFactory.Function(
                        instance,
                        functionName,
                        false,
                        returnType,
                        resultTypeMapping);
                }

                if (Equals(member, _ogcGeometryType))
                {
                    var whenClauses = new List<CaseWhenClause>
                    {
                        new CaseWhenClause(_sqlExpressionFactory.Constant("Point"), _sqlExpressionFactory.Constant(OgcGeometryType.Point)),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("LineString"), _sqlExpressionFactory.Constant(OgcGeometryType.LineString)),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("Polygon"), _sqlExpressionFactory.Constant(OgcGeometryType.Polygon)),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("MultiPoint"), _sqlExpressionFactory.Constant(OgcGeometryType.MultiPoint)),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("MultiLineString"), _sqlExpressionFactory.Constant(OgcGeometryType.MultiLineString)),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("MultiPolygon"), _sqlExpressionFactory.Constant(OgcGeometryType.MultiPolygon)),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("GeometryCollection"), _sqlExpressionFactory.Constant(OgcGeometryType.GeometryCollection)),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("CircularString"), _sqlExpressionFactory.Constant(OgcGeometryType.CircularString)),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("CompoundCurve"), _sqlExpressionFactory.Constant(OgcGeometryType.CompoundCurve)),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("CurvePolygon"), _sqlExpressionFactory.Constant(OgcGeometryType.CurvePolygon))
                    };

                    if (isGeography)
                    {
                        whenClauses.Add(new CaseWhenClause(_sqlExpressionFactory.Constant("FullGlobe"), _sqlExpressionFactory.Constant((OgcGeometryType)126)));
                    }

                    return _sqlExpressionFactory.Case(
                        _sqlExpressionFactory.Function(
                            instance,
                            "STGeometryType",
                            false,
                            typeof(string)),
                        whenClauses.ToArray());
                }

                if (Equals(member, _srid))
                {
                    return _sqlExpressionFactory.Function(
                        instance,
                        "STSrid",
                        true,
                        returnType);
                }
            }

            return null;
        }
    }
}
