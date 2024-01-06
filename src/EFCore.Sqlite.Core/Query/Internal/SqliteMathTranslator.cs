// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteMathTranslator : IMethodCallTranslator
{
    private static readonly Dictionary<MethodInfo, string> SupportedMethods = new()
    {
        { typeof(Math).GetMethod(nameof(Math.Abs), [typeof(double)])!, "abs" },
        { typeof(Math).GetMethod(nameof(Math.Abs), [typeof(float)])!, "abs" },
        { typeof(Math).GetMethod(nameof(Math.Abs), [typeof(int)])!, "abs" },
        { typeof(Math).GetMethod(nameof(Math.Abs), [typeof(long)])!, "abs" },
        { typeof(Math).GetMethod(nameof(Math.Abs), [typeof(sbyte)])!, "abs" },
        { typeof(Math).GetMethod(nameof(Math.Abs), [typeof(short)])!, "abs" },
        { typeof(Math).GetMethod(nameof(Math.Acos), [typeof(double)])!, "acos" },
        { typeof(Math).GetMethod(nameof(Math.Acosh), [typeof(double)])!, "acosh" },
        { typeof(Math).GetMethod(nameof(Math.Asin), [typeof(double)])!, "asin" },
        { typeof(Math).GetMethod(nameof(Math.Asinh), [typeof(double)])!, "asinh" },
        { typeof(Math).GetMethod(nameof(Math.Atan), [typeof(double)])!, "atan" },
        { typeof(Math).GetMethod(nameof(Math.Atan2), [typeof(double), typeof(double)])!, "atan2" },
        { typeof(Math).GetMethod(nameof(Math.Atanh), [typeof(double)])!, "atanh" },
        { typeof(Math).GetMethod(nameof(Math.Ceiling), [typeof(double)])!, "ceiling" },
        { typeof(Math).GetMethod(nameof(Math.Cos), [typeof(double)])!, "cos" },
        { typeof(Math).GetMethod(nameof(Math.Cosh), [typeof(double)])!, "cosh" },
        { typeof(Math).GetMethod(nameof(Math.Exp), [typeof(double)])!, "exp" },
        { typeof(Math).GetMethod(nameof(Math.Floor), [typeof(double)])!, "floor" },
        { typeof(Math).GetMethod(nameof(Math.Log), [typeof(double)])!, "ln" },
        { typeof(Math).GetMethod(nameof(Math.Log2), [typeof(double)])!, "log2" },
        { typeof(Math).GetMethod(nameof(Math.Log10), [typeof(double)])!, "log10" },
        { typeof(Math).GetMethod(nameof(Math.Pow), [typeof(double), typeof(double)])!, "pow" },
        { typeof(Math).GetMethod(nameof(Math.Round), [typeof(double)])!, "round" },
        { typeof(Math).GetMethod(nameof(Math.Sign), [typeof(double)])!, "sign" },
        { typeof(Math).GetMethod(nameof(Math.Sign), [typeof(float)])!, "sign" },
        { typeof(Math).GetMethod(nameof(Math.Sign), [typeof(long)])!, "sign" },
        { typeof(Math).GetMethod(nameof(Math.Sign), [typeof(sbyte)])!, "sign" },
        { typeof(Math).GetMethod(nameof(Math.Sign), [typeof(short)])!, "sign" },
        { typeof(Math).GetMethod(nameof(Math.Sin), [typeof(double)])!, "sin" },
        { typeof(Math).GetMethod(nameof(Math.Sinh), [typeof(double)])!, "sinh" },
        { typeof(Math).GetMethod(nameof(Math.Sqrt), [typeof(double)])!, "sqrt" },
        { typeof(Math).GetMethod(nameof(Math.Tan), [typeof(double)])!, "tan" },
        { typeof(Math).GetMethod(nameof(Math.Tanh), [typeof(double)])!, "tanh" },
        { typeof(Math).GetMethod(nameof(Math.Truncate), [typeof(double)])!, "trunc" },
        { typeof(double).GetRuntimeMethod(nameof(double.DegreesToRadians), [typeof(double)])!, "radians" },
        { typeof(double).GetRuntimeMethod(nameof(double.RadiansToDegrees), [typeof(double)])!, "degrees" },
        { typeof(MathF).GetMethod(nameof(MathF.Acos), [typeof(float)])!, "acos" },
        { typeof(MathF).GetMethod(nameof(MathF.Acosh), [typeof(float)])!, "acosh" },
        { typeof(MathF).GetMethod(nameof(MathF.Asin), [typeof(float)])!, "asin" },
        { typeof(MathF).GetMethod(nameof(MathF.Asinh), [typeof(float)])!, "asinh" },
        { typeof(MathF).GetMethod(nameof(MathF.Atan), [typeof(float)])!, "atan" },
        { typeof(MathF).GetMethod(nameof(MathF.Atan2), [typeof(float), typeof(float)])!, "atan2" },
        { typeof(MathF).GetMethod(nameof(MathF.Atanh), [typeof(float)])!, "atanh" },
        { typeof(MathF).GetMethod(nameof(MathF.Ceiling), [typeof(float)])!, "ceiling" },
        { typeof(MathF).GetMethod(nameof(MathF.Cos), [typeof(float)])!, "cos" },
        { typeof(MathF).GetMethod(nameof(MathF.Cosh), [typeof(float)])!, "cosh" },
        { typeof(MathF).GetMethod(nameof(MathF.Exp), [typeof(float)])!, "exp" },
        { typeof(MathF).GetMethod(nameof(MathF.Floor), [typeof(float)])!, "floor" },
        { typeof(MathF).GetMethod(nameof(MathF.Log), [typeof(float)])!, "ln" },
        { typeof(MathF).GetMethod(nameof(MathF.Log10), [typeof(float)])!, "log10" },
        { typeof(MathF).GetMethod(nameof(MathF.Log2), [typeof(float)])!, "log2" },
        { typeof(MathF).GetMethod(nameof(MathF.Pow), [typeof(float), typeof(float)])!, "pow" },
        { typeof(MathF).GetMethod(nameof(MathF.Round), [typeof(float)])!, "round" },
        { typeof(MathF).GetMethod(nameof(MathF.Sin), [typeof(float)])!, "sin" },
        { typeof(MathF).GetMethod(nameof(MathF.Sinh), [typeof(float)])!, "sinh" },
        { typeof(MathF).GetMethod(nameof(MathF.Sqrt), [typeof(float)])!, "sqrt" },
        { typeof(MathF).GetMethod(nameof(MathF.Tan), [typeof(float)])!, "tan" },
        { typeof(MathF).GetMethod(nameof(MathF.Tanh), [typeof(float)])!, "tanh" },
        { typeof(MathF).GetMethod(nameof(MathF.Truncate), [typeof(float)])!, "trunc" },
        { typeof(float).GetRuntimeMethod(nameof(float.DegreesToRadians), [typeof(float)])!, "radians" },
        { typeof(float).GetRuntimeMethod(nameof(float.RadiansToDegrees), [typeof(float)])!, "degrees" }
    };

    private static readonly List<MethodInfo> _roundWithDecimalMethods =
    [
        typeof(Math).GetMethod(nameof(Math.Round), [typeof(double), typeof(int)])!,
        typeof(MathF).GetMethod(nameof(MathF.Round), [typeof(float), typeof(int)])!
    ];

    private static readonly List<MethodInfo> _logWithBaseMethods =
    [
        typeof(Math).GetMethod(nameof(Math.Log), [typeof(double), typeof(double)])!,
        typeof(MathF).GetMethod(nameof(MathF.Log), [typeof(float), typeof(float)])!
    ];

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteMathTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (SupportedMethods.TryGetValue(method, out var sqlFunctionName))
        {
            var typeMapping = ExpressionExtensions.InferTypeMapping(arguments.ToArray());
            var newArguments = arguments
                .Select(a => _sqlExpressionFactory.ApplyTypeMapping(a, typeMapping))
                .ToList();

            return _sqlExpressionFactory.Function(
                sqlFunctionName,
                newArguments,
                nullable: true,
                argumentsPropagateNullability: newArguments.Select(_ => true).ToList(),
                method.ReturnType,
                typeMapping);
        }

        if (_roundWithDecimalMethods.Contains(method))
        {
            return _sqlExpressionFactory.Function(
                "round",
                arguments,
                nullable: true,
                argumentsPropagateNullability: new[] { true, true },
                method.ReturnType,
                arguments[0].TypeMapping);
        }

        if (_logWithBaseMethods.Contains(method))
        {
            var a = arguments[0];
            var newBase = arguments[1];
            var typeMapping = ExpressionExtensions.InferTypeMapping(a, newBase);

            return _sqlExpressionFactory.Function(
                "log",
                new[]
                {
                    _sqlExpressionFactory.ApplyTypeMapping(newBase, typeMapping), _sqlExpressionFactory.ApplyTypeMapping(a, typeMapping)
                },
                nullable: true,
                argumentsPropagateNullability: new[] { true, true },
                method.ReturnType,
                typeMapping);
        }

        if (method.DeclaringType == typeof(Math))
        {
            if (method.Name == nameof(Math.Min))
            {
                var success = _sqlExpressionFactory.TryCreateLeast(
                    new[] { arguments[0], arguments[1] }, method.ReturnType, out var leastExpression);
                Check.DebugAssert(success, "Couldn't generate min");
                return leastExpression;
            }

            if (method.Name == nameof(Math.Max))
            {
                var success = _sqlExpressionFactory.TryCreateGreatest(
                    new[] { arguments[0], arguments[1] }, method.ReturnType, out var leastExpression);
                Check.DebugAssert(success, "Couldn't generate max");
                return leastExpression;
            }
        }

        return null;
    }
}
