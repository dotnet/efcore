// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Cosmos-specific extension methods for metadata.
    /// </summary>
    public static class CosmosMetadataExtensions
    {
        /// <summary>
        ///     Gets the Cosmos-specific metadata for an entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get metadata for. </param>
        /// <returns> The Cosmos-specific metadata for the entity type. </returns>
        public static ICosmosEntityTypeAnnotations Cosmos(this IEntityType entityType)
            => new CosmosEntityTypeAnnotations(entityType);

        /// <summary>
        ///     Gets the Cosmos-specific metadata for an entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get metadata for. </param>
        /// <returns> The Cosmos-specific metadata for the entity type. </returns>
        public static CosmosEntityTypeAnnotations Cosmos(this IMutableEntityType entityType)
            => (CosmosEntityTypeAnnotations)Cosmos((IEntityType)entityType);
    }
}
