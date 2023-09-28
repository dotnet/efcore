// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     A service typically implemented by database providers to reverse engineer a database into
///     a <see cref="DatabaseModel" />.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-scaffolding">Reverse engineering (scaffolding) an existing database</see>, and
///         <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
///     </para>
/// </remarks>
public interface IDatabaseModelFactory
{
    /// <summary>
    ///     Connects to the database using the given connection string and creates a <see cref="DatabaseModel" />
    ///     for the database.
    /// </summary>
    /// <param name="connectionString">The connection string for the database to reverse engineer.</param>
    /// <param name="options">The options specifying which metadata to read.</param>
    /// <returns>The database model.</returns>
    DatabaseModel Create(string connectionString, DatabaseModelFactoryOptions options);

    /// <summary>
    ///     Connects to the database using the given connection and creates a <see cref="DatabaseModel" />
    ///     for the database.
    /// </summary>
    /// <param name="connection">The connection to the database to reverse engineer.</param>
    /// <param name="options">The options specifying which metadata to read.</param>
    /// <returns>The database model.</returns>
    DatabaseModel Create(DbConnection connection, DatabaseModelFactoryOptions options);
}
