// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract partial class StateEntry : IPropertyBagEntry
    {
        private readonly StateEntryMetadataServices _metadataServices;
        private StateData _stateData;
        private Sidecar[] _sidecars;

        private readonly Dictionary<IForeignKey, EntityKey> _principalKeys = new Dictionary<IForeignKey, EntityKey>();

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected StateEntry()
        {
        }

        protected StateEntry(
            [NotNull] StateManager stateManager,
            [NotNull] IEntityType entityType,
            [NotNull] StateEntryMetadataServices metadataServices)
        {
            Check.NotNull(stateManager, "stateManager");
            Check.NotNull(entityType, "entityType");
            Check.NotNull(metadataServices, "metadataServices");

            StateManager = stateManager;
            _metadataServices = metadataServices;
            EntityType = entityType;
            _stateData = new StateData(EntityType.Properties.Count);
        }

        [CanBeNull]
        public abstract object Entity { get; }

        public virtual IEntityType EntityType { get; }

        public virtual StateManager StateManager { get; }

        public virtual Sidecar OriginalValues => TryGetSidecar(Sidecar.WellKnownNames.OriginalValues)
                                                 ?? AddSidecar(_metadataServices.CreateOriginalValues(this));

        public virtual Sidecar RelationshipsSnapshot => TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot)
                                                        ?? AddSidecar(_metadataServices.CreateRelationshipSnapshot(this));

        public virtual Sidecar AddSidecar([NotNull] Sidecar sidecar)
        {
            Check.NotNull(sidecar, "sidecar");

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

        public virtual Sidecar TryGetSidecar([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            return _sidecars?.FirstOrDefault(s => s.Name == name);
        }

        public virtual void RemoveSidecar([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

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

        public virtual void SetEntityState(EntityState entityState)
        {
            Check.IsDefined(entityState, "entityState");

            var oldState = _stateData.EntityState;

            if (PrepareForAdd(entityState))
            {
                StateManager.ValueGeneration.Generate(this);
            }

            SetEntityState(oldState, entityState);
        }

        public virtual async Task SetEntityStateAsync(
            EntityState entityState, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.IsDefined(entityState, "entityState");

            var oldState = _stateData.EntityState;

            if (PrepareForAdd(entityState))
            {
                await StateManager.ValueGeneration.GenerateAsync(this, cancellationToken).WithCurrentCulture();
            }

            SetEntityState(oldState, entityState);
        }

        private bool PrepareForAdd(EntityState newState)
        {
            if (newState != EntityState.Added
                || EntityState == EntityState.Added)
            {
                return false;
            }

            // Temporarily change the internal state to unknown so that key generation, including setting key values
            // can happen without constraints on changing read-only values kicking in
            _stateData.EntityState = EntityState.Unknown;

            _stateData.FlagAllProperties(EntityType.Properties.Count(), isFlagged: false);

            return true;
        }

        private void SetEntityState(EntityState oldState, EntityState newState)
        {
            // Prevent temp values from becoming permanent values
            if (oldState == EntityState.Added
                && newState != EntityState.Added
                && newState != EntityState.Unknown)
            {
                var hasTempValue = EntityType.Properties.FirstOrDefault(p => _stateData.IsPropertyFlagged(p.Index));
                if (hasTempValue != null)
                {
                    throw new InvalidOperationException(Strings.TempValuePersists(hasTempValue.Name, EntityType.SimpleName, newState));
                }
            }

            // The entity state can be Modified even if some properties are not modified so always
            // set all properties to modified if the entity state is explicitly set to Modified.
            if (newState == EntityState.Modified)
            {
                _stateData.FlagAllProperties(EntityType.Properties.Count(), isFlagged: true);

                foreach (var keyProperty in EntityType.Properties.Where(
                    p => p.IsReadOnly
                         || p.IsStoreComputed))
                {
                    _stateData.FlagProperty(keyProperty.Index, isFlagged: false);
                }
            }

            if (oldState == newState)
            {
                return;
            }

            if (newState == EntityState.Unchanged)
            {
                _stateData.FlagAllProperties(EntityType.Properties.Count(), isFlagged: false);
            }

            StateManager.Notify.StateChanging(this, newState);

            _stateData.EntityState = newState;

            if (oldState == EntityState.Unknown)
            {
                StateManager.StartTracking(this);
            }
            else if (newState == EntityState.Unknown)
            {
                if (oldState == EntityState.Added)
                {
                    foreach (var property in EntityType.Properties.Where(p => _stateData.IsPropertyFlagged(p.Index)))
                    {
                        this[property] = property.PropertyType.GetDefaultValue();
                    }
                }
                _stateData.FlagAllProperties(EntityType.Properties.Count(), isFlagged: false);

                // TODO: Does changing to Unknown really mean stop tracking?
                // Issue #323
                StateManager.StopTracking(this);
            }

            StateManager.Notify.StateChanged(this, oldState);
        }

        public virtual EntityState EntityState => _stateData.EntityState;

        public virtual bool IsPropertyModified([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            if (_stateData.EntityState != EntityState.Modified)
            {
                return false;
            }

            return _stateData.IsPropertyFlagged(property.Index);
        }

        public virtual void SetPropertyModified([NotNull] IProperty property, bool isModified = true)
        {
            Check.NotNull(property, "property");

            // TODO: Restore original value to reject changes when isModified is false
            // Issue #742

            var currentState = _stateData.EntityState;

            if (currentState == EntityState.Added || currentState == EntityState.Unknown)
            {
                MarkAsTemporary(property, isTemporary: false);
                OriginalValues.TakeSnapshot(property);
            }

            if ((currentState != EntityState.Modified
                 && currentState != EntityState.Unchanged)
                // TODO: Consider allowing computed properties to be forcibly marked as modified
                // Issue #711
                || property.IsStoreComputed)
            {
                return;
            }

            if (isModified && property.IsReadOnly)
            {
                throw new NotSupportedException(Strings.PropertyReadOnly(property.Name, EntityType.Name));
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
        {
            Check.NotNull(property, "property");

            if (_stateData.EntityState != EntityState.Added
                && _stateData.EntityState != EntityState.Unknown)
            {
                return false;
            }

            return _stateData.IsPropertyFlagged(property.Index);
        }

        public virtual void MarkAsTemporary([NotNull] IProperty property, bool isTemporary = true)
        {
            Check.NotNull(property, "property");

            if (_stateData.EntityState != EntityState.Added
                && _stateData.EntityState != EntityState.Unknown)
            {
                return;
            }

            _stateData.FlagProperty(property.Index, isTemporary);
        }

        protected virtual object ReadPropertyValue([NotNull] IPropertyBase propertyBase)
        {
            Check.NotNull(propertyBase, "propertyBase");

            Debug.Assert(!(propertyBase is IProperty) || !((IProperty)propertyBase).IsShadowProperty);

            return _metadataServices.ReadValue(Entity, propertyBase);
        }

        protected virtual void WritePropertyValue([NotNull] IPropertyBase propertyBase, [CanBeNull] object value)
        {
            Check.NotNull(propertyBase, "propertyBase");

            Debug.Assert(!(propertyBase is IProperty) || !((IProperty)propertyBase).IsShadowProperty);

            _metadataServices.WriteValue(Entity, propertyBase, value);
        }

        public virtual object this[IPropertyBase property]
        {
            get
            {
                Check.NotNull(property, "property");

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
                Check.NotNull(property, "property");

                if (_stateData.TransparentSidecarInUse)
                {
                    var wrote = false;
                    foreach (var sidecar in _sidecars)
                    {
                        if (sidecar.TransparentWrite
                            && sidecar.CanStoreValue(property))
                        {
                            StateManager.Notify.SidecarPropertyChanging(this, property);

                            sidecar[property] = value;
                            wrote = true;

                            StateManager.Notify.SidecarPropertyChanged(this, property);
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
            [NotNull] IPropertyBagEntry propertyBagEntry)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotEmpty(properties, "properties");
            Check.NotNull(propertyBagEntry, "propertyBagEntry");

            return _metadataServices.CreateKey(entityType, properties, propertyBagEntry);
        }

        public virtual EntityKey GetDependentKeySnapshot([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return CreateKey(foreignKey.ReferencedEntityType, foreignKey.Properties, RelationshipsSnapshot);
        }

        public virtual EntityKey GetPrincipalKey([NotNull] IForeignKey foreignKey, [NotNull] IEntityType referencedEntityType, [NotNull] IReadOnlyList<IProperty> referencedProperties)
        {
            Check.NotNull(foreignKey, "foreignKey");
            Check.NotNull(referencedEntityType, "referencedEntityType");
            Check.NotNull(referencedProperties, "referencedProperties");

            EntityKey result;
            if (!_principalKeys.TryGetValue(foreignKey, out result))
            {
                _principalKeys.Add(foreignKey,
                    result = CreateKey(referencedEntityType, referencedProperties, this));
            }

            return result;
        }

        public virtual object[] GetValueBuffer()
        {
            return EntityType.Properties.Select(p => this[p]).ToArray();
        }

        public virtual void AcceptChanges()
        {
            var currentState = EntityState;
            if (currentState == EntityState.Unchanged
                || currentState == EntityState.Unknown)
            {
                return;
            }

            if (currentState == EntityState.Added
                || currentState == EntityState.Modified)
            {
                TryGetSidecar(Sidecar.WellKnownNames.OriginalValues)?.UpdateSnapshot();

                SetEntityState(EntityState.Unchanged);
            }
            else if (currentState == EntityState.Deleted)
            {
                SetEntityState(EntityState.Unknown);
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

        public virtual StateEntry PrepareToSave()
        {
            // TODO: Issue #1303
            //if (EntityType.Properties.Any(NeedsStoreValue))

            AddSidecar(_metadataServices.CreateStoreGeneratedValues(this));

            return this;
        }

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

        public virtual bool NeedsStoreValue([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return HasTemporaryValue(property)
                   || (property.UseStoreDefault && this.HasDefaultValue(property))
                   || (property.IsStoreComputed
                       && (EntityState == EntityState.Modified || EntityState == EntityState.Added)
                       && !IsPropertyModified(property));
        }

        public virtual bool IsKeySet => !EntityType.GetPrimaryKey().Properties.Any(this.HasDefaultValue);

        [UsedImplicitly]
        private string DebuggerDisplay => this.GetPrimaryKeyValue() + " - " + EntityState;

        StateEntry IPropertyBagEntry.StateEntry => this;
    }
}
