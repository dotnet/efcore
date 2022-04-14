// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Provides translations for LINQ <see cref="MemberExpression" /> expressions.
/// </summary>
/// <remarks>
///     The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///     <see cref="DbContext" /> instance will use its own instance of this service.
///     The implementation may depend on other services registered with any lifetime.
///     The implementation does not need to be thread-safe.
/// </remarks>
public interface IMemberTranslatorProvider
{
    /// <summary>
    ///     Translates a LINQ <see cref="MemberExpression" /> to a SQL equivalent.
    /// </summary>
    /// <param name="instance">A SQL representation of <see cref="MemberExpression.Expression" />.</param>
    /// <param name="member">The member info from <see cref="MemberExpression.Member" />.</param>
    /// <param name="returnType">The return type from <see cref="Expression.Type" />.</param>
    /// <param name="logger">The query logger to use.</param>
    /// <returns>A SQL translation of the <see cref="MemberExpression" />.</returns>
    SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger);
}
