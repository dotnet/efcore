// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
                        var property = TryGetPropertyBase(entityType, e.PropertyName);
                        if (property != null)
                        {
                            _notifier.PropertyChanging(entry, property);
                        }
                    };
            }

            var changed = entry.Entity as INotifyPropertyChanged;
            if (changed != null)
            {
                changed.PropertyChanged += (s, e) =>
                    {
                        var property = TryGetPropertyBase(entityType, e.PropertyName);
                        if (property != null)
                        {
                            _notifier.PropertyChanged(entry, property, setModified: true);
                        }
                    };
            }

            return entry;
        }

        private static IPropertyBase TryGetPropertyBase(IEntityType entityType, string propertyName)
            => (IPropertyBase)entityType.FindProperty(propertyName)
               ?? entityType.FindNavigation(propertyName);
    }
}
