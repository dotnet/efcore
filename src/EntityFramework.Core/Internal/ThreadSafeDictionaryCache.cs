// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Internal
{
    public class ThreadSafeDictionaryCache<TKey, TValue>
    {
        private readonly ThreadSafeLazyRef<ImmutableDictionary<TKey, TValue>> _cache;

        public ThreadSafeDictionaryCache()
            : this(null)
        {
        }

        public ThreadSafeDictionaryCache([CanBeNull] IEqualityComparer<TKey> equalityComparer)
        {
            _cache
                = new ThreadSafeLazyRef<ImmutableDictionary<TKey, TValue>>(
                    () => ImmutableDictionary<TKey, TValue>
                        .Empty
                        .WithComparers(equalityComparer));
        }

        public virtual TValue GetOrAdd([NotNull] TKey key, [NotNull] Func<TKey, TValue> factory)
        {
            TValue value;
            if (!_cache.Value.TryGetValue(key, out value))
            {
                var newValue = factory(key);

                _cache.ExchangeValue(d => d.ContainsKey(key) ? d : d.Add(key, newValue));

                value = _cache.Value[key];
            }

            return value;
        }
    }
}
