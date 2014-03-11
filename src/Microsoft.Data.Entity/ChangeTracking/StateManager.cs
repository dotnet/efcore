// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly Dictionary<object, StateEntry> _entityReferenceMap
            = new Dictionary<object, StateEntry>(ReferenceEqualityComparer.Instance);

        private readonly Dictionary<EntityKey, StateEntry> _identityMap = new Dictionary<EntityKey, StateEntry>();
        private readonly IEntityStateListener[] _entityStateListeners;
        private readonly EntityKeyFactorySource _keyFactorySource;
        private readonly StateEntryFactory _stateEntryFactory;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected StateManager()
        {
        }

        public StateManager(
            [NotNull] IModel model,
            [NotNull] ActiveIdentityGenerators identityGenerators,
            [NotNull] IEnumerable<IEntityStateListener> entityStateListeners,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] StateEntryFactory stateEntryFactory)
        {
            Check.NotNull(model, "model");
            Check.NotNull(identityGenerators, "identityGenerators");
            Check.NotNull(entityStateListeners, "entityStateListeners");
            Check.NotNull(entityKeyFactorySource, "entityKeyFactorySource");
            Check.NotNull(stateEntryFactory, "stateEntryFactory");

            _model = model;
            _identityGenerators = identityGenerators;
            _keyFactorySource = entityKeyFactorySource;
            _stateEntryFactory = stateEntryFactory;

            var stateListeners = entityStateListeners.ToArray();
            _entityStateListeners = stateListeners.Length == 0 ? null : stateListeners;
        }

        public virtual StateEntry CreateNewEntry([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            // TODO: Consider entities without parameterless constructor--use o/c mapping info?
            var entity = entityType.HasClrType ? Activator.CreateInstance(entityType.Type) : null;

            return _stateEntryFactory.Create(this, entityType, entity);
        }

        public virtual StateEntry GetOrCreateEntry([NotNull] object entity)
        {
            Check.NotNull(entity, "entity");

            // TODO: Consider how to handle derived types that are not explicitly in the model

            StateEntry stateEntry;
            if (!_entityReferenceMap.TryGetValue(entity, out stateEntry))
            {
                stateEntry = _stateEntryFactory.Create(this, Model.GetEntityType(entity.GetType()), entity);
                _entityReferenceMap[entity] = stateEntry;
            }
            return stateEntry;
        }

        public virtual IEnumerable<StateEntry> StateEntries
        {
            get { return _identityMap.Values; }
        }

        public virtual void StartTracking([NotNull] StateEntry entry)
        {
            Check.NotNull(entry, "entry");

            if (entry.StateManager != this)
            {
                throw new InvalidOperationException(Strings.FormatWrongStateManager(entry.EntityType.Name));
            }

            StateEntry existingEntry;
            if (entry.Entity != null)
            {
                if (!_entityReferenceMap.TryGetValue(entry.Entity, out existingEntry))
                {
                    _entityReferenceMap[entry.Entity] = entry;
                }
                else if (existingEntry != entry)
                {
                    throw new InvalidOperationException(Strings.FormatMultipleStateEntries(entry.EntityType.Name));
                }
            }

            var keyValue = GetKeyFactory(entry.EntityType).Create(entry);

            if (_identityMap.TryGetValue(keyValue, out existingEntry))
            {
                if (existingEntry != entry)
                {
                    // TODO: Consider a hook for identity resolution
                    // TODO: Consider specialized exception types
                    throw new InvalidOperationException(Strings.FormatIdentityConflict(entry.EntityType.Name));
                }
            }
            else
            {
                _identityMap[keyValue] = entry;
            }
        }

        public virtual void StopTracking([NotNull] StateEntry entry)
        {
            Check.NotNull(entry, "entry");

            if (entry.Entity != null)
            {
                _entityReferenceMap.Remove(entry.Entity);
            }

            var keyValue = GetKeyFactory(entry.EntityType).Create(entry);

            StateEntry existingEntry;
            if (_identityMap.TryGetValue(keyValue, out existingEntry)
                && existingEntry == entry)
            {
                _identityMap.Remove(keyValue);
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

            var dependentKeyValue = foreignKey.DependentProperties.Select(dependentEntry.GetPropertyValue).ToArray();

            // TODO: Add additional indexes so that this isn't a linear lookup
            var principals = StateEntries.Where(
                e => e.EntityType == foreignKey.PrincipalType
                     && dependentKeyValue.SequenceEqual(foreignKey.PrincipalProperties.Select(e.GetPropertyValue))).ToArray();

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

            var principalKeyValue = foreignKey.PrincipalProperties.Select(principalEntry.GetPropertyValue).ToArray();

            // TODO: Add additional indexes so that this isn't a linear lookup
            return StateEntries.Where(
                e => e.EntityType == dependentType
                     && principalKeyValue.SequenceEqual(foreignKey.DependentProperties.Select(e.GetPropertyValue)));
        }

        public virtual EntityKeyFactory GetKeyFactory([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return _keyFactorySource.GetKeyFactory(entityType);
        }
    }
}
