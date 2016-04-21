// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class InternalEntityEntrySubscriber : IInternalEntityEntrySubscriber
    {
        private readonly IInternalEntityEntryNotifier _notifier;

        public InternalEntityEntrySubscriber([NotNull] IInternalEntityEntryNotifier notifier)
        {
            _notifier = notifier;
        }

        public virtual InternalEntityEntry SnapshotAndSubscribe(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;

            if (entityType.UseEagerSnapshots())
            {
                entry.EnsureOriginalValues();
                entry.EnsureRelationshipSnapshot();
            }

            var changeTrackingStrategy = entityType.GetChangeTrackingStrategy();
            if (changeTrackingStrategy != ChangeTrackingStrategy.Snapshot)
            {
                foreach (var navigation in entityType.GetNavigations().Where(n => n.IsCollection()))
                {
                    var notifyingCollection = navigation.GetCollectionAccessor().GetOrCreate(entry.Entity) as INotifyCollectionChanged;
                    if (notifyingCollection == null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.NonNotifyingCollection(navigation.Name, entityType.DisplayName(), changeTrackingStrategy));
                    }

                    notifyingCollection.CollectionChanged += (s, e) =>
                        {
                            switch (e.Action)
                            {
                                case NotifyCollectionChangedAction.Add:
                                    _notifier.NavigationCollectionChanged(
                                        entry,
                                        navigation,
                                        e.NewItems.OfType<object>(),
                                        Enumerable.Empty<object>());
                                    break;
                                case NotifyCollectionChangedAction.Remove:
                                    _notifier.NavigationCollectionChanged(
                                        entry,
                                        navigation,
                                        Enumerable.Empty<object>(),
                                        e.OldItems.OfType<object>());
                                    break;
                                case NotifyCollectionChangedAction.Replace:
                                    _notifier.NavigationCollectionChanged(
                                        entry,
                                        navigation,
                                        e.NewItems.OfType<object>(),
                                        e.OldItems.OfType<object>());
                                    break;
                                case NotifyCollectionChangedAction.Reset:
                                    if (e.OldItems == null)
                                    {
                                        throw new InvalidOperationException(CoreStrings.ResetNotSupported);
                                    }

                                    _notifier.NavigationCollectionChanged(
                                        entry,
                                        navigation,
                                        Enumerable.Empty<object>(),
                                        e.OldItems.OfType<object>());
                                    break;
                                // Note: ignoring Move since index not important
                            }
                        };
                }

                if (changeTrackingStrategy != ChangeTrackingStrategy.ChangedNotifications)
                {
                    var changing = entry.Entity as INotifyPropertyChanging;
                    if (changing == null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ChangeTrackingInterfaceMissing(
                                entityType.DisplayName(), changeTrackingStrategy, typeof(INotifyPropertyChanging).Name));
                    }

                    changing.PropertyChanging += (s, e) =>
                        {
                            foreach (var propertyBase in GetNotificationProperties(entityType, e.PropertyName))
                            {
                                _notifier.PropertyChanging(entry, propertyBase);
                            }
                        };
                }

                var changed = entry.Entity as INotifyPropertyChanged;
                if (changed == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ChangeTrackingInterfaceMissing(
                            entityType.DisplayName(), changeTrackingStrategy, typeof(INotifyPropertyChanged).Name));
                }

                changed.PropertyChanged += (s, e) =>
                    {
                        foreach (var propertyBase in GetNotificationProperties(entityType, e.PropertyName))
                        {
                            _notifier.PropertyChanged(entry, propertyBase, setModified: true);
                        }
                    };
            }

            return entry;
        }

        private static IEnumerable<IPropertyBase> GetNotificationProperties(IEntityType entityType, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                foreach (var property in entityType.GetProperties().Where(p => !p.IsReadOnlyAfterSave))
                {
                    yield return property;
                }

                foreach (var navigation in entityType.GetNavigations())
                {
                    yield return navigation;
                }
            }
            else
            {
                var property = TryGetPropertyBase(entityType, propertyName);
                if (property != null)
                {
                    yield return property;
                }
            }
        }

        private static IPropertyBase TryGetPropertyBase(IEntityType entityType, string propertyName)
            => (IPropertyBase)entityType.FindProperty(propertyName) ?? entityType.FindNavigation(propertyName);
    }
}
