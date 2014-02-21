// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Entity.ChangeTracking
{
    internal class ChangeTrackerEntry
    {
        private readonly ChangeTracker _changeTracker;
        private readonly object _entity;
        private EntityState _entityState = EntityState.Unknown;
        private readonly BitArray _propertyStates;

        public ChangeTrackerEntry(ChangeTracker changeTracker, object entity)
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
            get { return _changeTracker.Model.Entity(_entity).CreateEntityKey(_entity); }
        }

        public async virtual Task SetEntityStateAsync(EntityState value, CancellationToken cancellationToken)
        {
            var oldState = _entityState;
            _entityState = value;

            // The entity state can be Modified even if some properties are not modified so always
            // set all properties to modified if the entity state is explicitly set to Modified.
            if (value == EntityState.Modified)
            {
                // TODO: Avoid setting keys/readonly properties to modified
                _propertyStates.SetAll(true);
            }

            if (oldState == value)
            {
                return;
            }

            if (value == EntityState.Added)
            {
                var entityType = _changeTracker.Model.Entity(_entity);
                Debug.Assert(entityType.Key.Count() == 1, "Composite keys not implemented yet.");

                var keyProperty = entityType.Key.First();
                var identityGenerator = _changeTracker.GetIdentityGenerator(keyProperty);

                if (identityGenerator != null)
                {
                    keyProperty.SetValue(_entity, await identityGenerator.NextAsync(cancellationToken));
                }
            }

            if (oldState == EntityState.Unknown)
            {
                _changeTracker.Track(this);
            }
            else if (value == EntityState.Unknown)
            {
                _changeTracker.StopTracking(this);
            }
        }

        public virtual EntityState GetEntityState()
        {
            return _entityState;
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
