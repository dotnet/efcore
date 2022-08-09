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
    Expression QueryCompilationStarting(
        Expression queryExpression,
        QueryExpressionEventData eventData)
        => queryExpression;
}
