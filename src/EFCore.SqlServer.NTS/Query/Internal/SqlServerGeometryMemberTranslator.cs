// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerGeometryMemberTranslator : IMemberTranslator
{
    private static readonly IDictionary<MemberInfo, string> MemberToFunctionName = new Dictionary<MemberInfo, string>
    {
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.Area))!, "STArea" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.Dimension))!, "STDimension" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.GeometryType))!, "STGeometryType" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.IsEmpty))!, "STIsEmpty" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.IsValid))!, "STIsValid" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.Length))!, "STLength" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.NumGeometries))!, "STNumGeometries" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.NumPoints))!, "STNumPoints" }
    };

    private static readonly IDictionary<MemberInfo, string> GeometryMemberToFunctionName = new Dictionary<MemberInfo, string>
    {
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.Boundary))!, "STBoundary" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.Centroid))!, "STCentroid" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.Envelope))!, "STEnvelope" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.InteriorPoint))!, "STPointOnSurface" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.IsSimple))!, "STIsSimple" },
        { typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.PointOnSurface))!, "STPointOnSurface" }
    };

    private static readonly MemberInfo OgcGeometryType
        = typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.OgcGeometryType))!;

    private static readonly MemberInfo Srid
        = typeof(Geometry).GetTypeInfo().GetRuntimeProperty(nameof(Geometry.SRID))!;

    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerGeometryMemberTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        _typeMappingSource = typeMappingSource;
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
        if (typeof(Geometry).IsAssignableFrom(member.DeclaringType))
        {
            Check.DebugAssert(instance!.TypeMapping != null, "Instance must have typeMapping assigned.");
            var storeType = instance.TypeMapping.StoreType;
            var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

            if (MemberToFunctionName.TryGetValue(member, out var functionName)
                || (!isGeography && GeometryMemberToFunctionName.TryGetValue(member, out functionName)))
            {
                var resultTypeMapping = typeof(Geometry).IsAssignableFrom(returnType)
                    ? _typeMappingSource.FindMapping(returnType, storeType)
                    : _typeMappingSource.FindMapping(returnType);

                return _sqlExpressionFactory.Function(
                    instance,
                    functionName,
                    Enumerable.Empty<SqlExpression>(),
                    nullable: true,
                    instancePropagatesNullability: true,
                    argumentsPropagateNullability: Enumerable.Empty<bool>(),
                    returnType,
                    resultTypeMapping);
            }

            if (Equals(member, OgcGeometryType))
            {
                var whenClauses = new List<CaseWhenClause>
                {
                    new(
                        _sqlExpressionFactory.Constant("Point"),
                        _sqlExpressionFactory.Constant(NetTopologySuite.Geometries.OgcGeometryType.Point)),
                    new(
                        _sqlExpressionFactory.Constant("LineString"),
                        _sqlExpressionFactory.Constant(NetTopologySuite.Geometries.OgcGeometryType.LineString)),
                    new(
                        _sqlExpressionFactory.Constant("Polygon"),
                        _sqlExpressionFactory.Constant(NetTopologySuite.Geometries.OgcGeometryType.Polygon)),
                    new(
                        _sqlExpressionFactory.Constant("MultiPoint"),
                        _sqlExpressionFactory.Constant(NetTopologySuite.Geometries.OgcGeometryType.MultiPoint)),
                    new(
                        _sqlExpressionFactory.Constant("MultiLineString"),
                        _sqlExpressionFactory.Constant(NetTopologySuite.Geometries.OgcGeometryType.MultiLineString)),
                    new(
                        _sqlExpressionFactory.Constant("MultiPolygon"),
                        _sqlExpressionFactory.Constant(NetTopologySuite.Geometries.OgcGeometryType.MultiPolygon)),
                    new(
                        _sqlExpressionFactory.Constant("GeometryCollection"),
                        _sqlExpressionFactory.Constant(NetTopologySuite.Geometries.OgcGeometryType.GeometryCollection)),
                    new(
                        _sqlExpressionFactory.Constant("CircularString"),
                        _sqlExpressionFactory.Constant(NetTopologySuite.Geometries.OgcGeometryType.CircularString)),
                    new(
                        _sqlExpressionFactory.Constant("CompoundCurve"),
                        _sqlExpressionFactory.Constant(NetTopologySuite.Geometries.OgcGeometryType.CompoundCurve)),
                    new(
                        _sqlExpressionFactory.Constant("CurvePolygon"),
                        _sqlExpressionFactory.Constant(NetTopologySuite.Geometries.OgcGeometryType.CurvePolygon))
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
                        Enumerable.Empty<SqlExpression>(),
                        nullable: true,
                        instancePropagatesNullability: true,
                        argumentsPropagateNullability: Enumerable.Empty<bool>(),
                        typeof(string)),
                    whenClauses,
                    null);
            }

            if (Equals(member, Srid))
            {
                return _sqlExpressionFactory.NiladicFunction(
                    instance,
                    "STSrid",
                    nullable: true,
                    instancePropagatesNullability: true,
                    returnType);
            }
        }

        return null;
    }
}
