// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed partial class InternalEntityEntry : IUpdateEntry
{
    private readonly StateData _stateData;
    private OriginalValues _originalValues;
    private RelationshipsSnapshot _relationshipsSnapshot;
    private SidecarValues _temporaryValues;
    private SidecarValues _storeGeneratedValues;
    private readonly ISnapshot _shadowValues;

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
    {
        StateManager = stateManager;
        EntityType = (IRuntimeEntityType)entityType;
        Entity = entity;
        _shadowValues = EntityType.EmptyShadowValuesFactory();
        _stateData = new StateData(EntityType.PropertyCount, EntityType.NavigationCount);

        foreach (var property in entityType.GetFlattenedProperties())
        {
            if (property.IsShadowProperty())
            {
                _stateData.FlagProperty(property.GetIndex(), PropertyFlag.Unknown, true);
            }
        }
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
        in ISnapshot snapshot)
    {
        StateManager = stateManager;
        EntityType = (IRuntimeEntityType)entityType;
        Entity = entity;
        _shadowValues = snapshot;
        _stateData = new StateData(EntityType.PropertyCount, EntityType.NavigationCount);
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
    {
        StateManager = stateManager;
        EntityType = (IRuntimeEntityType)entityType;

        var valuesArray = new object?[EntityType.PropertyCount];
        var shadowPropertyValuesArray = EntityType.ShadowValuesFactory(values);
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
        _shadowValues = EntityType.ShadowValuesFactory(values);
        _stateData = new StateData(EntityType.PropertyCount, EntityType.NavigationCount);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DbContext Context
        => StateManager.Context;

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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IRuntimeEntityType EntityType { get; }

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
    public IStateManager StateManager { [DebuggerStepThrough] get; }

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
    public void SetEntityState(
        EntityState entityState,
        bool acceptChanges = false,
        bool modifyProperties = true,
        EntityState? forceStateWhenUnknownKey = null,
        EntityState? fallbackState = null)
    {
        var oldState = _stateData.EntityState;
        bool adding;
        Setup();

        if ((adding || oldState is EntityState.Detached)
            && StateManager.ValueGenerationManager.Generate(this, includePrimaryKey: adding)
            && fallbackState.HasValue)
        {
            entityState = fallbackState.Value;
            Setup();
        }

        SetEntityState(oldState, entityState, acceptChanges, modifyProperties);

        void Setup()
        {
            adding = PrepareForAdd(entityState);
            entityState = PropagateToUnknownKey(oldState, entityState, adding, forceStateWhenUnknownKey);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public async Task SetEntityStateAsync(
        EntityState entityState,
        bool acceptChanges = false,
        bool modifyProperties = true,
        EntityState? forceStateWhenUnknownKey = null,
        EntityState? fallbackState = null,
        CancellationToken cancellationToken = default)
    {
        var oldState = _stateData.EntityState;
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

    private bool PrepareForAdd(EntityState newState)
    {
        if (newState != EntityState.Added
            || EntityState == EntityState.Added)
        {
            return false;
        }

        if (EntityState == EntityState.Modified)
        {
            _stateData.FlagAllProperties(
                EntityType.PropertyCount, PropertyFlag.Modified,
                flagged: false);
        }

        // Temporarily change the internal state to unknown so that key generation, including setting key values
        // can happen without constraints on changing read-only values kicking in
        _stateData.EntityState = EntityState.Detached;

        return true;
    }

    private void SetEntityState(EntityState oldState, EntityState newState, bool acceptChanges, bool modifyProperties)
    {
        var entityType = EntityType;

        // Prevent temp values from becoming permanent values
        if (oldState == EntityState.Added
            && newState != EntityState.Added
            && newState != EntityState.Detached)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var property in entityType.GetFlattenedProperties())
            {
                if (property.IsKey() && HasTemporaryValue(property))
                {
                    throw new InvalidOperationException(
                        CoreStrings.TempValuePersists(
                            property.Name,
                            entityType.DisplayName(), newState));
                }
            }
        }

        // The entity state can be Modified even if some properties are not modified so always
        // set all properties to modified if the entity state is explicitly set to Modified.
        if (newState == EntityState.Modified
            && modifyProperties)
        {
            _stateData.FlagAllProperties(EntityType.PropertyCount, PropertyFlag.Modified, flagged: true);

            // Hot path; do not use LINQ
            foreach (var property in entityType.GetFlattenedProperties())
            {
                if (property.GetAfterSaveBehavior() != PropertySaveBehavior.Save)
                {
                    _stateData.FlagProperty(property.GetIndex(), PropertyFlag.Modified, isFlagged: false);
                }
            }
        }

        if (oldState == newState)
        {
            return;
        }

        if (newState == EntityState.Unchanged)
        {
            _stateData.FlagAllProperties(
                EntityType.PropertyCount, PropertyFlag.Modified,
                flagged: false);
        }

        if (_stateData.EntityState != oldState)
        {
            _stateData.EntityState = oldState;
        }

        FireStateChanging(newState);

        if (newState == EntityState.Unchanged
            && oldState == EntityState.Modified)
        {
            if (acceptChanges)
            {
                _originalValues.AcceptChanges(this);
            }
            else
            {
                _originalValues.RejectChanges(this);
            }
        }

        SetServiceProperties(oldState, newState);

        _stateData.EntityState = newState;

        // Save shared identity entity before it's detached
        var sharedIdentityEntry = SharedIdentityEntry;
        if (oldState == EntityState.Detached)
        {
            StateManager.StartTracking(this);
        }
        else if (newState == EntityState.Detached)
        {
            StateManager.StopTracking(this, oldState);
        }

        if (newState is EntityState.Deleted or EntityState.Detached
            && HasConceptualNull)
        {
            _stateData.FlagAllProperties(EntityType.PropertyCount, PropertyFlag.Null, flagged: false);
        }

        if (oldState is EntityState.Detached or EntityState.Unchanged)
        {
            if (newState is EntityState.Added or EntityState.Deleted or EntityState.Modified)
            {
                StateManager.ChangedCount++;
            }
        }
        else if (newState is EntityState.Detached or EntityState.Unchanged)
        {
            StateManager.ChangedCount--;
        }

        FireStateChanged(oldState);

        HandleSharedIdentityEntry(newState);

        if (newState is EntityState.Deleted or EntityState.Detached
            && sharedIdentityEntry == null
            && StateManager.CascadeDeleteTiming == CascadeTiming.Immediate)
        {
            StateManager.CascadeDelete(this, force: false);
        }
    }

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

    private void SetServiceProperties(EntityState oldState, EntityState newState)
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
    public void MarkUnchangedFromQuery()
    {
        StateManager.OnTracking(this, EntityState.Unchanged, fromQuery: true);

        StateManager.InternalEntityEntryNotifier.StateChanging(this, EntityState.Unchanged);

        _stateData.EntityState = EntityState.Unchanged;

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
    public EntityState EntityState
        => _stateData.EntityState;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool IsModified(IProperty property)
    {
        var propertyIndex = property.GetIndex();

        return _stateData.EntityState == EntityState.Modified
            && _stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.Modified)
            && !_stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.Unknown);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool IsUnknown(IProperty property)
        => _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.Unknown);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void SetPropertyModified(
        IProperty property,
        bool changeState = true,
        bool isModified = true,
        bool isConceptualNull = false,
        bool acceptChanges = false)
    {
        var propertyIndex = property.GetIndex();
        _stateData.FlagProperty(propertyIndex, PropertyFlag.Unknown, false);

        var currentState = _stateData.EntityState;

        if (currentState is EntityState.Added or EntityState.Detached
            || !changeState)
        {
            var index = property.GetOriginalValueIndex();
            if (index != -1 && !IsConceptualNull(property))
            {
                SetOriginalValue(property, this[property], index);
            }

            if (currentState == EntityState.Added)
            {
                if (FlaggedAsTemporary(propertyIndex)
                    && !FlaggedAsStoreGenerated(propertyIndex)
                    && !HasSentinel(property))
                {
                    _stateData.FlagProperty(propertyIndex, PropertyFlag.IsTemporary, false);
                }

                return;
            }
        }

        if (changeState
            && !isConceptualNull
            && isModified
            && !StateManager.SavingChanges
            && property.IsKey()
            && property.GetAfterSaveBehavior() == PropertySaveBehavior.Throw)
        {
            throw new InvalidOperationException(CoreStrings.KeyReadOnly(property.Name, EntityType.DisplayName()));
        }

        if (currentState == EntityState.Deleted)
        {
            return;
        }

        if (changeState)
        {
            if (!isModified
                && currentState != EntityState.Detached
                && property.GetOriginalValueIndex() != -1)
            {
                if (acceptChanges)
                {
                    SetOriginalValue(property, GetCurrentValue(property));
                }

                SetProperty(property, GetOriginalValue(property), isMaterialization: false, setModified: false);
            }

            _stateData.FlagProperty(propertyIndex, PropertyFlag.Modified, isModified);
        }

        if (isModified
            && currentState is EntityState.Unchanged or EntityState.Detached)
        {
            if (changeState)
            {
                FireStateChanging(EntityState.Modified);

                SetServiceProperties(currentState, EntityState.Modified);

                _stateData.EntityState = EntityState.Modified;

                if (currentState == EntityState.Detached)
                {
                    StateManager.StartTracking(this);
                }
            }

            if (changeState)
            {
                StateManager.ChangedCount++;
                FireStateChanged(currentState);
            }
        }
        else if (currentState == EntityState.Modified
                 && changeState
                 && !isModified
                 && !_stateData.AnyPropertiesFlagged(PropertyFlag.Modified))
        {
            FireStateChanging(EntityState.Unchanged);
            _stateData.EntityState = EntityState.Unchanged;
            StateManager.ChangedCount--;
            FireStateChanged(currentState);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void OnComplexPropertyModified(IComplexProperty property, bool isModified = true)
    {
        var currentState = _stateData.EntityState;
        if (currentState == EntityState.Deleted)
        {
            return;
        }

        if (isModified
            && currentState is EntityState.Unchanged or EntityState.Detached)
        {
            _stateData.EntityState = EntityState.Modified;
        }
        else if (currentState == EntityState.Modified
                 && !isModified
                 && !_stateData.AnyPropertiesFlagged(PropertyFlag.Modified))
        {
            _stateData.EntityState = EntityState.Unchanged;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool HasConceptualNull
        => _stateData.AnyPropertiesFlagged(PropertyFlag.Null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool IsConceptualNull(IProperty property)
        => _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.Null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool HasTemporaryValue(IProperty property)
        => GetValueType(property) == CurrentValueType.Temporary;

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

    private CurrentValueType GetValueType(IProperty property)
        => _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.IsStoreGenerated)
            ? CurrentValueType.StoreGenerated
            : _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.IsTemporary)
                ? CurrentValueType.Temporary
                : CurrentValueType.Normal;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void SetTemporaryValue(IProperty property, object? value, bool setModified = true)
    {
        if (property.GetStoreGeneratedIndex() == -1)
        {
            throw new InvalidOperationException(
                CoreStrings.TempValue(property.Name, EntityType.DisplayName()));
        }

        SetProperty(property, value, isMaterialization: false, setModified, isCascadeDelete: false, CurrentValueType.Temporary);
        _stateData.FlagProperty(property.GetIndex(), PropertyFlag.IsTemporary, true);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void MarkAsTemporary(IProperty property, bool temporary)
        => _stateData.FlagProperty(property.GetIndex(), PropertyFlag.IsTemporary, temporary);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly MethodInfo FlaggedAsTemporaryMethod
        = typeof(InternalEntityEntry).GetMethod(nameof(FlaggedAsTemporary))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly MethodInfo FlaggedAsStoreGeneratedMethod
        = typeof(InternalEntityEntry).GetMethod(nameof(FlaggedAsStoreGenerated))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void SetStoreGeneratedValue(IProperty property, object? value, bool setModified = true)
    {
        if (property.GetStoreGeneratedIndex() == -1)
        {
            throw new InvalidOperationException(
                CoreStrings.StoreGenValue(property.Name, EntityType.DisplayName()));
        }

        SetProperty(
            property,
            value,
            isMaterialization: false,
            setModified,
            isCascadeDelete: false,
            CurrentValueType.StoreGenerated);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void MarkUnknown(IProperty property)
        => _stateData.FlagProperty(property.GetIndex(), PropertyFlag.Unknown, true);

    internal static MethodInfo MakeReadShadowValueMethod(Type type)
        => typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethod(nameof(ReadShadowValue))!
            .MakeGenericMethod(type);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T ReadShadowValue<T>(int shadowIndex)
        => _shadowValues.GetValue<T>(shadowIndex);

    private static readonly MethodInfo ReadOriginalValueMethod
        = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethod(nameof(ReadOriginalValue))!;

    [UnconditionalSuppressMessage(
        "ReflectionAnalysis", "IL2060",
        Justification = "MakeGenericMethod wrapper, see https://github.com/dotnet/linker/issues/2482")]
    internal static MethodInfo MakeReadOriginalValueMethod(Type type)
        => ReadOriginalValueMethod.MakeGenericMethod(type);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T ReadOriginalValue<T>(IProperty property, int originalValueIndex)
        => _originalValues.GetValue<T>(this, property, originalValueIndex);

    private static readonly MethodInfo ReadRelationshipSnapshotValueMethod
        = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethod(nameof(ReadRelationshipSnapshotValue))!;

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

    [UnconditionalSuppressMessage(
        "ReflectionAnalysis", "IL2060",
        Justification = "MakeGenericMethod wrapper, see https://github.com/dotnet/linker/issues/2482")]
    internal static MethodInfo MakeReadStoreGeneratedValueMethod(Type type)
        => ReadStoreGeneratedValueMethod.MakeGenericMethod(type);

    private static readonly MethodInfo ReadStoreGeneratedValueMethod
        = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethod(nameof(ReadStoreGeneratedValue))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T ReadStoreGeneratedValue<T>(int storeGeneratedIndex)
        => _storeGeneratedValues.GetValue<T>(storeGeneratedIndex);

    private static readonly MethodInfo ReadTemporaryValueMethod
        = typeof(InternalEntityEntry).GetMethod(nameof(ReadTemporaryValue))!;

    [UnconditionalSuppressMessage(
        "ReflectionAnalysis", "IL2060",
        Justification = "MakeGenericMethod wrapper, see https://github.com/dotnet/linker/issues/2482")]
    internal static MethodInfo MakeReadTemporaryValueMethod(Type type)
        => ReadTemporaryValueMethod.MakeGenericMethod(type);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T ReadTemporaryValue<T>(int storeGeneratedIndex)
        => _temporaryValues.GetValue<T>(storeGeneratedIndex);

    private static readonly MethodInfo GetCurrentValueMethod
        = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethods(nameof(GetCurrentValue)).Single(
            m => m.IsGenericMethod);

    [UnconditionalSuppressMessage(
        "ReflectionAnalysis", "IL2060",
        Justification = "MakeGenericMethod wrapper, see https://github.com/dotnet/linker/issues/2482")]
    internal static MethodInfo MakeGetCurrentValueMethod(Type type)
        => GetCurrentValueMethod.MakeGenericMethod(type);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase)
        => ((Func<InternalEntityEntry, TProperty>)propertyBase.GetPropertyAccessors().CurrentValueGetter)(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TProperty GetOriginalValue<TProperty>(IProperty property)
        => ((Func<InternalEntityEntry, TProperty>)property.GetPropertyAccessors().OriginalValueGetter!)(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TProperty GetRelationshipSnapshotValue<TProperty>(IPropertyBase propertyBase)
        => ((Func<InternalEntityEntry, TProperty>)propertyBase.GetPropertyAccessors().RelationshipSnapshotGetter)(
            this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? ReadPropertyValue(IPropertyBase propertyBase)
        => propertyBase.IsShadowProperty()
            ? _shadowValues[propertyBase.GetShadowIndex()]
            : propertyBase.GetGetter().GetClrValueUsingContainingEntity(Entity);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private void WritePropertyValue(
        IPropertyBase propertyBase,
        object? value,
        bool forMaterialization)
    {
        if (propertyBase.IsShadowProperty())
        {
            _shadowValues[propertyBase.GetShadowIndex()] = value;
        }
        else
        {
            var concretePropertyBase = (IRuntimePropertyBase)propertyBase;

            var setter = forMaterialization
                ? concretePropertyBase.MaterializationSetter
                : concretePropertyBase.GetSetter();

            setter.SetClrValue(Entity, value);
        }
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
            : navigationBase.GetCollectionAccessor()!.GetOrCreate(Entity, forMaterialization);

    private object GetOrCreateShadowCollection(INavigationBase navigation)
    {
        var collection = _shadowValues[navigation.GetShadowIndex()];
        if (collection == null)
        {
            collection = navigation.GetCollectionAccessor()!.Create();
            _shadowValues[navigation.GetShadowIndex()] = collection;
        }

        return collection;
    }

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
            : collectionAccessor.Contains(Entity, value);
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
            return collectionAccessor.Add(Entity, value, forMaterialization);
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
            : collectionAccessor.Remove(Entity, value);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? GetCurrentValue(IPropertyBase propertyBase)
        => propertyBase is not IProperty property || !IsConceptualNull(property)
            ? this[propertyBase]
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? GetPreStoreGeneratedCurrentValue(IPropertyBase propertyBase)
        => propertyBase is not IProperty property || !IsConceptualNull(property)
            ? ReadPropertyValue(propertyBase)
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? GetOriginalValue(IPropertyBase propertyBase)
        => _originalValues.GetValue(this, (IProperty)propertyBase);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? GetOriginalOrCurrentValue(IPropertyBase propertyBase)
        => propertyBase.GetOriginalValueIndex() >= 0
            ? _originalValues.GetValue(this, (IProperty)propertyBase)
            : GetCurrentValue(propertyBase);

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
    public void SetOriginalValue(
        IPropertyBase propertyBase,
        object? value,
        int index = -1)
    {
        EnsureOriginalValues();

        var property = (IProperty)propertyBase;

        _originalValues.SetValue(property, value, index);

        // If setting the original value results in the current value being different from the
        // original value, then mark the property as modified.
        if ((EntityState == EntityState.Unchanged
                || (EntityState == EntityState.Modified && !IsModified(property)))
            && !_stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.Unknown))
        {
            ((StateManager as StateManager)?.ChangeDetector as ChangeDetector)?.DetectValueChange(this, property);
        }
    }

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
    public void EnsureOriginalValues()
    {
        if (_originalValues.IsEmpty)
        {
            _originalValues = new OriginalValues(this);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void EnsureTemporaryValues()
    {
        if (_temporaryValues.IsEmpty)
        {
            _temporaryValues = new SidecarValues(EntityType.TemporaryValuesFactory(this));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void EnsureStoreGeneratedValues()
    {
        if (_storeGeneratedValues.IsEmpty)
        {
            _storeGeneratedValues = new SidecarValues(EntityType.StoreGeneratedValuesFactory());
        }
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
    public bool HasOriginalValuesSnapshot
        => !_originalValues.IsEmpty;

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
    public object? this[IPropertyBase propertyBase]
    {
        get
        {
            var storeGeneratedIndex = propertyBase.GetStoreGeneratedIndex();
            if (storeGeneratedIndex != -1)
            {
                var property = (IProperty)propertyBase;
                var propertyIndex = property.GetIndex();

                if (FlaggedAsStoreGenerated(propertyIndex))
                {
                    return _storeGeneratedValues.GetValue(storeGeneratedIndex);
                }

                if (FlaggedAsTemporary(propertyIndex)
                    && HasSentinel(property))
                {
                    return _temporaryValues.GetValue(storeGeneratedIndex);
                }
            }

            return ReadPropertyValue(propertyBase);
        }

        set => SetProperty(propertyBase, value, isMaterialization: false);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool FlaggedAsStoreGenerated(int propertyIndex)
        => !_storeGeneratedValues.IsEmpty
            && _stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.IsStoreGenerated);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool FlaggedAsTemporary(int propertyIndex)
        => !_temporaryValues.IsEmpty
            && _stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.IsTemporary);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void SetProperty(
        IPropertyBase propertyBase,
        object? value,
        bool isMaterialization,
        bool setModified = true,
        bool isCascadeDelete = false)
        => SetProperty(propertyBase, value, isMaterialization, setModified, isCascadeDelete, CurrentValueType.Normal);

    private void SetProperty(
        IPropertyBase propertyBase,
        object? value,
        bool isMaterialization,
        bool setModified,
        bool isCascadeDelete,
        CurrentValueType valueType)
    {
        var currentValue = ReadPropertyValue(propertyBase);

        var asProperty = propertyBase as IProperty;
        int propertyIndex;
        CurrentValueType currentValueType;
        int storeGeneratedIndex;
        bool valuesEqual;

        if (asProperty != null)
        {
            propertyIndex = asProperty.GetIndex();
            valuesEqual = AreEqual(currentValue, value, asProperty);
            currentValueType = GetValueType(asProperty);
            storeGeneratedIndex = asProperty.GetStoreGeneratedIndex();
        }
        else
        {
            propertyIndex = -1;
            valuesEqual = ReferenceEquals(currentValue, value);
            currentValueType = CurrentValueType.Normal;
            storeGeneratedIndex = -1;
        }

        if (!valuesEqual
            || (propertyIndex != -1
                && (_stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.Unknown)
                    || _stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.Null)
                    || valueType != currentValueType)))
        {
            var writeValue = true;

            if (asProperty != null
                && valueType == CurrentValueType.Normal
                && (!asProperty.ClrType.IsNullableType()
                    || asProperty.GetContainingForeignKeys().Any(
                        fk => fk is { IsRequired: true, DeleteBehavior: DeleteBehavior.Cascade or DeleteBehavior.ClientCascade }
                            && fk.DeclaringEntityType.IsAssignableFrom(EntityType))))
            {
                if (value == null)
                {
                    HandleNullForeignKey(asProperty, setModified, isCascadeDelete);
                    writeValue = false;
                }
                else
                {
                    _stateData.FlagProperty(propertyIndex, PropertyFlag.Null, isFlagged: false);
                }
            }

            if (writeValue)
            {
                StateManager.InternalEntityEntryNotifier.PropertyChanging(this, propertyBase);

                if (storeGeneratedIndex == -1)
                {
                    WritePropertyValue(propertyBase, value, isMaterialization);
                }
                else
                {
                    switch (valueType)
                    {
                        case CurrentValueType.Normal:
                            WritePropertyValue(propertyBase, value, isMaterialization);
                            _stateData.FlagProperty(propertyIndex, PropertyFlag.IsTemporary, isFlagged: false);
                            _stateData.FlagProperty(propertyIndex, PropertyFlag.IsStoreGenerated, isFlagged: false);
                            break;
                        case CurrentValueType.StoreGenerated:
                            EnsureStoreGeneratedValues();
                            _storeGeneratedValues.SetValue(asProperty!, value, storeGeneratedIndex);
                            _stateData.FlagProperty(propertyIndex, PropertyFlag.IsStoreGenerated, isFlagged: true);
                            break;
                        case CurrentValueType.Temporary:
                            EnsureTemporaryValues();
                            _temporaryValues.SetValue(asProperty!, value, storeGeneratedIndex);
                            _stateData.FlagProperty(propertyIndex, PropertyFlag.IsTemporary, isFlagged: true);
                            _stateData.FlagProperty(propertyIndex, PropertyFlag.IsStoreGenerated, isFlagged: false);
                            if (!HasSentinel(asProperty!))
                            {
                                WritePropertyValue(propertyBase, value, isMaterialization);
                            }

                            break;
                        default:
                            Check.DebugFail($"Bad value type {valueType}");
                            break;
                    }
                }

                if (propertyIndex != -1)
                {
                    if (_stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.Unknown))
                    {
                        if (!_originalValues.IsEmpty)
                        {
                            SetOriginalValue(propertyBase, value);
                        }

                        _stateData.FlagProperty(propertyIndex, PropertyFlag.Unknown, isFlagged: false);
                    }
                }

                if (propertyBase is INavigationBase { IsCollection: false } navigation)
                {
                    SetIsLoaded(navigation, value != null);
                }

                StateManager.InternalEntityEntryNotifier.PropertyChanged(this, propertyBase, setModified);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void HandleNullForeignKey(
        IProperty property,
        bool setModified = false,
        bool isCascadeDelete = false)
    {
        if (EntityState != EntityState.Deleted
            && EntityState != EntityState.Detached)
        {
            _stateData.FlagProperty(property.GetIndex(), PropertyFlag.Null, isFlagged: true);

            if (setModified)
            {
                SetPropertyModified(
                    property, changeState: true, isModified: true,
                    isConceptualNull: true);
            }

            if (!isCascadeDelete
                && StateManager.DeleteOrphansTiming == CascadeTiming.Immediate)
            {
                HandleConceptualNulls(
                    StateManager.SensitiveLoggingEnabled,
                    force: false,
                    isCascadeDelete: false);
            }
        }
    }

    private static bool AreEqual(object? value, object? otherValue, IProperty property)
        => property.GetValueComparer().Equals(value, otherValue);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void AcceptChanges()
    {
        if (!_storeGeneratedValues.IsEmpty)
        {
            foreach (var property in EntityType.GetFlattenedProperties())
            {
                var storeGeneratedIndex = property.GetStoreGeneratedIndex();
                if (storeGeneratedIndex != -1
                    && _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.IsStoreGenerated)
                    && _storeGeneratedValues.TryGetValue(storeGeneratedIndex, out var value))
                {
                    this[property] = value;
                }
            }

            _storeGeneratedValues = new SidecarValues();
            _temporaryValues = new SidecarValues();
        }

        _stateData.FlagAllProperties(EntityType.PropertyCount, PropertyFlag.IsStoreGenerated, false);
        _stateData.FlagAllProperties(EntityType.PropertyCount, PropertyFlag.IsTemporary, false);
        _stateData.FlagAllProperties(EntityType.PropertyCount, PropertyFlag.Unknown, false);

        var currentState = EntityState;
        switch (currentState)
        {
            case EntityState.Unchanged:
            case EntityState.Detached:
                return;
            case EntityState.Added:
            case EntityState.Modified:
                _originalValues.AcceptChanges(this);
                SharedIdentityEntry?.AcceptChanges();

                SetEntityState(EntityState.Unchanged, true);
                break;
            case EntityState.Deleted:
                SetEntityState(EntityState.Detached);
                break;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalEntityEntry PrepareToSave()
    {
        var entityType = EntityType;

        if (EntityState == EntityState.Added)
        {
            foreach (var property in entityType.GetFlattenedProperties())
            {
                if (property.GetBeforeSaveBehavior() == PropertySaveBehavior.Throw
                    && !HasTemporaryValue(property)
                    && HasExplicitValue(property))
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyReadOnlyBeforeSave(
                            property.Name,
                            EntityType.DisplayName()));
                }

                if (property.IsKey()
                    && property.IsForeignKey()
                    && _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.Unknown)
                    && !IsStoreGenerated(property))
                {
                    if (property.GetContainingForeignKeys().Any(fk => fk.IsOwnership))
                    {
                        throw new InvalidOperationException(CoreStrings.SaveOwnedWithoutOwner(entityType.DisplayName()));
                    }

                    throw new InvalidOperationException(CoreStrings.UnknownKeyValue(entityType.DisplayName(), property.Name));
                }

                CheckForNullCollection(property);
            }

            CheckForNullComplexProperties();
        }
        else if (EntityState == EntityState.Modified)
        {
            foreach (var property in entityType.GetFlattenedProperties())
            {
                if (property.GetAfterSaveBehavior() == PropertySaveBehavior.Throw
                    && IsModified(property))
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyReadOnlyAfterSave(
                            property.Name,
                            EntityType.DisplayName()));
                }

                CheckForNullCollection(property);
                CheckForUnknownKey(property);
            }

            CheckForNullComplexProperties();
        }
        else if (EntityState == EntityState.Deleted)
        {
            foreach (var property in entityType.GetFlattenedProperties())
            {
                CheckForUnknownKey(property);
            }
        }

        DiscardStoreGeneratedValues();

        return this;

        void CheckForUnknownKey(IProperty property)
        {
            if (property.IsKey()
                && _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.Unknown))
            {
                throw new InvalidOperationException(CoreStrings.UnknownShadowKeyValue(entityType.DisplayName(), property.Name));
            }
        }

        void CheckForNullCollection(IProperty property)
        {
            if (property.GetElementType() != null
                && !property.IsNullable
                && GetCurrentValue(property) == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NullRequiredPrimitiveCollection(EntityType.DisplayName(), property.Name));
            }
        }

        void CheckForNullComplexProperties()
        {
            foreach (var complexProperty in entityType.GetFlattenedComplexProperties())
            {
                if (!complexProperty.IsNullable
                    && this[complexProperty] == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.NullRequiredComplexProperty(
                            complexProperty.DeclaringType.ClrType.ShortDisplayName(), complexProperty.Name));
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
    public void HandleConceptualNulls(bool sensitiveLoggingEnabled, bool force, bool isCascadeDelete)
    {
        var fks = new List<IForeignKey>();
        foreach (var foreignKey in EntityType.GetForeignKeys())
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            var properties = foreignKey.Properties;
            foreach (var property in properties)
            {
                if (_stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.Null))
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
                                _stateData.FlagProperty(toNull.GetIndex(), PropertyFlag.Null, isFlagged: false);
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
                    && _stateData.IsPropertyFlagged(p.GetIndex(), PropertyFlag.Null));

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
    public void DiscardStoreGeneratedValues()
    {
        if (!_storeGeneratedValues.IsEmpty)
        {
            _storeGeneratedValues = new SidecarValues();
            _stateData.FlagAllProperties(EntityType.PropertyCount, PropertyFlag.IsStoreGenerated, false);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool IsStoreGenerated(IProperty property)
        => (property.ValueGenerated.ForAdd()
                && EntityState == EntityState.Added
                && ((property.GetBeforeSaveBehavior() == PropertySaveBehavior.Ignore
                        && GetValueType(property) != CurrentValueType.StoreGenerated)
                    || HasTemporaryValue(property)
                    || !HasExplicitValue(property)))
            || (property.ValueGenerated.ForUpdate()
                && (EntityState is EntityState.Modified or EntityState.Deleted)
                && (property.GetAfterSaveBehavior() == PropertySaveBehavior.Ignore
                    || !IsModified(property)));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool HasStoreGeneratedValue(IProperty property)
        => GetValueType(property) == CurrentValueType.StoreGenerated;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasExplicitValue(IProperty property)
        => !HasSentinel(property)
            || _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.IsStoreGenerated)
            || _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.IsTemporary);

    private bool HasSentinel(IProperty property)
        => property.IsShadowProperty()
            ? AreEqual(_shadowValues[property.GetShadowIndex()], property.Sentinel, property)
            : property.GetGetter().HasSentinelUsingContainingEntity(Entity);

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
                if (_stateData.IsPropertyFlagged(keyProperty.GetIndex(), PropertyFlag.Unknown))
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
            _stateData.FlagProperty(navigation.GetIndex(), PropertyFlag.IsLoaded, isFlagged: loaded);
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
            ?? _stateData.IsPropertyFlagged(navigation.GetIndex(), PropertyFlag.IsLoaded);

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

    IUpdateEntry? IUpdateEntry.SharedIdentityEntry
        => SharedIdentityEntry;

    IEntityType IUpdateEntry.EntityType
        => EntityType;

    private enum CurrentValueType
    {
        Normal,
        StoreGenerated,
        Temporary
    }
}
