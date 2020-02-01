// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerStringMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _indexOfMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), new[] { typeof(string) });

        private static readonly MethodInfo _replaceMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.Replace), new[] { typeof(string), typeof(string) });

        private static readonly MethodInfo _toLowerMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.ToLower), Array.Empty<Type>());

        private static readonly MethodInfo _toUpperMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.ToUpper), Array.Empty<Type>());

        private static readonly MethodInfo _substringMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.Substring), new[] { typeof(int), typeof(int) });

        private static readonly MethodInfo _isNullOrWhiteSpaceMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.IsNullOrWhiteSpace), new[] { typeof(string) });

        // Method defined in netcoreapp2.0 only
        private static readonly MethodInfo _trimStartMethodInfoWithoutArgs
            = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), Array.Empty<Type>());

        private static readonly MethodInfo _trimEndMethodInfoWithoutArgs
            = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), Array.Empty<Type>());

        private static readonly MethodInfo _trimMethodInfoWithoutArgs
            = typeof(string).GetRuntimeMethod(nameof(string.Trim), Array.Empty<Type>());

        // Method defined in netstandard2.0
        private static readonly MethodInfo _trimStartMethodInfoWithCharArrayArg
            = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new[] { typeof(char[]) });

        private static readonly MethodInfo _trimEndMethodInfoWithCharArrayArg
            = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), new[] { typeof(char[]) });

        private static readonly MethodInfo _trimMethodInfoWithCharArrayArg
            = typeof(string).GetRuntimeMethod(nameof(string.Trim), new[] { typeof(char[]) });

        private static readonly MethodInfo _startsWithMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });

        private static readonly MethodInfo _containsMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) });

        private static readonly MethodInfo _endsWithMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) });

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        private const char LikeEscapeChar = '\\';

        public SqlServerStringMethodTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

            if (_indexOfMethodInfo.Equals(method))
            {
                var argument = arguments[0];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, argument);
                argument = _sqlExpressionFactory.ApplyTypeMapping(argument, stringTypeMapping);

                SqlExpression charIndexExpression;
                var storeType = stringTypeMapping.StoreType;
                if (string.Equals(storeType, "nvarchar(max)", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(storeType, "varchar(max)", StringComparison.OrdinalIgnoreCase))
                {
                    charIndexExpression = _sqlExpressionFactory.Function(
                        "CHARINDEX",
                        new[] { argument, _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping) },
                        nullResultAllowed: true,
                        argumentsPropagateNullability: new [] { true, true },
                        typeof(long));

                    charIndexExpression = _sqlExpressionFactory.Convert(charIndexExpression, typeof(int));
                }
                else
                {
                    charIndexExpression = _sqlExpressionFactory.Function(
                        "CHARINDEX",
                        new[] { argument, _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping) },
                        nullResultAllowed: true,
                        argumentsPropagateNullability: new[] { true, true },
                        method.ReturnType);
                }

                charIndexExpression = _sqlExpressionFactory.Subtract(charIndexExpression, _sqlExpressionFactory.Constant(1));

                return _sqlExpressionFactory.Case(
                    new[]
                    {
                        new CaseWhenClause(
                            _sqlExpressionFactory.Equal(
                                argument,
                                _sqlExpressionFactory.Constant(string.Empty, stringTypeMapping)),
                            _sqlExpressionFactory.Constant(0))
                    },
                    charIndexExpression);
            }

            if (_replaceMethodInfo.Equals(method))
            {
                var firstArgument = arguments[0];
                var secondArgument = arguments[1];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, firstArgument, secondArgument);

                instance = _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping);
                firstArgument = _sqlExpressionFactory.ApplyTypeMapping(firstArgument, stringTypeMapping);
                secondArgument = _sqlExpressionFactory.ApplyTypeMapping(secondArgument, stringTypeMapping);

                return _sqlExpressionFactory.Function(
                    "REPLACE",
                    new[] { instance, firstArgument, secondArgument },
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new[] { true, true, true },
                    method.ReturnType,
                    stringTypeMapping);
            }

            if (_toLowerMethodInfo.Equals(method)
                || _toUpperMethodInfo.Equals(method))
            {
                return _sqlExpressionFactory.Function(
                    _toLowerMethodInfo.Equals(method) ? "LOWER" : "UPPER",
                    new[] { instance },
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new[] { true },
                    method.ReturnType,
                    instance.TypeMapping);
            }

            if (_substringMethodInfo.Equals(method))
            {
                return _sqlExpressionFactory.Function(
                    "SUBSTRING",
                    new[]
                    {
                        instance,
                        _sqlExpressionFactory.Add(
                            arguments[0],
                            _sqlExpressionFactory.Constant(1)),
                        arguments[1]
                    },
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new[] { true, true, true },
                    method.ReturnType,
                    instance.TypeMapping);
            }

            if (_isNullOrWhiteSpaceMethodInfo.Equals(method))
            {
                var argument = arguments[0];

                return _sqlExpressionFactory.OrElse(
                    _sqlExpressionFactory.IsNull(argument),
                    _sqlExpressionFactory.Equal(
                        _sqlExpressionFactory.Function(
                            "LTRIM",
                            new[]
                            {
                                _sqlExpressionFactory.Function(
                                    "RTRIM",
                                    new[] { argument },
                                    nullResultAllowed: true,
                                    argumentsPropagateNullability: new[] { true },
                                    argument.Type,
                                    argument.TypeMapping)
                            },
                            nullResultAllowed: true,
                            argumentsPropagateNullability: new[] { true },
                            argument.Type,
                            argument.TypeMapping),
                        _sqlExpressionFactory.Constant(string.Empty, argument.TypeMapping)));
            }

            if (_trimStartMethodInfoWithoutArgs?.Equals(method) == true
                || (_trimStartMethodInfoWithCharArrayArg.Equals(method)
                    // SqlServer LTRIM does not take arguments
                    && ((arguments[0] as SqlConstantExpression)?.Value as Array)?.Length == 0))
            {
                return _sqlExpressionFactory.Function(
                    "LTRIM",
                    new[] { instance },
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new[] { true },
                    instance.Type,
                    instance.TypeMapping);
            }

            if (_trimEndMethodInfoWithoutArgs?.Equals(method) == true
                || (_trimEndMethodInfoWithCharArrayArg.Equals(method)
                    // SqlServer RTRIM does not take arguments
                    && ((arguments[0] as SqlConstantExpression)?.Value as Array)?.Length == 0))
            {
                return _sqlExpressionFactory.Function(
                    "RTRIM",
                    new[] { instance },
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new[] { true },
                    instance.Type,
                    instance.TypeMapping);
            }

            if (_trimMethodInfoWithoutArgs?.Equals(method) == true
                || (_trimMethodInfoWithCharArrayArg.Equals(method)
                    // SqlServer LTRIM/RTRIM does not take arguments
                    && ((arguments[0] as SqlConstantExpression)?.Value as Array)?.Length == 0))
            {
                return _sqlExpressionFactory.Function(
                    "LTRIM",
                    new[]
                    {
                        _sqlExpressionFactory.Function(
                            "RTRIM",
                            new[] { instance },
                            nullResultAllowed: true,
                            argumentsPropagateNullability: new[] { true },
                            instance.Type,
                            instance.TypeMapping)
                    },
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new[] { true },
                    instance.Type,
                    instance.TypeMapping);
            }

            if (_containsMethodInfo.Equals(method))
            {
                var pattern = arguments[0];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, pattern);
                instance = _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping);
                pattern = _sqlExpressionFactory.ApplyTypeMapping(pattern, stringTypeMapping);

                if (pattern is SqlConstantExpression constantPattern)
                {
                     // Intentionally string.Empty since we don't want to match nulls here.
#pragma warning disable CA1820 // Test for empty strings using string length
                    if ((string)constantPattern.Value == string.Empty)
#pragma warning restore CA1820 // Test for empty strings using string length
                    {
                        return _sqlExpressionFactory.Constant(true);
                    }

                    return _sqlExpressionFactory.GreaterThan(
                        _sqlExpressionFactory.Function(
                            "CHARINDEX",
                            new[] { pattern, instance },
                            nullResultAllowed: true,
                            argumentsPropagateNullability: new[] { true, true },
                            typeof(int)),
                        _sqlExpressionFactory.Constant(0));
                }

                return _sqlExpressionFactory.OrElse(
                    _sqlExpressionFactory.Equal(
                        pattern,
                        _sqlExpressionFactory.Constant(string.Empty, stringTypeMapping)),
                    _sqlExpressionFactory.GreaterThan(
                        _sqlExpressionFactory.Function(
                            "CHARINDEX",
                            new[] { pattern, instance },
                            nullResultAllowed: true,
                            argumentsPropagateNullability: new[] { true, true },
                            typeof(int)),
                        _sqlExpressionFactory.Constant(0)));
            }

            if (_startsWithMethodInfo.Equals(method))
            {
                return TranslateStartsEndsWith(instance, arguments[0], true);
            }

            if (_endsWithMethodInfo.Equals(method))
            {
                return TranslateStartsEndsWith(instance, arguments[0], false);
            }

            return null;
        }

        private SqlExpression TranslateStartsEndsWith(SqlExpression instance, SqlExpression pattern, bool startsWith)
        {
            var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, pattern);

            instance = _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping);
            pattern = _sqlExpressionFactory.ApplyTypeMapping(pattern, stringTypeMapping);

            if (pattern is SqlConstantExpression constantExpression)
            {
                // The pattern is constant. Aside from null or empty, we escape all special characters (%, _, \)
                // in C# and send a simple LIKE
                if (!(constantExpression.Value is string constantString))
                {
                    return _sqlExpressionFactory.Like(
                        instance,
                        _sqlExpressionFactory.Constant(null, stringTypeMapping));
                }

                return constantString.Any(c => IsLikeWildChar(c))
                    ? _sqlExpressionFactory.Like(
                        instance,
                        _sqlExpressionFactory.Constant(
                            startsWith
                                ? EscapeLikePattern(constantString) + '%'
                                : '%' + EscapeLikePattern(constantString)),
                        _sqlExpressionFactory.Constant(
                            LikeEscapeChar.ToString())) // SQL Server has no char mapping, avoid value conversion warning)
                    : _sqlExpressionFactory.Like(
                        instance,
                        _sqlExpressionFactory.Constant(startsWith ? constantString + '%' : '%' + constantString));
            }

            // The pattern is non-constant, we use LEFT or RIGHT to extract substring and compare.
            if (startsWith)
            {
                return _sqlExpressionFactory.Equal(
                    _sqlExpressionFactory.Function(
                        "LEFT",
                        new[]
                        {
                            instance,
                            _sqlExpressionFactory.Function(
                                "LEN",
                                new[] { pattern },
                                nullResultAllowed: true,
                                argumentsPropagateNullability: new[] { true },
                                typeof(int))
                        },
                        nullResultAllowed: true,
                        argumentsPropagateNullability: new[] { true, true },
                        typeof(string),
                        stringTypeMapping),
                    pattern);
            }

            return _sqlExpressionFactory.Equal(
                _sqlExpressionFactory.Function(
                    "RIGHT",
                    new[]
                    {
                        instance,
                        _sqlExpressionFactory.Function(
                            "LEN",
                            new[] { pattern },
                            nullResultAllowed: true,
                            argumentsPropagateNullability: new[] { true },
                            typeof(int))
                    },
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new[] { true, true },
                    typeof(string),
                    stringTypeMapping),
                pattern);
        }

        // See https://docs.microsoft.com/en-us/sql/t-sql/language-elements/like-transact-sql
        private bool IsLikeWildChar(char c) => c == '%' || c == '_' || c == '[';

        private string EscapeLikePattern(string pattern)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < pattern.Length; i++)
            {
                var c = pattern[i];
                if (IsLikeWildChar(c)
                    || c == LikeEscapeChar)
                {
                    builder.Append(LikeEscapeChar);
                }

                builder.Append(c);
            }

            return builder.ToString();
        }
    }
}
