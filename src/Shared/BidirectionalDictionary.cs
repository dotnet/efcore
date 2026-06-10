// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    /// <summary>
    /// Represents a dictionary with non-null unique values that contains an inverse dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [DebuggerTypeProxy(typeof(IDictionaryDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    internal sealed class BidirectionalDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, TValue> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="BidirectionalDictionary{TKey, TValue}"/> class that is empty,
        /// has the default initial capacity, and uses the default equality comparers.
        /// </summary>
        public BidirectionalDictionary() : this(new Dictionary<TKey, TValue>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BidirectionalDictionary{TKey, TValue}"/> class that is empty,
        /// has the specified initial capacity, and uses the default equality comparers.
        /// </summary>
        /// <param name="capacity">The initial number of elements that <see cref="BidirectionalDictionary{TKey, TValue}"/> can contain.</param>
        public BidirectionalDictionary(int capacity) : this(capacity, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BidirectionalDictionary{TKey, TValue}"/> class
        /// that contains elements copied from the specified <see cref="IDictionary{TKey, TValue}"/>
        /// and uses the default equality comparers.
        /// </summary>
        /// <param name="dictionary">The <see cref="IDictionary{TKey, TValue}"/> whose elements are copied to the new <see cref="BidirectionalDictionary{TKey, TValue}"/>.</param>
        public BidirectionalDictionary(IDictionary<TKey, TValue> dictionary)
            : this(new Dictionary<TKey, TValue>(dictionary))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BidirectionalDictionary{TKey, TValue}"/> class
        /// that contains elements copied from the specified <see cref="IEnumerable{T}"/>
        /// and uses the default equality comparers.
        /// </summary>
        /// <param name="collection">The <see cref="IEnumerable{T}"/> whose elements are copied to the new <see cref="BidirectionalDictionary{TKey, TValue}"/>.</param>
        public BidirectionalDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : this(new Dictionary<TKey, TValue>(collection))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BidirectionalDictionary{TKey, TValue}"/> class that is empty,
        /// has the default initial capacity, and uses the specified equality comparers.
        /// </summary>
        /// <param name="keyComparer">The <see cref="IEqualityComparer{T}"/> implementation to use when
        /// comparing keys, or null to use the default <see cref="IEqualityComparer{T}"/> for the type of the key.</param>
        /// <param name="valueComparer">The <see cref="IEqualityComparer{T}"/> implementation to use when
        /// comparing values, or null to use the default <see cref="IEqualityComparer{T}"/> for the type of the value.</param>
        public BidirectionalDictionary(IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
            : this(new Dictionary<TKey, TValue>(keyComparer), valueComparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BidirectionalDictionary{TKey, TValue}"/> class that is empty,
        /// has the specified initial capacity, and uses the specified equality comparers.
        /// </summary>
        /// <param name="capacity">The initial number of elements that <see cref="BidirectionalDictionary{TKey, TValue}"/> can contain.</param>
        /// <param name="keyComparer">The <see cref="IEqualityComparer{T}"/> implementation to use when
        /// comparing keys, or null to use the default <see cref="IEqualityComparer{T}"/> for the type of the key.</param>
        /// <param name="valueComparer">The <see cref="IEqualityComparer{T}"/> implementation to use when
        /// comparing values, or null to use the default <see cref="IEqualityComparer{T}"/> for the type of the value.</param>
        public BidirectionalDictionary(int capacity, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
            : this(new Dictionary<TKey, TValue>(capacity, keyComparer), valueComparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BidirectionalDictionary{TKey, TValue}"/> class that
        /// contains elements copied from the specified <see cref="IDictionary{TKey, TValue}"/>, and uses the specified equality comparers.
        /// </summary>
        /// <param name="dictionary">The <see cref="IDictionary{TKey, TValue}"/> whose elements are copied to the new <see cref="BidirectionalDictionary{TKey, TValue}"/>.</param>
        /// <param name="keyComparer">The <see cref="IEqualityComparer{T}"/> implementation to use when
        /// comparing keys, or null to use the default <see cref="IEqualityComparer{T}"/> for the type of the key.</param>
        /// <param name="valueComparer">The <see cref="IEqualityComparer{T}"/> implementation to use when
        /// comparing values, or null to use the default <see cref="IEqualityComparer{T}"/> for the type of the value.</param>
        public BidirectionalDictionary(
            IDictionary<TKey, TValue> dictionary,
            IEqualityComparer<TKey>? keyComparer,
            IEqualityComparer<TValue>? valueComparer)
            : this(new Dictionary<TKey, TValue>(dictionary, keyComparer), valueComparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BidirectionalDictionary{TKey, TValue}"/> class that
        /// contains elements copied from the specified <see cref="IEnumerable{T}"/>, and uses the specified equality comparers.
        /// </summary>
        /// <param name="collection">The <see cref="IEnumerable{T}"/> whose elements are copied to the new <see cref="BidirectionalDictionary{TKey, TValue}"/>.</param>
        /// <param name="keyComparer">The <see cref="IEqualityComparer{T}"/> implementation to use when
        /// comparing keys, or null to use the default <see cref="IEqualityComparer{T}"/> for the type of the key.</param>
        /// <param name="valueComparer">The <see cref="IEqualityComparer{T}"/> implementation to use when
        /// comparing values, or null to use the default <see cref="IEqualityComparer{T}"/> for the type of the value.</param>
        public BidirectionalDictionary(
            IEnumerable<KeyValuePair<TKey, TValue>> collection,
            IEqualityComparer<TKey>? keyComparer,
            IEqualityComparer<TValue>? valueComparer)
            : this(new Dictionary<TKey, TValue>(collection, keyComparer), valueComparer)
        {
        }

        private BidirectionalDictionary(BidirectionalDictionary<TValue, TKey> inverse, IEqualityComparer<TKey>? keyComparer = null)
        {
            _dictionary = inverse._dictionary.ToDictionary(pair => pair.Value, pair => pair.Key, keyComparer);
            Inverse = inverse;
        }

        private BidirectionalDictionary(Dictionary<TKey, TValue> dictionary, IEqualityComparer<TValue>? valueComparer = null)
        {
            _dictionary = dictionary;
            Inverse = new BidirectionalDictionary<TValue, TKey>(this, valueComparer ?? EqualityComparer<TValue>.Default);
        }

        /// <summary>
        /// Gets the inverse <see cref="BidirectionalDictionary{TKey,TValue}"/>.
        /// </summary>
        public BidirectionalDictionary<TValue, TKey> Inverse { get; }

        /// <summary>
        /// Gets the number of key/value pairs contained in the <see cref="BidirectionalDictionary{TKey, TValue}"/>.
        /// </summary>
        public int Count => _dictionary.Count;

        /// <summary>
        /// Gets a collection containing the keys in the <see cref="BidirectionalDictionary{TKey, TValue}"/>.
        /// </summary>
        public Dictionary<TKey, TValue>.KeyCollection Keys => _dictionary.Keys;

        /// <summary>
        /// Gets a collection containing the values in the <see cref="BidirectionalDictionary{TKey, TValue}"/>.
        /// </summary>
        public Dictionary<TKey, TValue>.ValueCollection Values => _dictionary.Values;

        /// <summary>
        /// Gets the <see cref="IEqualityComparer{T}"/> that is used to determine equality of keys for the dictionary.
        /// </summary>
        public IEqualityComparer<TKey> KeyValueComparer => _dictionary.Comparer;

        /// <summary>
        /// Gets the <see cref="IEqualityComparer{T}"/> that is used to determine equality of values for the dictionary.
        /// </summary>
        public IEqualityComparer<TValue> ValueComparer => Inverse._dictionary.Comparer;

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a
        /// <see cref="KeyNotFoundException"/>, and a set operation creates a new element with the specified key.</returns>
        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                Check.NotNull(value, nameof(value));

                if (TryGetValue(key, out var oldValue))
                {
                    if (ValueComparer.Equals(oldValue, value))
                    {
                        return;
                    }

                    Inverse._dictionary.Add(value, key);
                    Inverse._dictionary.Remove(oldValue);
                    _dictionary[key] = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                _dictionary.Add(key, value);
            }

            Inverse._dictionary.Add(value, key);
            _dictionary.Add(key, value);
        }

        /// <summary>
        /// Removes the value with the specified key from the <see cref="BidirectionalDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise, <see langword="false"/>.
        /// This method returns <see langword="false"/> if key is not found in the <see cref="BidirectionalDictionary{TKey, TValue}"/>.</returns>
        public bool Remove(TKey key) => Remove(key, out _);

        /// <summary>
        /// Removes the value with the specified key from the <see cref="BidirectionalDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key,
        /// if the key is found; otherwise, the default value for the type of the value parameter.
        /// This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise, <see langword="false"/>.
        /// This method returns <see langword="false"/> if key is not found in the <see cref="BidirectionalDictionary{TKey, TValue}"/>.</returns>
        public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
            => _dictionary.Remove(key, out value)
            && Inverse._dictionary.Remove(value);

        /// <summary>
        /// Removes all keys and values from the <see cref="BidirectionalDictionary{TKey, TValue}"/>.
        /// </summary>
        public void Clear()
        {
            _dictionary.Clear();
            Inverse._dictionary.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="BidirectionalDictionary{TKey, TValue}"/> contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="BidirectionalDictionary{TKey, TValue}"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="BidirectionalDictionary{TKey, TValue}"/> contains
        /// an element with the specified key; otherwise, <see langword="false"/>.</returns>
        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        /// <summary>
        /// Determines whether the <see cref="BidirectionalDictionary{TKey, TValue}"/> contains the specified value.
        /// </summary>
        /// <param name="value">The value to locate in the <see cref="BidirectionalDictionary{TKey, TValue}"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="BidirectionalDictionary{TKey, TValue}"/> contains
        /// an element with the specified value; otherwise, <see langword="false"/>.</returns>
        public bool ContainsValue(TValue value) => Inverse._dictionary.ContainsKey(value);

        /// <summary>
        /// Attempts to add the specified key and value to the <see cref="BidirectionalDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns><see langword="true"/> if the key/value pair was added to the <see cref="BidirectionalDictionary{TKey, TValue}"/>
        /// successfully; otherwise, <see langword="false"/>.</returns>
        public bool TryAdd(TKey key, TValue value)
        {
            if (ContainsKey(key) || ContainsValue(value))
            {
                return false;
            }

            _dictionary.Add(key, value);
            Inverse._dictionary.Add(value, key);

            return true;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key,
        /// if the key is found; otherwise, the default value for the type of the value parameter.
        /// This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the <see cref="BidirectionalDictionary{TKey, TValue}"/> contains
        /// an element with the specified key; otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _dictionary.TryGetValue(key, out value);

        /// <summary>
        /// Resizes the internal data structure if necessary to ensure no additional resizing to support the specified capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the <see cref="BidirectionalDictionary{TKey, TValue}" /> must be able to contain.</param>
        /// <returns>The capacity of the <see cref="BidirectionalDictionary{TKey, TValue}" />.</returns>
        public int EnsureCapacity(int capacity)
        {
            _dictionary.EnsureCapacity(capacity);
            return Inverse._dictionary.EnsureCapacity(capacity);
        }

        /// <summary>
        /// Sets the capacity of an <see cref="BidirectionalDictionary{TKey, TValue}" /> object to the actual number of elements it contains,
        /// rounded up to a nearby, implementation-specific value.
        /// </summary>
        public void TrimExcess()
        {
            _dictionary.TrimExcess();
            Inverse._dictionary.TrimExcess();
        }

        /// <summary>
        /// Sets the capacity of an <see cref="BidirectionalDictionary{TKey, TValue}" /> object to the specified capacity, rounded up to a nearby,
        /// implementation-specific value.
        /// </summary>
        /// <param name="capacity">The number of elements that the <see cref="BidirectionalDictionary{TKey, TValue}" /> must be able to contain.</param>
        public void TrimExcess(int capacity)
        {
            _dictionary.TrimExcess(capacity);
            Inverse._dictionary.TrimExcess(capacity);
        }

        /// <summary>
        /// Returns an <see cref="IReadOnlyDictionary{TKey, TValue}"></see> wrapper for the current dictionary.
        /// </summary>
        /// <returns>An object that acts as a read-only wrapper around the current <see cref="BidirectionalDictionary{TKey, TValue}"></see>.</returns>
        public IReadOnlyDictionary<TKey, TValue> AsReadOnly() => _dictionary;

        public IEnumerator GetEnumerator() => _dictionary.GetEnumerator();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
            => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Remove(item)
            && Inverse._dictionary.Remove(item.Value);

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
            => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            => _dictionary.GetEnumerator();
    }
}
