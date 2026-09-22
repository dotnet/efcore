// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     A factory for creating <see cref="RelationalParameterBasedSqlProcessor" /> instances.
/// </summary>
/// <remarks>
///     The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///     <see cref="DbContext" /> instance will use its own instance of this service.
///     The implementation may depend on other services registered with any lifetime.
///     The implementation does not need to be thread-safe.
/// </remarks>
public interface IRelationalParameterBasedSqlProcessorFactory
{
    /// <summary>
    ///     Creates a new <see cref="RelationalParameterBasedSqlProcessor" />.
    /// </summary>
    /// <param name="parameters">Parameters for <see cref="RelationalParameterBasedSqlProcessor" />.</param>
    /// <returns>A relational parameter based sql processor.</returns>
    RelationalParameterBasedSqlProcessor Create(RelationalParameterBasedSqlProcessorParameters parameters);
}
