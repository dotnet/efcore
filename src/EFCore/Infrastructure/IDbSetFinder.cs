// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     A service for finding <see cref="DbSet{TEntity}" /> properties on a type that inherits from <see cref="DbContext" />.
/// </summary>
/// <remarks>
///     The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///     is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///     This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
/// </remarks>
public interface IDbSetFinder
{
    /// <summary>
    ///     Finds <see cref="DbSet{TEntity}" /> properties on a type that inherits from <see cref="DbContext" />.
    /// </summary>
    /// <param name="contextType">A type that inherits from <see cref="DbContext" /></param>
    /// <returns>A list of the found properties.</returns>
    IReadOnlyList<DbSetProperty> FindSets(Type contextType);
}
