// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class WeakReferenceIdentityMap<TKey> : IWeakReferenceIdentityMap
    {
        private const int IdentityMapGarbageCollectionThreshold = 500;

        private int _identityMapGarbageCollectionIterations;

        private readonly Dictionary<TKey, WeakReference<object>> _identityMap
            = new Dictionary<TKey, WeakReference<object>>();

        public WeakReferenceIdentityMap(
            [NotNull] IKey key,
            [NotNull] IPrincipalKeyValueFactory<TKey> principalKeyValueFactory)
        {
            Key = key;
            PrincipalKeyValueFactory = principalKeyValueFactory;
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
                GetDependentKeyValueFactory(navigation.ForeignKey).TryCreateFromBuffer(valueBuffer, out keyValue);
                return new DependentToPrincipalIncludeComparer<TKey>(keyValue, PrincipalKeyValueFactory);
            }

            return new PrincipalToDependentIncludeComparer<TKey>(
                (TKey)PrincipalKeyValueFactory.CreateFromBuffer(valueBuffer),
                GetDependentKeyValueFactory(navigation.ForeignKey));
        }

        public virtual IIncludeKeyComparer CreateIncludeKeyComparer(INavigation navigation, InternalEntityEntry entry)
        {
            if (navigation.IsDependentToPrincipal())
            {
                TKey keyValue;
                GetDependentKeyValueFactory(navigation.ForeignKey).TryCreateFromCurrentValues(entry, out keyValue);
                return new DependentToPrincipalIncludeComparer<TKey>(keyValue, PrincipalKeyValueFactory);
            }

            return new PrincipalToDependentIncludeComparer<TKey>(
                PrincipalKeyValueFactory.CreateFromCurrentValues(entry),
                GetDependentKeyValueFactory(navigation.ForeignKey));
        }

        private static IDependentKeyValueFactory<TKey> GetDependentKeyValueFactory(IForeignKey foreignKey)
        {
            var factorySource = foreignKey as IDependentKeyValueFactorySource;

            return factorySource != null
                ? (IDependentKeyValueFactory<TKey>)factorySource.DependentKeyValueFactory
                : new DependentKeyValueFactoryFactory().Create<TKey>(foreignKey);
        }
    }
}
