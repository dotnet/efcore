// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Collections;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    internal enum InsertionBehavior
    {
        None = 0,
        OverwriteExisting = 1,
        ThrowOnExisting = 2
    }

    /// <summary>
    /// Represents an ordered collection of keys and values with the same performance as <see cref="Dictionary{TKey, TValue}"/> with O(1) lookups and adds but with O(n) inserts and removes.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [DebuggerTypeProxy(typeof(IDictionaryDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    internal sealed partial class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IList<KeyValuePair<TKey, TValue>>, IReadOnlyList<KeyValuePair<TKey, TValue>>
    {
        private struct Entry
        {
            public uint HashCode;
            public TKey Key;
            public TValue Value;
            public int Next; // the index of the next item in the same bucket, -1 if last
        }

        // We want to initialize without allocating arrays. We also want to avoid null checks.
        // Array.Empty would give divide by zero in modulo operation. So we use static one element arrays.
        // The first add will cause a resize replacing these with real arrays of three elements.
        // Arrays are wrapped in a class to avoid being duplicated for each <TKey, TValue>
        private static readonly Entry[] InitialEntries = new Entry[1];
        // 1-based index into _entries; 0 means empty
        private int[] _buckets = HashHelpers.SizeOneIntArray;
        // remains contiguous and maintains order
        private Entry[] _entries = InitialEntries;
        private int _count;
        private int _version;
        // is null when comparer is EqualityComparer<TKey>.Default so that the GetHashCode method is used explicitly on the object
        private readonly IEqualityComparer<TKey>? _comparer;
        private KeyCollection? _keys;
        private ValueCollection? _values;

        /// <summary>
        /// Gets the number of key/value pairs contained in the <see cref="OrderedDictionary{TKey, TValue}" />.
        /// </summary>
        /// <returns>The number of key/value pairs contained in the <see cref="OrderedDictionary{TKey, TValue}" />.</returns>
        public int Count => _count;

        /// <summary>
        /// Gets the <see cref="IEqualityComparer{TKey}" /> that is used to determine equality of keys for the dictionary.
        /// </summary>
        /// <returns>The <see cref="IEqualityComparer{TKey}" /> generic interface implementation that is used to determine equality of keys for the current <see cref="OrderedDictionary{TKey, TValue}" /> and to provide hash values for the keys.</returns>
        public IEqualityComparer<TKey> Comparer => _comparer ?? EqualityComparer<TKey>.Default;

        /// <summary>
        /// Gets a collection containing the keys in the <see cref="OrderedDictionary{TKey, TValue}" />.
        /// </summary>
        /// <returns>An <see cref="OrderedDictionary{TKey, TValue}.KeyCollection" /> containing the keys in the <see cref="OrderedDictionary{TKey, TValue}" />.</returns>
        public KeyCollection Keys => _keys ??= new KeyCollection(this);

        /// <summary>
        /// Gets a collection containing the values in the <see cref="OrderedDictionary{TKey, TValue}" />.
        /// </summary>
        /// <returns>An <see cref="OrderedDictionary{TKey, TValue}.ValueCollection" /> containing the values in the <see cref="OrderedDictionary{TKey, TValue}" />.</returns>
        public ValueCollection Values => _values ??= new ValueCollection(this);

        /// <summary>
        /// Gets or sets the value associated with the specified key as an O(1) operation.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="KeyNotFoundException" />, and a set operation creates a new element with the specified key.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is null.</exception>
        /// <exception cref="KeyNotFoundException">The property is retrieved and <paramref name="key" /> does not exist in the collection.</exception>
        public TValue this[TKey key]
        {
            get
            {
                var index = IndexOf(key);
                return index < 0
                    ? throw new KeyNotFoundException($"Key {key} not found in the dictionary")
                    : _entries[index].Value;
            }
            set => TryInsert(null, key, value, InsertionBehavior.OverwriteExisting);
        }

        /// <summary>
        /// Gets or sets the value at the specified index as an O(1) operation.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The value at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is less than 0.-or-<paramref name="index" /> is equal to or greater than <see cref="OrderedDictionary{TKey, TValue}.Count" />.</exception>
        public TValue this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

                return _entries[index].Value;
            }
            set
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

                _entries[index].Value = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}" /> class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        public OrderedDictionary()
            : this(0, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}" /> class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="OrderedDictionary{TKey, TValue}" /> can contain.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity" /> is less than 0.</exception>
        public OrderedDictionary(int capacity)
            : this(capacity, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}" /> class that is empty, has the default initial capacity, and uses the specified <see cref="IEqualityComparer{T}" />.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing keys, or null to use the default <see cref="EqualityComparer{T}" /> for the type of the key.</param>
        public OrderedDictionary(IEqualityComparer<TKey> comparer)
            : this(0, comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}" /> class that is empty, has the specified initial capacity, and uses the specified <see cref="IEqualityComparer{T}" />.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="OrderedDictionary{TKey, TValue}" /> can contain.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing keys, or null to use the default <see cref="EqualityComparer{T}" /> for the type of the key.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity" /> is less than 0.</exception>
        public OrderedDictionary(int capacity, IEqualityComparer<TKey>? comparer)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(capacity);

            if (capacity > 0)
            {
                var newSize = HashHelpers.GetPrime(capacity);
                _buckets = new int[newSize];
                _entries = new Entry[newSize];
            }

            if (comparer != EqualityComparer<TKey>.Default)
            {
                _comparer = comparer;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}" /> class that contains elements copied from the specified <see cref="IEnumerable{T}" /> and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="collection">The <see cref="IEnumerable{T}" /> whose elements are copied to the new <see cref="OrderedDictionary{TKey, TValue}" />.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="collection" /> contains one or more duplicate keys.</exception>
        public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : this(collection, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}" /> class that contains elements copied from the specified <see cref="IEnumerable{T}" /> and uses the specified <see cref="IEqualityComparer{TKey}" />.
        /// </summary>
        /// <param name="collection">The <see cref="IEnumerable{T}" /> whose elements are copied to the new <see cref="OrderedDictionary{TKey, TValue}" />.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{TKey}" /> implementation to use when comparing keys, or null to use the default <see cref="EqualityComparer{TKey}" /> for the type of the key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="collection" /> contains one or more duplicate keys.</exception>
        public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer)
            : this((collection as ICollection<KeyValuePair<TKey, TValue>>)?.Count ?? 0, comparer)
        {
            ArgumentNullException.ThrowIfNull(collection);

            foreach (var pair in collection)
            {
                Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary as an O(1) operation.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is null.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="OrderedDictionary{TKey, TValue}" />.</exception>
        public void Add(TKey key, TValue value) => TryInsert(null, key, value, InsertionBehavior.ThrowOnExisting);

        /// <summary>
        /// Removes all keys and values from the <see cref="OrderedDictionary{TKey, TValue}" />.
        /// </summary>
        public void Clear()
        {
            if (_count > 0)
            {
                Array.Clear(_buckets, 0, _buckets.Length);
                Array.Clear(_entries, 0, _count);
                _count = 0;
                ++_version;
            }
        }

        /// <summary>
        /// Determines whether the <see cref="OrderedDictionary{TKey, TValue}" /> contains the specified key as an O(1) operation.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="OrderedDictionary{TKey, TValue}" />.</param>
        /// <returns>true if the <see cref="OrderedDictionary{TKey, TValue}" /> contains an element with the specified key; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is null.</exception>
        public bool ContainsKey(TKey key) => IndexOf(key) >= 0;

        /// <summary>
        /// Resizes the internal data structure if necessary to ensure no additional resizing to support the specified capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the <see cref="OrderedDictionary{TKey, TValue}" /> must be able to contain.</param>
        /// <returns>The capacity of the <see cref="OrderedDictionary{TKey, TValue}" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity" /> is less than 0.</exception>
        public int EnsureCapacity(int capacity)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(capacity);

            if (_entries.Length >= capacity)
            {
                return _entries.Length;
            }
            var newSize = HashHelpers.GetPrime(capacity);
            Resize(newSize);
            ++_version;
            return newSize;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="OrderedDictionary{TKey, TValue}" />.
        /// </summary>
        /// <returns>An <see cref="OrderedDictionary{TKey, TValue}.Enumerator" /> structure for the <see cref="OrderedDictionary{TKey, TValue}" />.</returns>
        public Enumerator GetEnumerator() => new Enumerator(this);

        /// <summary>
        /// Adds a key/value pair to the <see cref="OrderedDictionary{TKey, TValue}" /> if the key does not already exist as an O(1) operation.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value to be added, if the key does not already exist.</param>
        /// <returns>The value for the key. This will be either the existing value for the key if the key is already in the dictionary, or the new value if the key was not in the dictionary.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is null.</exception>
        public TValue GetOrAdd(TKey key, TValue value) => GetOrAdd(key, () => value);

        /// <summary>
        /// Adds a key/value pair to the <see cref="OrderedDictionary{TKey, TValue}" /> by using the specified function, if the key does not already exist as an O(1) operation.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key.</param>
        /// <returns>The value for the key. This will be either the existing value for the key if the key is already in the dictionary, or the new value for the key as returned by valueFactory if the key was not in the dictionary.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is null.-or-<paramref name="valueFactory"/> is null.</exception>
        public TValue GetOrAdd(TKey key, Func<TValue> valueFactory)
        {
            ArgumentNullException.ThrowIfNull(valueFactory);

            var index = IndexOf(key, out var hashCode);
            TValue value;
            if (index < 0)
            {
                value = valueFactory();
                AddInternal(null, key, value, hashCode);
            }
            else
            {
                value = _entries[index].Value;
            }
            return value;
        }

        /// <summary>
        /// Returns the zero-based index of the element with the specified key within the <see cref="OrderedDictionary{TKey, TValue}" /> as an O(1) operation.
        /// </summary>
        /// <param name="key">The key of the element to locate.</param>
        /// <returns>The zero-based index of the element with the specified key within the <see cref="OrderedDictionary{TKey, TValue}" />, if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is null.</exception>
        public int IndexOf(TKey key) => IndexOf(key, out _);

        /// <summary>
        /// Inserts the specified key/value pair into the <see cref="OrderedDictionary{TKey, TValue}" /> at the specified index as an O(n) operation.
        /// </summary>
        /// <param name="index">The zero-based index of the key/value pair to insert.</param>
        /// <param name="key">The key of the element to insert.</param>
        /// <param name="value">The value of the element to insert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is null.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="OrderedDictionary{TKey, TValue}" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is less than 0.-or-<paramref name="index" /> is greater than <see cref="OrderedDictionary{TKey, TValue}.Count" />.</exception>
        public void Insert(int index, TKey key, TValue value)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Count);

            TryInsert(index, key, value, InsertionBehavior.ThrowOnExisting);
        }

        /// <summary>
        /// Inserts the element in this sorted dictionary to the corresponding index using the default comparer.
        /// </summary>
        /// <param name="key">The key of the element to insert.</param>
        /// <param name="value">The value of the element to insert.</param>
        public void Insert(TKey key, TValue value)
            => Insert(key, value, Comparer<TKey>.Default);

        /// <summary>
        /// Inserts the element in this sorted dictionary to the corresponding index using the default comparer.
        /// </summary>
        /// <param name="key">The key of the element to insert.</param>
        /// <param name="value">The value of the element to insert.</param>
        /// <param name="comparer">The comparer to use.</param>
        public void Insert(TKey key, TValue value, IComparer<TKey> comparer)
        {
            var existingIndex = IndexOf(key, out var hashCode);
            if (existingIndex >= 0)
            {
                throw new ArgumentException($"Key {key} is already present");
            }

            for (var i = _count - 1; i >= 0; i--)
            {
                if (comparer.Compare(key, _entries[i].Key) >= 0)
                {
                    AddInternal(i + 1, key, value, hashCode);
                    return;
                }
            }

            AddInternal(0, key, value, hashCode);
        }

        /// <summary>
        /// Moves the element at the specified fromIndex to the specified toIndex while re-arranging the elements in between.
        /// </summary>
        /// <param name="fromIndex">The zero-based index of the element to move.</param>
        /// <param name="toIndex">The zero-based index to move the element to.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="fromIndex"/> is less than 0.
        /// -or-
        /// <paramref name="fromIndex"/> is equal to or greater than <see cref="OrderedDictionary{TKey, TValue}.Count" />
        /// -or-
        /// <paramref name="toIndex"/> is less than 0.
        /// -or-
        /// <paramref name="toIndex"/> is equal to or greater than <see cref="OrderedDictionary{TKey, TValue}.Count" />
        /// </exception>
        public void Move(int fromIndex, int toIndex)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(fromIndex, Count);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(toIndex, Count);

            if (fromIndex == toIndex)
            {
                return;
            }

            var entries = _entries;
            var temp = entries[fromIndex];
            RemoveEntryFromBucket(fromIndex);
            var direction = fromIndex < toIndex ? 1 : -1;
            for (var i = fromIndex; i != toIndex; i += direction)
            {
                entries[i] = entries[i + direction];
                UpdateBucketIndex(i + direction, -direction);
            }
            AddEntryToBucket(ref temp, toIndex, _buckets);
            entries[toIndex] = temp;
            ++_version;
        }

        /// <summary>
        /// Moves the specified number of elements at the specified fromIndex to the specified toIndex while re-arranging the elements in between.
        /// </summary>
        /// <param name="fromIndex">The zero-based index of the elements to move.</param>
        /// <param name="toIndex">The zero-based index to move the elements to.</param>
        /// <param name="count">The number of elements to move.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="fromIndex"/> is less than 0.
        /// -or-
        /// <paramref name="fromIndex"/> is equal to or greater than <see cref="OrderedDictionary{TKey, TValue}.Count" />.
        /// -or-
        /// <paramref name="toIndex"/> is less than 0.
        /// -or-
        /// <paramref name="toIndex"/> is equal to or greater than <see cref="OrderedDictionary{TKey, TValue}.Count" />.
        /// -or-
        /// <paramref name="count"/> is less than 0.</exception>
        /// <exception cref="ArgumentException"><paramref name="fromIndex"/> + <paramref name="count"/> is greater than <see cref="OrderedDictionary{TKey, TValue}.Count" />.
        /// -or-
        /// <paramref name="toIndex"/> + <paramref name="count"/> is greater than <see cref="OrderedDictionary{TKey, TValue}.Count" />.</exception>
        public void MoveRange(int fromIndex, int toIndex, int count)
        {
            if (count == 1)
            {
                Move(fromIndex, toIndex);
                return;
            }

            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(fromIndex, Count);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(toIndex, Count);
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(fromIndex + count, Count, nameof(fromIndex));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(toIndex + count, Count, nameof(toIndex));

            if (fromIndex == toIndex || count == 0)
            {
                return;
            }

            var entries = _entries;
            // Make a copy of the entries to move. Consider using ArrayPool instead to avoid allocations?
            var entriesToMove = new Entry[count];
            for (var i = 0; i < count; ++i)
            {
                entriesToMove[i] = entries[fromIndex + i];
                RemoveEntryFromBucket(fromIndex + i);
            }

            // Move entries in between
            var direction = 1;
            var amount = count;
            var start = fromIndex;
            var end = toIndex;
            if (fromIndex > toIndex)
            {
                direction = -1;
                amount = -count;
                start = fromIndex + count - 1;
                end = toIndex + count - 1;
            }
            for (var i = start; i != end; i += direction)
            {
                entries[i] = entries[i + amount];
                UpdateBucketIndex(i + amount, -amount);
            }

            var buckets = _buckets;
            // Copy entries to destination
            for (var i = 0; i < count; ++i)
            {
                var temp = entriesToMove[i];
                AddEntryToBucket(ref temp, toIndex + i, buckets);
                entries[toIndex + i] = temp;
            }
            ++_version;
        }

        /// <summary>
        /// Removes the value with the specified key from the <see cref="OrderedDictionary{TKey, TValue}" /> as an O(n) operation.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if <paramref name="key" /> is not found in the <see cref="OrderedDictionary{TKey, TValue}" />.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is null.</exception>
        public bool Remove(TKey key) => Remove(key, out _);

        /// <summary>
        /// Removes the value with the specified key from the <see cref="OrderedDictionary{TKey, TValue}" /> and returns the value as an O(n) operation.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if <paramref name="key" /> is not found in the <see cref="OrderedDictionary{TKey, TValue}" />.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is null.</exception>
        public bool Remove(TKey key, out TValue value)
        {
            var index = IndexOf(key);
            if (index >= 0)
            {
                value = _entries[index].Value;
                RemoveAt(index);
                return true;
            }
            value = default!;
            return false;
        }

        /// <summary>
        /// Removes the value at the specified index from the <see cref="OrderedDictionary{TKey, TValue}" /> as an O(n) operation.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is less than 0.-or-<paramref name="index" /> is equal to or greater than <see cref="OrderedDictionary{TKey, TValue}.Count" />.</exception>
        public void RemoveAt(int index)
        {
            var count = Count;
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, count);

            // Remove the entry from the bucket
            RemoveEntryFromBucket(index);

            // Decrement the indices > index
            var entries = _entries;
            for (var i = index + 1; i < count; ++i)
            {
                entries[i - 1] = entries[i];
                UpdateBucketIndex(i, incrementAmount: -1);
            }
            --_count;
            entries[_count] = default;
            ++_version;
        }

        /// <summary>
        /// Sets the capacity of an <see cref="OrderedDictionary{TKey, TValue}" /> object to the actual number of elements it contains, rounded up to a nearby, implementation-specific value.
        /// </summary>
        public void TrimExcess() => TrimExcess(Count);

        /// <summary>
        /// Sets the capacity of an <see cref="OrderedDictionary{TKey, TValue}" /> object to the specified capacity, rounded up to a nearby, implementation-specific value.
        /// </summary>
        /// <param name="capacity">The number of elements that the <see cref="OrderedDictionary{TKey, TValue}" /> must be able to contain.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than <see cref="OrderedDictionary{TKey, TValue}.Count" />.</exception>
        public void TrimExcess(int capacity)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(capacity, Count);

            var newSize = HashHelpers.GetPrime(capacity);
            if (newSize < _entries.Length)
            {
                Resize(newSize);
                ++_version;
            }
        }

        /// <summary>
        /// Tries to add the specified key and value to the dictionary as an O(1) operation.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        /// <returns>true if the element was added to the <see cref="OrderedDictionary{TKey, TValue}" />; false if the <see cref="OrderedDictionary{TKey, TValue}" /> already contained an element with the specified key.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is null.</exception>
        public bool TryAdd(TKey key, TValue value) => TryInsert(null, key, value, InsertionBehavior.None);

        /// <summary>
        /// Gets the value associated with the specified key as an O(1) operation.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the <see cref="OrderedDictionary{TKey, TValue}" /> contains an element with the specified key; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is null.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            var index = IndexOf(key);
            if (index >= 0)
            {
                value = _entries[index].Value;
                return true;
            }
            value = default!;
            return false;
        }

        #region Explicit Interface Implementation
        KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

                var entry = _entries[index];
                return new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
            }
            set
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

                var key = value.Key;
                var foundIndex = IndexOf(key, out var hashCode);
                // key does not exist in dictionary thus replace entry at index
                if (foundIndex < 0)
                {
                    RemoveEntryFromBucket(index);
                    var entry = new Entry { HashCode = hashCode, Key = key, Value = value.Value };
                    AddEntryToBucket(ref entry, index, _buckets);
                    _entries[index] = entry;
                    ++_version;
                }
                // key already exists in dictionary at the specified index thus just replace the key and value as hashCode remains the same
                else if (foundIndex == index)
                {
                    ref var entry = ref _entries[index];
                    entry.Key = key;
                    entry.Value = value.Value;
                }
                // key already exists in dictionary but not at the specified index thus throw exception as this method shouldn't affect the indices of other entries
                else
                {
                    throw new ArgumentException($"Key {key} already exists in dictionary but not at the specified index {index}");
                }
            }
        }

        KeyValuePair<TKey, TValue> IReadOnlyList<KeyValuePair<TKey, TValue>>.this[int index] => ((IList<KeyValuePair<TKey, TValue>>)this)[index];

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length);
            var count = Count;
            ArgumentOutOfRangeException.ThrowIfLessThan(array.Length - arrayIndex, count);

            var entries = _entries;
            for (var i = 0; i < count; ++i)
            {
                var entry = entries[i];
                array[i + arrayIndex] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            var index = IndexOf(item.Key);
            if (index >= 0 && EqualityComparer<TValue>.Default.Equals(_entries[index].Value, item.Value))
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        int IList<KeyValuePair<TKey, TValue>>.IndexOf(KeyValuePair<TKey, TValue> item)
        {
            var index = IndexOf(item.Key);
            if (index >= 0 && !EqualityComparer<TValue>.Default.Equals(_entries[index].Value, item.Value))
            {
                index = -1;
            }
            return index;
        }

        void IList<KeyValuePair<TKey, TValue>>.Insert(int index, KeyValuePair<TKey, TValue> item) => Insert(index, item.Key, item.Value);
        #endregion

        private Entry[] Resize(int newSize)
        {
            var newBuckets = new int[newSize];
            var newEntries = new Entry[newSize];

            var count = Count;
            Array.Copy(_entries, newEntries, count);
            for (var i = 0; i < count; ++i)
            {
                AddEntryToBucket(ref newEntries[i], i, newBuckets);
            }

            _buckets = newBuckets;
            _entries = newEntries;
            return newEntries;
        }

        private int IndexOf(TKey key, out uint hashCode)
        {
            ArgumentNullException.ThrowIfNull(key);

            var comparer = _comparer;
            hashCode = (uint)(comparer?.GetHashCode(key) ?? key.GetHashCode());
            var index = _buckets[(int)(hashCode % (uint)_buckets.Length)] - 1;
            if (index >= 0)
            {
                comparer ??= EqualityComparer<TKey>.Default;
                var entries = _entries;
                var collisionCount = 0;
                do
                {
                    var entry = entries[index];
                    if (entry.HashCode == hashCode && comparer.Equals(entry.Key, key))
                    {
                        break;
                    }
                    index = entry.Next;
                    if (collisionCount >= entries.Length)
                    {
                        // The chain of entries forms a loop; which means a concurrent update has happened.
                        // Break out of the loop and throw, rather than looping forever.
                        throw new InvalidOperationException("Concurrent update detected");
                    }
                    ++collisionCount;
                } while (index >= 0);
            }
            return index;
        }

        private bool TryInsert(int? index, TKey key, TValue value, InsertionBehavior behavior)
        {
            var i = IndexOf(key, out var hashCode);
            if (i >= 0)
            {
                switch (behavior)
                {
                    case InsertionBehavior.OverwriteExisting:
                        _entries[i].Value = value;
                        return true;
                    case InsertionBehavior.ThrowOnExisting:
                        throw new ArgumentException($"Key {key} is already present");
                    default:
                        return false;
                }
            }

            AddInternal(index, key, value, hashCode);
            return true;
        }

        private int AddInternal(int? index, TKey key, TValue value, uint hashCode)
        {
            var entries = _entries;
            // Check if resize is needed
            var count = Count;
            if (entries.Length == count || entries.Length == 1)
            {
                entries = Resize(HashHelpers.ExpandPrime(entries.Length));
            }

            // Increment indices >= index;
            var actualIndex = index ?? count;
            for (var i = count - 1; i >= actualIndex; --i)
            {
                entries[i + 1] = entries[i];
                UpdateBucketIndex(i, incrementAmount: 1);
            }

            ref var entry = ref entries[actualIndex];
            entry.HashCode = hashCode;
            entry.Key = key;
            entry.Value = value;
            AddEntryToBucket(ref entry, actualIndex, _buckets);
            ++_count;
            ++_version;
            return actualIndex;
        }

        // Returns the index of the next entry in the bucket
        private void AddEntryToBucket(ref Entry entry, int entryIndex, int[] buckets)
        {
            ref var b = ref buckets[(int)(entry.HashCode % (uint)buckets.Length)];
            entry.Next = b - 1;
            b = entryIndex + 1;
        }

        private void RemoveEntryFromBucket(int entryIndex)
        {
            var entries = _entries;
            var entry = entries[entryIndex];
            ref var b = ref _buckets[(int)(entry.HashCode % (uint)_buckets.Length)];
            // Bucket was pointing to removed entry. Update it to point to the next in the chain
            if (b == entryIndex + 1)
            {
                b = entry.Next + 1;
            }
            else
            {
                // Start at the entry the bucket points to, and walk the chain until we find the entry with the index we want to remove, then fix the chain
                var i = b - 1;
                var collisionCount = 0;
                while (true)
                {
                    ref var e = ref entries[i];
                    if (e.Next == entryIndex)
                    {
                        e.Next = entry.Next;
                        return;
                    }
                    i = e.Next;
                    if (collisionCount >= entries.Length)
                    {
                        // The chain of entries forms a loop; which means a concurrent update has happened.
                        // Break out of the loop and throw, rather than looping forever.
                        throw new InvalidOperationException("Concurrent update detected");
                    }
                    ++collisionCount;
                }
            }
        }

        private void UpdateBucketIndex(int entryIndex, int incrementAmount)
        {
            var entries = _entries;
            var entry = entries[entryIndex];
            ref var b = ref _buckets[(int)(entry.HashCode % (uint)_buckets.Length)];
            // Bucket was pointing to entry. Increment the index by incrementAmount.
            if (b == entryIndex + 1)
            {
                b += incrementAmount;
            }
            else
            {
                // Start at the entry the bucket points to, and walk the chain until we find the entry with the index we want to increment.
                var i = b - 1;
                var collisionCount = 0;
                while (true)
                {
                    ref var e = ref entries[i];
                    if (e.Next == entryIndex)
                    {
                        e.Next += incrementAmount;
                        return;
                    }
                    i = e.Next;
                    if (collisionCount >= entries.Length)
                    {
                        // The chain of entries forms a loop; which means a concurrent update has happened.
                        // Break out of the loop and throw, rather than looping forever.
                        throw new InvalidOperationException("Concurrent update detected");
                    }
                    ++collisionCount;
                }
            }
        }

        /// <summary>
        /// Enumerates the elements of a <see cref="OrderedDictionary{TKey, TValue}" />.
        /// </summary>
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly OrderedDictionary<TKey, TValue> _orderedDictionary;
            private readonly int _version;
            private int _index;
            private KeyValuePair<TKey, TValue> _current;

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the <see cref="OrderedDictionary{TKey, TValue}" /> at the current position of the enumerator.</returns>
            public KeyValuePair<TKey, TValue> Current => _current;

            object IEnumerator.Current => _current;

            internal Enumerator(OrderedDictionary<TKey, TValue> orderedDictionary)
            {
                _orderedDictionary = orderedDictionary;
                _version = orderedDictionary._version;
                _index = 0;
            }

            /// <summary>
            /// Releases all resources used by the <see cref="OrderedDictionary{TKey, TValue}.Enumerator" />.
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            /// Advances the enumerator to the next element of the <see cref="OrderedDictionary{TKey, TValue}" />.
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            /// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
            public bool MoveNext()
            {
                if (_version != _orderedDictionary._version)
                {
                    throw new InvalidOperationException("The dictionary has been modified during enumeration");
                }

                if (_index < _orderedDictionary.Count)
                {
                    var entry = _orderedDictionary._entries[_index];
                    _current = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
                    ++_index;
                    return true;
                }
                _current = default;
                return false;
            }

            void IEnumerator.Reset()
            {
                if (_version != _orderedDictionary._version)
                {
                    throw new InvalidOperationException("The dictionary has been modified during enumeration");
                }

                _index = 0;
                _current = default;
            }
        }
    }
}
