// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerDateTimeMemberTranslator : IMemberTranslator
{
    private static readonly Dictionary<string, string> DatePartMapping
        = new()
        {
            { nameof(DateTime.Year), "year" },
            { nameof(DateTime.Month), "month" },
            { nameof(DateTime.DayOfYear), "dayofyear" },
            { nameof(DateTime.Day), "day" },
            { nameof(DateTime.Hour), "hour" },
            { nameof(DateTime.Minute), "minute" },
            { nameof(DateTime.Second), "second" },
            { nameof(DateTime.Millisecond), "millisecond" }
        };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerDateTimeMemberTranslator(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _typeMappingSource = typeMappingSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        var declaringType = member.DeclaringType;

        if (declaringType == typeof(DateTime)
            || declaringType == typeof(DateTimeOffset))
        {
            var memberName = member.Name;

            if (DatePartMapping.TryGetValue(memberName, out var datePart))
            {
                return _sqlExpressionFactory.Function(
                    "DATEPART",
                    new[] { _sqlExpressionFactory.Fragment(datePart), instance! },
                    nullable: true,
                    argumentsPropagateNullability: new[] { false, true },
                    returnType);
            }

            switch (memberName)
            {
                case nameof(DateTime.Date):
                    return _sqlExpressionFactory.Function(
                        "CONVERT",
                        new[] { _sqlExpressionFactory.Fragment("date"), instance! },
                        nullable: true,
                        argumentsPropagateNullability: new[] { false, true },
                        returnType,
                        declaringType == typeof(DateTime)
                            ? instance!.TypeMapping
                            : _typeMappingSource.FindMapping(typeof(DateTime)));

                case nameof(DateTime.TimeOfDay):
                    return _sqlExpressionFactory.Function(
                        "CONVERT",
                        new[] { _sqlExpressionFactory.Fragment("time"), instance! },
                        nullable: true,
                        argumentsPropagateNullability: new[] { false, true },
                        returnType);

                case nameof(DateTime.Now):
                    return _sqlExpressionFactory.Function(
                        declaringType == typeof(DateTime) ? "GETDATE" : "SYSDATETIMEOFFSET",
                        Enumerable.Empty<SqlExpression>(),
                        nullable: false,
                        argumentsPropagateNullability: Enumerable.Empty<bool>(),
                        returnType);

                case nameof(DateTime.UtcNow):
                    var serverTranslation = _sqlExpressionFactory.Function(
                        declaringType == typeof(DateTime) ? "GETUTCDATE" : "SYSUTCDATETIME",
                        Enumerable.Empty<SqlExpression>(),
                        nullable: false,
                        argumentsPropagateNullability: Enumerable.Empty<bool>(),
                        returnType);

                    return declaringType == typeof(DateTime)
                        ? serverTranslation
                        : _sqlExpressionFactory.Convert(serverTranslation, returnType);

                case nameof(DateTime.Today):
                    return _sqlExpressionFactory.Function(
                        "CONVERT",
                        new SqlExpression[]
                        {
                            _sqlExpressionFactory.Fragment("date"),
                            _sqlExpressionFactory.Function(
                                "GETDATE",
                                Enumerable.Empty<SqlExpression>(),
                                nullable: false,
                                argumentsPropagateNullability: Enumerable.Empty<bool>(),
                                typeof(DateTime))
                        },
                        nullable: true,
                        argumentsPropagateNullability: new[] { false, true },
                        returnType);
            }
        }

        return null;
    }
}
