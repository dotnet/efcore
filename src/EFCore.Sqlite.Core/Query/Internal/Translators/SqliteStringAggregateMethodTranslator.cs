// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;
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
    // group_concat supports an in-function ORDER BY clause since SQLite 3.44.0.
    private readonly bool _isOrderedAggregateSupported
        = new Version(new SqliteConnection().ServerVersion) >= new Version(3, 44);

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

        // SQLite's group_concat() accepts only a single argument when DISTINCT is used, so it cannot be combined
        // with the separator that string.Join/Concat always supply ("DISTINCT aggregates must have exactly one
        // argument"). In-aggregate ORDER BY additionally requires SQLite 3.44.0. Fall back to client evaluation
        // rather than emit SQL that fails at execution time.
        if (source.IsDistinct
            || (source.Orderings.Count > 0 && !_isOrderedAggregateSupported))
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

        var functionArguments = new[]
        {
            sqlExpression,
            sqlExpressionFactory.ApplyTypeMapping(separator, sqlExpression.TypeMapping)
        };

        // SQLite supports ORDER BY inside aggregate functions since 3.44.0: group_concat(value, separator ORDER BY ...).
        // When the user specified an ordering we emit our custom expression that renders it; otherwise a plain function call.
        SqlExpression aggregate = source.Orderings.Count == 0
            ? sqlExpressionFactory.Function(
                "group_concat",
                functionArguments,
                nullable: true,
                argumentsPropagateNullability: Statics.FalseArrays[2],
                typeof(string))
            : new SqliteAggregateFunctionExpression(
                "group_concat",
                functionArguments,
                source.Orderings,
                nullable: true,
                argumentsPropagateNullability: Statics.FalseArrays[2],
                typeof(string),
                sqlExpression.TypeMapping);

        // group_concat returns null when there are no rows (or non-null values), but string.Join returns an empty string.
        return sqlExpressionFactory.Coalesce(
            aggregate,
            sqlExpressionFactory.Constant(string.Empty, typeof(string)),
            sqlExpression.TypeMapping);
    }
}
