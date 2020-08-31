// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class ProxyablePropertyBag : IDictionary<string, object>
    {
        private readonly IDictionary<string, object> _keyValueStore = new Dictionary<string, object>();

        public void Add(string key, object value)
            => _keyValueStore.Add(key, value);

        public bool ContainsKey(string key)
            => _keyValueStore.ContainsKey(key);

        public bool Remove(string key)
            => _keyValueStore.Remove(key);

        public bool TryGetValue(string key, out object value)
            => _keyValueStore.TryGetValue(key, out value);

        public virtual object this[string key]
        {
            get => _keyValueStore[key];
            set => _keyValueStore[key] = value;
        }

        public ICollection<string> Keys
            => _keyValueStore.Keys;

        public ICollection<object> Values
            => _keyValueStore.Values;

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            => _keyValueStore.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public void Add(KeyValuePair<string, object> item)
            => _keyValueStore.Add(item);

        public void Clear()
            => _keyValueStore.Clear();

        public bool Contains(KeyValuePair<string, object> item)
            => _keyValueStore.Contains(item);

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            => _keyValueStore.CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, object> item)
            => _keyValueStore.Remove(item);

        public int Count
            => _keyValueStore.Count;

        public bool IsReadOnly
            => _keyValueStore.IsReadOnly;
    }
}
