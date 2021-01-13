// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     <para>
    ///         A service for generating migration identifiers from names and names from identifiers.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IMigrationsIdGenerator
    {
        /// <summary>
        ///     Generates an identifier given a migration name.
        /// </summary>
        /// <param name="name"> The migration name. </param>
        /// <returns> The identifier. </returns>
        string GenerateId([NotNull] string name);

        /// <summary>
        ///     Gets a migration name based on the given identifier.
        /// </summary>
        /// <param name="id"> The migration identifier. </param>
        /// <returns> The migration name. </returns>
        string GetName([NotNull] string id);

        /// <summary>
        ///     Checks whether or not the given string is a valid migration identifier.
        /// </summary>
        /// <param name="value"> The candidate string. </param>
        /// <returns> <see langword="true" /> if the string is a valid migration identifier; <see langword="false" /> otherwise. </returns>
        bool IsValidId([NotNull] string value);
    }
}
