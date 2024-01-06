// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteStringAggregateMethodTranslator : IAggregateMethodCallTranslator
{
    private static readonly MethodInfo StringConcatMethod
        = typeof(string).GetRuntimeMethod(nameof(string.Concat), [typeof(IEnumerable<string>)])!;

    private static readonly MethodInfo StringJoinMethod
        = typeof(string).GetRuntimeMethod(nameof(string.Join), [typeof(string), typeof(IEnumerable<string>)])!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteStringAggregateMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        MethodInfo method,
        EnumerableExpression source,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        // Docs: https://sqlite.org/lang_aggfunc.html#group_concat

        if (source.Selector is not SqlExpression sqlExpression
            || (method != StringJoinMethod && method != StringConcatMethod))
        {
            return null;
        }

        // SQLite does not support input ordering on aggregate methods. Since ordering matters very much for translating, if the user
        // specified an ordering we refuse to translate (but to error than to ignore in this case).
        if (source.Orderings.Count > 0)
        {
            return null;
        }

        if (sqlExpression is not ColumnExpression { IsNullable: false })
        {
            sqlExpression = _sqlExpressionFactory.Coalesce(
                sqlExpression,
                _sqlExpressionFactory.Constant(string.Empty, typeof(string)));
        }

        if (source.Predicate != null)
        {
            if (sqlExpression is SqlFragmentExpression)
            {
                sqlExpression = _sqlExpressionFactory.Constant(1);
            }

            sqlExpression = _sqlExpressionFactory.Case(
                new List<CaseWhenClause> { new(source.Predicate, sqlExpression) },
                elseResult: null);
        }

        if (source.IsDistinct)
        {
            sqlExpression = new DistinctExpression(sqlExpression);
        }

        // group_concat returns null when there are no rows (or non-null values), but string.Join returns an empty string.
        return _sqlExpressionFactory.Coalesce(
            _sqlExpressionFactory.Function(
                "group_concat",
                new[]
                {
                    sqlExpression,
                    _sqlExpressionFactory.ApplyTypeMapping(
                        method == StringJoinMethod ? arguments[0] : _sqlExpressionFactory.Constant(string.Empty, typeof(string)),
                        sqlExpression.TypeMapping)
                },
                nullable: true,
                argumentsPropagateNullability: new[] { false, true },
                typeof(string)),
            _sqlExpressionFactory.Constant(string.Empty, typeof(string)),
            sqlExpression.TypeMapping);
    }
}
