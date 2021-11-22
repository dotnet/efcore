// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     A factory for creating <see cref="QuerySqlGenerator" /> instances.
/// </summary>
/// <remarks>
///     The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///     is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///     This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
/// </remarks>
public interface IQuerySqlGeneratorFactory
{
    /// <summary>
    ///     Creates a new <see cref="QuerySqlGenerator" />.
    /// </summary>
    /// <returns>A SQL generator.</returns>
    QuerySqlGenerator Create();
}
