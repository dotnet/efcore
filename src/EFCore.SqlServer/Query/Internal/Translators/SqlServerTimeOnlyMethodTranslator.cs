// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerTimeOnlyMethodTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
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
        if (method.DeclaringType != typeof(TimeOnly))
        {
            return null;
        }

        if (instance is null)
        {
            return method.Name switch
            {
                nameof(TimeOnly.FromDateTime) or nameof(TimeOnly.FromTimeSpan) when arguments is [_]
                    => sqlExpressionFactory.Convert(arguments[0], typeof(TimeOnly)),
                _ => null
            };
        }

        var datePart = method.Name switch
        {
            nameof(TimeOnly.AddHours) => "hour",
            nameof(TimeOnly.AddMinutes) => "minute",
            _ => null
        };

        if (datePart is not null)
        {
            // Some Add methods accept a double, and SQL Server DateAdd does not accept number argument outside of int range
            if (arguments[0] is SqlConstantExpression { Value: double and (<= int.MinValue or >= int.MaxValue) })
            {
                return null;
            }

            instance = sqlExpressionFactory.ApplyDefaultTypeMapping(instance);

            return sqlExpressionFactory.Function(
                "DATEADD",
                [sqlExpressionFactory.Fragment(datePart), sqlExpressionFactory.Convert(arguments[0], typeof(int)), instance],
                nullable: true,
                argumentsPropagateNullability: [false, true, true],
                instance.Type,
                instance.TypeMapping);
        }

        // Translate TimeOnly.IsBetween to a >= b AND a < c.
        // Since a is evaluated multiple times, only translate for simple constructs (i.e. avoid duplicating complex subqueries).
        if (method.Name == nameof(TimeOnly.IsBetween)
            && instance is ColumnExpression or SqlConstantExpression or SqlParameterExpression)
        {
            var typeMapping = ExpressionExtensions.InferTypeMapping(instance, arguments[0], arguments[1]);
            instance = sqlExpressionFactory.ApplyTypeMapping(instance, typeMapping);

            var start = sqlExpressionFactory.ApplyTypeMapping(arguments[0], typeMapping);
            var end = sqlExpressionFactory.ApplyTypeMapping(arguments[1], typeMapping);

            var isAfterStart = sqlExpressionFactory.GreaterThanOrEqual(instance, start);
            var isBeforeEnd = sqlExpressionFactory.LessThan(instance, end);

            // A range whose start is after its end wraps around midnight, and matches the times that are either after
            // the start or before the end.
            if (arguments is [SqlConstantExpression { Value: TimeOnly startValue }, SqlConstantExpression { Value: TimeOnly endValue }])
            {
                return startValue > endValue
                    ? sqlExpressionFactory.OrElse(isAfterStart, isBeforeEnd)
                    : sqlExpressionFactory.AndAlso(isAfterStart, isBeforeEnd);
            }

            // The bounds aren't known when translating, so both cases have to be handled in the SQL.
            return sqlExpressionFactory.OrElse(
                sqlExpressionFactory.AndAlso(
                    sqlExpressionFactory.LessThanOrEqual(start, end),
                    sqlExpressionFactory.AndAlso(isAfterStart, isBeforeEnd)),
                sqlExpressionFactory.AndAlso(
                    sqlExpressionFactory.GreaterThan(start, end),
                    sqlExpressionFactory.OrElse(isAfterStart, isBeforeEnd)));
        }

        return null;
    }
}
