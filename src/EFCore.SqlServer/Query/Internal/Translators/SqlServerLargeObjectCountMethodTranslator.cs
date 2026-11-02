// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerLargeObjectCountMethodTranslator : IAggregateMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerLargeObjectCountMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

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
        // COUNT doesn't accept the legacy text, ntext and image types, but IS NOT NULL does, so count the non-null values through a
        // CASE instead. DISTINCT is left alone, since these types can't be compared either.
        if (method.DeclaringType == typeof(Queryable)
            && method.IsGenericMethod
            && method.GetGenericMethodDefinition() is { } genericMethod
            && !source.IsDistinct
            && source.Selector is SqlExpression { TypeMapping: { } typeMapping } selector
            && IsLargeObjectType(typeMapping.StoreTypeNameBase))
        {
            var isLongCount = genericMethod == QueryableMethods.LongCountWithoutPredicate
                || genericMethod == QueryableMethods.LongCountWithPredicate;

            if (!isLongCount
                && genericMethod != QueryableMethods.CountWithoutPredicate
                && genericMethod != QueryableMethods.CountWithPredicate)
            {
                return null;
            }

            var predicate = _sqlExpressionFactory.IsNotNull(selector);
            if (source.Predicate != null)
            {
                predicate = _sqlExpressionFactory.AndAlso(source.Predicate, predicate);
            }

            var countSqlExpression = _sqlExpressionFactory.Case(
                [new CaseWhenClause(predicate, _sqlExpressionFactory.Constant(1))],
                elseResult: null);

            return _sqlExpressionFactory.ApplyDefaultTypeMapping(
                _sqlExpressionFactory.Function(
                    isLongCount ? "COUNT_BIG" : "COUNT",
                    [countSqlExpression],
                    nullable: false,
                    argumentsPropagateNullability: Statics.FalseArrays[1],
                    isLongCount ? typeof(long) : typeof(int)));
        }

        return null;
    }

    // The store type keeps the spelling from HasColumnType, which SQL Server matches regardless of case or brackets.
    private static bool IsLargeObjectType(string storeTypeNameBase)
    {
        var name = storeTypeNameBase.Trim();
        if (name.StartsWith('[') && name.EndsWith(']'))
        {
            name = name[1..^1];
        }

        return name.Equals("text", StringComparison.OrdinalIgnoreCase)
            || name.Equals("ntext", StringComparison.OrdinalIgnoreCase)
            || name.Equals("image", StringComparison.OrdinalIgnoreCase);
    }
}
