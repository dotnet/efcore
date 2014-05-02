// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
