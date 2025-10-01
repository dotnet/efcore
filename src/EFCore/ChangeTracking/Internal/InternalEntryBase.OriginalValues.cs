// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public partial class InternalEntryBase
{
    private struct OriginalValues(InternalEntryBase entry)
    {
        private ISnapshot _values = entry.StructuralType.OriginalValuesFactory(entry);

        public object? GetValue(IInternalEntry entry, IPropertyBase property)
            => property.GetOriginalValueIndex() is var index && index == -1
                ? throw new InvalidOperationException(
                    CoreStrings.OriginalValueNotTracked(property.Name, property.DeclaringType.DisplayName()))
                : IsEmpty
                    ? entry[property]
                    : _values[index];

        public T GetValue<T>(IInternalEntry entry, IPropertyBase property, int index)
            => index == -1
                ? throw new InvalidOperationException(
                    CoreStrings.OriginalValueNotTracked(property.Name, property.DeclaringType.DisplayName()))
                : IsEmpty
                    ? entry.GetCurrentValue<T>(property)
                    : _values.GetValue<T>(index);

        public void SetValue(IPropertyBase property, object? value, int index)
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
                        property.Name, property.DeclaringType.DisplayName(), property.ClrType.ShortDisplayName()));
            }

            _values[index] = SnapshotValue(property, value);
        }

        public void RejectChanges(IInternalEntry entry)
        {
            if (IsEmpty)
            {
                return;
            }

            // This isn't efficient, but avoids duplicating the logic
            new CurrentPropertyValues((InternalEntryBase)entry).SetValues(new OriginalPropertyValues((InternalEntryBase)entry));
        }

        public void AcceptChanges(IInternalEntry entry)
        {
            if (IsEmpty)
            {
                return;
            }

            _values = entry.StructuralType.OriginalValuesFactory(entry);
        }

        private static object? SnapshotValue(IPropertyBase propertyBase, object? value)
            => propertyBase switch
            {
                IProperty property => property.GetValueComparer().Snapshot(value),
                IComplexProperty complexProperty when complexProperty.IsCollection
                    => value is IList list
                        ? SnapshotFactoryFactory.SnapshotComplexCollection(list, (IRuntimeComplexProperty)complexProperty)
                        : value is null
                            ? null
                            : throw new InvalidOperationException(
                                CoreStrings.ComplexPropertyValueNotList(
                                    complexProperty.Name, complexProperty.ClrType, value.GetType().ShortDisplayName())),
                _ => value
            };

        public bool IsEmpty
            => _values == null;
    }
}
