// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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

        private static readonly MethodInfo _firstOrDefaultMethodInfoWithoutArgs
            = typeof(Enumerable).GetRuntimeMethods().Single(
                m => m.Name == nameof(Enumerable.FirstOrDefault)
                    && m.GetParameters().Length == 1).MakeGenericMethod(typeof(char));

        private static readonly MethodInfo _lastOrDefaultMethodInfoWithoutArgs
            = typeof(Enumerable).GetRuntimeMethods().Single(
                m => m.Name == nameof(Enumerable.LastOrDefault)
                    && m.GetParameters().Length == 1).MakeGenericMethod(typeof(char));

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        private const char LikeEscapeChar = '\\';
        private const string LikeEscapeString = "\\";

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerStringMethodTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(logger, nameof(logger));

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
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, true },
                        typeof(long));

                    charIndexExpression = _sqlExpressionFactory.Convert(charIndexExpression, typeof(int));
                }
                else
                {
                    charIndexExpression = _sqlExpressionFactory.Function(
                        "CHARINDEX",
                        new[] { argument, _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping) },
                        nullable: true,
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
                    nullable: true,
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
                    nullable: true,
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
                    nullable: true,
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
                                    nullable: true,
                                    argumentsPropagateNullability: new[] { true },
                                    argument.Type,
                                    argument.TypeMapping)
                            },
                            nullable: true,
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
                    nullable: true,
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
                    nullable: true,
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
                            nullable: true,
                            argumentsPropagateNullability: new[] { true },
                            instance.Type,
                            instance.TypeMapping)
                    },
                    nullable: true,
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
                    if (!(constantPattern.Value is string patternValue))
                    {
                        return _sqlExpressionFactory.Like(
                            instance,
                            _sqlExpressionFactory.Constant(null, stringTypeMapping));
                    }

                    if (patternValue.Length == 0)
                    {
                        return _sqlExpressionFactory.Constant(true);
                    }

                    return patternValue.Any(IsLikeWildChar)
                        ? _sqlExpressionFactory.Like(
                            instance,
                            _sqlExpressionFactory.Constant($"%{EscapeLikePattern(patternValue)}%"),
                            _sqlExpressionFactory.Constant(LikeEscapeString))
                        : _sqlExpressionFactory.Like(instance, _sqlExpressionFactory.Constant($"%{patternValue}%"));
                }

                return _sqlExpressionFactory.OrElse(
                    _sqlExpressionFactory.Like(
                        pattern,
                        _sqlExpressionFactory.Constant(string.Empty, stringTypeMapping)),
                    _sqlExpressionFactory.GreaterThan(
                        _sqlExpressionFactory.Function(
                            "CHARINDEX",
                            new[] { pattern, instance },
                            nullable: true,
                            argumentsPropagateNullability: new[] { true, true },
                            typeof(int)),
                        _sqlExpressionFactory.Constant(0)));
            }

            if (_firstOrDefaultMethodInfoWithoutArgs.Equals(method))
            {
                var argument = arguments[0];
                return _sqlExpressionFactory.Function(
                    "SUBSTRING",
                    new[] { argument, _sqlExpressionFactory.Constant(1), _sqlExpressionFactory.Constant(1) },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true, true },
                    method.ReturnType);
            }

            if (_lastOrDefaultMethodInfoWithoutArgs.Equals(method))
            {
                var argument = arguments[0];
                return _sqlExpressionFactory.Function(
                    "SUBSTRING",
                    new[]
                    {
                        argument,
                        _sqlExpressionFactory.Function(
                            "LEN",
                            new[] { argument },
                            nullable: true,
                            argumentsPropagateNullability: new[] { true },
                            typeof(int)),
                        _sqlExpressionFactory.Constant(1)
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true, true },
                    method.ReturnType);
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
                if (!(constantExpression.Value is string patternValue))
                {
                    return _sqlExpressionFactory.Like(
                        instance,
                        _sqlExpressionFactory.Constant(null, stringTypeMapping));
                }

                return patternValue.Any(IsLikeWildChar)
                    ? _sqlExpressionFactory.Like(
                        instance,
                        _sqlExpressionFactory.Constant(
                            startsWith
                                ? EscapeLikePattern(patternValue) + '%'
                                : '%' + EscapeLikePattern(patternValue)),
                        _sqlExpressionFactory.Constant(LikeEscapeString))
                    : _sqlExpressionFactory.Like(
                        instance,
                        _sqlExpressionFactory.Constant(startsWith ? patternValue + '%' : '%' + patternValue));
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
                                nullable: true,
                                argumentsPropagateNullability: new[] { true },
                                typeof(int))
                        },
                        nullable: true,
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
                            nullable: true,
                            argumentsPropagateNullability: new[] { true },
                            typeof(int))
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true },
                    typeof(string),
                    stringTypeMapping),
                pattern);
        }

        // See https://docs.microsoft.com/en-us/sql/t-sql/language-elements/like-transact-sql
        private bool IsLikeWildChar(char c)
            => c == '%' || c == '_' || c == '[';

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
