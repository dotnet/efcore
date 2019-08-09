// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         Provides access to change tracking information and operations for a given entity.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ChangeTracker" /> API and it is
    ///         not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    [DebuggerDisplay("{" + nameof(InternalEntry) + ",nq}")]
    public class EntityEntry : IInfrastructure<InternalEntityEntry>
    {
        private static readonly int _maxEntityState = Enum.GetValues(typeof(EntityState)).Cast<int>().Max();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual InternalEntityEntry InternalEntry { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public EntityEntry([NotNull] InternalEntityEntry internalEntry)
        {
            Check.NotNull(internalEntry, nameof(internalEntry));

            InternalEntry = internalEntry;
        }

        /// <summary>
        ///     Gets the entity being tracked by this entry.
        /// </summary>
        public virtual object Entity => InternalEntry.Entity;

        /// <summary>
        ///     <para>
        ///         Gets or sets that state that this entity is being tracked in.
        ///     </para>
        ///     <para>
        ///         This method sets only the state of the single entity represented by this entry. It does
        ///         not change the state of other entities reachable from this one.
        ///     </para>
        ///     <para>
        ///         When setting the state, the entity will always end up in the specified state. For example, if you
        ///         change the state to <see cref="EntityState.Deleted" /> the entity will be marked for deletion regardless
        ///         of its current state. This is different than calling <see cref="DbSet{TEntity}.Remove(TEntity)" /> where the entity
        ///         will be disconnected (rather than marked for deletion) if it is in the <see cref="EntityState.Added" /> state.
        ///     </para>
        /// </summary>
        public virtual EntityState State
        {
            get => InternalEntry.EntityState;
            set
            {
                if (value < 0
                    || (int)value > _maxEntityState)
                {
                    throw new ArgumentException(CoreStrings.InvalidEnumValue(nameof(value), typeof(EntityState)));
                }

                InternalEntry.SetEntityState(value);
            }
        }

        /// <summary>
        ///     Scans this entity instance to detect any changes made to the instance data. <see cref="DetectChanges()" />
        ///     is usually called automatically by the context to get up-to-date information on an individual entity before
        ///     returning change tracking information. You typically only need to call this method if you have
        ///     disabled <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
        /// </summary>
        public virtual void DetectChanges()
        {
            if ((string)Context.Model[ChangeDetector.SkipDetectChangesAnnotation] != "true")
            {
                Context.GetDependencies().ChangeDetector.DetectChanges(InternalEntry);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        InternalEntityEntry IInfrastructure<InternalEntityEntry>.Instance => InternalEntry;

        /// <summary>
        ///     Gets the context that is tracking the entity.
        /// </summary>
        public virtual DbContext Context => InternalEntry.StateManager.Context;

        /// <summary>
        ///     Gets the metadata about the shape of the entity, its relationships to other entities, and how it maps to the database.
        /// </summary>
        public virtual IEntityType Metadata => InternalEntry.EntityType;

        /// <summary>
        ///     Provides access to change tracking information and operations for a given
        ///     property or navigation property of this entity.
        /// </summary>
        /// <param name="propertyName"> The property to access information and operations for. </param>
        /// <returns> An object that exposes change tracking information and operations for the given property. </returns>
        public virtual MemberEntry Member([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            var property = InternalEntry.EntityType.FindProperty(propertyName);
            if (property != null)
            {
                return new PropertyEntry(InternalEntry, propertyName);
            }

            var navigation = InternalEntry.EntityType.FindNavigation(propertyName);
            if (navigation != null)
            {
                return navigation.IsCollection()
                    ? (MemberEntry)new CollectionEntry(InternalEntry, propertyName)
                    : new ReferenceEntry(InternalEntry, propertyName);
            }

            throw new InvalidOperationException(
                CoreStrings.PropertyNotFound(propertyName, InternalEntry.EntityType.DisplayName()));
        }

        /// <summary>
        ///     Provides access to change tracking information and operations for all
        ///     properties and navigation properties of this entity.
        /// </summary>
        public virtual IEnumerable<MemberEntry> Members
            => Properties.Cast<MemberEntry>().Concat(Navigations);

        /// <summary>
        ///     Provides access to change tracking information and operations for a given
        ///     navigation property of this entity.
        /// </summary>
        /// <param name="propertyName"> The property to access information and operations for. </param>
        /// <returns> An object that exposes change tracking information and operations for the given property. </returns>
        public virtual NavigationEntry Navigation([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            var navigation = InternalEntry.EntityType.FindNavigation(propertyName);
            if (navigation != null)
            {
                return navigation.IsCollection()
                    ? (NavigationEntry)new CollectionEntry(InternalEntry, propertyName)
                    : new ReferenceEntry(InternalEntry, propertyName);
            }

            if (InternalEntry.EntityType.FindProperty(propertyName) != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NavigationIsProperty(
                        propertyName, InternalEntry.EntityType.DisplayName(),
                        nameof(Reference), nameof(Collection), nameof(Property)));
            }

            throw new InvalidOperationException(
                CoreStrings.PropertyNotFound(propertyName, InternalEntry.EntityType.DisplayName()));
        }

        /// <summary>
        ///     Provides access to change tracking information and operations for all
        ///     navigation properties of this entity.
        /// </summary>
        public virtual IEnumerable<NavigationEntry> Navigations
            => InternalEntry.EntityType.GetNavigations().Select(
                navigation => navigation.IsCollection()
                    ? (NavigationEntry)new CollectionEntry(InternalEntry, navigation)
                    : new ReferenceEntry(InternalEntry, navigation));

        /// <summary>
        ///     Provides access to change tracking information and operations for a given
        ///     property of this entity.
        /// </summary>
        /// <param name="propertyName"> The property to access information and operations for. </param>
        /// <returns> An object that exposes change tracking information and operations for the given property. </returns>
        public virtual PropertyEntry Property([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            return new PropertyEntry(InternalEntry, propertyName);
        }

        /// <summary>
        ///     Provides access to change tracking information and operations for all
        ///     properties of this entity.
        /// </summary>
        public virtual IEnumerable<PropertyEntry> Properties
            => InternalEntry.EntityType.GetProperties().Select(property => new PropertyEntry(InternalEntry, property));

        /// <summary>
        ///     Provides access to change tracking and loading information for a reference (i.e. non-collection)
        ///     navigation property that associates this entity to another entity.
        /// </summary>
        /// <param name="propertyName"> The name of the navigation property. </param>
        /// <returns>
        ///     An object that exposes change tracking information and operations for the
        ///     given navigation property.
        /// </returns>
        public virtual ReferenceEntry Reference([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            return new ReferenceEntry(InternalEntry, propertyName);
        }

        /// <summary>
        ///     Provides access to change tracking information and loading information for all
        ///     reference (i.e. non-collection) navigation properties of this entity.
        /// </summary>
        public virtual IEnumerable<ReferenceEntry> References
            => InternalEntry.EntityType.GetNavigations().Where(n => !n.IsCollection())
                .Select(navigation => new ReferenceEntry(InternalEntry, navigation));

        /// <summary>
        ///     Provides access to change tracking and loading information for a collection
        ///     navigation property that associates this entity to a collection of another entities.
        /// </summary>
        /// <param name="propertyName"> The name of the navigation property. </param>
        /// <returns>
        ///     An object that exposes change tracking information and operations for the
        ///     given navigation property.
        /// </returns>
        public virtual CollectionEntry Collection([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            return new CollectionEntry(InternalEntry, propertyName);
        }

        /// <summary>
        ///     Provides access to change tracking information and loading information for all
        ///     collection navigation properties of this entity.
        /// </summary>
        public virtual IEnumerable<CollectionEntry> Collections
            => InternalEntry.EntityType.GetNavigations().Where(n => n.IsCollection())
                .Select(navigation => new CollectionEntry(InternalEntry, navigation));

        /// <summary>
        ///     <para>
        ///         Gets a value indicating if the key values of this entity have been assigned a value.
        ///     </para>
        ///     <para>
        ///         For keys with store-generated properties (e.g. mapping to Identity columns), the
        ///         return value will  be false if any of the store-generated properties have the
        ///         CLR default value.
        ///     </para>
        ///     <para>
        ///         For keys without any store-generated properties, the return value will always be
        ///         true since any value is considered a valid key value.
        ///     </para>
        /// </summary>
        public virtual bool IsKeySet => InternalEntry.IsKeySet.IsSet;

        /// <summary>
        ///     Gets the current property values for this entity.
        /// </summary>
        /// <value> The current values. </value>
        public virtual PropertyValues CurrentValues
        {
            [DebuggerStepThrough] get => new CurrentPropertyValues(InternalEntry);
        }

        /// <summary>
        ///     Gets the original property values for this entity. The original values are the property
        ///     values as they were when the entity was retrieved from the database.
        /// </summary>
        /// <value> The original values. </value>
        public virtual PropertyValues OriginalValues
        {
            [DebuggerStepThrough] get => new OriginalPropertyValues(InternalEntry);
        }

        /// <summary>
        ///     <para>
        ///         Queries the database for copies of the values of the tracked entity as they currently
        ///         exist in the database. If the entity is not found in the database, then null is returned.
        ///     </para>
        ///     <para>
        ///         Note that changing the values in the returned dictionary will not update the values
        ///         in the database.
        ///     </para>
        /// </summary>
        /// <returns> The store values, or null if the entity does not exist in the database. </returns>
        public virtual PropertyValues GetDatabaseValues()
        {
            var values = Finder.GetDatabaseValues(InternalEntry);

            return values == null ? null : new ArrayPropertyValues(InternalEntry, values);
        }

        /// <summary>
        ///     <para>
        ///         Queries the database for copies of the values of the tracked entity as they currently
        ///         exist in the database. If the entity is not found in the database, then null is returned.
        ///     </para>
        ///     <para>
        ///         Note that changing the values in the returned dictionary will not update the values
        ///         in the database.
        ///     </para>
        ///     <para>
        ///         Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///         that any asynchronous operations have completed before calling another method on this context.
        ///     </para>
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains the store values,
        ///     or null if the entity does not exist in the database.
        /// </returns>
        public virtual async Task<PropertyValues> GetDatabaseValuesAsync(CancellationToken cancellationToken = default)
        {
            var values = await Finder.GetDatabaseValuesAsync(InternalEntry, cancellationToken);

            return values == null ? null : new ArrayPropertyValues(InternalEntry, values);
        }

        /// <summary>
        ///     <para>
        ///         Reloads the entity from the database overwriting any property values with values from the database.
        ///     </para>
        ///     <para>
        ///         The entity will be in the <see cref="EntityState.Unchanged" /> state after calling this method,
        ///         unless the entity does not exist in the database, in which case the entity will be
        ///         <see cref="EntityState.Detached" />. Finally, calling Reload on an <see cref="EntityState.Added" />
        ///         entity that does not exist in the database is a no-op. Note, however, that an Added entity may
        ///         not yet have had its permanent key value created.
        ///     </para>
        /// </summary>
        public virtual void Reload() => Reload(GetDatabaseValues());

        /// <summary>
        ///     <para>
        ///         Reloads the entity from the database overwriting any property values with values from the database.
        ///     </para>
        ///     <para>
        ///         The entity will be in the <see cref="EntityState.Unchanged" /> state after calling this method,
        ///         unless the entity does not exist in the database, in which case the entity will be
        ///         <see cref="EntityState.Detached" />. Finally, calling Reload on an <see cref="EntityState.Added" />
        ///         entity that does not exist in the database is a no-op. Note, however, that an Added entity may
        ///         not yet have had its permanent key value created.
        ///     </para>
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        public virtual async Task ReloadAsync(CancellationToken cancellationToken = default)
            => Reload(await GetDatabaseValuesAsync(cancellationToken));

        private void Reload(PropertyValues storeValues)
        {
            if (storeValues == null)
            {
                if (State != EntityState.Added)
                {
                    State = EntityState.Detached;
                }
            }
            else
            {
                CurrentValues.SetValues(storeValues);
                OriginalValues.SetValues(storeValues);
                State = EntityState.Unchanged;
            }
        }

        private IEntityFinder Finder
            => InternalEntry.StateManager.CreateEntityFinder(InternalEntry.EntityType);

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
