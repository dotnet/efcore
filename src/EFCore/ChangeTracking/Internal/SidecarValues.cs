// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public sealed partial class InternalEntityEntry
{
    private readonly struct SidecarValues
    {
        private readonly ISnapshot _values;

        public SidecarValues(ISnapshot values)
        {
            _values = values;
        }

        public bool TryGetValue(int index, out object? value)
        {
            if (IsEmpty)
            {
                value = null;
                return false;
            }

            value = _values[index];
            return true;
        }

        public object? GetValue(int index)
            => _values[index];

        public T GetValue<T>(int index)
            => _values.GetValue<T>(index);

        public void SetValue(IProperty property, object? value, int index)
        {
            Check.DebugAssert(!IsEmpty, "sidecar is empty");

            if (value == null
                && !property.ClrType.IsNullableType())
            {
                throw new InvalidOperationException(
                    CoreStrings.ValueCannotBeNull(
                        property.Name, property.DeclaringType.DisplayName(), property.ClrType.DisplayName()));
            }

            _values[index] = SnapshotValue(property, value);
        }

        private static object? SnapshotValue(IProperty property, object? value)
            => property.GetValueComparer().Snapshot(value);

        public bool IsEmpty
            => _values == null;
    }
}
