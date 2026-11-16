// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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

        var typeMapping = ExpressionExtensions.InferTypeMapping(instance, arguments[0], arguments[1]);
        instance = sqlExpressionFactory.ApplyTypeMapping(instance, typeMapping);
        var start = sqlExpressionFactory.ApplyTypeMapping(arguments[0], typeMapping);
        var end = sqlExpressionFactory.ApplyTypeMapping(arguments[1], typeMapping);
        var isAfterStart = sqlExpressionFactory.GreaterThanOrEqual(instance, start);
        var isBeforeEnd = sqlExpressionFactory.LessThan(instance, end);

        if (arguments is [SqlConstantExpression { Value: TimeOnly startValue }, SqlConstantExpression { Value: TimeOnly endValue }])
        {
            return startValue > endValue
                ? sqlExpressionFactory.OrElse(isAfterStart, isBeforeEnd)
                : sqlExpressionFactory.AndAlso(isAfterStart, isBeforeEnd);
        }

        return sqlExpressionFactory.OrElse(
            sqlExpressionFactory.AndAlso(
                sqlExpressionFactory.LessThanOrEqual(start, end),
                sqlExpressionFactory.AndAlso(isAfterStart, isBeforeEnd)),
            sqlExpressionFactory.AndAlso(
                sqlExpressionFactory.GreaterThan(start, end),
                sqlExpressionFactory.OrElse(isAfterStart, isBeforeEnd)));
    }
}
