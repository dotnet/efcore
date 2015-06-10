// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract partial class InternalEntityEntry : IPropertyAccessor
    {
        private StateData _stateData;
        private Sidecar[] _sidecars;

        protected InternalEntityEntry(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType,
            [NotNull] IEntityEntryMetadataServices metadataServices)
        {
            StateManager = stateManager;
            MetadataServices = metadataServices;
            EntityType = entityType;
            _stateData = new StateData(entityType.GetProperties().Count());
        }

        public abstract object Entity { get; }

        public virtual IEntityType EntityType { get; }

        public virtual IStateManager StateManager { get; }

        protected virtual IEntityEntryMetadataServices MetadataServices { get; }

        public virtual Sidecar OriginalValues => TryGetSidecar(Sidecar.WellKnownNames.OriginalValues)
                                                 ?? AddSidecar(MetadataServices.CreateOriginalValues(this));

        public virtual Sidecar RelationshipsSnapshot => TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot)
                                                        ?? AddSidecar(MetadataServices.CreateRelationshipSnapshot(this));

        public virtual Sidecar AddSidecar([NotNull] Sidecar sidecar)
        {
            var newArray = new[] { sidecar };
            _sidecars = _sidecars == null
                ? newArray
                : newArray.Concat(_sidecars).ToArray();

            if (sidecar.TransparentRead
                || sidecar.TransparentWrite
                || sidecar.AutoCommit)
            {
                _stateData.TransparentSidecarInUse = true;
            }

            return sidecar;
        }

        public virtual Sidecar TryGetSidecar([NotNull] string name) => _sidecars?.FirstOrDefault(s => s.Name == name);

        public virtual void RemoveSidecar([NotNull] string name)
        {
            if (_sidecars == null)
            {
                return;
            }

            _sidecars = _sidecars.Where(v => v.Name != name).ToArray();

            if (_sidecars.Length == 0)
            {
                _sidecars = null;
                _stateData.TransparentSidecarInUse = false;
            }
            else
            {
                _stateData.TransparentSidecarInUse
                    = _sidecars.Any(s => s.TransparentRead || s.TransparentWrite || s.AutoCommit);
            }
        }

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
            if (newState != EntityState.Added
                || EntityState == EntityState.Added)
            {
                return false;
            }

            if (EntityState == EntityState.Modified)
            {
                _stateData.FlagAllProperties(EntityType.GetProperties().Count(), isFlagged: false);
            }

            // Temporarily change the internal state to unknown so that key generation, including setting key values
            // can happen without constraints on changing read-only values kicking in
            _stateData.EntityState = EntityState.Detached;

            return true;
        }

        private void SetEntityState(EntityState oldState, EntityState newState, bool acceptChanges)
        {
            // Prevent temp values from becoming permanent values
            if (oldState == EntityState.Added
                && newState != EntityState.Added
                && newState != EntityState.Detached)
            {
                var hasTempValue = EntityType.GetProperties().FirstOrDefault(p => _stateData.IsPropertyFlagged(p.Index));
                if (hasTempValue != null)
                {
                    throw new InvalidOperationException(Strings.TempValuePersists(hasTempValue.Name, EntityType.DisplayName(), newState));
                }
            }

            // The entity state can be Modified even if some properties are not modified so always
            // set all properties to modified if the entity state is explicitly set to Modified.
            if (newState == EntityState.Modified)
            {
                foreach (var property in EntityType.GetProperties().Where(
                    p => !p.IsReadOnlyAfterSave))
                {
                    _stateData.FlagProperty(property.Index, isFlagged: true);
                }
            }

            if (oldState == newState)
            {
                return;
            }

            if (newState == EntityState.Unchanged)
            {
                _stateData.FlagAllProperties(EntityType.GetProperties().Count(), isFlagged: false);
            }

            StateManager.Notify.StateChanging(this, newState);

            if (newState == EntityState.Unchanged
                && oldState == EntityState.Modified)
            {
                if (acceptChanges)
                {
                    SetOriginalValue();
                }
                else
                {
                    ResetToOriginalValue();
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
                    foreach (var property in EntityType.GetProperties().Where(p => _stateData.IsPropertyFlagged(p.Index)))
                    {
                        this[property] = property.SentinelValue;
                    }
                }
                _stateData.FlagAllProperties(EntityType.GetProperties().Count(), isFlagged: false);

                StateManager.StopTracking(this);
            }

            StateManager.Notify.StateChanged(this, oldState);
        }

        public virtual EntityState EntityState => _stateData.EntityState;

        public virtual bool IsPropertyModified([NotNull] IProperty property)
            => _stateData.EntityState == EntityState.Modified && _stateData.IsPropertyFlagged(property.Index);

        public virtual void SetPropertyModified([NotNull] IProperty property, bool isModified = true)
        {
            // TODO: Restore original value to reject changes when isModified is false
            // Issue #742

            var currentState = _stateData.EntityState;

            if (currentState == EntityState.Added
                || currentState == EntityState.Detached)
            {
                MarkAsTemporary(property, isTemporary: false);
                OriginalValues.TakeSnapshot(property);
            }

            if (currentState != EntityState.Modified
                && currentState != EntityState.Unchanged)
            {
                return;
            }

            if (isModified && property.IsKey())
            {
                throw new NotSupportedException(Strings.KeyReadOnly(property.Name, EntityType.DisplayName()));
            }

            _stateData.FlagProperty(property.Index, isModified);

            // Don't change entity state if it is Added or Deleted
            if (isModified && currentState == EntityState.Unchanged)
            {
                StateManager.Notify.StateChanging(this, EntityState.Modified);
                _stateData.EntityState = EntityState.Modified;
                StateManager.Notify.StateChanged(this, currentState);
            }
            else if (!isModified
                     && !_stateData.AnyPropertiesFlagged())
            {
                StateManager.Notify.StateChanging(this, EntityState.Unchanged);
                _stateData.EntityState = EntityState.Unchanged;
                StateManager.Notify.StateChanged(this, currentState);
            }
        }

        public virtual bool HasTemporaryValue([NotNull] IProperty property)
            => (_stateData.EntityState == EntityState.Added || _stateData.EntityState == EntityState.Detached)
               && _stateData.IsPropertyFlagged(property.Index);

        public virtual void MarkAsTemporary([NotNull] IProperty property, bool isTemporary = true)
        {
            if (_stateData.EntityState != EntityState.Added
                && _stateData.EntityState != EntityState.Detached)
            {
                return;
            }

            _stateData.FlagProperty(property.Index, isTemporary);
        }

        protected virtual object ReadPropertyValue([NotNull] IPropertyBase propertyBase)
        {
            Debug.Assert(!(propertyBase is IProperty) || !((IProperty)propertyBase).IsShadowProperty);

            return MetadataServices.ReadValue(Entity, propertyBase);
        }

        protected virtual void WritePropertyValue([NotNull] IPropertyBase propertyBase, [CanBeNull] object value)
        {
            Debug.Assert(!(propertyBase is IProperty) || !((IProperty)propertyBase).IsShadowProperty);

            MetadataServices.WriteValue(Entity, propertyBase, value);
        }

        public virtual object this[IPropertyBase property]
        {
            get
            {
                if (_stateData.TransparentSidecarInUse)
                {
                    foreach (var sidecar in _sidecars)
                    {
                        if (sidecar.TransparentRead
                            && sidecar.HasValue(property))
                        {
                            return sidecar[property];
                        }
                    }
                }

                return ReadPropertyValue(property);
            }
            set
            {
                if (_stateData.TransparentSidecarInUse)
                {
                    var wrote = false;
                    foreach (var sidecar in _sidecars)
                    {
                        if (sidecar.TransparentWrite
                            && sidecar.CanStoreValue(property))
                        {
                            StateManager.Notify.PropertyChanging(this, property);

                            sidecar[property] = value;
                            wrote = true;

                            StateManager.Notify.PropertyChanged(this, property);
                        }
                    }
                    if (wrote)
                    {
                        return;
                    }
                }

                var currentValue = this[property];

                if (!Equals(currentValue, value))
                {
                    StateManager.Notify.PropertyChanging(this, property);

                    WritePropertyValue(property, value);

                    StateManager.Notify.PropertyChanged(this, property);
                }
            }
        }

        [NotNull]
        public virtual EntityKey CreateKey(
            [NotNull] IEntityType entityType,
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IPropertyAccessor propertyAccessor) => MetadataServices.CreateKey(entityType, properties, propertyAccessor);

        public virtual EntityKey GetDependentKeySnapshot([NotNull] IForeignKey foreignKey)
            => CreateKey(foreignKey.PrincipalEntityType, foreignKey.Properties, RelationshipsSnapshot);

        // TODO cache principal key in a way that does not break store-generated keys
        public virtual EntityKey GetPrincipalKey(
            [NotNull] IForeignKey foreignKey,
            [NotNull] IEntityType principalEntityType,
            [NotNull] IReadOnlyList<IProperty> principalProperties) => CreateKey(principalEntityType, principalProperties, this);

        public virtual object[] GetValueBuffer() => EntityType.GetProperties().Select(p => this[p]).ToArray();

        public virtual void AcceptChanges()
        {
            var currentState = EntityState;
            if (currentState == EntityState.Unchanged
                || currentState == EntityState.Detached)
            {
                return;
            }

            if (currentState == EntityState.Added
                || currentState == EntityState.Modified)
            {
                TryGetSidecar(Sidecar.WellKnownNames.OriginalValues)?.UpdateSnapshot();

                SetEntityState(EntityState.Unchanged, true);
            }
            else if (currentState == EntityState.Deleted)
            {
                SetEntityState(EntityState.Detached);
            }
        }

        public virtual void AutoCommitSidecars()
        {
            if (_stateData.TransparentSidecarInUse)
            {
                foreach (var sidecar in _sidecars)
                {
                    if (sidecar.AutoCommit)
                    {
                        sidecar.Commit();
                    }
                }
            }
        }

        public virtual InternalEntityEntry PrepareToSave()
        {
            if (EntityState == EntityState.Added)
            {
                var setProperty = EntityType.GetProperties().FirstOrDefault(p => p.IsReadOnlyBeforeSave && !IsTemporaryOrSentinel(p));
                if (setProperty != null)
                {
                    throw new InvalidOperationException(Strings.PropertyReadOnlyBeforeSave(setProperty.Name, EntityType.DisplayName()));
                }
            }
            else if (EntityState == EntityState.Modified)
            {
                var modifiedProperty = EntityType.GetProperties().FirstOrDefault(p => p.IsReadOnlyAfterSave && IsPropertyModified(p));
                if (modifiedProperty != null)
                {
                    throw new InvalidOperationException(Strings.PropertyReadOnlyAfterSave(modifiedProperty.Name, EntityType.DisplayName()));
                }
            }

            var properties = MayGetStoreValue();
            if (properties.Any())
            {
                AddSidecar(MetadataServices.CreateStoreGeneratedValues(this, properties));
            }

            return this;
        }

        private IReadOnlyList<IProperty> MayGetStoreValue()
        {
            var properties = EntityType.GetProperties().Where(p => MayGetStoreValue(p, EntityType)).ToList();

            foreach (var foreignKey in EntityType.GetForeignKeys())
            {
                foreach (var property in foreignKey.Properties)
                {
                    if (!properties.Contains(property)
                        && MayGetStoreValue(property.GetGenerationProperty(), EntityType))
                    {
                        properties.Add(property);
                    }
                }
            }
            return properties;
        }

        private bool MayGetStoreValue([CanBeNull] IProperty property, IEntityType entityType)
            => property != null
               && (property.StoreGeneratedPattern != StoreGeneratedPattern.None
                   || StateManager.ValueGeneration.MayGetTemporaryValue(property, entityType));

        public virtual void AutoRollbackSidecars()
        {
            if (_stateData.TransparentSidecarInUse)
            {
                foreach (var sidecar in _sidecars)
                {
                    if (sidecar.AutoCommit)
                    {
                        sidecar.Rollback();
                    }
                }
            }
        }

        public virtual void SetOriginalValue()
        {
            var entityType = EntityType;
            var originalValues = TryGetSidecar(Sidecar.WellKnownNames.OriginalValues);

            if (originalValues == null)
            {
                return;
            }

            foreach (var property in entityType.GetProperties())
            {
                if (originalValues.HasValue(property)
                    && !Equals(originalValues[property], this[property]))
                {
                    originalValues[property] = this[property];
                }
            }
        }

        public virtual void ResetToOriginalValue()
        {
            var entityType = EntityType;
            var originalValues = TryGetSidecar(Sidecar.WellKnownNames.OriginalValues);

            if (originalValues == null)
            {
                return;
            }

            foreach (var property in entityType.GetProperties())
            {
                if (!Equals(this[property], originalValues[property]))
                {
                    this[property] = originalValues[property];
                }
            }
        }

        public virtual bool StoreMustGenerateValue([NotNull] IProperty property)
            => property.StoreGeneratedPattern != StoreGeneratedPattern.None
               && ((EntityState == EntityState.Added && IsTemporaryOrSentinel(property))
                   || (property.StoreGeneratedPattern == StoreGeneratedPattern.Computed
                       && (EntityState == EntityState.Modified && !IsPropertyModified(property))));

        private bool IsTemporaryOrSentinel(IProperty property) => HasTemporaryValue(property) || property.IsSentinelValue(this[property]);

        public virtual bool IsKeySet => !EntityType.GetPrimaryKey().Properties.Any(p => p.IsSentinelValue(this[p]));

        [UsedImplicitly]
        private string DebuggerDisplay => this.GetPrimaryKeyValue() + " - " + EntityState;

        InternalEntityEntry IPropertyAccessor.InternalEntityEntry => this;
    }
}
