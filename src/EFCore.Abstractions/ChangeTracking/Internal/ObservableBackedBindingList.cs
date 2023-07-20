// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
[RequiresUnreferencedCode(
    "BindingList raises ListChanged events with PropertyDescriptors. PropertyDescriptors require unreferenced code.")]
[RequiresDynamicCode("Requires calling MakeGenericType on the property descriptor's type")]
public class ObservableBackedBindingList<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : SortableBindingList<T>
{
    private bool _addingNewInstance;
    private T? _addNewInstance;
    private T? _cancelNewInstance;

    private readonly ICollection<T> _observableCollection;
    private bool _inCollectionChanged;
    private bool _changingObservableCollection;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [RequiresUnreferencedCode(
        "BindingList raises ListChanged events with PropertyDescriptors. PropertyDescriptors require unreferenced code.")]
    public ObservableBackedBindingList(ICollection<T> observableCollection)
        : base(observableCollection.ToList())
    {
        _observableCollection = observableCollection;

        Check.DebugAssert(_observableCollection is INotifyCollectionChanged, "_observableCollection is not INotifyCollectionChanged");

        ((INotifyCollectionChanged)observableCollection).CollectionChanged += ObservableCollectionChanged;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override object? AddNewCore()
    {
        _addingNewInstance = true;
        _addNewInstance = (T?)base.AddNewCore();
        return _addNewInstance;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void EndNew(int itemIndex)
    {
        if (itemIndex >= 0
            && itemIndex < Count
            && Equals(base[itemIndex], _addNewInstance))
        {
            AddToObservableCollection(_addNewInstance!);
            _addNewInstance = default;
            _addingNewInstance = false;
        }

        base.EndNew(itemIndex);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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

    private void ObservableCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Don't try to change the binding list if the original change came from the binding list
        // and the ObservableCollection is just being changed to match it.
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

                if (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace)
                {
                    foreach (T entity in e.OldItems!)
                    {
                        Remove(entity);
                    }
                }

                if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Replace)
                {
                    foreach (T entity in e.NewItems!)
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
    // <param name="item">The item.</param>
    private void AddToObservableCollection(T item)
    {
        // Don't try to change the ObservableCollection if the original change
        // came from the ObservableCollection
        if (!_inCollectionChanged)
        {
            try
            {
                // We are about to change the ObservableCollection based on the binding list.
                // We don't want to try to put that change into the ObservableCollection again,
                // so we set a flag to prevent this.
                _changingObservableCollection = true;
                _observableCollection.Add(item);
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
    // <param name="item">The item.</param>
    private void RemoveFromObservableCollection(T item)
    {
        // Don't try to change the ObservableCollection if the original change
        // came from the ObservableCollection
        if (!_inCollectionChanged)
        {
            try
            {
                // We are about to change the ObservableCollection based on the binding list.
                // We don't want to try to put that change into the ObservableCollection again,
                // so we set a flag to prevent this.
                _changingObservableCollection = true;
                _observableCollection.Remove(item);
            }
            finally
            {
                _changingObservableCollection = false;
            }
        }
    }
}
