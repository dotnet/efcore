// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Collections;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    internal partial class OrderedDictionary<TKey, TValue>
    {
        /// <summary>
        /// Represents the collection of values in a <see cref="OrderedDictionary{TKey, TValue}" />. This class cannot be inherited.
        /// </summary>
        [DebuggerTypeProxy(typeof(DictionaryValueCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        public sealed class ValueCollection : IList<TValue>, IReadOnlyList<TValue>
        {
            private readonly OrderedDictionary<TKey, TValue> _orderedDictionary;

            /// <summary>
            /// Gets the number of elements contained in the <see cref="OrderedDictionary{TKey, TValue}.ValueCollection" />.
            /// </summary>
            /// <returns>The number of elements contained in the <see cref="OrderedDictionary{TKey, TValue}.ValueCollection" />.</returns>
            public int Count => _orderedDictionary.Count;

            /// <summary>
            /// Gets the value at the specified index as an O(1) operation.
            /// </summary>
            /// <param name="index">The zero-based index of the value to get.</param>
            /// <returns>The value at the specified index.</returns>
            /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is less than 0.-or-<paramref name="index" /> is equal to or greater than <see cref="OrderedDictionary{TKey, TValue}.ValueCollection.Count" />.</exception>
            public TValue this[int index] => _orderedDictionary[index];

            TValue IList<TValue>.this[int index]
            {
                get => this[index];
                set => throw new NotSupportedException();
            }

            bool ICollection<TValue>.IsReadOnly => true;

            internal ValueCollection(OrderedDictionary<TKey, TValue> orderedDictionary)
            {
                _orderedDictionary = orderedDictionary;
            }

            /// <summary>
            /// Returns an enumerator that iterates through the <see cref="OrderedDictionary{TKey, TValue}.ValueCollection" />.
            /// </summary>
            /// <returns>A <see cref="OrderedDictionary{TKey, TValue}.ValueCollection.Enumerator" /> for the <see cref="OrderedDictionary{TKey, TValue}.ValueCollection" />.</returns>
            public Enumerator GetEnumerator() => new Enumerator(_orderedDictionary);

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            int IList<TValue>.IndexOf(TValue item)
            {
                var comparer = EqualityComparer<TValue>.Default;
                var entries = _orderedDictionary._entries;
                var count = Count;
                for (var i = 0; i < count; ++i)
                {
                    if (comparer.Equals(entries[i].Value, item))
                    {
                        return i;
                    }
                }
                return -1;
            }

            void IList<TValue>.Insert(int index, TValue item) => throw new NotSupportedException();

            void IList<TValue>.RemoveAt(int index) => throw new NotSupportedException();

            void ICollection<TValue>.Add(TValue item) => throw new NotSupportedException();

            void ICollection<TValue>.Clear() => throw new NotSupportedException();

            bool ICollection<TValue>.Contains(TValue item) => ((IList<TValue>)this).IndexOf(item) >= 0;

            void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex)
            {
                ArgumentNullException.ThrowIfNull(array);

                if ((uint)arrayIndex > (uint)array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                }
                var count = Count;
                if (array.Length - arrayIndex < count)
                {
                    throw new ArgumentException();
                }

                var entries = _orderedDictionary._entries;
                for (var i = 0; i < count; ++i)
                {
                    array[i + arrayIndex] = entries[i].Value;
                }
            }

            bool ICollection<TValue>.Remove(TValue item) => throw new NotSupportedException();

            /// <summary>
            /// Enumerates the elements of a <see cref="OrderedDictionary{TKey, TValue}.ValueCollection" />.
            /// </summary>
            public struct Enumerator : IEnumerator<TValue>
            {
                private readonly OrderedDictionary<TKey, TValue> _orderedDictionary;
                private readonly int _version;
                private int _index;
                private TValue _current;

                /// <summary>
                /// Gets the element at the current position of the enumerator.
                /// </summary>
                /// <returns>The element in the <see cref="OrderedDictionary{TKey, TValue}.ValueCollection" /> at the current position of the enumerator.</returns>
                public TValue Current => _current;

                object? IEnumerator.Current => _current;

                internal Enumerator(OrderedDictionary<TKey, TValue> orderedDictionary)
                {
                    _orderedDictionary = orderedDictionary;
                    _version = orderedDictionary._version;
                    _index = 0;
                    _current = default!;
                }

                /// <summary>
                /// Releases all resources used by the <see cref="OrderedDictionary{TKey, TValue}.ValueCollection.Enumerator" />.
                /// </summary>
                public void Dispose()
                {
                }

                /// <summary>
                /// Advances the enumerator to the next element of the <see cref="OrderedDictionary{TKey, TValue}.ValueCollection" />.
                /// </summary>
                /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
                /// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
                public bool MoveNext()
                {
                    if (_version != _orderedDictionary._version)
                    {
                        throw new InvalidOperationException();
                    }

                    if (_index < _orderedDictionary.Count)
                    {
                        _current = _orderedDictionary._entries[_index].Value;
                        ++_index;
                        return true;
                    }
                    _current = default!;
                    return false;
                }

                void IEnumerator.Reset()
                {
                    if (_version != _orderedDictionary._version)
                    {
                        throw new InvalidOperationException();
                    }

                    _index = 0;
                    _current = default!;
                }
            }
        }
    }
}
