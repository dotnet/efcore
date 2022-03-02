// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
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
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
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
        EntityType = entityType;
        Entity = entity;
        _shadowValues = entityType.GetEmptyShadowValuesFactory()();
        _stateData = new StateData(entityType.PropertyCount(), entityType.NavigationCount());

        MarkShadowPropertiesNotSet(entityType);
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
        in ValueBuffer valueBuffer)
    {
        StateManager = stateManager;
        EntityType = entityType;
        Entity = entity;
        _shadowValues = ((IRuntimeEntityType)entityType).ShadowValuesFactory(valueBuffer);
        _stateData = new StateData(entityType.PropertyCount(), entityType.NavigationCount());
    }

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
    public IEntityType EntityType { [DebuggerStepThrough] get; }

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
        EntityState? forceStateWhenUnknownKey = null)
    {
        var oldState = _stateData.EntityState;
        var adding = PrepareForAdd(entityState);

        entityState = PropagateToUnknownKey(oldState, entityState, adding, forceStateWhenUnknownKey);

        if (adding || oldState is EntityState.Detached)
        {
            StateManager.ValueGenerationManager.Generate(this, includePrimaryKey: adding);
        }

        SetEntityState(oldState, entityState, acceptChanges, modifyProperties);
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
        CancellationToken cancellationToken = default)
    {
        var oldState = _stateData.EntityState;
        var adding = PrepareForAdd(entityState);

        entityState = PropagateToUnknownKey(oldState, entityState, adding, forceStateWhenUnknownKey);

        if (adding || oldState is EntityState.Detached)
        {
            await StateManager.ValueGenerationManager.GenerateAsync(this, includePrimaryKey: adding, cancellationToken)
                .ConfigureAwait(false);
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

        if (adding
            || (oldState == EntityState.Detached
                && keyUnknown))
        {
            var principalEntry = StateManager.ValueGenerationManager.Propagate(this);

            if (forceStateWhenUnknownKey.HasValue
                && keyUnknown
                && principalEntry != null
                && principalEntry.EntityState != EntityState.Detached
                && principalEntry.EntityState != EntityState.Deleted)
            {
                entityState = principalEntry.EntityState == EntityState.Added
                    ? EntityState.Added
                    : forceStateWhenUnknownKey.Value;
            }
        }

        return entityState;
    }

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
                EntityType.PropertyCount(), PropertyFlag.Modified,
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
            foreach (var property in entityType.GetProperties())
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
            _stateData.FlagAllProperties(entityType.PropertyCount(), PropertyFlag.Modified, flagged: true);

            // Hot path; do not use LINQ
            foreach (var property in entityType.GetProperties())
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
                entityType.PropertyCount(), PropertyFlag.Modified,
                flagged: false);
        }

        if (_stateData.EntityState != oldState)
        {
            _stateData.EntityState = oldState;
        }

        StateManager.StateChanging(this, newState);

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

        if ((newState == EntityState.Deleted
                || newState == EntityState.Detached)
            && HasConceptualNull)
        {
            _stateData.FlagAllProperties(entityType.PropertyCount(), PropertyFlag.Null, flagged: false);
        }

        if (oldState == EntityState.Detached
            || oldState == EntityState.Unchanged)
        {
            if (newState == EntityState.Added
                || newState == EntityState.Deleted
                || newState == EntityState.Modified)
            {
                StateManager.ChangedCount++;
            }
        }
        else if (newState == EntityState.Detached
                 || newState == EntityState.Unchanged)
        {
            StateManager.ChangedCount--;
        }

        FireStateChanged(oldState);

        HandleSharedIdentityEntry(newState);

        if ((newState == EntityState.Deleted
                || newState == EntityState.Detached)
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
                if (sharedIdentityEntry.EntityState == EntityState.Added
                    || sharedIdentityEntry.EntityState == EntityState.Modified)
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
        if (oldState == EntityState.Detached)
        {
            foreach (var serviceProperty in EntityType.GetServiceProperties())
            {
                this[serviceProperty]
                    = serviceProperty
                        .ParameterBinding
                        .ServiceDelegate(
                            new MaterializationContext(
                                ValueBuffer.Empty,
                                StateManager.Context),
                            EntityType,
                            Entity);
            }
        }
        else if (newState == EntityState.Detached)
        {
            foreach (var serviceProperty in EntityType.GetServiceProperties())
            {
                this[serviceProperty] = null;
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

        if (currentState == EntityState.Added
            || currentState == EntityState.Detached
            || !changeState)
        {
            var index = property.GetOriginalValueIndex();
            if (index != -1
                && !IsConceptualNull(property))
            {
                SetOriginalValue(property, this[property], index);
            }
        }

        if (currentState == EntityState.Added)
        {
            return;
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
            && (currentState == EntityState.Unchanged
                || currentState == EntityState.Detached))
        {
            if (changeState)
            {
                StateManager.StateChanging(this, EntityState.Modified);

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
            StateManager.StateChanging(this, EntityState.Unchanged);
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
            if (principalEntry._stateData.IsPropertyFlagged(principalProperty.GetIndex(), PropertyFlag.IsTemporary))
            {
                SetProperty(dependentProperty, principalValue, isMaterialization, setModified);
                _stateData.FlagProperty(dependentProperty.GetIndex(), PropertyFlag.IsTemporary, true);
            }
            else
            {
                SetTemporaryValue(dependentProperty, principalValue);
            }
        }
        else if (principalEntry.GetValueType(principalProperty) == CurrentValueType.StoreGenerated)
        {
            SetStoreGeneratedValue(dependentProperty, principalValue);
        }
        else
        {
            SetProperty(dependentProperty, principalValue, isMaterialization, setModified);
            _stateData.FlagProperty(dependentProperty.GetIndex(), PropertyFlag.IsTemporary, false);
        }
    }

    private CurrentValueType GetValueType(
        IProperty property,
        Func<object?, object?, bool>? equals = null)
    {
        if (_stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.IsTemporary))
        {
            return CurrentValueType.Temporary;
        }

        var tempIndex = property.GetStoreGeneratedIndex();
        if (tempIndex == -1)
        {
            return CurrentValueType.Normal;
        }

        if (!PropertyHasDefaultValue(property))
        {
            return CurrentValueType.Normal;
        }

        equals ??= ValuesEqualFunc(property);
        var defaultValue = property.ClrType.GetDefaultValue();
        var value = ReadPropertyValue(property);
        if (!equals(value, defaultValue))
        {
            return CurrentValueType.Normal;
        }

        if (_storeGeneratedValues.TryGetValue(tempIndex, out value)
            && !equals(value, defaultValue))
        {
            return CurrentValueType.StoreGenerated;
        }

        if (_temporaryValues.TryGetValue(tempIndex, out value)
            && !equals(value, defaultValue))
        {
            return CurrentValueType.Temporary;
        }

        return CurrentValueType.Normal;
    }

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
    public void SetStoreGeneratedValue(IProperty property, object? value)
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
            setModified: true,
            isCascadeDelete: false,
            CurrentValueType.StoreGenerated);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private void MarkShadowPropertiesNotSet(IEntityType entityType)
    {
        foreach (var property in entityType.GetProperties())
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
    public void MarkUnknown(IProperty property)
        => _stateData.FlagProperty(property.GetIndex(), PropertyFlag.Unknown, true);

    internal static readonly MethodInfo ReadShadowValueMethod
        = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethod(nameof(ReadShadowValue))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private T ReadShadowValue<T>(int shadowIndex)
        => _shadowValues.GetValue<T>(shadowIndex);

    internal static readonly MethodInfo ReadOriginalValueMethod
        = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethod(nameof(ReadOriginalValue))!;

    [UsedImplicitly]
    private T ReadOriginalValue<T>(IProperty property, int originalValueIndex)
        => _originalValues.GetValue<T>(this, property, originalValueIndex);

    internal static readonly MethodInfo ReadRelationshipSnapshotValueMethod
        = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethod(nameof(ReadRelationshipSnapshotValue))!;

    [UsedImplicitly]
    private T ReadRelationshipSnapshotValue<T>(IPropertyBase propertyBase, int relationshipSnapshotIndex)
        => _relationshipsSnapshot.GetValue<T>(this, propertyBase, relationshipSnapshotIndex);

    internal static readonly MethodInfo ReadStoreGeneratedValueMethod
        = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethod(nameof(ReadStoreGeneratedValue))!;

    [UsedImplicitly]
    private T ReadStoreGeneratedValue<T>(int storeGeneratedIndex)
        => _storeGeneratedValues.GetValue<T>(storeGeneratedIndex);

    internal static readonly MethodInfo ReadTemporaryValueMethod
        = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethod(nameof(ReadTemporaryValue))!;

    [UsedImplicitly]
    private T ReadTemporaryValue<T>(int storeGeneratedIndex)
        => _temporaryValues.GetValue<T>(storeGeneratedIndex);

    internal static readonly MethodInfo GetCurrentValueMethod
        = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethods(nameof(GetCurrentValue)).Single(
            m => m.IsGenericMethod);

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
        => ((Func<IUpdateEntry, TProperty>)propertyBase.GetPropertyAccessors().RelationshipSnapshotGetter)(
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
            : propertyBase.GetGetter().GetClrValue(Entity);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private bool PropertyHasDefaultValue(IPropertyBase propertyBase)
        => propertyBase.IsShadowProperty()
            ? propertyBase.ClrType.IsDefaultValue(_shadowValues[propertyBase.GetShadowIndex()])
            : propertyBase.GetGetter().HasDefaultValue(Entity);

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
                : concretePropertyBase.Setter;

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
            ? GetOrCreateCollectionTyped(navigationBase)
            : navigationBase.GetCollectionAccessor()!.GetOrCreate(Entity, forMaterialization);

    private ICollection<object> GetOrCreateCollectionTyped(INavigationBase navigation)
    {
        if (!(_shadowValues[navigation.GetShadowIndex()] is ICollection<object> collection))
        {
            collection = new HashSet<object>();
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
    public bool CollectionContains(INavigationBase navigationBase, InternalEntityEntry value)
        => navigationBase.IsShadowProperty()
            ? GetOrCreateCollectionTyped(navigationBase).Contains(value.Entity)
            : navigationBase.GetCollectionAccessor()!.Contains(Entity, value.Entity);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool AddToCollection(
        INavigationBase navigationBase,
        InternalEntityEntry value,
        bool forMaterialization)
    {
        if (!navigationBase.IsShadowProperty())
        {
            return navigationBase.GetCollectionAccessor()!.Add(Entity, value.Entity, forMaterialization);
        }

        var collection = GetOrCreateCollectionTyped(navigationBase);
        if (!collection.Contains(value.Entity))
        {
            collection.Add(value.Entity);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool RemoveFromCollection(INavigationBase navigationBase, InternalEntityEntry value)
        => navigationBase.IsShadowProperty()
            ? GetOrCreateCollectionTyped(navigationBase).Remove(value.Entity)
            : navigationBase.GetCollectionAccessor()!.Remove(Entity, value.Entity);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? GetCurrentValue(IPropertyBase propertyBase)
        => !(propertyBase is IProperty property) || !IsConceptualNull(property)
            ? this[propertyBase]
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? GetPreStoreGeneratedCurrentValue(IPropertyBase propertyBase)
        => !(propertyBase is IProperty property) || !IsConceptualNull(property)
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
            _temporaryValues = new SidecarValues(((IRuntimeEntityType)EntityType).TemporaryValuesFactory(this));
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
            _storeGeneratedValues = new SidecarValues(((IRuntimeEntityType)EntityType).StoreGeneratedValuesFactory());
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
                var propertyClrType = propertyBase.ClrType;
                var defaultValue = propertyClrType.GetDefaultValue();
                var property = (IProperty)propertyBase;

                var equals = ValuesEqualFunc(property);

                if (_storeGeneratedValues.TryGetValue(storeGeneratedIndex, out var generatedValue)
                    && !equals(generatedValue, defaultValue))
                {
                    return generatedValue;
                }

                var value = ReadPropertyValue(propertyBase);
                if (equals(value, defaultValue))
                {
                    if (_temporaryValues.TryGetValue(storeGeneratedIndex, out generatedValue)
                        && !equals(generatedValue, defaultValue))
                    {
                        return generatedValue;
                    }
                }

                return value;
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
        Func<object?, object?, bool> equals;

        if (asProperty != null)
        {
            propertyIndex = asProperty.GetIndex();
            equals = ValuesEqualFunc(asProperty);
            currentValueType = GetValueType(asProperty, equals);
        }
        else
        {
            propertyIndex = -1;
            equals = ReferenceEquals;
            currentValueType = CurrentValueType.Normal;
        }

        var valuesEqual = equals(currentValue, value);

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
                        fk => fk.IsRequired
                                && (fk.DeleteBehavior == DeleteBehavior.Cascade
                                || fk.DeleteBehavior == DeleteBehavior.ClientCascade)
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

                if (valueType == CurrentValueType.Normal)
                {
                    WritePropertyValue(propertyBase, value, isMaterialization);

                    switch (currentValueType)
                    {
                        case CurrentValueType.StoreGenerated:
                            if (!_storeGeneratedValues.IsEmpty)
                            {
                                var defaultValue = asProperty!.ClrType.GetDefaultValue();
                                var storeGeneratedIndex = asProperty.GetStoreGeneratedIndex();
                                _storeGeneratedValues.SetValue(asProperty, defaultValue, storeGeneratedIndex);
                            }

                            break;
                        case CurrentValueType.Temporary:
                            if (!_temporaryValues.IsEmpty)
                            {
                                var defaultValue = asProperty!.ClrType.GetDefaultValue();
                                var storeGeneratedIndex = asProperty.GetStoreGeneratedIndex();
                                _temporaryValues.SetValue(asProperty, defaultValue, storeGeneratedIndex);
                            }

                            break;
                    }
                }
                else
                {
                    var storeGeneratedIndex = asProperty!.GetStoreGeneratedIndex();
                    Check.DebugAssert(storeGeneratedIndex >= 0, $"storeGeneratedIndex is {storeGeneratedIndex}");

                    if (valueType == CurrentValueType.StoreGenerated)
                    {
                        var defaultValue = asProperty!.ClrType.GetDefaultValue();
                        if (!equals(currentValue, defaultValue))
                        {
                            WritePropertyValue(asProperty, defaultValue, isMaterialization);
                        }

                        EnsureStoreGeneratedValues();
                        _storeGeneratedValues.SetValue(asProperty, value, storeGeneratedIndex);
                    }
                    else
                    {
                        var defaultValue = asProperty!.ClrType.GetDefaultValue();
                        if (!equals(currentValue, defaultValue))
                        {
                            WritePropertyValue(asProperty, defaultValue, isMaterialization);
                        }

                        if (_storeGeneratedValues.TryGetValue(storeGeneratedIndex, out var generatedValue)
                            && !equals(generatedValue, defaultValue))
                        {
                            _storeGeneratedValues.SetValue(asProperty, defaultValue, storeGeneratedIndex);
                        }

                        EnsureTemporaryValues();
                        _temporaryValues.SetValue(asProperty, value, storeGeneratedIndex);
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

                if (propertyBase is INavigationBase navigation)
                {
                    if (!navigation.IsCollection)
                    {
                        SetIsLoaded(navigation, value != null);
                    }
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

    private static Func<object?, object?, bool> ValuesEqualFunc(IProperty property)
        => property.GetValueComparer().Equals;

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
            foreach (var property in EntityType.GetProperties())
            {
                var storeGeneratedIndex = property.GetStoreGeneratedIndex();
                if (storeGeneratedIndex != -1
                    && _storeGeneratedValues.TryGetValue(storeGeneratedIndex, out var value))
                {
                    var equals = ValuesEqualFunc(property);
                    var defaultValue = property.ClrType.GetDefaultValue();
                    if (!equals(value, defaultValue))
                    {
                        this[property] = value;
                    }
                }
            }

            _storeGeneratedValues = new SidecarValues();
            _temporaryValues = new SidecarValues();
        }

        _stateData.FlagAllProperties(EntityType.PropertyCount(), PropertyFlag.IsTemporary, false);
        _stateData.FlagAllProperties(EntityType.PropertyCount(), PropertyFlag.Unknown, false);

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
            foreach (var property in entityType.GetProperties())
            {
                if (property.GetBeforeSaveBehavior() == PropertySaveBehavior.Throw
                    && !HasTemporaryValue(property)
                    && !HasDefaultValue(property))
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyReadOnlyBeforeSave(
                            property.Name,
                            EntityType.DisplayName()));
                }

                if (property.IsKey()
                    && property.IsForeignKey()
                    && _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.Unknown))
                {
                    if (property.GetContainingForeignKeys().Any(fk => fk.IsOwnership))
                    {
                        throw new InvalidOperationException(CoreStrings.SaveOwnedWithoutOwner(entityType.DisplayName()));
                    }

                    throw new InvalidOperationException(CoreStrings.UnknownKeyValue(entityType.DisplayName(), property.Name));
                }
            }
        }
        else if (EntityState == EntityState.Modified)
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.GetAfterSaveBehavior() == PropertySaveBehavior.Throw
                    && IsModified(property))
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyReadOnlyAfterSave(
                            property.Name,
                            EntityType.DisplayName()));
                }

                CheckForUnknownKey(property);
            }
        }
        else if (EntityState == EntityState.Deleted)
        {
            foreach (var property in entityType.GetProperties())
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

        var cascadeFk = fks.FirstOrDefault(
            fk => fk.DeleteBehavior == DeleteBehavior.Cascade
                || fk.DeleteBehavior == DeleteBehavior.ClientCascade);
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
            var property = EntityType.GetProperties().FirstOrDefault(
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
                && (property.GetBeforeSaveBehavior() == PropertySaveBehavior.Ignore
                    || HasTemporaryValue(property)
                    || HasDefaultValue(property)))
            || (property.ValueGenerated.ForUpdate()
                && (EntityState == EntityState.Modified || EntityState == EntityState.Deleted)
                && (property.GetAfterSaveBehavior() == PropertySaveBehavior.Ignore
                    || !IsModified(property)));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasDefaultValue(IProperty property)
    {
        if (!PropertyHasDefaultValue(property))
        {
            return false;
        }

        var storeGeneratedIndex = property.GetStoreGeneratedIndex();
        if (storeGeneratedIndex == -1)
        {
            return true;
        }

        var defaultValue = property.ClrType.GetDefaultValue();
        var equals = ValuesEqualFunc(property);

        return (!_storeGeneratedValues.TryGetValue(storeGeneratedIndex, out var generatedValue)
                || equals(defaultValue, generatedValue))
            && (!_temporaryValues.TryGetValue(storeGeneratedIndex, out generatedValue)
                || equals(defaultValue, generatedValue));
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
                        || HasDefaultValue(keyProperty))
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
        }
    }

    private static IEnumerable<IPropertyBase> GetNotificationProperties(
        IEntityType entityType,
        string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            foreach (var property in entityType.GetProperties()
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

        _stateData.FlagProperty(navigation.GetIndex(), PropertyFlag.IsLoaded, isFlagged: loaded);

        foreach (var lazyLoaderProperty in EntityType.GetServiceProperties().Where(p => p.ClrType == typeof(ILazyLoader)))
        {
            ((ILazyLoader?)this[lazyLoaderProperty])?.SetLoaded(Entity, navigation.Name, loaded);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool IsLoaded(INavigationBase navigation)
        => _stateData.IsPropertyFlagged(navigation.GetIndex(), PropertyFlag.IsLoaded);

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

    private enum CurrentValueType
    {
        Normal,
        StoreGenerated,
        Temporary
    }
}
