// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
        ///     <para>
        ///         Gets or sets a value indicating when a dependent/child entity will have its state
        ///         set to <see cref="EntityState.Deleted" /> once severed from a parent/principal entity
        ///         through either a navigation or foreign key property being set to null. The default
        ///         value is <see cref="CascadeTiming.Immediate" />.
        ///     </para>
        ///     <para>
        ///         Dependent/child entities are only deleted automatically when the relationship
        ///         is configured with <see cref="DeleteBehavior.Cascade" />. This is set by default
        ///         for required relationships.
        ///     </para>
        /// </summary>
        CascadeTiming DeleteOrphansTiming { get; set; }

        /// <summary>
        ///     <para>
        ///         Gets or sets a value indicating when a dependent/child entity will have its state
        ///         set to <see cref="EntityState.Deleted" /> once its parent/principal entity has been marked
        ///         as <see cref="EntityState.Deleted" />. The default value is<see cref="CascadeTiming.Immediate" />.
        ///     </para>
        ///     <para>
        ///         Dependent/child entities are only deleted automatically when the relationship
        ///         is configured with <see cref="DeleteBehavior.Cascade" />. This is set by default
        ///         for required relationships.
        ///     </para>
        /// </summary>
        CascadeTiming CascadeDeleteTiming { get; set; }

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
        ///     <para>
        ///         Forces immediate cascading deletion of child/dependent entities when they are either
        ///         severed from a required parent/principal entity, or the required parent/principal entity
        ///         is itself deleted. See <see cref="DeleteBehavior" />.
        ///     </para>
        ///     <para>
        ///         This method is usually used when <see cref="ChangeTracker.CascadeDeleteTiming" /> and/or
        ///         <see cref="ChangeTracker.DeleteOrphansTiming" /> have been set to <see cref="CascadeTiming.Never" />
        ///         to manually force the deletes to have at a time controlled by the application.
        ///     </para>
        ///     <para>
        ///         If <see cref="ChangeTracker.AutoDetectChangesEnabled"/> is <code>true</code> then this method
        ///         will call <see cref="DetectChanges"/>.
        ///     </para>
        /// </summary>
        void CascadeChanges();

        /// <summary>
        ///     Forces immediate cascading deletion of child/dependent entities when they are either
        ///     severed from a required parent/principal entity, or the required parent/principal entity
        ///     is itself deleted. See <see cref="DeleteBehavior" />.
        /// </summary>
        /// <param name="entry"> The entry. </param>
        /// <param name="foreignKeys"> The foreign keys to consider when cascading. </param>
        void CascadeDelete([NotNull] IUpdateEntry entry, [CanBeNull] IEnumerable<IForeignKey> foreignKeys = null);

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
