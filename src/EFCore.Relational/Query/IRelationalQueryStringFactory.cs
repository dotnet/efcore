// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         Implemented by database providers to generate the query string for <see cref="EntityFrameworkQueryableExtensions.ToQueryString" />.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///     <see cref="DbContext" /> instance will use its own instance of this service.
///     The implementation may depend on other services registered with any lifetime.
///     The implementation does not need to be thread-safe.
/// </remarks>
public interface IRelationalQueryStringFactory
{
    /// <summary>
    ///     Returns a formatted query string for the given command.
    /// </summary>
    /// <param name="command">The command that represents the query.</param>
    /// <returns>The formatted string.</returns>
    string Create(DbCommand command);
}
