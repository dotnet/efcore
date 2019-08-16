// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract partial class InternalEntityEntry : IUpdateEntry
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private readonly StateData _stateData;
        private OriginalValues _originalValues;
        private RelationshipsSnapshot _relationshipsSnapshot;
        private SidecarValues _temporaryValues;
        private SidecarValues _storeGeneratedValues;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected InternalEntityEntry(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType)
        {
            StateManager = stateManager;
            EntityType = entityType;
            _stateData = new StateData(entityType.PropertyCount(), entityType.NavigationCount());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public abstract object Entity { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IUpdateEntry.SetOriginalValue(IProperty property, object value)
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
        public virtual IEntityType EntityType { [DebuggerStepThrough] get; }

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
        public virtual IStateManager StateManager { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityEntry SharedIdentityEntry { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetEntityState(
            EntityState entityState,
            bool acceptChanges = false,
            bool modifyProperties = true,
            EntityState? forceStateWhenUnknownKey = null)
        {
            var oldState = _stateData.EntityState;
            var adding = PrepareForAdd(entityState);

            entityState = PropagateToUnknownKey(oldState, entityState, adding, forceStateWhenUnknownKey);

            if (adding)
            {
                StateManager.ValueGenerationManager.Generate(this);
            }

            SetEntityState(oldState, entityState, acceptChanges, modifyProperties);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task SetEntityStateAsync(
            EntityState entityState,
            bool acceptChanges = false,
            bool modifyProperties = true,
            EntityState? forceStateWhenUnknownKey = null,
            CancellationToken cancellationToken = default)
        {
            var oldState = _stateData.EntityState;
            var adding = PrepareForAdd(entityState);

            entityState = PropagateToUnknownKey(oldState, entityState, adding, forceStateWhenUnknownKey);

            if (adding)
            {
                await StateManager.ValueGenerationManager.GenerateAsync(this, cancellationToken);
            }

            SetEntityState(oldState, entityState, acceptChanges, modifyProperties);
        }

        private EntityState PropagateToUnknownKey(
            EntityState oldState, EntityState entityState, bool adding, EntityState? forceStateWhenUnknownKey)
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
            var entityType = (EntityType)EntityType;

            // Prevent temp values from becoming permanent values
            if (oldState == EntityState.Added
                && newState != EntityState.Added
                && newState != EntityState.Detached)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var property in entityType.GetProperties())
                {
                    if (HasTemporaryValue(property))
                    {
                        throw new InvalidOperationException(
                            CoreStrings.TempValuePersists(
                                property.Name,
                                EntityType.DisplayName(), newState));
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
                    EntityType.PropertyCount(), PropertyFlag.Modified,
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
                _stateData.FlagAllProperties(EntityType.PropertyCount(), PropertyFlag.Null, flagged: false);
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

            if ((newState == EntityState.Deleted
                 || newState == EntityState.Detached)
                && StateManager.CascadeDeleteTiming == CascadeTiming.Immediate)
            {
                StateManager.CascadeDelete(this, force: false);
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
                foreach (var serviceProperty in ((EntityType)EntityType).GetServiceProperties())
                {
                    this[serviceProperty]
                        = serviceProperty
                            .GetParameterBinding()
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
                foreach (var serviceProperty in ((EntityType)EntityType).GetServiceProperties())
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
        public virtual void MarkUnchangedFromQuery()
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
        public virtual EntityState EntityState => _stateData.EntityState;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsModified(IProperty property)
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
        public virtual void SetPropertyModified(
            [NotNull] IProperty property,
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
                && property.IsKey())
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
        public virtual bool HasConceptualNull
            => _stateData.AnyPropertiesFlagged(PropertyFlag.Null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsConceptualNull([NotNull] IProperty property)
            => _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.Null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool HasTemporaryValue(IProperty property)
            => GetValueType(property) == CurrentValueType.Temporary;

        private CurrentValueType GetValueType(
            IProperty property,
            Func<object, object, bool> equals = null)
        {
            var tempIndex = property.GetStoreGeneratedIndex();
            if (tempIndex == -1)
            {
                return CurrentValueType.Normal;
            }

            if (equals == null)
            {
                equals = ValuesEqualFunc(property);
            }

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
        public virtual void SetTemporaryValue([NotNull] IProperty property, object value, bool setModified = true)
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
        public virtual void SetStoreGeneratedValue(IProperty property, object value)
        {
            if (property.GetStoreGeneratedIndex() == -1)
            {
                throw new InvalidOperationException(
                    CoreStrings.StoreGenValue(property.Name, EntityType.DisplayName()));
            }

            SetProperty(property, value, isMaterialization: false, setModified: true, isCascadeDelete: false, CurrentValueType.StoreGenerated);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void MarkShadowPropertiesNotSet([NotNull] IEntityType entityType)
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.IsShadowProperty())
                {
                    _stateData.FlagProperty(property.GetIndex(), PropertyFlag.Unknown, true);
                }
            }
        }

        internal static readonly MethodInfo ReadShadowValueMethod
            = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethod(nameof(ReadShadowValue));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [UsedImplicitly]
        protected virtual T ReadShadowValue<T>(int shadowIndex) => default;

        internal static readonly MethodInfo ReadOriginalValueMethod
            = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethod(nameof(ReadOriginalValue));

        [UsedImplicitly]
        private T ReadOriginalValue<T>(IProperty property, int originalValueIndex)
            => _originalValues.GetValue<T>(this, property, originalValueIndex);

        internal static readonly MethodInfo ReadRelationshipSnapshotValueMethod
            = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethod(nameof(ReadRelationshipSnapshotValue));

        [UsedImplicitly]
        private T ReadRelationshipSnapshotValue<T>(IPropertyBase propertyBase, int relationshipSnapshotIndex)
            => _relationshipsSnapshot.GetValue<T>(this, propertyBase, relationshipSnapshotIndex);

        internal static readonly MethodInfo ReadStoreGeneratedValueMethod
            = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethod(nameof(ReadStoreGeneratedValue));

        [UsedImplicitly]
        private T ReadStoreGeneratedValue<T>(int storeGeneratedIndex)
            => _storeGeneratedValues.GetValue<T>(storeGeneratedIndex);

        internal static readonly MethodInfo ReadTemporaryValueMethod
            = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethod(nameof(ReadTemporaryValue));

        [UsedImplicitly]
        private T ReadTemporaryValue<T>(int storeGeneratedIndex)
            => _temporaryValues.GetValue<T>(storeGeneratedIndex);

        internal static readonly MethodInfo GetCurrentValueMethod
            =
            typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethods(nameof(GetCurrentValue)).Single(
                m => m.IsGenericMethod);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase)
            => ((Func<InternalEntityEntry, TProperty>)propertyBase.GetPropertyAccessors().CurrentValueGetter)(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TProperty GetOriginalValue<TProperty>(IProperty property)
            => ((Func<InternalEntityEntry, TProperty>)property.GetPropertyAccessors().OriginalValueGetter)(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TProperty GetRelationshipSnapshotValue<TProperty>([NotNull] IPropertyBase propertyBase)
            => ((Func<InternalEntityEntry, TProperty>)propertyBase.GetPropertyAccessors().RelationshipSnapshotGetter)(
                    this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual object ReadPropertyValue([NotNull] IPropertyBase propertyBase)
        {
            Debug.Assert(!propertyBase.IsShadowProperty());

            return ((PropertyBase)propertyBase).Getter.GetClrValue(Entity);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual bool PropertyHasDefaultValue([NotNull] IPropertyBase propertyBase)
        {
            Debug.Assert(!propertyBase.IsShadowProperty());

            return ((PropertyBase)propertyBase).Getter.HasDefaultValue(Entity);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void WritePropertyValue(
            [NotNull] IPropertyBase propertyBase,
            [CanBeNull] object value,
            bool forMaterialization)
        {
            Debug.Assert(!propertyBase.IsShadowProperty());

            var concretePropertyBase = (PropertyBase)propertyBase;

            var setter = forMaterialization
                ? concretePropertyBase.MaterializationSetter
                : concretePropertyBase.Setter;

            setter.SetClrValue(Entity, value);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual object GetOrCreateCollection([NotNull] INavigation navigation, bool forMaterialization)
        {
            Debug.Assert(!navigation.IsShadowProperty());

            return ((Navigation)navigation).CollectionAccessor.GetOrCreate(Entity, forMaterialization);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CollectionContains([NotNull] INavigation navigation, [NotNull] InternalEntityEntry value)
        {
            Debug.Assert(!navigation.IsShadowProperty());

            return ((Navigation)navigation).CollectionAccessor.Contains(Entity, value.Entity);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool AddToCollection(
            [NotNull] INavigation navigation,
            [NotNull] InternalEntityEntry value,
            bool forMaterialization)
        {
            Debug.Assert(!navigation.IsShadowProperty());

            return ((Navigation)navigation).CollectionAccessor.Add(Entity, value.Entity, forMaterialization);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool RemoveFromCollection([NotNull] INavigation navigation, [NotNull] InternalEntityEntry value)
        {
            Debug.Assert(!navigation.IsShadowProperty());

            return ((Navigation)navigation).CollectionAccessor.Remove(Entity, value.Entity);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual object GetCurrentValue(IPropertyBase propertyBase)
            => !(propertyBase is IProperty property) || !IsConceptualNull(property)
                ? this[propertyBase]
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual object GetPreStoreGeneratedCurrentValue([NotNull] IPropertyBase propertyBase)
            => !(propertyBase is IProperty property) || !IsConceptualNull(property)
                ? ReadPropertyValue(propertyBase)
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual object GetOriginalValue(IPropertyBase propertyBase)
            => _originalValues.GetValue(this, (IProperty)propertyBase);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual object GetRelationshipSnapshotValue([NotNull] IPropertyBase propertyBase)
            => _relationshipsSnapshot.GetValue(this, propertyBase);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetOriginalValue(
            [NotNull] IPropertyBase propertyBase, [CanBeNull] object value, int index = -1)
        {
            EnsureOriginalValues();

            var property = (Property)propertyBase;

            _originalValues.SetValue(property, value, index);

            // If setting the original value results in the current value being different from the
            // original value, then mark the property as modified.
            if (EntityState == EntityState.Unchanged
                || (EntityState == EntityState.Modified
                    && !IsModified(property)))
            {
                var currentValue = this[propertyBase];
                var propertyIndex = property.GetIndex();
                if (!ValuesEqualFunc(property)(currentValue, value)
                    && !_stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.Unknown))
                {
                    SetPropertyModified(property);
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetRelationshipSnapshotValue([NotNull] IPropertyBase propertyBase, [CanBeNull] object value)
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
        public virtual void EnsureOriginalValues()
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
        public virtual void EnsureTemporaryValues()
        {
            if (_temporaryValues.IsEmpty)
            {
                _temporaryValues = new SidecarValues(this);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void EnsureStoreGeneratedValues()
        {
            if (_storeGeneratedValues.IsEmpty)
            {
                _storeGeneratedValues = new SidecarValues(this);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void EnsureRelationshipSnapshot()
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
        public virtual bool HasOriginalValuesSnapshot => !_originalValues.IsEmpty;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool HasRelationshipSnapshot => !_relationshipsSnapshot.IsEmpty;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void RemoveFromCollectionSnapshot(
            [NotNull] IPropertyBase propertyBase,
            [NotNull] object removedEntity)
        {
            EnsureRelationshipSnapshot();
            _relationshipsSnapshot.RemoveFromCollection(propertyBase, removedEntity);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddToCollectionSnapshot([NotNull] IPropertyBase propertyBase, [NotNull] object addedEntity)
        {
            EnsureRelationshipSnapshot();
            _relationshipsSnapshot.AddToCollection(propertyBase, addedEntity);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddRangeToCollectionSnapshot(
            [NotNull] IPropertyBase propertyBase,
            [NotNull] IEnumerable<object> addedEntities)
        {
            EnsureRelationshipSnapshot();
            _relationshipsSnapshot.AddRangeToCollection(propertyBase, addedEntities);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[[NotNull] IPropertyBase propertyBase]
        {
            get
            {
                var value = ReadPropertyValue(propertyBase);

                var storeGeneratedIndex = propertyBase.GetStoreGeneratedIndex();
                if (storeGeneratedIndex != -1)
                {
                    var propertyClrType = propertyBase.ClrType;
                    var defaultValue = propertyClrType.GetDefaultValue();
                    var property = (IProperty)propertyBase;

                    var equals = ValuesEqualFunc(property);

                    if (equals(value, defaultValue))
                    {
                        if (_storeGeneratedValues.TryGetValue(storeGeneratedIndex, out var generatedValue)
                            && !equals(generatedValue, defaultValue))
                        {
                            return generatedValue;
                        }

                        if (_temporaryValues.TryGetValue(storeGeneratedIndex, out generatedValue)
                            && !equals(generatedValue, defaultValue))
                        {
                            return generatedValue;
                        }
                    }
                }

                return value;
            }

            [param: CanBeNull] set => SetProperty(propertyBase, value, isMaterialization: false);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetProperty(
            [NotNull] IPropertyBase propertyBase,
            [CanBeNull] object value,
            bool isMaterialization,
            bool setModified = true,
            bool isCascadeDelete = false)
            => SetProperty(propertyBase, value, isMaterialization, setModified, isCascadeDelete, CurrentValueType.Normal);

        private void SetProperty(
            [NotNull] IPropertyBase propertyBase,
            [CanBeNull] object value,
            bool isMaterialization,
            bool setModified,
            bool isCascadeDelete,
            CurrentValueType valueType)
        {
            var currentValue = this[propertyBase];

            var asProperty = propertyBase as Property;
            int propertyIndex;
            CurrentValueType currentValueType;
            Func<object, object, bool> equals;

            if (asProperty != null)
            {
                propertyIndex = asProperty.GetIndex();
                equals = ValuesEqualFunc(asProperty);
                currentValueType = GetValueType(asProperty, equals);
            }
            else
            {
                propertyIndex = -1;
                equals = (l, r) => ReferenceEquals(l, r);
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
                            fk => (fk.DeleteBehavior == DeleteBehavior.Cascade
                                   || fk.DeleteBehavior == DeleteBehavior.ClientCascade)
                                  && fk.DeclaringEntityType.IsAssignableFrom(EntityType))))
                {
                    if (value == null)
                    {
                        if (EntityState != EntityState.Deleted
                            && EntityState != EntityState.Detached)
                        {
                            _stateData.FlagProperty(propertyIndex, PropertyFlag.Null, isFlagged: true);

                            if (setModified)
                            {
                                SetPropertyModified(
                                    asProperty, changeState: true, isModified: true,
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
                    }
                    else
                    {
                        var storeGeneratedIndex = asProperty.GetStoreGeneratedIndex();
                        Debug.Assert(storeGeneratedIndex >= 0);

                        if (valueType == CurrentValueType.StoreGenerated)
                        {
                            EnsureStoreGeneratedValues();
                            _storeGeneratedValues.SetValue(asProperty, value, storeGeneratedIndex);
                        }
                        else
                        {
                            var defaultValue = asProperty.ClrType.GetDefaultValue();
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
                        _stateData.FlagProperty(propertyIndex, PropertyFlag.Unknown, isFlagged: false);
                    }

                    if (propertyBase is INavigation navigation)
                    {
                        if (!navigation.IsCollection())
                        {
                            SetIsLoaded(navigation, value != null);
                        }
                    }

                    StateManager.InternalEntityEntryNotifier.PropertyChanged(this, propertyBase, setModified);
                }
            }
        }

        private static Func<object, object, bool> ValuesEqualFunc(IProperty property)
        {
            var comparer = property.GetValueComparer()
                           ?? property.FindTypeMapping()?.Comparer;

            return comparer != null
                ? (Func<object, object, bool>)((l, r) => comparer.Equals(l, r))
                : (l, r) => Equals(l, r);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AcceptChanges()
        {
            if (!_storeGeneratedValues.IsEmpty)
            {
                foreach (var property in EntityType.GetProperties())
                {
                    var storeGeneratedIndex = property.GetStoreGeneratedIndex();
                    if (storeGeneratedIndex != -1
                        && _storeGeneratedValues.TryGetValue(storeGeneratedIndex, out var value))
                    {
                        this[property] = value;
                    }
                }

                _storeGeneratedValues = new SidecarValues();
                _temporaryValues = new SidecarValues();
            }

            var currentState = EntityState;
            if ((currentState == EntityState.Unchanged)
                || (currentState == EntityState.Detached))
            {
                return;
            }

            if ((currentState == EntityState.Added)
                || (currentState == EntityState.Modified))
            {
                _originalValues.AcceptChanges(this);
                SharedIdentityEntry?.AcceptChanges();

                SetEntityState(EntityState.Unchanged, true);
            }
            else if (currentState == EntityState.Deleted)
            {
                SetEntityState(EntityState.Detached);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityEntry PrepareToSave()
        {
            var entityType = (EntityType)EntityType;

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
                }
            }

            DiscardStoreGeneratedValues();

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void HandleConceptualNulls(bool sensitiveLoggingEnabled, bool force, bool isCascadeDelete)
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

            var cascadeFk = fks.FirstOrDefault(fk => fk.DeleteBehavior == DeleteBehavior.Cascade
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
        public virtual void DiscardStoreGeneratedValues()
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
        public virtual bool IsStoreGenerated(IProperty property)
            => (property.ValueGenerated.ForAdd()
                && EntityState == EntityState.Added
                && (property.GetBeforeSaveBehavior() == PropertySaveBehavior.Ignore
                    || HasTemporaryValue(property)
                    || HasDefaultValue(property)))
               || (property.ValueGenerated.ForUpdate()
                   && EntityState == EntityState.Modified
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

            if (_storeGeneratedValues.TryGetValue(storeGeneratedIndex, out var generatedValue)
                && !equals(defaultValue, generatedValue))
            {
                return false;
            }

            if (_temporaryValues.TryGetValue(storeGeneratedIndex, out generatedValue)
                && !equals(defaultValue, generatedValue))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual (bool IsGenerated, bool IsSet) IsKeySet
        {
            get
            {
                var isGenerated = false;
                var keyProperties = ((EntityType)EntityType).FindPrimaryKey().Properties;

                // ReSharper disable once ForCanBeConvertedToForeach
                // ReSharper disable once LoopCanBeConvertedToQuery
                for (var i = 0; i < keyProperties.Count; i++)
                {
                    var keyProperty = keyProperties[i];
                    var keyGenerated = keyProperty.ValueGenerated == ValueGenerated.OnAdd;

                    if ((HasTemporaryValue(keyProperty)
                         || HasDefaultValue(keyProperty))
                        && (keyGenerated || keyProperty.IsForeignKey()))
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
        public virtual bool IsKeyUnknown
        {
            get
            {
                var keyProperties = ((EntityType)EntityType).FindPrimaryKey().Properties;
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
        public virtual EntityEntry ToEntityEntry() => new EntityEntry(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void HandleINotifyPropertyChanging(
            [NotNull] object sender,
            [NotNull] PropertyChangingEventArgs eventArgs)
        {
            foreach (var propertyBase in EntityType.GetNotificationProperties(eventArgs.PropertyName))
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
        public virtual void HandleINotifyPropertyChanged(
            [NotNull] object sender,
            [NotNull] PropertyChangedEventArgs eventArgs)
        {
            foreach (var propertyBase in EntityType.GetNotificationProperties(eventArgs.PropertyName))
            {
                StateManager.InternalEntityEntryNotifier.PropertyChanged(this, propertyBase, setModified: true);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void HandleINotifyCollectionChanged(
            [NotNull] object sender,
            [NotNull] NotifyCollectionChangedEventArgs eventArgs)
        {
            var navigation = EntityType.GetNavigations().FirstOrDefault(n => n.IsCollection() && this[n] == sender);
            if (navigation != null)
            {
                switch (eventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        StateManager.InternalEntityEntryNotifier.NavigationCollectionChanged(
                            this,
                            navigation,
                            eventArgs.NewItems.OfType<object>(),
                            Enumerable.Empty<object>());
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        StateManager.InternalEntityEntryNotifier.NavigationCollectionChanged(
                            this,
                            navigation,
                            Enumerable.Empty<object>(),
                            eventArgs.OldItems.OfType<object>());
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        StateManager.InternalEntityEntryNotifier.NavigationCollectionChanged(
                            this,
                            navigation,
                            eventArgs.NewItems.OfType<object>(),
                            eventArgs.OldItems.OfType<object>());
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
        public virtual void SetIsLoaded([NotNull] INavigation navigation, bool loaded = true)
        {
            if (!loaded
                && !navigation.IsCollection()
                && this[navigation] != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.ReferenceMustBeLoaded(navigation.Name, navigation.DeclaringEntityType.DisplayName()));
            }

            _stateData.FlagProperty(navigation.GetIndex(), PropertyFlag.IsLoaded, isFlagged: loaded);

            var lazyLoaderProperty = EntityType.GetServiceProperties().FirstOrDefault(p => p.ClrType == typeof(ILazyLoader));
            if (lazyLoaderProperty != null)
            {
                ((ILazyLoader)this[lazyLoaderProperty])?.SetLoaded(Entity, navigation.Name, loaded);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsLoaded([NotNull] INavigation navigation)
            => _stateData.IsPropertyFlagged(navigation.GetIndex(), PropertyFlag.IsLoaded);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString()
            => $"{this.BuildCurrentValuesString(EntityType.FindPrimaryKey().Properties)} {EntityState}"
               + $"{(((IUpdateEntry)this).SharedIdentityEntry == null ? "" : " Shared")} {EntityType}";

        IUpdateEntry IUpdateEntry.SharedIdentityEntry => SharedIdentityEntry;

        private enum CurrentValueType
        {
            Normal,
            StoreGenerated,
            Temporary
        }
    }
}
