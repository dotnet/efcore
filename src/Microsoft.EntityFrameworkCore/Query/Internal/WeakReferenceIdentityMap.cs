// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class WeakReferenceIdentityMap<TKey> : IWeakReferenceIdentityMap
    {
        private const int IdentityMapGarbageCollectionThreshold = 500;
        private int _identityMapGarbageCollectionIterations;
        private readonly Dictionary<TKey, WeakReference<object>> _identityMap;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public WeakReferenceIdentityMap(
            [NotNull] IKey key,
            [NotNull] IPrincipalKeyValueFactory<TKey> principalKeyValueFactory)
        {
            Key = key;
            PrincipalKeyValueFactory = principalKeyValueFactory;

            _identityMap = new Dictionary<TKey, WeakReference<object>>(principalKeyValueFactory.EqualityComparer);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IPrincipalKeyValueFactory<TKey> PrincipalKeyValueFactory { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IKey Key { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual WeakReference<object> TryGetEntity(ValueBuffer valueBuffer, out bool hasNullKey)
        {
            var key = PrincipalKeyValueFactory.CreateFromBuffer(valueBuffer);
            if (key == null)
            {
                hasNullKey = true;
                return null;
            }

            hasNullKey = false;
            WeakReference<object> entity;
            return _identityMap.TryGetValue((TKey)key, out entity) ? entity : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void CollectGarbage()
        {
            if (++_identityMapGarbageCollectionIterations == IdentityMapGarbageCollectionThreshold)
            {
                var deadEntries = new List<TKey>();

                foreach (var entry in _identityMap)
                {
                    object _;
                    if (!entry.Value.TryGetTarget(out _))
                    {
                        deadEntries.Add(entry.Key);
                    }
                }

                foreach (var keyValue in deadEntries)
                {
                    _identityMap.Remove(keyValue);
                }

                _identityMapGarbageCollectionIterations = 0;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Add(ValueBuffer valueBuffer, object entity)
            => _identityMap[(TKey)PrincipalKeyValueFactory.CreateFromBuffer(valueBuffer)] = new WeakReference<object>(entity);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IIncludeKeyComparer CreateIncludeKeyComparer(INavigation navigation, ValueBuffer valueBuffer)
        {
            if (navigation.IsDependentToPrincipal())
            {
                TKey keyValue;
                return navigation.ForeignKey.GetDependentKeyValueFactory<TKey>().TryCreateFromBuffer(valueBuffer, out keyValue)
                    ? (IIncludeKeyComparer)new DependentToPrincipalIncludeComparer<TKey>(keyValue, PrincipalKeyValueFactory)
                    : new NullIncludeComparer();
            }

            return new PrincipalToDependentIncludeComparer<TKey>(
                (TKey)PrincipalKeyValueFactory.CreateFromBuffer(valueBuffer),
                navigation.ForeignKey.GetDependentKeyValueFactory<TKey>(),
                PrincipalKeyValueFactory);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IIncludeKeyComparer CreateIncludeKeyComparer(INavigation navigation, InternalEntityEntry entry)
        {
            if (navigation.IsDependentToPrincipal())
            {
                TKey keyValue;
                return navigation.ForeignKey.GetDependentKeyValueFactory<TKey>().TryCreateFromCurrentValues(entry, out keyValue)
                    ? new DependentToPrincipalIncludeComparer<TKey>(keyValue, PrincipalKeyValueFactory)
                    : (IIncludeKeyComparer)new NullIncludeComparer();
            }

            return new PrincipalToDependentIncludeComparer<TKey>(
                PrincipalKeyValueFactory.CreateFromCurrentValues(entry),
                navigation.ForeignKey.GetDependentKeyValueFactory<TKey>(),
                PrincipalKeyValueFactory);
        }
    }
}
