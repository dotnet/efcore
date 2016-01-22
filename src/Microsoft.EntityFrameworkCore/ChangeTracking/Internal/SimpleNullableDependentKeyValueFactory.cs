// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class SimpleNullableDependentKeyValueFactory<TKey> : IDependentKeyValueFactory<TKey>
        where TKey : struct
    {
        private readonly PropertyAccessors _propertyAccessors;

        public SimpleNullableDependentKeyValueFactory([NotNull] PropertyAccessors propertyAccessors)
        {
            _propertyAccessors = propertyAccessors;
        }

        public virtual bool TryCreateFromBuffer(ValueBuffer valueBuffer, out TKey key)
        {
            var value = _propertyAccessors.ValueBufferGetter(valueBuffer);
            if (value == null)
            {
                key = default(TKey);
                return false;
            }
            key = (TKey)value;
            return true;
        }

        public virtual bool TryCreateFromCurrentValues(InternalEntityEntry entry, out TKey key)
            => HandleNullableValue(((Func<InternalEntityEntry, TKey?>)_propertyAccessors.CurrentValueGetter)(entry), out key);

        public virtual bool TryCreateFromOriginalValues(InternalEntityEntry entry, out TKey key)
            => HandleNullableValue(((Func<InternalEntityEntry, TKey?>)_propertyAccessors.OriginalValueGetter)(entry), out key);

        public virtual bool TryCreateFromRelationshipSnapshot(InternalEntityEntry entry, out TKey key)
            => HandleNullableValue(((Func<InternalEntityEntry, TKey?>)_propertyAccessors.RelationshipSnapshotGetter)(entry), out key);

        private static bool HandleNullableValue(TKey? value, out TKey key)
        {
            if (value.HasValue)
            {
                key = (TKey)value;
                return true;
            }

            key = default(TKey);
            return false;
        }
    }
}
