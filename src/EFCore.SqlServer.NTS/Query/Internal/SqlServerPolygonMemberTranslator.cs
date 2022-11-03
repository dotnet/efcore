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
public class SqlServerPolygonMemberTranslator : IMemberTranslator
{
    private static readonly MemberInfo ExteriorRing
        = typeof(Polygon).GetTypeInfo().GetRuntimeProperty(nameof(Polygon.ExteriorRing))!;

    private static readonly MemberInfo NumInteriorRings
        = typeof(Polygon).GetTypeInfo().GetRuntimeProperty(nameof(Polygon.NumInteriorRings))!;

    private static readonly IDictionary<MemberInfo, string> GeometryMemberToFunctionName = new Dictionary<MemberInfo, string>
    {
        { ExteriorRing, "STExteriorRing" }, { NumInteriorRings, "STNumInteriorRing" }
    };

    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerPolygonMemberTranslator(
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
        if (typeof(Polygon).IsAssignableFrom(member.DeclaringType))
        {
            Check.DebugAssert(instance!.TypeMapping != null, "Instance must have typeMapping assigned.");
            var storeType = instance.TypeMapping.StoreType;
            var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

            if (isGeography)
            {
                if (Equals(ExteriorRing, member))
                {
                    return _sqlExpressionFactory.Function(
                        instance,
                        "RingN",
                        new[] { _sqlExpressionFactory.Constant(1) },
                        nullable: true,
                        instancePropagatesNullability: true,
                        argumentsPropagateNullability: new[] { false },
                        returnType,
                        _typeMappingSource.FindMapping(returnType, storeType));
                }

                if (Equals(NumInteriorRings, member))
                {
                    return _sqlExpressionFactory.Subtract(
                        _sqlExpressionFactory.Function(
                            instance,
                            "NumRings",
                            Enumerable.Empty<SqlExpression>(),
                            nullable: true,
                            instancePropagatesNullability: true,
                            argumentsPropagateNullability: Enumerable.Empty<bool>(),
                            returnType),
                        _sqlExpressionFactory.Constant(1));
                }
            }

            if (GeometryMemberToFunctionName.TryGetValue(member, out var functionName))
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
        }

        return null;
    }
}
