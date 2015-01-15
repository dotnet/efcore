// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class ChangeDetector : IPropertyListener
    {
        private readonly DbContextService<IModel> _model;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ChangeDetector()
        {
        }

        public ChangeDetector([NotNull] DbContextService<IModel> model)
        {
            Check.NotNull(model, "model");

            _model = model;
        }

        public virtual void SidecarPropertyChanged(StateEntry entry, IPropertyBase propertyBase)
        {
            Check.NotNull(entry, "entry");
            Check.NotNull(propertyBase, "propertyBase");

            var property = propertyBase as IProperty;
            if (property == null)
            {
                return;
            }

            var snapshot = entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot);
            if (snapshot == null)
            {
                return;
            }

            DetectKeyChange(entry, property, snapshot);
        }

        public virtual void SidecarPropertyChanging(StateEntry entry, IPropertyBase propertyBase)
        {
            PropertyChanging(entry, propertyBase);
        }

        public virtual void PropertyChanged(StateEntry entry, IPropertyBase propertyBase)
        {
            Check.NotNull(entry, "entry");
            Check.NotNull(propertyBase, "propertyBase");

            var snapshot = entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot);

            var property = propertyBase as IProperty;
            if (property != null)
            {
                entry.SetPropertyModified(property);

                if (snapshot != null)
                {
                    DetectKeyChange(entry, property, snapshot);
                }
            }
            else
            {
                var navigation = propertyBase as INavigation;
                if (navigation != null
                    && snapshot != null)
                {
                    TrackAddedEntities(entry.StateManager, DetectNavigationChange(entry, navigation, snapshot));
                }
            }
        }

        public virtual void PropertyChanging(StateEntry entry, IPropertyBase propertyBase)
        {
            Check.NotNull(entry, "entry");
            Check.NotNull(propertyBase, "propertyBase");

            if (!entry.EntityType.UseEagerSnapshots)
            {
                var property = propertyBase as IProperty;
                if (property != null
                    && property.OriginalValueIndex >= 0)
                {
                    entry.OriginalValues.EnsureSnapshot(property);
                }

                var navigation = propertyBase as INavigation;
                if ((navigation != null && !navigation.IsCollection())
                    || (property != null && (property.IsKey() || property.IsForeignKey())))
                {
                    // TODO: Consider making snapshot temporary here since it is no longer required after PropertyChanged is called
                    // See issue #730
                    entry.RelationshipsSnapshot.TakeSnapshot(propertyBase);
                }
            }
        }

        public virtual void DetectChanges([NotNull] StateManager stateManager)
        {
            Check.NotNull(stateManager, "stateManager");

            foreach (var entry in stateManager.StateEntries.ToList())
            {
                DetectChanges(entry);
            }
        }

        public virtual async Task DetectChangesAsync(
            [NotNull] StateManager stateManager, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(stateManager, "stateManager");

            foreach (var entry in stateManager.StateEntries.ToList())
            {
                await DetectChangesAsync(entry, cancellationToken).WithCurrentCulture();
            }
        }

        public virtual void DetectChanges([NotNull] StateEntry entry)
        {
            Check.NotNull(entry, "entry");

            DetectPropertyChanges(entry);
            DetectRelationshipChanges(entry);
        }

        public virtual Task DetectChangesAsync(
            [NotNull] StateEntry entry, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(entry, "entry");

            DetectPropertyChanges(entry);

            return DetectRelationshipChangesAsync(entry, cancellationToken);
        }

        private void DetectPropertyChanges(StateEntry entry)
        {
            var entityType = entry.EntityType;

            if (entityType.HasPropertyChangedNotifications())
            {
                return;
            }

            var snapshot = entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues);
            if (snapshot == null)
            {
                return;
            }

            foreach (var property in entityType.Properties)
            {
                if (property.OriginalValueIndex >= 0
                    && !Equals(entry[property], snapshot[property]))
                {
                    entry.SetPropertyModified(property);
                }
            }
        }

        private void DetectRelationshipChanges(StateEntry entry)
        {
            var snapshot = entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot);
            if (snapshot != null)
            {
                DetectKeyChanges(entry, snapshot);
                DetectNavigationChanges(entry, snapshot);
            }
        }

        private Task DetectRelationshipChangesAsync(StateEntry entry, CancellationToken cancellationToken = default(CancellationToken))
        {
            var snapshot = entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot);
            if (snapshot == null)
            {
                return Task.FromResult(false);
            }

            DetectKeyChanges(entry, snapshot);

            return DetectNavigationChangesAsync(entry, snapshot, cancellationToken);
        }

        private void DetectKeyChanges(StateEntry entry, Sidecar snapshot)
        {
            var entityType = entry.EntityType;

            if (!entityType.HasPropertyChangedNotifications())
            {
                foreach (var property in entityType.Properties)
                {
                    DetectKeyChange(entry, property, snapshot);
                }
            }
        }

        private void DetectNavigationChanges(StateEntry entry, Sidecar snapshot)
        {
            var entityType = entry.EntityType;

            if (!entityType.HasPropertyChangedNotifications()
                || entityType.Navigations.Any(n => n.IsNonNotifyingCollection(entry)))
            {
                foreach (var navigation in entityType.Navigations)
                {
                    TrackAddedEntities(entry.StateManager, DetectNavigationChange(entry, navigation, snapshot));
                }
            }
        }

        private async Task DetectNavigationChangesAsync(
            StateEntry entry, Sidecar snapshot, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entityType = entry.EntityType;

            if (!entityType.HasPropertyChangedNotifications()
                || entityType.Navigations.Any(n => n.IsNonNotifyingCollection(entry)))
            {
                foreach (var navigation in entityType.Navigations)
                {
                    await TrackAddedEntitiesAsync(entry.StateManager, DetectNavigationChange(entry, navigation, snapshot), cancellationToken)
                        .WithCurrentCulture();
                }
            }
        }

        private void DetectKeyChange(StateEntry entry, IProperty property, Sidecar snapshot)
        {
            if (!snapshot.HasValue(property))
            {
                return;
            }

            // TODO: Perf: make it fast to check if a property is part of any key
            var isPrimaryKey = property.IsPrimaryKey();
            var isPrincipalKey = _model.Service.GetReferencingForeignKeys(property).Any();
            var isForeignKey = property.IsForeignKey();

            if (isPrimaryKey
                || isPrincipalKey
                || isForeignKey)
            {
                var snapshotValue = snapshot[property];
                var currentValue = entry[property];

                // Note that mutation of a byte[] key is not supported or detected, but two different instances
                // of byte[] with the same content must be detected as equal.
                if (!StructuralComparisons.StructuralEqualityComparer.Equals(currentValue, snapshotValue))
                {
                    if (isForeignKey)
                    {
                        entry.StateManager.Notify.ForeignKeyPropertyChanged(entry, property, snapshotValue, currentValue);
                    }

                    if (isPrimaryKey)
                    {
                        entry.StateManager.UpdateIdentityMap(entry, snapshot.GetPrimaryKeyValue());
                    }

                    if (isPrincipalKey)
                    {
                        entry.StateManager.Notify.PrincipalKeyPropertyChanged(entry, property, snapshotValue, currentValue);
                    }

                    snapshot.TakeSnapshot(property);
                }
            }
        }

        private IEnumerable<object> DetectNavigationChange(StateEntry entry, INavigation navigation, Sidecar snapshot)
        {
            var snapshotValue = snapshot[navigation];
            var currentValue = entry[navigation];
            var stateManager = entry.StateManager;

            var added = new HashSet<object>(ReferenceEqualityComparer.Instance);

            if (navigation.IsCollection())
            {
                var snapshotCollection = (IEnumerable)snapshotValue;
                var currentCollection = (IEnumerable)currentValue;

                var removed = new HashSet<object>(ReferenceEqualityComparer.Instance);
                if (snapshotCollection != null)
                {
                    foreach (var entity in snapshotCollection)
                    {
                        removed.Add(entity);
                    }
                }

                if (currentCollection != null)
                {
                    foreach (var entity in currentCollection)
                    {
                        if (!removed.Remove(entity))
                        {
                            added.Add(entity);
                        }
                    }
                }

                if (added.Any()
                    || removed.Any())
                {
                    stateManager.Notify.NavigationCollectionChanged(entry, navigation, added, removed);

                    snapshot.TakeSnapshot(navigation);
                }
            }
            else if (!ReferenceEquals(currentValue, snapshotValue))
            {
                stateManager.Notify.NavigationReferenceChanged(entry, navigation, snapshotValue, currentValue);

                if (currentValue != null)
                {
                    added.Add(currentValue);
                }

                snapshot.TakeSnapshot(navigation);
            }

            return added;
        }

        private void TrackAddedEntities(StateManager stateManager, IEnumerable<object> addedEntities)
        {
            foreach (var addedEntity in addedEntities)
            {
                var addedEntry = stateManager.GetOrCreateEntry(addedEntity);
                if (addedEntry.EntityState == EntityState.Unknown)
                {
                    addedEntry.SetEntityState(EntityState.Added);
                }
            }
        }

        private async Task TrackAddedEntitiesAsync(
            StateManager stateManager,
            IEnumerable<object> addedEntities,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var addedEntity in addedEntities)
            {
                var addedEntry = stateManager.GetOrCreateEntry(addedEntity);
                if (addedEntry.EntityState == EntityState.Unknown)
                {
                    await addedEntry.SetEntityStateAsync(
                        EntityState.Added, acceptChanges: false, cancellationToken: cancellationToken).WithCurrentCulture();
                }
            }
        }
    }
}
