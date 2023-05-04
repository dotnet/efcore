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
public class SqlServerSqlNullabilityProcessor : SqlNullabilityProcessor
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerSqlNullabilityProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies,
        bool useRelationalNulls)
        : base(dependencies, useRelationalNulls)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override SqlExpression VisitCustomSqlExpression(
        SqlExpression sqlExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
        => sqlExpression switch
        {
            SqlServerAggregateFunctionExpression aggregateFunctionExpression
                => VisitSqlServerAggregateFunction(aggregateFunctionExpression, allowOptimizedExpansion, out nullable),

            _ => base.VisitCustomSqlExpression(sqlExpression, allowOptimizedExpansion, out nullable)
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlExpression VisitSqlServerAggregateFunction(
        SqlServerAggregateFunctionExpression aggregateFunctionExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
    {
        nullable = aggregateFunctionExpression.IsNullable;

        SqlExpression[]? arguments = null;
        for (var i = 0; i < aggregateFunctionExpression.Arguments.Count; i++)
        {
            var visitedArgument = Visit(aggregateFunctionExpression.Arguments[i], out _);
            if (visitedArgument != aggregateFunctionExpression.Arguments[i] && arguments is null)
            {
                arguments = new SqlExpression[aggregateFunctionExpression.Arguments.Count];

                for (var j = 0; j < i; j++)
                {
                    arguments[j] = aggregateFunctionExpression.Arguments[j];
                }
            }

            if (arguments is not null)
            {
                arguments[i] = visitedArgument;
            }
        }

        OrderingExpression[]? orderings = null;
        for (var i = 0; i < aggregateFunctionExpression.Orderings.Count; i++)
        {
            var ordering = aggregateFunctionExpression.Orderings[i];
            var visitedOrdering = ordering.Update(Visit(ordering.Expression, out _));
            if (visitedOrdering != aggregateFunctionExpression.Orderings[i] && orderings is null)
            {
                orderings = new OrderingExpression[aggregateFunctionExpression.Orderings.Count];

                for (var j = 0; j < i; j++)
                {
                    orderings[j] = aggregateFunctionExpression.Orderings[j];
                }
            }

            if (orderings is not null)
            {
                orderings[i] = visitedOrdering;
            }
        }

        return arguments is not null || orderings is not null
            ? aggregateFunctionExpression.Update(
                arguments ?? aggregateFunctionExpression.Arguments,
                orderings ?? aggregateFunctionExpression.Orderings)
            : aggregateFunctionExpression;
    }
}
