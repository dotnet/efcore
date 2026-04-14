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
public class CosmosMathTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
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
        if (method.DeclaringType != typeof(Math)
            && method.DeclaringType != typeof(MathF)
            && method.DeclaringType != typeof(double)
            && method.DeclaringType != typeof(float))
        {
            return null;
        }

        var sqlFunctionName = method.Name switch
        {
            nameof(Math.Abs) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(decimal) || t == typeof(double) || t == typeof(float)
                    || t == typeof(int) || t == typeof(long) || t == typeof(sbyte) || t == typeof(short))
                => "ABS",

            nameof(Math.Ceiling) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(decimal) || t == typeof(double) || t == typeof(float))
                => "CEILING",
            nameof(Math.Floor) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(decimal) || t == typeof(double) || t == typeof(float))
                => "FLOOR",
            nameof(Math.Round) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(decimal) || t == typeof(double) || t == typeof(float))
                => "ROUND",
            nameof(Math.Truncate) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(decimal) || t == typeof(double) || t == typeof(float))
                => "TRUNC",
            nameof(Math.Sign) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(decimal) || t == typeof(double) || t == typeof(float)
                    || t == typeof(int) || t == typeof(long) || t == typeof(sbyte) || t == typeof(short))
                => "SIGN",

            nameof(Math.Pow) when arguments is [_, _]
                => "POWER",
            nameof(Math.Exp) when arguments is [_]
                => "EXP",
            nameof(Math.Log10) when arguments is [_]
                => "LOG10",
            nameof(Math.Log) when arguments is [_] or [_, _]
                => "LOG",
            nameof(Math.Sqrt) when arguments is [_]
                => "SQRT",
            nameof(Math.Acos) when arguments is [_]
                => "ACOS",
            nameof(Math.Asin) when arguments is [_]
                => "ASIN",
            nameof(Math.Atan) when arguments is [_]
                => "ATAN",
            nameof(Math.Atan2) when arguments is [_, _]
                => "ATN2",
            nameof(Math.Cos) when arguments is [_]
                => "COS",
            nameof(Math.Sin) when arguments is [_]
                => "SIN",
            nameof(Math.Tan) when arguments is [_]
                => "TAN",
            nameof(double.DegreesToRadians) when arguments is [_]
                => "RADIANS",
            nameof(double.RadiansToDegrees) when arguments is [_]
                => "DEGREES",

            _ => null
        };

        if (sqlFunctionName is null)
        {
            return null;
        }

        var typeMapping = arguments.Count == 1
            ? ExpressionExtensions.InferTypeMapping(arguments[0])
            : ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);

        var newArguments = arguments.Select(e => sqlExpressionFactory.ApplyTypeMapping(e, typeMapping!));

        return sqlExpressionFactory.Function(
            sqlFunctionName,
            newArguments,
            method.ReturnType,
            typeMapping);
    }
}
