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
        { typeof(Math).GetMethod(nameof(Math.Round), new[] { typeof(double) })!, "round" },
        { typeof(Math).GetMethod(nameof(Math.Round), new[] { typeof(double), typeof(int) })!, "round" },
        { typeof(MathF).GetMethod(nameof(MathF.Abs), new[] { typeof(float) })!, "abs" },
        { typeof(MathF).GetMethod(nameof(MathF.Max), new[] { typeof(float), typeof(float) })!, "max" },
        { typeof(MathF).GetMethod(nameof(MathF.Min), new[] { typeof(float), typeof(float) })!, "min" },
        { typeof(MathF).GetMethod(nameof(MathF.Round), new[] { typeof(float) })!, "round" },
        { typeof(MathF).GetMethod(nameof(MathF.Round), new[] { typeof(float), typeof(int) })!, "round" }
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
            RelationalTypeMapping? typeMapping;
            List<SqlExpression>? newArguments = null;
            if (sqlFunctionName == "max" || sqlFunctionName == "max")
            {
                typeMapping = ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);
                newArguments = new List<SqlExpression>
                {
                    _sqlExpressionFactory.ApplyTypeMapping(arguments[0], typeMapping),
                    _sqlExpressionFactory.ApplyTypeMapping(arguments[1], typeMapping)
                };
            }
            else
            {
                typeMapping = arguments[0].TypeMapping;
            }

            var finalArguments = newArguments ?? arguments;

            return _sqlExpressionFactory.Function(
                sqlFunctionName,
                finalArguments,
                nullable: true,
                argumentsPropagateNullability: finalArguments.Select(_ => true).ToList(),
                method.ReturnType,
                typeMapping);
        }

        return null;
    }
}
