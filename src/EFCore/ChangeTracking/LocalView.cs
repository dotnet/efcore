// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         A collection that stays in sync with entities of a given type being tracked by
    ///         a <see cref="DbContext" />. Call <see cref="DbSet{TEntity}.Local" /> to obtain a
    ///         local view.
    ///     </para>
    ///     <para>
    ///         This local view will stay in sync as entities are added or removed from the context. Likewise, entities
    ///         added to or removed from the local view will automatically be added to or removed
    ///         from the context.
    ///     </para>
    ///     <para>
    ///         Adding an entity to this collection will cause it to be tracked in the <see cref="EntityState.Added" />
    ///         state by the context unless it is already being tracked.
    ///     </para>
    ///     <para>
    ///         Removing an entity from this collection will cause it to be marked as <see cref="EntityState.Deleted" />,
    ///         unless it was previously in the Added state, in which case it will be detached from the context.
    ///     </para>
    ///     <para>
    ///         The collection implements <see cref="INotifyCollectionChanged" />,
    ///         <see cref="INotifyPropertyChanging" />, and <see cref="INotifyPropertyChanging" /> such that
    ///         notifications are generated when an entity starts being tracked by the context or is
    ///         marked as <see cref="EntityState.Deleted" /> or <see cref="EntityState.Detached" />.
    ///     </para>
    ///     <para>
    ///         Do not use this type directly for data binding. Instead call <see cref="ToObservableCollection" />
    ///         for WPF binding, or <see cref="ToBindingList" /> for WinForms.
    ///     </para>
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity in the local view.</typeparam>
    public class LocalView<TEntity> :
        ICollection<TEntity>,
        INotifyCollectionChanged,
        INotifyPropertyChanged,
        INotifyPropertyChanging,
        IListSource
        where TEntity : class
    {
        private ObservableBackedBindingList<TEntity>? _bindingList;
        private ObservableCollection<TEntity>? _observable;
        private readonly DbContext _context;
        private int _countChanges;
        private int? _count;
        private bool _triggeringStateManagerChange;
        private bool _triggeringObservableChange;
        private bool _triggeringLocalViewChange;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public LocalView(DbSet<TEntity> set)
        {
            _context = set.GetService<ICurrentDbContext>().Context;

            set.GetService<ILocalViewListener>().RegisterView(StateManagerChangedHandler);
        }

        /// <summary>
        ///     Returns an <see cref="ObservableCollection{T}" /> implementation that stays in sync with this collection.
        ///     Use this for WPF data binding.
        /// </summary>
        /// <returns> The collection. </returns>
        public virtual ObservableCollection<TEntity> ToObservableCollection()
        {
            if (_observable == null)
            {
                _observable = new ObservableCollection<TEntity>(this);
                _observable.CollectionChanged += ObservableCollectionChanged;
                CollectionChanged += LocalViewCollectionChanged;
            }

            return _observable;
        }

        private void LocalViewCollectionChanged(object? _, NotifyCollectionChangedEventArgs args)
        {
            Check.DebugAssert(
                args.Action == NotifyCollectionChangedAction.Add || args.Action == NotifyCollectionChangedAction.Remove,
                "action is not Add or Remove");

            if (_triggeringLocalViewChange)
            {
                return;
            }

            try
            {
                _triggeringObservableChange = true;

                if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    Check.DebugAssert(args.OldItems!.Count == 1, $"OldItems.Count is {args.OldItems.Count}");
                    _observable!.Remove((TEntity)args.OldItems[0]!);
                }
                else
                {
                    Check.DebugAssert(args.NewItems!.Count == 1, $"NewItems.Count is {args.NewItems.Count}");
                    _observable!.Add((TEntity)args.NewItems[0]!);
                }
            }
            finally
            {
                _triggeringObservableChange = false;
            }
        }

        private void ObservableCollectionChanged(object? _, NotifyCollectionChangedEventArgs args)
        {
            if (_triggeringObservableChange)
            {
                return;
            }

            try
            {
                _triggeringLocalViewChange = true;

                if (args.Action == NotifyCollectionChangedAction.Reset)
                {
                    Clear();
                }
                else
                {
                    if (args.Action == NotifyCollectionChangedAction.Remove
                        || args.Action == NotifyCollectionChangedAction.Replace)
                    {
                        foreach (TEntity entity in args.OldItems!)
                        {
                            Remove(entity);
                        }
                    }

                    if (args.Action == NotifyCollectionChangedAction.Add
                        || args.Action == NotifyCollectionChangedAction.Replace)
                    {
                        foreach (TEntity entity in args.NewItems!)
                        {
                            Add(entity);
                        }
                    }
                }
            }
            finally
            {
                _triggeringLocalViewChange = false;
            }
        }

        /// <summary>
        ///     Returns an <see cref="IEnumerator{T}" /> for all tracked entities of type TEntity
        ///     that are not marked as deleted.
        /// </summary>
        /// <returns> An enumerator for the collection. </returns>
        public virtual IEnumerator<TEntity> GetEnumerator()
            => _context.GetDependencies().StateManager.GetNonDeletedEntities<TEntity>().GetEnumerator();

        /// <summary>
        ///     Returns an <see cref="IEnumerator{T}" /> for all tracked entities of type TEntity
        ///     that are not marked as deleted.
        /// </summary>
        /// <returns> An enumerator for the collection. </returns>
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        ///     <para>
        ///         Adds a new entity to the <see cref="DbContext" />. If the entity is not being tracked or is currently
        ///         marked as deleted, then it becomes tracked as <see cref="EntityState.Added" />.
        ///     </para>
        ///     <para>
        ///         Note that only the given entity is tracked. Any related entities discoverable from
        ///         the given entity are not automatically tracked.
        ///     </para>
        /// </summary>
        /// <param name="item">The item to start tracking. </param>
        public virtual void Add(TEntity item)
        {
            // For something that is already in the state manager as Unchanged or Modified we don't try
            // to Add it again since doing so would change its state to Added, which is probably not what
            // was wanted in this case.

            var entry = _context.GetDependencies().StateManager.GetOrCreateEntry(item);
            if (entry.EntityState == EntityState.Deleted
                || entry.EntityState == EntityState.Detached)
            {
                try
                {
                    _triggeringStateManagerChange = true;

                    OnCountPropertyChanging();

                    _context.Add(item);

                    _countChanges++;

                    OnCollectionChanged(NotifyCollectionChangedAction.Add, item);

                    OnCountPropertyChanged();
                }
                finally
                {
                    _triggeringStateManagerChange = false;
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         Marks all entities of type TEntity being tracked by the <see cref="DbContext" />
        ///         as <see cref="EntityState.Deleted" />.
        ///     </para>
        ///     <para>
        ///         Entities that are currently marked as <see cref="EntityState.Added" /> will be marked
        ///         as <see cref="EntityState.Detached" /> since the Added state indicates that the entity
        ///         has not been saved to the database and hence it does not make sense to attempt to
        ///         delete it from the database.
        ///     </para>
        /// </summary>
        public virtual void Clear()
        {
            foreach (var entity in _context.GetDependencies().StateManager.GetNonDeletedEntities<TEntity>().ToList())
            {
                Remove(entity);
            }
        }

        /// <summary>
        ///     Returns <see langword="true" /> if the entity is being tracked by the context and has not been
        ///     marked as Deleted.
        /// </summary>
        /// <param name="item"> The entity to check. </param>
        /// <returns> <see langword="true" /> if the entity is being tracked by the context and has not been marked as Deleted. </returns>
        public virtual bool Contains(TEntity item)
        {
            var entry = _context.GetDependencies().StateManager.TryGetEntry(item);

            return entry != null && entry.EntityState != EntityState.Deleted;
        }

        /// <summary>
        ///     Copies to an array all entities of type TEntity that are being tracked and are
        ///     not marked as Deleted.
        /// </summary>
        /// <param name="array"> The array into which to copy entities. </param>
        /// <param name="arrayIndex"> The index into the array to start copying. </param>
        public virtual void CopyTo(TEntity[] array, int arrayIndex)
        {
            foreach (var entity in _context.GetDependencies().StateManager.GetNonDeletedEntities<TEntity>())
            {
                array[arrayIndex++] = entity;
            }
        }

        /// <summary>
        ///     <para>
        ///         Marks the given entity as <see cref="EntityState.Deleted" />.
        ///     </para>
        ///     <para>
        ///         Entities that are currently marked as <see cref="EntityState.Added" /> will be marked
        ///         as <see cref="EntityState.Detached" /> since the Added state indicates that the entity
        ///         has not been saved to the database and hence it does not make sense to attempt to
        ///         delete it from the database.
        ///     </para>
        /// </summary>
        /// <param name="item"> The entity to delete. </param>
        /// <returns><see langword="true" /> if the entity was being tracked and was not already Deleted. </returns>
        public virtual bool Remove(TEntity item)
        {
            var entry = _context.GetDependencies().StateManager.TryGetEntry(item);
            if (entry != null
                && entry.EntityState != EntityState.Deleted)
            {
                try
                {
                    _triggeringStateManagerChange = true;

                    OnCountPropertyChanging();

                    _context.Remove(item);

                    _countChanges--;

                    OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);

                    OnCountPropertyChanged();
                }
                finally
                {
                    _triggeringStateManagerChange = false;
                }

                return true;
            }

            return false;
        }

        private void StateManagerChangedHandler(InternalEntityEntry entry, EntityState previousState)
        {
            if (_triggeringStateManagerChange)
            {
                return;
            }

            if (entry.Entity is TEntity entity)
            {
                var wasIn = previousState != EntityState.Detached
                    && previousState != EntityState.Deleted;

                var isIn = entry.EntityState != EntityState.Detached
                    && entry.EntityState != EntityState.Deleted;

                if (wasIn != isIn)
                {
                    OnCountPropertyChanging();

                    if (isIn)
                    {
                        _countChanges++;

                        OnCollectionChanged(NotifyCollectionChangedAction.Add, entity);
                    }
                    else
                    {
                        _countChanges--;

                        OnCollectionChanged(NotifyCollectionChangedAction.Remove, entity);
                    }

                    OnCountPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     The number of entities of type TEntity that are being tracked and are not marked
        ///     as Deleted.
        /// </summary>
        public virtual int Count
        {
            get
            {
                if (!_count.HasValue)
                {
                    var stateManager = _context.GetDependencies().StateManager;

                    var count = 0;
                    foreach (var _ in stateManager.GetNonDeletedEntities<TEntity>())
                    {
                        count++;
                    }

                    _count = count;
                    _countChanges = 0;
                }

                return _count.Value + _countChanges;
            }
        }

        /// <summary>
        ///     False, since the collection is not read-only.
        /// </summary>
        public virtual bool IsReadOnly
            => false;

        /// <summary>
        ///     Occurs when a property of this collection (such as <see cref="Count" />) changes.
        /// </summary>
        public virtual event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        ///     Occurs when a property of this collection (such as <see cref="Count" />) is changing.
        /// </summary>
        public virtual event PropertyChangingEventHandler? PropertyChanging;

        /// <summary>
        ///     Occurs when the contents of the collection changes, either because an entity
        ///     has been directly added or removed from the collection, or because an entity
        ///     starts being tracked, or because an entity is marked as Deleted.
        /// </summary>
        public virtual event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        ///     Raises the <see cref="PropertyChanged" /> event.
        /// </summary>
        /// <param name="e"> Details of the property that changed. </param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
            => PropertyChanged?.Invoke(this, e);

        /// <summary>
        ///     Raises the <see cref="PropertyChanging" /> event.
        /// </summary>
        /// <param name="e"> Details of the property that is changing. </param>
        protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
            => PropertyChanging?.Invoke(this, e);

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> event.
        /// </summary>
        /// <param name="e"> Details of the change. </param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            => CollectionChanged?.Invoke(this, e);

        private void OnCountPropertyChanged()
            => OnPropertyChanged(ObservableHashSetSingletons._countPropertyChanged);

        private void OnCountPropertyChanging()
            => OnPropertyChanging(ObservableHashSetSingletons._countPropertyChanging);

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item)
            => OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item));

        /// <summary>
        ///     Returns an <see cref="BindingList{T}" /> implementation that stays in sync with this collection.
        ///     Use this for WinForms data binding.
        /// </summary>
        /// <returns> The binding list. </returns>
        public virtual BindingList<TEntity> ToBindingList()
            => _bindingList ??= new ObservableBackedBindingList<TEntity>(ToObservableCollection());

        /// <summary>
        ///     <para>
        ///         This method is called by data binding frameworks when attempting to data bind
        ///         directly to a <see cref="LocalView{TEntity}" />.
        ///     </para>
        ///     <para>
        ///         This implementation always throws an exception as <see cref="LocalView{TEntity}" />
        ///         does not maintain an ordered list with indexes. Instead call <see cref="ToObservableCollection" />
        ///         for WPF binding, or <see cref="ToBindingList" /> for WinForms.
        ///     </para>
        /// </summary>
        /// <exception cref="NotSupportedException"> Always thrown. </exception>
        /// <returns> Never returns, always throws an exception. </returns>
        IList IListSource.GetList()
            => throw new NotSupportedException(CoreStrings.DataBindingToLocalWithIListSource);

        /// <summary>
        ///     Gets a value indicating whether the collection is a collection of System.Collections.IList objects.
        ///     Always returns <see langword="false" />.
        /// </summary>
        bool IListSource.ContainsListCollection
            => false;
    }
}
