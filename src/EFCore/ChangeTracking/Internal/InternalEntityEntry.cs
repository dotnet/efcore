// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract partial class InternalEntityEntry : IUpdateEntry
    {
        private StateData _stateData;
        private OriginalValues _originalValues;
        private RelationshipsSnapshot _relationshipsSnapshot;
        private StoreGeneratedValues _storeGeneratedValues;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public abstract object Entity { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEntityType EntityType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IStateManager StateManager { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetEntityState(EntityState entityState, bool acceptChanges = false)
        {
            var oldState = _stateData.EntityState;

            if (PrepareForAdd(entityState))
            {
                StateManager.ValueGeneration.Propagate(this);
                StateManager.ValueGeneration.Generate(this);
            }
            else if (EntityType.IsOwned()
                     && oldState == EntityState.Detached)
            {
                StateManager.ValueGeneration.Propagate(this);
            }

            SetEntityState(oldState, entityState, acceptChanges);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual async Task SetEntityStateAsync(
            EntityState entityState,
            bool acceptChanges,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var oldState = _stateData.EntityState;

            if (PrepareForAdd(entityState))
            {
                StateManager.ValueGeneration.Propagate(this);
                await StateManager.ValueGeneration.GenerateAsync(this, cancellationToken);
            }
            else if (EntityType.IsOwned()
                     && oldState == EntityState.Detached)
            {
                StateManager.ValueGeneration.Propagate(this);
            }

            SetEntityState(oldState, entityState, acceptChanges);
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
                _stateData.FlagAllProperties(EntityType.PropertyCount(), PropertyFlag.TemporaryOrModified, flagged: false);
            }

            // Temporarily change the internal state to unknown so that key generation, including setting key values
            // can happen without constraints on changing read-only values kicking in
            _stateData.EntityState = EntityState.Detached;

            StateManager.EndSingleQueryMode();

            return true;
        }

        private void SetEntityState(EntityState oldState, EntityState newState, bool acceptChanges)
        {
            // Prevent temp values from becoming permanent values
            if (oldState == EntityState.Added
                && newState != EntityState.Added
                && newState != EntityState.Detached)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var property in EntityType.GetProperties())
                {
                    if (HasTemporaryValue(property))
                    {
                        throw new InvalidOperationException(CoreStrings.TempValuePersists(property.Name, EntityType.DisplayName(), newState));
                    }
                }
            }

            // The entity state can be Modified even if some properties are not modified so always
            // set all properties to modified if the entity state is explicitly set to Modified.
            if (newState == EntityState.Modified)
            {
                _stateData.FlagAllProperties(EntityType.PropertyCount(), PropertyFlag.TemporaryOrModified, flagged: true);

                // Hot path; do not use LINQ
                foreach (var property in EntityType.GetProperties())
                {
                    if (property.IsReadOnlyAfterSave)
                    {
                        _stateData.FlagProperty(property.GetIndex(), PropertyFlag.TemporaryOrModified, isFlagged: false);
                    }
                }

                StateManager.EndSingleQueryMode();
            }

            if (oldState == newState)
            {
                return;
            }

            if (newState == EntityState.Unchanged)
            {
                _stateData.FlagAllProperties(EntityType.PropertyCount(), PropertyFlag.TemporaryOrModified, flagged: false);
            }

            StateManager.Notify.StateChanging(this, newState);

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
            _stateData.EntityState = newState;

            if (oldState == EntityState.Detached)
            {
                StateManager.StartTracking(this);
            }
            else if (newState == EntityState.Detached)
            {
                if (oldState == EntityState.Added)
                {
                    foreach (var property in EntityType.GetProperties()
                        .Where(p =>
                            {
                                var propertyIndex = p.GetIndex();
                                return _stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.TemporaryOrModified)
                                       && !_stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.Unknown);
                            }))
                    {
                        this[property] = property.ClrType.GetDefaultValue();
                    }
                }

                StateManager.StopTracking(this);
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

            StateManager.Notify.StateChanged(this, oldState, fromQuery: false);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void MarkUnchangedFromQuery([CanBeNull] ISet<IForeignKey> handledForeignKeys)
        {
            StateManager.Notify.StateChanging(this, EntityState.Unchanged);
            _stateData.EntityState = EntityState.Unchanged;
            StateManager.Notify.StateChanged(this, EntityState.Detached, fromQuery: true);

            var trackingQueryMode = StateManager.GetTrackingQueryMode(EntityType);
            if (trackingQueryMode != TrackingQueryMode.Simple)
            {
                StateManager.Notify.TrackedFromQuery(
                    this,
                    trackingQueryMode == TrackingQueryMode.Single
                        ? handledForeignKeys
                        : null);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityState EntityState => _stateData.EntityState;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsModified(IProperty property)
        {
            var propertyIndex = property.GetIndex();

            return _stateData.EntityState == EntityState.Modified
                   && _stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.TemporaryOrModified)
                   && !_stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.Unknown);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetPropertyModified(
            [NotNull] IProperty property,
            bool changeState = true,
            bool isModified = true,
            bool isConceptualNull = false)
        {
            var propertyIndex = property.GetIndex();
            _stateData.FlagProperty(propertyIndex, PropertyFlag.Unknown, false);

            var currentState = _stateData.EntityState;

            if (currentState == EntityState.Added
                || currentState == EntityState.Detached
                || !changeState)
            {
                object _;
                if (!_storeGeneratedValues.TryGetValue(property, out _))
                {
                    MarkAsTemporary(property, isTemporary: false);

                    var index = property.GetOriginalValueIndex();
                    if (index != -1)
                    {
                        SetOriginalValue(property, this[property], index);
                    }
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
                    SetProperty(property, GetOriginalValue(property), setModified: false);
                }
                _stateData.FlagProperty(propertyIndex, PropertyFlag.TemporaryOrModified, isModified);
            }

            if (isModified
                && (currentState == EntityState.Unchanged
                    || currentState == EntityState.Detached))
            {
                if (changeState)
                {
                    StateManager.Notify.StateChanging(this, EntityState.Modified);
                    _stateData.EntityState = EntityState.Modified;
                }

                StateManager.EndSingleQueryMode();

                if (changeState)
                {
                    StateManager.ChangedCount++;
                    StateManager.Notify.StateChanged(this, currentState, fromQuery: false);
                }
            }
            else if (currentState != EntityState.Detached
                     && changeState
                     && !isModified
                     && !_stateData.AnyPropertiesFlagged(PropertyFlag.TemporaryOrModified))
            {
                StateManager.Notify.StateChanging(this, EntityState.Unchanged);
                _stateData.EntityState = EntityState.Unchanged;
                StateManager.ChangedCount--;
                StateManager.Notify.StateChanged(this, currentState, fromQuery: false);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasConceptualNull
            => _stateData.AnyPropertiesFlagged(PropertyFlag.Null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsConceptualNull([NotNull] IProperty property)
            => _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.Null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasTemporaryValue(IProperty property)
        {
            object _;
            return (_stateData.EntityState == EntityState.Added || _stateData.EntityState == EntityState.Detached)
                   && _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.TemporaryOrModified)
                   && (_storeGeneratedValues.IsEmpty
                       || !_storeGeneratedValues.TryGetValue(property, out _));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void MarkAsTemporary([NotNull] IProperty property, bool isTemporary = true)
        {
            if (_stateData.EntityState != EntityState.Added
                && _stateData.EntityState != EntityState.Detached)
            {
                return;
            }

            var index = property.GetIndex();
            _stateData.FlagProperty(index, PropertyFlag.TemporaryOrModified, isTemporary);
            _stateData.FlagProperty(index, PropertyFlag.Unknown, isFlagged: false);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void MarkShadowPropertiesNotSet([NotNull] IEntityType entityType)
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.IsShadowProperty)
                {
                    _stateData.FlagProperty(property.GetIndex(), PropertyFlag.Unknown, true);
                }
            }
        }

        internal static readonly MethodInfo ReadShadowValueMethod
            = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethod(nameof(ReadShadowValue));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [UsedImplicitly]
        protected virtual T ReadShadowValue<T>(int shadowIndex) => default(T);

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
        private T ReadStoreGeneratedValue<T>(T currentValue, int storeGeneratedIndex)
            => _storeGeneratedValues.GetValue(currentValue, storeGeneratedIndex);

        internal static readonly MethodInfo GetCurrentValueMethod
            = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethods(nameof(GetCurrentValue)).Single(m => m.IsGenericMethod);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase)
            => ((Func<InternalEntityEntry, TProperty>)propertyBase.GetPropertyAccessors().CurrentValueGetter)(this);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TProperty GetOriginalValue<TProperty>(IProperty property)
            => ((Func<InternalEntityEntry, TProperty>)property.GetPropertyAccessors().OriginalValueGetter)(this);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TProperty GetRelationshipSnapshotValue<TProperty>([NotNull] IPropertyBase propertyBase)
            => ((Func<InternalEntityEntry, TProperty>)propertyBase.GetPropertyAccessors().RelationshipSnapshotGetter)(this);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual object ReadPropertyValue([NotNull] IPropertyBase propertyBase)
        {
            Debug.Assert(!propertyBase.IsShadowProperty);

            return propertyBase.GetGetter().GetClrValue(Entity);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void WritePropertyValue([NotNull] IPropertyBase propertyBase, [CanBeNull] object value)
        {
            Debug.Assert(!propertyBase.IsShadowProperty);

            propertyBase.GetSetter().SetClrValue(Entity, value);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object GetCurrentValue(IPropertyBase propertyBase)
        {
            var property = propertyBase as IProperty;
            return property == null || !IsConceptualNull(property)
                ? this[propertyBase]
                : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object GetPreStoreGeneratedCurrentValue([NotNull] IPropertyBase propertyBase)
        {
            var property = propertyBase as IProperty;
            return property == null || !IsConceptualNull(property)
                ? ReadPropertyValue(propertyBase)
                : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object GetOriginalValue(IPropertyBase propertyBase)
            => _originalValues.GetValue(this, (IProperty)propertyBase);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object GetRelationshipSnapshotValue([NotNull] IPropertyBase propertyBase)
            => _relationshipsSnapshot.GetValue(this, propertyBase);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetCurrentValue(IPropertyBase propertyBase, object value)
            => this[propertyBase] = value;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetOriginalValue([NotNull] IPropertyBase propertyBase, [CanBeNull] object value, int index = -1)
        {
            EnsureOriginalValues();

            var property = (IProperty)propertyBase;

            _originalValues.SetValue(property, value, index);

            // If setting the original value results in the current value being different from the
            // original value, then mark the property as modified.
            if (EntityState == EntityState.Unchanged
                || (EntityState == EntityState.Modified
                    && !IsModified(property)))
            {
                var currentValue = this[propertyBase];
                var propertyIndex = property.GetIndex();
                if (!Equals(currentValue, value)
                    && !_stateData.IsPropertyFlagged(propertyIndex, PropertyFlag.Unknown))
                {
                    SetPropertyModified(property);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetRelationshipSnapshotValue([NotNull] IPropertyBase propertyBase, [CanBeNull] object value)
        {
            EnsureRelationshipSnapshot();
            _relationshipsSnapshot.SetValue(propertyBase, value);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void EnsureOriginalValues()
        {
            if (_originalValues.IsEmpty)
            {
                _originalValues = new OriginalValues(this);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void EnsureRelationshipSnapshot()
        {
            if (_relationshipsSnapshot.IsEmpty)
            {
                _relationshipsSnapshot = new RelationshipsSnapshot(this);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasOriginalValuesSnapshot => !_originalValues.IsEmpty;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasRelationshipSnapshot => !_relationshipsSnapshot.IsEmpty;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void RemoveFromCollectionSnapshot([NotNull] IPropertyBase propertyBase, [NotNull] object removedEntity)
        {
            EnsureRelationshipSnapshot();
            _relationshipsSnapshot.RemoveFromCollection(propertyBase, removedEntity);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddToCollectionSnapshot([NotNull] IPropertyBase propertyBase, [NotNull] object addedEntity)
        {
            EnsureRelationshipSnapshot();
            _relationshipsSnapshot.AddToCollection(propertyBase, addedEntity);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddRangeToCollectionSnapshot([NotNull] IPropertyBase propertyBase, [NotNull] IEnumerable<object> addedEntities)
        {
            EnsureRelationshipSnapshot();
            _relationshipsSnapshot.AddRangeToCollection(propertyBase, addedEntities);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object this[[NotNull] IPropertyBase propertyBase]
        {
            get
            {
                object value;
                return _storeGeneratedValues.TryGetValue(propertyBase, out value)
                    ? value
                    : ReadPropertyValue(propertyBase);
            }
            [param: CanBeNull] set { SetProperty(propertyBase, value); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetProperty([NotNull] IPropertyBase propertyBase, [CanBeNull] object value, bool setModified = true)
        {
            if (_storeGeneratedValues.CanStoreValue(propertyBase))
            {
                StateManager.Notify.PropertyChanging(this, propertyBase);
                _storeGeneratedValues.SetValue(propertyBase, value);
                StateManager.Notify.PropertyChanged(this, propertyBase, setModified);
            }
            else
            {
                var currentValue = this[propertyBase];

                var asProperty = propertyBase as IProperty;
                var propertyIndex = asProperty?.GetIndex();

                if (!Equals(currentValue, value)
                    || (propertyIndex.HasValue
                        && (_stateData.IsPropertyFlagged(propertyIndex.Value, PropertyFlag.Unknown)
                            || _stateData.IsPropertyFlagged(propertyIndex.Value, PropertyFlag.Null))))
                {
                    var writeValue = true;

                    if (asProperty != null
                        && !asProperty.IsNullable)
                    {
                        if (value == null)
                        {
                            if (EntityState != EntityState.Deleted
                                && EntityState != EntityState.Detached)
                            {
                                _stateData.FlagProperty(propertyIndex.Value, PropertyFlag.Null, isFlagged: true);
                                SetPropertyModified(asProperty, changeState: true, isModified: true, isConceptualNull: true);
                            }
                            writeValue = false;
                        }
                        else
                        {
                            _stateData.FlagProperty(propertyIndex.Value, PropertyFlag.Null, isFlagged: false);
                        }
                    }

                    if (writeValue)
                    {
                        StateManager.Notify.PropertyChanging(this, propertyBase);
                        WritePropertyValue(propertyBase, value);

                        if (propertyIndex.HasValue)
                        {
                            _stateData.FlagProperty(propertyIndex.Value, PropertyFlag.Unknown, isFlagged: false);
                        }

                        if (asProperty == null)
                        {
                            var navigation = (INavigation)propertyBase;
                            if (!navigation.IsCollection()
                                && value == null)
                            {
                                SetIsLoaded(navigation, loaded: false);
                            }
                        }

                        StateManager.Notify.PropertyChanged(this, propertyBase, setModified);
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AcceptChanges()
        {
            if (!_storeGeneratedValues.IsEmpty)
            {
                var storeGeneratedValues = _storeGeneratedValues;
                _storeGeneratedValues = new StoreGeneratedValues();

                foreach (var property in EntityType.GetProperties())
                {
                    object value;
                    if (storeGeneratedValues.TryGetValue(property, out value))
                    {
                        this[property] = value;
                    }
                }
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

                SetEntityState(EntityState.Unchanged, true);
            }
            else if (currentState == EntityState.Deleted)
            {
                SetEntityState(EntityState.Detached);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry PrepareToSave()
        {
            if (EntityState == EntityState.Added)
            {
                foreach (var property in EntityType.GetProperties())
                {
                    if (property.IsReadOnlyBeforeSave
                        && !HasTemporaryValue(property)
                        && !HasDefaultValue(property))
                    {
                        throw new InvalidOperationException(CoreStrings.PropertyReadOnlyBeforeSave(property.Name, EntityType.DisplayName()));
                    }
                }
            }
            else if (EntityState == EntityState.Modified)
            {
                foreach (var property in EntityType.GetProperties())
                {
                    if (property.IsReadOnlyAfterSave
                        && IsModified(property))
                    {
                        throw new InvalidOperationException(CoreStrings.PropertyReadOnlyAfterSave(property.Name, EntityType.DisplayName()));
                    }
                }
            }

            if (EntityType.StoreGeneratedCount() > 0)
            {
                DiscardStoreGeneratedValues();
                _storeGeneratedValues = new StoreGeneratedValues(this);
            }

            return this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void HandleConceptualNulls()
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
                        if (properties.Any(p => p.IsNullable))
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
                        else
                        {
                            fks.Add(foreignKey);
                        }
                        break;
                    }
                }
            }

            if (fks.Any(fk => fk.DeleteBehavior == DeleteBehavior.Cascade))
            {
                SetEntityState(EntityState == EntityState.Added
                    ? EntityState.Detached
                    : EntityState.Deleted);
            }
            else if (fks.Any())
            {
                throw new InvalidOperationException(CoreStrings.RelationshipConceptualNull(
                    fks.First().PrincipalEntityType.DisplayName(),
                    EntityType.DisplayName()));
            }
            else
            {
                var property = EntityType.GetProperties().FirstOrDefault(
                    p => _stateData.IsPropertyFlagged(p.GetIndex(), PropertyFlag.Null));

                if (property != null)
                {
                    throw new InvalidOperationException(CoreStrings.PropertyConceptualNull(
                        property.Name,
                        EntityType.DisplayName()));
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void CascadeDelete()
        {
            foreach (var fk in EntityType.GetReferencingForeignKeys())
            {
                foreach (var dependent in (StateManager.GetDependentsFromNavigation(this, fk)
                                           ?? StateManager.GetDependents(this, fk)).ToList())
                {
                    if ((dependent.EntityState != EntityState.Deleted)
                        && (dependent.EntityState != EntityState.Detached))
                    {
                        if (fk.DeleteBehavior == DeleteBehavior.Cascade)
                        {
                            dependent.SetEntityState(dependent.EntityState == EntityState.Added
                                ? EntityState.Detached
                                : EntityState.Deleted);

                            dependent.CascadeDelete();
                        }
                        else
                        {
                            foreach (var dependentProperty in fk.Properties)
                            {
                                dependent[dependentProperty] = null;
                            }

                            if (dependent.HasConceptualNull)
                            {
                                dependent.HandleConceptualNulls();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void DiscardStoreGeneratedValues()
        {
            if (!_storeGeneratedValues.IsEmpty)
            {
                var storeGeneratedValues = _storeGeneratedValues;
                _storeGeneratedValues = new StoreGeneratedValues();

                foreach (var property in EntityType.GetProperties())
                {
                    object value;
                    if (storeGeneratedValues.TryGetValue(property, out value))
                    {
                        var isTemp = HasTemporaryValue(property);

                        StateManager.Notify.PropertyChanged(this, property, setModified: false);

                        if (isTemp)
                        {
                            MarkAsTemporary(property);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsStoreGenerated(IProperty property)
            => property.ValueGenerated != ValueGenerated.Never
               && ((EntityState == EntityState.Added
                    && (property.IsStoreGeneratedAlways
                        || HasTemporaryValue(property)
                        || HasDefaultValue(property)))
                   || (property.ValueGenerated == ValueGenerated.OnAddOrUpdate && EntityState == EntityState.Modified && (property.IsStoreGeneratedAlways || !IsModified(property))));

        private bool HasDefaultValue(IProperty property)
            => property.ClrType.IsDefaultValue(this[property]);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsKeySet => !EntityType.FindPrimaryKey().Properties.Any(
            p => HasDefaultValue(p)
                 && (p.ValueGenerated == ValueGenerated.OnAdd
                     || p.IsForeignKey()));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityEntry ToEntityEntry() => new EntityEntry(this);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void HandleINotifyPropertyChanging([NotNull] object sender, [NotNull] PropertyChangingEventArgs eventArgs)
        {
            foreach (var propertyBase in EntityType.GetNotificationProperties(eventArgs.PropertyName))
            {
                StateManager.Notify.PropertyChanging(this, propertyBase);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void HandleINotifyPropertyChanged([NotNull] object sender, [NotNull] PropertyChangedEventArgs eventArgs)
        {
            foreach (var propertyBase in EntityType.GetNotificationProperties(eventArgs.PropertyName))
            {
                StateManager.Notify.PropertyChanged(this, propertyBase, setModified: true);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void HandleINotifyCollectionChanged([NotNull] object sender, [NotNull] NotifyCollectionChangedEventArgs eventArgs)
        {
            var navigation = EntityType.GetNavigations().FirstOrDefault(n => n.IsCollection() && this[n] == sender);
            if (navigation != null)
            {
                switch (eventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        StateManager.Notify.NavigationCollectionChanged(
                            this,
                            navigation,
                            eventArgs.NewItems.OfType<object>(),
                            Enumerable.Empty<object>());
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        StateManager.Notify.NavigationCollectionChanged(
                            this,
                            navigation,
                            Enumerable.Empty<object>(),
                            eventArgs.OldItems.OfType<object>());
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        StateManager.Notify.NavigationCollectionChanged(
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsLoaded([NotNull] INavigation navigation)
            => (!navigation.IsCollection()
                && EntityState != EntityState.Detached
                && this[navigation] != null)
               || _stateData.IsPropertyFlagged(navigation.GetIndex(), PropertyFlag.IsLoaded);
    }
}
