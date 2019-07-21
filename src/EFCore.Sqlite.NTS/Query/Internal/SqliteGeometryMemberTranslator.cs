// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    public class SqliteGeometryMemberTranslator : IMemberTranslator
    {
        private static readonly IDictionary<MemberInfo, string> _memberToFunctionName = new Dictionary<MemberInfo, string>
        {
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.Area)), "Area" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.Boundary)), "Boundary" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.Centroid)), "Centroid" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.Dimension)), "Dimension" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.Envelope)), "Envelope" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.InteriorPoint)), "PointOnSurface" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.IsEmpty)), "IsEmpty" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.IsSimple)), "IsSimple" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.IsValid)), "IsValid" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.Length)), "GLength" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.NumGeometries)), "NumGeometries" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.NumPoints)), "NumPoints" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.PointOnSurface)), "PointOnSurface" },
            { typeof(Geometry).GetRuntimeProperty(nameof(Geometry.SRID)), "SRID" }
        };

        private static readonly MemberInfo _geometryType = typeof(Geometry).GetRuntimeProperty(nameof(Geometry.GeometryType));
        private static readonly MemberInfo _ogcGeometryType = typeof(Geometry).GetRuntimeProperty(nameof(Geometry.OgcGeometryType));
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqliteGeometryMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
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
