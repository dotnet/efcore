// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteSubstrMethodTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo MethodInfo = typeof(SqliteDbFunctionsExtensions)
        .GetMethod(nameof(SqliteDbFunctionsExtensions.Substr), [typeof(DbFunctions), typeof(byte[]), typeof(int)])!;

    private static readonly MethodInfo MethodInfoWithLength = typeof(SqliteDbFunctionsExtensions)
        .GetMethod(
            nameof(SqliteDbFunctionsExtensions.Substr), [typeof(DbFunctions), typeof(byte[]), typeof(int), typeof(int)])!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteSubstrMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (method.Equals(MethodInfo)
            || method.Equals(MethodInfoWithLength))
        {
            return _sqlExpressionFactory.Function(
                "substr",
                arguments.Skip(1),
                nullable: true,
                arguments.Skip(1).Select(_ => true).ToArray(),
                typeof(byte[]),
                arguments[1].TypeMapping);
        }

        return null;
    }
}
