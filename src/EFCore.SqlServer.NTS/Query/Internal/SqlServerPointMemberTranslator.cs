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
public class SqlServerPointMemberTranslator : IMemberTranslator
{
    private static readonly IDictionary<MemberInfo, string> MemberToPropertyName = new Dictionary<MemberInfo, string>
    {
        { typeof(Point).GetTypeInfo().GetRuntimeProperty(nameof(Point.M))!, "M" },
        { typeof(Point).GetTypeInfo().GetRuntimeProperty(nameof(Point.Z))!, "Z" }
    };

    private static readonly IDictionary<MemberInfo, string> GeographyMemberToPropertyName = new Dictionary<MemberInfo, string>
    {
        { typeof(Point).GetTypeInfo().GetRuntimeProperty(nameof(Point.X))!, "Long" },
        { typeof(Point).GetTypeInfo().GetRuntimeProperty(nameof(Point.Y))!, "Lat" }
    };

    private static readonly IDictionary<MemberInfo, string> GeometryMemberToPropertyName = new Dictionary<MemberInfo, string>
    {
        { typeof(Point).GetTypeInfo().GetRuntimeProperty(nameof(Point.X))!, "STX" },
        { typeof(Point).GetTypeInfo().GetRuntimeProperty(nameof(Point.Y))!, "STY" }
    };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerPointMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (typeof(Point).IsAssignableFrom(member.DeclaringType))
        {
            Check.DebugAssert(instance!.TypeMapping != null, "Instance must have typeMapping assigned.");
            var storeType = instance.TypeMapping.StoreType;
            var isGeography = string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

            if (MemberToPropertyName.TryGetValue(member, out var propertyName)
                || (isGeography
                    ? GeographyMemberToPropertyName.TryGetValue(member, out propertyName)
                    : GeometryMemberToPropertyName.TryGetValue(member, out propertyName))
                && propertyName != null)
            {
                return _sqlExpressionFactory.NiladicFunction(
                    instance,
                    propertyName,
                    nullable: true,
                    instancePropagatesNullability: true,
                    returnType);
            }
        }

        return null;
    }
}
