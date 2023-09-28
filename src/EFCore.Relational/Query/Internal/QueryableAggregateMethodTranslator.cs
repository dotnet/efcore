// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class QueryableAggregateMethodTranslator : IAggregateMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public QueryableAggregateMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (method.DeclaringType == typeof(Queryable))
        {
            var methodInfo = method.IsGenericMethod
                ? method.GetGenericMethodDefinition()
                : method;
            switch (methodInfo.Name)
            {
                case nameof(Queryable.Average)
                    when (QueryableMethods.IsAverageWithoutSelector(methodInfo)
                        || QueryableMethods.IsAverageWithSelector(methodInfo))
                    && source.Selector is SqlExpression averageSqlExpression:
                    var averageInputType = averageSqlExpression.Type;
                    if (averageInputType == typeof(int)
                        || averageInputType == typeof(long))
                    {
                        averageSqlExpression = _sqlExpressionFactory.ApplyDefaultTypeMapping(
                            _sqlExpressionFactory.Convert(averageSqlExpression, typeof(double)));
                    }

                    averageSqlExpression = CombineTerms(source, averageSqlExpression);
                    return averageInputType == typeof(float)
                        ? _sqlExpressionFactory.Convert(
                            _sqlExpressionFactory.Function(
                                "AVG",
                                new[] { averageSqlExpression },
                                nullable: true,
                                argumentsPropagateNullability: new[] { false },
                                typeof(double)),
                            averageSqlExpression.Type,
                            averageSqlExpression.TypeMapping)
                        : _sqlExpressionFactory.Function(
                            "AVG",
                            new[] { averageSqlExpression },
                            nullable: true,
                            argumentsPropagateNullability: new[] { false },
                            averageSqlExpression.Type,
                            averageSqlExpression.TypeMapping);

                // Count/LongCount are special since if the argument is a star fragment, it needs to be transformed to any non-null constant
                // when a predicate is applied.
                case nameof(Queryable.Count)
                    when methodInfo == QueryableMethods.CountWithoutPredicate
                    || methodInfo == QueryableMethods.CountWithPredicate:
                    var countSqlExpression = (source.Selector as SqlExpression) ?? _sqlExpressionFactory.Fragment("*");
                    countSqlExpression = CombineTerms(source, countSqlExpression);
                    return _sqlExpressionFactory.Function(
                        "COUNT",
                        new[] { countSqlExpression },
                        nullable: false,
                        argumentsPropagateNullability: new[] { false },
                        typeof(int));

                case nameof(Queryable.LongCount)
                    when methodInfo == QueryableMethods.LongCountWithoutPredicate
                    || methodInfo == QueryableMethods.LongCountWithPredicate:
                    var longCountSqlExpression = (source.Selector as SqlExpression) ?? _sqlExpressionFactory.Fragment("*");
                    longCountSqlExpression = CombineTerms(source, longCountSqlExpression);
                    return _sqlExpressionFactory.Function(
                        "COUNT",
                        new[] { longCountSqlExpression },
                        nullable: false,
                        argumentsPropagateNullability: new[] { false },
                        typeof(long));

                case nameof(Queryable.Max)
                    when (methodInfo == QueryableMethods.MaxWithoutSelector
                        || methodInfo == QueryableMethods.MaxWithSelector)
                    && source.Selector is SqlExpression maxSqlExpression:
                    maxSqlExpression = CombineTerms(source, maxSqlExpression);
                    return _sqlExpressionFactory.Function(
                        "MAX",
                        new[] { maxSqlExpression },
                        nullable: true,
                        argumentsPropagateNullability: new[] { false },
                        maxSqlExpression.Type,
                        maxSqlExpression.TypeMapping);

                case nameof(Queryable.Min)
                    when (methodInfo == QueryableMethods.MinWithoutSelector
                        || methodInfo == QueryableMethods.MinWithSelector)
                    && source.Selector is SqlExpression minSqlExpression:
                    minSqlExpression = CombineTerms(source, minSqlExpression);
                    return _sqlExpressionFactory.Function(
                        "MIN",
                        new[] { minSqlExpression },
                        nullable: true,
                        argumentsPropagateNullability: new[] { false },
                        minSqlExpression.Type,
                        minSqlExpression.TypeMapping);

                case nameof(Queryable.Sum)
                    when (QueryableMethods.IsSumWithoutSelector(methodInfo)
                        || QueryableMethods.IsSumWithSelector(methodInfo))
                    && source.Selector is SqlExpression sumSqlExpression:
                    sumSqlExpression = CombineTerms(source, sumSqlExpression);
                    var sumInputType = sumSqlExpression.Type;
                    return sumInputType == typeof(float)
                        ? _sqlExpressionFactory.Convert(
                            _sqlExpressionFactory.Function(
                                "SUM",
                                new[] { sumSqlExpression },
                                nullable: true,
                                argumentsPropagateNullability: new[] { false },
                                typeof(double)),
                            sumInputType,
                            sumSqlExpression.TypeMapping)
                        : _sqlExpressionFactory.Function(
                            "SUM",
                            new[] { sumSqlExpression },
                            nullable: true,
                            argumentsPropagateNullability: new[] { false },
                            sumInputType,
                            sumSqlExpression.TypeMapping);
            }
        }

        return null;
    }

    private SqlExpression CombineTerms(EnumerableExpression enumerableExpression, SqlExpression sqlExpression)
    {
        if (enumerableExpression.Predicate != null)
        {
            if (sqlExpression is SqlFragmentExpression)
            {
                sqlExpression = _sqlExpressionFactory.Constant(1);
            }

            sqlExpression = _sqlExpressionFactory.Case(
                new List<CaseWhenClause> { new(enumerableExpression.Predicate, sqlExpression) },
                elseResult: null);
        }

        if (enumerableExpression.IsDistinct)
        {
            sqlExpression = new DistinctExpression(sqlExpression);
        }

        return sqlExpression;
    }
}
