// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteJsonFunctionsTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo JsonExistsMethodInfo = typeof(RelationalDbFunctionsExtensions)
        .GetRuntimeMethod(nameof(RelationalDbFunctionsExtensions.JsonExists), [typeof(DbFunctions), typeof(object), typeof(string)])!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteJsonFunctionsTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (JsonExistsMethodInfo.Equals(method)
            && (arguments[1].Type.Equals(typeof(string)) || (arguments[1].TypeMapping is SqliteJsonTypeMapping or StringTypeMapping)))
        {
            return _sqlExpressionFactory.Case(
                [new CaseWhenClause(
                    _sqlExpressionFactory.IsNotNull(arguments[1]),
                    _sqlExpressionFactory.IsNotNull(
                        _sqlExpressionFactory.Function("JSON_TYPE",
                            [arguments[1], arguments[2]],
                            nullable: true,
                            argumentsPropagateNullability: Statics.TrueArrays[2],
                            returnType: typeof(string))))
                ],
                null);
        }

        return null;
    }
}
