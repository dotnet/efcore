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
public class SqlServerFromPartsFunctionTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo DateFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
        .GetRuntimeMethod(
            nameof(SqlServerDbFunctionsExtensions.DateFromParts),
            [typeof(DbFunctions), typeof(int), typeof(int), typeof(int)])!;

    private static readonly MethodInfo DateTimeFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
        .GetRuntimeMethod(
            nameof(SqlServerDbFunctionsExtensions.DateTimeFromParts),
            [typeof(DbFunctions), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int)])!;

    private static readonly MethodInfo DateTime2FromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
        .GetRuntimeMethod(
            nameof(SqlServerDbFunctionsExtensions.DateTime2FromParts),
            [
                typeof(DbFunctions),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int)
            ])!;

    private static readonly MethodInfo DateTimeOffsetFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
        .GetRuntimeMethod(
            nameof(SqlServerDbFunctionsExtensions.DateTimeOffsetFromParts),
            [
                typeof(DbFunctions),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int)
            ])!;

    private static readonly MethodInfo SmallDateTimeFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
        .GetRuntimeMethod(
            nameof(SqlServerDbFunctionsExtensions.SmallDateTimeFromParts),
            [typeof(DbFunctions), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int)])!;

    private static readonly MethodInfo TimeFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
        .GetRuntimeMethod(
            nameof(SqlServerDbFunctionsExtensions.TimeFromParts),
            [typeof(DbFunctions), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int)])!;

    private static readonly IDictionary<MethodInfo, (string FunctionName, string ReturnType)> MethodFunctionMapping
        = new Dictionary<MethodInfo, (string, string)>
        {
            { DateFromPartsMethodInfo, ("DATEFROMPARTS", "date") },
            { DateTimeFromPartsMethodInfo, ("DATETIMEFROMPARTS", "datetime") },
            { DateTime2FromPartsMethodInfo, ("DATETIME2FROMPARTS", "datetime2") },
            { DateTimeOffsetFromPartsMethodInfo, ("DATETIMEOFFSETFROMPARTS", "datetimeoffset") },
            { SmallDateTimeFromPartsMethodInfo, ("SMALLDATETIMEFROMPARTS", "smalldatetime") },
            { TimeFromPartsMethodInfo, ("TIMEFROMPARTS", "time") }
        };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerFromPartsFunctionTranslator(
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
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (MethodFunctionMapping.TryGetValue(method, out var value))
        {
            return _sqlExpressionFactory.Function(
                value.FunctionName,
                arguments.Skip(1),
                nullable: true,
                argumentsPropagateNullability: arguments.Skip(1).Select(_ => true),
                method.ReturnType,
                _typeMappingSource.FindMapping(method.ReturnType, value.ReturnType));
        }

        return null;
    }
}
