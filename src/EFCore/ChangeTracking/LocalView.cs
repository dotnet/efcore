// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     A collection that stays in sync with entities of a given type being tracked by
///     a <see cref="DbContext" />. Call <see cref="DbSet{TEntity}.Local" /> to obtain a
///     local view.
/// </summary>
/// <remarks>
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
///     <para>
///         See <see href="https://aka.ms/efcore-docs-local-views">Local views of tracked entities in EF Core</see> for more information and
///         examples.
///     </para>
/// </remarks>
/// <typeparam name="TEntity">The type of the entity in the local view.</typeparam>
public class LocalView<[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TEntity> :
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
    private readonly IEntityType _entityType;
    private int _countChanges;
    private IEntityFinder<TEntity>? _finder;
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
        _entityType = set.EntityType;

        set.GetService<ILocalViewListener>().RegisterView(StateManagerChangedHandler);
    }

    /// <summary>
    ///     Returns an <see cref="ObservableCollection{T}" /> implementation that stays in sync with this collection.
    ///     Use this for WPF data binding.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-local-views">Local views of tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <returns>The collection.</returns>
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
            args.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove,
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
                if (args.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace)
                {
                    foreach (TEntity entity in args.OldItems!)
                    {
                        Remove(entity);
                    }
                }

                if (args.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Replace)
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
    /// <returns>An enumerator for the collection.</returns>
    public virtual IEnumerator<TEntity> GetEnumerator()
        => _context.GetDependencies().StateManager.GetNonDeletedEntities<TEntity>().GetEnumerator();

    /// <summary>
    ///     Returns an <see cref="IEnumerator{T}" /> for all tracked entities of type TEntity
    ///     that are not marked as deleted.
    /// </summary>
    /// <returns>An enumerator for the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    /// <summary>
    ///     Adds a new entity to the <see cref="DbContext" />. If the entity is not being tracked or is currently
    ///     marked as deleted, then it becomes tracked as <see cref="EntityState.Added" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that only the given entity is tracked. Any related entities discoverable from
    ///         the given entity are not automatically tracked.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-local-views">Local views of tracked entities in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    /// <param name="item">The item to start tracking.</param>
    public virtual void Add(TEntity item)
    {
        // For something that is already in the state manager as Unchanged or Modified we don't try
        // to Add it again since doing so would change its state to Added, which is probably not what
        // was wanted in this case.

        var entry = _context.GetDependencies().StateManager.GetOrCreateEntry(item, _entityType);
        if (entry.EntityState is EntityState.Deleted or EntityState.Detached)
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
    ///     Marks all entities of type TEntity being tracked by the <see cref="DbContext" />
    ///     as <see cref="EntityState.Deleted" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Entities that are currently marked as <see cref="EntityState.Added" /> will be marked
    ///         as <see cref="EntityState.Detached" /> since the Added state indicates that the entity
    ///         has not been saved to the database and hence it does not make sense to attempt to
    ///         delete it from the database.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-local-views">Local views of tracked entities in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-local-views">Local views of tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="item">The entity to check.</param>
    /// <returns><see langword="true" /> if the entity is being tracked by the context and has not been marked as Deleted.</returns>
    public virtual bool Contains(TEntity item)
    {
        var entry = _context.GetDependencies().StateManager.TryGetEntry(item);

        return entry != null
            && entry.EntityState != EntityState.Deleted
            && entry.EntityState != EntityState.Detached;
    }

    /// <summary>
    ///     Copies to an array all entities of type TEntity that are being tracked and are
    ///     not marked as Deleted.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-local-views">Local views of tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <param name="array">The array into which to copy entities.</param>
    /// <param name="arrayIndex">The index into the array to start copying.</param>
    public virtual void CopyTo(TEntity[] array, int arrayIndex)
    {
        foreach (var entity in _context.GetDependencies().StateManager.GetNonDeletedEntities<TEntity>())
        {
            array[arrayIndex++] = entity;
        }
    }

    /// <summary>
    ///     Marks the given entity as <see cref="EntityState.Deleted" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Entities that are currently marked as <see cref="EntityState.Added" /> will be marked
    ///         as <see cref="EntityState.Detached" /> since the Added state indicates that the entity
    ///         has not been saved to the database and hence it does not make sense to attempt to
    ///         delete it from the database.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-local-views">Local views of tracked entities in EF Core</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    /// <param name="item">The entity to delete.</param>
    /// <returns><see langword="true" /> if the entity was being tracked and was not already Deleted.</returns>
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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-local-views">Local views of tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
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
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Occurs when a property of this collection (such as <see cref="Count" />) is changing.
    /// </summary>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    ///     Occurs when the contents of the collection changes, either because an entity
    ///     has been directly added or removed from the collection, or because an entity
    ///     starts being tracked, or because an entity is marked as Deleted.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    ///     Raises the <see cref="PropertyChanged" /> event.
    /// </summary>
    /// <param name="e">Details of the property that changed.</param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        => PropertyChanged?.Invoke(this, e);

    /// <summary>
    ///     Raises the <see cref="PropertyChanging" /> event.
    /// </summary>
    /// <param name="e">Details of the property that is changing.</param>
    protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
        => PropertyChanging?.Invoke(this, e);

    /// <summary>
    ///     Raises the <see cref="CollectionChanged" /> event.
    /// </summary>
    /// <param name="e">Details of the change.</param>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        => CollectionChanged?.Invoke(this, e);

    private void OnCountPropertyChanged()
        => OnPropertyChanged(ObservableHashSetSingletons.CountPropertyChanged);

    private void OnCountPropertyChanging()
        => OnPropertyChanging(ObservableHashSetSingletons.CountPropertyChanging);

    private void OnCollectionChanged(NotifyCollectionChangedAction action, object item)
        => OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item));

    /// <summary>
    ///     Returns a <see cref="BindingList{T}" /> implementation that stays in sync with this collection.
    ///     Use this for WinForms data binding.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-local-views">Local views of tracked entities in EF Core</see> for more information and
    ///     examples.
    /// </remarks>
    /// <returns>The binding list.</returns>
    [RequiresUnreferencedCode(
        "BindingList raises ListChanged events with PropertyDescriptors. PropertyDescriptors require unreferenced code.")]
    public virtual BindingList<TEntity> ToBindingList()
        => _bindingList ??= new ObservableBackedBindingList<TEntity>(ToObservableCollection());

    /// <summary>
    ///     This method is called by data binding frameworks when attempting to data bind
    ///     directly to a <see cref="LocalView{TEntity}" />.
    /// </summary>
    /// <remarks>
    ///     This implementation always throws an exception as <see cref="LocalView{TEntity}" />
    ///     does not maintain an ordered list with indexes. Instead call <see cref="ToObservableCollection" />
    ///     for WPF binding, or <see cref="ToBindingList" /> for WinForms.
    /// </remarks>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    /// <returns>Never returns, always throws an exception.</returns>
    IList IListSource.GetList()
        => throw new NotSupportedException(CoreStrings.DataBindingToLocalWithIListSource);

    /// <summary>
    ///     Gets a value indicating whether the collection is a collection of System.Collections.IList objects.
    ///     Always returns <see langword="false" />.
    /// </summary>
    bool IListSource.ContainsListCollection
        => false;

    /// <summary>
    ///     Resets this view, clearing any <see cref="IBindingList" /> created with <see cref="ToBindingList" /> and
    ///     any <see cref="ObservableCollection{T}" /> created with <see cref="ToObservableCollection" />, and clearing any
    ///     events registered on <see cref="PropertyChanged" />, <see cref="PropertyChanging" />, or <see cref="CollectionChanged" />.
    /// </summary>
    public virtual void Reset()
    {
        _bindingList = null;
        _observable = null;
        _countChanges = 0;
        _count = 0;
        _triggeringStateManagerChange = false;
        _triggeringObservableChange = false;
        _triggeringLocalViewChange = false;
        PropertyChanged = null;
        PropertyChanging = null;
        CollectionChanged = null;
    }

    /// <summary>
    ///     Finds an <see cref="EntityEntry{TEntity}" /> for the entity with the given primary key value in the change tracker, if it is
    ///     being tracked. <see langword="null" /> is returned if no entity with the given key value is being tracked.
    ///     This method never queries the database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, accessing <see cref="DbSet{TEntity}.Local" /> will call <see cref="ChangeTracker.DetectChanges" /> to
    ///         ensure that all entities searched and returned are up-to-date. Calling this method will not result in another call to
    ///         <see cref="ChangeTracker.DetectChanges" />. Since this method is commonly used for fast lookups, consider reusing
    ///         the <see cref="DbSet{TEntity}.Local" /> object for multiple lookups and/or disabling automatic detecting of changes using
    ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-change-tracking">EF Core change tracking</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TKey">The type of the primary key property.</typeparam>
    /// <param name="keyValue">The value of the primary key for the entity to be found.</param>
    /// <returns>An entry for the entity found, or <see langword="null" />.</returns>
    public virtual EntityEntry<TEntity>? FindEntry<TKey>(TKey keyValue)
    {
        var internalEntityEntry = Finder.FindEntry(keyValue);

        return internalEntityEntry == null ? null : new EntityEntry<TEntity>(internalEntityEntry);
    }

    /// <summary>
    ///     Finds an <see cref="EntityEntry{TEntity}" /> for the entity with the given primary key values in the change tracker, if it is
    ///     being tracked. <see langword="null" /> is returned if no entity with the given key values is being tracked.
    ///     This method never queries the database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, accessing <see cref="DbSet{TEntity}.Local" /> will call <see cref="ChangeTracker.DetectChanges" /> to
    ///         ensure that all entities searched and returned are up-to-date. Calling this method will not result in another call to
    ///         <see cref="ChangeTracker.DetectChanges" />. Since this method is commonly used for fast lookups, consider reusing
    ///         the <see cref="DbSet{TEntity}.Local" /> object for multiple lookups and/or disabling automatic detecting of changes using
    ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-change-tracking">EF Core change tracking</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
    /// <returns>An entry for the entity found, or <see langword="null" />.</returns>
    public virtual EntityEntry<TEntity>? FindEntryUntyped(IEnumerable<object?> keyValues)
    {
        Check.NotNull(keyValues, nameof(keyValues));

        var internalEntityEntry = Finder.FindEntry(keyValues);

        return internalEntityEntry == null ? null : new EntityEntry<TEntity>(internalEntityEntry);
    }

    /// <summary>
    ///     Returns an <see cref="EntityEntry{TEntity}" /> for the first entity being tracked by the context where the value of the
    ///     given property matches the given value. The entry provide access to change tracking information and operations for the entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is frequently used to get the entity with a given non-null foreign key, primary key, or alternate key value.
    ///         Lookups using a key property like this are more efficient than lookups on other property value.
    ///     </para>
    ///     <para>
    ///         By default, accessing <see cref="DbSet{TEntity}.Local" /> will call <see cref="ChangeTracker.DetectChanges" /> to
    ///         ensure that all entities searched and returned are up-to-date. Calling this method will not result in another call to
    ///         <see cref="ChangeTracker.DetectChanges" />. Since this method is commonly used for fast lookups, consider reusing
    ///         the <see cref="DbSet{TEntity}.Local" /> object for multiple lookups and/or disabling automatic detecting of changes using
    ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-change-tracking">EF Core change tracking</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="propertyName">The name of the property to match.</param>
    /// <param name="propertyValue">The value of the property to match.</param>
    /// <typeparam name="TProperty">The type of the property value.</typeparam>
    /// <returns>An entry for the entity found, or <see langword="null" />.</returns>
    public virtual EntityEntry<TEntity>? FindEntry<TProperty>(string propertyName, TProperty? propertyValue)
        => FindEntry(FindAndValidateProperty<TProperty>(propertyName), propertyValue);

    /// <summary>
    ///     Returns an <see cref="EntityEntry{TEntity}" /> for the first entity being tracked by the context where the value of the
    ///     given property matches the given values. The entry provide access to change tracking information and operations for the entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is frequently used to get the entity with a given non-null foreign key, primary key, or alternate key values.
    ///         Lookups using a key property like this are more efficient than lookups on other property value.
    ///     </para>
    ///     <para>
    ///         By default, accessing <see cref="DbSet{TEntity}.Local" /> will call <see cref="ChangeTracker.DetectChanges" /> to
    ///         ensure that all entities searched and returned are up-to-date. Calling this method will not result in another call to
    ///         <see cref="ChangeTracker.DetectChanges" />. Since this method is commonly used for fast lookups, consider reusing
    ///         the <see cref="DbSet{TEntity}.Local" /> object for multiple lookups and/or disabling automatic detecting of changes using
    ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-change-tracking">EF Core change tracking</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="propertyNames">The name of the properties to match.</param>
    /// <param name="propertyValues">The values of the properties to match.</param>
    /// <returns>An entry for the entity found, or <see langword="null" />.</returns>
    public virtual EntityEntry<TEntity>? FindEntry(IEnumerable<string> propertyNames, IEnumerable<object?> propertyValues)
    {
        Check.NotNull(propertyNames, nameof(propertyNames));

        return FindEntry(propertyNames.Select(n => _entityType.GetProperty(n)), propertyValues);
    }

    /// <summary>
    ///     Returns an <see cref="EntityEntry{TEntity}" /> for each entity being tracked by the context where the value of the given
    ///     property matches the given value. The entries provide access to change tracking information and operations for each entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is frequently used to get the entities with a given non-null foreign key, primary key, or alternate key values.
    ///         Lookups using a key property like this are more efficient than lookups on other property values.
    ///     </para>
    ///     <para>
    ///         By default, accessing <see cref="DbSet{TEntity}.Local" /> will call <see cref="ChangeTracker.DetectChanges" /> to
    ///         ensure that all entities searched and returned are up-to-date. Calling this method will not result in another call to
    ///         <see cref="ChangeTracker.DetectChanges" />. Since this method is commonly used for fast lookups, consider reusing
    ///         the <see cref="DbSet{TEntity}.Local" /> object for multiple lookups and/or disabling automatic detecting of changes using
    ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
    ///     </para>
    ///     <para>
    ///         Note that modification of entity state while iterating over the returned enumeration may result in
    ///         an <see cref="InvalidOperationException" /> indicating that the collection was modified while enumerating.
    ///         To avoid this, create a defensive copy using <see cref="Enumerable.ToList{TSource}" /> or similar before iterating.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-change-tracking">EF Core change tracking</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="propertyName">The name of the property to match.</param>
    /// <param name="propertyValue">The value of the property to match.</param>
    /// <typeparam name="TProperty">The type of the property value.</typeparam>
    /// <returns>An entry for each entity being tracked.</returns>
    public virtual IEnumerable<EntityEntry<TEntity>> GetEntries<TProperty>(string propertyName, TProperty? propertyValue)
        => GetEntries(FindAndValidateProperty<TProperty>(propertyName), propertyValue);

    /// <summary>
    ///     Returns an <see cref="EntityEntry" /> for each entity being tracked by the context where the values of the given properties
    ///     matches the given values. The entries provide access to change tracking information and operations for each entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is frequently used to get the entities with a given non-null foreign key, primary key, or alternate key values.
    ///         Lookups using a key property like this are more efficient than lookups on other property values.
    ///     </para>
    ///     <para>
    ///         By default, accessing <see cref="DbSet{TEntity}.Local" /> will call <see cref="ChangeTracker.DetectChanges" /> to
    ///         ensure that all entities searched and returned are up-to-date. Calling this method will not result in another call to
    ///         <see cref="ChangeTracker.DetectChanges" />. Since this method is commonly used for fast lookups, consider reusing
    ///         the <see cref="DbSet{TEntity}.Local" /> object for multiple lookups and/or disabling automatic detecting of changes using
    ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
    ///     </para>
    ///     <para>
    ///         Note that modification of entity state while iterating over the returned enumeration may result in
    ///         an <see cref="InvalidOperationException" /> indicating that the collection was modified while enumerating.
    ///         To avoid this, create a defensive copy using <see cref="Enumerable.ToList{TSource}" /> or similar before iterating.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-change-tracking">EF Core change tracking</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="propertyNames">The name of the properties to match.</param>
    /// <param name="propertyValues">The values of the properties to match.</param>
    /// <returns>An entry for each entity being tracked.</returns>
    public virtual IEnumerable<EntityEntry<TEntity>> GetEntries(IEnumerable<string> propertyNames, IEnumerable<object?> propertyValues)
    {
        Check.NotNull(propertyNames, nameof(propertyNames));

        return GetEntries(propertyNames.Select(n => _entityType.GetProperty(n)), propertyValues);
    }

    /// <summary>
    ///     Returns an <see cref="EntityEntry{TEntity}" /> for the first entity being tracked by the context where the value of the
    ///     given property matches the given value. The entry provide access to change tracking information and operations for the entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is frequently used to get the entity with a given non-null foreign key, primary key, or alternate key value.
    ///         Lookups using a key property like this are more efficient than lookups on other property value.
    ///     </para>
    ///     <para>
    ///         By default, accessing <see cref="DbSet{TEntity}.Local" /> will call <see cref="ChangeTracker.DetectChanges" /> to
    ///         ensure that all entities searched and returned are up-to-date. Calling this method will not result in another call to
    ///         <see cref="ChangeTracker.DetectChanges" />. Since this method is commonly used for fast lookups, consider reusing
    ///         the <see cref="DbSet{TEntity}.Local" /> object for multiple lookups and/or disabling automatic detecting of changes using
    ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-change-tracking">EF Core change tracking</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="property">The property to match.</param>
    /// <param name="propertyValue">The value of the property to match.</param>
    /// <typeparam name="TProperty">The type of the property value.</typeparam>
    /// <returns>An entry for the entity found, or <see langword="null" />.</returns>
    public virtual EntityEntry<TEntity>? FindEntry<TProperty>(IProperty property, TProperty? propertyValue)
    {
        Check.NotNull(property, nameof(property));

        var internalEntityEntry = Finder.FindEntry(property, propertyValue);

        return internalEntityEntry == null ? null : new EntityEntry<TEntity>(internalEntityEntry);
    }

    /// <summary>
    ///     Returns an <see cref="EntityEntry{TEntity}" /> for the first entity being tracked by the context where the value of the
    ///     given property matches the given values. The entry provide access to change tracking information and operations for the entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is frequently used to get the entity with a given non-null foreign key, primary key, or alternate key values.
    ///         Lookups using a key property like this are more efficient than lookups on other property value.
    ///     </para>
    ///     <para>
    ///         By default, accessing <see cref="DbSet{TEntity}.Local" /> will call <see cref="ChangeTracker.DetectChanges" /> to
    ///         ensure that all entities searched and returned are up-to-date. Calling this method will not result in another call to
    ///         <see cref="ChangeTracker.DetectChanges" />. Since this method is commonly used for fast lookups, consider reusing
    ///         the <see cref="DbSet{TEntity}.Local" /> object for multiple lookups and/or disabling automatic detecting of changes using
    ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-change-tracking">EF Core change tracking</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="properties">The properties to match.</param>
    /// <param name="propertyValues">The values of the properties to match.</param>
    /// <returns>An entry for the entity found, or <see langword="null" />.</returns>
    public virtual EntityEntry<TEntity>? FindEntry(IEnumerable<IProperty> properties, IEnumerable<object?> propertyValues)
    {
        Check.NotNull(properties, nameof(properties));
        Check.NotNull(propertyValues, nameof(propertyValues));

        var internalEntityEntry = Finder.FindEntry(properties, propertyValues);

        return internalEntityEntry == null ? null : new EntityEntry<TEntity>(internalEntityEntry);
    }

    /// <summary>
    ///     Returns an <see cref="EntityEntry{TEntity}" /> for each entity being tracked by the context where the value of the given
    ///     property matches the given value. The entries provide access to change tracking information and operations for each entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is frequently used to get the entities with a given non-null foreign key, primary key, or alternate key values.
    ///         Lookups using a key property like this are more efficient than lookups on other property values.
    ///     </para>
    ///     <para>
    ///         By default, accessing <see cref="DbSet{TEntity}.Local" /> will call <see cref="ChangeTracker.DetectChanges" /> to
    ///         ensure that all entities searched and returned are up-to-date. Calling this method will not result in another call to
    ///         <see cref="ChangeTracker.DetectChanges" />. Since this method is commonly used for fast lookups, consider reusing
    ///         the <see cref="DbSet{TEntity}.Local" /> object for multiple lookups and/or disabling automatic detecting of changes using
    ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
    ///     </para>
    ///     <para>
    ///         Note that modification of entity state while iterating over the returned enumeration may result in
    ///         an <see cref="InvalidOperationException" /> indicating that the collection was modified while enumerating.
    ///         To avoid this, create a defensive copy using <see cref="Enumerable.ToList{TSource}" /> or similar before iterating.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-change-tracking">EF Core change tracking</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="property">The property to match.</param>
    /// <param name="propertyValue">The value of the property to match.</param>
    /// <typeparam name="TProperty">The type of the property value.</typeparam>
    /// <returns>An entry for each entity being tracked.</returns>
    public virtual IEnumerable<EntityEntry<TEntity>> GetEntries<TProperty>(IProperty property, TProperty? propertyValue)
    {
        Check.NotNull(property, nameof(property));

        return Finder.GetEntries(property, propertyValue).Select(e => new EntityEntry<TEntity>(e));
    }

    /// <summary>
    ///     Returns an <see cref="EntityEntry" /> for each entity being tracked by the context where the values of the given properties
    ///     matches the given values. The entries provide access to change tracking information and operations for each entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is frequently used to get the entities with a given non-null foreign key, primary key, or alternate key values.
    ///         Lookups using a key property like this are more efficient than lookups on other property values.
    ///     </para>
    ///     <para>
    ///         By default, accessing <see cref="DbSet{TEntity}.Local" /> will call <see cref="ChangeTracker.DetectChanges" /> to
    ///         ensure that all entities searched and returned are up-to-date. Calling this method will not result in another call to
    ///         <see cref="ChangeTracker.DetectChanges" />. Since this method is commonly used for fast lookups, consider reusing
    ///         the <see cref="DbSet{TEntity}.Local" /> object for multiple lookups and/or disabling automatic detecting of changes using
    ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
    ///     </para>
    ///     <para>
    ///         Note that modification of entity state while iterating over the returned enumeration may result in
    ///         an <see cref="InvalidOperationException" /> indicating that the collection was modified while enumerating.
    ///         To avoid this, create a defensive copy using <see cref="Enumerable.ToList{TSource}" /> or similar before iterating.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-change-tracking">EF Core change tracking</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="properties">The properties to match.</param>
    /// <param name="propertyValues">The values of the properties to match.</param>
    /// <returns>An entry for each entity being tracked.</returns>
    public virtual IEnumerable<EntityEntry<TEntity>> GetEntries(IEnumerable<IProperty> properties, IEnumerable<object?> propertyValues)
    {
        Check.NotNull(properties, nameof(properties));
        Check.NotNull(propertyValues, nameof(propertyValues));

        return Finder.GetEntries(properties, propertyValues).Select(e => new EntityEntry<TEntity>(e));
    }

    private IProperty FindAndValidateProperty<TProperty>(string propertyName)
    {
        Check.NotEmpty(propertyName, nameof(propertyName));

        var property = _entityType.GetProperty(propertyName);

        if (property.ClrType != typeof(TProperty))
        {
            throw new ArgumentException(
                CoreStrings.WrongGenericPropertyType(
                    property.Name,
                    property.DeclaringType.DisplayName(),
                    property.ClrType.ShortDisplayName(),
                    typeof(TProperty).ShortDisplayName()));
        }

        return property;
    }

    private IEntityFinder<TEntity> Finder
        => _finder ??= (IEntityFinder<TEntity>)_context.GetDependencies().EntityFinderFactory.Create(_entityType);
}
