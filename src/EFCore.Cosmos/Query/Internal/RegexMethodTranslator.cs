// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RegexMethodTranslator : IMethodCallTranslator
{
    private static readonly HashSet<MethodInfo> MethodInfoDataLengthMapping
        = new()
        {
            typeof(Regex).GetRuntimeMethod(nameof(Regex.IsMatch), new[] { typeof(string), typeof(string) })!,
            typeof(Regex).GetRuntimeMethod(nameof(Regex.IsMatch), new[] { typeof(ReadOnlySpan<char>), typeof(string) })!
        };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RegexMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (MethodInfoDataLengthMapping.Contains(method))
        {
            var typeMapping = arguments.Count == 2
                ? ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1])
                : ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1], arguments[2]);

            var newArguments = arguments.Select(e => _sqlExpressionFactory.ApplyTypeMapping(e, typeMapping));

            return _sqlExpressionFactory.Function(
                "RegexMatch",
                newArguments,
                method.ReturnType,
                typeMapping);
        }

        return null;
    }
}

