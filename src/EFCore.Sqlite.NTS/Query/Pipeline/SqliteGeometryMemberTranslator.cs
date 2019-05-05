// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public class SqliteGeometryMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Area)), "Area" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Boundary)), "Boundary" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Centroid)), "Centroid" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Dimension)), "Dimension" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Envelope)), "Envelope" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.InteriorPoint)), "PointOnSurface" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.IsEmpty)), "IsEmpty" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.IsSimple)), "IsSimple" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.IsValid)), "IsValid" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.Length)), "GLength" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.NumGeometries)), "NumGeometries" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.NumPoints)), "NumPoints" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.PointOnSurface)), "PointOnSurface" },
            { typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.SRID)), "SRID" }
        };

        private static readonly MemberInfo _geometryType = typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.GeometryType));
        private static readonly MemberInfo _ogcGeometryType = typeof(IGeometry).GetRuntimeProperty(nameof(IGeometry.OgcGeometryType));
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqliteGeometryMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            member = member.OnInterface(typeof(IGeometry));
            if (_memberToFunctionName.TryGetValue(member, out var functionName))
            {
                SqlExpression translation = _sqlExpressionFactory.Function(functionName, new[] { instance }, returnType);

                if (returnType == typeof(bool))
                {
                    translation = _sqlExpressionFactory.Case(
                        new[]
                        {
                            new CaseWhenClause(
                                _sqlExpressionFactory.IsNotNull(instance),
                                translation)
                        },
                        null);
                }

                return translation;
            }

            if (Equals(member, _geometryType))
            {
                return _sqlExpressionFactory.Case(
                    _sqlExpressionFactory.Function(
                        "rtrim",
                        new SqlExpression[]
                        {
                            _sqlExpressionFactory.Function(
                                "GeometryType",
                                new []
                                {
                                    instance,
                                },
                                returnType),
                            _sqlExpressionFactory.Constant(" ZM")
                        },
                        returnType),
                    new CaseWhenClause(_sqlExpressionFactory.Constant("POINT"), _sqlExpressionFactory.Constant("Point")),
                    new CaseWhenClause(_sqlExpressionFactory.Constant("LINESTRING"), _sqlExpressionFactory.Constant("LineString")),
                    new CaseWhenClause(_sqlExpressionFactory.Constant("POLYGON"), _sqlExpressionFactory.Constant("Polygon")),
                    new CaseWhenClause(_sqlExpressionFactory.Constant("MULTIPOINT"), _sqlExpressionFactory.Constant("MultiPoint")),
                    new CaseWhenClause(_sqlExpressionFactory.Constant("MULTILINESTRING"), _sqlExpressionFactory.Constant("MultiLineString")),
                    new CaseWhenClause(_sqlExpressionFactory.Constant("MULTIPOLYGON"), _sqlExpressionFactory.Constant("MultiPolygon")),
                    new CaseWhenClause(_sqlExpressionFactory.Constant("GEOMETRYCOLLECTION"), _sqlExpressionFactory.Constant("GeometryCollection")));
            }

            if (Equals(member, _ogcGeometryType))
            {
                return _sqlExpressionFactory.Case(
                    _sqlExpressionFactory.Function(
                        "rtrim",
                        new SqlExpression[]
                        {
                            _sqlExpressionFactory.Function(
                                "GeometryType",
                                new []
                                {
                                    instance,
                                },
                                typeof(string)),
                            _sqlExpressionFactory.Constant(" ZM")
                        },
                        typeof(string)),
                    new CaseWhenClause(_sqlExpressionFactory.Constant("POINT"), _sqlExpressionFactory.Constant(OgcGeometryType.Point)),
                    new CaseWhenClause(_sqlExpressionFactory.Constant("LINESTRING"), _sqlExpressionFactory.Constant(OgcGeometryType.LineString)),
                    new CaseWhenClause(_sqlExpressionFactory.Constant("POLYGON"), _sqlExpressionFactory.Constant(OgcGeometryType.Polygon)),
                    new CaseWhenClause(_sqlExpressionFactory.Constant("MULTIPOINT"), _sqlExpressionFactory.Constant(OgcGeometryType.MultiPoint)),
                    new CaseWhenClause(_sqlExpressionFactory.Constant("MULTILINESTRING"), _sqlExpressionFactory.Constant(OgcGeometryType.MultiLineString)),
                    new CaseWhenClause(_sqlExpressionFactory.Constant("MULTIPOLYGON"), _sqlExpressionFactory.Constant(OgcGeometryType.MultiPolygon)),
                    new CaseWhenClause(_sqlExpressionFactory.Constant("GEOMETRYCOLLECTION"), _sqlExpressionFactory.Constant(OgcGeometryType.GeometryCollection)));
            }

            return null;
        }
    }
}
