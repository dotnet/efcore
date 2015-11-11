// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    /// <summary>
    ///     <para>
    ///         Metadata about the shape of entities, the relationships between them, and how they map to the database. A model is typically
    ///         created by overriding the <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> method on a derived context, or
    ///         using <see cref="ModelBuilder" />.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IModel" /> represents a ready-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IMutableModel : IModel, IMutableAnnotatable
    {
        /// <summary>
        ///     Adds an entity from the model.
        /// </summary>
        /// <param name="name"> The name of the entity to be added. </param>
        /// <returns> The new created entity. </returns>
        IMutableEntityType AddEntityType([NotNull] string name);

        /// <summary>
        ///     Gets the entity with the given name. Returns null if no navigation property with the given name is found.
        /// </summary>
        /// <param name="name"> The name of the entity to find. </param>
        /// <returns> The entity type, or null if none if found. </returns>
        new IMutableEntityType FindEntityType([NotNull] string name);

        /// <summary>
        ///     Removes an entity from the model.
        /// </summary>
        /// <param name="name"> The name of the entity to be removed. </param>
        /// <returns> The entity that was removed. </returns>
        IMutableEntityType RemoveEntityType([NotNull] string name);

        /// <summary>
        ///     Gets all entities types defined in the model.
        /// </summary>
        /// <returns> All entities types defined in the model. </returns>
        new IEnumerable<IMutableEntityType> GetEntityTypes();
    }
}
