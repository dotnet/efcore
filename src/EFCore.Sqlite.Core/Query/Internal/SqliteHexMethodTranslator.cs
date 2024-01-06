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
public class SqliteHexMethodTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo HexMethodInfo = typeof(SqliteDbFunctionsExtensions)
        .GetMethod(nameof(SqliteDbFunctionsExtensions.Hex), [typeof(DbFunctions), typeof(byte[])])!;

    private static readonly MethodInfo UnhexMethodInfo = typeof(SqliteDbFunctionsExtensions)
        .GetMethod(nameof(SqliteDbFunctionsExtensions.Unhex), [typeof(DbFunctions), typeof(string)])!;

    private static readonly MethodInfo UnhexWithIgnoreCharsMethodInfo = typeof(SqliteDbFunctionsExtensions)
        .GetMethod(nameof(SqliteDbFunctionsExtensions.Unhex), [typeof(DbFunctions), typeof(string), typeof(string)])!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteHexMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (method.Equals(HexMethodInfo))
        {
            return _sqlExpressionFactory.Function(
                "hex",
                new[] { arguments[1] },
                nullable: true,
                argumentsPropagateNullability: new[] { true },
                typeof(string));
        }

        if (method.Equals(UnhexMethodInfo)
            || method.Equals(UnhexWithIgnoreCharsMethodInfo))
        {
            return _sqlExpressionFactory.Function(
                "unhex",
                arguments.Skip(1),
                nullable: true,
                arguments.Skip(1).Select(_ => true).ToArray(),
                typeof(byte[]));
        }

        return null;
    }
}
