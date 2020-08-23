// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteGeometryMemberTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
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
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));
            Check.NotNull(logger, nameof(logger));

            if (_memberToFunctionName.TryGetValue(member, out var functionName))
            {
                return returnType == typeof(bool)
                    ? _sqlExpressionFactory.Case(
                        new[]
                        {
                            new CaseWhenClause(
                                _sqlExpressionFactory.IsNotNull(instance),
                                _sqlExpressionFactory.Function(
                                    functionName,
                                    new[] { instance },
                                    nullable: false,
                                    argumentsPropagateNullability: new[] { false },
                                    returnType))
                        },
                        null)
                    : (SqlExpression)_sqlExpressionFactory.Function(
                        functionName,
                        new[] { instance },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        returnType);
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
                                new[] { instance },
                                nullable: true,
                                argumentsPropagateNullability: new[] { true },
                                returnType),
                            _sqlExpressionFactory.Constant(" ZM")
                        },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        returnType),
                    new[]
                    {
                        new CaseWhenClause(_sqlExpressionFactory.Constant("POINT"), _sqlExpressionFactory.Constant("Point")),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("LINESTRING"), _sqlExpressionFactory.Constant("LineString")),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("POLYGON"), _sqlExpressionFactory.Constant("Polygon")),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("MULTIPOINT"), _sqlExpressionFactory.Constant("MultiPoint")),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("MULTILINESTRING"), _sqlExpressionFactory.Constant("MultiLineString")),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("MULTIPOLYGON"), _sqlExpressionFactory.Constant("MultiPolygon")),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("GEOMETRYCOLLECTION"), _sqlExpressionFactory.Constant("GeometryCollection"))
                    },
                    null);
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
                                new[] { instance },
                                nullable: true,
                                argumentsPropagateNullability: new[] { true },
                                typeof(string)),
                            _sqlExpressionFactory.Constant(" ZM")
                        },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        typeof(string)),
                    new[]
                    {
                        new CaseWhenClause(_sqlExpressionFactory.Constant("POINT"), _sqlExpressionFactory.Constant(OgcGeometryType.Point)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("LINESTRING"), _sqlExpressionFactory.Constant(OgcGeometryType.LineString)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("POLYGON"), _sqlExpressionFactory.Constant(OgcGeometryType.Polygon)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("MULTIPOINT"), _sqlExpressionFactory.Constant(OgcGeometryType.MultiPoint)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("MULTILINESTRING"),
                            _sqlExpressionFactory.Constant(OgcGeometryType.MultiLineString)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("MULTIPOLYGON"), _sqlExpressionFactory.Constant(OgcGeometryType.MultiPolygon)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("GEOMETRYCOLLECTION"),
                            _sqlExpressionFactory.Constant(OgcGeometryType.GeometryCollection))
                    },
                    null);
            }

            return null;
        }
    }
}
