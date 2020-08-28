// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
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
        private ICollectionLoader _loader;

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
                var targetType = Metadata.TargetEntityType;
                var context = InternalEntry.StateManager.Context;

                var changeDetector = context.ChangeTracker.AutoDetectChangesEnabled
                    && !((Model)context.Model).SkipDetectChanges
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
        ///     Gets or sets a value indicating whether any of foreign key property values associated
        ///     with this navigation property have been modified and should be updated in the database
        ///     when <see cref="DbContext.SaveChanges()" /> is called.
        /// </summary>
        public override bool IsModified
        {
            get
            {
                var stateManager = InternalEntry.StateManager;

                if (Metadata is ISkipNavigation skipNavigation)
                {
                    if (InternalEntry.EntityState != EntityState.Unchanged
                        && InternalEntry.EntityState != EntityState.Detached)
                    {
                        return true;
                    }

                    var joinEntityType = skipNavigation.JoinEntityType;
                    var foreignKey = skipNavigation.ForeignKey;
                    var inverseForeignKey = skipNavigation.Inverse.ForeignKey;
                    foreach (var joinEntry in stateManager.Entries)
                    {
                        if (joinEntry.EntityType == joinEntityType
                            && stateManager.FindPrincipal(joinEntry, foreignKey) == InternalEntry
                            && (joinEntry.EntityState == EntityState.Added
                                || joinEntry.EntityState == EntityState.Deleted
                                || foreignKey.Properties.Any(joinEntry.IsModified)
                                || inverseForeignKey.Properties.Any(joinEntry.IsModified)
                                || (stateManager.FindPrincipal(joinEntry, inverseForeignKey)?.EntityState == EntityState.Deleted)))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    var navigationValue = CurrentValue;
                    if (navigationValue != null)
                    {
                        var targetEntityType = Metadata.TargetEntityType;
                        var foreignKey = ((INavigation)Metadata).ForeignKey;

                        foreach (var relatedEntity in navigationValue)
                        {
                            var relatedEntry = stateManager.TryGetEntry(relatedEntity, targetEntityType);

                            if (relatedEntry != null
                                && (relatedEntry.EntityState == EntityState.Added
                                    || relatedEntry.EntityState == EntityState.Deleted
                                    || foreignKey.Properties.Any(relatedEntry.IsModified)))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
            set
            {
                var stateManager = InternalEntry.StateManager;

                if (Metadata is ISkipNavigation skipNavigation)
                {
                    var joinEntityType = skipNavigation.JoinEntityType;
                    var foreignKey = skipNavigation.ForeignKey;
                    foreach (var joinEntry in stateManager
                        .GetEntriesForState(added: !value, modified: !value, deleted: !value, unchanged: value).Where(
                            e => e.EntityType == joinEntityType
                                && stateManager.FindPrincipal(e, foreignKey) == InternalEntry)
                        .ToList())
                    {
                        joinEntry.SetEntityState(value ? EntityState.Modified : EntityState.Unchanged);
                    }
                }
                else
                {
                    var foreignKey = ((INavigation)Metadata).ForeignKey;
                    var navigationValue = CurrentValue;
                    if (navigationValue != null)
                    {
                        foreach (var relatedEntity in navigationValue)
                        {
                            var relatedEntry = InternalEntry.StateManager.TryGetEntry(relatedEntity, Metadata.TargetEntityType);
                            if (relatedEntry != null)
                            {
                                var anyNonPk = foreignKey.Properties.Any(p => !p.IsPrimaryKey());
                                foreach (var property in foreignKey.Properties)
                                {
                                    if (anyNonPk
                                        && !property.IsPrimaryKey())
                                    {
                                        relatedEntry.SetPropertyModified(property, isModified: value, acceptChanges: false);
                                    }
                                }
                            }
                        }
                    }
                }
            }
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

            if (!IsLoaded)
            {
                TargetLoader.Load(InternalEntry);
            }
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

            return IsLoaded
                ? Task.CompletedTask
                : TargetLoader.LoadAsync(InternalEntry, cancellationToken);
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

            return TargetLoader.Query(InternalEntry);
        }

        private void EnsureInitialized()
            => Metadata.GetCollectionAccessor().GetOrCreate(InternalEntry.Entity, forMaterialization: true);

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
        [EntityFrameworkInternal]
        protected virtual InternalEntityEntry GetInternalTargetEntry([NotNull] object entity)
            => CurrentValue == null
                || !Metadata.GetCollectionAccessor().Contains(InternalEntry.Entity, entity)
                    ? null
                    : InternalEntry.StateManager.GetOrCreateEntry(entity, Metadata.TargetEntityType);

        private ICollectionLoader TargetLoader
            => _loader ??= Metadata is ISkipNavigation skipNavigation
                ? skipNavigation.GetManyToManyLoader()
                : new EntityFinderCollectionLoaderAdapter(
                    InternalEntry.StateManager.CreateEntityFinder(Metadata.TargetEntityType),
                    (INavigation)Metadata);
    }
}
