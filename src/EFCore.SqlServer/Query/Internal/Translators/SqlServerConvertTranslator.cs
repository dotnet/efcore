// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerConvertTranslator : IMethodCallTranslator
{
    private static readonly Dictionary<string, string> TypeMapping = new()
    {
        [nameof(Convert.ToBoolean)] = "bit",
        [nameof(Convert.ToByte)] = "tinyint",
        [nameof(Convert.ToDecimal)] = "decimal(18, 2)",
        [nameof(Convert.ToDouble)] = "float",
        [nameof(Convert.ToInt16)] = "smallint",
        [nameof(Convert.ToInt32)] = "int",
        [nameof(Convert.ToInt64)] = "bigint",
        [nameof(Convert.ToString)] = "nvarchar(max)"
    };

    private static readonly List<Type> SupportedTypes =
    [
        typeof(bool),
        typeof(byte),
        typeof(DateTime),
        typeof(decimal),
        typeof(double),
        typeof(float),
        typeof(int),
        typeof(long),
        typeof(short),
        typeof(string),
        typeof(object)
    ];

    private static readonly MethodInfo[] SupportedMethods;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerConvertTranslator(ISqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    static SqlServerConvertTranslator()
    {
        var convertInfo = typeof(Convert).GetTypeInfo();
        SupportedMethods = TypeMapping.Keys
            .SelectMany(
                name => convertInfo.GetDeclaredMethods(name)
                    .Where(
                        method =>
                        {
                            var parameters = method.GetParameters();
                            return parameters.Length == 1
                                && SupportedTypes.Contains(parameters[0].ParameterType);
                        }))
            .ToArray();
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
                [_sqlExpressionFactory.Fragment(TypeMapping[method.Name]), arguments[0]],
                nullable: true,
                argumentsPropagateNullability: Statics.FalseTrue,
                method.ReturnType)
            : null;
}
