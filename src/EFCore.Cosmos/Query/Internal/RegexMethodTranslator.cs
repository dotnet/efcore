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
    private static readonly MethodInfo IsMatch =
        typeof(Regex).GetRuntimeMethod(nameof(Regex.IsMatch), new[] { typeof(string), typeof(string) })!;

    private static readonly MethodInfo IsMatchWithRegexOptions =
        typeof(Regex).GetRuntimeMethod(nameof(Regex.IsMatch), new[] { typeof(string), typeof(string), typeof(RegexOptions) })!;

    private static readonly ISet<RegexOptions> AllowedOptions = new HashSet<RegexOptions>
    {
        RegexOptions.None,
        RegexOptions.IgnoreCase,
        RegexOptions.Multiline,
        RegexOptions.Singleline,
        RegexOptions.IgnorePatternWhitespace
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
        if (method != IsMatch && method != IsMatchWithRegexOptions)
        {
            return null;
        }

        RegexOptions options;
        if (method == IsMatch)
        {
            options = RegexOptions.None;
        }
        else if (arguments[2] is SqlConstantExpression { Value: RegexOptions regexOptions }
            && AllowedOptions.Contains(regexOptions))
        {
            options = regexOptions;
        }
        else
        {
            return null;
        }

        string modifier = options switch
        {
            RegexOptions.Multiline => "m",
            RegexOptions.Singleline => "s",
            RegexOptions.IgnoreCase => "i",
            RegexOptions.IgnorePatternWhitespace => "x",
            _ => ""
        };

        var (input, pattern) = (arguments[0], arguments[1]);
        var stringTypeMapping = ExpressionExtensions.InferTypeMapping(input, pattern);

        return _sqlExpressionFactory.Function(
            "RegexMatch",
            new[] { input, pattern, _sqlExpressionFactory.Constant(modifier) },
            method.ReturnType,
            stringTypeMapping);
    }
}
