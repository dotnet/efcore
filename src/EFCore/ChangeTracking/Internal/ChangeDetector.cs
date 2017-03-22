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
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ChangeDetector : IChangeDetector
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public const string SkipDetectChangesAnnotation = "ChangeDetector.SkipDetectChanges";

        private bool _suspended;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Suspend() => _suspended = true;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Resume() => _suspended = false;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void PropertyChanged(InternalEntityEntry entry, IPropertyBase propertyBase, bool setModified)
        {
            if (_suspended || entry.EntityState == EntityState.Detached)
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
                if (propertyBase.GetRelationshipIndex() != -1)
                {
                    var navigation = propertyBase as INavigation;
                    if (navigation != null)
                    {
                        DetectNavigationChange(entry, navigation);
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void PropertyChanging(InternalEntityEntry entry, IPropertyBase propertyBase)
        {
            if (_suspended || entry.EntityState == EntityState.Detached)
            {
                return;
            }

            if (!entry.EntityType.UseEagerSnapshots())
            {
                var asProperty = propertyBase as IProperty;
                if (asProperty != null
                    && asProperty.GetOriginalValueIndex() != -1)
                {
                    entry.EnsureOriginalValues();
                }

                if (propertyBase.GetRelationshipIndex() != -1)
                {
                    entry.EnsureRelationshipSnapshot();
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void DetectChanges(IStateManager stateManager)
        {
            if (stateManager.Context.Model[SkipDetectChangesAnnotation] == null)
            {
                foreach (var entry in stateManager.Entries.Where(
                    e => e.EntityState != EntityState.Detached
                         && e.EntityType.GetChangeTrackingStrategy() == ChangeTrackingStrategy.Snapshot).ToList())
                {
                    DetectChanges(entry);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void DetectChanges(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;

            foreach (var property in entityType.GetProperties())
            {
                if (property.GetOriginalValueIndex() >= 0
                    && !Equals(entry[property], entry.GetOriginalValue(property)))
                {
                    entry.SetPropertyModified(property);
                }
            }

            foreach (var property in entityType.GetProperties())
            {
                DetectKeyChange(entry, property);
            }

            if (entry.HasRelationshipSnapshot)
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
                    var keys = property.GetContainingKeys().ToList();
                    var foreignKeys = property.GetContainingForeignKeys().ToList();

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
            else if (!ReferenceEquals(currentValue, snapshotValue)
                && (!navigation.ForeignKey.IsOwnership
                || !navigation.IsDependentToPrincipal()))
            {
                stateManager.Notify.NavigationReferenceChanged(entry, navigation, snapshotValue, currentValue);
            }
        }
    }
}
