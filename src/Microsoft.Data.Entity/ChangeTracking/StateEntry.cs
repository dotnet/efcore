// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public abstract class StateEntry
    {
        private readonly ContextConfiguration _configuration;
        private readonly IEntityType _entityType;
        private StateData _stateData;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected StateEntry()
        {
        }

        protected StateEntry([NotNull] ContextConfiguration configuration, [NotNull] IEntityType entityType)
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

        public virtual ContextConfiguration Configuration
        {
            get { return _configuration; }
        }

        public virtual async Task SetEntityStateAsync(
            EntityState value, CancellationToken cancellationToken = default(CancellationToken))
        {
            // The entity state can be Modified even if some properties are not modified so always
            // set all properties to modified if the entity state is explicitly set to Modified.
            if (value == EntityState.Modified)
            {
                _stateData.SetAllPropertiesModified(_entityType.Properties.Count());
            }

            var oldState = _stateData.EntityState;
            if (oldState == value)
            {
                return;
            }

            _configuration.StateEntryNotifier.StateChanging(this, value);

            _stateData.EntityState = value;

            if (value == EntityState.Added)
            {
                var keyProperty = _entityType.GetKey().Properties.Single(); // TODO: Composite keys not implemented yet.
                var identityGenerator = _configuration.ActiveIdentityGenerators.GetOrAdd(keyProperty);

                if (identityGenerator != null)
                {
                    SetPropertyValue(keyProperty, await identityGenerator.NextAsync(cancellationToken).ConfigureAwait(false));
                }
            }

            if (oldState == EntityState.Unknown)
            {
                _configuration.StateManager.StartTracking(this);
            }
            else if (value == EntityState.Unknown)
            {
                // TODO: Does changing to Unknown really mean stop tracking?
                _configuration.StateManager.StopTracking(this);
            }

            _configuration.StateEntryNotifier.StateChanged(this, oldState);
        }

        public virtual EntityState EntityState
        {
            get { return _stateData.EntityState; }
        }

        public virtual bool IsPropertyModified([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            if (_stateData.EntityState != EntityState.Modified)
            {
                return false;
            }

            return _stateData.IsPropertyModified(property.Index);
        }

        public virtual void SetPropertyModified([NotNull] IProperty property, bool isModified)
        {
            Check.NotNull(property, "property");

            // TODO: Restore original value to reject changes when isModified is false

            _stateData.SetPropertyModified(property.Index, isModified);

            // Don't change entity state if it is Added or Deleted
            var currentState = _stateData.EntityState;
            if (isModified && currentState == EntityState.Unchanged)
            {
                var notifier = _configuration.StateEntryNotifier;
                notifier.StateChanging(this, EntityState.Modified);
                _stateData.EntityState = EntityState.Modified;
                notifier.StateChanged(this, currentState);
            }
            else if (!isModified
                     && !_stateData.AnyPropertiesModified())
            {
                var notifier = _configuration.StateEntryNotifier;
                notifier.StateChanging(this, EntityState.Unchanged);
                _stateData.EntityState = EntityState.Unchanged;
                notifier.StateChanged(this, currentState);
            }
        }

        internal virtual void SetAttached()
        {
            var notifier = _configuration.StateEntryNotifier;
            notifier.StateChanging(this, EntityState.Unchanged);
            _stateData.EntityState = EntityState.Unchanged;
            notifier.StateChanged(this, EntityState.Unknown);
        }

        public abstract object GetPropertyValue([NotNull] IProperty property);

        public abstract void SetPropertyValue([NotNull] IProperty property, [CanBeNull] object value);

        public virtual EntityKey GetPrimaryKeyValue()
        {
            return CreateKey(_entityType, _entityType.GetKey().Properties, this);
        }

        public virtual EntityKey GetDependentKeyValue([NotNull] IForeignKey foreignKey)
        {
            return CreateKey(foreignKey.ReferencedEntityType, foreignKey.Properties, this);
        }

        public virtual EntityKey GetPrincipalKeyValue([NotNull] IForeignKey foreignKey)
        {
            return CreateKey(foreignKey.ReferencedEntityType, foreignKey.ReferencedProperties, this);
        }

        private EntityKey CreateKey(IEntityType entityType, IReadOnlyList<IProperty> properties, StateEntry entry)
        {
            return _configuration.EntityKeyFactorySource
                .GetKeyFactory(properties)
                .Create(entityType, properties, entry);
        }

        public virtual object[] GetValueBuffer()
        {
            return _entityType.Properties.Select(GetPropertyValue).ToArray();
        }

        internal struct StateData
        {
            private const int BitsPerInt = 32;
            private const int BitsForEntityState = 3;
            private const int EntityStateMask = 0x07;

            private readonly int[] _bits;

            public StateData(int propertyCount)
            {
                _bits = new int[(propertyCount + BitsForEntityState - 1) / BitsPerInt + 1];
            }

            public void SetAllPropertiesModified(int propertyCount)
            {
                for (var i = 0; i < _bits.Length; i++)
                {
                    _bits[i] |= CreateMaskForWrite(i, propertyCount);
                }
            }

            public EntityState EntityState
            {
                get { return (EntityState)(_bits[0] & EntityStateMask); }
                set { _bits[0] = (_bits[0] & ~EntityStateMask) | (int)value; }
            }

            public bool IsPropertyModified(int propertyIndex)
            {
                propertyIndex += BitsForEntityState;

                return (_bits[propertyIndex / BitsPerInt] & (1 << propertyIndex % BitsPerInt)) != 0;
            }

            public void SetPropertyModified(int propertyIndex, bool isModified)
            {
                propertyIndex += BitsForEntityState;

                if (isModified)
                {
                    _bits[propertyIndex / BitsPerInt] |= 1 << propertyIndex % BitsPerInt;
                }
                else
                {
                    _bits[propertyIndex / BitsPerInt] &= ~(1 << propertyIndex % BitsPerInt);
                }
            }

            public bool AnyPropertiesModified()
            {
                return _bits.Where((t, i) => (t & CreateMaskForRead(i)) != 0).Any();
            }

            private static int CreateMaskForRead(int i)
            {
                var mask = unchecked(((int)0xFFFFFFFF));
                if (i == 0)
                {
                    mask &= ~EntityStateMask;
                }

                // TODO: Remove keys/readonly indexes from the mask to avoid setting them to modified
                return mask;
            }

            private int CreateMaskForWrite(int i, int propertyCount)
            {
                var mask = CreateMaskForRead(i);

                if (i == _bits.Length - 1)
                {
                    var overlay = unchecked(((int)0xFFFFFFFF));
                    var shift = (propertyCount + BitsForEntityState) % BitsPerInt;
                    overlay = shift != 0 ? overlay << shift : 0;
                    mask &= ~overlay;
                }

                // TODO: Remove keys/readonly indexes from the mask to avoid setting them to modified
                return mask;
            }
        }
    }
}
