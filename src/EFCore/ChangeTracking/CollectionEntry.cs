// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
    public class CollectionEntry : NavigationEntry
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public CollectionEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] string name)
            : base(internalEntry, name, collection: true)
        {
            LocalDetectChanges();
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
            LocalDetectChanges();
        }

        private void LocalDetectChanges()
        {
            var collection = CurrentValue;
            if (collection != null)
            {
                var targetType = Metadata.GetTargetType();
                var context = InternalEntry.StateManager.Context;
                var changeDetector = context.ChangeTracker.AutoDetectChangesEnabled
                    && (string)context.Model[ChangeDetector.SkipDetectChangesAnnotation] != "true"
                     ? context.GetDependencies().ChangeDetector
                     : null;
                foreach (var entity in collection.OfType<object>().ToList())
                {
                    var entry = InternalEntry.StateManager.GetOrCreateEntry(entity, targetType);
                    changeDetector?.DetectChanges(entry);
                }
            }
        }

        /// <summary>
        ///     Gets or sets the value currently assigned to this property. If the current value is set using this property,
        ///     the change tracker is aware of the change and <see cref="ChangeTracker.DetectChanges" /> is not required
        ///     for the context to detect the change.
        /// </summary>
        public new virtual IEnumerable CurrentValue
        {
            get => (IEnumerable)base.CurrentValue;
            [param: CanBeNull] set => base.CurrentValue = value;
        }

        /// <summary>
        ///     <para>
        ///         Loads the entities referenced by this navigation property, unless <see cref="NavigationEntry.IsLoaded" />
        ///         is already set to true.
        ///     </para>
        ///     <para>
        ///         Note that entities that are already being tracked are not overwritten with new data from the database.
        ///     </para>
        /// </summary>
        public override void Load()
        {
            EnsureInitialized();

            base.Load();
        }

        /// <summary>
        ///     <para>
        ///         Loads entities referenced by this navigation property, unless <see cref="NavigationEntry.IsLoaded" />
        ///         is already set to true.
        ///     </para>
        ///     <para>
        ///         Note that entities that are already being tracked are not overwritten with new data from the database.
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
        ///     A task that represents the asynchronous save operation.
        /// </returns>
        public override Task LoadAsync(CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            return base.LoadAsync(cancellationToken);
        }

        /// <summary>
        ///     <para>
        ///         Returns the query that would be used by <see cref="Load" /> to load entities referenced by
        ///         this navigation property.
        ///     </para>
        ///     <para>
        ///         The query can be composed over using LINQ to perform filtering, counting, etc. without
        ///         actually loading all entities from the database.
        ///     </para>
        /// </summary>
        public override IQueryable Query()
        {
            EnsureInitialized();

            return base.Query();
        }

        private void EnsureInitialized()
            => Metadata.AsNavigation().CollectionAccessor.GetOrCreate(InternalEntry.Entity, forMaterialization: true);

        /// <summary>
        ///     The <see cref="EntityEntry" /> of an entity this navigation targets.
        /// </summary>
        /// <param name="entity"> The entity to get the entry for. </param>
        /// <value> An entry for an entity that this navigation targets. </value>
        public virtual EntityEntry FindEntry([NotNull] object entity)
        {
            var entry = GetInternalTargetEntry(entity);
            return entry == null
                    ? null
                    : new EntityEntry(entry);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual InternalEntityEntry GetInternalTargetEntry([NotNull] object entity)
            => CurrentValue == null
               || !((Navigation)Metadata).CollectionAccessor.Contains(InternalEntry.Entity, entity)
                  ? null
                  : InternalEntry.StateManager.GetOrCreateEntry(entity, Metadata.GetTargetType());
    }
}
