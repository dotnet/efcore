// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
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
            _model = model;
        }

        public virtual void PropertyChanged(InternalEntityEntry entry, IPropertyBase propertyBase)
        {
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

        public virtual void PropertyChanging(InternalEntityEntry entry, IPropertyBase propertyBase)
        {
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
            foreach (var entry in stateManager.Entries.ToList())
            {
                DetectChanges(entry);
            }
        }

        public virtual void DetectChanges([NotNull] InternalEntityEntry entry)
        {
            DetectPropertyChanges(entry);
            DetectRelationshipChanges(entry);
        }

        private void DetectPropertyChanges(InternalEntityEntry entry)
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

        private void DetectRelationshipChanges(InternalEntityEntry entry)
        {
            var snapshot = entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot);
            if (snapshot != null)
            {
                DetectKeyChanges(entry, snapshot);
                DetectNavigationChanges(entry, snapshot);
            }
        }

        private void DetectKeyChanges(InternalEntityEntry entry, Sidecar snapshot)
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

        private void DetectNavigationChanges(InternalEntityEntry entry, Sidecar snapshot)
        {
            var entityType = entry.EntityType;

            if (!entityType.HasPropertyChangedNotifications()
                || entityType.Navigations.Any(n => n.IsNonNotifyingCollection(entry)))
            {
                foreach (var navigation in entityType.Navigations)
                {
                    DetectNavigationChange(entry, navigation, snapshot);
                }
            }
        }

        private void DetectKeyChange(InternalEntityEntry entry, IProperty property, Sidecar snapshot)
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

        private void DetectNavigationChange(InternalEntityEntry entry, INavigation navigation, Sidecar snapshot)
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

            foreach (var addedEntity in added)
            {
                var addedEntry = stateManager.GetOrCreateEntry(addedEntity);
                if (addedEntry.EntityState == EntityState.Detached)
                {
                    addedEntry.SetEntityState(EntityState.Added);
                }
            }
        }
    }
}
