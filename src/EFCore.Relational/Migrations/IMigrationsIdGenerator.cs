// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     A service for generating migration identifiers from names and names from identifiers.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
///     </para>
/// </remarks>
public interface IMigrationsIdGenerator
{
    /// <summary>
    ///     Generates an identifier given a migration name.
    /// </summary>
    /// <param name="name">The migration name.</param>
    /// <returns>The identifier.</returns>
    string GenerateId(string name);

    /// <summary>
    ///     Gets a migration name based on the given identifier.
    /// </summary>
    /// <param name="id">The migration identifier.</param>
    /// <returns>The migration name.</returns>
    string GetName(string id);

    /// <summary>
    ///     Checks whether or not the given string is a valid migration identifier.
    /// </summary>
    /// <param name="value">The candidate string.</param>
    /// <returns><see langword="true" /> if the string is a valid migration identifier; <see langword="false" /> otherwise.</returns>
    bool IsValidId(string value);
}
