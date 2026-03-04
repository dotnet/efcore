// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Collections;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    internal partial class OrderedDictionary<TKey, TValue>
    {
        /// <summary>
        /// Represents the collection of keys in a <see cref="OrderedDictionary{TKey, TValue}" />. This class cannot be inherited.
        /// </summary>
        [DebuggerTypeProxy(typeof(DictionaryKeyCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        internal sealed class KeyCollection : IList<TKey>, IReadOnlyList<TKey>
        {
            private readonly OrderedDictionary<TKey, TValue> _orderedDictionary;

            /// <summary>
            /// Gets the number of elements contained in the <see cref="OrderedDictionary{TKey, TValue}.KeyCollection" />.
            /// </summary>
            /// <returns>The number of elements contained in the <see cref="OrderedDictionary{TKey, TValue}.KeyCollection" />.</returns>
            public int Count => _orderedDictionary.Count;

            /// <summary>
            /// Gets the key at the specified index as an O(1) operation.
            /// </summary>
            /// <param name="index">The zero-based index of the key to get.</param>
            /// <returns>The key at the specified index.</returns>
            /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is less than 0.-or-<paramref name="index" /> is equal to or greater than <see cref="OrderedDictionary{TKey, TValue}.KeyCollection.Count" />.</exception>
            public TKey this[int index] => ((IList<KeyValuePair<TKey, TValue>>)_orderedDictionary)[index].Key;

            TKey IList<TKey>.this[int index]
            {
                get => this[index];
                set => throw new NotSupportedException();
            }

            bool ICollection<TKey>.IsReadOnly => true;

            internal KeyCollection(OrderedDictionary<TKey, TValue> orderedDictionary)
            {
                _orderedDictionary = orderedDictionary;
            }

            /// <summary>
            /// Returns an enumerator that iterates through the <see cref="OrderedDictionary{TKey, TValue}.KeyCollection" />.
            /// </summary>
            /// <returns>A <see cref="OrderedDictionary{TKey, TValue}.KeyCollection.Enumerator" /> for the <see cref="OrderedDictionary{TKey, TValue}.KeyCollection" />.</returns>
            public Enumerator GetEnumerator() => new Enumerator(_orderedDictionary);

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            int IList<TKey>.IndexOf(TKey item) => _orderedDictionary.IndexOf(item);

            void IList<TKey>.Insert(int index, TKey item) => throw new NotSupportedException();

            void IList<TKey>.RemoveAt(int index) => throw new NotSupportedException();

            void ICollection<TKey>.Add(TKey item) => throw new NotSupportedException();

            void ICollection<TKey>.Clear() => throw new NotSupportedException();

            bool ICollection<TKey>.Contains(TKey item) => _orderedDictionary.ContainsKey(item);

            void ICollection<TKey>.CopyTo(TKey[] array, int arrayIndex)
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
                    array[i + arrayIndex] = entries[i].Key;
                }
            }

            bool ICollection<TKey>.Remove(TKey item) => throw new NotSupportedException();

            /// <summary>
            /// Enumerates the elements of a <see cref="OrderedDictionary{TKey, TValue}.KeyCollection" />.
            /// </summary>
            public struct Enumerator : IEnumerator<TKey>
            {
                private readonly OrderedDictionary<TKey, TValue> _orderedDictionary;
                private readonly int _version;
                private int _index;
                private TKey _current;

                /// <summary>
                /// Gets the element at the current position of the enumerator.
                /// </summary>
                /// <returns>The element in the <see cref="OrderedDictionary{TKey, TValue}.KeyCollection" /> at the current position of the enumerator.</returns>
                public readonly TKey Current => _current;

                readonly object? IEnumerator.Current => _current;

                internal Enumerator(OrderedDictionary<TKey, TValue> orderedDictionary)
                {
                    _orderedDictionary = orderedDictionary;
                    _version = orderedDictionary._version;
                    _index = 0;
                    _current = default!;
                }

                /// <summary>
                /// Releases all resources used by the <see cref="OrderedDictionary{TKey, TValue}.KeyCollection.Enumerator" />.
                /// </summary>
                public void Dispose()
                {
                }

                /// <summary>
                /// Advances the enumerator to the next element of the <see cref="OrderedDictionary{TKey, TValue}.KeyCollection" />.
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
                        _current = _orderedDictionary._entries[_index].Key;
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
