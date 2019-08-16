// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public abstract partial class InternalEntityEntry
    {
        private readonly struct OriginalValues
        {
            private readonly ISnapshot _values;

            public OriginalValues(InternalEntityEntry entry)
            {
                _values = ((EntityType)entry.EntityType).OriginalValuesFactory(entry);
            }

            public object GetValue(InternalEntityEntry entry, IProperty property)
            {
                var index = property.GetOriginalValueIndex();

                if (index == -1)
                {
                    throw new InvalidOperationException(
                        CoreStrings.OriginalValueNotTracked(property.Name, property.DeclaringEntityType.DisplayName()));
                }

                return IsEmpty ? entry[property] : _values[index];
            }

            public T GetValue<T>(InternalEntityEntry entry, IProperty property, int index)
            {
                if (index == -1)
                {
                    throw new InvalidOperationException(
                        CoreStrings.OriginalValueNotTracked(property.Name, property.DeclaringEntityType.DisplayName()));
                }

                return IsEmpty ? entry.GetCurrentValue<T>(property) : _values.GetValue<T>(index);
            }

            public void SetValue(IProperty property, object value, int index)
            {
                Debug.Assert(!IsEmpty);

                if (index == -1)
                {
                    index = property.GetOriginalValueIndex();

                    if (index == -1)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.OriginalValueNotTracked(property.Name, property.DeclaringEntityType.DisplayName()));
                    }
                }

                if (value == null
                    && !property.ClrType.IsNullableType())
                {
                    throw new InvalidOperationException(
                        CoreStrings.ValueCannotBeNull(
                            property.Name, property.DeclaringEntityType.DisplayName(), property.ClrType.DisplayName()));
                }

                _values[index] = SnapshotValue(property, value);
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
                        entry[property] = SnapshotValue(property, _values[index]);
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
                        _values[index] = SnapshotValue(property, entry[property]);
                    }
                }
            }

            private static object SnapshotValue(IProperty property, object value)
            {
                var comparer = property.GetValueComparer() ?? property.FindTypeMapping()?.Comparer;

                return comparer == null ? value : comparer.Snapshot(value);
            }

            public bool IsEmpty => _values == null;
        }
    }
}
