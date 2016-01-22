// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    // The methods here box, but this is only used when the primary key is nullable, but the FK is non-nullable,
    // which is not common.
    public class SimpleNullablePrincipalDependentKeyValueFactory<TKey, TNonNullableKey> : IDependentKeyValueFactory<TKey>
        where TNonNullableKey : struct
    {
        private readonly PropertyAccessors _propertyAccessors;

        public SimpleNullablePrincipalDependentKeyValueFactory([NotNull] PropertyAccessors propertyAccessors)
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
        {
            key = (TKey)(object)((Func<InternalEntityEntry, TNonNullableKey>)_propertyAccessors.CurrentValueGetter)(entry);
            return true;
        }

        public virtual bool TryCreateFromOriginalValues(InternalEntityEntry entry, out TKey key)
        {
            key = (TKey)(object)((Func<InternalEntityEntry, TNonNullableKey>)_propertyAccessors.OriginalValueGetter)(entry);
            return true;
        }

        public virtual bool TryCreateFromRelationshipSnapshot(InternalEntityEntry entry, out TKey key)
        {
            key = (TKey)(object)((Func<InternalEntityEntry, TNonNullableKey>)_propertyAccessors.RelationshipSnapshotGetter)(entry);
            return true;
        }
    }
}
