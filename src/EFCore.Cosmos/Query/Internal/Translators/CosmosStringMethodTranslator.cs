// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosStringMethodTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
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
                    nameof(string.IndexOf) when arguments is [var arg]
                        => TranslateSystemFunction("INDEX_OF", typeof(int), instance, arg),
                    nameof(string.IndexOf) when arguments is [var arg, var startIndex]
                        => TranslateSystemFunction("INDEX_OF", typeof(int), instance, arg, startIndex),
                    nameof(string.Replace) when arguments is [var oldValue, var newValue]
                        => TranslateSystemFunction("REPLACE", method.ReturnType, instance, oldValue, newValue),
                    nameof(string.Contains) when arguments is [var arg]
                        => TranslateSystemFunction("CONTAINS", typeof(bool), instance, arg),
                    nameof(string.Contains) when arguments is [var arg, SqlConstantExpression { Value: StringComparison comparisonType }]
                        => comparisonType switch
                        {
                            StringComparison.Ordinal
                                => TranslateSystemFunction("CONTAINS", typeof(bool), instance, arg, sqlExpressionFactory.Constant(false)),
                            StringComparison.OrdinalIgnoreCase
                                => TranslateSystemFunction("CONTAINS", typeof(bool), instance, arg, sqlExpressionFactory.Constant(true)),
                            _ => null
                        },
                    nameof(string.StartsWith) when arguments is [var arg] && arg.Type is { } t && (t == typeof(string) || t == typeof(char))
                        => TranslateSystemFunction("STARTSWITH", typeof(bool), instance, arg),
                    nameof(string.StartsWith) when arguments is [var arg, SqlConstantExpression { Value: StringComparison comparisonType }]
                        && arg.Type == typeof(string)
                        => comparisonType switch
                        {
                            StringComparison.Ordinal
                                => TranslateSystemFunction(
                                    "STARTSWITH", typeof(bool), instance, arg, sqlExpressionFactory.Constant(false)),
                            StringComparison.OrdinalIgnoreCase
                                => TranslateSystemFunction(
                                    "STARTSWITH", typeof(bool), instance, arg, sqlExpressionFactory.Constant(true)),
                            _ => null
                        },
                    nameof(string.EndsWith) when arguments is [var arg] && arg.Type is { } t && (t == typeof(string) || t == typeof(char))
                        => TranslateSystemFunction("ENDSWITH", typeof(bool), instance, arg),
                    nameof(string.EndsWith) when arguments is [var arg, SqlConstantExpression { Value: StringComparison comparisonType }]
                        && arg.Type == typeof(string)
                        => comparisonType switch
                        {
                            StringComparison.Ordinal
                                => TranslateSystemFunction(
                                    "ENDSWITH", typeof(bool), instance, arg, sqlExpressionFactory.Constant(false)),
                            StringComparison.OrdinalIgnoreCase
                                => TranslateSystemFunction(
                                    "ENDSWITH", typeof(bool), instance, arg, sqlExpressionFactory.Constant(true)),
                            _ => null
                        },
                    nameof(string.ToLower) when arguments is []
                        => TranslateSystemFunction("LOWER", method.ReturnType, instance),
                    nameof(string.ToUpper) when arguments is []
                        => TranslateSystemFunction("UPPER", method.ReturnType, instance),
                    nameof(string.TrimStart) when arguments is []
                        => TranslateSystemFunction("LTRIM", method.ReturnType, instance),
                    nameof(string.TrimStart) when arguments is [SqlConstantExpression { Value: char[] { Length: 0 } }]
                        // Cosmos DB LTRIM does not take arguments
                        => TranslateSystemFunction("LTRIM", method.ReturnType, instance),
                    nameof(string.TrimEnd) when arguments is []
                        => TranslateSystemFunction("RTRIM", method.ReturnType, instance),
                    nameof(string.TrimEnd) when arguments is [SqlConstantExpression { Value: char[] { Length: 0 } }]
                        // Cosmos DB RTRIM does not take arguments
                        => TranslateSystemFunction("RTRIM", method.ReturnType, instance),
                    nameof(string.Trim) when arguments is []
                        => TranslateSystemFunction("TRIM", method.ReturnType, instance),
                    nameof(string.Trim) when arguments is [SqlConstantExpression { Value: char[] { Length: 0 } }]
                        // Cosmos DB TRIM does not take arguments
                        => TranslateSystemFunction("TRIM", method.ReturnType, instance),
                    nameof(string.Substring) when arguments is [var startIndex]
                        => TranslateSystemFunction(
                            "SUBSTRING",
                            method.ReturnType,
                            instance,
                            startIndex,
                            TranslateSystemFunction("LENGTH", typeof(int), instance)),
                    nameof(string.Substring) when arguments is [SqlConstantExpression { Value: 0 }, var length]
                        => TranslateSystemFunction("LEFT", method.ReturnType, instance, length),
                    nameof(string.Substring) when arguments is [var startIndex, var length]
                        => TranslateSystemFunction("SUBSTRING", method.ReturnType, instance, startIndex, length),
                    nameof(string.Equals) when arguments is [var other, SqlConstantExpression
                        {
                            Value: StringComparison comparisonTypeValue
                                and (StringComparison.OrdinalIgnoreCase or StringComparison.Ordinal)
                        }]
                        => comparisonTypeValue == StringComparison.OrdinalIgnoreCase
                            ? TranslateSystemFunction(
                                "STRINGEQUALS", typeof(bool), instance, other, sqlExpressionFactory.Constant(true))
                            : TranslateSystemFunction("STRINGEQUALS", typeof(bool), instance, other),
                    _ => null
                };
            }

            // Static string methods
            return method.Name switch
            {
                nameof(string.Concat) when arguments is [var a, var b]
                    => sqlExpressionFactory.Add(a, b),
                nameof(string.Concat) when arguments is [var a, var b, var c]
                    => sqlExpressionFactory.Add(a, sqlExpressionFactory.Add(b, c)),
                nameof(string.Concat) when arguments is [var a, var b, var c, var d]
                    => sqlExpressionFactory.Add(a, sqlExpressionFactory.Add(b, sqlExpressionFactory.Add(c, d))),
                nameof(string.Equals) when arguments is [var left, var right, SqlConstantExpression
                    {
                        Value: StringComparison comparisonTypeValue
                            and (StringComparison.OrdinalIgnoreCase or StringComparison.Ordinal)
                    }]
                    => comparisonTypeValue == StringComparison.OrdinalIgnoreCase
                        ? TranslateSystemFunction(
                            "STRINGEQUALS", typeof(bool), left, right, sqlExpressionFactory.Constant(true))
                        : TranslateSystemFunction("STRINGEQUALS", typeof(bool), left, right),
                _ => null
            };
        }

        if (method.DeclaringType == typeof(Enumerable)
            && method.IsGenericMethod
            && method.GetGenericArguments()[0] == typeof(char)
            && arguments is [var source])
        {
            return method.Name switch
            {
                nameof(Enumerable.FirstOrDefault)
                    => TranslateSystemFunction("LEFT", typeof(char), source, sqlExpressionFactory.Constant(1)),
                nameof(Enumerable.LastOrDefault)
                    => TranslateSystemFunction("RIGHT", typeof(char), source, sqlExpressionFactory.Constant(1)),
                _ => null
            };
        }

        return null;
    }

    private SqlExpression TranslateSystemFunction(string function, Type returnType, params SqlExpression[] arguments)
        => sqlExpressionFactory.Function(function, arguments, returnType);
}
