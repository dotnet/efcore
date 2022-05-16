// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerStringMethodTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo IndexOfMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), new[] { typeof(string) })!;

    private static readonly MethodInfo IndexOfMethodInfoWithStartingPosition
        = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), new[] { typeof(string), typeof(int) })!;

    private static readonly MethodInfo ReplaceMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.Replace), new[] { typeof(string), typeof(string) })!;

    private static readonly MethodInfo ToLowerMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.ToLower), Type.EmptyTypes)!;

    private static readonly MethodInfo ToUpperMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.ToUpper), Type.EmptyTypes)!;

    private static readonly MethodInfo SubstringMethodInfoWithOneArg
        = typeof(string).GetRuntimeMethod(nameof(string.Substring), new[] { typeof(int) })!;

    private static readonly MethodInfo SubstringMethodInfoWithTwoArgs
        = typeof(string).GetRuntimeMethod(nameof(string.Substring), new[] { typeof(int), typeof(int) })!;

    private static readonly MethodInfo IsNullOrEmptyMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.IsNullOrEmpty), new[] { typeof(string) })!;

    private static readonly MethodInfo IsNullOrWhiteSpaceMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.IsNullOrWhiteSpace), new[] { typeof(string) })!;

    // Method defined in netcoreapp2.0 only
    private static readonly MethodInfo TrimStartMethodInfoWithoutArgs
        = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), Type.EmptyTypes)!;

    private static readonly MethodInfo TrimEndMethodInfoWithoutArgs
        = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), Type.EmptyTypes)!;

    private static readonly MethodInfo TrimMethodInfoWithoutArgs
        = typeof(string).GetRuntimeMethod(nameof(string.Trim), Type.EmptyTypes)!;

    // Method defined in netstandard2.0
    private static readonly MethodInfo TrimStartMethodInfoWithCharArrayArg
        = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new[] { typeof(char[]) })!;

    private static readonly MethodInfo TrimEndMethodInfoWithCharArrayArg
        = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), new[] { typeof(char[]) })!;

    private static readonly MethodInfo TrimMethodInfoWithCharArrayArg
        = typeof(string).GetRuntimeMethod(nameof(string.Trim), new[] { typeof(char[]) })!;

    private static readonly MethodInfo StartsWithMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) })!;

    private static readonly MethodInfo ContainsMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) })!;

    private static readonly MethodInfo EndsWithMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) })!;

    private static readonly MethodInfo FirstOrDefaultMethodInfoWithoutArgs
        = typeof(Enumerable).GetRuntimeMethods().Single(
            m => m.Name == nameof(Enumerable.FirstOrDefault)
                && m.GetParameters().Length == 1).MakeGenericMethod(typeof(char));

    private static readonly MethodInfo LastOrDefaultMethodInfoWithoutArgs
        = typeof(Enumerable).GetRuntimeMethods().Single(
            m => m.Name == nameof(Enumerable.LastOrDefault)
                && m.GetParameters().Length == 1).MakeGenericMethod(typeof(char));

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    private const char LikeEscapeChar = '\\';
    private const string LikeEscapeString = "\\";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerStringMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (instance != null)
        {
            if (IndexOfMethodInfo.Equals(method))
            {
                return TranslateIndexOf(instance, method, arguments[0], null);
            }

            if (IndexOfMethodInfoWithStartingPosition.Equals(method))
            {
                return TranslateIndexOf(instance, method, arguments[0], arguments[1]);
            }

            if (ReplaceMethodInfo.Equals(method))
            {
                var firstArgument = arguments[0];
                var secondArgument = arguments[1];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, firstArgument, secondArgument);

                instance = _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping);
                firstArgument = _sqlExpressionFactory.ApplyTypeMapping(firstArgument, stringTypeMapping);
                secondArgument = _sqlExpressionFactory.ApplyTypeMapping(secondArgument, stringTypeMapping);

                return _sqlExpressionFactory.Function(
                    "REPLACE",
                    new[] { instance, firstArgument, secondArgument },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true, true },
                    method.ReturnType,
                    stringTypeMapping);
            }

            if (ToLowerMethodInfo.Equals(method)
                || ToUpperMethodInfo.Equals(method))
            {
                return _sqlExpressionFactory.Function(
                    ToLowerMethodInfo.Equals(method) ? "LOWER" : "UPPER",
                    new[] { instance },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    method.ReturnType,
                    instance.TypeMapping);
            }

            if (SubstringMethodInfoWithOneArg.Equals(method))
            {
                return _sqlExpressionFactory.Function(
                    "SUBSTRING",
                    new[]
                    {
                        instance,
                        _sqlExpressionFactory.Add(
                            arguments[0],
                            _sqlExpressionFactory.Constant(1)),
                        _sqlExpressionFactory.Function(
                            "LEN",
                            new[] { instance },
                            nullable: true,
                            argumentsPropagateNullability: new[] { true },
                            typeof(int))
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true, true },
                    method.ReturnType,
                    instance.TypeMapping);
            }

            if (SubstringMethodInfoWithTwoArgs.Equals(method))
            {
                return _sqlExpressionFactory.Function(
                    "SUBSTRING",
                    new[]
                    {
                        instance,
                        _sqlExpressionFactory.Add(
                            arguments[0],
                            _sqlExpressionFactory.Constant(1)),
                        arguments[1]
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true, true },
                    method.ReturnType,
                    instance.TypeMapping);
            }

            if (TrimStartMethodInfoWithoutArgs.Equals(method) == true
                || (TrimStartMethodInfoWithCharArrayArg.Equals(method)
                    // SqlServer LTRIM does not take arguments
                    && ((arguments[0] as SqlConstantExpression)?.Value as Array)?.Length == 0))
            {
                return _sqlExpressionFactory.Function(
                    "LTRIM",
                    new[] { instance },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    instance.Type,
                    instance.TypeMapping);
            }

            if (TrimEndMethodInfoWithoutArgs.Equals(method) == true
                || (TrimEndMethodInfoWithCharArrayArg.Equals(method)
                    // SqlServer RTRIM does not take arguments
                    && ((arguments[0] as SqlConstantExpression)?.Value as Array)?.Length == 0))
            {
                return _sqlExpressionFactory.Function(
                    "RTRIM",
                    new[] { instance },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    instance.Type,
                    instance.TypeMapping);
            }

            if (TrimMethodInfoWithoutArgs.Equals(method) == true
                || (TrimMethodInfoWithCharArrayArg.Equals(method)
                    // SqlServer LTRIM/RTRIM does not take arguments
                    && ((arguments[0] as SqlConstantExpression)?.Value as Array)?.Length == 0))
            {
                return _sqlExpressionFactory.Function(
                    "LTRIM",
                    new[]
                    {
                        _sqlExpressionFactory.Function(
                            "RTRIM",
                            new[] { instance },
                            nullable: true,
                            argumentsPropagateNullability: new[] { true },
                            instance.Type,
                            instance.TypeMapping)
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    instance.Type,
                    instance.TypeMapping);
            }

            if (ContainsMethodInfo.Equals(method))
            {
                var pattern = arguments[0];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, pattern);
                instance = _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping);
                pattern = _sqlExpressionFactory.ApplyTypeMapping(pattern, stringTypeMapping);

                if (pattern is SqlConstantExpression constantPattern)
                {
                    if (!(constantPattern.Value is string patternValue))
                    {
                        return _sqlExpressionFactory.Like(
                            instance,
                            _sqlExpressionFactory.Constant(null, stringTypeMapping));
                    }

                    if (patternValue.Length == 0)
                    {
                        return _sqlExpressionFactory.Constant(true);
                    }

                    return patternValue.Any(IsLikeWildChar)
                        ? _sqlExpressionFactory.Like(
                            instance,
                            _sqlExpressionFactory.Constant($"%{EscapeLikePattern(patternValue)}%"),
                            _sqlExpressionFactory.Constant(LikeEscapeString))
                        : _sqlExpressionFactory.Like(instance, _sqlExpressionFactory.Constant($"%{patternValue}%"));
                }

                return _sqlExpressionFactory.OrElse(
                    _sqlExpressionFactory.Like(
                        pattern,
                        _sqlExpressionFactory.Constant(string.Empty, stringTypeMapping)),
                    _sqlExpressionFactory.GreaterThan(
                        _sqlExpressionFactory.Function(
                            "CHARINDEX",
                            new[] { pattern, instance },
                            nullable: true,
                            argumentsPropagateNullability: new[] { true, true },
                            typeof(int)),
                        _sqlExpressionFactory.Constant(0)));
            }

            if (StartsWithMethodInfo.Equals(method))
            {
                return TranslateStartsEndsWith(instance, arguments[0], true);
            }

            if (EndsWithMethodInfo.Equals(method))
            {
                return TranslateStartsEndsWith(instance, arguments[0], false);
            }
        }

        if (IsNullOrEmptyMethodInfo.Equals(method))
        {
            var argument = arguments[0];

            return _sqlExpressionFactory.OrElse(
                _sqlExpressionFactory.IsNull(argument),
                _sqlExpressionFactory.Like(
                    argument,
                    _sqlExpressionFactory.Constant(string.Empty)));
        }

        if (IsNullOrWhiteSpaceMethodInfo.Equals(method))
        {
            var argument = arguments[0];

            return _sqlExpressionFactory.OrElse(
                _sqlExpressionFactory.IsNull(argument),
                _sqlExpressionFactory.Equal(
                    argument,
                    _sqlExpressionFactory.Constant(string.Empty, argument.TypeMapping)));
        }

        if (FirstOrDefaultMethodInfoWithoutArgs.Equals(method))
        {
            var argument = arguments[0];
            return _sqlExpressionFactory.Function(
                "SUBSTRING",
                new[] { argument, _sqlExpressionFactory.Constant(1), _sqlExpressionFactory.Constant(1) },
                nullable: true,
                argumentsPropagateNullability: new[] { true, true, true },
                method.ReturnType);
        }

        if (LastOrDefaultMethodInfoWithoutArgs.Equals(method))
        {
            var argument = arguments[0];
            return _sqlExpressionFactory.Function(
                "SUBSTRING",
                new[]
                {
                    argument,
                    _sqlExpressionFactory.Function(
                        "LEN",
                        new[] { argument },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        typeof(int)),
                    _sqlExpressionFactory.Constant(1)
                },
                nullable: true,
                argumentsPropagateNullability: new[] { true, true, true },
                method.ReturnType);
        }

        return null;
    }

    private SqlExpression TranslateStartsEndsWith(SqlExpression instance, SqlExpression pattern, bool startsWith)
    {
        var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, pattern);

        instance = _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping);
        pattern = _sqlExpressionFactory.ApplyTypeMapping(pattern, stringTypeMapping);

        if (pattern is SqlConstantExpression constantExpression)
        {
            // The pattern is constant. Aside from null or empty, we escape all special characters (%, _, \)
            // in C# and send a simple LIKE
            if (!(constantExpression.Value is string patternValue))
            {
                return _sqlExpressionFactory.Like(
                    instance,
                    _sqlExpressionFactory.Constant(null, stringTypeMapping));
            }

            return patternValue.Any(IsLikeWildChar)
                ? _sqlExpressionFactory.Like(
                    instance,
                    _sqlExpressionFactory.Constant(
                        startsWith
                            ? EscapeLikePattern(patternValue) + '%'
                            : '%' + EscapeLikePattern(patternValue)),
                    _sqlExpressionFactory.Constant(LikeEscapeString))
                : _sqlExpressionFactory.Like(
                    instance,
                    _sqlExpressionFactory.Constant(startsWith ? patternValue + '%' : '%' + patternValue));
        }

        // The pattern is non-constant, we use LEFT or RIGHT to extract substring and compare.
        if (startsWith)
        {
            return _sqlExpressionFactory.Equal(
                _sqlExpressionFactory.Function(
                    "LEFT",
                    new[]
                    {
                        instance,
                        _sqlExpressionFactory.Function(
                            "LEN",
                            new[] { pattern },
                            nullable: true,
                            argumentsPropagateNullability: new[] { true },
                            typeof(int))
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true },
                    typeof(string),
                    stringTypeMapping),
                pattern);
        }

        return _sqlExpressionFactory.Equal(
            _sqlExpressionFactory.Function(
                "RIGHT",
                new[]
                {
                    instance,
                    _sqlExpressionFactory.Function(
                        "LEN",
                        new[] { pattern },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        typeof(int))
                },
                nullable: true,
                argumentsPropagateNullability: new[] { true, true },
                typeof(string),
                stringTypeMapping),
            pattern);
    }

    private SqlExpression TranslateIndexOf(
        SqlExpression instance,
        MethodInfo method,
        SqlExpression searchExpression,
        SqlExpression? startIndex)
    {
        var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, searchExpression)!;
        searchExpression = _sqlExpressionFactory.ApplyTypeMapping(searchExpression, stringTypeMapping);
        instance = _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping);

        var charIndexArguments = new List<SqlExpression> { searchExpression, instance };

        if (startIndex is not null)
        {
            charIndexArguments.Add(
                startIndex is SqlConstantExpression { Value : int constantStartIndex }
                    ? _sqlExpressionFactory.Constant(constantStartIndex + 1, typeof(int))
                    : _sqlExpressionFactory.Add(startIndex, _sqlExpressionFactory.Constant(1)));
        }

        var argumentsPropagateNullability = Enumerable.Repeat(true, charIndexArguments.Count);

        SqlExpression charIndexExpression;
        var storeType = stringTypeMapping.StoreType;
        if (string.Equals(storeType, "nvarchar(max)", StringComparison.OrdinalIgnoreCase)
            || string.Equals(storeType, "varchar(max)", StringComparison.OrdinalIgnoreCase))
        {
            charIndexExpression = _sqlExpressionFactory.Function(
                "CHARINDEX",
                charIndexArguments,
                nullable: true,
                argumentsPropagateNullability,
                typeof(long));

            charIndexExpression = _sqlExpressionFactory.Convert(charIndexExpression, typeof(int));
        }
        else
        {
            charIndexExpression = _sqlExpressionFactory.Function(
                "CHARINDEX",
                charIndexArguments,
                nullable: true,
                argumentsPropagateNullability,
                method.ReturnType);
        }

        charIndexExpression = _sqlExpressionFactory.Subtract(charIndexExpression, _sqlExpressionFactory.Constant(1));

        // If the pattern is an empty string, we need to special case to always return 0 (since CHARINDEX return 0, which we'd subtract to
        // -1). Handle separately for constant and non-constant patterns.
        if (searchExpression is SqlConstantExpression { Value : string constantSearchPattern })
        {
            return constantSearchPattern == string.Empty
                ? _sqlExpressionFactory.Constant(0, typeof(int))
                : charIndexExpression;
        }

        return _sqlExpressionFactory.Case(
            new[]
            {
                new CaseWhenClause(
                    _sqlExpressionFactory.Equal(
                        searchExpression,
                        _sqlExpressionFactory.Constant(string.Empty, stringTypeMapping)),
                    _sqlExpressionFactory.Constant(0))
            },
            charIndexExpression);
    }

    // See https://docs.microsoft.com/en-us/sql/t-sql/language-elements/like-transact-sql
    private static bool IsLikeWildChar(char c)
        => c == '%' || c == '_' || c == '[';

    private static string EscapeLikePattern(string pattern)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < pattern.Length; i++)
        {
            var c = pattern[i];
            if (IsLikeWildChar(c)
                || c == LikeEscapeChar)
            {
                builder.Append(LikeEscapeChar);
            }

            builder.Append(c);
        }

        return builder.ToString();
    }
}
