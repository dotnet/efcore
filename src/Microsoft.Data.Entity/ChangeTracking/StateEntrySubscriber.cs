// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class StateEntrySubscriber
    {
        public virtual StateEntry SnapshotAndSubscribe([NotNull] StateEntry entry)
        {
            if (!entry.EntityType.UseLazyOriginalValues)
            {
                entry.OriginalValues.TakeSnapshot();
            }

            var changing = entry.Entity as INotifyPropertyChanging;
            if (changing != null)
            {
                changing.PropertyChanging += (s, e) =>
                    {
                        var property = entry.EntityType.TryGetProperty(e.PropertyName);
                        if (property != null)
                        {
                            entry.PropertyChanging(property);
                        }
                    };
            }

            var changed = entry.Entity as INotifyPropertyChanged;
            if (changed != null)
            {
                changed.PropertyChanged += (s, e) =>
                    {
                        var property = entry.EntityType.TryGetProperty(e.PropertyName);
                        if (property != null)
                        {
                            entry.PropertyChanged(property);
                        }
                    };
            }

            return entry;
        }
    }
}
