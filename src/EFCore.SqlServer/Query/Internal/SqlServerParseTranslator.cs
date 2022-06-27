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
public class SqlServerParseTranslator : IMethodCallTranslator
{
    private static readonly Dictionary<Type, string> TypeMapping = new()
    {
        [typeof(bool)] = "bit",
        [typeof(byte)] = "tinyint",
        [typeof(decimal)] = "decimal(18, 2)",
        [typeof(double)] = "float",
        [typeof(float)] = "float",
        [typeof(short)] = "smallint",
        [typeof(int)] = "int",
        [typeof(long)] = "bigint"
    };

    private static readonly IEnumerable<MethodInfo> SupportedMethods
        = TypeMapping.Keys
            .SelectMany(
                t => t.GetTypeInfo().GetDeclaredMethods("Parse")
                    .Where(
                        m => m.GetParameters().Length == 1
                            && m.GetParameters().First().ParameterType == typeof(string)));

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerParseTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        => SupportedMethods.Contains(method)
            ? _sqlExpressionFactory.Function(
                "CONVERT",
                new[] { _sqlExpressionFactory.Fragment(TypeMapping[method.DeclaringType!]), arguments[0] },
                nullable: true,
                argumentsPropagateNullability: new[] { false, true },
                method.ReturnType)
            : null;
}
