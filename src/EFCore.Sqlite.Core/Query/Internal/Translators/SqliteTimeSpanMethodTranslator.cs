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

    private static readonly MethodInfo FromDaysMethod
        = typeof(TimeSpan).GetRuntimeMethod(nameof(TimeSpan.FromDays), [typeof(double)])!;

    private static readonly MethodInfo FromHoursMethod
        = typeof(TimeSpan).GetRuntimeMethod(nameof(TimeSpan.FromHours), [typeof(double)])!;

    private static readonly MethodInfo FromMinutesMethod
        = typeof(TimeSpan).GetRuntimeMethod(nameof(TimeSpan.FromMinutes), [typeof(double)])!;

    private static readonly MethodInfo FromSecondsMethod
        = typeof(TimeSpan).GetRuntimeMethod(nameof(TimeSpan.FromSeconds), [typeof(double)])!;

    private static readonly MethodInfo FromMillisecondsMethod
        = typeof(TimeSpan).GetRuntimeMethod(nameof(TimeSpan.FromMilliseconds), [typeof(double)])!;

    private static readonly MethodInfo FromTicksMethod
        = typeof(TimeSpan).GetRuntimeMethod(nameof(TimeSpan.FromTicks), [typeof(long)])!;

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
        if (method.DeclaringType != typeof(TimeSpan))
        {
            return null;
        }

        if (DurationMethod.Equals(method) && instance != null)
        {
            var daysExpression = sqlExpressionFactory.EfDays(instance);
            var absExpression = sqlExpressionFactory.Function(
                "abs",
                [daysExpression],
                nullable: true,
                argumentsPropagateNullability: Statics.TrueArrays[1],
                typeof(double));
            return sqlExpressionFactory.EfTimespan(absExpression);
        }

        if (instance != null)
        {
            return null;
        }

        if (FromDaysMethod.Equals(method) && arguments.Count == 1)
        {
            return sqlExpressionFactory.EfTimespan(arguments[0]);
        }

        if (FromHoursMethod.Equals(method) && arguments.Count == 1)
        {
            return sqlExpressionFactory.EfTimespan(
                sqlExpressionFactory.Divide(arguments[0], sqlExpressionFactory.Constant(24.0)));
        }

        if (FromMinutesMethod.Equals(method) && arguments.Count == 1)
        {
            return sqlExpressionFactory.EfTimespan(
                sqlExpressionFactory.Divide(arguments[0], sqlExpressionFactory.Constant(1440.0)));
        }

        if (FromSecondsMethod.Equals(method) && arguments.Count == 1)
        {
            return sqlExpressionFactory.EfTimespan(
                sqlExpressionFactory.Divide(arguments[0], sqlExpressionFactory.Constant(86400.0)));
        }

        if (FromMillisecondsMethod.Equals(method) && arguments.Count == 1)
        {
            return sqlExpressionFactory.EfTimespan(
                sqlExpressionFactory.Divide(arguments[0], sqlExpressionFactory.Constant(86400000.0)));
        }

        if (FromTicksMethod.Equals(method) && arguments.Count == 1)
        {
            return sqlExpressionFactory.EfTimespan(
                sqlExpressionFactory.Divide(
                    sqlExpressionFactory.Convert(arguments[0], typeof(double)),
                    sqlExpressionFactory.Constant(864000000000.0)));
        }

        return null;
    }
}
