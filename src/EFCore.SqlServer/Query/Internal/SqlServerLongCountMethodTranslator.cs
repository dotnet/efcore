// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerLongCountMethodTranslator : IAggregateMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerLongCountMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (method.DeclaringType == typeof(Queryable)
            && method.IsGenericMethod
            && method.GetGenericMethodDefinition() is MethodInfo genericMethod
            && (genericMethod == QueryableMethods.LongCountWithoutPredicate
                || genericMethod == QueryableMethods.LongCountWithPredicate))
        {
            var sqlExpression = (source.Selector as SqlExpression) ?? _sqlExpressionFactory.Fragment("*");
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

            return _sqlExpressionFactory.ApplyDefaultTypeMapping(
                _sqlExpressionFactory.Function(
                    "COUNT_BIG",
                    new[] { sqlExpression },
                    nullable: false,
                    argumentsPropagateNullability: new[] { false },
                    typeof(long)));
        }

        return null;
    }
}
