// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteStringAggregateMethodTranslator(ISqlExpressionFactory sqlExpressionFactory) : IAggregateMethodCallTranslator
{
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
            || method.DeclaringType != typeof(string))
        {
            return null;
        }

        SqlExpression separator;
        switch (method.Name)
        {
            case nameof(string.Concat) when arguments is []:
                separator = sqlExpressionFactory.Constant(string.Empty, typeof(string));
                break;
            case nameof(string.Join) when arguments is [var sep]:
                separator = sep;
                break;
            default:
                return null;
        }

        // SQLite does not support input ordering on aggregate methods. Since ordering matters very much for translating, if the user
        // specified an ordering we refuse to translate (but to error than to ignore in this case).
        if (source.Orderings.Count > 0)
        {
            return null;
        }

        sqlExpression = sqlExpressionFactory.Coalesce(
            sqlExpression,
            sqlExpressionFactory.Constant(string.Empty, typeof(string)));

        if (source.Predicate != null)
        {
            if (sqlExpression is SqlFragmentExpression)
            {
                sqlExpression = sqlExpressionFactory.Constant(1);
            }

            sqlExpression = sqlExpressionFactory.Case(
                new List<CaseWhenClause> { new(source.Predicate, sqlExpression) },
                elseResult: null);
        }

        if (source.IsDistinct)
        {
            sqlExpression = new DistinctExpression(sqlExpression);
        }

        // group_concat returns null when there are no rows (or non-null values), but string.Join returns an empty string.
        return sqlExpressionFactory.Coalesce(
            sqlExpressionFactory.Function(
                "group_concat",
                [
                    sqlExpression,
                    sqlExpressionFactory.ApplyTypeMapping(separator, sqlExpression.TypeMapping)
                ],
                nullable: true,
                argumentsPropagateNullability: Statics.FalseArrays[2],
                typeof(string)),
            sqlExpressionFactory.Constant(string.Empty, typeof(string)),
            sqlExpression.TypeMapping);
    }
}
