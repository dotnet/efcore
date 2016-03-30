// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class ChangeDetector : IChangeDetector
    {
        private bool _suspended;

        public virtual void Suspend() => _suspended = true;

        public virtual void Resume() => _suspended = false;

        public virtual void PropertyChanged(InternalEntityEntry entry, IPropertyBase propertyBase, bool setModified)
        {
            if (_suspended)
            {
                return;
            }

            var property = propertyBase as IProperty;
            if (property != null)
            {
                entry.SetPropertyModified(property, setModified);

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
            if (_suspended)
            {
                return;
            }

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
            foreach (var entry in stateManager.Entries.Where(e => e.EntityState != EntityState.Detached).ToList())
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
                if (property.GetOriginalValueIndex() >= 0
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
            if (property.GetRelationshipIndex() >= 0)
            {
                var snapshotValue = entry.GetRelationshipSnapshotValue(property);
                var currentValue = entry[property];

                // Note that mutation of a byte[] key is not supported or detected, but two different instances
                // of byte[] with the same content must be detected as equal.
                if (!StructuralComparisons.StructuralEqualityComparer.Equals(currentValue, snapshotValue))
                {
                    var keys = property.FindContainingKeys().ToList();
                    var foreignKeys = property.FindContainingForeignKeys().ToList();

                    entry.StateManager.Notify.KeyPropertyChanged(entry, property, keys, foreignKeys, snapshotValue, currentValue);
                }
            }
        }

        private void DetectNavigationChange(InternalEntityEntry entry, INavigation navigation)
        {
            var snapshotValue = entry.GetRelationshipSnapshotValue(navigation);
            var currentValue = entry[navigation];
            var stateManager = entry.StateManager;


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

                var added = new HashSet<object>(ReferenceEqualityComparer.Instance);

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
                }
            }
            else if (!ReferenceEquals(currentValue, snapshotValue))
            {
                stateManager.Notify.NavigationReferenceChanged(entry, navigation, snapshotValue, currentValue);
            }
        }
    }
}
