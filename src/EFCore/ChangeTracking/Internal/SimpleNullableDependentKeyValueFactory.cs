// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SimpleNullableDependentKeyValueFactory<TKey> : IDependentKeyValueFactory<TKey>
        where TKey : struct
    {
        private readonly PropertyAccessors _propertyAccessors;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SimpleNullableDependentKeyValueFactory([NotNull] PropertyAccessors propertyAccessors)
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
            => HandleNullableValue(((Func<InternalEntityEntry, TKey?>)_propertyAccessors.CurrentValueGetter)(entry), out key);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TryCreateFromPreStoreGeneratedCurrentValues(InternalEntityEntry entry, out TKey key)
            => HandleNullableValue(((Func<InternalEntityEntry, TKey?>)_propertyAccessors.PreStoreGeneratedCurrentValueGetter)(entry), out key);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TryCreateFromOriginalValues(InternalEntityEntry entry, out TKey key)
            => HandleNullableValue(((Func<InternalEntityEntry, TKey?>)_propertyAccessors.OriginalValueGetter)(entry), out key);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TryCreateFromRelationshipSnapshot(InternalEntityEntry entry, out TKey key)
            => HandleNullableValue(((Func<InternalEntityEntry, TKey?>)_propertyAccessors.RelationshipSnapshotGetter)(entry), out key);

        private static bool HandleNullableValue(TKey? value, out TKey key)
        {
            if (value.HasValue)
            {
                key = (TKey)value;
                return true;
            }

            key = default;
            return false;
        }
    }
}
