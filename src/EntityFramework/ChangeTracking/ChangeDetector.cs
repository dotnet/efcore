// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class ChangeDetector
    {
        public virtual void PropertyChanged([NotNull] StateEntry entry, [NotNull] IPropertyBase propertyBase)
        {
            Check.NotNull(entry, "entry");
            Check.NotNull(propertyBase, "propertyBase");

            var property = propertyBase as IProperty;

            if (property != null)
            {
                entry.SetPropertyModified(property, true);

                if (DetectForeignKeyChange(entry, property))
                {
                    entry.RelationshipsSnapshot.TakeSnapshot(property);
                }
            }
            else
            {
                var navigation = propertyBase as INavigation;

                if (navigation != null)
                {
                    if (DetectNavigationChange(entry, navigation))
                    {
                        entry.RelationshipsSnapshot.TakeSnapshot(navigation);
                    }
                }
            }
        }

        public virtual void PropertyChanging([NotNull] StateEntry entry, [NotNull] IPropertyBase propertyBase)
        {
            Check.NotNull(entry, "entry");
            Check.NotNull(propertyBase, "propertyBase");

            if (entry.EntityType.UseLazyOriginalValues)
            {
                entry.OriginalValues.EnsureSnapshot(propertyBase);

                // TODO: Consider making snapshot temporary here since it is no longer required after PropertyChanged is called
                entry.RelationshipsSnapshot.TakeSnapshot(propertyBase);
            }
        }

        public virtual bool DetectChanges([NotNull] StateEntry entry)
        {
            Check.NotNull(entry, "entry");

            var entityType = entry.EntityType;
            var originalValues = entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues);

            // TODO: Consider more efficient/higher-level/abstract mechanism for checking if DetectChanges is needed
            if (entityType.Type == null
                || originalValues == null
                || typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(entityType.Type.GetTypeInfo()))
            {
                return false;
            }

            var changedFkProperties = new List<IProperty>();
            var foundChanges = false;
            foreach (var property in entityType.Properties)
            {
                // TODO: Perf: don't lookup accessor twice
                if (!Equals(entry[property], originalValues[property]))
                {
                    entry.SetPropertyModified(property, true);
                    foundChanges = true;
                }

                if (DetectForeignKeyChange(entry, property))
                {
                    changedFkProperties.Add(property);
                }
            }

            foreach (var property in changedFkProperties)
            {
                entry.RelationshipsSnapshot.TakeSnapshot(property);
            }

            foreach (var navigation in entityType.Navigations)
            {
                if (DetectNavigationChange(entry, navigation))
                {
                    entry.RelationshipsSnapshot.TakeSnapshot(navigation);
                }
            }

            return foundChanges;
        }

        private bool DetectForeignKeyChange(StateEntry entry, IProperty property)
        {
            // TODO: Consider flag/index for fast check for FK
            if (entry.EntityType.ForeignKeys.SelectMany(fk => fk.Properties).Contains(property))
            {
                var snapshotValue = entry.RelationshipsSnapshot[property];
                var currentValue = entry[property];

                // Note that mutation of a byte[] key is not supported or detected, but two different instances
                // of byte[] with the same content must be detected as equal.
                if (!StructuralComparisons.StructuralEqualityComparer.Equals(currentValue, snapshotValue))
                {
                    // TODO: Constructor injection
                    var notifier = entry.Configuration.Services.StateEntryNotifier;
                    notifier.ForeignKeyPropertyChanged(entry, property, snapshotValue, currentValue);

                    return true;
                }
            }

            return false;
        }

        private bool DetectNavigationChange(StateEntry entry, INavigation navigation)
        {
            var snapshotValue = entry.RelationshipsSnapshot[navigation];
            var currentValue = entry[navigation];

            if (navigation.IsCollection())
            {
                var snapshotCollection = (IEnumerable)snapshotValue;
                var currentCollection = (IEnumerable)currentValue;

                var added = new HashSet<object>(ReferenceEqualityComparer.Instance);

                var removed = new HashSet<object>(ReferenceEqualityComparer.Instance);
                foreach (var entity in snapshotCollection)
                {
                    removed.Add(entity);
                }

                foreach (var entity in currentCollection)
                {
                    if (!removed.Remove(entity))
                    {
                        added.Add(entity);
                    }
                }

                if (added.Any()
                    || removed.Any())
                {
                    var notifier = entry.Configuration.Services.StateEntryNotifier;
                    notifier.NavigationCollectionChanged(entry, navigation, added, removed);

                    return true;
                }
            }
            else if (!ReferenceEquals(currentValue, snapshotValue))
            {
                var notifier = entry.Configuration.Services.StateEntryNotifier;
                notifier.NavigationReferenceChanged(entry, navigation, snapshotValue, currentValue);

                return true;
            }

            return false;
        }
    }
}
