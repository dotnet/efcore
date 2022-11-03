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
public class SqlServerLineStringMemberTranslator : IMemberTranslator
{
    private static readonly IDictionary<MemberInfo, string> MemberToFunctionName = new Dictionary<MemberInfo, string>
    {
        { typeof(LineString).GetTypeInfo().GetRuntimeProperty(nameof(LineString.Count))!, "STNumPoints" },
        { typeof(LineString).GetTypeInfo().GetRuntimeProperty(nameof(LineString.EndPoint))!, "STEndPoint" },
        { typeof(LineString).GetTypeInfo().GetRuntimeProperty(nameof(LineString.IsClosed))!, "STIsClosed" },
        { typeof(LineString).GetTypeInfo().GetRuntimeProperty(nameof(LineString.StartPoint))!, "STStartPoint" },
        { typeof(LineString).GetTypeInfo().GetRuntimeProperty(nameof(LineString.IsRing))!, "STIsRing" }
    };

    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerLineStringMemberTranslator(
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
        if (MemberToFunctionName.TryGetValue(member, out var functionName))
        {
            Check.DebugAssert(instance!.TypeMapping != null, "Instance must have typeMapping assigned.");
            var storeType = instance.TypeMapping.StoreType;
            var isGeography = storeType == "geography";

            if (isGeography && functionName == "STIsRing")
            {
                return null;
            }

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

        return null;
    }
}
