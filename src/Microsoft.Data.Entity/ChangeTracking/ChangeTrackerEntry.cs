// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class ChangeTrackerEntry
    {
        private const int BitsPerInt = 32;
        private const int BitsForEntityState = 3;
        private const int EntityStateMask = 0x07;

        private readonly ChangeTracker _changeTracker;
        private readonly object _entity;
        private readonly int[] _propertyStates;

        public ChangeTrackerEntry([NotNull] ChangeTracker changeTracker, [NotNull] object entity)
        {
            Check.NotNull(changeTracker, "changeTracker");
            Check.NotNull(entity, "entity");

            _changeTracker = changeTracker;
            _entity = entity;

            var entityType = _changeTracker.Model.GetEntityType(_entity.GetType());

            _propertyStates = new int[(entityType.Properties.Count() + BitsForEntityState - 1) / BitsPerInt + 1];
        }

        public virtual object Entity
        {
            get { return _entity; }
        }

        public virtual async Task SetEntityStateAsync(EntityState value, CancellationToken cancellationToken)
        {
            var oldState = EntityState;
            EntityState = value;

            // The entity state can be Modified even if some properties are not modified so always
            // set all properties to modified if the entity state is explicitly set to Modified.
            if (value == EntityState.Modified)
            {
                var propertyCount = _changeTracker.Model.GetEntityType(_entity.GetType()).Properties.Count();
                for (var i = 0; i < _propertyStates.Length; i++)
                {
                    _propertyStates[i] |= CreateMask(i, propertyCount);
                }
            }

            if (oldState == value)
            {
                return;
            }

            if (value == EntityState.Added)
            {
                var entityType = _changeTracker.Model.GetEntityType(_entity.GetType());
                Debug.Assert(entityType.Key.Count() == 1, "Composite keys not implemented yet.");

                var keyProperty = entityType.Key.First();
                var identityGenerator = _changeTracker.GetIdentityGenerator(keyProperty);

                if (identityGenerator != null)
                {
                    keyProperty.SetValue(_entity, await identityGenerator.NextAsync(cancellationToken).ConfigureAwait(false));
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

        public virtual EntityState EntityState
        {
            get { return (EntityState)(_propertyStates[0] & EntityStateMask); }
            private set { _propertyStates[0] = (_propertyStates[0] & ~EntityStateMask) | (int)value; }
        }

        public virtual bool IsPropertyModified([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, "propertyName");

            if (EntityState != EntityState.Modified)
            {
                return false;
            }

            var index = GetPropertyIndex(propertyName);
            return (_propertyStates[index / BitsPerInt] & (1 << index % BitsPerInt)) != 0;
        }

        public virtual void SetPropertyModified([NotNull] string propertyName, bool isModified)
        {
            Check.NotEmpty(propertyName, "propertyName");

            // TODO: Restore original value to reject changes when isModified is false

            var index = GetPropertyIndex(propertyName);
            if (isModified)
            {
                _propertyStates[index / BitsPerInt] |= 1 << index % BitsPerInt;
            }
            else
            {
                _propertyStates[index / BitsPerInt] &= ~(1 << index % BitsPerInt);
            }

            // Don't change entity state if it is Added or Deleted
            if (isModified && EntityState == EntityState.Unchanged)
            {
                EntityState = EntityState.Modified;
            }
            else if (!isModified
                     && !_propertyStates.Where((t, i) => (t & CreateMask(i)) != 0).Any())
            {
                EntityState = EntityState.Unchanged;
            }
        }

        public virtual object[] GetValueBuffer()
        {
            return _changeTracker.Model
                .GetEntityType(_entity.GetType())
                .Properties.Select(p => p.GetValue(_entity)).ToArray();
        }

        private static int CreateMask(int i)
        {
            var mask = unchecked(((int)0xFFFFFFFF));
            if (i == 0)
            {
                mask &= ~EntityStateMask;
            }

            // TODO: Remove keys/readonly indexes from the mask to avoid setting them to modified
            return mask;
        }

        private int CreateMask(int i, int propertyCount)
        {
            var mask = CreateMask(i);

            if (i == _propertyStates.Length - 1)
            {
                var overlay = unchecked(((int)0xFFFFFFFF));
                var shift = (propertyCount + BitsForEntityState) % BitsPerInt;
                overlay = shift != 0 ? overlay << shift : 0;
                mask &= ~overlay;
            }

            // TODO: Remove keys/readonly indexes from the mask to avoid setting them to modified
            return mask;
        }

        private int GetPropertyIndex(string propertyName)
        {
            var index = _changeTracker.Model.GetEntityType(_entity.GetType()).PropertyIndex(propertyName);

            if (index < 0)
            {
                // TODO: Proper message
                throw new InvalidOperationException("Bad property name");
            }

            return index + BitsForEntityState;
        }
    }
}
