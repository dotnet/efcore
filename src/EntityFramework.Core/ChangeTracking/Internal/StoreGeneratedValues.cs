// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public abstract partial class InternalEntityEntry
    {
        private struct StoreGeneratedValues
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
                        ? default(T)
                        : (T)value;
            }

            public bool CanStoreValue(IPropertyBase propertyBase)
                => (_values != null)
                   && (propertyBase.GetStoreGeneratedIndex() != -1);

            public void SetValue(IPropertyBase propertyBase, object value)
            {
                var index = propertyBase.GetStoreGeneratedIndex();

                Debug.Assert(index != -1);

                _values[index] = value ?? _nullSentinel;
            }
        }
    }
}
