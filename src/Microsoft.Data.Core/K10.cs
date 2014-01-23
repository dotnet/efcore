// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if K10

namespace System.Collections.Immutable
{
    using System.Collections.Generic;

    public class ImmutableList<T> : IEnumerable<T>
    {
        public static readonly ImmutableList<T> Empty = new ImmutableList<T>();

        private readonly List<T> _list = new List<T>();

        public ImmutableList<T> Clear()
        {
            _list.Clear();

            return this;
        }

        public ImmutableList<T> Remove(T item)
        {
            _list.Remove(item);

            return this;
        }

        public ImmutableList<T> AddRange(IEnumerable<T> items)
        {
            _list.AddRange(items);

            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }

    public class ImmutableDictionary<TKey, TValue>// : IImmutableDictionary<TKey, TValue>
    {
        public static readonly ImmutableDictionary<TKey, TValue> Empty = new ImmutableDictionary<TKey, TValue>();

        private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

        public ImmutableDictionary<TKey, TValue> Clear()
        {
            _dictionary.Clear();

            return this;
        }

        public ImmutableDictionary<TKey, TValue> Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);

            return this;
        }

        public ImmutableDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            foreach (var keyValuePair in items)
            {
                _dictionary[keyValuePair.Key] = keyValuePair.Value;
            }

            return this;
        }

        public ImmutableDictionary<TKey, TValue> Remove(TKey key)
        {
            _dictionary.Remove(key);

            return this;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public IEnumerable<TValue> Values
        {
            get { return _dictionary.Values; }
        }
    }
}

#endif