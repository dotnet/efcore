// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ObservableBackedBindingList<T> : SortableBindingList<T>
    {
        private bool _addingNewInstance;
        private T _addNewInstance;
        private T _cancelNewInstance;

        private readonly ICollection<T> _obervableCollection;
        private bool _inCollectionChanged;
        private bool _changingObservableCollection;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ObservableBackedBindingList([NotNull] ICollection<T> obervableCollection)
            : base(obervableCollection.ToList())
        {
            _obervableCollection = obervableCollection;

            Debug.Assert(obervableCollection is INotifyCollectionChanged);

            ((INotifyCollectionChanged)obervableCollection).CollectionChanged += ObservableCollectionChanged;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override object AddNewCore()
        {
            _addingNewInstance = true;
            _addNewInstance = (T)base.AddNewCore();
            return _addNewInstance;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void CancelNew(int itemIndex)
        {
            if (itemIndex >= 0
                && itemIndex < Count
                && Equals(base[itemIndex], _addNewInstance))
            {
                _cancelNewInstance = _addNewInstance;
                _addNewInstance = default;
                _addingNewInstance = false;
            }

            base.CancelNew(itemIndex);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var entity in Items)
            {
                RemoveFromObservableCollection(entity);
            }

            base.ClearItems();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void EndNew(int itemIndex)
        {
            if (itemIndex >= 0
                && itemIndex < Count
                && Equals(base[itemIndex], _addNewInstance))
            {
                AddToObservableCollection(_addNewInstance);
                _addNewInstance = default;
                _addingNewInstance = false;
            }

            base.EndNew(itemIndex);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            if (!_addingNewInstance
                && index >= 0
                && index <= Count)
            {
                AddToObservableCollection(item);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void RemoveItem(int index)
        {
            if (index >= 0
                && index < Count
                && Equals(base[index], _cancelNewInstance))
            {
                _cancelNewInstance = default;
            }
            else
            {
                RemoveFromObservableCollection(base[index]);
            }

            base.RemoveItem(index);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void SetItem(int index, T item)
        {
            var entity = base[index];
            base.SetItem(index, item);

            if (index >= 0
                && index < Count)
            {
                // Check to see if the user is trying to set an item that is currently being added via AddNew
                // If so then the list should not continue the AddNew; but instead add the item
                // that is being passed in.
                if (Equals(entity, _addNewInstance))
                {
                    _addNewInstance = default;
                    _addingNewInstance = false;
                }
                else
                {
                    RemoveFromObservableCollection(entity);
                }

                AddToObservableCollection(item);
            }
        }

        private void ObservableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Don't try to change the binding list if the original change came from the binding list
            // and the ObervableCollection is just being changed to match it.
            if (!_changingObservableCollection)
            {
                try
                {
                    // We are about to change the underlying binding list.  We want to prevent those
                    // changes trying to go back into the ObservableCollection, so we set a flag
                    // to prevent that.
                    _inCollectionChanged = true;

                    if (e.Action
                        == NotifyCollectionChangedAction.Reset)
                    {
                        Clear();
                    }

                    if (e.Action == NotifyCollectionChangedAction.Remove
                        || e.Action == NotifyCollectionChangedAction.Replace)
                    {
                        foreach (T entity in e.OldItems)
                        {
                            Remove(entity);
                        }
                    }

                    if (e.Action == NotifyCollectionChangedAction.Add
                        || e.Action == NotifyCollectionChangedAction.Replace)
                    {
                        foreach (T entity in e.NewItems)
                        {
                            Add(entity);
                        }
                    }
                }
                finally
                {
                    _inCollectionChanged = false;
                }
            }
        }

        // <summary>
        // Adds the item to the underlying observable collection.
        // </summary>
        // <param name="item"> The item. </param>
        private void AddToObservableCollection(T item)
        {
            // Don't try to change the ObervableCollection if the original change
            // came from the ObservableCollection
            if (!_inCollectionChanged)
            {
                try
                {
                    // We are about to change the ObservableCollection based on the binding list.
                    // We don't want to try to put that change into the ObservableCollection again,
                    // so we set a flag to prevent this.
                    _changingObservableCollection = true;
                    _obervableCollection.Add(item);
                }
                finally
                {
                    _changingObservableCollection = false;
                }
            }
        }

        // <summary>
        // Removes the item from the underlying from observable collection.
        // </summary>
        // <param name="item"> The item. </param>
        private void RemoveFromObservableCollection(T item)
        {
            // Don't try to change the ObervableCollection if the original change
            // came from the ObservableCollection
            if (!_inCollectionChanged)
            {
                try
                {
                    // We are about to change the ObservableCollection based on the binding list.
                    // We don't want to try to put that change into the ObservableCollection again,
                    // so we set a flag to prevent this.
                    _changingObservableCollection = true;
                    _obervableCollection.Remove(item);
                }
                finally
                {
                    _changingObservableCollection = false;
                }
            }
        }
    }
}
