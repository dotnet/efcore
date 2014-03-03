// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    // This is lower-level change tracking services used by the ChangeTracker and other parts of the system
    public class StateManager
    {
        private readonly IModel _model;
        private readonly ActiveIdentityGenerators _identityGenerators;
        private readonly Dictionary<object, StateEntry> _identityMap;
        private readonly IEntityStateListener[] _entityStateListeners;

        // Intended only for creation of test doubles
        internal StateManager()
        {
        }

        public StateManager(
            [NotNull] IModel model,
            [NotNull] ActiveIdentityGenerators identityGenerators,
            [NotNull] IEnumerable<IEntityStateListener> entityStateListeners)
        {
            Check.NotNull(model, "model");
            Check.NotNull(identityGenerators, "identityGenerators");
            Check.NotNull(entityStateListeners, "entityStateListeners");

            _model = model;
            _identityGenerators = identityGenerators;
            _identityMap = new Dictionary<object, StateEntry>(_model.EntityEqualityComparer);

            var stateListeners = entityStateListeners.ToArray();
            _entityStateListeners = stateListeners.Length == 0 ? null : stateListeners;
        }

        public virtual StateEntry GetOrCreateEntry([NotNull] object entity)
        {
            Check.NotNull(entity, "entity");

            // TODO: Consider how to handle derived types that are not explicitly in the model

            StateEntry stateEntry;
            return _identityMap.TryGetValue(entity, out stateEntry)
                   && ReferenceEquals(stateEntry.Entity, entity)
                ? stateEntry
                : new StateEntry(this, entity);
        }

        public virtual IEnumerable<StateEntry> StateEntries
        {
            get { return _identityMap.Values; }
        }

        public virtual void StartTracking([NotNull] StateEntry entry)
        {
            Check.NotNull(entry, "entry");

            StateEntry existingEntry;
            if (_identityMap.TryGetValue(entry.Entity, out existingEntry)
                && !ReferenceEquals(entry.Entity, existingEntry.Entity))
            {
                // TODO: Consider a hook for identity resolution
                // TODO: Consider specialized exception types
                throw new InvalidOperationException(Strings.IdentityConflict(entry.Entity.GetType().Name));
            }

            // TODO: Consider the case where two EntityEntry instances both track the same entity instance
            _identityMap[entry.Entity] = entry;

            if (entry.EntityState == EntityState.Unknown)
            {
                entry.SetEntityStateAsync(EntityState.Unchanged, CancellationToken.None).Wait();
            }
        }

        public virtual void StopTracking([NotNull] StateEntry entry)
        {
            Check.NotNull(entry, "entry");

            if (_identityMap.ContainsKey(entry.Entity))
            {
                _identityMap.Remove(entry.Entity);
            }
        }

        public virtual IModel Model
        {
            get { return _model; }
        }

        public virtual IIdentityGenerator GetIdentityGenerator([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return _identityGenerators.GetOrAdd(property);
        }

        internal void StateChanging(StateEntry entry, EntityState newState)
        {
            if (_entityStateListeners == null)
            {
                return;
            }

            foreach (var listener in _entityStateListeners)
            {
                listener.StateChanging(entry, newState);
            }
        }

        internal void StateChanged(StateEntry entry, EntityState oldState)
        {
            if (_entityStateListeners == null)
            {
                return;
            }

            foreach (var listener in _entityStateListeners)
            {
                listener.StateChanged(entry, oldState);
            }
        }

        public virtual StateEntry GetPrincipal([NotNull] StateEntry dependentEntry, [NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(dependentEntry, "dependentEntry");
            Check.NotNull(foreignKey, "foreignKey");

            var dependentKeyValue = foreignKey.Properties.Select(f => f.Dependent.GetValue(dependentEntry.Entity)).ToArray();

            // TODO: Add additional indexes so that this isn't a linear lookup
            var principals = StateEntries.Where(
                e => e.EntityType == foreignKey.PrincipalType
                     && dependentKeyValue.SequenceEqual(foreignKey.Properties.Select(f => f.Principal.GetValue(e.Entity)))).ToArray();

            if (principals.Length > 1)
            {
                // TODO: Better exception message
                throw new InvalidOperationException("Multiple matching principals.");
            }

            return principals.FirstOrDefault();
        }

        public virtual IEnumerable<StateEntry> GetDependents(
            [NotNull] StateEntry principalEntry, [NotNull] IEntityType dependentType, [NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(principalEntry, "principalEntry");
            Check.NotNull(dependentType, "dependentType");
            Check.NotNull(foreignKey, "foreignKey");

            var principalKeyValue = foreignKey.Properties.Select(f => f.Principal.GetValue(principalEntry.Entity)).ToArray();

            // TODO: Add additional indexes so that this isn't a linear lookup
            return StateEntries.Where(
                e => e.EntityType == dependentType
                     && principalKeyValue.SequenceEqual(foreignKey.Properties.Select(f => f.Dependent.GetValue(e.Entity))));
        }
    }
}
