// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerFromPartsFunctionTranslator(
    ISqlExpressionFactory sqlExpressionFactory,
    IRelationalTypeMappingSource typeMappingSource)
    : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory = sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource = typeMappingSource;

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
        if (method.DeclaringType != typeof(SqlServerDbFunctionsExtensions))
        {
            return null;
        }

        var (functionName, returnType) = method.Name switch
        {
            nameof(SqlServerDbFunctionsExtensions.DateFromParts) => ("DATEFROMPARTS", "date"),
            nameof(SqlServerDbFunctionsExtensions.DateTimeFromParts) => ("DATETIMEFROMPARTS", "datetime"),
            nameof(SqlServerDbFunctionsExtensions.DateTime2FromParts) => ("DATETIME2FROMPARTS", "datetime2"),
            nameof(SqlServerDbFunctionsExtensions.DateTimeOffsetFromParts) => ("DATETIMEOFFSETFROMPARTS", "datetimeoffset"),
            nameof(SqlServerDbFunctionsExtensions.SmallDateTimeFromParts) => ("SMALLDATETIMEFROMPARTS", "smalldatetime"),
            nameof(SqlServerDbFunctionsExtensions.TimeFromParts) => ("TIMEFROMPARTS", "time"),
            _ => (null, null)
        };

        if (functionName is null)
        {
            return null;
        }

        return _sqlExpressionFactory.Function(
            functionName,
            arguments.Skip(1),
            nullable: true,
            argumentsPropagateNullability: arguments.Skip(1).Select(_ => true),
            method.ReturnType,
            _typeMappingSource.FindMapping(method.ReturnType, returnType));
    }
}
