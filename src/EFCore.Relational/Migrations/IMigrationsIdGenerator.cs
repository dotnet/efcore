// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     A service for generating migration identifiers from names and names from identifiers.
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
        /// <returns> <c>True</c> if the string is a valid migration identifier; <c>false</c> otherwise. </returns>
        bool IsValidId([NotNull] string value);
    }
}
