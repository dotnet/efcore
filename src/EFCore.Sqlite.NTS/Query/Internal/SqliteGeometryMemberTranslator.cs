// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteGeometryMemberTranslator : IMemberTranslator
{
    private static readonly IDictionary<MemberInfo, string> MemberToFunctionName = new Dictionary<MemberInfo, string>
    {
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.Area))!, "Area" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.Boundary))!, "Boundary" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.Centroid))!, "Centroid" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.Dimension))!, "Dimension" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.Envelope))!, "Envelope" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.InteriorPoint))!, "PointOnSurface" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.IsEmpty))!, "IsEmpty" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.IsSimple))!, "IsSimple" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.IsValid))!, "IsValid" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.Length))!, "GLength" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.NumGeometries))!, "NumGeometries" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.NumPoints))!, "NumPoints" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.PointOnSurface))!, "PointOnSurface" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.SRID))!, "SRID" }
    };

    private static readonly MemberInfo GeometryType
        = typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.GeometryType))!;

    private static readonly MemberInfo OgcGeometryType
        = typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.OgcGeometryType))!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteGeometryMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
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
        if (instance != null)
        {
            if (MemberToFunctionName.TryGetValue(member, out var functionName))
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
                    : _sqlExpressionFactory.Function(
                        functionName,
                        new[] { instance },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        returnType);
            }

            if (Equals(member, GeometryType))
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
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("MULTIPOLYGON"), _sqlExpressionFactory.Constant("MultiPolygon")),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("GEOMETRYCOLLECTION"), _sqlExpressionFactory.Constant("GeometryCollection"))
                    },
                    null);
            }

            if (Equals(member, OgcGeometryType))
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
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("POINT"),
                            _sqlExpressionFactory.Constant(NetTopologySuite.Geometries.OgcGeometryType.Point)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("LINESTRING"),
                            _sqlExpressionFactory.Constant(NetTopologySuite.Geometries.OgcGeometryType.LineString)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("POLYGON"),
                            _sqlExpressionFactory.Constant(NetTopologySuite.Geometries.OgcGeometryType.Polygon)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("MULTIPOINT"),
                            _sqlExpressionFactory.Constant(NetTopologySuite.Geometries.OgcGeometryType.MultiPoint)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("MULTILINESTRING"),
                            _sqlExpressionFactory.Constant(NetTopologySuite.Geometries.OgcGeometryType.MultiLineString)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("MULTIPOLYGON"),
                            _sqlExpressionFactory.Constant(NetTopologySuite.Geometries.OgcGeometryType.MultiPolygon)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.Constant("GEOMETRYCOLLECTION"),
                            _sqlExpressionFactory.Constant(NetTopologySuite.Geometries.OgcGeometryType.GeometryCollection))
                    },
                    null);
            }
        }

        return null;
    }
}
