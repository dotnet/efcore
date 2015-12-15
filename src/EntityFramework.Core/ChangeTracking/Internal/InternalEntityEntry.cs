// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Update;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract partial class InternalEntityEntry : IUpdateEntry
    {
        private StateData _stateData;
        private OriginalValues _originalValues;
        private RelationshipsSnapshot _relationshipsSnapshot;
        private StoreGeneratedValues _storeGeneratedValues;

        protected InternalEntityEntry(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType)
        {
            StateManager = stateManager;
            EntityType = entityType;
            _stateData = new StateData(entityType.PropertyCount());
        }

        public abstract object Entity { get; }

        public virtual IEntityType EntityType { get; }

        public virtual IStateManager StateManager { get; }

        public virtual void SetEntityState(EntityState entityState, bool acceptChanges = false)
        {
            var oldState = _stateData.EntityState;

            if (PrepareForAdd(entityState))
            {
                StateManager.ValueGeneration.Generate(this);
            }

            SetEntityState(oldState, entityState, acceptChanges);
        }

        private bool PrepareForAdd(EntityState newState)
        {
            if ((newState != EntityState.Added)
                || (EntityState == EntityState.Added))
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

            StateManager.SingleQueryMode = false;

            return true;
        }

        private void SetEntityState(EntityState oldState, EntityState newState, bool acceptChanges)
        {
            // Prevent temp values from becoming permanent values
            if ((oldState == EntityState.Added)
                && (newState != EntityState.Added)
                && (newState != EntityState.Detached))
            {
                var hasTempValue = EntityType.GetProperties()
                    .FirstOrDefault(p => _stateData.IsPropertyFlagged(p.GetIndex(), PropertyFlag.TemporaryOrModified));

                if (hasTempValue != null)
                {
                    throw new InvalidOperationException(CoreStrings.TempValuePersists(hasTempValue.Name, EntityType.DisplayName(), newState));
                }
            }

            // The entity state can be Modified even if some properties are not modified so always
            // set all properties to modified if the entity state is explicitly set to Modified.
            if (newState == EntityState.Modified)
            {
                foreach (var property in EntityType.GetProperties().Where(
                    p => !p.IsReadOnlyAfterSave))
                {
                    _stateData.FlagProperty(property.GetIndex(), PropertyFlag.TemporaryOrModified, isFlagged: true);
                }

                StateManager.SingleQueryMode = false;
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

            if ((newState == EntityState.Unchanged)
                && (oldState == EntityState.Modified))
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
                        .Where(p => _stateData.IsPropertyFlagged(p.GetIndex(), PropertyFlag.TemporaryOrModified)))
                    {
                        this[property] = property.ClrType.GetDefaultValue();
                    }
                }
                var propertyCount = EntityType.PropertyCount();

                _stateData.FlagAllProperties(propertyCount, PropertyFlag.TemporaryOrModified, flagged: false);
                _stateData.FlagAllProperties(propertyCount, PropertyFlag.Null, flagged: false);

                StateManager.StopTracking(this);
            }

            StateManager.Notify.StateChanged(this, oldState, StateManager.SingleQueryMode == true);
        }

        public virtual void MarkUnchangedFromQuery()
        {
            StateManager.Notify.StateChanging(this, EntityState.Unchanged);
            _stateData.EntityState = EntityState.Unchanged;
            StateManager.Notify.StateChanged(this, EntityState.Detached, StateManager.SingleQueryMode == true);
        }

        public virtual EntityState EntityState => _stateData.EntityState;

        public virtual bool IsModified(IProperty property)
            => (_stateData.EntityState == EntityState.Modified)
               && _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.TemporaryOrModified);

        public virtual void SetPropertyModified([NotNull] IProperty property, bool isModified = true)
        {
            // TODO: Restore original value to reject changes when isModified is false
            // Issue #742

            var currentState = _stateData.EntityState;

            if ((currentState == EntityState.Added)
                || (currentState == EntityState.Detached))
            {
                MarkAsTemporary(property, isTemporary: false);

                SetOriginalValue(property, this[property]);
            }

            if ((currentState != EntityState.Modified)
                && (currentState != EntityState.Unchanged))
            {
                return;
            }

            if (isModified && property.IsKey())
            {
                throw new NotSupportedException(CoreStrings.KeyReadOnly(property.Name, EntityType.DisplayName()));
            }

            _stateData.FlagProperty(property.GetIndex(), PropertyFlag.TemporaryOrModified, isModified);

            // Don't change entity state if it is Added or Deleted
            if (isModified && (currentState == EntityState.Unchanged))
            {
                StateManager.Notify.StateChanging(this, EntityState.Modified);
                _stateData.EntityState = EntityState.Modified;
                StateManager.SingleQueryMode = false;
                StateManager.Notify.StateChanged(this, currentState, skipInitialFixup: false);
            }
            else if (!isModified
                     && !_stateData.AnyPropertiesFlagged(PropertyFlag.TemporaryOrModified))
            {
                StateManager.Notify.StateChanging(this, EntityState.Unchanged);
                _stateData.EntityState = EntityState.Unchanged;
                StateManager.Notify.StateChanged(this, currentState, skipInitialFixup: false);
            }
        }

        public virtual bool HasConceptualNull
            => (_stateData.EntityState != EntityState.Deleted)
               && _stateData.AnyPropertiesFlagged(PropertyFlag.Null);

        public virtual bool HasTemporaryValue([NotNull] IProperty property)
            => ((_stateData.EntityState == EntityState.Added) || (_stateData.EntityState == EntityState.Detached))
               && _stateData.IsPropertyFlagged(property.GetIndex(), PropertyFlag.TemporaryOrModified);

        public virtual void MarkAsTemporary([NotNull] IProperty property, bool isTemporary = true)
        {
            if ((_stateData.EntityState != EntityState.Added)
                && (_stateData.EntityState != EntityState.Detached))
            {
                return;
            }

            _stateData.FlagProperty(property.GetIndex(), PropertyFlag.TemporaryOrModified, isTemporary);
        }

        internal static readonly MethodInfo ReadShadowValueMethod
            = typeof(InternalEntityEntry).GetTypeInfo().GetDeclaredMethod(nameof(ReadShadowValue));

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
            => _storeGeneratedValues.GetValue<T>(currentValue, storeGeneratedIndex);

        internal static readonly MethodInfo GetCurrentValueMethod
            = typeof(InternalEntityEntry).GetMethods()
                .Single(m => m.Name == nameof(GetCurrentValue) && m.IsGenericMethod);

        public virtual TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase)
            => ((Func<InternalEntityEntry, TProperty>)propertyBase.GetPropertyAccessors().CurrentValueGetter)(this);

        public virtual TProperty GetOriginalValue<TProperty>(IProperty property)
            => ((Func<InternalEntityEntry, TProperty>)property.GetPropertyAccessors().OriginalValueGetter)(this);

        public virtual TProperty GetRelationshipSnapshotValue<TProperty>([NotNull] IPropertyBase propertyBase)
            => ((Func<InternalEntityEntry, TProperty>)propertyBase.GetPropertyAccessors().RelationshipSnapshotGetter)(this);

        protected virtual object ReadPropertyValue([NotNull] IPropertyBase propertyBase)
        {
            Debug.Assert(!(propertyBase is IProperty) || !((IProperty)propertyBase).IsShadowProperty);

            return propertyBase.GetGetter().GetClrValue(Entity);
        }

        protected virtual void WritePropertyValue([NotNull] IPropertyBase propertyBase, [CanBeNull] object value)
        {
            Debug.Assert(!(propertyBase is IProperty) || !((IProperty)propertyBase).IsShadowProperty);

            propertyBase.GetSetter().SetClrValue(Entity, value);
        }

        public virtual object GetCurrentValue(IPropertyBase propertyBase)
            => this[propertyBase];

        public virtual object GetOriginalValue(IPropertyBase propertyBase)
            => _originalValues.GetValue(this, (IProperty)propertyBase);

        public virtual object GetRelationshipSnapshotValue([NotNull] IPropertyBase propertyBase)
            => _relationshipsSnapshot.GetValue(this, propertyBase);

        public virtual void SetCurrentValue(IPropertyBase propertyBase, object value)
            => this[propertyBase] = value;

        public virtual void SetOriginalValue([NotNull] IPropertyBase propertyBase, [CanBeNull] object value)
        {
            EnsureOriginalValues();
            _originalValues.SetValue((IProperty)propertyBase, value);
        }

        public virtual void SetRelationshipSnapshotValue([NotNull] IPropertyBase propertyBase, [CanBeNull] object value)
        {
            EnsureRelationshipSnapshot();
            _relationshipsSnapshot.SetValue(propertyBase, value);
        }

        public virtual void EnsureOriginalValues()
        {
            if (_originalValues.IsEmpty)
            {
                _originalValues = new OriginalValues(this);
            }
        }

        public virtual void EnsureRelationshipSnapshot()
        {
            if (_relationshipsSnapshot.IsEmpty)
            {
                _relationshipsSnapshot = new RelationshipsSnapshot(this);
            }
        }

        public virtual bool HasRelationshipSnapshot => !_relationshipsSnapshot.IsEmpty;

        public virtual void RemoveFromCollectionSnapshot([NotNull] IPropertyBase propertyBase, [NotNull] object removedEntity)
        {
            EnsureRelationshipSnapshot();
            _relationshipsSnapshot.RemoveFromCollection(propertyBase, removedEntity);
        }

        public virtual void AddToCollectionSnapshot([NotNull] IPropertyBase propertyBase, [NotNull] object addedEntity)
        {
            EnsureRelationshipSnapshot();
            _relationshipsSnapshot.AddToCollection(propertyBase, addedEntity);
        }

        public virtual IKeyValue GetPrimaryKeyValue(ValueSource valueSource = ValueSource.Current)
        {
            var key = EntityType.FindPrimaryKey();
            return CreateKey(key, key.Properties, valueSource);
        }

        public virtual IKeyValue GetPrincipalKeyValue([NotNull] IKey key, ValueSource valueSource = ValueSource.Current)
            => CreateKey(key, key.Properties, valueSource);

        public virtual IKeyValue GetPrincipalKeyValue([NotNull] IForeignKey foreignKey, ValueSource valueSource = ValueSource.Current)
        {
            var key = foreignKey.PrincipalKey;
            return CreateKey(key, key.Properties, valueSource);
        }

        public virtual IKeyValue GetDependentKeyValue([NotNull] IForeignKey foreignKey, ValueSource valueSource = ValueSource.Current)
        {
            var key = foreignKey.PrincipalKey;
            return CreateKey(key, foreignKey.Properties, valueSource);
        }

        private IKeyValue CreateKey(
            [NotNull] IKey key,
            [NotNull] IReadOnlyList<IProperty> properties,
            ValueSource valueSource = ValueSource.Current)
        {
            var value = properties.Count == 1
                ? (valueSource == ValueSource.Current
                    ? this[properties[0]]
                    : valueSource == ValueSource.Original
                        ? _originalValues.GetValue(this, properties[0])
                        : _relationshipsSnapshot.GetValue(this, properties[0]))
                : (valueSource == ValueSource.Current
                    ? properties.Select(p => this[p]).ToArray()
                    : valueSource == ValueSource.Original
                        ? properties.Select(p => _originalValues.GetValue(this, p)).ToArray()
                        : properties.Select(p => _relationshipsSnapshot.GetValue(this, p)).ToArray());

            return StateManager.CreateKey(key, value);
        }

        public virtual object this[[NotNull] IPropertyBase propertyBase]
        {
            get
            {
                object value;
                return _storeGeneratedValues.TryGetValue(propertyBase, out value)
                    ? value
                    : ReadPropertyValue(propertyBase);
            }
            [param: CanBeNull]
            set
            {
                if (_storeGeneratedValues.CanStoreValue(propertyBase))
                {
                    StateManager.Notify.PropertyChanging(this, propertyBase);
                    _storeGeneratedValues.SetValue(propertyBase, value);
                    StateManager.Notify.PropertyChanged(this, propertyBase);
                }
                else
                {
                    var currentValue = this[propertyBase];

                    if (!Equals(currentValue, value))
                    {
                        var writeValue = true;
                        var asProperty = propertyBase as IProperty;

                        if ((asProperty != null)
                            && !asProperty.IsNullable)
                        {
                            if (value == null)
                            {
                                _stateData.FlagProperty(asProperty.GetIndex(), PropertyFlag.Null, isFlagged: true);
                                writeValue = false;
                            }
                            else
                            {
                                _stateData.FlagProperty(asProperty.GetIndex(), PropertyFlag.Null, isFlagged: false);
                            }
                        }

                        if (writeValue)
                        {
                            StateManager.Notify.PropertyChanging(this, propertyBase);
                            WritePropertyValue(propertyBase, value);
                            StateManager.Notify.PropertyChanged(this, propertyBase);
                        }
                    }
                }
            }
        }

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

        public virtual InternalEntityEntry PrepareToSave()
        {
            if (EntityState == EntityState.Added)
            {
                var setProperty = EntityType.GetProperties().FirstOrDefault(p => p.IsReadOnlyBeforeSave && !IsTemporaryOrDefault(p));
                if (setProperty != null)
                {
                    throw new InvalidOperationException(CoreStrings.PropertyReadOnlyBeforeSave(setProperty.Name, EntityType.DisplayName()));
                }
            }
            else if (EntityState == EntityState.Modified)
            {
                var modifiedProperty = EntityType.GetProperties().FirstOrDefault(p => p.IsReadOnlyAfterSave && IsModified(p));
                if (modifiedProperty != null)
                {
                    throw new InvalidOperationException(CoreStrings.PropertyReadOnlyAfterSave(modifiedProperty.Name, EntityType.DisplayName()));
                }
            }

            if (EntityType.StoreGeneratedCount() > 0)
            {
                _storeGeneratedValues = new StoreGeneratedValues(this);
            }

            return this;
        }

        public virtual void HandleConceptualNulls()
        {
            var fks = EntityType.GetForeignKeys()
                .Where(fk => fk.Properties
                    .Any(p => _stateData.IsPropertyFlagged(p.GetIndex(), PropertyFlag.Null)))
                .ToList();

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
                throw new InvalidOperationException(CoreStrings.PropertyConceptualNull(
                    EntityType.GetProperties().First(p => _stateData.IsPropertyFlagged(p.GetIndex(), PropertyFlag.Null)).Name,
                    EntityType.DisplayName()));
            }
        }

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

        private bool MayGetStoreValue([CanBeNull] IProperty property, IEntityType entityType)
            => (property != null)
               && ((property.ValueGenerated != ValueGenerated.Never)
                   || StateManager.ValueGeneration.MayGetTemporaryValue(property, entityType));

        public virtual void DiscardStoreGeneratedValues() => _storeGeneratedValues = new StoreGeneratedValues();

        public virtual bool IsStoreGenerated(IProperty property)
            => (property.ValueGenerated != ValueGenerated.Never)
               && (((EntityState == EntityState.Added)
                    && (property.IsStoreGeneratedAlways || IsTemporaryOrDefault(property)))
                   || ((property.ValueGenerated == ValueGenerated.OnAddOrUpdate) && (EntityState == EntityState.Modified) && (property.IsStoreGeneratedAlways || !IsModified(property))));

        private bool IsTemporaryOrDefault(IProperty property)
            => HasTemporaryValue(property)
               || property.ClrType.IsDefaultValue(this[property]);

        public virtual bool IsKeySet => !EntityType.FindPrimaryKey().Properties.Any(p => p.ClrType.IsDefaultValue(this[p]));

        [UsedImplicitly]
        private string DebuggerDisplay => GetPrimaryKeyValue() + " - " + EntityState;

        public virtual EntityEntry ToEntityEntry() => new EntityEntry(this);
    }
}
