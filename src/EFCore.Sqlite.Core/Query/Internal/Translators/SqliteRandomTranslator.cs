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
public class SqliteRandomTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
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
        // Issue #15586: Query: TypeCompatibility chart for inference.
        => method.DeclaringType == typeof(DbFunctionsExtensions)
            && method.Name == nameof(DbFunctionsExtensions.Random)
                ? sqlExpressionFactory.Function(
                    "abs",
                    [
                        sqlExpressionFactory.Divide(
                            sqlExpressionFactory.Function(
                                "random",
                                [],
                                nullable: false,
                                argumentsPropagateNullability: [],
                                method.ReturnType),
                            sqlExpressionFactory.Constant(9223372036854780000.0))
                    ],
                    nullable: false,
                    argumentsPropagateNullability: Statics.TrueArrays[1],
                    method.ReturnType)
                : null;
}
