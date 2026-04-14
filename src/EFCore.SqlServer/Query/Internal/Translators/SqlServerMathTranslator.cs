// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerMathTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
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
        var declaringType = method.DeclaringType;

        if (declaringType != typeof(Math)
            && declaringType != typeof(MathF)
            && declaringType != typeof(double)
            && declaringType != typeof(float))
        {
            return null;
        }

        return method.Name switch
        {
            nameof(Math.Abs) when arguments is [var arg]
                && (arg.Type == typeof(decimal) || arg.Type == typeof(double) || arg.Type == typeof(float)
                    || arg.Type == typeof(int) || arg.Type == typeof(long) || arg.Type == typeof(sbyte) || arg.Type == typeof(short))
                => TranslateFunction("ABS", arg),
            nameof(Math.Ceiling) when arguments is [var arg]
                && (arg.Type == typeof(decimal) || arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("CEILING", arg),
            nameof(Math.Floor) when arguments is [var arg]
                && (arg.Type == typeof(decimal) || arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("FLOOR", arg),
            nameof(Math.Exp) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("EXP", arg),
            nameof(Math.Log10) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("LOG10", arg),
            nameof(Math.Log) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("LOG", arg),
            nameof(Math.Log) when arguments is [var arg1, var arg2]
                && (arg1.Type == typeof(double) || arg1.Type == typeof(float))
                => TranslateBinaryFunction("LOG", arg1, arg2),
            nameof(Math.Sqrt) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("SQRT", arg),
            nameof(Math.Acos) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("ACOS", arg),
            nameof(Math.Asin) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("ASIN", arg),
            nameof(Math.Atan) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("ATAN", arg),
            nameof(Math.Atan2) when arguments is [var arg1, var arg2]
                && (arg1.Type == typeof(double) || arg1.Type == typeof(float))
                => TranslateBinaryFunction("ATN2", arg1, arg2),
            nameof(Math.Cos) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("COS", arg),
            nameof(Math.Sin) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("SIN", arg),
            nameof(Math.Tan) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("TAN", arg),
            nameof(Math.Pow) when arguments is [var arg1, var arg2]
                && (arg1.Type == typeof(double) || arg1.Type == typeof(float))
                => TranslateBinaryFunction("POWER", arg1, arg2),
            nameof(Math.Sign) when arguments is [var arg]
                && (arg.Type == typeof(decimal) || arg.Type == typeof(double) || arg.Type == typeof(float)
                    || arg.Type == typeof(int) || arg.Type == typeof(long) || arg.Type == typeof(sbyte) || arg.Type == typeof(short))
                => TranslateFunction("SIGN", arg, nullTypeMapping: true),
            nameof(double.DegreesToRadians) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("RADIANS", arg),
            nameof(double.RadiansToDegrees) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("DEGREES", arg),

            nameof(Math.Truncate) when arguments is [var arg]
                && (arg.Type == typeof(decimal) || arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateTruncate(arg),
            nameof(Math.Round) when arguments is [var arg]
                && (arg.Type == typeof(decimal) || arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateRound(arg, digits: null),
            nameof(Math.Round) when arguments is [var arg, var digits]
                && digits.Type == typeof(int)
                && (arg.Type == typeof(decimal) || arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateRound(arg, digits),

            _ => null
        };

        SqlExpression TranslateFunction(string sqlFunctionName, SqlExpression arg, bool nullTypeMapping = false)
        {
            var typeMapping = ExpressionExtensions.InferTypeMapping(arg);
            return sqlExpressionFactory.Function(
                sqlFunctionName,
                [sqlExpressionFactory.ApplyTypeMapping(arg, typeMapping)],
                nullable: true,
                argumentsPropagateNullability: Statics.TrueArrays[1],
                method.ReturnType,
                nullTypeMapping ? null : typeMapping);
        }

        SqlExpression TranslateBinaryFunction(string sqlFunctionName, SqlExpression arg1, SqlExpression arg2)
        {
            var typeMapping = ExpressionExtensions.InferTypeMapping(arg1, arg2);
            return sqlExpressionFactory.Function(
                sqlFunctionName,
                [
                    sqlExpressionFactory.ApplyTypeMapping(arg1, typeMapping),
                    sqlExpressionFactory.ApplyTypeMapping(arg2, typeMapping)
                ],
                nullable: true,
                argumentsPropagateNullability: Statics.TrueArrays[2],
                method.ReturnType,
                typeMapping);
        }

        SqlExpression TranslateTruncate(SqlExpression argument)
        {
            // C# has Truncate over decimal/double/float only so our argument will be one of those types (compiler puts convert node)
            // In database result will be same type except for float which returns double which we need to cast back to float.
            var resultType = argument.Type;
            if (resultType == typeof(float))
            {
                resultType = typeof(double);
            }

            var result = sqlExpressionFactory.Function(
                "ROUND",
                [argument, sqlExpressionFactory.Constant(0), sqlExpressionFactory.Constant(1)],
                nullable: true,
                argumentsPropagateNullability: [true, false, false],
                resultType);

            if (argument.Type == typeof(float))
            {
                result = sqlExpressionFactory.Convert(result, typeof(float));
            }

            return sqlExpressionFactory.ApplyTypeMapping(result, argument.TypeMapping);
        }

        SqlExpression TranslateRound(SqlExpression argument, SqlExpression? digits)
        {
            digits ??= sqlExpressionFactory.Constant(0);

            // C# has Round over decimal/double/float only so our argument will be one of those types (compiler puts convert node)
            // In database result will be same type except for float which returns double which we need to cast back to float.
            var resultType = argument.Type;
            if (resultType == typeof(float))
            {
                resultType = typeof(double);
            }

            var result = sqlExpressionFactory.Function(
                "ROUND",
                [argument, digits],
                nullable: true,
                argumentsPropagateNullability: Statics.TrueArrays[2],
                resultType);

            if (argument.Type == typeof(float))
            {
                result = sqlExpressionFactory.Convert(result, typeof(float));
            }

            return sqlExpressionFactory.ApplyTypeMapping(result, argument.TypeMapping);
        }
    }
}
