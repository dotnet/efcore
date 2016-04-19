// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
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
        ///     <para>
        ///         Adds a shadow state entity type to the model.
        ///     </para>
        ///     <para>
        ///         Shadow entities are not currently supported in a model that is used at runtime with a <see cref="DbContext" />.
        ///         Therefore, shadow state entity types will only exist in migration model snapshots, etc.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the entity to be added. </param>
        /// <returns> The new entity type. </returns>
        IMutableEntityType AddEntityType([NotNull] string name);

        /// <summary>
        ///     Adds an entity type to the model.
        /// </summary>
        /// <param name="clrType"> The CLR class that is used to represent instances of this entity type. </param>
        /// <returns> The new entity type. </returns>
        IMutableEntityType AddEntityType([CanBeNull] Type clrType);

        /// <summary>
        ///     Gets the entity with the given name. Returns null if no entity type with the given name is found.
        /// </summary>
        /// <param name="name"> The name of the entity type to find. </param>
        /// <returns> The entity type, or null if none are found. </returns>
        new IMutableEntityType FindEntityType([NotNull] string name);

        /// <summary>
        ///     Removes an entity type from the model.
        /// </summary>
        /// <param name="name"> The name of the entity type to be removed. </param>
        /// <returns> The entity type that was removed. </returns>
        IMutableEntityType RemoveEntityType([NotNull] string name);

        /// <summary>
        ///     Gets all entity types defined in the model.
        /// </summary>
        /// <returns> All entity types defined in the model. </returns>
        new IEnumerable<IMutableEntityType> GetEntityTypes();
    }
}
