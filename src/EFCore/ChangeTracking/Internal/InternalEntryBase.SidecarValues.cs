// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public partial class InternalEntryBase
{
    private readonly struct SidecarValues(ISnapshot values)
    {
        public bool TryGetValue(int index, out object? value)
        {
            if (IsEmpty)
            {
                value = null;
                return false;
            }

            value = values[index];
            return true;
        }

        public object? GetValue(int index)
            => values[index];

        public T GetValue<T>(int index)
            => values.GetValue<T>(index);

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

            values[index] = SnapshotValue(property, value);
        }

        private static object? SnapshotValue(IProperty property, object? value)
            => property.GetValueComparer().Snapshot(value);

        public bool IsEmpty
            => values == null;
    }
}
