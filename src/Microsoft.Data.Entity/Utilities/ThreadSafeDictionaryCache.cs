// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
