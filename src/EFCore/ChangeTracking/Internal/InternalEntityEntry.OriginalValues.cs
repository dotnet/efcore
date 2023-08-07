// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public sealed partial class InternalEntityEntry
{
    private readonly struct OriginalValues
    {
        private readonly ISnapshot _values;

        public OriginalValues(IInternalEntry entry)
        {
            _values = entry.StructuralType.OriginalValuesFactory(entry);
        }

        public object? GetValue(IInternalEntry entry, IProperty property)
        {
            var index = property.GetOriginalValueIndex();
            if (index == -1)
            {
                throw new InvalidOperationException(
                    CoreStrings.OriginalValueNotTracked(property.Name, property.DeclaringType.DisplayName()));
            }

            return IsEmpty ? entry[property] : _values[index];
        }

        public T GetValue<T>(IInternalEntry entry, IProperty property, int index)
        {
            if (index == -1)
            {
                throw new InvalidOperationException(
                    CoreStrings.OriginalValueNotTracked(property.Name, property.DeclaringType.DisplayName()));
            }

            return IsEmpty ? entry.GetCurrentValue<T>(property) : _values.GetValue<T>(index);
        }

        public void SetValue(IProperty property, object? value, int index)
        {
            Check.DebugAssert(!IsEmpty, "Original values are empty");

            if (index == -1)
            {
                index = property.GetOriginalValueIndex();

                if (index == -1)
                {
                    throw new InvalidOperationException(
                        CoreStrings.OriginalValueNotTracked(property.Name, property.DeclaringType.DisplayName()));
                }
            }

            if (value == null
                && !property.ClrType.IsNullableType())
            {
                throw new InvalidOperationException(
                    CoreStrings.ValueCannotBeNull(
                        property.Name, property.DeclaringType.DisplayName(), property.ClrType.DisplayName()));
            }

            _values[index] = SnapshotValue(property, value);
        }

        public void RejectChanges(IInternalEntry entry)
        {
            if (IsEmpty)
            {
                return;
            }

            foreach (var property in entry.StructuralType.GetProperties())
            {
                var index = property.GetOriginalValueIndex();
                if (index >= 0)
                {
                    entry[property] = SnapshotValue(property, _values[index]);
                }
            }
        }

        public void AcceptChanges(IInternalEntry entry)
        {
            if (IsEmpty)
            {
                return;
            }

            foreach (var property in entry.StructuralType.GetProperties())
            {
                var index = property.GetOriginalValueIndex();
                if (index >= 0)
                {
                    _values[index] = SnapshotValue(property, entry[property]);
                }
            }
        }

        private static object? SnapshotValue(IProperty property, object? value)
            => property.GetValueComparer().Snapshot(value);

        public bool IsEmpty
            => _values == null;
    }
}
