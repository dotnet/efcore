// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqliteStringMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _indexOfMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.IndexOf), new[] { typeof(string) });

        private static readonly MethodInfo _replaceMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.Replace), new[] { typeof(string), typeof(string) });

        private static readonly MethodInfo _toLowerMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.ToLower), Array.Empty<Type>());

        private static readonly MethodInfo _toUpperMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.ToUpper), Array.Empty<Type>());

        private static readonly MethodInfo _substringMethodInfoWithOneArg
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.Substring), new[] { typeof(int) });

        private static readonly MethodInfo _substringMethodInfoWithTwoArgs
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.Substring), new[] { typeof(int), typeof(int) });

        private static readonly MethodInfo _isNullOrWhiteSpaceMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.IsNullOrWhiteSpace), new[] { typeof(string) });

        // Method defined in netcoreapp2.0 only
        private static readonly MethodInfo _trimStartMethodInfoWithoutArgs
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.TrimStart), Array.Empty<Type>());

        private static readonly MethodInfo _trimStartMethodInfoWithCharArg
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.TrimStart), new[] { typeof(char) });

        private static readonly MethodInfo _trimEndMethodInfoWithoutArgs
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.TrimEnd), Array.Empty<Type>());

        private static readonly MethodInfo _trimEndMethodInfoWithCharArg
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.TrimEnd), new[] { typeof(char) });

        private static readonly MethodInfo _trimMethodInfoWithoutArgs
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.Trim), Array.Empty<Type>());

        private static readonly MethodInfo _trimMethodInfoWithCharArg
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.Trim), new[] { typeof(char) });

        // Method defined in netstandard2.0
        private static readonly MethodInfo _trimStartMethodInfoWithCharArrayArg
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.TrimStart), new[] { typeof(char[]) });

        private static readonly MethodInfo _trimEndMethodInfoWithCharArrayArg
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.TrimEnd), new[] { typeof(char[]) });

        private static readonly MethodInfo _trimMethodInfoWithCharArrayArg
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.Trim), new[] { typeof(char[]) });

        private static readonly MethodInfo _startsWithMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });

        private static readonly MethodInfo _containsMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.Contains), new[] { typeof(string) });

        private static readonly MethodInfo _endsWithMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) });

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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteStringMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(logger, nameof(logger));

            if (instance != null)
            {
                if (_indexOfMethodInfo.Equals(method))
                {
                    var argument = arguments[0];
                    var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, argument);

                    return _sqlExpressionFactory.Subtract(
                        _sqlExpressionFactory.Function(
                            "instr",
                            new[]
                            {
                                _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping),
                                _sqlExpressionFactory.ApplyTypeMapping(argument, stringTypeMapping)
                            },
                            nullable: true,
                            argumentsPropagateNullability: new[] { true, true },
                            method.ReturnType),
                        _sqlExpressionFactory.Constant(1));
                }

                if (_replaceMethodInfo.Equals(method))
                {
                    var firstArgument = arguments[0];
                    var secondArgument = arguments[1];
                    var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, firstArgument, secondArgument);

                    return _sqlExpressionFactory.Function(
                        "replace",
                        new[]
                        {
                        _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(firstArgument, stringTypeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(secondArgument, stringTypeMapping)
                        },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, true, true },
                        method.ReturnType,
                        stringTypeMapping);
                }

                if (_toLowerMethodInfo.Equals(method)
                    || _toUpperMethodInfo.Equals(method))
                {
                    return _sqlExpressionFactory.Function(
                        _toLowerMethodInfo.Equals(method) ? "lower" : "upper",
                        new[] { instance },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        method.ReturnType,
                        instance.TypeMapping);
                }

                if (_substringMethodInfoWithOneArg.Equals(method))
                {
                    return _sqlExpressionFactory.Function(
                        "substr",
                        new[] { instance, _sqlExpressionFactory.Add(arguments[0], _sqlExpressionFactory.Constant(1)) },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, true },
                        method.ReturnType,
                        instance.TypeMapping);
                }

                if (_substringMethodInfoWithTwoArgs.Equals(method))
                {
                    return _sqlExpressionFactory.Function(
                        "substr",
                        new[] { instance, _sqlExpressionFactory.Add(arguments[0], _sqlExpressionFactory.Constant(1)), arguments[1] },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, true, true },
                        method.ReturnType,
                        instance.TypeMapping);
                }

                if (_trimStartMethodInfoWithoutArgs?.Equals(method) == true
                    || _trimStartMethodInfoWithCharArg?.Equals(method) == true
                    || _trimStartMethodInfoWithCharArrayArg.Equals(method))
                {
                    return ProcessTrimMethod(instance, arguments, "ltrim");
                }

                if (_trimEndMethodInfoWithoutArgs?.Equals(method) == true
                    || _trimEndMethodInfoWithCharArg?.Equals(method) == true
                    || _trimEndMethodInfoWithCharArrayArg.Equals(method))
                {
                    return ProcessTrimMethod(instance, arguments, "rtrim");
                }

                if (_trimMethodInfoWithoutArgs?.Equals(method) == true
                    || _trimMethodInfoWithCharArg?.Equals(method) == true
                    || _trimMethodInfoWithCharArrayArg.Equals(method))
                {
                    return ProcessTrimMethod(instance, arguments, "trim");
                }

                if (_containsMethodInfo.Equals(method))
                {
                    var pattern = arguments[0];
                    var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, pattern);

                    instance = _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping);
                    pattern = _sqlExpressionFactory.ApplyTypeMapping(pattern, stringTypeMapping);

                    return _sqlExpressionFactory.OrElse(
                        _sqlExpressionFactory.Equal(
                            pattern,
                            _sqlExpressionFactory.Constant(string.Empty, stringTypeMapping)),
                        _sqlExpressionFactory.GreaterThan(
                            _sqlExpressionFactory.Function(
                                "instr",
                                new[] { instance, pattern },
                                nullable: true,
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
            }

            if (_isNullOrWhiteSpaceMethodInfo.Equals(method))
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

            if (_firstOrDefaultMethodInfoWithoutArgs.Equals(method))
            {
                var argument = arguments[0];
                return _sqlExpressionFactory.Function(
                    "substr",
                    new[] { argument, _sqlExpressionFactory.Constant(1), _sqlExpressionFactory.Constant(1) },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true, true },
                    method.ReturnType);
            }

            if (_lastOrDefaultMethodInfoWithoutArgs.Equals(method))
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
                            argumentsPropagateNullability: new[] { true },
                            typeof(int)),
                        _sqlExpressionFactory.Constant(1)
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true, true },
                    method.ReturnType);
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
                    return _sqlExpressionFactory.Like(instance, _sqlExpressionFactory.Constant(null, stringTypeMapping));
                }

                if (constantString.Length == 0)
                {
                    return _sqlExpressionFactory.Constant(true);
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
            // For StartsWith we also first run a LIKE to quickly filter out most non-matching results (sargable, but imprecise
            // because of wildchars).
            if (startsWith)
            {
                return _sqlExpressionFactory.OrElse(
                    _sqlExpressionFactory.AndAlso(
                        _sqlExpressionFactory.Like(
                            instance,
                            _sqlExpressionFactory.Add(
                                pattern,
                                _sqlExpressionFactory.Constant("%"))),
                        _sqlExpressionFactory.Equal(
                            _sqlExpressionFactory.Function(
                                "substr",
                                new[]
                                {
                                    instance,
                                    _sqlExpressionFactory.Constant(1),
                                    _sqlExpressionFactory.Function(
                                        "length",
                                        new[] { pattern },
                                        nullable: true,
                                        argumentsPropagateNullability: new[] { true },
                                        typeof(int))
                                },
                                nullable: true,
                                argumentsPropagateNullability: new[] { true, false, true },
                                typeof(string),
                                stringTypeMapping),
                            pattern)),
                    _sqlExpressionFactory.Equal(
                        pattern,
                        _sqlExpressionFactory.Constant(string.Empty)));
            }

            return _sqlExpressionFactory.OrElse(
                _sqlExpressionFactory.Equal(
                    _sqlExpressionFactory.Function(
                        "substr",
                        new[]
                        {
                            instance,
                            _sqlExpressionFactory.Negate(
                                _sqlExpressionFactory.Function(
                                    "length",
                                    new[] { pattern },
                                    nullable: true,
                                    argumentsPropagateNullability: new[] { true },
                                    typeof(int)))
                        },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, true },
                        typeof(string),
                        stringTypeMapping),
                    pattern),
                _sqlExpressionFactory.Equal(
                    pattern,
                    _sqlExpressionFactory.Constant(string.Empty)));
        }

        // See https://www.sqlite.org/lang_expr.html
        private bool IsLikeWildChar(char c)
            => c == '%' || c == '_';

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
                argumentsPropagateNullability: sqlArguments.Select(a => true).ToList(),
                typeof(string),
                typeMapping);
        }
    }
}
