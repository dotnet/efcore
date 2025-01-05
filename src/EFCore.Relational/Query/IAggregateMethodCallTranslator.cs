// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         A SQL translator for LINQ <see cref="MethodCallExpression" /> expression representing an aggregate function.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public interface IAggregateMethodCallTranslator
{
    /// <summary>
    ///     Translates a LINQ <see cref="MethodCallExpression" /> to a SQL equivalent.
    /// </summary>
    /// <param name="method">The method info from <see cref="MethodCallExpression.Method" />.</param>
    /// <param name="source">The source on which the aggregate method is applied.</param>
    /// <param name="arguments">SQL representations of scalar <see cref="MethodCallExpression.Arguments" />.</param>
    /// <param name="logger">The query logger to use.</param>
    /// <returns>A SQL translation of the <see cref="MethodCallExpression" />.</returns>
    SqlExpression? Translate(
        MethodInfo method,
        EnumerableExpression source,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger);
}
