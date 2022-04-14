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
public class SqliteLineStringMemberTranslator : IMemberTranslator
{
    private static readonly IDictionary<MemberInfo, string> MemberToFunctionName
        = new Dictionary<MemberInfo, string>
        {
            { typeof(LineString).GetTypeInfo().GetRuntimeProperty(nameof(LineString.Count))!, "NumPoints" },
            { typeof(LineString).GetTypeInfo().GetRuntimeProperty(nameof(LineString.EndPoint))!, "EndPoint" },
            { typeof(LineString).GetTypeInfo().GetRuntimeProperty(nameof(LineString.IsClosed))!, "IsClosed" },
            { typeof(LineString).GetTypeInfo().GetRuntimeProperty(nameof(LineString.IsRing))!, "IsRing" },
            { typeof(LineString).GetTypeInfo().GetRuntimeProperty(nameof(LineString.StartPoint))!, "StartPoint" }
        };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteLineStringMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (MemberToFunctionName.TryGetValue(member, out var functionName)
            && instance != null)
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

        return null;
    }
}
