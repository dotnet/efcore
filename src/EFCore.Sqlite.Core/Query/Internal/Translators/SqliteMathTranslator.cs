// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteMathTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
{
    // Note: Math.Max/Min are handled in RelationalSqlTranslatingExpressionVisitor

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

        return method.Name switch
        {
            nameof(Math.Abs) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float) || t == typeof(int) || t == typeof(long) || t == typeof(sbyte) || t == typeof(short))
                => TranslateFunction("abs", arg),

            nameof(Math.Acos) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("acos", arg),
            nameof(Math.Acosh) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("acosh", arg),
            nameof(Math.Asin) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("asin", arg),
            nameof(Math.Asinh) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("asinh", arg),
            nameof(Math.Atan) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("atan", arg),
            nameof(Math.Atan2) when arguments is [var arg1, var arg2]
                => TranslateFunction("atan2", arg1, arg2),
            nameof(Math.Atanh) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("atanh", arg),
            nameof(Math.Ceiling) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("ceiling", arg),
            nameof(Math.Cos) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("cos", arg),
            nameof(Math.Cosh) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("cosh", arg),
            nameof(Math.Exp) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("exp", arg),
            nameof(Math.Floor) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("floor", arg),

            nameof(Math.Log) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("ln", arg),
            nameof(Math.Log) when arguments is [var a, var newBase]
                => TranslateLogWithBase(a, newBase),

            nameof(Math.Log2) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("log2", arg),
            nameof(Math.Log10) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("log10", arg),
            nameof(Math.Pow) when arguments is [var arg1, var arg2]
                => TranslateFunction("pow", arg1, arg2),

            nameof(Math.Round) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("round", arg),
            nameof(Math.Round) when arguments is [var arg, var digits]
                && digits.Type == typeof(int)
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateRoundWithDigits(arg, digits),

            nameof(Math.Sign) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float) || t == typeof(long) || t == typeof(sbyte) || t == typeof(short))
                => TranslateFunction("sign", arg),

            nameof(Math.Sin) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("sin", arg),
            nameof(Math.Sinh) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("sinh", arg),
            nameof(Math.Sqrt) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("sqrt", arg),
            nameof(Math.Tan) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("tan", arg),
            nameof(Math.Tanh) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("tanh", arg),
            nameof(Math.Truncate) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("trunc", arg),
            nameof(double.DegreesToRadians) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("radians", arg),
            nameof(double.RadiansToDegrees) when arguments is [var arg]
                && arg.Type is { } t && (t == typeof(double) || t == typeof(float))
                => TranslateFunction("degrees", arg),

            _ => null
        };
    }

    private SqlExpression TranslateFunction(string sqlFunctionName, SqlExpression argument)
    {
        var typeMapping = argument.TypeMapping;
        argument = sqlExpressionFactory.ApplyTypeMapping(argument, typeMapping);

        return sqlExpressionFactory.Function(
            sqlFunctionName,
            [argument],
            nullable: true,
            argumentsPropagateNullability: Statics.TrueArrays[1],
            argument.Type,
            typeMapping);
    }

    private SqlExpression TranslateFunction(string sqlFunctionName, SqlExpression arg1, SqlExpression arg2)
    {
        var typeMapping = ExpressionExtensions.InferTypeMapping(arg1, arg2);
        arg1 = sqlExpressionFactory.ApplyTypeMapping(arg1, typeMapping);
        arg2 = sqlExpressionFactory.ApplyTypeMapping(arg2, typeMapping);

        return sqlExpressionFactory.Function(
            sqlFunctionName,
            [arg1, arg2],
            nullable: true,
            argumentsPropagateNullability: Statics.TrueArrays[2],
            arg1.Type,
            typeMapping);
    }

    private SqlExpression TranslateRoundWithDigits(SqlExpression arg, SqlExpression digits)
        => sqlExpressionFactory.Function(
            "round",
            [arg, digits],
            nullable: true,
            argumentsPropagateNullability: Statics.TrueArrays[2],
            arg.Type,
            arg.TypeMapping);

    private SqlExpression TranslateLogWithBase(SqlExpression a, SqlExpression newBase)
    {
        var typeMapping = ExpressionExtensions.InferTypeMapping(a, newBase);

        return sqlExpressionFactory.Function(
            "log",
            [
                sqlExpressionFactory.ApplyTypeMapping(newBase, typeMapping),
                sqlExpressionFactory.ApplyTypeMapping(a, typeMapping)
            ],
            nullable: true,
            argumentsPropagateNullability: Statics.TrueArrays[2],
            a.Type,
            typeMapping);
    }
}
