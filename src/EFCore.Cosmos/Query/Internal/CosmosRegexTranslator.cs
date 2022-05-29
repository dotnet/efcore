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
public class CosmosRegexTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo IsMatch =
        typeof(Regex).GetRuntimeMethod(nameof(Regex.IsMatch), new[] { typeof(string), typeof(string) })!;

    private static readonly MethodInfo IsMatchWithRegexOptions =
        typeof(Regex).GetRuntimeMethod(nameof(Regex.IsMatch), new[] { typeof(string), typeof(string), typeof(RegexOptions) })!;

    private const RegexOptions SupportedOptions = RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosRegexTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (method != IsMatch && method != IsMatchWithRegexOptions)
        {
            return null;
        }

        var (input, pattern) = (arguments[0], arguments[1]);
        var typeMapping = ExpressionExtensions.InferTypeMapping(input, pattern);

        if (method == IsMatch)
        {
            return _sqlExpressionFactory.Function(
                "RegexMatch",
                new[] {
                    _sqlExpressionFactory.ApplyTypeMapping(input, typeMapping),
                    _sqlExpressionFactory.ApplyTypeMapping(pattern, typeMapping)
                },
                typeof(bool));
        }
        else if (arguments[2] is SqlConstantExpression { Value: RegexOptions regexOptions })
        {
            string modifier = "";
            if (regexOptions.HasFlag(RegexOptions.Multiline))
            {
                modifier += "m";
            }
            if (regexOptions.HasFlag(RegexOptions.Singleline))
            {
                modifier += "s";
            }
            if (regexOptions.HasFlag(RegexOptions.IgnoreCase))
            {
                modifier += "i";
            }
            if (regexOptions.HasFlag(RegexOptions.IgnorePatternWhitespace))
            {
                modifier += "x";
            }

            return (regexOptions & ~SupportedOptions) == 0
                ? _sqlExpressionFactory.Function(
                    "RegexMatch",
                     new[]
                     {
                        _sqlExpressionFactory.ApplyTypeMapping(input, typeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(pattern, typeMapping),
                        _sqlExpressionFactory.Constant(modifier)
                     },
                     typeof(bool))
                : null;
        }

        return null;
    }
}
