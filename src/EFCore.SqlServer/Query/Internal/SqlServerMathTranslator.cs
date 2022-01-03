// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerMathTranslator : IMethodCallTranslator
{
    private static readonly Dictionary<MethodInfo, string> SupportedMethodTranslations = new()
    {
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(decimal) })!, "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(double) })!, "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(float) })!, "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(int) })!, "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(long) })!, "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(sbyte) })!, "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(short) })!, "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(decimal) })!, "CEILING" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(double) })!, "CEILING" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(decimal) })!, "FLOOR" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(double) })!, "FLOOR" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Pow), new[] { typeof(double), typeof(double) })!, "POWER" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Exp), new[] { typeof(double) })!, "EXP" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Log10), new[] { typeof(double) })!, "LOG10" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double) })!, "LOG" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double), typeof(double) })!, "LOG" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sqrt), new[] { typeof(double) })!, "SQRT" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Acos), new[] { typeof(double) })!, "ACOS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Asin), new[] { typeof(double) })!, "ASIN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Atan), new[] { typeof(double) })!, "ATAN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Atan2), new[] { typeof(double), typeof(double) })!, "ATN2" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Cos), new[] { typeof(double) })!, "COS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sin), new[] { typeof(double) })!, "SIN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Tan), new[] { typeof(double) })!, "TAN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(decimal) })!, "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(double) })!, "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(float) })!, "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(int) })!, "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(long) })!, "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(sbyte) })!, "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(short) })!, "SIGN" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Abs), new[] { typeof(float) })!, "ABS" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Ceiling), new[] { typeof(float) })!, "CEILING" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Floor), new[] { typeof(float) })!, "FLOOR" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Pow), new[] { typeof(float), typeof(float) })!, "POWER" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Exp), new[] { typeof(float) })!, "EXP" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Log10), new[] { typeof(float) })!, "LOG10" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Log), new[] { typeof(float) })!, "LOG" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Log), new[] { typeof(float), typeof(float) })!, "LOG" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Sqrt), new[] { typeof(float) })!, "SQRT" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Acos), new[] { typeof(float) })!, "ACOS" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Asin), new[] { typeof(float) })!, "ASIN" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Atan), new[] { typeof(float) })!, "ATAN" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Atan2), new[] { typeof(float), typeof(float) })!, "ATN2" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Cos), new[] { typeof(float) })!, "COS" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Sin), new[] { typeof(float) })!, "SIN" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Tan), new[] { typeof(float) })!, "TAN" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Sign), new[] { typeof(float) })!, "SIGN" }
    };

    private static readonly IEnumerable<MethodInfo> TruncateMethodInfos = new[]
    {
        typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), new[] { typeof(decimal) })!,
        typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), new[] { typeof(double) })!,
        typeof(MathF).GetRuntimeMethod(nameof(MathF.Truncate), new[] { typeof(float) })!
    };

    private static readonly IEnumerable<MethodInfo> RoundMethodInfos = new[]
    {
        typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal) })!,
        typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(double) })!,
        typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal), typeof(int) })!,
        typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(double), typeof(int) })!,
        typeof(MathF).GetRuntimeMethod(nameof(MathF.Round), new[] { typeof(float) })!,
        typeof(MathF).GetRuntimeMethod(nameof(MathF.Round), new[] { typeof(float), typeof(int) })!
    };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerMathTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (SupportedMethodTranslations.TryGetValue(method, out var sqlFunctionName))
        {
            var typeMapping = arguments.Count == 1
                ? ExpressionExtensions.InferTypeMapping(arguments[0])
                : ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);

            var newArguments = new SqlExpression[arguments.Count];
            newArguments[0] = _sqlExpressionFactory.ApplyTypeMapping(arguments[0], typeMapping);

            if (arguments.Count == 2)
            {
                newArguments[1] = _sqlExpressionFactory.ApplyTypeMapping(arguments[1], typeMapping);
            }

            return _sqlExpressionFactory.Function(
                sqlFunctionName,
                newArguments,
                nullable: true,
                argumentsPropagateNullability: newArguments.Select(_ => true).ToArray(),
                method.ReturnType,
                sqlFunctionName == "SIGN" ? null : typeMapping);
        }

        if (TruncateMethodInfos.Contains(method))
        {
            var argument = arguments[0];
            // Result of ROUND for float/double is always double in server side
            var result = (SqlExpression)_sqlExpressionFactory.Function(
                "ROUND",
                new[] { argument, _sqlExpressionFactory.Constant(0), _sqlExpressionFactory.Constant(1) },
                nullable: true,
                argumentsPropagateNullability: new[] { true, false, false },
                typeof(double));

            if (argument.Type == typeof(float))
            {
                result = _sqlExpressionFactory.Convert(result, typeof(float));
            }

            return _sqlExpressionFactory.ApplyTypeMapping(result, argument.TypeMapping);
        }

        if (RoundMethodInfos.Contains(method))
        {
            var argument = arguments[0];
            var digits = arguments.Count == 2 ? arguments[1] : _sqlExpressionFactory.Constant(0);
            // Result of ROUND for float/double is always double in server side
            var result = (SqlExpression)_sqlExpressionFactory.Function(
                "ROUND",
                new[] { argument, digits },
                nullable: true,
                argumentsPropagateNullability: new[] { true, true },
                typeof(double));

            if (argument.Type == typeof(float))
            {
                result = _sqlExpressionFactory.Convert(result, typeof(float));
            }

            return _sqlExpressionFactory.ApplyTypeMapping(result, argument.TypeMapping);
        }

        return null;
    }
}
