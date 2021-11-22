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
    private static readonly IDictionary<MemberInfo, string> _memberToFunctionName = new Dictionary<MemberInfo, string>
    {
        { typeof(Geometry).GetRequiredRuntimeProperty(nameof(Geometry.Area)), "STArea" },
        { typeof(Geometry).GetRequiredRuntimeProperty(nameof(Geometry.Dimension)), "STDimension" },
        { typeof(Geometry).GetRequiredRuntimeProperty(nameof(Geometry.GeometryType)), "STGeometryType" },
        { typeof(Geometry).GetRequiredRuntimeProperty(nameof(Geometry.IsEmpty)), "STIsEmpty" },
        { typeof(Geometry).GetRequiredRuntimeProperty(nameof(Geometry.IsValid)), "STIsValid" },
        { typeof(Geometry).GetRequiredRuntimeProperty(nameof(Geometry.Length)), "STLength" },
        { typeof(Geometry).GetRequiredRuntimeProperty(nameof(Geometry.NumGeometries)), "STNumGeometries" },
        { typeof(Geometry).GetRequiredRuntimeProperty(nameof(Geometry.NumPoints)), "STNumPoints" }
    };

    private static readonly IDictionary<MemberInfo, string> _geometryMemberToFunctionName = new Dictionary<MemberInfo, string>
    {
        { typeof(Geometry).GetRequiredRuntimeProperty(nameof(Geometry.Boundary)), "STBoundary" },
        { typeof(Geometry).GetRequiredRuntimeProperty(nameof(Geometry.Centroid)), "STCentroid" },
        { typeof(Geometry).GetRequiredRuntimeProperty(nameof(Geometry.Envelope)), "STEnvelope" },
        { typeof(Geometry).GetRequiredRuntimeProperty(nameof(Geometry.InteriorPoint)), "STPointOnSurface" },
        { typeof(Geometry).GetRequiredRuntimeProperty(nameof(Geometry.IsSimple)), "STIsSimple" },
        { typeof(Geometry).GetRequiredRuntimeProperty(nameof(Geometry.PointOnSurface)), "STPointOnSurface" }
    };

    private static readonly MemberInfo _ogcGeometryType = typeof(Geometry).GetRequiredRuntimeProperty(nameof(Geometry.OgcGeometryType));
    private static readonly MemberInfo _srid = typeof(Geometry).GetRequiredRuntimeProperty(nameof(Geometry.SRID));

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
                    nullable: true,
                    instancePropagatesNullability: true,
                    argumentsPropagateNullability: Array.Empty<bool>(),
                    returnType,
                    resultTypeMapping);
            }

            if (Equals(member, _ogcGeometryType))
            {
                var whenClauses = new List<CaseWhenClause>
                {
                    new(_sqlExpressionFactory.Constant("Point"), _sqlExpressionFactory.Constant(OgcGeometryType.Point)),
                    new(_sqlExpressionFactory.Constant("LineString"), _sqlExpressionFactory.Constant(OgcGeometryType.LineString)),
                    new(_sqlExpressionFactory.Constant("Polygon"), _sqlExpressionFactory.Constant(OgcGeometryType.Polygon)),
                    new(_sqlExpressionFactory.Constant("MultiPoint"), _sqlExpressionFactory.Constant(OgcGeometryType.MultiPoint)),
                    new(
                        _sqlExpressionFactory.Constant("MultiLineString"),
                        _sqlExpressionFactory.Constant(OgcGeometryType.MultiLineString)),
                    new(_sqlExpressionFactory.Constant("MultiPolygon"), _sqlExpressionFactory.Constant(OgcGeometryType.MultiPolygon)),
                    new(
                        _sqlExpressionFactory.Constant("GeometryCollection"),
                        _sqlExpressionFactory.Constant(OgcGeometryType.GeometryCollection)),
                    new(
                        _sqlExpressionFactory.Constant("CircularString"),
                        _sqlExpressionFactory.Constant(OgcGeometryType.CircularString)),
                    new(
                        _sqlExpressionFactory.Constant("CompoundCurve"),
                        _sqlExpressionFactory.Constant(OgcGeometryType.CompoundCurve)),
                    new(
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
                        nullable: true,
                        instancePropagatesNullability: true,
                        argumentsPropagateNullability: Array.Empty<bool>(),
                        typeof(string)),
                    whenClauses,
                    null);
            }

            if (Equals(member, _srid))
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
