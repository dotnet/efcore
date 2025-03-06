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
public class SqliteStringMethodTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo IndexOfMethodInfoString
        = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), [typeof(string)])!;

    private static readonly MethodInfo IndexOfMethodInfoChar
        = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), [typeof(char)])!;

    private static readonly MethodInfo IndexOfMethodInfoWithStartingPositionString
        = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), [typeof(string), typeof(int)])!;

    private static readonly MethodInfo IndexOfMethodInfoWithStartingPositionChar
        = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), [typeof(char), typeof(int)])!;

    private static readonly MethodInfo ReplaceMethodInfoString
        = typeof(string).GetRuntimeMethod(nameof(string.Replace), [typeof(string), typeof(string)])!;

    private static readonly MethodInfo ReplaceMethodInfoChar
        = typeof(string).GetRuntimeMethod(nameof(string.Replace), [typeof(char), typeof(char)])!;

    private static readonly MethodInfo ToLowerMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.ToLower), Type.EmptyTypes)!;

    private static readonly MethodInfo ToUpperMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.ToUpper), Type.EmptyTypes)!;

    private static readonly MethodInfo SubstringMethodInfoWithOneArg
        = typeof(string).GetRuntimeMethod(nameof(string.Substring), [typeof(int)])!;

    private static readonly MethodInfo SubstringMethodInfoWithTwoArgs
        = typeof(string).GetRuntimeMethod(nameof(string.Substring), [typeof(int), typeof(int)])!;

    private static readonly MethodInfo IsNullOrWhiteSpaceMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.IsNullOrWhiteSpace), [typeof(string)])!;

    // Method defined in netcoreapp2.0 only
    private static readonly MethodInfo TrimStartMethodInfoWithoutArgs
        = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), Type.EmptyTypes)!;

    private static readonly MethodInfo TrimStartMethodInfoWithCharArg
        = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), [typeof(char)])!;

    private static readonly MethodInfo TrimEndMethodInfoWithoutArgs
        = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), Type.EmptyTypes)!;

    private static readonly MethodInfo TrimEndMethodInfoWithCharArg
        = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), [typeof(char)])!;

    private static readonly MethodInfo TrimMethodInfoWithoutArgs
        = typeof(string).GetRuntimeMethod(nameof(string.Trim), Type.EmptyTypes)!;

    private static readonly MethodInfo TrimMethodInfoWithCharArg
        = typeof(string).GetRuntimeMethod(nameof(string.Trim), [typeof(char)])!;

    // Method defined in netstandard2.0
    private static readonly MethodInfo TrimStartMethodInfoWithCharArrayArg
        = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), [typeof(char[])])!;

    private static readonly MethodInfo TrimEndMethodInfoWithCharArrayArg
        = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), [typeof(char[])])!;

    private static readonly MethodInfo TrimMethodInfoWithCharArrayArg
        = typeof(string).GetRuntimeMethod(nameof(string.Trim), [typeof(char[])])!;

    private static readonly MethodInfo ContainsMethodInfoString
        = typeof(string).GetRuntimeMethod(nameof(string.Contains), [typeof(string)])!;
    private static readonly MethodInfo ContainsMethodInfoChar
        = typeof(string).GetRuntimeMethod(nameof(string.Contains), [typeof(char)])!;

    private static readonly MethodInfo FirstOrDefaultMethodInfoWithoutArgs
        = typeof(Enumerable).GetRuntimeMethods().Single(
            m => m.Name == nameof(Enumerable.FirstOrDefault)
                && m.GetParameters().Length == 1).MakeGenericMethod(typeof(char));

    private static readonly MethodInfo LastOrDefaultMethodInfoWithoutArgs
        = typeof(Enumerable).GetRuntimeMethods().Single(
            m => m.Name == nameof(Enumerable.LastOrDefault)
                && m.GetParameters().Length == 1).MakeGenericMethod(typeof(char));

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteStringMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

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
        if (instance != null)
        {
            if (IndexOfMethodInfoString.Equals(method) || IndexOfMethodInfoChar.Equals(method))
            {
                var argument = arguments[0];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, argument);

                return _sqlExpressionFactory.Subtract(
                    _sqlExpressionFactory.Function(
                        "instr",
                        new[]
                        {
                            _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping),
                            _sqlExpressionFactory.ApplyTypeMapping(argument, argument.Type == typeof(char) ? CharTypeMapping.Default : stringTypeMapping)
                        },
                        nullable: true,
                        argumentsPropagateNullability: Statics.TrueArrays[2],
                        method.ReturnType),
                    _sqlExpressionFactory.Constant(1));
            }

            if (IndexOfMethodInfoWithStartingPositionString.Equals(method) || IndexOfMethodInfoWithStartingPositionChar.Equals(method))
            {
                var argument = arguments[0];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, argument);
                instance = _sqlExpressionFactory.Function(
                    "substr",
                    new[] { instance, _sqlExpressionFactory.Add(arguments[1], _sqlExpressionFactory.Constant(1)) },
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[2],
                    method.ReturnType,
                    instance.TypeMapping);

                return _sqlExpressionFactory.Add(
                    _sqlExpressionFactory.Subtract(
                        _sqlExpressionFactory.Function(
                            "instr",
                            new[]
                            {
                                _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping),
                                _sqlExpressionFactory.ApplyTypeMapping(argument, argument.Type == typeof(char) ? CharTypeMapping.Default : stringTypeMapping)
                            },
                            nullable: true,
                            argumentsPropagateNullability: Statics.TrueArrays[2],
                            method.ReturnType),
                        _sqlExpressionFactory.Constant(1)),
                    arguments[1]);
            }

            if (ReplaceMethodInfoString.Equals(method) || ReplaceMethodInfoChar.Equals(method))
            {
                var firstArgument = arguments[0];
                var secondArgument = arguments[1];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, firstArgument, secondArgument);

                return _sqlExpressionFactory.Function(
                    "replace",
                    new[]
                    {
                        _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(firstArgument, firstArgument.Type == typeof(char) ? CharTypeMapping.Default : stringTypeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(secondArgument, secondArgument.Type == typeof(char) ? CharTypeMapping.Default : stringTypeMapping)
                    },
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[3],
                    method.ReturnType,
                    stringTypeMapping);
            }

            if (ToLowerMethodInfo.Equals(method)
                || ToUpperMethodInfo.Equals(method))
            {
                return _sqlExpressionFactory.Function(
                    ToLowerMethodInfo.Equals(method) ? "lower" : "upper",
                    new[] { instance },
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[1],
                    method.ReturnType,
                    instance.TypeMapping);
            }

            if (SubstringMethodInfoWithOneArg.Equals(method))
            {
                return _sqlExpressionFactory.Function(
                    "substr",
                    new[] { instance, _sqlExpressionFactory.Add(arguments[0], _sqlExpressionFactory.Constant(1)) },
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[2],
                    method.ReturnType,
                    instance.TypeMapping);
            }

            if (SubstringMethodInfoWithTwoArgs.Equals(method))
            {
                return _sqlExpressionFactory.Function(
                    "substr",
                    new[] { instance, _sqlExpressionFactory.Add(arguments[0], _sqlExpressionFactory.Constant(1)), arguments[1] },
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[3],
                    method.ReturnType,
                    instance.TypeMapping);
            }

            if (TrimStartMethodInfoWithoutArgs.Equals(method)
                || TrimStartMethodInfoWithCharArg.Equals(method)
                || TrimStartMethodInfoWithCharArrayArg.Equals(method))
            {
                return ProcessTrimMethod(instance, arguments, "ltrim");
            }

            if (TrimEndMethodInfoWithoutArgs.Equals(method)
                || TrimEndMethodInfoWithCharArg.Equals(method)
                || TrimEndMethodInfoWithCharArrayArg.Equals(method))
            {
                return ProcessTrimMethod(instance, arguments, "rtrim");
            }

            if (TrimMethodInfoWithoutArgs.Equals(method)
                || TrimMethodInfoWithCharArg.Equals(method)
                || TrimMethodInfoWithCharArrayArg.Equals(method))
            {
                return ProcessTrimMethod(instance, arguments, "trim");
            }

            if (ContainsMethodInfoString.Equals(method) || ContainsMethodInfoChar.Equals(method))
            {
                var pattern = arguments[0];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, pattern);

                instance = _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping);
                pattern = _sqlExpressionFactory.ApplyTypeMapping(pattern, pattern.Type == typeof(char) ? CharTypeMapping.Default : stringTypeMapping);

                return
                    _sqlExpressionFactory.GreaterThan(
                        _sqlExpressionFactory.Function(
                            "instr",
                            new[] { instance, pattern },
                            nullable: true,
                            argumentsPropagateNullability: Statics.TrueArrays[2],
                            typeof(int)),
                        _sqlExpressionFactory.Constant(0));
            }
        }

        if (IsNullOrWhiteSpaceMethodInfo.Equals(method))
        {
            var argument = arguments[0];

            return _sqlExpressionFactory.OrElse(
                _sqlExpressionFactory.IsNull(argument),
                _sqlExpressionFactory.Equal(
                    _sqlExpressionFactory.Function(
                        "trim",
                        new[] { argument },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        argument.Type,
                        argument.TypeMapping),
                    _sqlExpressionFactory.Constant(string.Empty)));
        }

        if (FirstOrDefaultMethodInfoWithoutArgs.Equals(method))
        {
            var argument = arguments[0];
            return _sqlExpressionFactory.Function(
                "substr",
                new[] { argument, _sqlExpressionFactory.Constant(1), _sqlExpressionFactory.Constant(1) },
                nullable: true,
                argumentsPropagateNullability: Statics.TrueArrays[3],
                method.ReturnType);
        }

        if (LastOrDefaultMethodInfoWithoutArgs.Equals(method))
        {
            var argument = arguments[0];
            return _sqlExpressionFactory.Function(
                "substr",
                new[]
                {
                    argument,
                    _sqlExpressionFactory.Function(
                        "length",
                        new[] { argument },
                        nullable: true,
                        argumentsPropagateNullability: Statics.TrueArrays[1],
                        typeof(int)),
                    _sqlExpressionFactory.Constant(1)
                },
                nullable: true,
                argumentsPropagateNullability: Statics.TrueArrays[3],
                method.ReturnType);
        }

        return null;
    }

    private SqlExpression? ProcessTrimMethod(SqlExpression instance, IReadOnlyList<SqlExpression> arguments, string functionName)
    {
        var typeMapping = instance.TypeMapping;
        if (typeMapping == null)
        {
            return null;
        }

        var sqlArguments = new List<SqlExpression> { instance };
        if (arguments.Count == 1)
        {
            var constantValue = (arguments[0] as SqlConstantExpression)?.Value;
            var charactersToTrim = new List<char>();

            if (constantValue is char singleChar)
            {
                charactersToTrim.Add(singleChar);
            }
            else if (constantValue is char[] charArray)
            {
                charactersToTrim.AddRange(charArray);
            }
            else
            {
                return null;
            }

            if (charactersToTrim.Count > 0)
            {
                sqlArguments.Add(_sqlExpressionFactory.Constant(new string(charactersToTrim.ToArray()), typeMapping));
            }
        }

        return _sqlExpressionFactory.Function(
            functionName,
            sqlArguments,
            nullable: true,
            argumentsPropagateNullability: sqlArguments.Select(_ => true).ToList(),
            typeof(string),
            typeMapping);
    }
}
