// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal
{
    public class XGStringComparisonMethodTranslator : XGQueryCompilationContextMethodTranslator
    {
        private static readonly MethodInfo _equalsMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.Equals), new[] {typeof(string), typeof(StringComparison)});

        private static readonly MethodInfo _staticEqualsMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.Equals), new[] {typeof(string), typeof(string), typeof(StringComparison)});

        private static readonly MethodInfo _startsWithMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] {typeof(string), typeof(StringComparison)});

        private static readonly MethodInfo _endsWithMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] {typeof(string), typeof(StringComparison)});

        private static readonly MethodInfo _containsMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] {typeof(string), typeof(StringComparison)});

        private static readonly MethodInfo _indexOfMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), new[] {typeof(string), typeof(StringComparison)});

        private static readonly MethodInfo _indexOfMethodInfoWithStartIndexArg
            = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), new[] { typeof(string), typeof(int), typeof(StringComparison) });

        internal static readonly MethodInfo[] StringComparisonMethodInfos =
        {
            _equalsMethodInfo,
            _staticEqualsMethodInfo,
            _startsWithMethodInfo,
            _endsWithMethodInfo,
            _containsMethodInfo,
            _indexOfMethodInfo,
            _indexOfMethodInfoWithStartIndexArg
        };

        internal static readonly MethodInfo[] RelationalErrorHandledStringComparisonMethodInfos =
        {
            _equalsMethodInfo,
            _staticEqualsMethodInfo,
        };

        private readonly IReadOnlyList<SqlExpression> _caseSensitiveComparisons;

        private readonly XGSqlExpressionFactory _sqlExpressionFactory;
        private readonly IXGOptions _options;

        private static readonly MethodInfo _escapeLikePatternParameterMethod =
            typeof(XGStringComparisonMethodTranslator).GetTypeInfo().GetDeclaredMethod(nameof(ConstructLikePatternParameter))!;

        public XGStringComparisonMethodTranslator(
            ISqlExpressionFactory sqlExpressionFactory,
            Func<QueryCompilationContext> queryCompilationContextResolver,
            IXGOptions options)
        : base(queryCompilationContextResolver)
        {
            _sqlExpressionFactory = (XGSqlExpressionFactory)sqlExpressionFactory;
            _options = options;
            _caseSensitiveComparisons = new[]
            {
                _sqlExpressionFactory.Constant(StringComparison.Ordinal),
                _sqlExpressionFactory.Constant(StringComparison.CurrentCulture),
                _sqlExpressionFactory.Constant(StringComparison.InvariantCulture)
            }.ToList().AsReadOnly();
        }

        public override SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            QueryCompilationContext queryCompilationContext)
        {
            if(_options.StringComparisonTranslations)
            {
                if(instance != null)
                {
                    if (Equals(method, _equalsMethodInfo))
                    {
                        return MakeStringEqualsExpression(
                            instance,
                            arguments[0],
                            arguments[1]
                        );
                    }

                    if (Equals(method, _startsWithMethodInfo))
                    {
                        return MakeStartsWithExpression(queryCompilationContext, instance, arguments[0], arguments[1]);
                    }

                    if (Equals(method, _endsWithMethodInfo))
                    {
                        return MakeEndsWithExpression(queryCompilationContext, instance, arguments[0], arguments[1]);
                    }

                    if (Equals(method, _containsMethodInfo))
                    {
                        return MakeContainsExpression(queryCompilationContext, instance, arguments[0], arguments[1]);
                    }

                    if (Equals(method, _indexOfMethodInfo))
                    {
                        return MakeIndexOfExpression(
                            instance,
                            arguments[0],
                            stringComparison: arguments[1]
                        );
                    }

                    if (Equals(method, _indexOfMethodInfoWithStartIndexArg))
                    {
                        return MakeIndexOfExpression(
                            instance,
                            arguments[0],
                            startIndex: arguments[1],
                            stringComparison: arguments[2]
                        );
                    }
                }

                if (Equals(method, _staticEqualsMethodInfo))
                {
                    return MakeStringEqualsExpression(
                        arguments[0],
                        arguments[1],
                        arguments[2]
                    );
                }
            }

            return null;
        }

        public virtual SqlExpression MakeStringEqualsExpression(
            [NotNull] SqlExpression leftValue,
            [NotNull] SqlExpression rightValue,
            [NotNull] SqlExpression stringComparison)
        {
            if (TryGetExpressionValue<StringComparison>(stringComparison, out var cmp))
            {
                return CreateExpressionForCaseSensitivity(
                    cmp,
                    () =>
                    {
                        if (leftValue is ColumnExpression)
                        {
                            // Applying the binary operator to the non-column value enables SQL to
                            // utilize an index if one exists.
                            return _sqlExpressionFactory.Equal(
                                leftValue,
                                Utf8Bin(rightValue));
                        }

                        return _sqlExpressionFactory.Equal(
                            Utf8Bin(leftValue),
                            rightValue);
                    },
                    () => _sqlExpressionFactory.Equal(
                        LCase(leftValue),
                        Utf8Bin(LCase(rightValue))));
            }

            return new CaseExpression(
                new[]
                {
                    new CaseWhenClause(
                        _sqlExpressionFactory.In(stringComparison, _caseSensitiveComparisons),
                        // Case sensitive, accent sensitive
                        _sqlExpressionFactory.Equal(
                            leftValue,
                            Utf8Bin(rightValue)
                        )
                    )
                },
                // Case insensitive, accent sensitive
                _sqlExpressionFactory.Equal(
                    LCase(leftValue),
                    Utf8Bin(LCase(rightValue)))
            );
        }

        public virtual SqlExpression MakeStartsWithExpression(
            QueryCompilationContext queryCompilationContext,
            [NotNull] SqlExpression target,
            [NotNull] SqlExpression prefix,
            [CanBeNull] SqlExpression stringComparison = null)
        {
            if (stringComparison == null)
            {
                return MakeStartsWithEndsWithExpressionImpl(
                    queryCompilationContext,
                    target,
                    e => e,
                    prefix,
                    e => e,
                    true);
            }

            if (TryGetExpressionValue<StringComparison>(stringComparison, out var cmp))
            {
                return CreateExpressionForCaseSensitivity(
                    cmp,
                    () => MakeStartsWithEndsWithExpressionImpl(
                        queryCompilationContext,
                        target,
                        e => e,
                        prefix,
                        e => Utf8Bin(e),
                        true),
                    () => MakeStartsWithEndsWithExpressionImpl(
                        queryCompilationContext,
                        LCase(target),
                        e => LCase(e),
                        prefix,
                        e => Utf8Bin(LCase(e)),
                        true));
            }

            return new CaseExpression(
                new[]
                {
                    new CaseWhenClause(
                        _sqlExpressionFactory.In(
                            stringComparison,
                            _caseSensitiveComparisons),
                        // Case sensitive, accent sensitive
                        MakeStartsWithEndsWithExpressionImpl(
                            queryCompilationContext,
                            target,
                            e => e,
                            prefix,
                            e => Utf8Bin(prefix),
                            true))
                },
                // Case insensitive, accent sensitive
                MakeStartsWithEndsWithExpressionImpl(
                    queryCompilationContext,
                    target,
                    e => LCase(e),
                    prefix,
                    e => Utf8Bin(LCase(e)),
                    true));
        }

        public virtual SqlExpression MakeEndsWithExpression(
            QueryCompilationContext queryCompilationContext,
            [NotNull] SqlExpression target,
            [NotNull] SqlExpression suffix,
            [CanBeNull] SqlExpression stringComparison = null)
        {
            if (stringComparison == null)
            {
                return MakeStartsWithEndsWithExpressionImpl(
                    queryCompilationContext,
                    target,
                    e => e,
                    suffix,
                    e => e,
                    false);
            }

            if (TryGetExpressionValue<StringComparison>(stringComparison, out var cmp))
            {
                return CreateExpressionForCaseSensitivity(
                    cmp,
                    () => MakeStartsWithEndsWithExpressionImpl(
                        queryCompilationContext,
                        target,
                        e => e,
                        suffix,
                        e => Utf8Bin(e),
                        false),
                    () => MakeStartsWithEndsWithExpressionImpl(
                        queryCompilationContext,
                        target,
                        e => LCase(e),
                        suffix,
                        e => Utf8Bin(LCase(e)),
                        false));
            }

            return new CaseExpression(
                new[]
                {
                    new CaseWhenClause(
                        _sqlExpressionFactory.In(
                            stringComparison,
                            _caseSensitiveComparisons),
                        // Case sensitive, accent sensitive
                        MakeStartsWithEndsWithExpressionImpl(
                            queryCompilationContext,
                            target,
                            e => e,
                            suffix,
                            e => Utf8Bin(e),
                            false))
                },
                // Case insensitive, accent sensitive
                MakeStartsWithEndsWithExpressionImpl(
                    queryCompilationContext,
                    target,
                    e => LCase(e),
                    suffix,
                    e => Utf8Bin(LCase(e)),
                    false));
        }

        public virtual SqlExpression MakeContainsExpression(
            QueryCompilationContext queryCompilationContext,
            [NotNull] SqlExpression target,
            [NotNull] SqlExpression search,
            [CanBeNull] SqlExpression stringComparison = null)
        {
            // Check, whether we should generate an optimized expression, that uses the current database
            // settings instead of an explicit string comparison value.
            if (stringComparison == null)
            {
                return MakeContainsExpressionImpl(
                    queryCompilationContext,
                    target,
                    e => e,
                    search,
                    e => e);
            }

            if (TryGetExpressionValue<StringComparison>(stringComparison, out var cmp))
            {
                return CreateExpressionForCaseSensitivity(
                    cmp,
                    () =>
                        MakeContainsExpressionImpl(
                            queryCompilationContext,
                            target,
                            e => e,
                            search,
                            e => Utf8Bin(e)
                        ),
                    () =>
                        MakeContainsExpressionImpl(
                            queryCompilationContext,
                            target,
                            e => LCase(e),
                            search,
                            e => Utf8Bin(LCase(e))
                        )
                );
            }

            return new CaseExpression(
                new[]
                {
                    new CaseWhenClause(
                        _sqlExpressionFactory.In(stringComparison, _caseSensitiveComparisons),
                        // Case sensitive, accent sensitive
                        MakeContainsExpressionImpl(
                            queryCompilationContext,
                            target,
                            e => e,
                            search,
                            e => Utf8Bin(e)
                        )
                    )
                },
                // Case insensitive, accent sensitive
                MakeContainsExpressionImpl(
                    queryCompilationContext,
                    target,
                    e => LCase(e),
                    search,
                    e => Utf8Bin(LCase(e))
                )
            );
        }

        private SqlExpression MakeStartsWithEndsWithExpressionImpl(
            QueryCompilationContext queryCompilationContext,
            SqlExpression target,
            [NotNull] Func<SqlExpression, SqlExpression> targetTransform,
            SqlExpression prefixSuffix,
            [NotNull] Func<SqlExpression, SqlExpression> prefixSuffixTransform,
            bool startsWith)
        {
            var stringTypeMapping = ExpressionExtensions.InferTypeMapping(target, prefixSuffix);
            target = _sqlExpressionFactory.ApplyTypeMapping(target, stringTypeMapping);
            prefixSuffix = _sqlExpressionFactory.ApplyTypeMapping(prefixSuffix, stringTypeMapping);

            if (prefixSuffix is SqlConstantExpression constantPrefixSuffixExpression)
            {
                // The prefix is constant. Aside from null or empty, we escape all special characters (%, _, \)
                // in C# and send a simple LIKE.
                return constantPrefixSuffixExpression.Value switch
                {
                    null => _sqlExpressionFactory.Like(targetTransform(target), _sqlExpressionFactory.Constant(null, typeof(string), stringTypeMapping)),
                    "" => _sqlExpressionFactory.Like(targetTransform(target), _sqlExpressionFactory.Constant("%")),
                    string s => _sqlExpressionFactory.Like(
                        targetTransform(target),
                        prefixSuffixTransform(
                            _sqlExpressionFactory.Constant(
                                $"{(startsWith ? string.Empty : "%")}{(s.Any(IsLikeWildOrEscapeChar) ? EscapeLikePattern(s) : s)}{(startsWith ? "%" : string.Empty)}"))),
                    _ => throw new UnreachableException(),
                };
            }

            if (GetLikeExpressionUsingParameter(
                    queryCompilationContext,
                    target,
                    targetTransform,
                    prefixSuffix,
                    stringTypeMapping,
                    startsWith
                        ? StartsEndsWithContains.StartsWith
                        : StartsEndsWithContains.EndsWith) is { } likeExpressionUsingParameter)
            {
                return likeExpressionUsingParameter;
            }

            // TODO: Generally, LEFT & compare is faster than escaping potential pattern characters with REPLACE().
            // However, this might not be the case, if the pattern is constant after all (e.g. `LCASE('fo%o')`), in
            // which case, `something LIKE CONCAT(REPLACE(REPLACE(LCASE('fo%o'), '%', '\\%'), '_', '\\_'), '%')` should
            // be faster than `LEFT(something, CHAR_LENGTH('fo%o')) = LCASE('fo%o')`.
            // See https://github.com/PomeloFoundation/Microsoft.EntityFrameworkCore.XuGu/issues/996#issuecomment-607733553

            // The prefix is non-constant, we use LEFT/RIGHT to extract the substring and compare.
            return _sqlExpressionFactory.AndAlso(
                _sqlExpressionFactory.IsNotNull(targetTransform(target)),
                _sqlExpressionFactory.AndAlso(
                    _sqlExpressionFactory.IsNotNull(prefixSuffix),
                    _sqlExpressionFactory.Equal(
                        _sqlExpressionFactory.NullableFunction(
                            startsWith ? "LEFT" : "RIGHT",
                            new[] { targetTransform(target), CharLength(prefixSuffix), },
                            typeof(string),
                            stringTypeMapping),
                        prefixSuffixTransform(prefixSuffix))));
        }

        private SqlExpression MakeContainsExpressionImpl(
            QueryCompilationContext queryCompilationContext,
            SqlExpression target,
            [NotNull] Func<SqlExpression, SqlExpression> targetTransform,
            SqlExpression pattern,
            [NotNull] Func<SqlExpression, SqlExpression> patternTransform)
        {
            var stringTypeMapping = ExpressionExtensions.InferTypeMapping(target, pattern);
            target = _sqlExpressionFactory.ApplyTypeMapping(target, stringTypeMapping);
            pattern = _sqlExpressionFactory.ApplyTypeMapping(pattern, stringTypeMapping);

            if (pattern is SqlConstantExpression constantPatternExpression)
            {
                // The prefix is constant. Aside from null or empty, we escape all special characters (%, _, \)
                // in C# and send a simple LIKE.
                return constantPatternExpression.Value switch
                {
                    null => _sqlExpressionFactory.Like(targetTransform(target), _sqlExpressionFactory.Constant(null, typeof(string), stringTypeMapping)),
                    "" => _sqlExpressionFactory.Like(targetTransform(target), _sqlExpressionFactory.Constant("%")),
                    string s => _sqlExpressionFactory.Like(
                        targetTransform(target),
                        patternTransform(_sqlExpressionFactory.Constant($"%{(s.Any(IsLikeWildOrEscapeChar) ? EscapeLikePattern(s) : s)}%"))),
                    _ => throw new UnreachableException(),
                };
            }

            if (GetLikeExpressionUsingParameter(
                    queryCompilationContext,
                    target,
                    targetTransform,
                    pattern,
                    stringTypeMapping,
                    StartsEndsWithContains.Contains) is { } likeExpressionUsingParameter)
            {
                return likeExpressionUsingParameter;
            }

            // 'foo' LIKE '' OR LOCATE('foo', 'barfoobar') > 0
            // This cannot be "'   ' = '' OR ..", because '   ' would be trimmed to '' when using equals, but not when using LIKE.
            // Using an empty pattern `LOCATE('', 'barfoobar')` returns 1.
            // return _sqlExpressionFactory.OrElse(
            //     _sqlExpressionFactory.Like(
            //         pattern,
            //         _sqlExpressionFactory.Constant(string.Empty, stringTypeMapping)),
            //     _sqlExpressionFactory.GreaterThan(
            //         Locate(patternTransform(pattern), targetTransform(target)),
            //         _sqlExpressionFactory.Constant(0)));

            // For Contains, just use CHARINDEX and check if the result is greater than 0.
            // Add a check to return null when the pattern is an empty string (and the string isn't null)
            return _sqlExpressionFactory.AndAlso(
                _sqlExpressionFactory.IsNotNull(target),
                _sqlExpressionFactory.AndAlso(
                    _sqlExpressionFactory.IsNotNull(pattern),
                    _sqlExpressionFactory.OrElse(
                        _sqlExpressionFactory.GreaterThan(
                            Locate(patternTransform(pattern), targetTransform(target)),
                            _sqlExpressionFactory.Constant(0)),
                        _sqlExpressionFactory.Like(
                            pattern,
                            _sqlExpressionFactory.Constant(string.Empty, stringTypeMapping)))));
        }

        protected virtual SqlExpression GetLikeExpressionUsingParameter(
            QueryCompilationContext queryCompilationContext,
            SqlExpression target,
            Func<SqlExpression, SqlExpression> targetTransform,
            SqlExpression pattern,
            RelationalTypeMapping stringTypeMapping,
            StartsEndsWithContains methodType)
        {
            if (pattern is SqlParameterExpression patternParameter &&
                patternParameter.Name.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal))
            {
                // The pattern is a parameter, register a runtime parameter that will contain the rewritten LIKE pattern, where
                // all special characters have been escaped.
                var lambda = Expression.Lambda(
                    Expression.Call(
                        _escapeLikePatternParameterMethod,
                        QueryCompilationContext.QueryContextParameter,
                        Expression.Constant(patternParameter.Name),
                        Expression.Constant(methodType)),
                    QueryCompilationContext.QueryContextParameter);

                var escapedPatternParameter =
                    queryCompilationContext.RegisterRuntimeParameter(
                        $"{patternParameter.Name}_{methodType.ToString().ToLower(CultureInfo.InvariantCulture)}",
                        lambda);

                return _sqlExpressionFactory.Like(
                    targetTransform(target),
                    new SqlParameterExpression(escapedPatternParameter.Name!, escapedPatternParameter.Type, stringTypeMapping));
            }

            return null;
        }

        public virtual SqlExpression MakeIndexOfExpression(
            [NotNull] SqlExpression target,
            [NotNull] SqlExpression search,
            [CanBeNull] SqlExpression stringComparison = null,
            [CanBeNull] SqlExpression startIndex = null)
        {
            if (stringComparison == null)
            {
                return MakeIndexOfExpressionImpl(
                    target,
                    e => e,
                    search,
                    e => e,
                    startIndex);
            }

            // Users have to opt-in, to use string method translations with an explicit StringComparison parameter.
            if (!_options.StringComparisonTranslations)
            {
                return null;
            }

            if (TryGetExpressionValue<StringComparison>(stringComparison, out var cmp))
            {
                return CreateExpressionForCaseSensitivity(
                    cmp,
                    () => MakeIndexOfExpressionImpl(
                        target,
                        e => e,
                        search,
                        e => Utf8Bin(e),
                        startIndex),
                    () => MakeIndexOfExpressionImpl(
                        target,
                        e => LCase(e),
                        search,
                        e => Utf8Bin(LCase(e)),
                        startIndex));
            }

            return _sqlExpressionFactory.Case(
                new[]
                {
                    new CaseWhenClause(
                        _sqlExpressionFactory.In(
                            stringComparison,
                            _caseSensitiveComparisons),
                        // Case sensitive, accent sensitive
                        MakeIndexOfExpressionImpl(
                            target,
                            e => e,
                            search,
                            e => Utf8Bin(e),
                            startIndex))
                },
                // Case insensitive, accent sensitive
                MakeIndexOfExpressionImpl(
                    target,
                    e => LCase(e),
                    search,
                    e => Utf8Bin(LCase(e)),
                    startIndex));
        }

        private SqlExpression MakeIndexOfExpressionImpl(
            SqlExpression target,
            [NotNull] Func<SqlExpression, SqlExpression> targetTransform,
            SqlExpression pattern,
            [NotNull] Func<SqlExpression, SqlExpression> patternTransform,
            SqlExpression startIndex)
        {
            // LOCATE('foo', 'barfoobar') - 1
            // Using an empty pattern `LOCATE('', 'barfoobar') - 1` returns 0.
            return _sqlExpressionFactory.Subtract(
                Locate(patternTransform(pattern), targetTransform(target), startIndex),
                _sqlExpressionFactory.Constant(1));
        }

        private static bool TryGetExpressionValue<T>(SqlExpression expression, out T value)
        {
            if (expression.Type != typeof(T))
            {
                throw new ArgumentException(
                    XGStrings.ExpressionTypeMismatch,
                    nameof(expression)
                );
            }

            if (expression is SqlConstantExpression constant)
            {
                value = (T)constant.Value;
                return true;
            }

            value = default;
            return false;
        }

        private static SqlExpression CreateExpressionForCaseSensitivity(
            StringComparison cmp,
            Func<SqlExpression> ifCaseSensitive,
            Func<SqlExpression> ifCaseInsensitive)
            => cmp switch
            {
                StringComparison.Ordinal => ifCaseSensitive(),
                StringComparison.CurrentCulture => ifCaseSensitive(),
                StringComparison.InvariantCulture => ifCaseSensitive(),
                StringComparison.OrdinalIgnoreCase => ifCaseInsensitive(),
                StringComparison.CurrentCultureIgnoreCase => ifCaseInsensitive(),
                StringComparison.InvariantCultureIgnoreCase => ifCaseInsensitive(),
                _ => default
            };

        private SqlExpression LCase(SqlExpression value)
            => _sqlExpressionFactory.NullableFunction(
                "LCASE",
                new[] {value},
                value.Type);

        private SqlExpression Utf8Bin(SqlExpression value)
            => _sqlExpressionFactory.Collate(
                value,
                "utf8mb4",
                "utf8mb4_bin"
            );

        private SqlExpression CharLength(SqlExpression value)
            => _sqlExpressionFactory.NullableFunction(
                "CHAR_LENGTH",
                new[] {value},
                typeof(int));

        private SqlExpression Locate(SqlExpression sub, SqlExpression str, SqlExpression startIndex = null)
        {
            var args = startIndex switch
            {
                null => new SqlExpression[] { sub, str },
                SqlConstantExpression { Value:int idx } => new SqlExpression[] { sub, str, _sqlExpressionFactory.Constant(idx + 1) },
                _ => new SqlExpression[] { sub, str, _sqlExpressionFactory.Add(startIndex, _sqlExpressionFactory.Constant(1)) }
            };
            return _sqlExpressionFactory.NullableFunction("LOCATE", args, typeof(int));
        }

        private const char LikeEscapeChar = '\\';

        private static bool IsLikeWildOrEscapeChar(char c) => IsLikeWildChar(c) || LikeEscapeChar == c;
        private static bool IsLikeWildChar(char c) => c is '%' or '_';

        private static string EscapeLikePattern(string pattern)
        {
            var builder = new StringBuilder();
            foreach (var c in pattern)
            {
                if (IsLikeWildOrEscapeChar(c))
                {
                    builder.Append(LikeEscapeChar);
                }

                builder.Append(c);
            }

            return builder.ToString();
        }

        private static string ConstructLikePatternParameter(
            QueryContext queryContext,
            string baseParameterName,
            StartsEndsWithContains methodType)
            => queryContext.ParameterValues[baseParameterName] switch
            {
                null => null,

                // In .NET, all strings start/end with the empty string, but SQL LIKE return false for empty patterns.
                // Return % which always matches instead.
                "" => "%",

                string s => methodType switch
                {
                    StartsEndsWithContains.StartsWith => EscapeLikePattern(s) + '%',
                    StartsEndsWithContains.EndsWith => '%' + EscapeLikePattern(s),
                    StartsEndsWithContains.Contains => $"%{EscapeLikePattern(s)}%",
                    _ => throw new ArgumentOutOfRangeException(nameof(methodType), methodType, null)
                },

                _ => throw new UnreachableException()
            };

        protected enum StartsEndsWithContains
        {
            /// <summary>
            /// StartsWith => LIKE 'foo%'
            /// </summary>
            StartsWith,

            /// <summary>
            /// EndsWith => LIKE '%foo'
            /// </summary>
            EndsWith,

            /// <summary>
            /// Contains => LIKE '%foo%'
            /// </summary>
            Contains
        }
    }
}
