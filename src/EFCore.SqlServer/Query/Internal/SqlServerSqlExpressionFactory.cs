// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerSqlExpressionFactory : SqlExpressionFactory
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerSqlExpressionFactory(SqlExpressionFactoryDependencies dependencies)
        : base(dependencies)
    {
        _typeMappingSource = dependencies.TypeMappingSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull("sqlExpression")]
    public override SqlExpression? ApplyTypeMapping(SqlExpression? sqlExpression, RelationalTypeMapping? typeMapping)
    {
#pragma warning disable IDE0046 // Convert to conditional expression
        if (sqlExpression == null
#pragma warning restore IDE0046 // Convert to conditional expression
            || sqlExpression.TypeMapping != null)
        {
            return sqlExpression;
        }

        return sqlExpression switch
        {
            AtTimeZoneExpression e => ApplyTypeMappingOnAtTimeZone(e, typeMapping),
            SqlServerAggregateFunctionExpression e => e.ApplyTypeMapping(typeMapping),

            _ => base.ApplyTypeMapping(sqlExpression, typeMapping)
        };
    }

    private SqlExpression ApplyTypeMappingOnAtTimeZone(AtTimeZoneExpression atTimeZoneExpression, RelationalTypeMapping? typeMapping)
    {
        var operandTypeMapping = typeMapping is null
            ? null
            : atTimeZoneExpression.Operand.Type == typeof(DateTimeOffset)
                ? typeMapping
                : atTimeZoneExpression.Operand.Type == typeof(DateTime)
                    ? _typeMappingSource.FindMapping(typeof(DateTime), "datetime2", precision: typeMapping.Precision)
                    : null;

        return new AtTimeZoneExpression(
            operandTypeMapping is null ? atTimeZoneExpression.Operand : ApplyTypeMapping(atTimeZoneExpression.Operand, operandTypeMapping),
            atTimeZoneExpression.TimeZone,
            atTimeZoneExpression.Type,
            typeMapping);
    }
}
