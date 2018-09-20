// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public abstract partial class InternalEntityEntry
    {
        private readonly struct StoreGeneratedValues
        {
            private static readonly object _nullSentinel = new object();

            private readonly object[] _values;

            public StoreGeneratedValues(InternalEntityEntry entry)
            {
                var entityType = entry.EntityType;
                _values = new object[entityType.StoreGeneratedCount()];
            }

            public bool IsEmpty => _values == null;

            public bool TryGetValue(IPropertyBase propertyBase, out object value)
            {
                if (_values == null)
                {
                    value = null;
                    return false;
                }

                var index = propertyBase.GetStoreGeneratedIndex();
                if (index == -1)
                {
                    value = null;
                    return false;
                }

                value = _values[index];
                if (value == null)
                {
                    return false;
                }

                if (value == _nullSentinel)
                {
                    value = null;
                }

                return true;
            }

            public T GetValue<T>(T currentValue, int index)
            {
                if (IsEmpty)
                {
                    return currentValue;
                }

                var value = _values[index];

                return value == null
                    ? currentValue
                    : value == _nullSentinel
                        ? default
                        : (T)value;
            }

            public bool CanStoreValue(IPropertyBase propertyBase)
                => _values != null
                   && propertyBase.GetStoreGeneratedIndex() != -1;

            public void SetValue(IPropertyBase propertyBase, object value)
            {
                var index = propertyBase.GetStoreGeneratedIndex();

                Debug.Assert(index != -1);

                if (!((IProperty)propertyBase).IsNullable
                    && value == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.DatabaseGeneratedNull(propertyBase.Name, propertyBase.DeclaringType.DisplayName()));
                }

                _values[index] = SnapshotValue(propertyBase, value) ?? _nullSentinel;
            }

            private static object SnapshotValue(IPropertyBase propertyBase, object value)
            {
                if (propertyBase is IProperty property)
                {
                    var comparer = property.GetValueComparer() ?? property.FindMapping()?.Comparer;

                    if (comparer != null)
                    {
                        return comparer.Snapshot(value);
                    }
                }

                return value;
            }
        }
    }
}
