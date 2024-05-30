// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosRegexTranslator(ISqlExpressionFactory sqlExpressionFactory)
    : IMethodCallTranslator
{
    private static readonly MethodInfo IsMatch =
        typeof(Regex).GetRuntimeMethod(nameof(Regex.IsMatch), [typeof(string), typeof(string)])!;

    private static readonly MethodInfo IsMatchWithRegexOptions =
        typeof(Regex).GetRuntimeMethod(nameof(Regex.IsMatch), [typeof(string), typeof(string), typeof(RegexOptions)])!;

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
        (input, pattern) = (
            sqlExpressionFactory.ApplyTypeMapping(input, typeMapping),
            sqlExpressionFactory.ApplyTypeMapping(pattern, typeMapping));

        if (method == IsMatch || arguments[2] is SqlConstantExpression { Value: RegexOptions.None })
        {
            return sqlExpressionFactory.Function("RegexMatch", new[] { input, pattern }, typeof(bool));
        }

        if (arguments[2] is SqlConstantExpression { Value: RegexOptions regexOptions })
        {
            var modifier = "";

            if (regexOptions.HasFlag(RegexOptions.Multiline))
            {
                regexOptions &= ~RegexOptions.Multiline;
                modifier += "m";
            }

            if (regexOptions.HasFlag(RegexOptions.Singleline))
            {
                regexOptions &= ~RegexOptions.Singleline;
                modifier += "s";
            }

            if (regexOptions.HasFlag(RegexOptions.IgnoreCase))
            {
                regexOptions &= ~RegexOptions.IgnoreCase;
                modifier += "i";
            }

            if (regexOptions.HasFlag(RegexOptions.IgnorePatternWhitespace))
            {
                regexOptions &= ~RegexOptions.IgnorePatternWhitespace;
                modifier += "x";
            }

            return regexOptions == 0
                ? sqlExpressionFactory.Function(
                    "RegexMatch",
                    new[] { input, pattern, sqlExpressionFactory.Constant(modifier) },
                    typeof(bool))
                : null; // TODO: Report unsupported RegexOption, #26410
        }

        return null;
    }
}
