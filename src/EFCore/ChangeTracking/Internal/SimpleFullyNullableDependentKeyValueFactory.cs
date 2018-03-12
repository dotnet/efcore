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
    public class SimpleFullyNullableDependentKeyValueFactory<TKey> : IDependentKeyValueFactory<TKey>
    {
        private readonly PropertyAccessors _propertyAccessors;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SimpleFullyNullableDependentKeyValueFactory([NotNull] PropertyAccessors propertyAccessors)
        {
            _propertyAccessors = propertyAccessors;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TryCreateFromBuffer(in ValueBuffer valueBuffer, out TKey key)
        {
            key = (TKey)_propertyAccessors.ValueBufferGetter(valueBuffer);
            return key != null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TryCreateFromCurrentValues(InternalEntityEntry entry, out TKey key)
        {
            key = ((Func<InternalEntityEntry, TKey>)_propertyAccessors.CurrentValueGetter)(entry);
            return key != null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TryCreateFromPreStoreGeneratedCurrentValues(InternalEntityEntry entry, out TKey key)
        {
            key = ((Func<InternalEntityEntry, TKey>)_propertyAccessors.PreStoreGeneratedCurrentValueGetter)(entry);
            return key != null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TryCreateFromOriginalValues(InternalEntityEntry entry, out TKey key)
        {
            key = ((Func<InternalEntityEntry, TKey>)_propertyAccessors.OriginalValueGetter)(entry);
            return key != null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool TryCreateFromRelationshipSnapshot(InternalEntityEntry entry, out TKey key)
        {
            key = ((Func<InternalEntityEntry, TKey>)_propertyAccessors.RelationshipSnapshotGetter)(entry);
            return key != null;
        }
    }
}
