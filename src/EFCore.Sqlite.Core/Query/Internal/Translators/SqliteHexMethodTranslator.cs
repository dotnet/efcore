// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteHexMethodTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
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
        if (method.DeclaringType != typeof(SqliteDbFunctionsExtensions))
        {
            return null;
        }

        return method.Name switch
        {
            nameof(SqliteDbFunctionsExtensions.Hex) when arguments is [_, var arg]
                => sqlExpressionFactory.Function(
                    "hex",
                    [arg],
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[1],
                    typeof(string)),

            // unhex returns NULL whenever the decoding fails, hence mark as
            // nullable and use an all-false argumentsPropagateNullability
            nameof(SqliteDbFunctionsExtensions.Unhex) when arguments is [_, var arg]
                => sqlExpressionFactory.Function(
                    "unhex",
                    [arg],
                    nullable: true,
                    argumentsPropagateNullability: Statics.FalseArrays[1],
                    typeof(byte[])),

            nameof(SqliteDbFunctionsExtensions.Unhex) when arguments is [_, var arg, var ignoreChars]
                => sqlExpressionFactory.Function(
                    "unhex",
                    [arg, ignoreChars],
                    nullable: true,
                    argumentsPropagateNullability: Statics.FalseArrays[2],
                    typeof(byte[])),

            _ => null
        };
    }
}
