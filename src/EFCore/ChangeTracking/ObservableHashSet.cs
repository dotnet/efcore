// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     A hash set that implements the interfaces required for Entity Framework to use notification based change tracking
    ///     for a collection navigation property.
    /// </summary>
    /// <typeparam name="T"> The type of elements in the hash set. </typeparam>
    public class ObservableHashSet<T>
        : ISet<T>, IReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged, INotifyPropertyChanging
    {
        private HashSet<T> _set;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableHashSet{T}" /> class
        ///     that is empty and uses the default equality comparer for the set type.
        /// </summary>
        public ObservableHashSet()
            : this(EqualityComparer<T>.Default)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableHashSet{T}" /> class
        ///     that is empty and uses the specified equality comparer for the set type.
        /// </summary>
        /// <param name="comparer">
        ///     The <see cref="IEqualityComparer{T}" /> implementation to use when
        ///     comparing values in the set, or null to use the default <see cref="IEqualityComparer{T}" />
        ///     implementation for the set type.
        /// </param>
        public ObservableHashSet([NotNull] IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(comparer);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableHashSet{T}" /> class
        ///     that uses the default equality comparer for the set type, contains elements copied
        ///     from the specified collection, and has sufficient capacity to accommodate the
        ///     number of elements copied.
        /// </summary>
        /// <param name="collection"> The collection whose elements are copied to the new set. </param>
        public ObservableHashSet([NotNull] IEnumerable<T> collection)
            : this(collection, EqualityComparer<T>.Default)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableHashSet{T}" /> class
        ///     that uses the specified equality comparer for the set type, contains elements
        ///     copied from the specified collection, and has sufficient capacity to accommodate
        ///     the number of elements copied.
        /// </summary>
        /// <param name="collection"> The collection whose elements are copied to the new set. </param>
        /// <param name="comparer">
        ///     The <see cref="IEqualityComparer{T}" /> implementation to use when
        ///     comparing values in the set, or null to use the default <see cref="IEqualityComparer{T}" />
        ///     implementation for the set type.
        /// </param>
        public ObservableHashSet([NotNull] IEnumerable<T> collection, [NotNull] IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(collection, comparer);
        }

        /// <summary>
        ///     Occurs when a property of this hash set (such as <see cref="Count" />) changes.
        /// </summary>
        public virtual event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Occurs when a property of this hash set (such as <see cref="Count" />) is changing.
        /// </summary>
        public virtual event PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        ///     Occurs when the contents of the hash set changes.
        /// </summary>
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        void ICollection<T>.Add(T item)
            => Add(item);

        /// <inheritdoc />
        public virtual void Clear()
        {
            if (_set.Count == 0)
            {
                return;
            }

            OnCountPropertyChanging();

            var removed = this.ToList();

            _set.Clear();

            OnCollectionChanged(ObservableHashSetSingletons._noItems, removed);

            OnCountPropertyChanged();
        }

        /// <inheritdoc />
        public virtual bool Contains(T item)
            => _set.Contains(item);

        /// <inheritdoc />
        public virtual void CopyTo(T[] array, int arrayIndex)
            => _set.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public virtual bool Remove(T item)
        {
            if (!_set.Contains(item))
            {
                return false;
            }

            OnCountPropertyChanging();

            _set.Remove(item);

            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);

            OnCountPropertyChanged();

            return true;
        }

        /// <inheritdoc cref="ICollection{T}" />
        public virtual int Count
            => _set.Count;

        /// <inheritdoc />
        public virtual bool IsReadOnly
            => ((ICollection<T>)_set).IsReadOnly;

        /// <summary>
        ///     Returns an enumerator that iterates through the hash set.
        /// </summary>
        /// <returns>
        ///     An enumerator for the hash set.
        /// </returns>
        public virtual HashSet<T>.Enumerator GetEnumerator()
            => _set.GetEnumerator();

        /// <inheritdoc />
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <inheritdoc />
        public virtual bool Add(T item)
        {
            if (_set.Contains(item))
            {
                return false;
            }

            OnCountPropertyChanging();

            _set.Add(item);

            OnCollectionChanged(NotifyCollectionChangedAction.Add, item);

            OnCountPropertyChanged();

            return true;
        }

        /// <inheritdoc />
        public virtual void UnionWith(IEnumerable<T> other)
        {
            var copy = new HashSet<T>(_set, _set.Comparer);

            copy.UnionWith(other);

            if (copy.Count == _set.Count)
            {
                return;
            }

            var added = copy.Where(i => !_set.Contains(i)).ToList();

            OnCountPropertyChanging();

            _set = copy;

            OnCollectionChanged(added, ObservableHashSetSingletons._noItems);

            OnCountPropertyChanged();
        }

        /// <inheritdoc />
        public virtual void IntersectWith(IEnumerable<T> other)
        {
            var copy = new HashSet<T>(_set, _set.Comparer);

            copy.IntersectWith(other);

            if (copy.Count == _set.Count)
            {
                return;
            }

            var removed = _set.Where(i => !copy.Contains(i)).ToList();

            OnCountPropertyChanging();

            _set = copy;

            OnCollectionChanged(ObservableHashSetSingletons._noItems, removed);

            OnCountPropertyChanged();
        }

        /// <inheritdoc />
        public virtual void ExceptWith(IEnumerable<T> other)
        {
            var copy = new HashSet<T>(_set, _set.Comparer);

            copy.ExceptWith(other);

            if (copy.Count == _set.Count)
            {
                return;
            }

            var removed = _set.Where(i => !copy.Contains(i)).ToList();

            OnCountPropertyChanging();

            _set = copy;

            OnCollectionChanged(ObservableHashSetSingletons._noItems, removed);

            OnCountPropertyChanged();
        }

        /// <inheritdoc />
        public virtual void SymmetricExceptWith(IEnumerable<T> other)
        {
            var copy = new HashSet<T>(_set, _set.Comparer);

            copy.SymmetricExceptWith(other);

            var removed = _set.Where(i => !copy.Contains(i)).ToList();
            var added = copy.Where(i => !_set.Contains(i)).ToList();

            if (removed.Count == 0
                && added.Count == 0)
            {
                return;
            }

            OnCountPropertyChanging();

            _set = copy;

            OnCollectionChanged(added, removed);

            OnCountPropertyChanged();
        }

        /// <inheritdoc />
        public virtual bool IsSubsetOf(IEnumerable<T> other)
            => _set.IsSubsetOf(other);

        /// <inheritdoc />
        public virtual bool IsProperSubsetOf(IEnumerable<T> other)
            => _set.IsProperSubsetOf(other);

        /// <inheritdoc />
        public virtual bool IsSupersetOf(IEnumerable<T> other)
            => _set.IsSupersetOf(other);

        /// <inheritdoc />
        public virtual bool IsProperSupersetOf(IEnumerable<T> other)
            => _set.IsProperSupersetOf(other);

        /// <inheritdoc />
        public virtual bool Overlaps(IEnumerable<T> other)
            => _set.Overlaps(other);

        /// <inheritdoc />
        public virtual bool SetEquals(IEnumerable<T> other)
            => _set.SetEquals(other);

        /// <summary>
        ///     Copies the elements of the hash set to an array.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional array that is the destination of the elements copied from
        ///     the hash set. The array must have zero-based indexing.
        /// </param>
        public virtual void CopyTo([NotNull] T[] array)
            => _set.CopyTo(array);

        /// <summary>
        ///     Copies the specified number of elements of the hash set to an array, starting at the specified array index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional array that is the destination of the elements copied from
        ///     the hash set. The array must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex"> The zero-based index in array at which copying begins. </param>
        /// <param name="count"> The number of elements to copy to array. </param>
        public virtual void CopyTo([NotNull] T[] array, int arrayIndex, int count)
            => _set.CopyTo(array, arrayIndex, count);

        /// <summary>
        ///     Removes all elements that match the conditions defined by the specified predicate
        ///     from the hash set.
        /// </summary>
        /// <param name="match">
        ///     The <see cref="Predicate{T}" /> delegate that defines the conditions of the elements to remove.
        /// </param>
        /// <returns> The number of elements that were removed from the hash set. </returns>
        public virtual int RemoveWhere([NotNull] Predicate<T> match)
        {
            var copy = new HashSet<T>(_set, _set.Comparer);

            var removedCount = copy.RemoveWhere(match);

            if (removedCount == 0)
            {
                return 0;
            }

            var removed = _set.Where(i => !copy.Contains(i)).ToList();

            OnCountPropertyChanging();

            _set = copy;

            OnCollectionChanged(ObservableHashSetSingletons._noItems, removed);

            OnCountPropertyChanged();

            return removedCount;
        }

        /// <summary>
        ///     Gets the <see cref="IEqualityComparer{T}" /> object that is used to determine equality for the values in the set.
        /// </summary>
        public virtual IEqualityComparer<T> Comparer
            => _set.Comparer;

        /// <summary>
        ///     Sets the capacity of the hash set to the actual number of elements it contains, rounded up to a nearby,
        ///     implementation-specific value.
        /// </summary>
        public virtual void TrimExcess()
            => _set.TrimExcess();

        /// <summary>
        ///     Raises the <see cref="PropertyChanged" /> event.
        /// </summary>
        /// <param name="e"> Details of the property that changed. </param>
        protected virtual void OnPropertyChanged([NotNull] PropertyChangedEventArgs e)
            => PropertyChanged?.Invoke(this, e);

        /// <summary>
        ///     Raises the <see cref="PropertyChanging" /> event.
        /// </summary>
        /// <param name="e"> Details of the property that is changing. </param>
        protected virtual void OnPropertyChanging([NotNull] PropertyChangingEventArgs e)
            => PropertyChanging?.Invoke(this, e);

        private void OnCountPropertyChanged()
            => OnPropertyChanged(ObservableHashSetSingletons._countPropertyChanged);

        private void OnCountPropertyChanging()
            => OnPropertyChanging(ObservableHashSetSingletons._countPropertyChanging);

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item)
            => OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item));

        private void OnCollectionChanged(IList newItems, IList oldItems)
            => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems));

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> event.
        /// </summary>
        /// <param name="e"> Details of the change. </param>
        protected virtual void OnCollectionChanged([NotNull] NotifyCollectionChangedEventArgs e)
            => CollectionChanged?.Invoke(this, e);
    }

    internal static class ObservableHashSetSingletons
    {
        public static readonly PropertyChangedEventArgs _countPropertyChanged
            = new PropertyChangedEventArgs("Count");

        public static readonly PropertyChangingEventArgs _countPropertyChanging
            = new PropertyChangingEventArgs("Count");

        public static readonly object[] _noItems = Array.Empty<object>();
    }
}
