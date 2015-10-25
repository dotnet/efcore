// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class InternalEntityEntrySubscriber : IInternalEntityEntrySubscriber
    {
        private readonly IInternalEntityEntryNotifier _notifier;

        public InternalEntityEntrySubscriber([NotNull] IInternalEntityEntryNotifier notifier)
        {
            _notifier = notifier;
        }

        public virtual InternalEntityEntry SnapshotAndSubscribe(InternalEntityEntry entry, ValueBuffer? values)
        {
            var entityType = entry.EntityType;

            if (entityType.UseEagerSnapshots())
            {
                if (values != null)
                {
                    entry.OriginalValues = new ValueBufferOriginalValues(entry, values.Value);
                }
                else
                {
                    entry.OriginalValues.TakeSnapshot();
                }

                entry.RelationshipsSnapshot.TakeSnapshot();
            }
            else
            {
                foreach (var navigation in entityType.GetNavigations().Where(n => n.IsNonNotifyingCollection(entry)))
                {
                    entry.RelationshipsSnapshot.TakeSnapshot(navigation);
                }
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
                            _notifier.PropertyChanged(entry, property);
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
