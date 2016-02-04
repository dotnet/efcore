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
    public class WeakReferenceIdentityMap<TKey> : IWeakReferenceIdentityMap
    {
        private const int IdentityMapGarbageCollectionThreshold = 500;

        private readonly Dictionary<TKey, WeakReference<object>> _identityMap;

        private int _identityMapGarbageCollectionIterations;

        public WeakReferenceIdentityMap(
            [NotNull] IKey key,
            [NotNull] IPrincipalKeyValueFactory<TKey> principalKeyValueFactory)
        {
            Key = key;
            PrincipalKeyValueFactory = principalKeyValueFactory;

            _identityMap = new Dictionary<TKey, WeakReference<object>>(principalKeyValueFactory.EqualityComparer);
        }

        protected virtual IPrincipalKeyValueFactory<TKey> PrincipalKeyValueFactory { get; }

        public virtual IKey Key { get; }

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

        public virtual void Add(ValueBuffer valueBuffer, object entity)
            => _identityMap[(TKey)PrincipalKeyValueFactory.CreateFromBuffer(valueBuffer)] = new WeakReference<object>(entity);

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
