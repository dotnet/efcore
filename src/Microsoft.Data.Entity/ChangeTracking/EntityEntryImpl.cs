// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq;

namespace Microsoft.Data.Entity.ChangeTracking
{
    internal class EntityEntryImpl
    {
        private readonly ChangeTracker _changeTracker;
        private readonly object _entity;
        private EntityState _entityState = EntityState.Unknown;
        private readonly BitArray _propertyStates;

        public EntityEntryImpl(ChangeTracker changeTracker, object entity)
        {
            _changeTracker = changeTracker;
            _entity = entity;

            // TODO: Possible perf--avoid counting properties here or even create lazily
            _propertyStates = new BitArray(_changeTracker.Model.Entity(_entity).Properties.Count());
        }

        public virtual object Entity
        {
            get { return _entity; }
        }

        public virtual EntityKey Key
        {
            get { return _changeTracker.Model.Entity(_entity).CreateKey(_entity); }
        }

        public virtual EntityState EntityState
        {
            get { return _entityState; }
            set
            {
                if (_entityState == EntityState.Unknown
                    && value != EntityState.Unknown)
                {
                    _changeTracker.Track(this);
                }
                else if (_entityState != EntityState.Unknown
                         && value == EntityState.Unknown)
                {
                    _changeTracker.Detach(this);
                }

                if (value == EntityState.Modified
                    || value == EntityState.Unchanged)
                {
                    // TODO: Avoid setting keys/readonly properties to modified
                    _propertyStates.SetAll(value == EntityState.Modified);
                }

                _entityState = value;
            }
        }

        public virtual bool IsPropertyModified(string propertyName)
        {
            return _entityState == EntityState.Modified && _propertyStates[GetPropertyIndex(propertyName)];
        }

        public virtual void SetPropertyModified(string propertyName, bool isModified)
        {
            // TODO: Restore original value to reject changes when isModified is false
            _propertyStates[GetPropertyIndex(propertyName)] = isModified;

            // Don't change entity state if it is Added or Deleted
            if (isModified && _entityState == EntityState.Unchanged)
            {
                _entityState = EntityState.Modified;
            }
            else if (!isModified
                     && _propertyStates.OfType<bool>().All(s => !s))
            {
                _entityState = EntityState.Unchanged;
            }
        }

        private int GetPropertyIndex(string property)
        {
            // TODO: Possible perf--make it faster to find the index for a property
            var index = 0;
            var enumerator = _changeTracker.Model.Entity(_entity).Properties.GetEnumerator();
            while (enumerator.MoveNext()
                   && enumerator.Current.Name != property)
            {
                index++;
            }

            if (index >= _propertyStates.Length)
            {
                throw new InvalidOperationException("Bad property name");
            }
            return index;
        }
    }
}
