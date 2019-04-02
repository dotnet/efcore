// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         Providers tracking capabilities for seed data stored in the model using
    ///         <see cref="EntityTypeBuilder.HasData(object[])"/>.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IUpdateAdapter
    {
        /// <summary>
        ///     Gets the principal entry for the given dependent entry and foreign key.
        /// </summary>
        /// <param name="dependentEntry"> The dependent entry. </param>
        /// <param name="foreignKey"> The foreign key that defines the relationship. </param>
        /// <returns> The principal, or null if none was found. </returns>
        IUpdateEntry FindPrincipal([NotNull] IUpdateEntry dependentEntry, [NotNull] IForeignKey foreignKey);

        /// <summary>
        ///     Returns the dependents associated with the given principal and foreign key.
        /// </summary>
        /// <param name="principalEntry"> The principal entry. </param>
        /// <param name="foreignKey"> The foreign key that defines the relationship. </param>
        /// <returns> The dependents. </returns>
        IEnumerable<IUpdateEntry> GetDependents([NotNull] IUpdateEntry principalEntry, [NotNull] IForeignKey foreignKey);

        /// <summary>
        ///     Finds the tracked entity for the given key values.
        /// </summary>
        /// <param name="key"> The primary or alternate key to use. </param>
        /// <param name="keyValues"> The key values. </param>
        /// <returns> The entry for the found entity, or null if no entity with these key values is being tracked. </returns>
        IUpdateEntry TryGetEntry([NotNull] IKey key, [NotNull] object[] keyValues);

        /// <summary>
        ///    All the entries currently being tracked.
        /// </summary>
        IEnumerable<IUpdateEntry> Entries { get; }

        /// <summary>
        ///    Causes the underlying tracker to detect changes made to the tracked entities.
        /// </summary>
        void DetectChanges();

        /// <summary>
        ///     Gets all the entries that require inserts/updates/deletes in the database.
        /// </summary>
        /// <returns> The entries that need to be saved. </returns>
        IList<IUpdateEntry> GetEntriesToSave();

        /// <summary>
        ///     Creates a new entry with the given property values for the given entity type.
        /// </summary>
        /// <param name="values"> A dictionary of property names to values. </param>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The created entry. </returns>
        IUpdateEntry CreateEntry([NotNull] IDictionary<string, object> values, [NotNull] IEntityType entityType);

        /// <summary>
        ///     The model with which the data is associated.
        /// </summary>
        IModel Model { get; }
    }
}
