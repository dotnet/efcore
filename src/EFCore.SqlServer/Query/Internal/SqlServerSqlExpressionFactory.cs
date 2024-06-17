// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

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
    private readonly ISqlEngineSingletonOptions _sqlEngineSingletonOptions;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerSqlExpressionFactory(
        SqlExpressionFactoryDependencies dependencies,
        ISqlEngineSingletonOptions sqlEngineSingletonOptions)
        : base(dependencies)
    {
        _typeMappingSource = dependencies.TypeMappingSource;
        _sqlEngineSingletonOptions = sqlEngineSingletonOptions;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull(nameof(sqlExpression))]
    public override SqlExpression? ApplyTypeMapping(SqlExpression? sqlExpression, RelationalTypeMapping? typeMapping)
        => sqlExpression switch
        {
            null or { TypeMapping: not null } => sqlExpression,

            AtTimeZoneExpression e => ApplyTypeMappingOnAtTimeZone(e, typeMapping),
            SqlServerAggregateFunctionExpression e => e.ApplyTypeMapping(typeMapping),

            _ => base.ApplyTypeMapping(sqlExpression, typeMapping)
        };

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

    /// <inheritdoc />
    public override bool TryCreateLeast(
        IReadOnlyList<SqlExpression> expressions,
        Type resultType,
        [NotNullWhen(true)] out SqlExpression? leastExpression)
    {
        if (_sqlEngineSingletonOptions is ISqlServerSingletonOptions { CompatibilityLevel: >= 160 }
            || _sqlEngineSingletonOptions is IAzureSqlSingletonOptions { CompatibilityLevel: >= 160 }
            || _sqlEngineSingletonOptions is IAzureSynapseSingletonOptions)
        {
            return base.TryCreateLeast(expressions, resultType, out leastExpression);
        }

        leastExpression = null;
        return false;
    }

    /// <inheritdoc />
    public override bool TryCreateGreatest(
        IReadOnlyList<SqlExpression> expressions,
        Type resultType,
        [NotNullWhen(true)] out SqlExpression? greatestExpression)
    {
        if (_sqlEngineSingletonOptions is ISqlServerSingletonOptions { CompatibilityLevel: >= 160 }
            || _sqlEngineSingletonOptions is IAzureSqlSingletonOptions { CompatibilityLevel: >= 160 }
            || _sqlEngineSingletonOptions is IAzureSynapseSingletonOptions)
        {
            return base.TryCreateGreatest(expressions, resultType, out greatestExpression);
        }

        greatestExpression = null;
        return false;
    }
}
