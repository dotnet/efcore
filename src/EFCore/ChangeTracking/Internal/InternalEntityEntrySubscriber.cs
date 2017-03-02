// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
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
    public class InternalEntityEntrySubscriber : IInternalEntityEntrySubscriber
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool SnapshotAndSubscribe(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;

            if (entityType.UseEagerSnapshots())
            {
                entry.EnsureOriginalValues();
                entry.EnsureRelationshipSnapshot();
            }

            var changeTrackingStrategy = entityType.GetChangeTrackingStrategy();

            if (changeTrackingStrategy == ChangeTrackingStrategy.Snapshot)
            {
                return false;
            }

            foreach (var navigation in entityType.GetNavigations().Where(n => n.IsCollection()))
            {
                AsINotifyCollectionChanged(entry, navigation, entityType, changeTrackingStrategy).CollectionChanged
                    += entry.HandleINotifyCollectionChanged;
            }

            if (changeTrackingStrategy != ChangeTrackingStrategy.ChangedNotifications)
            {
                AsINotifyPropertyChanging(entry, entityType, changeTrackingStrategy).PropertyChanging
                    += entry.HandleINotifyPropertyChanging;
            }

            AsINotifyPropertyChanged(entry, entityType, changeTrackingStrategy).PropertyChanged
                += entry.HandleINotifyPropertyChanged;

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Unsubscribe(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;
            var changeTrackingStrategy = entityType.GetChangeTrackingStrategy();

            if (changeTrackingStrategy != ChangeTrackingStrategy.Snapshot)
            {
                foreach (var navigation in entityType.GetNavigations().Where(n => n.IsCollection()))
                {
                    AsINotifyCollectionChanged(entry, navigation, entityType, changeTrackingStrategy).CollectionChanged
                        -= entry.HandleINotifyCollectionChanged;
                }

                if (changeTrackingStrategy != ChangeTrackingStrategy.ChangedNotifications)
                {
                    AsINotifyPropertyChanging(entry, entityType, changeTrackingStrategy).PropertyChanging
                        -= entry.HandleINotifyPropertyChanging;
                }

                AsINotifyPropertyChanged(entry, entityType, changeTrackingStrategy).PropertyChanged
                    -= entry.HandleINotifyPropertyChanged;
            }
        }

        private static INotifyCollectionChanged AsINotifyCollectionChanged(
            InternalEntityEntry entry,
            INavigation navigation,
            IEntityType entityType,
            ChangeTrackingStrategy changeTrackingStrategy)
        {
            var notifyingCollection = navigation.GetCollectionAccessor().GetOrCreate(entry.Entity) as INotifyCollectionChanged;
            if (notifyingCollection == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NonNotifyingCollection(navigation.Name, entityType.DisplayName(), changeTrackingStrategy));
            }

            return notifyingCollection;
        }

        private static INotifyPropertyChanged AsINotifyPropertyChanged(
            InternalEntityEntry entry,
            IEntityType entityType,
            ChangeTrackingStrategy changeTrackingStrategy)
        {
            var changed = entry.Entity as INotifyPropertyChanged;
            if (changed == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.ChangeTrackingInterfaceMissing(
                        entityType.DisplayName(), changeTrackingStrategy, nameof(INotifyPropertyChanged)));
            }

            return changed;
        }

        private static INotifyPropertyChanging AsINotifyPropertyChanging(
            InternalEntityEntry entry,
            IEntityType entityType,
            ChangeTrackingStrategy changeTrackingStrategy)
        {
            var changing = entry.Entity as INotifyPropertyChanging;
            if (changing == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.ChangeTrackingInterfaceMissing(
                        entityType.DisplayName(), changeTrackingStrategy, nameof(INotifyPropertyChanging)));
            }

            return changing;
        }
    }
}
