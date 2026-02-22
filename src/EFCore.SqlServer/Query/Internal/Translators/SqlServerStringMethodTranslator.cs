// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using CharTypeMapping = Microsoft.EntityFrameworkCore.Storage.CharTypeMapping;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerStringMethodTranslator(
    ISqlExpressionFactory sqlExpressionFactory,
    ISqlServerSingletonOptions sqlServerSingletonOptions)
    : IMethodCallTranslator
{
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
        if (method.DeclaringType == typeof(string))
        {
            if (instance is not null)
            {
                return method.Name switch
                {
                    nameof(string.IndexOf) when arguments is [var search]
                        => TranslateIndexOf(instance, method, search, null),
                    nameof(string.IndexOf) when arguments is [var search, var startIndex] && startIndex.Type == typeof(int)
                        => TranslateIndexOf(instance, method, search, startIndex),

                    nameof(string.Replace) when arguments is [var oldValue, var newValue]
                        => TranslateReplace(instance, method, oldValue, newValue),

                    nameof(string.ToLower) when arguments is []
                        => sqlExpressionFactory.Function(
                            "LOWER", [instance], nullable: true,
                            argumentsPropagateNullability: Statics.TrueArrays[1],
                            method.ReturnType, instance.TypeMapping),
                    nameof(string.ToUpper) when arguments is []
                        => sqlExpressionFactory.Function(
                            "UPPER", [instance], nullable: true,
                            argumentsPropagateNullability: Statics.TrueArrays[1],
                            method.ReturnType, instance.TypeMapping),

                    nameof(string.Substring) when arguments is [var startIndex]
                        => TranslateSubstring(instance, method, startIndex, length: null),
                    nameof(string.Substring) when arguments is [var startIndex, var length]
                        => TranslateSubstring(instance, method, startIndex, length),

                    // There's single-parameter LTRIM/RTRIM for all versions (trims whitespace), but starting with SQL Server 2022 there's
                    // also an overload that accepts the characters to trim.
                    nameof(string.TrimStart) => TranslateTrimStartEnd(instance, arguments, "LTRIM"),
                    nameof(string.TrimEnd) => TranslateTrimStartEnd(instance, arguments, "RTRIM"),
                    nameof(string.Trim) when arguments is [] or [SqlConstantExpression { Value: char[] { Length: 0 } }]
                        => sqlExpressionFactory.Function(
                            "LTRIM",
                            [
                                sqlExpressionFactory.Function(
                                    "RTRIM", [instance], nullable: true,
                                    argumentsPropagateNullability: Statics.TrueArrays[1],
                                    instance.Type, instance.TypeMapping)
                            ],
                            nullable: true,
                            argumentsPropagateNullability: Statics.TrueArrays[1],
                            instance.Type,
                            instance.TypeMapping),

                    _ => null
                };
            }

            return method.Name switch
            {
                nameof(string.IsNullOrEmpty) when arguments is [var argument]
                    => sqlExpressionFactory.OrElse(
                        sqlExpressionFactory.IsNull(argument),
                        sqlExpressionFactory.Like(
                            argument,
                            sqlExpressionFactory.Constant(string.Empty))),

                nameof(string.IsNullOrWhiteSpace) when arguments is [var argument]
                    => sqlExpressionFactory.OrElse(
                        sqlExpressionFactory.IsNull(argument),
                        sqlExpressionFactory.Equal(
                            argument,
                            sqlExpressionFactory.Constant(string.Empty, argument.TypeMapping))),

                _ => null
            };
        }

        if (method.DeclaringType == typeof(Enumerable)
            && method.IsGenericMethod
            && arguments is [var source]
            && source.Type == typeof(string))
        {
            return method.Name switch
            {
                nameof(Enumerable.FirstOrDefault)
                    => sqlExpressionFactory.Function(
                        "SUBSTRING",
                        [source, sqlExpressionFactory.Constant(1), sqlExpressionFactory.Constant(1)],
                        nullable: true,
                        argumentsPropagateNullability: Statics.TrueArrays[3],
                        method.ReturnType),

                nameof(Enumerable.LastOrDefault)
                    => sqlExpressionFactory.Function(
                        "SUBSTRING",
                        [
                            source,
                            sqlExpressionFactory.Function(
                                "LEN", [source], nullable: true,
                                argumentsPropagateNullability: Statics.TrueArrays[1],
                                typeof(int)),
                            sqlExpressionFactory.Constant(1)
                        ],
                        nullable: true,
                        argumentsPropagateNullability: Statics.TrueArrays[3],
                        method.ReturnType),

                _ => null
            };
        }

        if (method.DeclaringType == typeof(SqlServerDbFunctionsExtensions)
            && method.Name == nameof(SqlServerDbFunctionsExtensions.PatIndex)
            && arguments is [_, var pattern, var expression])
        {
            return sqlExpressionFactory.Function(
                "PATINDEX",
                [pattern, expression],
                nullable: true,
                argumentsPropagateNullability: Statics.TrueArrays[2],
                method.ReturnType);
        }

        return null;
    }

    private SqlExpression TranslateReplace(
        SqlExpression instance,
        MethodInfo method,
        SqlExpression oldValue,
        SqlExpression newValue)
    {
        var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, oldValue, newValue);

        instance = sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping);
        oldValue = sqlExpressionFactory.ApplyTypeMapping(
            oldValue, oldValue.Type == typeof(char) ? CharTypeMapping.Default : stringTypeMapping);
        newValue = sqlExpressionFactory.ApplyTypeMapping(
            newValue, newValue.Type == typeof(char) ? CharTypeMapping.Default : stringTypeMapping);

        return sqlExpressionFactory.Function(
            "REPLACE",
            [instance, oldValue, newValue],
            nullable: true,
            argumentsPropagateNullability: Statics.TrueArrays[3],
            method.ReturnType,
            stringTypeMapping);
    }

    private SqlExpression TranslateSubstring(
        SqlExpression instance,
        MethodInfo method,
        SqlExpression startIndex,
        SqlExpression? length)
        => sqlExpressionFactory.Function(
            "SUBSTRING",
            [
                instance,
                sqlExpressionFactory.Add(startIndex, sqlExpressionFactory.Constant(1)),
                length ?? sqlExpressionFactory.Function(
                    "LEN", [instance], nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[1],
                    typeof(int))
            ],
            nullable: true,
            argumentsPropagateNullability: Statics.TrueArrays[3],
            method.ReturnType,
            instance.TypeMapping);

    private SqlExpression? TranslateTrimStartEnd(SqlExpression instance, IReadOnlyList<SqlExpression> arguments, string functionName)
        => arguments switch
        {
            // No args or empty char[] constant - whitespace trim, always supported
            ([]) or ([SqlConstantExpression { Value: char[] { Length: 0 } }])
                => ProcessTrimStartEnd(instance, arguments, functionName),

            // Char or char[] argument - requires SQL Server 2022+ (compatibility level 160)
            [_] when (sqlServerSingletonOptions.EngineType == SqlServerEngineType.SqlServer
                    && sqlServerSingletonOptions.SqlServerCompatibilityLevel >= 160)
                || (sqlServerSingletonOptions.EngineType == SqlServerEngineType.AzureSql
                    && sqlServerSingletonOptions.AzureSqlCompatibilityLevel >= 160)
                || sqlServerSingletonOptions.EngineType == SqlServerEngineType.AzureSynapse
                => ProcessTrimStartEnd(instance, arguments, functionName),

            _ => null
        };

    private SqlExpression TranslateIndexOf(
        SqlExpression instance,
        MethodInfo method,
        SqlExpression searchExpression,
        SqlExpression? startIndex)
    {
        var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, searchExpression)!;
        searchExpression = sqlExpressionFactory.ApplyTypeMapping(
            searchExpression, searchExpression.Type == typeof(char) ? CharTypeMapping.Default : stringTypeMapping);

        instance = sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping);

        var charIndexArguments = new List<SqlExpression> { searchExpression, instance };

        if (startIndex is not null)
        {
            charIndexArguments.Add(
                startIndex is SqlConstantExpression { Value : int constantStartIndex }
                    ? sqlExpressionFactory.Constant(constantStartIndex + 1, typeof(int))
                    : sqlExpressionFactory.Add(startIndex, sqlExpressionFactory.Constant(1)));
        }

        var argumentsPropagateNullability = Enumerable.Repeat(true, charIndexArguments.Count);

        SqlExpression charIndexExpression;
        var storeType = stringTypeMapping.StoreType;
        if (string.Equals(storeType, "nvarchar(max)", StringComparison.OrdinalIgnoreCase)
            || string.Equals(storeType, "varchar(max)", StringComparison.OrdinalIgnoreCase))
        {
            charIndexExpression = sqlExpressionFactory.Function(
                "CHARINDEX",
                charIndexArguments,
                nullable: true,
                argumentsPropagateNullability,
                typeof(long));

            charIndexExpression = sqlExpressionFactory.Convert(charIndexExpression, typeof(int));
        }
        else
        {
            charIndexExpression = sqlExpressionFactory.Function(
                "CHARINDEX",
                charIndexArguments,
                nullable: true,
                argumentsPropagateNullability,
                method.ReturnType);
        }

        // If the pattern is an empty string, we need to special case to always return 0 (since CHARINDEX return 0, which we'd subtract to
        // -1). Handle separately for constant and non-constant patterns.
        if (searchExpression is SqlConstantExpression { Value: "" })
        {
            return sqlExpressionFactory.Case(
                [new CaseWhenClause(sqlExpressionFactory.IsNotNull(instance), sqlExpressionFactory.Constant(0))],
                elseResult: null
            );
        }

        var offsetExpression = searchExpression is SqlConstantExpression
            ? sqlExpressionFactory.Constant(1)
            : sqlExpressionFactory.Case(
                [
                    new CaseWhenClause(
                        sqlExpressionFactory.Equal(
                            searchExpression,
                            sqlExpressionFactory.Constant(string.Empty, stringTypeMapping)),
                        sqlExpressionFactory.Constant(0))
                ],
                sqlExpressionFactory.Constant(1));

        return sqlExpressionFactory.Subtract(charIndexExpression, offsetExpression);
    }

    private SqlExpression? ProcessTrimStartEnd(SqlExpression instance, IReadOnlyList<SqlExpression> arguments, string functionName)
    {
        SqlExpression? charactersToTrim = null;
        if (arguments.Count > 0 && arguments[0] is SqlConstantExpression { Value: var charactersToTrimValue })
        {
            charactersToTrim = charactersToTrimValue switch
            {
                char singleChar => sqlExpressionFactory.Constant(singleChar.ToString(), instance.TypeMapping),
                char[] charArray => sqlExpressionFactory.Constant(new string(charArray), instance.TypeMapping),
                _ => throw new UnreachableException("Invalid parameter type for string.TrimStart/TrimEnd")
            };
        }

        return sqlExpressionFactory.Function(
            functionName,
            arguments: charactersToTrim is null ? [instance] : [instance, charactersToTrim],
            nullable: true,
            argumentsPropagateNullability: Statics.TrueArrays[charactersToTrim is null ? 1 : 2],
            instance.Type,
            instance.TypeMapping);
    }
}
