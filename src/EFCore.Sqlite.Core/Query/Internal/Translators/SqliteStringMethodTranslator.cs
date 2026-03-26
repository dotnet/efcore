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
public class SqliteStringMethodTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
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
                        => TranslateIndexOf(instance, arg),
                    nameof(string.IndexOf) when arguments is [var arg, var startIndex]
                        => TranslateIndexOfWithStartingPosition(instance, arg, startIndex),
                    nameof(string.Replace) when arguments is [var oldValue, var newValue]
                        => TranslateReplace(instance, oldValue, newValue),
                    nameof(string.ToLower) when arguments is []
                        => TranslateSimpleFunction("lower", instance),
                    nameof(string.ToUpper) when arguments is []
                        => TranslateSimpleFunction("upper", instance),
                    nameof(string.Substring) when arguments is [var startIndex]
                        => sqlExpressionFactory.Function(
                            "substr",
                            [instance, sqlExpressionFactory.Add(startIndex, sqlExpressionFactory.Constant(1))],
                            nullable: true,
                            argumentsPropagateNullability: Statics.TrueArrays[2],
                            method.ReturnType,
                            instance.TypeMapping),
                    nameof(string.Substring) when arguments is [var startIndex, var length]
                        => sqlExpressionFactory.Function(
                            "substr",
                            [instance, sqlExpressionFactory.Add(startIndex, sqlExpressionFactory.Constant(1)), length],
                            nullable: true,
                            argumentsPropagateNullability: Statics.TrueArrays[3],
                            method.ReturnType,
                            instance.TypeMapping),
                    nameof(string.TrimStart) when arguments is [] or [_]
                        => ProcessTrimMethod(instance, arguments, "ltrim"),
                    nameof(string.TrimEnd) when arguments is [] or [_]
                        => ProcessTrimMethod(instance, arguments, "rtrim"),
                    nameof(string.Trim) when arguments is [] or [_]
                        => ProcessTrimMethod(instance, arguments, "trim"),
                    nameof(string.Contains) when arguments is [var pattern]
                        => TranslateContains(instance, pattern),
                    _ => null
                };
            }

            // Static string methods
            return method.Name switch
            {
                nameof(string.IsNullOrWhiteSpace) when arguments is [var arg]
                    => sqlExpressionFactory.OrElse(
                        sqlExpressionFactory.IsNull(arg),
                        sqlExpressionFactory.Equal(
                            sqlExpressionFactory.Function(
                                "trim",
                                [arg],
                                nullable: true,
                                argumentsPropagateNullability: [true],
                                arg.Type,
                                arg.TypeMapping),
                            sqlExpressionFactory.Constant(string.Empty))),
                _ => null
            };
        }

        if (method.DeclaringType == typeof(Enumerable)
            && method.Name is nameof(Enumerable.FirstOrDefault) or nameof(Enumerable.LastOrDefault)
            && method.IsGenericMethod
            && method.GetGenericArguments()[0] == typeof(char)
            && arguments is [var source])
        {
            return method.Name == nameof(Enumerable.FirstOrDefault)
                ? sqlExpressionFactory.Function(
                    "substr",
                    [source, sqlExpressionFactory.Constant(1), sqlExpressionFactory.Constant(1)],
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[3],
                    method.ReturnType)
                : sqlExpressionFactory.Function(
                    "substr",
                    [
                        source,
                        sqlExpressionFactory.Function(
                            "length",
                            [source],
                            nullable: true,
                            argumentsPropagateNullability: Statics.TrueArrays[1],
                            typeof(int)),
                        sqlExpressionFactory.Constant(1)
                    ],
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[3],
                    method.ReturnType);
        }

        return null;
    }

    private SqlExpression TranslateIndexOf(SqlExpression instance, SqlExpression argument)
    {
        var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, argument);

        return sqlExpressionFactory.Subtract(
            sqlExpressionFactory.Function(
                "instr",
                [
                    sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping),
                    sqlExpressionFactory.ApplyTypeMapping(
                        argument, argument.Type == typeof(char) ? CharTypeMapping.Default : stringTypeMapping)
                ],
                nullable: true,
                argumentsPropagateNullability: Statics.TrueArrays[2],
                typeof(int)),
            sqlExpressionFactory.Constant(1));
    }

    private SqlExpression TranslateIndexOfWithStartingPosition(
        SqlExpression instance, SqlExpression argument, SqlExpression startIndex)
    {
        var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, argument);
        instance = sqlExpressionFactory.Function(
            "substr",
            [instance, sqlExpressionFactory.Add(startIndex, sqlExpressionFactory.Constant(1))],
            nullable: true,
            argumentsPropagateNullability: Statics.TrueArrays[2],
            typeof(string),
            instance.TypeMapping);

        return sqlExpressionFactory.Add(
            sqlExpressionFactory.Subtract(
                sqlExpressionFactory.Function(
                    "instr",
                    [
                        sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping),
                        sqlExpressionFactory.ApplyTypeMapping(
                            argument, argument.Type == typeof(char) ? CharTypeMapping.Default : stringTypeMapping)
                    ],
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[2],
                    typeof(int)),
                sqlExpressionFactory.Constant(1)),
            startIndex);
    }

    private SqlExpression TranslateReplace(SqlExpression instance, SqlExpression oldValue, SqlExpression newValue)
    {
        var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, oldValue, newValue);

        return sqlExpressionFactory.Function(
            "replace",
            [
                sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping),
                sqlExpressionFactory.ApplyTypeMapping(
                    oldValue, oldValue.Type == typeof(char) ? CharTypeMapping.Default : stringTypeMapping),
                sqlExpressionFactory.ApplyTypeMapping(
                    newValue, newValue.Type == typeof(char) ? CharTypeMapping.Default : stringTypeMapping)
            ],
            nullable: true,
            argumentsPropagateNullability: Statics.TrueArrays[3],
            typeof(string),
            stringTypeMapping);
    }

    private SqlExpression TranslateSimpleFunction(string functionName, SqlExpression instance)
        => sqlExpressionFactory.Function(
            functionName,
            [instance],
            nullable: true,
            argumentsPropagateNullability: Statics.TrueArrays[1],
            instance.Type,
            instance.TypeMapping);

    private SqlExpression TranslateContains(SqlExpression instance, SqlExpression pattern)
    {
        var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, pattern);

        instance = sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping);
        pattern = sqlExpressionFactory.ApplyTypeMapping(
            pattern, pattern.Type == typeof(char) ? CharTypeMapping.Default : stringTypeMapping);

        return sqlExpressionFactory.GreaterThan(
            sqlExpressionFactory.Function(
                "instr",
                [instance, pattern],
                nullable: true,
                argumentsPropagateNullability: Statics.TrueArrays[2],
                typeof(int)),
            sqlExpressionFactory.Constant(0));
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
                sqlArguments.Add(sqlExpressionFactory.Constant(new string(charactersToTrim.ToArray()), typeMapping));
            }
        }

        return sqlExpressionFactory.Function(
            functionName,
            sqlArguments,
            nullable: true,
            argumentsPropagateNullability: sqlArguments.Select(_ => true).ToList(),
            typeof(string),
            typeMapping);
    }
}
