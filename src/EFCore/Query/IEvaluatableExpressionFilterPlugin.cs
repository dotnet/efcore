// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Represents a plugin evaluatable expression filter.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" /> and multiple registrations
///         are allowed. This means a single instance of each service is used by many <see cref="DbContext" />
///         instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
///     </para>
/// </remarks>
public interface IEvaluatableExpressionFilterPlugin
{
    /// <summary>
    ///     Checks whether the given expression can be evaluated.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns><see langword="true" /> if the expression can be evaluated; <see langword="false" /> otherwise.</returns>
    bool IsEvaluatableExpression(Expression expression);
}
