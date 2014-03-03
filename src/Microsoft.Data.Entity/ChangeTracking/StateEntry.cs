// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class StateEntry
    {
        private readonly StateManager _stateManager;
        private readonly object _entity;
        private StateData _stateData;

        // Intended only for creation of test doubles
        internal StateEntry()
        {
        }

        public StateEntry([NotNull] StateManager stateManager, [NotNull] object entity)
        {
            Check.NotNull(stateManager, "stateManager");
            Check.NotNull(entity, "entity");

            _stateManager = stateManager;
            _entity = entity;
            _stateData = new StateData(EntityType.Properties.Count());
        }

        public virtual object Entity
        {
            get { return _entity; }
        }

        public virtual IEntityType EntityType
        {
            get { return _stateManager.Model.GetEntityType(_entity.GetType()); }
        }

        public virtual StateManager StateManager
        {
            get { return _stateManager; }
        }

        public virtual async Task SetEntityStateAsync(EntityState value, CancellationToken cancellationToken)
        {
            // The entity state can be Modified even if some properties are not modified so always
            // set all properties to modified if the entity state is explicitly set to Modified.
            if (value == EntityState.Modified)
            {
                _stateData.SetAllPropertiesModified(EntityType.Properties.Count());
            }

            var oldState = _stateData.EntityState;
            if (oldState == value)
            {
                return;
            }

            _stateManager.StateChanging(this, value);
            _stateData.EntityState = value;

            if (value == EntityState.Added)
            {
                Debug.Assert(EntityType.Key.Count() == 1, "Composite keys not implemented yet.");

                var keyProperty = EntityType.Key.First();
                var identityGenerator = _stateManager.GetIdentityGenerator(keyProperty);

                if (identityGenerator != null)
                {
                    keyProperty.SetValue(_entity, await identityGenerator.NextAsync(cancellationToken).ConfigureAwait(false));
                }
            }

            if (oldState == EntityState.Unknown)
            {
                _stateManager.StartTracking(this);
            }
            else if (value == EntityState.Unknown)
            {
                // TODO: Does changing to Unknown really mean stop tracking?
                _stateManager.StopTracking(this);
            }

            _stateManager.StateChanged(this, oldState);
        }

        public virtual EntityState EntityState
        {
            get { return _stateData.EntityState; }
        }

        public virtual bool IsPropertyModified([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, "propertyName");

            if (_stateData.EntityState != EntityState.Modified)
            {
                return false;
            }

            return _stateData.IsPropertyModified(GetPropertyIndex(propertyName));
        }

        public virtual void SetPropertyModified([NotNull] string propertyName, bool isModified)
        {
            Check.NotEmpty(propertyName, "propertyName");

            // TODO: Restore original value to reject changes when isModified is false

            _stateData.SetPropertyModified(GetPropertyIndex(propertyName), isModified);

            // Don't change entity state if it is Added or Deleted
            var currentState = _stateData.EntityState;
            if (isModified && currentState == EntityState.Unchanged)
            {
                _stateManager.StateChanging(this, EntityState.Modified);
                _stateData.EntityState = EntityState.Modified;
                _stateManager.StateChanged(this, currentState);
            }
            else if (!isModified
                     && !_stateData.AnyPropertiesModified())
            {
                _stateManager.StateChanging(this, EntityState.Unchanged);
                _stateData.EntityState = EntityState.Unchanged;
                _stateManager.StateChanged(this, currentState);
            }
        }

        public virtual object[] GetValueBuffer()

        {
            return _stateManager.Model
                .GetEntityType(_entity.GetType())
                .Properties.Select(p => p.GetValue(_entity)).ToArray();
        }

        private int GetPropertyIndex(string propertyName)
        {
            var index = EntityType.PropertyIndex(propertyName);

            if (index >= 0)
            {
                return index;
            }

            // TODO: Proper message
            throw new InvalidOperationException("Bad property name");
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
