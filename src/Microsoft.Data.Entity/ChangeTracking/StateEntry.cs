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
            else if (!isModified && !_stateData.AnyPropertiesModified())
            {
                _stateManager.StateChanging(this, EntityState.Unchanged);
                _stateData.EntityState = EntityState.Unchanged;
                _stateManager.StateChanged(this, currentState);
            }
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
    }
}
