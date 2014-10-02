// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class StateEntrySubscriber
    {
        private readonly ChangeDetector _changeDetector;

        public StateEntrySubscriber([NotNull] ChangeDetector changeDetector)
        {
            Check.NotNull(changeDetector, "changeDetector");

            _changeDetector = changeDetector;
        }

        public virtual StateEntry SnapshotAndSubscribe([NotNull] StateEntry entry)
        {
            var entityType = entry.EntityType;

            if (!entityType.UseLazyOriginalValues)
            {
                entry.OriginalValues.TakeSnapshot();
                entry.RelationshipsSnapshot.TakeSnapshot();
            }

            var changing = entry.Entity as INotifyPropertyChanging;
            if (changing != null)
            {
                changing.PropertyChanging += (s, e) =>
                    {
                        var property = TryGetPropertyBase(entityType, e.PropertyName);
                        if (property != null)
                        {
                            _changeDetector.PropertyChanging(entry, property);
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
                            _changeDetector.PropertyChanged(entry, property);
                        }
                    };
            }

            return entry;
        }

        private static IPropertyBase TryGetPropertyBase(IEntityType entityType, string propertyName)
        {
            // TODO: Consider optimizing/consolidating property/navigation lookup
            // Issue #635
            return (IPropertyBase)entityType.TryGetProperty(propertyName)
                   ?? entityType.Navigations.FirstOrDefault(n => n.Name == propertyName);
        }
    }
}
