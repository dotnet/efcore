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
    /// <typeparam name="TProperty"> The type of the property. </typeparam>
    public class CollectionEntry<TEntity, TProperty> : CollectionEntry
        where TEntity : class
        where TProperty : class
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CollectionEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] string name)
            : base(internalEntry, name)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
        public new virtual IEnumerable<TProperty> CurrentValue
        {
            get { return this.GetInfrastructure().GetCurrentValue<IEnumerable<TProperty>>(Metadata); }
            [param: CanBeNull] set { base.CurrentValue = value; }
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
        public new virtual IQueryable<TProperty> Query()
        {
            EnsureInitialized();

            return (IQueryable<TProperty>)base.Query();
        }
    }
}
