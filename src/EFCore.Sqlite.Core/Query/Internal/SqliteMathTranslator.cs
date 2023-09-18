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
        { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(double) })!, "abs" },
        { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(float) })!, "abs" },
        { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(int) })!, "abs" },
        { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(long) })!, "abs" },
        { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(sbyte) })!, "abs" },
        { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(short) })!, "abs" },
        { typeof(Math).GetMethod(nameof(Math.Acos), new[] { typeof(double) })!, "acos" },
        { typeof(Math).GetMethod(nameof(Math.Acosh), new[] { typeof(double) })!, "acosh" },
        { typeof(Math).GetMethod(nameof(Math.Asin), new[] { typeof(double) })!, "asin" },
        { typeof(Math).GetMethod(nameof(Math.Asinh), new[] { typeof(double) })!, "asinh" },
        { typeof(Math).GetMethod(nameof(Math.Atan), new[] { typeof(double) })!, "atan" },
        { typeof(Math).GetMethod(nameof(Math.Atan2), new[] { typeof(double), typeof(double) })!, "atan2" },
        { typeof(Math).GetMethod(nameof(Math.Atanh), new[] { typeof(double) })!, "atanh" },
        { typeof(Math).GetMethod(nameof(Math.Ceiling), new[] { typeof(double) })!, "ceiling" },
        { typeof(Math).GetMethod(nameof(Math.Cos), new[] { typeof(double) })!, "cos" },
        { typeof(Math).GetMethod(nameof(Math.Cosh), new[] { typeof(double) })!, "cosh" },
        { typeof(Math).GetMethod(nameof(Math.Exp), new[] { typeof(double) })!, "exp" },
        { typeof(Math).GetMethod(nameof(Math.Floor), new[] { typeof(double) })!, "floor" },
        { typeof(Math).GetMethod(nameof(Math.Log), new[] { typeof(double) })!, "ln" },
        { typeof(Math).GetMethod(nameof(Math.Log2), new[] { typeof(double) })!, "log2" },
        { typeof(Math).GetMethod(nameof(Math.Log10), new[] { typeof(double) })!, "log10" },
        { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(byte), typeof(byte) })!, "max" },
        { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(double), typeof(double) })!, "max" },
        { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(float), typeof(float) })!, "max" },
        { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(int), typeof(int) })!, "max" },
        { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(long), typeof(long) })!, "max" },
        { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(sbyte), typeof(sbyte) })!, "max" },
        { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(short), typeof(short) })!, "max" },
        { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(uint), typeof(uint) })!, "max" },
        { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(ushort), typeof(ushort) })!, "max" },
        { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(byte), typeof(byte) })!, "min" },
        { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(double), typeof(double) })!, "min" },
        { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(float), typeof(float) })!, "min" },
        { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(int), typeof(int) })!, "min" },
        { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(long), typeof(long) })!, "min" },
        { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(sbyte), typeof(sbyte) })!, "min" },
        { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(short), typeof(short) })!, "min" },
        { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(uint), typeof(uint) })!, "min" },
        { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(ushort), typeof(ushort) })!, "min" },
        { typeof(Math).GetMethod(nameof(Math.Pow), new[] { typeof(double), typeof(double) })!, "pow" },
        { typeof(Math).GetMethod(nameof(Math.Round), new[] { typeof(double) })!, "round" },
        { typeof(Math).GetMethod(nameof(Math.Sign), new[] { typeof(double) })!, "sign" },
        { typeof(Math).GetMethod(nameof(Math.Sign), new[] { typeof(float) })!, "sign" },
        { typeof(Math).GetMethod(nameof(Math.Sign), new[] { typeof(long) })!, "sign" },
        { typeof(Math).GetMethod(nameof(Math.Sign), new[] { typeof(sbyte) })!, "sign" },
        { typeof(Math).GetMethod(nameof(Math.Sign), new[] { typeof(short) })!, "sign" },
        { typeof(Math).GetMethod(nameof(Math.Sin), new[] { typeof(double) })!, "sin" },
        { typeof(Math).GetMethod(nameof(Math.Sinh), new[] { typeof(double) })!, "sinh" },
        { typeof(Math).GetMethod(nameof(Math.Sqrt), new[] { typeof(double) })!, "sqrt" },
        { typeof(Math).GetMethod(nameof(Math.Tan), new[] { typeof(double) })!, "tan" },
        { typeof(Math).GetMethod(nameof(Math.Tanh), new[] { typeof(double) })!, "tanh" },
        { typeof(Math).GetMethod(nameof(Math.Truncate), new[] { typeof(double) })!, "trunc" },
        { typeof(double).GetRuntimeMethod(nameof(double.DegreesToRadians), new[] { typeof(double) })!, "radians" },
        { typeof(double).GetRuntimeMethod(nameof(double.RadiansToDegrees), new[] { typeof(double) })!, "degrees" },
        { typeof(MathF).GetMethod(nameof(MathF.Acos), new[] { typeof(float) })!, "acos" },
        { typeof(MathF).GetMethod(nameof(MathF.Acosh), new[] { typeof(float) })!, "acosh" },
        { typeof(MathF).GetMethod(nameof(MathF.Asin), new[] { typeof(float) })!, "asin" },
        { typeof(MathF).GetMethod(nameof(MathF.Asinh), new[] { typeof(float) })!, "asinh" },
        { typeof(MathF).GetMethod(nameof(MathF.Atan), new[] { typeof(float) })!, "atan" },
        { typeof(MathF).GetMethod(nameof(MathF.Atan2), new[] { typeof(float), typeof(float) })!, "atan2" },
        { typeof(MathF).GetMethod(nameof(MathF.Atanh), new[] { typeof(float) })!, "atanh" },
        { typeof(MathF).GetMethod(nameof(MathF.Ceiling), new[] { typeof(float) })!, "ceiling" },
        { typeof(MathF).GetMethod(nameof(MathF.Cos), new[] { typeof(float) })!, "cos" },
        { typeof(MathF).GetMethod(nameof(MathF.Cosh), new[] { typeof(float) })!, "cosh" },
        { typeof(MathF).GetMethod(nameof(MathF.Exp), new[] { typeof(float) })!, "exp" },
        { typeof(MathF).GetMethod(nameof(MathF.Floor), new[] { typeof(float) })!, "floor" },
        { typeof(MathF).GetMethod(nameof(MathF.Log), new[] { typeof(float) })!, "ln" },
        { typeof(MathF).GetMethod(nameof(MathF.Log10), new[] { typeof(float) })!, "log10" },
        { typeof(MathF).GetMethod(nameof(MathF.Log2), new[] { typeof(float) })!, "log2" },
        { typeof(MathF).GetMethod(nameof(MathF.Pow), new[] { typeof(float), typeof(float) })!, "pow" },
        { typeof(MathF).GetMethod(nameof(MathF.Round), new[] { typeof(float) })!, "round" },
        { typeof(MathF).GetMethod(nameof(MathF.Sin), new[] { typeof(float) })!, "sin" },
        { typeof(MathF).GetMethod(nameof(MathF.Sinh), new[] { typeof(float) })!, "sinh" },
        { typeof(MathF).GetMethod(nameof(MathF.Sqrt), new[] { typeof(float) })!, "sqrt" },
        { typeof(MathF).GetMethod(nameof(MathF.Tan), new[] { typeof(float) })!, "tan" },
        { typeof(MathF).GetMethod(nameof(MathF.Tanh), new[] { typeof(float) })!, "tanh" },
        { typeof(MathF).GetMethod(nameof(MathF.Truncate), new[] { typeof(float) })!, "trunc" },
        { typeof(float).GetRuntimeMethod(nameof(float.DegreesToRadians), new[] { typeof(float) })!, "radians" },
        { typeof(float).GetRuntimeMethod(nameof(float.RadiansToDegrees), new[] { typeof(float) })!, "degrees" }
    };

    private static readonly List<MethodInfo> _roundWithDecimalMethods = new()
    {
        typeof(Math).GetMethod(nameof(Math.Round), new[] { typeof(double), typeof(int) })!,
        typeof(MathF).GetMethod(nameof(MathF.Round), new[] { typeof(float), typeof(int) })!
    };

    private static readonly List<MethodInfo> _logWithBaseMethods = new()
    {
        typeof(Math).GetMethod(nameof(Math.Log), new[] { typeof(double), typeof(double) })!,
        typeof(MathF).GetMethod(nameof(MathF.Log), new[] { typeof(float), typeof(float) })!
    };

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

        return null;
    }
}
