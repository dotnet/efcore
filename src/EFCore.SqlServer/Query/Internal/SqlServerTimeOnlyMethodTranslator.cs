// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerTimeOnlyMethodTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo AddHoursMethod = typeof(TimeOnly).GetRuntimeMethod(
        nameof(TimeOnly.AddHours), [typeof(double)])!;

    private static readonly MethodInfo AddMinutesMethod = typeof(TimeOnly).GetRuntimeMethod(
        nameof(TimeOnly.AddMinutes), [typeof(double)])!;

    private static readonly MethodInfo IsBetweenMethod = typeof(TimeOnly).GetRuntimeMethod(
        nameof(TimeOnly.IsBetween), [typeof(TimeOnly), typeof(TimeOnly)])!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerTimeOnlyMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType != typeof(TimeOnly) || instance is null)
        {
            return null;
        }

        if (method == AddHoursMethod || method == AddMinutesMethod)
        {
            var datePart = method == AddHoursMethod ? "hour" : "minute";

            // Some Add methods accept a double, and SQL Server DateAdd does not accept number argument outside of int range
            if (arguments[0] is SqlConstantExpression { Value: double and (<= int.MinValue or >= int.MaxValue) })
            {
                return null;
            }

            instance = _sqlExpressionFactory.ApplyDefaultTypeMapping(instance);

            return _sqlExpressionFactory.Function(
                "DATEADD",
                new[] { _sqlExpressionFactory.Fragment(datePart), _sqlExpressionFactory.Convert(arguments[0], typeof(int)), instance },
                nullable: true,
                argumentsPropagateNullability: new[] { false, true, true },
                instance.Type,
                instance.TypeMapping);
        }

        // Translate TimeOnly.IsBetween to a >= b AND a < c.
        // Since a is evaluated multiple times, only translate for simple constructs (i.e. avoid duplicating complex subqueries).
        if (method == IsBetweenMethod
            && instance is ColumnExpression or SqlConstantExpression or SqlParameterExpression)
        {
            var typeMapping = ExpressionExtensions.InferTypeMapping(instance, arguments[0], arguments[1]);
            instance = _sqlExpressionFactory.ApplyTypeMapping(instance, typeMapping);

            return _sqlExpressionFactory.And(
                _sqlExpressionFactory.GreaterThanOrEqual(
                    instance,
                    _sqlExpressionFactory.ApplyTypeMapping(arguments[0], typeMapping)),
                _sqlExpressionFactory.LessThan(
                    instance,
                    _sqlExpressionFactory.ApplyTypeMapping(arguments[1], typeMapping)));
        }

        return null;
    }
}
