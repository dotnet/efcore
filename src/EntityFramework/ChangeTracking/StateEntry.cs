// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract partial class StateEntry : IPropertyBagEntry
    {
        private readonly DbContextConfiguration _configuration;
        private readonly IEntityType _entityType;
        private StateData _stateData;
        private Sidecar[] _sidecars;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected StateEntry()
        {
        }

        protected StateEntry(
            [NotNull] DbContextConfiguration configuration,
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(entityType, "entityType");

            _configuration = configuration;
            _entityType = entityType;
            _stateData = new StateData(entityType.Properties.Count);
        }

        [CanBeNull]
        public abstract object Entity { get; }

        public virtual IEntityType EntityType
        {
            get { return _entityType; }
        }

        public virtual DbContextConfiguration Configuration
        {
            get { return _configuration; }
        }

        public virtual Sidecar OriginalValues
        {
            get
            {
                return TryGetSidecar(Sidecar.WellKnownNames.OriginalValues)
                       ?? AddSidecar(_configuration.Services.OriginalValuesFactory.Create(this));
            }
        }

        public virtual Sidecar RelationshipsSnapshot
        {
            get
            {
                return TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot)
                       ?? AddSidecar(_configuration.Services.RelationshipsSnapshotFactory.Create(this));
            }
        }

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

            return _sidecars == null
                ? null
                : _sidecars.FirstOrDefault(s => s.Name == name);
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

        private void SetEntityState(EntityState entityState)
        {
            var oldState = _stateData.EntityState;
            var valueGenerators = PrepareForAdd(entityState);

            if (valueGenerators != null)
            {
                GenerateValues(valueGenerators);
            }

            SetEntityState(oldState, entityState);
        }

        public virtual async Task SetEntityStateAsync(
            EntityState entityState, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.IsDefined(entityState, "entityState");

            var oldState = _stateData.EntityState;
            var valueGenerators = PrepareForAdd(entityState);

            if (valueGenerators != null)
            {
                await GenerateValuesAsync(valueGenerators, cancellationToken).WithCurrentCulture();
            }

            SetEntityState(oldState, entityState);
        }

        private void GenerateValues(IEnumerable<Tuple<IProperty, IValueGenerator>> generators)
        {
            Contract.Assert(generators != null);

            foreach (var generator in generators)
            {
                var property = generator.Item1;
                generator.Item2.Next(this, property);
            }
        }

        private async Task GenerateValuesAsync(
            IEnumerable<Tuple<IProperty, IValueGenerator>> generators, CancellationToken cancellationToken)
        {
            Contract.Assert(generators != null);

            foreach (var generator in generators)
            {
                var property = generator.Item1;
                await generator.Item2.NextAsync(this, property, cancellationToken)
                    .WithCurrentCulture();
            }
        }

        private IEnumerable<Tuple<IProperty, IValueGenerator>> PrepareForAdd(EntityState newState)
        {
            if (newState != EntityState.Added
                || EntityState == EntityState.Added)
            {
                return null;
            }

            // Temporarily change the internal state to unknown so that key generation, including setting key values
            // can happen without constraints on changing read-only values kicking in
            _stateData.EntityState = EntityState.Unknown;

            _stateData.FlagAllProperties(_entityType.Properties.Count(), isFlagged: false);

            var generators = _entityType.Properties
                .Where(p => (p.ValueGeneration == ValueGeneration.OnAdd || p.IsForeignKey()) && HasDefaultValue(p))
                .Select(p => Tuple.Create(p, _configuration.ValueGeneratorCache.GetGenerator(p)))
                .Where(g => g.Item2 != null)
                .ToList();

            // Return null if there are no generators to avoid subsequent async method overhead
            return generators.Count > 0 ? generators : null;
        }

        private bool HasDefaultValue(IProperty p)
        {
            var value = this[p];
            return value == null || value.Equals(p.PropertyType.GetDefaultValue());
        }

        private void SetEntityState(EntityState oldState, EntityState newState)
        {
            // The entity state can be Modified even if some properties are not modified so always
            // set all properties to modified if the entity state is explicitly set to Modified.
            if (newState == EntityState.Modified)
            {
                _stateData.FlagAllProperties(_entityType.Properties.Count(), isFlagged: true);

                foreach (var keyProperty in EntityType.Properties.Where(
                    p => p.IsReadOnly
                         || p.ValueGeneration == ValueGeneration.OnAddAndUpdate))
                {
                    _stateData.FlagProperty(keyProperty.Index, isFlagged: false);
                }
            }

            if (oldState == newState)
            {
                return;
            }

            // An Added entity does not yet exist in the database. If it is then marked as deleted there is
            // nothing to delete because it was not yet inserted, so just make sure it doesn't get inserted.
            if (oldState == EntityState.Added
                && newState == EntityState.Deleted)
            {
                newState = EntityState.Unknown;
            }

            if (newState == EntityState.Unchanged)
            {
                _stateData.FlagAllProperties(_entityType.Properties.Count(), isFlagged: false);
            }

            _configuration.Services.StateEntryNotifier.StateChanging(this, newState);

            _stateData.EntityState = newState;

            if (oldState == EntityState.Unknown)
            {
                _configuration.StateManager.StartTracking(this);
            }
            else if (newState == EntityState.Unknown)
            {
                if (oldState == EntityState.Added)
                {
                    foreach (var property in _entityType.Properties.Where(p => _stateData.IsPropertyFlagged(p.Index)))
                    {
                        this[property] = property.PropertyType.GetDefaultValue();
                    }
                }

                // TODO: Does changing to Unknown really mean stop tracking?
                // Issue #323
                _configuration.StateManager.StopTracking(this);
            }

            _configuration.Services.StateEntryNotifier.StateChanged(this, oldState);
        }

        public virtual EntityState EntityState
        {
            get { return _stateData.EntityState; }
            set
            {
                Check.IsDefined(value, "value");

                SetEntityState(value);
            }
        }

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

            if ((currentState != EntityState.Modified
                 && currentState != EntityState.Unchanged)
                // TODO: Consider allowing computed properties to be forcibly marked as modified
                // Issue #711
                || property.ValueGeneration == ValueGeneration.OnAddAndUpdate)
            {
                return;
            }

            if (isModified && property.IsReadOnly)
            {
                throw new NotSupportedException(Strings.FormatPropertyReadOnly(property.Name, EntityType.Name));
            }

            _stateData.FlagProperty(property.Index, isModified);

            // Don't change entity state if it is Added or Deleted
            if (isModified && currentState == EntityState.Unchanged)
            {
                var notifier = _configuration.Services.StateEntryNotifier;
                notifier.StateChanging(this, EntityState.Modified);
                _stateData.EntityState = EntityState.Modified;
                notifier.StateChanged(this, currentState);
            }
            else if (!isModified
                     && !_stateData.AnyPropertiesFlagged())
            {
                var notifier = _configuration.Services.StateEntryNotifier;
                notifier.StateChanging(this, EntityState.Unchanged);
                _stateData.EntityState = EntityState.Unchanged;
                notifier.StateChanged(this, currentState);
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

            Contract.Assert(!(propertyBase is IProperty) || !((IProperty)propertyBase).IsShadowProperty);

            return _configuration.Services.ClrPropertyGetterSource.GetAccessor(propertyBase).GetClrValue(Entity);
        }

        protected virtual void WritePropertyValue([NotNull] IPropertyBase propertyBase, [CanBeNull] object value)
        {
            Check.NotNull(propertyBase, "propertyBase");

            Contract.Assert(!(propertyBase is IProperty) || !((IProperty)propertyBase).IsShadowProperty);

            _configuration.Services.ClrPropertySetterSource.GetAccessor(propertyBase).SetClrValue(Entity, value);
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
                            var changeDetector = _configuration.Services.ChangeDetector;
                            changeDetector.SidecarPropertyChanging(this, property);

                            sidecar[property] = value;
                            wrote = true;

                            changeDetector.SidecarPropertyChanged(this, property);
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
                    var changeDetector = _configuration.Services.ChangeDetector;

                    changeDetector.PropertyChanging(this, property);

                    WritePropertyValue(property, value);

                    changeDetector.PropertyChanged(this, property);
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

            return _configuration.Services.EntityKeyFactorySource
                .GetKeyFactory(properties)
                .Create(entityType, properties, propertyBagEntry);
        }

        public virtual EntityKey GetDependentKeySnapshot([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return _configuration.Services.EntityKeyFactorySource
                .GetKeyFactory(foreignKey.Properties)
                .Create(foreignKey.ReferencedEntityType, foreignKey.Properties, RelationshipsSnapshot);
        }

        public virtual object[] GetValueBuffer()
        {
            return _entityType.Properties.Select(p => this[p]).ToArray();
        }

        public virtual bool DetectChanges()
        {
            return _configuration.Services.ChangeDetector.DetectChanges(this);
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
                var originalValues = TryGetSidecar(Sidecar.WellKnownNames.OriginalValues);
                if (originalValues != null)
                {
                    originalValues.UpdateSnapshot();
                }

                EntityState = EntityState.Unchanged;
            }
            else if (currentState == EntityState.Deleted)
            {
                EntityState = EntityState.Unknown;
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
            if (_entityType.Properties.Any(NeedsStoreValue))
            {
                AddSidecar(_configuration.Services.StoreGeneratedValuesFactory.Create(this));
            }

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
                   || (property.UseStoreDefault && HasDefaultValue(property))
                   || (property.ValueGeneration == ValueGeneration.OnAddAndUpdate
                       && (EntityState == EntityState.Modified || EntityState == EntityState.Added)
                       && !IsPropertyModified(property));
        }

        [UsedImplicitly]
        private string DebuggerDisplay
        {
            get { return this.GetPrimaryKeyValue() + " - " + EntityState; }
        }

        StateEntry IPropertyBagEntry.StateEntry
        {
            get { return this; }
        }
    }
}
