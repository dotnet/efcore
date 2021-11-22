// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class MathTranslator : IMethodCallTranslator
{
    private static readonly Dictionary<MethodInfo, string> _supportedMethodTranslations = new()
    {
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Abs), typeof(decimal)), "ABS" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Abs), typeof(double)), "ABS" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Abs), typeof(float)), "ABS" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Abs), typeof(int)), "ABS" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Abs), typeof(long)), "ABS" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Abs), typeof(sbyte)), "ABS" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Abs), typeof(short)), "ABS" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Ceiling), typeof(decimal)), "CEILING" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Ceiling), typeof(double)), "CEILING" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Floor), typeof(decimal)), "FLOOR" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Floor), typeof(double)), "FLOOR" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Pow), typeof(double), typeof(double)), "POWER" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Exp), typeof(double)), "EXP" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Log10), typeof(double)), "LOG10" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Log), typeof(double)), "LOG" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Log), typeof(double), typeof(double)), "LOG" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sqrt), typeof(double)), "SQRT" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Acos), typeof(double)), "ACOS" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Asin), typeof(double)), "ASIN" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Atan), typeof(double)), "ATAN" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Atan2), typeof(double), typeof(double)), "ATN2" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Cos), typeof(double)), "COS" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sin), typeof(double)), "SIN" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Tan), typeof(double)), "TAN" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sign), typeof(decimal)), "SIGN" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sign), typeof(double)), "SIGN" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sign), typeof(float)), "SIGN" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sign), typeof(int)), "SIGN" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sign), typeof(long)), "SIGN" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sign), typeof(sbyte)), "SIGN" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sign), typeof(short)), "SIGN" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Truncate), typeof(decimal)), "TRUNC" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Truncate), typeof(double)), "TRUNC" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Round), typeof(decimal)), "ROUND" },
        { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Round), typeof(double)), "ROUND" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Abs), typeof(float)), "ABS" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Ceiling), typeof(float)), "CEILING" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Floor), typeof(float)), "FLOOR" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Pow), typeof(float), typeof(float)), "POWER" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Exp), typeof(float)), "EXP" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Log10), typeof(float)), "LOG10" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Log), typeof(float)), "LOG" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Log), typeof(float), typeof(float)), "LOG" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Sqrt), typeof(float)), "SQRT" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Acos), typeof(float)), "ACOS" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Asin), typeof(float)), "ASIN" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Atan), typeof(float)), "ATAN" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Atan2), typeof(float), typeof(float)), "ATN2" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Cos), typeof(float)), "COS" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Sin), typeof(float)), "SIN" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Tan), typeof(float)), "TAN" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Sign), typeof(float)), "SIGN" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Truncate), typeof(float)), "TRUNC" },
        { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Round), typeof(float)), "ROUND" }
    };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public MathTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (_supportedMethodTranslations.TryGetValue(method, out var sqlFunctionName))
        {
            var typeMapping = arguments.Count == 1
                ? ExpressionExtensions.InferTypeMapping(arguments[0])
                : ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);

            var newArguments = arguments.Select(e => _sqlExpressionFactory.ApplyTypeMapping(e, typeMapping!));

            return _sqlExpressionFactory.Function(
                sqlFunctionName,
                newArguments,
                method.ReturnType,
                typeMapping);
        }

        return null;
    }
}
