// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Utilities
{
    public class ThreadSafeDictionaryCache<TKey, TValue>
    {
        private readonly ThreadSafeLazyRef<ImmutableDictionary<TKey, TValue>> _cache
            = new ThreadSafeLazyRef<ImmutableDictionary<TKey, TValue>>(() => ImmutableDictionary<TKey, TValue>.Empty);

        public virtual TValue GetOrAdd([NotNull] TKey key, [NotNull] Func<TKey, TValue> factory)
        {
            Check.NotNull(key, "key");
            Check.NotNull("source", "factory");

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
