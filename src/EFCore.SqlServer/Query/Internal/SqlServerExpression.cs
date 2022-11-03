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
public static class SqlServerExpression
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SqlFunctionExpression AggregateFunction(
        ISqlExpressionFactory sqlExpressionFactory,
        string name,
        IEnumerable<SqlExpression> arguments,
        EnumerableExpression enumerableExpression,
        int enumerableArgumentIndex,
        bool nullable,
        IEnumerable<bool> argumentsPropagateNullability,
        Type returnType,
        RelationalTypeMapping? typeMapping = null)
        => new(
            name,
            ProcessAggregateFunctionArguments(sqlExpressionFactory, arguments, enumerableExpression, enumerableArgumentIndex),
            nullable,
            argumentsPropagateNullability,
            returnType,
            typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SqlExpression AggregateFunctionWithOrdering(
        ISqlExpressionFactory sqlExpressionFactory,
        string name,
        IEnumerable<SqlExpression> arguments,
        EnumerableExpression enumerableExpression,
        int enumerableArgumentIndex,
        bool nullable,
        IEnumerable<bool> argumentsPropagateNullability,
        Type returnType,
        RelationalTypeMapping? typeMapping = null)
        => enumerableExpression.Orderings.Count == 0
            ? AggregateFunction(
                sqlExpressionFactory, name, arguments, enumerableExpression, enumerableArgumentIndex, nullable,
                argumentsPropagateNullability, returnType, typeMapping)
            : new SqlServerAggregateFunctionExpression(
                name,
                ProcessAggregateFunctionArguments(sqlExpressionFactory, arguments, enumerableExpression, enumerableArgumentIndex),
                enumerableExpression.Orderings,
                nullable,
                argumentsPropagateNullability,
                returnType,
                typeMapping);

    private static IReadOnlyList<SqlExpression> ProcessAggregateFunctionArguments(
        ISqlExpressionFactory sqlExpressionFactory,
        IEnumerable<SqlExpression> arguments,
        EnumerableExpression enumerableExpression,
        int enumerableArgumentIndex)
    {
        var argIndex = 0;
        var typeMappedArguments = new List<SqlExpression>();

        foreach (var argument in arguments)
        {
            var modifiedArgument = sqlExpressionFactory.ApplyDefaultTypeMapping(argument);

            if (argIndex == enumerableArgumentIndex)
            {
                // This is the argument representing the enumerable inputs to be aggregated.
                // Wrap it with a CASE/WHEN for the predicate and with DISTINCT, if necessary.
                if (enumerableExpression.Predicate != null)
                {
                    modifiedArgument = sqlExpressionFactory.Case(
                        new List<CaseWhenClause> { new(enumerableExpression.Predicate, modifiedArgument) },
                        elseResult: null);
                }

                if (enumerableExpression.IsDistinct)
                {
                    modifiedArgument = new DistinctExpression(modifiedArgument);
                }
            }

            typeMappedArguments.Add(modifiedArgument);

            argIndex++;
        }

        return typeMappedArguments;
    }
}
