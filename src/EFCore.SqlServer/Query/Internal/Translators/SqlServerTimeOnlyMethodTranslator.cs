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

            return sqlExpressionFactory.And(
                sqlExpressionFactory.GreaterThanOrEqual(
                    instance,
                    sqlExpressionFactory.ApplyTypeMapping(arguments[0], typeMapping)),
                sqlExpressionFactory.LessThan(
                    instance,
                    sqlExpressionFactory.ApplyTypeMapping(arguments[1], typeMapping)));
        }

        return null;
    }
}
