// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed partial class InternalEntityEntry : InternalEntryBase, IUpdateEntry, IInternalEntry
{
    private RelationshipsSnapshot _relationshipsSnapshot;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalEntityEntry(
        IStateManager stateManager,
        IEntityType entityType,
        object entity)
        : base((IRuntimeTypeBase)entityType)
    {
        StateManager = stateManager;
        Entity = entity;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalEntityEntry(
        IStateManager stateManager,
        IEntityType entityType,
        object entity,
        ISnapshot shadowValues)
        : base((IRuntimeTypeBase)entityType, shadowValues)
    {
        StateManager = stateManager;
        Entity = entity;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalEntityEntry(
        IStateManager stateManager,
        IEntityType entityType,
        IDictionary<string, object?> values,
        IEntityMaterializerSource entityMaterializerSource)
        : base((IRuntimeTypeBase)entityType, values)
    {
        StateManager = stateManager;
        var valuesArray = new object?[EntityType.PropertyCount];
        foreach (var property in entityType.GetFlattenedProperties())
        {
            var index = property.GetIndex();
            if (index < 0)
            {
                continue;
            }

            valuesArray[index] = values.TryGetValue(property.Name, out var value)
                ? value
                : property.Sentinel;
        }

        Entity = entityType.GetOrCreateMaterializer(entityMaterializerSource)(
            new MaterializationContext(new ValueBuffer(valuesArray), stateManager.Context));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IStateManager StateManager { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object Entity { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IRuntimeEntityType EntityType => (IRuntimeEntityType)StructuralType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    EntityState IUpdateEntry.EntityState
    {
        get => EntityState;
        set => SetEntityState(value, modifyProperties: false);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalEntityEntry? SharedIdentityEntry { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void SetEntityState(
        EntityState entityState,
        bool acceptChanges = false,
        bool modifyProperties = true,
        EntityState? forceStateWhenUnknownKey = null,
        EntityState? fallbackState = null)
    {
        var oldState = EntityState;
        var adding = PrepareForAdd(entityState);
        entityState = PropagateToUnknownKey(oldState, entityState, adding, forceStateWhenUnknownKey);

        if ((adding || oldState is EntityState.Detached)
            && StateManager.ValueGenerationManager.Generate(this, includePrimaryKey: adding)
            && fallbackState.HasValue)
        {
            entityState = fallbackState.Value;
            adding = PrepareForAdd(entityState);
            entityState = PropagateToUnknownKey(oldState, entityState, adding, forceStateWhenUnknownKey);
        }

        SetEntityState(oldState, entityState, acceptChanges, modifyProperties);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override async Task SetEntityStateAsync(
        EntityState entityState,
        bool acceptChanges = false,
        bool modifyProperties = true,
        EntityState? forceStateWhenUnknownKey = null,
        EntityState? fallbackState = null,
        CancellationToken cancellationToken = default)
    {
        var oldState = EntityState;
        var adding = PrepareForAdd(entityState);
        entityState = await PropagateToUnknownKeyAsync(
            oldState, entityState, adding, forceStateWhenUnknownKey, cancellationToken).ConfigureAwait(false);

        if ((adding || oldState is EntityState.Detached)
            && await StateManager.ValueGenerationManager
                .GenerateAsync(this, includePrimaryKey: adding, cancellationToken).ConfigureAwait(false)
            && fallbackState.HasValue)
        {
            entityState = fallbackState.Value;
            adding = PrepareForAdd(entityState);
            entityState = await PropagateToUnknownKeyAsync(
                oldState, entityState, adding, forceStateWhenUnknownKey, cancellationToken).ConfigureAwait(false);
        }

        SetEntityState(oldState, entityState, acceptChanges, modifyProperties);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void OnStateChanging(EntityState newState)
    {
        base.OnStateChanging(newState);

        FireStateChanging(newState);

        if (EntityState == EntityState.Detached)
        {
            StateManager.StartTracking(this);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void OnStateChanged(EntityState oldState)
    {
        base.OnStateChanged(oldState);

        // Save shared identity entity before it's detached
        var sharedIdentityEntry = SharedIdentityEntry;
        if (EntityState == EntityState.Detached)
        {
            StateManager.StopTracking(this, oldState);
        }

        if (oldState is EntityState.Detached or EntityState.Unchanged)
        {
            if (EntityState is EntityState.Added or EntityState.Deleted or EntityState.Modified)
            {
                StateManager.ChangedCount++;
            }
        }
        else if (EntityState is EntityState.Detached or EntityState.Unchanged)
        {
            StateManager.ChangedCount--;
        }

        FireStateChanged(oldState);

        HandleSharedIdentityEntry(EntityState);

        if (EntityState is EntityState.Deleted or EntityState.Detached
            && sharedIdentityEntry == null
            && StateManager.CascadeDeleteTiming == CascadeTiming.Immediate)
        {
            StateManager.CascadeDelete(this, force: false);
        }
    }

    private EntityState PropagateToUnknownKey(
        EntityState oldState,
        EntityState entityState,
        bool adding,
        EntityState? forceStateWhenUnknownKey)
    {
        var keyUnknown = IsKeyUnknown;

        if (adding || (oldState == EntityState.Detached && keyUnknown))
        {
            var principalEntry = StateManager.ValueGenerationManager.Propagate(this);

            entityState = ForceState(entityState, forceStateWhenUnknownKey, keyUnknown, principalEntry);
        }

        return entityState;
    }

    private async Task<EntityState> PropagateToUnknownKeyAsync(
        EntityState oldState,
        EntityState entityState,
        bool adding,
        EntityState? forceStateWhenUnknownKey,
        CancellationToken cancellationToken)
    {
        var keyUnknown = IsKeyUnknown;

        if (adding || (oldState == EntityState.Detached && keyUnknown))
        {
            var principalEntry = await StateManager.ValueGenerationManager.PropagateAsync(this, cancellationToken).ConfigureAwait(false);

            entityState = ForceState(entityState, forceStateWhenUnknownKey, keyUnknown, principalEntry);
        }

        return entityState;
    }

    private static EntityState ForceState(
        EntityState entityState,
        EntityState? forceStateWhenUnknownKey,
        bool keyUnknown,
        InternalEntityEntry? principalEntry)
        => forceStateWhenUnknownKey.HasValue
            && keyUnknown
            && principalEntry != null
            && principalEntry.EntityState != EntityState.Detached
            && principalEntry.EntityState != EntityState.Deleted
                ? principalEntry.EntityState == EntityState.Added
                    ? EntityState.Added
                    : forceStateWhenUnknownKey.Value
                : entityState;

    private void HandleSharedIdentityEntry(EntityState newState)
    {
        var sharedIdentityEntry = SharedIdentityEntry;
        if (sharedIdentityEntry == null)
        {
            return;
        }

        switch (newState)
        {
            case EntityState.Unchanged:
                sharedIdentityEntry.SetEntityState(EntityState.Detached);
                break;
            case EntityState.Added:
            case EntityState.Modified:
                if (sharedIdentityEntry.EntityState is EntityState.Added or EntityState.Modified)
                {
                    if (StateManager.SensitiveLoggingEnabled)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.IdentityConflictSensitive(
                                EntityType.DisplayName(),
                                this.BuildCurrentValuesString(EntityType.FindPrimaryKey()!.Properties)));
                    }

                    throw new InvalidOperationException(
                        CoreStrings.IdentityConflict(
                            EntityType.DisplayName(),
                            EntityType.FindPrimaryKey()!.Properties.Format()));
                }

                break;
        }
    }

    private void FireStateChanging(EntityState newState)
    {
        if (EntityState != EntityState.Detached)
        {
            StateManager.OnStateChanging(this, newState);
        }
        else
        {
            StateManager.OnTracking(this, newState, fromQuery: false);
        }

        StateManager.ChangingState(this, newState);
    }

    private void FireStateChanged(EntityState oldState)
    {
        StateManager.InternalEntityEntryNotifier.StateChanged(this, oldState, fromQuery: false);

        if (oldState != EntityState.Detached)
        {
            StateManager.OnStateChanged(this, oldState);
        }
        else
        {
            StateManager.OnTracked(this, fromQuery: false);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void SetServiceProperties(EntityState oldState, EntityState newState)
    {
        if (EntityType.HasServiceProperties())
        {
            List<IServiceProperty>? dependentServices = null;
            foreach (var serviceProperty in EntityType.GetServiceProperties())
            {
                var service = this[serviceProperty]
                    ?? serviceProperty.ParameterBinding.ServiceDelegate(
                        new MaterializationContext(ValueBuffer.Empty, Context), EntityType, Entity);

                if (service == null)
                {
                    (dependentServices ??= []).Add(serviceProperty);
                }
                else
                {
                    if (service is IInjectableService injectableService)
                    {
                        injectableService.Attaching(Context, EntityType, Entity);
                    }

                    this[serviceProperty] = service;
                }
            }

            if (dependentServices != null)
            {
                foreach (var serviceProperty in dependentServices)
                {
                    this[serviceProperty] = serviceProperty.ParameterBinding.ServiceDelegate(
                        new MaterializationContext(ValueBuffer.Empty, Context), EntityType, Entity);
                }
            }
            else if (newState == EntityState.Detached)
            {
                foreach (var serviceProperty in EntityType.GetServiceProperties())
                {
                    if (this[serviceProperty] is not IInjectableService detachable
                        || detachable.Detaching(Context, Entity))
                    {
                        this[serviceProperty] = null;
                    }
                }
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void MarkUnchangedFromQuery()
    {
        StateManager.OnTracking(this, EntityState.Unchanged, fromQuery: true);

        StateManager.InternalEntityEntryNotifier.StateChanging(this, EntityState.Unchanged);

        base.MarkUnchangedFromQuery();

        StateManager.InternalEntityEntryNotifier.StateChanged(this, EntityState.Detached, fromQuery: true);

        StateManager.OnTracked(this, fromQuery: true);

        StateManager.InternalEntityEntryNotifier.TrackedFromQuery(this);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void PropagateValue(
        InternalEntityEntry principalEntry,
        IProperty principalProperty,
        IProperty dependentProperty,
        bool isMaterialization = false,
        bool setModified = true)
    {
        var principalValue = principalEntry[principalProperty];
        if (principalEntry.HasTemporaryValue(principalProperty))
        {
            SetTemporaryValue(dependentProperty, principalValue);
        }
        else if (principalEntry.GetValueType(principalProperty) == CurrentValueType.StoreGenerated)
        {
            SetStoreGeneratedValue(dependentProperty, principalValue);
        }
        else
        {
            SetProperty(dependentProperty, principalValue, isMaterialization, setModified);
        }
    }


    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void AcceptChanges()
    {
        var currentState = EntityState;

        base.AcceptChanges();

        switch (currentState)
        {
            case EntityState.Modified:
                SharedIdentityEntry?.AcceptChanges();
                break;
        }
    }

    private static readonly MethodInfo ReadRelationshipSnapshotValueMethod
        = typeof(InternalEntityEntry).GetMethod(nameof(InternalEntityEntry.ReadRelationshipSnapshotValue))!;

    [UnconditionalSuppressMessage(
        "ReflectionAnalysis", "IL2060",
        Justification = "MakeGenericMethod wrapper, see https://github.com/dotnet/linker/issues/2482")]
    internal static MethodInfo MakeReadRelationshipSnapshotValueMethod(Type type)
        => ReadRelationshipSnapshotValueMethod.MakeGenericMethod(type);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T ReadRelationshipSnapshotValue<T>(IPropertyBase propertyBase, int relationshipSnapshotIndex)
        => _relationshipsSnapshot.GetValue<T>(this, propertyBase, relationshipSnapshotIndex);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TProperty GetRelationshipSnapshotValue<TProperty>(IPropertyBase propertyBase)
        => ((Func<IInternalEntry, TProperty>)propertyBase.GetPropertyAccessors().RelationshipSnapshotGetter)(
            this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? GetRelationshipSnapshotValue(IPropertyBase propertyBase)
        => _relationshipsSnapshot.GetValue(this, propertyBase);


    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void SetRelationshipSnapshotValue(IPropertyBase propertyBase, object? value)
    {
        EnsureRelationshipSnapshot();

        _relationshipsSnapshot.SetValue(propertyBase, value);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void EnsureRelationshipSnapshot()
    {
        if (_relationshipsSnapshot.IsEmpty)
        {
            _relationshipsSnapshot = new RelationshipsSnapshot(this);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool HasRelationshipSnapshot
        => !_relationshipsSnapshot.IsEmpty;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void RemoveFromCollectionSnapshot(
        INavigationBase navigation,
        object removedEntity)
    {
        EnsureRelationshipSnapshot();
        _relationshipsSnapshot.RemoveFromCollection(navigation, removedEntity);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void AddToCollectionSnapshot(INavigationBase navigation, object addedEntity)
    {
        EnsureRelationshipSnapshot();
        _relationshipsSnapshot.AddToCollection(navigation, addedEntity);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void AddRangeToCollectionSnapshot(
        INavigationBase navigation,
        IEnumerable<object> addedEntities)
    {
        EnsureRelationshipSnapshot();
        _relationshipsSnapshot.AddRangeToCollection(navigation, addedEntities);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object GetOrCreateCollection(INavigationBase navigationBase, bool forMaterialization)
        => navigationBase.IsShadowProperty()
            ? GetOrCreateShadowCollection(navigationBase)
            : navigationBase.GetCollectionAccessor()!.GetOrCreate(EntityEntry.Entity, forMaterialization);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool CollectionContains(INavigationBase navigationBase, object value)
    {
        var collectionAccessor = navigationBase.GetCollectionAccessor()!;
        return navigationBase.IsShadowProperty()
            ? collectionAccessor.ContainsStandalone(GetOrCreateShadowCollection(navigationBase), value)
            : collectionAccessor.Contains(EntityEntry.Entity, value);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool AddToCollection(
        INavigationBase navigationBase,
        object value,
        bool forMaterialization)
    {
        var collectionAccessor = navigationBase.GetCollectionAccessor()!;
        if (!navigationBase.IsShadowProperty())
        {
            return collectionAccessor.Add(EntityEntry.Entity, value, forMaterialization);
        }

        var collection = GetOrCreateShadowCollection(navigationBase);
        return collectionAccessor.AddStandalone(collection, value);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool RemoveFromCollection(INavigationBase navigationBase, object value)
    {
        var collectionAccessor = navigationBase.GetCollectionAccessor()!;
        return navigationBase.IsShadowProperty()
            ? collectionAccessor.RemoveStandalone(GetOrCreateShadowCollection(navigationBase), value)
            : collectionAccessor.Remove(EntityEntry.Entity, value);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void OnPropertyChanged(IPropertyBase propertyBase, object? value, bool setModified)
    {
        if (propertyBase is INavigationBase { IsCollection: false } navigation)
        {
            SetIsLoaded(navigation, value != null);
        }

        base.OnPropertyChanged(propertyBase, value, setModified);
    }


    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void HandleConceptualNulls(bool sensitiveLoggingEnabled, bool force, bool isCascadeDelete)
    {
        var fks = new List<IForeignKey>();
        foreach (var foreignKey in EntityType.GetForeignKeys())
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            var properties = foreignKey.Properties;
            foreach (var property in properties)
            {
                if (PropertyStateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.Null))
                {
                    if (properties.Any(p => p.IsNullable)
                        && foreignKey.DeleteBehavior != DeleteBehavior.Cascade
                        && foreignKey.DeleteBehavior != DeleteBehavior.ClientCascade)
                    {
                        foreach (var toNull in properties)
                        {
                            if (toNull.IsNullable)
                            {
                                this[toNull] = null;
                            }
                            else
                            {
                                PropertyStateData.FlagProperty(toNull.GetIndex(), PropertyFlag.Null, isFlagged: false);
                            }
                        }
                    }
                    else if (EntityState != EntityState.Modified
                             || IsModified(property))
                    {
                        fks.Add(foreignKey);
                    }

                    break;
                }
            }
        }

        var cascadeFk = fks.FirstOrDefault(fk => fk.DeleteBehavior is DeleteBehavior.Cascade or DeleteBehavior.ClientCascade);
        if (cascadeFk != null
            && (force
                || (!isCascadeDelete
                    && StateManager.DeleteOrphansTiming != CascadeTiming.Never)))
        {
            var cascadeState = EntityState == EntityState.Added
                ? EntityState.Detached
                : EntityState.Deleted;

            if (StateManager.SensitiveLoggingEnabled)
            {
                StateManager.UpdateLogger.CascadeDeleteOrphanSensitive(
                    this, cascadeFk.PrincipalEntityType, cascadeState);
            }
            else
            {
                StateManager.UpdateLogger.CascadeDeleteOrphan(this, cascadeFk.PrincipalEntityType, cascadeState);
            }

            SetEntityState(cascadeState);
        }
        else if (fks.Count > 0)
        {
            var foreignKey = fks.First();

            if (sensitiveLoggingEnabled)
            {
                throw new InvalidOperationException(
                    CoreStrings.RelationshipConceptualNullSensitive(
                        foreignKey.PrincipalEntityType.DisplayName(),
                        EntityType.DisplayName(),
                        this.BuildOriginalValuesString(foreignKey.Properties)));
            }

            throw new InvalidOperationException(
                CoreStrings.RelationshipConceptualNull(
                    foreignKey.PrincipalEntityType.DisplayName(),
                    EntityType.DisplayName()));
        }
        else
        {
            var property = EntityType.GetFlattenedProperties().FirstOrDefault(
                p => (EntityState != EntityState.Modified
                        || IsModified(p))
                    && PropertyStateData.IsPropertyFlagged(p.GetIndex(), PropertyFlag.Null));

            if (property != null)
            {
                if (sensitiveLoggingEnabled)
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyConceptualNullSensitive(
                            property.Name,
                            EntityType.DisplayName(),
                            this.BuildOriginalValuesString(new[] { property })));
                }

                throw new InvalidOperationException(
                    CoreStrings.PropertyConceptualNull(
                        property.Name,
                        EntityType.DisplayName()));
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public (bool IsGenerated, bool IsSet) IsKeySet
    {
        get
        {
            var isGenerated = false;
            var keyProperties = EntityType.FindPrimaryKey()!.Properties;

            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < keyProperties.Count; i++)
            {
                var keyProperty = keyProperties[i];
                var keyGenerated = keyProperty.ValueGenerated == ValueGenerated.OnAdd;

                if ((HasTemporaryValue(keyProperty)
                        || !HasExplicitValue(keyProperty))
                    && (keyGenerated || keyProperty.FindGenerationProperty() != null))
                {
                    return (true, false);
                }

                if (keyGenerated)
                {
                    isGenerated = true;
                }
            }

            return (isGenerated, true);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool IsKeyUnknown
    {
        get
        {
            var keyProperties = EntityType.FindPrimaryKey()!.Properties;
            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < keyProperties.Count; i++)
            {
                var keyProperty = keyProperties[i];
                if (PropertyStateData.IsPropertyFlagged(keyProperty.GetIndex(), PropertyFlag.Unknown))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EntityEntry ToEntityEntry()
        => new(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void HandleINotifyPropertyChanging(
        object? sender,
        PropertyChangingEventArgs eventArgs)
    {
        foreach (var propertyBase in GetNotificationProperties(EntityType, eventArgs.PropertyName))
        {
            StateManager.InternalEntityEntryNotifier.PropertyChanging(this, propertyBase);

            if (propertyBase is INavigationBase { IsCollection: true } navigation
                && GetCurrentValue(navigation) != null)
            {
                StateManager.Dependencies.InternalEntityEntrySubscriber.UnsubscribeCollectionChanged(this, navigation);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void HandleINotifyPropertyChanged(
        object? sender,
        PropertyChangedEventArgs eventArgs)
    {
        foreach (var propertyBase in GetNotificationProperties(EntityType, eventArgs.PropertyName))
        {
            StateManager.InternalEntityEntryNotifier.PropertyChanged(this, propertyBase, setModified: true);

            if (propertyBase is INavigationBase { IsCollection: true } navigation
                && GetCurrentValue(navigation) != null)
            {
                StateManager.Dependencies.InternalEntityEntrySubscriber.SubscribeCollectionChanged(this, navigation);
            }
        }
    }

    private static IEnumerable<IPropertyBase> GetNotificationProperties(
        IEntityType entityType,
        string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            foreach (var property in entityType.GetFlattenedProperties()
                         .Where(p => p.GetAfterSaveBehavior() == PropertySaveBehavior.Save))
            {
                yield return property;
            }

            foreach (var navigation in entityType.GetNavigations())
            {
                yield return navigation;
            }

            foreach (var navigation in entityType.GetSkipNavigations())
            {
                yield return navigation;
            }
        }
        else
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var property = entityType.FindProperty(propertyName)
                ?? entityType.FindNavigation(propertyName)
                ?? (IPropertyBase?)entityType.FindSkipNavigation(propertyName);

            if (property != null)
            {
                yield return property;
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void HandleINotifyCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs eventArgs)
    {
        var navigation = EntityType.GetNavigations()
            .Concat<INavigationBase>(EntityType.GetSkipNavigations())
            .FirstOrDefault(n => n.IsCollection && this[n] == sender);

        if (navigation != null)
        {
            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    StateManager.InternalEntityEntryNotifier.NavigationCollectionChanged(
                        this,
                        navigation,
                        eventArgs.NewItems!.OfType<object>(),
                        Enumerable.Empty<object>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    StateManager.InternalEntityEntryNotifier.NavigationCollectionChanged(
                        this,
                        navigation,
                        Enumerable.Empty<object>(),
                        eventArgs.OldItems!.OfType<object>());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    StateManager.InternalEntityEntryNotifier.NavigationCollectionChanged(
                        this,
                        navigation,
                        eventArgs.NewItems!.OfType<object>(),
                        eventArgs.OldItems!.OfType<object>());
                    break;
                case NotifyCollectionChangedAction.Reset:
                    throw new InvalidOperationException(CoreStrings.ResetNotSupported);
                // Note: ignoring Move since index not important
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void SetIsLoaded(INavigationBase navigation, bool loaded = true)
    {
        if (!loaded
            && !navigation.IsCollection
            && this[navigation] != null)
        {
            throw new InvalidOperationException(
                CoreStrings.ReferenceMustBeLoaded(navigation.Name, navigation.DeclaringEntityType.DisplayName()));
        }

        var lazyLoader = GetLazyLoader();
        if (lazyLoader != null)
        {
            lazyLoader.SetLoaded(Entity, navigation.Name, loaded);
        }
        else
        {
            PropertyStateData.FlagProperty(navigation.GetIndex(), PropertyFlag.IsLoaded, isFlagged: loaded);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool IsLoaded(INavigationBase navigation)
        => GetLazyLoader()?.IsLoaded(Entity, navigation.Name)
            ?? PropertyStateData.IsPropertyFlagged(navigation.GetIndex(), PropertyFlag.IsLoaded);

    private ILazyLoader? GetLazyLoader()
    {
        if (!EntityType.HasServiceProperties())
        {
            return null;
        }

        var lazyLoaderProperty = EntityType.GetServiceProperties().FirstOrDefault(p => p.ClrType == typeof(ILazyLoader));
        return lazyLoaderProperty != null ? (ILazyLoader?)this[lazyLoaderProperty] : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => this.ToDebugString(ChangeTrackerDebugStringOptions.ShortDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DebugView DebugView
        => new(
            () => this.ToDebugString(ChangeTrackerDebugStringOptions.ShortDefault),
            () => this.ToDebugString());

    IInternalEntry IInternalEntry.PrepareToSave()
        => PrepareToSave();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    InternalEntityEntry IInternalEntry.EntityEntry
        => this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    object IInternalEntry.Entity
        => Entity;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IUpdateEntry? IUpdateEntry.SharedIdentityEntry
        => SharedIdentityEntry;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IEntityType IUpdateEntry.EntityType
        => EntityType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IRuntimeTypeBase IInternalEntry.StructuralType
        => EntityType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    void IUpdateEntry.SetOriginalValue(IProperty property, object? value)
        => SetOriginalValue(property, value);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    void IUpdateEntry.SetPropertyModified(IProperty property)
        => SetPropertyModified(property);
}
