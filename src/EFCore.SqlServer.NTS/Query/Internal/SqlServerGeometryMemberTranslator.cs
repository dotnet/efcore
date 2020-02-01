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
    public class SqlServerGeometryMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.Area)), "STArea" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.Dimension)), "STDimension" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.GeometryType)), "STGeometryType" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.IsEmpty)), "STIsEmpty" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.IsValid)), "STIsValid" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.Length)), "STLength" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.NumGeometries)), "STNumGeometries" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.NumPoints)), "STNumPoints" }
        };

        private static readonly IDictionary<MemberInfo, string> _geometryMemberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.Boundary)), "STBoundary" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.Centroid)), "STCentroid" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.Envelope)), "STEnvelope" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.InteriorPoint)), "STPointOnSurface" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.IsSimple)), "STIsSimple" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.PointOnSurface)), "STPointOnSurface" }
        };

        private static readonly MemberInfo _ogcGeometryType = typeof(Geometry).GetRuntimeProperty(nameof(Geometry.OgcGeometryType));
        private static readonly MemberInfo _srid = typeof(Geometry).GetRuntimeProperty(nameof(Geometry.SRID));

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerGeometryMemberTranslator(
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

            if (typeof(Geometry).IsAssignableFrom(member.DeclaringType))
            {
                Check.DebugAssert(instance.TypeMapping != null, "Instance must have typeMapping assigned.");
                var storeType = instance.TypeMapping.StoreType;
                var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

                if (_memberToFunctionName.TryGetValue(member, out var functionName)
                    || (!isGeography && _geometryMemberToFunctionName.TryGetValue(member, out functionName)))
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

                if (Equals(member, _ogcGeometryType))
                {
                    var whenClauses = new List<CaseWhenClause>
                    {
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("Point"), _sqlExpressionFactory.Constant(OgcGeometryType.Point)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("LineString"), _sqlExpressionFactory.Constant(OgcGeometryType.LineString)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("Polygon"), _sqlExpressionFactory.Constant(OgcGeometryType.Polygon)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("MultiPoint"), _sqlExpressionFactory.Constant(OgcGeometryType.MultiPoint)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("MultiLineString"),
                            _sqlExpressionFactory.Constant(OgcGeometryType.MultiLineString)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("MultiPolygon"),
                            _sqlExpressionFactory.Constant(OgcGeometryType.MultiPolygon)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("GeometryCollection"),
                            _sqlExpressionFactory.Constant(OgcGeometryType.GeometryCollection)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("CircularString"),
                            _sqlExpressionFactory.Constant(OgcGeometryType.CircularString)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("CompoundCurve"),
                            _sqlExpressionFactory.Constant(OgcGeometryType.CompoundCurve)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("CurvePolygon"),
                            _sqlExpressionFactory.Constant(OgcGeometryType.CurvePolygon))
                    };

                    if (isGeography)
                    {
                        whenClauses.Add(
                            new CaseWhenClause(
                                _sqlExpressionFactory.Constant("FullGlobe"), _sqlExpressionFactory.Constant((OgcGeometryType)126)));
                    }

                    return _sqlExpressionFactory.Case(
                        _sqlExpressionFactory.Function(
                            instance,
                            "STGeometryType",
                            Array.Empty<SqlExpression>(),
                            nullResultAllowed: true,
                            instancePropagatesNullability: true,
                            argumentsPropagateNullability: Array.Empty<bool>(),
                            typeof(string)),
                        whenClauses.ToArray());
                }

                if (Equals(member, _srid))
                {
                    return _sqlExpressionFactory.Function(
                        instance,
                        "STSrid",
                        nullResultAllowed: true,
                        instancePropagatesNullability: true,
                        returnType);
                }
            }

            return null;
        }
    }
}
