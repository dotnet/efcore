// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteTimeOnlyMethodTranslator(SqliteSqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
{
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
        // Each operand can be used more than once; avoid duplicating subqueries or volatile expressions.
        if (method.DeclaringType != typeof(TimeOnly)
            || method.Name != nameof(TimeOnly.IsBetween)
            || instance is not (ColumnExpression or SqlConstantExpression or SqlParameterExpression)
            || arguments is not [ColumnExpression or SqlConstantExpression or SqlParameterExpression,
                ColumnExpression or SqlConstantExpression or SqlParameterExpression])
        {
            return null;
        }

        var typeMapping = ExpressionExtensions.InferTypeMapping(instance, arguments[0], arguments[1])
            ?? sqlExpressionFactory.ApplyDefaultTypeMapping(instance)!.TypeMapping;
        instance = sqlExpressionFactory.ApplyTypeMapping(instance, typeMapping);
        var start = sqlExpressionFactory.ApplyTypeMapping(arguments[0], typeMapping);
        var end = sqlExpressionFactory.ApplyTypeMapping(arguments[1], typeMapping);
        var nativeText = IsNativeText(instance) && IsNativeText(start) && IsNativeText(end);
        if (!nativeText && (instance.TypeMapping != start.TypeMapping || instance.TypeMapping != end.TypeMapping))
        {
            // A custom converter may encode the same time differently, even with the same store type.
            return null;
        }

        var startKey = nativeText ? LowerBound(start) : start;
        var endKey = nativeText ? LowerBound(end) : end;
        var isAfterStart = sqlExpressionFactory.GreaterThanOrEqual(instance, startKey);
        var isBeforeEnd = sqlExpressionFactory.LessThan(instance, endKey);

        if (arguments is [SqlConstantExpression { Value: TimeOnly startValue }, SqlConstantExpression { Value: TimeOnly endValue }])
        {
            return startValue > endValue
                ? sqlExpressionFactory.OrElse(isAfterStart, isBeforeEnd)
                : sqlExpressionFactory.AndAlso(isAfterStart, isBeforeEnd);
        }

        // Constants and parameters use the provider's canonical format. A stored bound can have a shorter fraction.
        var compareKeys = nativeText && (start is ColumnExpression || end is ColumnExpression);
        return sqlExpressionFactory.OrElse(
            sqlExpressionFactory.AndAlso(
                compareKeys ? sqlExpressionFactory.GreaterThanOrEqual(end, startKey) : sqlExpressionFactory.LessThanOrEqual(start, end),
                sqlExpressionFactory.AndAlso(isAfterStart, isBeforeEnd)),
            sqlExpressionFactory.AndAlso(
                compareKeys ? sqlExpressionFactory.LessThan(end, startKey) : sqlExpressionFactory.GreaterThan(start, end),
                sqlExpressionFactory.OrElse(isAfterStart, isBeforeEnd)));
    }

    private static bool IsNativeText(SqlExpression time)
        => time.TypeMapping is SqliteTimeOnlyTypeMapping { Converter: null, DbType: System.Data.DbType.Time } mapping
            && mapping.StoreType.Equals(SqliteTypeMappingSource.TextTypeName, StringComparison.OrdinalIgnoreCase);

    private SqlExpression LowerBound(SqlExpression time)
    {
        if (time is SqlConstantExpression { Value: TimeOnly value }
            && (value.Ticks % TimeSpan.TicksPerSecond == 0 || value.Ticks % 10 != 0))
        {
            return time;
        }

        // With fixed-width HH:mm:ss, the trimmed prefix sorts after every earlier time and at or
        // before all representations of this time. Only the bound needs trimming for >= and <.
        return sqlExpressionFactory.Function(
            "rtrim", [time, sqlExpressionFactory.Constant("0.")],
            nullable: true, argumentsPropagateNullability: Statics.TrueFalse, typeof(TimeOnly), time.TypeMapping);
    }
}
