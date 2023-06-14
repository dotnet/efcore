// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         A class that processes the query expression after parameter values are known.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class RelationalParameterBasedSqlProcessor
{
    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalParameterBasedSqlProcessor" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="useRelationalNulls">A bool value indicating if relational nulls should be used.</param>
    public RelationalParameterBasedSqlProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies,
        bool useRelationalNulls)
    {
        Dependencies = dependencies;
        UseRelationalNulls = useRelationalNulls;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalParameterBasedSqlProcessorDependencies Dependencies { get; }

    /// <summary>
    ///     A bool value indicating if relational nulls should be used.
    /// </summary>
    protected virtual bool UseRelationalNulls { get; }

    /// <summary>
    ///     Optimizes the query expression for given parameter values.
    /// </summary>
    /// <param name="queryExpression">A query expression to optimize.</param>
    /// <param name="parametersValues">A dictionary of parameter values to use.</param>
    /// <param name="canCache">A bool value indicating if the query expression can be cached.</param>
    /// <returns>An optimized query expression.</returns>
    public virtual Expression Optimize(
        Expression queryExpression,
        IReadOnlyDictionary<string, object?> parametersValues,
        out bool canCache)
    {
        canCache = true;
        queryExpression = ProcessSqlNullability(queryExpression, parametersValues, out var sqlNullabilityCanCache);
        canCache &= sqlNullabilityCanCache;

        queryExpression = ExpandFromSqlParameter(queryExpression, parametersValues, out var fromSqlParameterCanCache);
        canCache &= fromSqlParameterCanCache;

        return queryExpression;
    }

    /// <summary>
    ///     Processes the query expression based on nullability of nodes to apply null semantics in use and
    ///     optimize it for given parameter values.
    /// </summary>
    /// <param name="queryExpression">A query expression to optimize.</param>
    /// <param name="parametersValues">A dictionary of parameter values to use.</param>
    /// <param name="canCache">A bool value indicating if the query expression can be cached.</param>
    /// <returns>A processed query expression.</returns>
    protected virtual Expression ProcessSqlNullability(
        Expression queryExpression,
        IReadOnlyDictionary<string, object?> parametersValues,
        out bool canCache)
        => new SqlNullabilityProcessor(Dependencies, UseRelationalNulls).Process(queryExpression, parametersValues, out canCache);

    /// <summary>
    ///     Expands the parameters to <see cref="FromSqlExpression" /> inside the query expression for given parameter values.
    /// </summary>
    /// <param name="queryExpression">A query expression to optimize.</param>
    /// <param name="parametersValues">A dictionary of parameter values to use.</param>
    /// <param name="canCache">A bool value indicating if the query expression can be cached.</param>
    /// <returns>A processed query expression.</returns>
    protected virtual Expression ExpandFromSqlParameter(
        Expression queryExpression,
        IReadOnlyDictionary<string, object?> parametersValues,
        out bool canCache)
        => new FromSqlParameterExpandingExpressionVisitor(Dependencies).Expand(queryExpression, parametersValues, out canCache);
}
