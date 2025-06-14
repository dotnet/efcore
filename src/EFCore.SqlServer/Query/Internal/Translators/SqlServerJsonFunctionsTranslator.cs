// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerJsonFunctionsTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo JsonExistsMethodInfo = typeof(RelationalDbFunctionsExtensions)
        .GetRuntimeMethod(nameof(RelationalDbFunctionsExtensions.JsonExists), [typeof(DbFunctions), typeof(object), typeof(string)])!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly ISqlServerSingletonOptions _sqlServerSingletonOptions;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerJsonFunctionsTranslator(ISqlExpressionFactory sqlExpressionFactory, ISqlServerSingletonOptions sqlServerSingletonOptions)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _sqlServerSingletonOptions = sqlServerSingletonOptions;
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
            && (arguments[1].Type.Equals(typeof(string)) || arguments[1].TypeMapping is SqlServerOwnedJsonTypeMapping or StringTypeMapping)
            && _sqlServerSingletonOptions.EngineType == SqlServerEngineType.SqlServer
            && _sqlServerSingletonOptions.SqlServerCompatibilityLevel >= 160)
        {
            return _sqlExpressionFactory.Function(
                "JSON_PATH_EXISTS",
                [arguments[1], arguments[2]],
                nullable: true,
                argumentsPropagateNullability: Statics.TrueArrays[2],
                method.ReturnType);
        }

        return null;
    }
}
