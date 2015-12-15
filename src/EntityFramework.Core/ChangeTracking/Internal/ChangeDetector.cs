// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Update;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class ChangeDetector : IChangeDetector
    {
        private readonly IEntityGraphAttacher _attacher;

        public ChangeDetector([NotNull] IEntityGraphAttacher attacher)
        {
            _attacher = attacher;
        }

        public virtual void PropertyChanged(InternalEntityEntry entry, IPropertyBase propertyBase)
        {
            var property = propertyBase as IProperty;
            if (property != null)
            {
                entry.SetPropertyModified(property);

                if (property.GetRelationshipIndex() != -1)
                {
                    DetectKeyChange(entry, property);
                }
            }
            else
            {
                var navigation = propertyBase as INavigation;
                if (navigation != null)
                {
                    DetectNavigationChange(entry, navigation);
                }
            }
        }

        public virtual void PropertyChanging(InternalEntityEntry entry, IPropertyBase propertyBase)
        {
            if (!entry.EntityType.UseEagerSnapshots())
            {
                entry.EnsureOriginalValues();

                if (propertyBase.GetRelationshipIndex() != -1)
                {
                    entry.EnsureRelationshipSnapshot();
                }
            }
        }

        public virtual void DetectChanges(IStateManager stateManager)
        {
            foreach (var entry in stateManager.Entries.ToList())
            {
                DetectChanges(entry);
            }
        }

        public virtual void DetectChanges(InternalEntityEntry entry)
        {
            DetectPropertyChanges(entry);
            DetectRelationshipChanges(entry);
        }

        private static void DetectPropertyChanges(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;

            if (entityType.HasPropertyChangedNotifications())
            {
                return;
            }

            foreach (var property in entityType.GetProperties())
            {
                if ((property.GetOriginalValueIndex() >= 0)
                    && !Equals(entry[property], entry.GetOriginalValue(property)))
                {
                    entry.SetPropertyModified(property);
                }
            }
        }

        private void DetectRelationshipChanges(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;

            if (!entityType.HasPropertyChangedNotifications())
            {
                DetectKeyChanges(entry);
            }

            if (entry.HasRelationshipSnapshot)
            {
                DetectNavigationChanges(entry);
            }
        }

        private void DetectKeyChanges(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;

            if (!entityType.HasPropertyChangedNotifications())
            {
                foreach (var property in entityType.GetProperties())
                {
                    DetectKeyChange(entry, property);
                }
            }
        }

        private void DetectNavigationChanges(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;

            if (!entityType.HasPropertyChangedNotifications()
                || entityType.GetNavigations().Any(n => n.IsNonNotifyingCollection(entry)))
            {
                foreach (var navigation in entityType.GetNavigations())
                {
                    DetectNavigationChange(entry, navigation);
                }
            }
        }

        private static void DetectKeyChange(InternalEntityEntry entry, IProperty property)
        {
            var keys = property.FindContainingKeys().ToList();
            var foreignKeys = property.FindContainingForeignKeys(entry.EntityType).ToList();

            if ((keys.Count > 0)
                || (foreignKeys.Count > 0))
            {
                var snapshotValue = entry.GetRelationshipSnapshotValue(property);
                var currentValue = entry[property];

                // Note that mutation of a byte[] key is not supported or detected, but two different instances
                // of byte[] with the same content must be detected as equal.
                if (!StructuralComparisons.StructuralEqualityComparer.Equals(currentValue, snapshotValue))
                {
                    var stateManager = entry.StateManager;

                    if (foreignKeys.Count > 0)
                    {
                        stateManager.Notify.ForeignKeyPropertyChanged(entry, property, snapshotValue, currentValue);
                    }

                    if (keys.Count > 0)
                    {
                        foreach (var key in keys)
                        {
                            stateManager.UpdateIdentityMap(entry, key);
                        }

                        stateManager.Notify.PrincipalKeyPropertyChanged(entry, property, snapshotValue, currentValue);
                    }

                    entry.SetRelationshipSnapshotValue(property, currentValue);
                }
            }
        }

        private void DetectNavigationChange(InternalEntityEntry entry, INavigation navigation)
        {
            var snapshotValue = entry.GetRelationshipSnapshotValue(navigation);
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

                    foreach (var addedEntity in added)
                    {
                        entry.AddToCollectionSnapshot(navigation, addedEntity);
                    }

                    foreach (var removedEntity in removed)
                    {
                        entry.RemoveFromCollectionSnapshot(navigation, removedEntity);
                    }
                }
            }
            else if (!ReferenceEquals(currentValue, snapshotValue))
            {
                stateManager.Notify.NavigationReferenceChanged(entry, navigation, snapshotValue, currentValue);

                if (currentValue != null)
                {
                    added.Add(currentValue);
                }

                entry.SetRelationshipSnapshotValue(navigation, currentValue);
            }

            foreach (var addedEntity in added)
            {
                var addedEntry = stateManager.GetOrCreateEntry(addedEntity);
                if (addedEntry.EntityState == EntityState.Detached)
                {
                    _attacher.AttachGraph(addedEntry, EntityState.Added);
                }
            }
        }
    }
}
