// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Provides translations for LINQ <see cref="MethodCallExpression" /> expressions which represents aggregate methods.
/// </summary>
/// <remarks>
///     The service lifetime is <see cref="ServiceLifetime.Scoped" /> and multiple registrations
///     are allowed. This means that each <see cref="DbContext" /> instance will use its own
///     set of instances of this service.
///     The implementations may depend on other services registered with any lifetime.
///     The implementations do not need to be thread-safe.
/// </remarks>
public interface IAggregateMethodCallTranslatorProvider
{
    /// <summary>
    ///     Translates a LINQ aggregate <see cref="MethodCallExpression" /> to a SQL equivalent.
    /// </summary>
    /// <param name="model">A model to use for translation.</param>
    /// <param name="method">The method info from <see cref="MethodCallExpression.Method" />.</param>
    /// <param name="source">The source on which the aggregate method is applied.</param>
    /// <param name="arguments">SQL representations of scalar <see cref="MethodCallExpression.Arguments" />.</param>
    /// <param name="logger">The query logger to use.</param>
    /// <returns>A SQL translation of the <see cref="MethodCallExpression" />.</returns>
    SqlExpression? Translate(
        IModel model,
        MethodInfo method,
        EnumerableExpression source,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger);
}
