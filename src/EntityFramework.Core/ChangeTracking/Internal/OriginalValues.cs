// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public abstract partial class InternalEntityEntry
    {
        private struct OriginalValues
        {
            private ValueBuffer _values;
            private readonly bool _partialSnapshot;

            public OriginalValues(ValueBuffer values)
            {
                _values = values;
                _partialSnapshot = false;
            }

            public OriginalValues(InternalEntityEntry entry)
            {
                var entityType = entry.EntityType;
                var values = new object[entityType.OriginalValueCount()];

                foreach (var property in entityType.GetProperties())
                {
                    var index = property.GetOriginalValueIndex();
                    if (index >= 0)
                    {
                        values[index] = entry[property];
                    }
                }

                _values = new ValueBuffer(values);
                _partialSnapshot = true;
            }

            public object GetValue(InternalEntityEntry entry, IProperty property)
            {
                if (_values.IsEmpty)
                {
                    return entry[property];
                }

                if (_partialSnapshot)
                {
                    var index = property.GetOriginalValueIndex();

                    return index != -1 ? _values[index] : entry[property];
                }

                return _values[property.GetIndex()];
            }

            public void SetValue(IProperty property, object value)
            {
                Debug.Assert(!_values.IsEmpty);

                var index = _partialSnapshot
                    ? property.GetOriginalValueIndex()
                    : property.GetIndex();

                if (index != -1)
                {
                    _values[index] = value;
                }
            }

            public void RejectChanges(InternalEntityEntry entry)
            {
                if (_values.IsEmpty)
                {
                    return;
                }

                if (_partialSnapshot)
                {
                    foreach (var property in entry.EntityType.GetProperties())
                    {
                        var index = property.GetOriginalValueIndex();
                        if (index >= 0)
                        {
                            entry[property] = _values[index];
                        }
                    }
                }
                else
                {
                    foreach (var property in entry.EntityType.GetProperties())
                    {
                        entry[property] = _values[property.GetIndex()];
                    }
                }
            }

            public void AcceptChanges(InternalEntityEntry entry)
            {
                if (_values.IsEmpty)
                {
                    return;
                }

                if (_partialSnapshot)
                {
                    foreach (var property in entry.EntityType.GetProperties())
                    {
                        var index = property.GetOriginalValueIndex();
                        if (index >= 0)
                        {
                            _values[index] = entry[property];
                        }
                    }
                }
                else
                {
                    foreach (var property in entry.EntityType.GetProperties())
                    {
                        _values[property.GetIndex()] = entry[property];
                    }
                }
            }

            public bool IsEmpty => _values.IsEmpty;
        }
    }
}
