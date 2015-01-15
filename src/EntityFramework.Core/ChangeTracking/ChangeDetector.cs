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
                    DetectNavigationChange(entry, navigation, snapshot);
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

            // TODO: Need real async once adding entities
            DetectChanges(entry);

            return Task.FromResult(false);
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
            var entityType = entry.EntityType;

            var snapshot = entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot);
            if (snapshot == null)
            {
                return;
            }

            var hasPropertyNotifications = entityType.HasPropertyChangedNotifications();
            if (!hasPropertyNotifications)
            {
                foreach (var property in entityType.Properties)
                {
                    DetectKeyChange(entry, property, snapshot);
                }
            }

            if (!hasPropertyNotifications
                || entityType.Navigations.Any(n => n.IsNonNotifyingCollection(entry)))
            {
                foreach (var navigation in entityType.Navigations)
                {
                    DetectNavigationChange(entry, navigation, snapshot);
                }
            }
        }

        private void DetectKeyChange(StateEntry entry, IProperty property, Sidecar snapshot)
        {
            if (!snapshot.HasValue(property))
            {
                return;
            }

            // TODO: Perf: make it fast to check if a property is part of the primary key
            var isPrimaryKey = property.IsPrimaryKey();

            // TODO: Perf: make it faster to check if the property is at the principal end or not
            var isPrincipalKey = _model.Service.GetReferencingForeignKeys(property).Any();

            if (isPrimaryKey
                || isPrincipalKey
                || property.IsForeignKey())
            {
                var snapshotValue = snapshot[property];
                var currentValue = entry[property];

                // Note that mutation of a byte[] key is not supported or detected, but two different instances
                // of byte[] with the same content must be detected as equal.
                if (!StructuralComparisons.StructuralEqualityComparer.Equals(currentValue, snapshotValue))
                {
                    if (property.IsForeignKey())
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

        private void DetectNavigationChange(StateEntry entry, INavigation navigation, Sidecar snapshot)
        {
            var snapshotValue = snapshot[navigation];
            var currentValue = entry[navigation];

            if (navigation.IsCollection())
            {
                var snapshotCollection = (IEnumerable)snapshotValue;
                var currentCollection = (IEnumerable)currentValue;

                var added = new HashSet<object>(ReferenceEqualityComparer.Instance);

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
                    entry.StateManager.Notify.NavigationCollectionChanged(entry, navigation, added, removed);

                    snapshot.TakeSnapshot(navigation);
                }
            }
            else if (!ReferenceEquals(currentValue, snapshotValue))
            {
                entry.StateManager.Notify.NavigationReferenceChanged(entry, navigation, snapshotValue, currentValue);

                snapshot.TakeSnapshot(navigation);
            }
        }
    }
}
