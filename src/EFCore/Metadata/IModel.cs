// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Metadata about the shape of entities, the relationships between them, and how they map to
    ///         the database. A model is typically created by overriding the
    ///         <see cref="DbContext.OnModelCreating(ModelBuilder)" /> method on a derived
    ///         <see cref="DbContext" />.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IModel : IAnnotatable
    {
        /// <summary>
        ///     Gets all entity types defined in the model.
        /// </summary>
        /// <returns> All entity types defined in the model. </returns>
        IEnumerable<IEntityType> GetEntityTypes();

        /// <summary>
        ///     Gets the entity type with the given name. Returns null if no entity type with the given name is found
        ///     or the given CLR type is being used by shared type entity type
        ///     or the entity type has a defining navigation.
        /// </summary>
        /// <param name="name"> The name of the entity type to find. </param>
        /// <returns> The entity type, or null if none are found. </returns>
        IEntityType FindEntityType([NotNull] string name);

        /// <summary>
        ///     Gets the entity type for the given name, defining navigation name
        ///     and the defining entity type. Returns null if no matching entity type is found.
        /// </summary>
        /// <param name="name"> The name of the entity type to find. </param>
        /// <param name="definingNavigationName"> The defining navigation of the entity type to find. </param>
        /// <param name="definingEntityType"> The defining entity type of the entity type to find. </param>
        /// <returns> The entity type, or null if none are found. </returns>
        IEntityType FindEntityType(
            [NotNull] string name,
            [NotNull] string definingNavigationName,
            [NotNull] IEntityType definingEntityType);
    }
}
