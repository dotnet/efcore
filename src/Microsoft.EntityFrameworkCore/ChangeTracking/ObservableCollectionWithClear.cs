// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    public class ObservableCollectionWithClear<T>
        : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public ObservableCollectionWithClear()
        {
        }

        public ObservableCollectionWithClear([NotNull] IEnumerable<T> collection)
            : base(new List<T>(Check.NotNull(collection, nameof(collection))))
        {
        }

        public virtual void Move(int oldIndex, int newIndex)
            => MoveItem(oldIndex, newIndex);

        public virtual event PropertyChangedEventHandler PropertyChanged;

        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        protected override void ClearItems()
        {
            CheckReentrancy();
            var items = this.ToList();
            base.ClearItems();
            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionReset(items);
        }

        protected override void RemoveItem(int index)
        {
            CheckReentrancy();
            var removedItem = this[index];

            base.RemoveItem(index);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItem, index);
        }

        protected override void InsertItem(int index, [CanBeNull] T item)
        {
            CheckReentrancy();
            base.InsertItem(index, item);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        protected override void SetItem(int index, [CanBeNull] T item)
        {
            CheckReentrancy();
            var originalItem = this[index];
            base.SetItem(index, item);

            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, item, index);
        }

        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            CheckReentrancy();

            var removedItem = this[oldIndex];

            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, removedItem);

            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex);
        }

        protected virtual void OnPropertyChanged([NotNull] PropertyChangedEventArgs e)
            => PropertyChanged?.Invoke(this, e);

        protected virtual void OnCollectionChanged([NotNull] NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                using (BlockReentrancy())
                {
                    CollectionChanged(this, e);
                }
            }
        }

        protected virtual IDisposable BlockReentrancy()
            => _monitor.Enter();

        protected virtual void CheckReentrancy()
        {
            if (_monitor.Busy
                && CollectionChanged != null
                && CollectionChanged.GetInvocationList().Length > 1)
            {
                throw new InvalidOperationException(CoreStrings.ObservableCollectionReentrancy);
            }
        }

        private void OnCountPropertyChanged()
            => OnPropertyChanged(EventArgsCache.CountPropertyChanged);

        private void OnIndexerPropertyChanged()
            => OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
            => OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
            => OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
            => OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));

        // Can't actually use Reset because event args constructor will throw!
        private void OnCollectionReset(IList oldItems)
            => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new object[0], oldItems));

        private class SimpleMonitor : IDisposable
        {
            public SimpleMonitor Enter()
            {
                ++_busyCount;

                return this;
            }

            public void Dispose()
            {
                --_busyCount;
            }

            public bool Busy => _busyCount > 0;

            private int _busyCount;
        }

        private readonly SimpleMonitor _monitor = new SimpleMonitor();

        private static class EventArgsCache
        {
            internal static readonly PropertyChangedEventArgs CountPropertyChanged = new PropertyChangedEventArgs("Count");
            internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new PropertyChangedEventArgs("Item[]");
        }
    }
}
