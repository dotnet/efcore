// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         Provides access to change tracking and loading information for a collection
    ///         navigation property that associates this entity to a collection of another entities.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ChangeTracker" /> API and it is
    ///         not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    /// <typeparam name="TEntity"> The type of the entity the property belongs to. </typeparam>
    /// <typeparam name="TRelatedEntity"> The type of the property. </typeparam>
    public class CollectionEntry<TEntity, TRelatedEntity> : CollectionEntry
        where TEntity : class
        where TRelatedEntity : class
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public CollectionEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] string name)
            : base(internalEntry, name)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public CollectionEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] INavigation navigation)
            : base(internalEntry, navigation)
        {
        }

        /// <summary>
        ///     The <see cref="EntityEntry{TEntity}" /> to which this member belongs.
        /// </summary>
        /// <value> An entry for the entity that owns this member. </value>
        public new virtual EntityEntry<TEntity> EntityEntry => new EntityEntry<TEntity>(InternalEntry);

        /// <summary>
        ///     Gets or sets the value currently assigned to this property. If the current value is set using this property,
        ///     the change tracker is aware of the change and <see cref="ChangeTracker.DetectChanges" /> is not required
        ///     for the context to detect the change.
        /// </summary>
        public new virtual IEnumerable<TRelatedEntity> CurrentValue
        {
            get => this.GetInfrastructure().GetCurrentValue<IEnumerable<TRelatedEntity>>(Metadata);
            [param: CanBeNull] set => base.CurrentValue = value;
        }

        /// <summary>
        ///     <para>
        ///         Returns the query that would be used by <see cref="CollectionEntry.Load" /> to load entities referenced by
        ///         this navigation property.
        ///     </para>
        ///     <para>
        ///         The query can be composed over using LINQ to perform filtering, counting, etc. without
        ///         actually loading all entities from the database.
        ///     </para>
        /// </summary>
        public new virtual IQueryable<TRelatedEntity> Query()
        {
            InternalEntry.GetOrCreateCollection(Metadata, forMaterialization: true);

            return (IQueryable<TRelatedEntity>)base.Query();
        }

        /// <summary>
        ///     The <see cref="EntityEntry{T}" /> of an entity this navigation targets.
        /// </summary>
        /// <param name="entity"> The entity to get the entry for. </param>
        /// <value> An entry for an entity that this navigation targets. </value>
        public new virtual EntityEntry<TRelatedEntity> FindEntry([NotNull] object entity)
        {
            var entry = GetInternalTargetEntry(entity);
            return entry == null
                ? null
                : new EntityEntry<TRelatedEntity>(entry);
        }
    }
}
