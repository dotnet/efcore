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
    /// <param name="parameters">Parameter object containing parameters for this class.</param>
    public RelationalParameterBasedSqlProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies,
        RelationalParameterBasedSqlProcessorParameters parameters)
    {
        Dependencies = dependencies;
        Parameters = parameters;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalParameterBasedSqlProcessorDependencies Dependencies { get; }

    /// <summary>
    ///     Parameter object containing parameters for this class.
    /// </summary>
    protected virtual RelationalParameterBasedSqlProcessorParameters Parameters { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual Expression Process(Expression queryExpression, Dictionary<string, object?> parameters, out bool canCache)
    {
        var parametersFacade = new CacheSafeParameterFacade(parameters);
        var result = Process(queryExpression, parametersFacade);
        canCache = parametersFacade.CanCache;

        return result;
    }

    /// <summary>
    ///     Performs final query processing that takes parameter values into account.
    /// </summary>
    /// <param name="queryExpression">A query expression to process.</param>
    /// <param name="parametersFacade">A facade allowing access to parameters in a cache-safe way.</param>
    public virtual Expression Process(Expression queryExpression, CacheSafeParameterFacade parametersFacade)
    {
        queryExpression = ProcessSqlNullability(queryExpression, parametersFacade);
        queryExpression = ExpandFromSqlParameter(queryExpression, parametersFacade);

        return queryExpression;
    }

    /// <summary>
    ///     Processes the query expression based on nullability of nodes to apply null semantics in use and
    ///     optimize it for given parameter values.
    /// </summary>
    /// <param name="queryExpression">A query expression to optimize.</param>
    /// <param name="parametersFacade">A facade allowing access to parameters in a cache-safe way.</param>
    /// <returns>A processed query expression.</returns>
    protected virtual Expression ProcessSqlNullability(Expression queryExpression, CacheSafeParameterFacade parametersFacade)
        => new SqlNullabilityProcessor(Dependencies, Parameters).Process(queryExpression, parametersFacade);

    /// <summary>
    ///     Expands the parameters to <see cref="FromSqlExpression" /> inside the query expression for given parameter values.
    /// </summary>
    /// <param name="queryExpression">A query expression to optimize.</param>
    /// <param name="parametersFacade">A facade allowing access to parameters in a cache-safe way.</param>
    /// <returns>A processed query expression.</returns>
    protected virtual Expression ExpandFromSqlParameter(Expression queryExpression, CacheSafeParameterFacade parametersFacade)
        => new RelationalParameterProcessor(Dependencies).Expand(queryExpression, parametersFacade);

    /// <summary>
    ///     Optimizes the query expression for given parameter values.
    /// </summary>
    /// <param name="queryExpression">A query expression to optimize.</param>
    /// <param name="parametersValues">A dictionary of parameter values to use.</param>
    /// <param name="canCache">A bool value indicating if the query expression can be cached.</param>
    /// <returns>An optimized query expression.</returns>
    [Obsolete("Override Process() instead", error: true)]
    public virtual Expression Optimize(
        Expression queryExpression,
        IReadOnlyDictionary<string, object?> parametersValues,
        out bool canCache)
        => throw new UnreachableException();
}
