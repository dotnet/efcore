// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public abstract partial class InternalEntityEntry
    {
        private struct OriginalValues
        {
            private readonly ISnapshot _values;

            public OriginalValues(InternalEntityEntry entry)
            {
                _values = entry.EntityType.GetOriginalValuesFactory()(entry);
            }

            public object GetValue(InternalEntityEntry entry, IProperty property)
            {
                if (IsEmpty)
                {
                    return entry[property];
                }

                var index = property.GetOriginalValueIndex();

                return index != -1 ? _values[index] : entry[property];
            }

            public void SetValue(IProperty property, object value)
            {
                Debug.Assert(!IsEmpty);

                var index = property.GetOriginalValueIndex();
                if (index != -1)
                {
                    _values[index] = value;
                }
            }

            public void RejectChanges(InternalEntityEntry entry)
            {
                if (IsEmpty)
                {
                    return;
                }

                foreach (var property in entry.EntityType.GetProperties())
                {
                    var index = property.GetOriginalValueIndex();
                    if (index >= 0)
                    {
                        entry[property] = _values[index];
                    }
                }
            }

            public void AcceptChanges(InternalEntityEntry entry)
            {
                if (IsEmpty)
                {
                    return;
                }

                    foreach (var property in entry.EntityType.GetProperties())
                    {
                        var index = property.GetOriginalValueIndex();
                        if (index >= 0)
                        {
                            _values[index] = entry[property];
                        }
                    }
            }

            public bool IsEmpty => _values == null;
        }
    }
}
