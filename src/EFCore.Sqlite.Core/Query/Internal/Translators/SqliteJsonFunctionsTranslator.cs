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
            && arguments[0].TypeMapping is SqliteJsonTypeMapping or StringTypeMapping)
        {
            // IIF(arguments_0 IS NULL, NULL, JSON_TYPE(arguments_0, arguments_1) IS NOT NULL)
            return _sqlExpressionFactory.Function("IFF",
                [
                    _sqlExpressionFactory.IsNull(arguments[0]),
                    _sqlExpressionFactory.Fragment("NULL", method.ReturnType),
                    _sqlExpressionFactory.IsNotNull(
                        _sqlExpressionFactory.Function("JSON_TYPE",
                            arguments,
                            nullable: true,
                            argumentsPropagateNullability: Statics.TrueArrays[2],
                            returnType: typeof(string)))
                ],
                nullable: true,
                argumentsPropagateNullability: Statics.TrueArrays[3],
                method.ReturnType);
        }

        return null;
    }
}
