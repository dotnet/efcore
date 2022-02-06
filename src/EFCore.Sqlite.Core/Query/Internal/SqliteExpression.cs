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
public static class SqliteExpression
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SqlFunctionExpression Strftime(
        ISqlExpressionFactory sqlExpressionFactory,
        Type returnType,
        string format,
        SqlExpression timestring,
        IEnumerable<SqlExpression>? modifiers = null,
        RelationalTypeMapping? typeMapping = null)
    {
        modifiers ??= Enumerable.Empty<SqlExpression>();

        // If the inner call is another strftime then shortcut a double call
        if (timestring is SqlFunctionExpression rtrimFunction
            && rtrimFunction.Name == "rtrim"
            && rtrimFunction.Arguments!.Count == 2
            && rtrimFunction.Arguments[0] is SqlFunctionExpression rtrimFunction2
            && rtrimFunction2.Name == "rtrim"
            && rtrimFunction2.Arguments!.Count == 2
            && rtrimFunction2.Arguments[0] is SqlFunctionExpression strftimeFunction
            && strftimeFunction.Name == "strftime"
            && strftimeFunction.Arguments!.Count > 1)
        {
            // Use its timestring parameter directly in place of ours
            timestring = strftimeFunction.Arguments[1];

            // Prepend its modifier arguments (if any) to the current call
            modifiers = strftimeFunction.Arguments.Skip(2).Concat(modifiers);
        }

        var finalArguments = new[] { sqlExpressionFactory.Constant(format), timestring }.Concat(modifiers);

        return sqlExpressionFactory.Function(
            "strftime",
            finalArguments,
            nullable: true,
            argumentsPropagateNullability: finalArguments.Select(_ => true),
            returnType,
            typeMapping);
    }
}
