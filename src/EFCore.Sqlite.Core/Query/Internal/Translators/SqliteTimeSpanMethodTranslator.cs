// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteTimeSpanMethodTranslator(SqliteSqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
{
    private static readonly MethodInfo DurationMethod
        = typeof(TimeSpan).GetRuntimeMethod(nameof(TimeSpan.Duration), Type.EmptyTypes)!;

    private static readonly MethodInfo NegateMethod
        = typeof(TimeSpan).GetRuntimeMethod(nameof(TimeSpan.Negate), Type.EmptyTypes)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance is null || method.DeclaringType != typeof(TimeSpan))
        {
            return null;
        }

        if (method == DurationMethod)
        {
            return sqlExpressionFactory.EfTimeSpan(
                sqlExpressionFactory.Function(
                    "abs",
                    [sqlExpressionFactory.EfTicks(instance)],
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[1],
                    typeof(long)),
                instance.TypeMapping);
        }

        if (method == NegateMethod)
        {
            return sqlExpressionFactory.EfTimeSpan(
                sqlExpressionFactory.Negate(sqlExpressionFactory.EfTicks(instance)),
                instance.TypeMapping);
        }

        return null;
    }
}
