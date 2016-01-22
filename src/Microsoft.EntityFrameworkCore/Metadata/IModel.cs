// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Metadata about the shape of entities, the relationships between them, and how they map to the database. A model is typically
    ///     created by overriding the <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> method on a derived context, or
    ///     using <see cref="ModelBuilder" />.
    /// </summary>
    public interface IModel : IAnnotatable
    {
        /// <summary>
        ///     Gets all entities types defined in the model.
        /// </summary>
        /// <returns> All entities types defined in the model. </returns>
        IEnumerable<IEntityType> GetEntityTypes();

        /// <summary>
        ///     Gets the entity with the given name. Returns null if no entity with the given name is found.
        /// </summary>
        /// <param name="name"> The name of the entity to find. </param>
        /// <returns> The entity type, or null if none if found. </returns>
        IEntityType FindEntityType([NotNull] string name);
    }
}
