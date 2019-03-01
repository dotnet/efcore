// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
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

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ITypeMappingApplyingExpressionVisitor _typeMappingApplyingExpressionVisitor;
        private readonly RelationalTypeMapping _intTypeMapping;
        private readonly RelationalTypeMapping _boolTypeMapping;

        private const char LikeEscapeChar = '\\';

        public SqlServerStringMethodTranslator(IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor)
        {
            _typeMappingSource = typeMappingSource;
            _typeMappingApplyingExpressionVisitor = typeMappingApplyingExpressionVisitor;
            _intTypeMapping = _typeMappingSource.FindMapping(typeof(int));
            _boolTypeMapping = _typeMappingSource.FindMapping(typeof(bool));
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (_indexOfMethodInfo.Equals(method))
            {
                var argument = arguments[0];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, argument);
                argument = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(argument, stringTypeMapping);

                var charIndexExpression =
                    new SqlBinaryExpression(
                        ExpressionType.Subtract,
                        new SqlFunctionExpression(
                            "CHARINDEX",
                            new[]
                            {
                                argument,
                                _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(instance, stringTypeMapping)
                            },
                            method.ReturnType,
                            _intTypeMapping,
                            false),
                        MakeSqlConstant(1),
                        method.ReturnType,
                        _intTypeMapping);

                return new CaseExpression(
                    new[]
                    {
                        new CaseWhenClause(
                            new SqlBinaryExpression(
                                ExpressionType.Equal,
                                argument,
                                new SqlConstantExpression(Expression.Constant(string.Empty), stringTypeMapping),
                                typeof(bool),
                                _boolTypeMapping),
                            MakeSqlConstant(0))
                    },
                    charIndexExpression);
            }

            if (_replaceMethodInfo.Equals(method))
            {
                var firstArgument = arguments[0];
                var secondArgument = arguments[1];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, firstArgument, secondArgument);

                instance = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(instance, stringTypeMapping);
                firstArgument = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(firstArgument, stringTypeMapping);
                secondArgument = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(secondArgument, stringTypeMapping);

                return new SqlFunctionExpression(
                    "REPLACE",
                    new[]
                    {
                        instance,
                        firstArgument,
                        secondArgument
                    },
                    method.ReturnType,
                    stringTypeMapping,
                    false);
            }

            if (_toLowerMethodInfo.Equals(method)
                || _toUpperMethodInfo.Equals(method))
            {
                return new SqlFunctionExpression(
                    _toLowerMethodInfo.Equals(method) ? "LOWER" : "UPPER",
                    new[] { instance },
                    method.ReturnType,
                    instance.TypeMapping,
                    false);
            }

            if (_substringMethodInfo.Equals(method))
            {
                return new SqlFunctionExpression(
                    "SUBSTRING",
                    new[]
                    {
                        instance,
                        new SqlBinaryExpression(
                            ExpressionType.Add,
                            _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(arguments[0], _intTypeMapping),
                            MakeSqlConstant(1),
                            typeof(int),
                            _intTypeMapping),
                        _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(arguments[1], _intTypeMapping),
                    },
                    method.ReturnType,
                    instance.TypeMapping,
                    false);
            }

            if (_isNullOrWhiteSpaceMethodInfo.Equals(method))
            {
                var argument = arguments[0];

                return new SqlBinaryExpression(
                    ExpressionType.OrElse,
                    new SqlNullExpression(argument, false, _boolTypeMapping),
                    new SqlBinaryExpression(
                        ExpressionType.Equal,
                        new SqlFunctionExpression(
                            "LTRIM",
                            new[] {
                                new SqlFunctionExpression(
                                    "RTRIM",
                                    new[]
                                    {
                                        argument
                                    },
                                    argument.Type,
                                    argument.TypeMapping,
                                    false)
                            },
                            argument.Type,
                            argument.TypeMapping,
                            false),
                        new SqlConstantExpression(
                            Expression.Constant(string.Empty),
                            argument.TypeMapping),
                        typeof(bool),
                        _boolTypeMapping),
                    typeof(bool),
                    _boolTypeMapping);
            }

            if (_trimStartMethodInfoWithoutArgs?.Equals(method) == true
                || (_trimStartMethodInfoWithCharArrayArg.Equals(method)
                    // SqlServer LTRIM does not take arguments
                    && ((arguments[0] as SqlConstantExpression)?.Value as Array)?.Length == 0))
            {
                return new SqlFunctionExpression(
                    "LTRIM",
                    new[]
                    {
                        instance
                    },
                    instance.Type,
                    instance.TypeMapping,
                    false);
            }

            if (_trimEndMethodInfoWithoutArgs?.Equals(method) == true
                || (_trimEndMethodInfoWithCharArrayArg.Equals(method)
                    // SqlServer RTRIM does not take arguments
                    && ((arguments[0] as SqlConstantExpression)?.Value as Array)?.Length == 0))
            {
                return new SqlFunctionExpression(
                    "RTRIM",
                    new[]
                    {
                        instance
                    },
                    instance.Type,
                    instance.TypeMapping,
                    false);
            }

            if (_trimMethodInfoWithoutArgs?.Equals(method) == true
                || (_trimMethodInfoWithCharArrayArg.Equals(method)
                    // SqlServer LTRIM/RTRIM does not take arguments
                    && ((arguments[0] as SqlConstantExpression)?.Value as Array)?.Length == 0))
            {
                return new SqlFunctionExpression(
                    "LTRIM",
                    new[]
                    {
                        new SqlFunctionExpression(
                            "RTRIM",
                            new []
                            {
                                instance
                            },
                            instance.Type,
                            instance.TypeMapping,
                            false)
                    },
                    instance.Type,
                    instance.TypeMapping,
                    false);
            }

            if (_containsMethodInfo.Equals(method))
            {
                var pattern = arguments[0];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, pattern);

                instance = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(instance, stringTypeMapping);
                pattern = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(pattern, stringTypeMapping);

                return new SqlBinaryExpression(
                    ExpressionType.OrElse,
                    new SqlBinaryExpression(
                        ExpressionType.Equal,
                        pattern,
                        new SqlConstantExpression(Expression.Constant(string.Empty), stringTypeMapping),
                        typeof(bool),
                        _boolTypeMapping),
                    new SqlBinaryExpression(
                        ExpressionType.GreaterThan,
                        new SqlFunctionExpression(
                            "CHARINDEX",
                            new[]
                            {
                                pattern,
                                instance
                            },
                            typeof(int),
                            _intTypeMapping,
                            false),
                        MakeSqlConstant(0),
                        typeof(bool),
                        _boolTypeMapping),
                    typeof(bool),
                    _boolTypeMapping);
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

        SqlExpression TranslateStartsEndsWith(SqlExpression instance, SqlExpression pattern, bool startsWith)
        {
            var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, pattern);

            instance = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(instance, stringTypeMapping);
            pattern = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(pattern, stringTypeMapping);

            if (pattern is SqlConstantExpression constExpr)
            {
                // The pattern is constant. Aside from null or empty, we escape all special characters (%, _, \)
                // in C# and send a simple LIKE
                if (!(constExpr.Value is string constPattern))
                {
                    return new LikeExpression(instance, new SqlConstantExpression(Expression.Constant(null), stringTypeMapping), null, _boolTypeMapping);
                }
                if (constPattern.Length == 0)
                {
                    return new SqlConstantExpression(Expression.Constant(true), _boolTypeMapping);
                }
                return constPattern.Any(c => IsLikeWildChar(c))
                    ? new LikeExpression(
                        instance,
                        new SqlConstantExpression(Expression.Constant(startsWith ? EscapeLikePattern(constPattern) + '%' : '%' + EscapeLikePattern(constPattern)), stringTypeMapping),
                        new SqlConstantExpression(Expression.Constant(LikeEscapeChar.ToString()), stringTypeMapping),  // SQL Server has no char mapping, avoid value conversion warning
                        _boolTypeMapping)
                    : new LikeExpression(
                        instance,
                        new SqlConstantExpression(Expression.Constant(startsWith ? constPattern + '%' : '%' + constPattern), stringTypeMapping),
                        null,
                        _boolTypeMapping);
            }

            // The pattern is non-constant, we use LEFT or RIGHT to extract substring and compare.
            // For StartsWith we also first run a LIKE to quickly filter out most non-matching results (sargable, but imprecise
            // because of wildchars).
            if (startsWith)
            {
                return new SqlBinaryExpression(
                    ExpressionType.OrElse,
                    new SqlBinaryExpression(
                        ExpressionType.AndAlso,
                        new LikeExpression(
                            // ReSharper disable once AssignNullToNotNullAttribute
                            instance,
                            new SqlBinaryExpression(
                                ExpressionType.Add,
                                instance,
                                new SqlConstantExpression(Expression.Constant("%"), stringTypeMapping),
                                typeof(string),
                                stringTypeMapping),
                            null,
                            _boolTypeMapping),
                        new SqlBinaryExpression(
                            ExpressionType.Equal,
                            new SqlFunctionExpression(
                                "LEFT",
                                new[] {
                                    instance,
                                    new SqlFunctionExpression("LEN", new[] { pattern }, typeof(int), _intTypeMapping, false)
                                },
                                typeof(string),
                                stringTypeMapping,
                                false),
                            pattern,
                            typeof(bool),
                            _boolTypeMapping),
                        typeof(bool),
                        _boolTypeMapping),
                    new SqlBinaryExpression(
                        ExpressionType.Equal,
                        pattern,
                        new SqlConstantExpression(Expression.Constant(string.Empty), stringTypeMapping),
                        typeof(bool),
                        _boolTypeMapping
                    ),
                    typeof(bool),
                    _boolTypeMapping);
            }

            return new SqlBinaryExpression(
                ExpressionType.OrElse,
                new SqlBinaryExpression(
                    ExpressionType.Equal,
                    new SqlFunctionExpression(
                        "RIGHT",
                        new[] {
                            instance,
                            new SqlFunctionExpression("LEN", new[] { pattern }, typeof(int), _intTypeMapping, false)
                        },
                        typeof(string),
                        stringTypeMapping,
                        false),
                    pattern,
                    typeof(bool),
                    _boolTypeMapping),
                new SqlBinaryExpression(
                    ExpressionType.Equal,
                    pattern,
                    new SqlConstantExpression(Expression.Constant(string.Empty), stringTypeMapping),
                    typeof(bool),
                    _boolTypeMapping),
                typeof(bool),
                _boolTypeMapping);
        }

        // See https://docs.microsoft.com/en-us/sql/t-sql/language-elements/like-transact-sql
        private bool IsLikeWildChar(char c) => c == '%' || c == '_' || c == '[';

        private string EscapeLikePattern(string pattern)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < pattern.Length; i++)
            {
                var c = pattern[i];
                if (IsLikeWildChar(c) || c == LikeEscapeChar)
                {
                    builder.Append(LikeEscapeChar);
                }
                builder.Append(c);
            }
            return builder.ToString();
        }

        private SqlExpression MakeSqlConstant(int value)
        {
            return new SqlConstantExpression(Expression.Constant(value), _intTypeMapping);
        }
    }
}
