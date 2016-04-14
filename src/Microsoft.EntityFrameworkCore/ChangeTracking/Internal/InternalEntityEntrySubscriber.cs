// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
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
            else if (entityType.GetNavigations().Any(n => n.IsNonNotifyingCollection(entry)))
            {
                entry.EnsureRelationshipSnapshot();
            }

            var changing = entry.Entity as INotifyPropertyChanging;
            if (changing != null)
            {
                changing.PropertyChanging += (s, e) =>
                    {
                        foreach (var propertyBase in GetNotificationProperties(entityType, e.PropertyName))
                        {
                            _notifier.PropertyChanging(entry, propertyBase);
                        }
                    };
            }

            var changed = entry.Entity as INotifyPropertyChanged;
            if (changed != null)
            {
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
            => (IPropertyBase)entityType.FindProperty(propertyName)
               ?? entityType.FindNavigation(propertyName);
    }
}
