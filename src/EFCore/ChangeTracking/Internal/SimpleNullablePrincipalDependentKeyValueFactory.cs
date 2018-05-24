// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    // The methods here box, but this is only used when the primary key is nullable, but the FK is non-nullable,
    // which is not common.
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SimpleNullablePrincipalDependentKeyValueFactory<TKey, TNonNullableKey> : IDependentKeyValueFactory<TKey>
        where TNonNullableKey : struct
    {
        private readonly PropertyAccessors _propertyAccessors;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SimpleNullablePrincipalDependentKeyValueFactory([NotNull] PropertyAccessors propertyAccessors)
        {
            _propertyAccessors = propertyAccessors;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TryCreateFromBuffer(in ValueBuffer valueBuffer, out TKey key)
        {
            var value = _propertyAccessors.ValueBufferGetter(valueBuffer);
            if (value == null)
            {
                key = default;
                return false;
            }

            key = (TKey)value;
            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TryCreateFromCurrentValues(InternalEntityEntry entry, out TKey key)
        {
            key = (TKey)(object)((Func<InternalEntityEntry, TNonNullableKey>)_propertyAccessors.CurrentValueGetter)(entry);
            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TryCreateFromPreStoreGeneratedCurrentValues(InternalEntityEntry entry, out TKey key)
        {
            key = (TKey)(object)((Func<InternalEntityEntry, TNonNullableKey>)_propertyAccessors.PreStoreGeneratedCurrentValueGetter)(entry);
            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TryCreateFromOriginalValues(InternalEntityEntry entry, out TKey key)
        {
            key = (TKey)(object)((Func<InternalEntityEntry, TNonNullableKey>)_propertyAccessors.OriginalValueGetter)(entry);
            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TryCreateFromRelationshipSnapshot(InternalEntityEntry entry, out TKey key)
        {
            key = (TKey)(object)((Func<InternalEntityEntry, TNonNullableKey>)_propertyAccessors.RelationshipSnapshotGetter)(entry);
            return true;
        }
    }
}
