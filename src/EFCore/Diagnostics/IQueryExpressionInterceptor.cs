// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Allows interception of query expression trees and resulting compiled delegates.
/// </summary>
/// <remarks>
///     <para>
///         Use <see cref="DbContextOptionsBuilder.AddInterceptors(Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor[])" />
///         to register application interceptors.
///     </para>
///     <para>
///         Extensions can also register interceptors in the internal service provider.
///         If both injected and application interceptors are found, then the injected interceptors are run in the
///         order that they are resolved from the service provider, and then the application interceptors are run last.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see> for more information and examples.
///     </para>
/// </remarks>
public interface IQueryExpressionInterceptor : IInterceptor
{
    /// <summary>
    ///     Called with the LINQ expression tree for a query before it is compiled.
    /// </summary>
    /// <param name="queryExpression">The query expression.</param>
    /// <param name="eventData">Contextual information about the query environment.</param>
    /// <returns>The query expression tree to continue with, which may have been changed by the interceptor.</returns>
    Expression ProcessingQuery(
        Expression queryExpression,
        QueryExpressionEventData eventData)
        => queryExpression;

    /// <summary>
    ///     Called when EF is about to compile the query delegate that will be used to execute the query.
    /// </summary>
    /// <param name="queryExpression">The query expression.</param>
    /// <param name="queryExecutorExpression">The expression that will be compiled into the execution delegate.</param>
    /// <param name="eventData">Contextual information about the query environment.</param>
    /// <typeparam name="TResult">The return type of the execution delegate.</typeparam>
    /// <returns>The expression that will be compiled into the execution delegate, which may have been changed by the interceptor.</returns>
    Expression<Func<QueryContext, TResult>> CompilingQuery<TResult>(
        Expression queryExpression,
        Expression<Func<QueryContext, TResult>> queryExecutorExpression,
        QueryExpressionEventData eventData)
        => queryExecutorExpression;

    /// <summary>
    ///     Called when EF is about to compile the query delegate that will be used to execute the query.
    /// </summary>
    /// <param name="queryExpression">The query expression.</param>
    /// <param name="eventData">Contextual information about the query environment.</param>
    /// <param name="queryExecutor">The delegate that will be used to execute the query.</param>
    /// <typeparam name="TResult">The return type of the execution delegate.</typeparam>
    /// <returns>The delegate that will be used to execute the query, which may have been changed by the interceptor.</returns>
    Func<QueryContext, TResult> CompiledQuery<TResult>(
        Expression queryExpression,
        QueryExpressionEventData eventData,
        Func<QueryContext, TResult> queryExecutor)
        => queryExecutor;
}
