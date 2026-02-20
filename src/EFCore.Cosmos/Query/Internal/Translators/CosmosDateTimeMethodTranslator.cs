// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosDateTimeMethodTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType != typeof(DateTime) && method.DeclaringType != typeof(DateTimeOffset))
        {
            return null;
        }

        if (instance is null || arguments is not [var arg])
        {
            return null;
        }

        var datePart = method.Name switch
        {
            nameof(DateTime.AddYears) => "yyyy",
            nameof(DateTime.AddMonths) => "mm",
            nameof(DateTime.AddDays) => "dd",
            nameof(DateTime.AddHours) => "hh",
            nameof(DateTime.AddMinutes) => "mi",
            nameof(DateTime.AddSeconds) => "ss",
            nameof(DateTime.AddMilliseconds) => "ms",
            nameof(DateTime.AddMicroseconds) => "mcs",
            _ => (string?)null
        };

        return datePart is not null
            ? sqlExpressionFactory.Function(
                "DateTimeAdd",
                arguments: [sqlExpressionFactory.Constant(datePart), arg, instance],
                instance.Type,
                instance.TypeMapping)
            : null;
    }
}
