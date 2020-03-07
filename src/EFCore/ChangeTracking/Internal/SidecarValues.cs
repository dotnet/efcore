// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public abstract partial class InternalEntityEntry
    {
        private readonly struct SidecarValues
        {
            private readonly ISnapshot _values;

            public SidecarValues(ISnapshot valuesFactory)
            {
                _values = valuesFactory;
            }

            public bool TryGetValue(int index, out object value)
            {
                if (IsEmpty)
                {
                    value = null;
                    return false;
                }

                value = _values[index];
                return true;
            }

            public T GetValue<T>(int index)
                => IsEmpty ? default : _values.GetValue<T>(index);

            public void SetValue(IProperty property, object value, int index)
            {
                Debug.Assert(!IsEmpty);

                if (value == null
                    && !property.ClrType.IsNullableType())
                {
                    throw new InvalidOperationException(
                        CoreStrings.ValueCannotBeNull(
                            property.Name, property.DeclaringEntityType.DisplayName(), property.ClrType.DisplayName()));
                }

                _values[index] = SnapshotValue(property, value);
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
