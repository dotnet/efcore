// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public sealed class WeakReferenceIdentityMap<TKey> : IWeakReferenceIdentityMap
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
        private IPrincipalKeyValueFactory<TKey> PrincipalKeyValueFactory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IKey Key
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WeakReference<object> TryGetEntity(
            in ValueBuffer valueBuffer,
            bool throwOnNullKey,
            out bool hasNullKey)
        {
            var key = PrincipalKeyValueFactory.CreateFromBuffer(valueBuffer);

            if (key == null)
            {
                if (throwOnNullKey)
                {
                    if (Key.IsPrimaryKey())
                    {
                        throw new InvalidOperationException(
                            CoreStrings.InvalidKeyValue(
                                Key.DeclaringEntityType.DisplayName(),
                                PrincipalKeyValueFactory.FindNullPropertyInValueBuffer(valueBuffer).Name));
                    }

                    throw new InvalidOperationException(
                        CoreStrings.InvalidAlternateKeyValue(
                            Key.DeclaringEntityType.DisplayName(),
                            PrincipalKeyValueFactory.FindNullPropertyInValueBuffer(valueBuffer).Name));
                }

                hasNullKey = true;
                return null;
            }

            hasNullKey = false;
            return _identityMap.TryGetValue((TKey)key, out var entity) ? entity : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CollectGarbage()
        {
            if (++_identityMapGarbageCollectionIterations == IdentityMapGarbageCollectionThreshold)
            {
                var deadEntries = new List<TKey>();

                foreach (var entry in _identityMap)
                {
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in ValueBuffer valueBuffer, object entity)
            => _identityMap[(TKey)PrincipalKeyValueFactory.CreateFromBuffer(valueBuffer)] = new WeakReference<object>(entity);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IIncludeKeyComparer CreateIncludeKeyComparer(INavigation navigation, in ValueBuffer valueBuffer)
        {
            return navigation.IsDependentToPrincipal()
                ? navigation.ForeignKey.GetDependentKeyValueFactory<TKey>().TryCreateFromBuffer(valueBuffer, out var keyValue)
                    ? (IIncludeKeyComparer)new DependentToPrincipalIncludeComparer<TKey>(keyValue, PrincipalKeyValueFactory)
                    : new NullIncludeComparer()
                : new PrincipalToDependentIncludeComparer<TKey>(
                (TKey)PrincipalKeyValueFactory.CreateFromBuffer(valueBuffer),
                navigation.ForeignKey.GetDependentKeyValueFactory<TKey>(),
                PrincipalKeyValueFactory);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IIncludeKeyComparer CreateIncludeKeyComparer(INavigation navigation, InternalEntityEntry entry)
        {
            return navigation.IsDependentToPrincipal()
                ? navigation.ForeignKey.GetDependentKeyValueFactory<TKey>().TryCreateFromCurrentValues(entry, out var keyValue)
                    ? new DependentToPrincipalIncludeComparer<TKey>(keyValue, PrincipalKeyValueFactory)
                    : (IIncludeKeyComparer)new NullIncludeComparer()
                : new PrincipalToDependentIncludeComparer<TKey>(
                PrincipalKeyValueFactory.CreateFromCurrentValues(entry),
                navigation.ForeignKey.GetDependentKeyValueFactory<TKey>(),
                PrincipalKeyValueFactory);
        }
    }
}
