// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    // This is lower-level change tracking services used by the ChangeTracker and other parts of the system
    public class StateManager
    {
        private readonly Dictionary<object, StateEntry> _entityReferenceMap
            = new Dictionary<object, StateEntry>(ReferenceEqualityComparer.Instance);

        private readonly Dictionary<EntityKey, StateEntry> _identityMap = new Dictionary<EntityKey, StateEntry>();
        private readonly EntityKeyFactorySource _keyFactorySource;
        private readonly StateEntryFactory _factory;
        private readonly StateEntrySubscriber _subscriber;
        private readonly ContextConfiguration _configuration;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected StateManager()
        {
        }

        public StateManager(
            [NotNull] ContextConfiguration configuration,
            [NotNull] StateEntryFactory factory,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] StateEntrySubscriber subscriber)
        {
            Check.NotNull(entityKeyFactorySource, "entityKeyFactorySource");
            Check.NotNull(factory, "factory");
            Check.NotNull(entityKeyFactorySource, "entityKeyFactorySource");

            _configuration = configuration;
            _keyFactorySource = entityKeyFactorySource;
            _factory = factory;
            _subscriber = subscriber;
        }

        public virtual StateEntry CreateNewEntry([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            // TODO: Consider entities without parameterless constructor--use o/c mapping info?
            var entity = entityType.HasClrType ? Activator.CreateInstance(entityType.Type) : null;

            return _subscriber.SnapshotAndSubscribe(_factory.Create(entityType, entity));
        }

        public virtual StateEntry GetOrCreateEntry([NotNull] object entity)
        {
            Check.NotNull(entity, "entity");

            // TODO: Consider how to handle derived types that are not explicitly in the model

            StateEntry stateEntry;
            if (!_entityReferenceMap.TryGetValue(entity, out stateEntry))
            {
                stateEntry = _subscriber.SnapshotAndSubscribe(_factory.Create(Model.GetEntityType(entity.GetType()), entity));
                _entityReferenceMap[entity] = stateEntry;
            }
            return stateEntry;
        }

        public virtual StateEntry GetOrMaterializeEntry([NotNull] IEntityType entityType, [NotNull] IValueReader valueReader)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(valueReader, "valueReader");

            // TODO: Pre-compute this for speed
            var keyProperties = entityType.GetKey().Properties;
            var keyValue = _keyFactorySource.GetKeyFactory(keyProperties).Create(entityType, keyProperties, valueReader);

            StateEntry existingEntry;
            if (_identityMap.TryGetValue(keyValue, out existingEntry))
            {
                return existingEntry;
            }

            var newEntry = _subscriber.SnapshotAndSubscribe(_factory.Create(entityType, valueReader));

            _identityMap.Add(keyValue, newEntry);

            var entity = newEntry.Entity;
            if (entity != null)
            {
                _entityReferenceMap[entity] = newEntry;
            }

            newEntry.EntityState = EntityState.Unchanged;

            return newEntry;
        }

        public virtual IEnumerable<StateEntry> StateEntries
        {
            get { return _identityMap.Values; }
        }

        public virtual bool DetectChanges()
        {
            var foundChanges = false;
            foreach (var entry in _identityMap.Values)
            {
                if (entry.DetectChanges())
                {
                    foundChanges = true;
                }
            }

            return foundChanges;
        }

        public virtual void StartTracking([NotNull] StateEntry entry)
        {
            Check.NotNull(entry, "entry");

            var entityType = entry.EntityType;

            if (entry.Configuration.Services.StateManager != this)
            {
                throw new InvalidOperationException(Strings.FormatWrongStateManager(entityType.Name));
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
                    throw new InvalidOperationException(Strings.FormatMultipleStateEntries(entityType.Name));
                }
            }

            var keyValue = CreateKey(entityType, entityType.GetKey().Properties, entry);

            if (_identityMap.TryGetValue(keyValue, out existingEntry))
            {
                if (existingEntry != entry)
                {
                    // TODO: Consider a hook for identity resolution
                    // TODO: Consider specialized exception types
                    throw new InvalidOperationException(Strings.FormatIdentityConflict(entityType.Name));
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

            var entityType = entry.EntityType;

            var keyValue = CreateKey(entityType, entityType.GetKey().Properties, entry);

            StateEntry existingEntry;
            if (_identityMap.TryGetValue(keyValue, out existingEntry)
                && existingEntry == entry)
            {
                _identityMap.Remove(keyValue);
            }
        }

        public virtual IModel Model
        {
            get { return _configuration.Model; }
        }

        public virtual StateEntry GetPrincipal([NotNull] StateEntry dependentEntry, [NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(dependentEntry, "dependentEntry");
            Check.NotNull(foreignKey, "foreignKey");

            var dependentKeyValue = dependentEntry.GetDependentKeyValue(foreignKey);

            // TODO: Add additional indexes so that this isn't a linear lookup
            var principals = StateEntries.Where(
                e => e.EntityType == foreignKey.ReferencedEntityType
                     && dependentKeyValue.Equals(e.GetPrincipalKeyValue(foreignKey))).ToArray();

            if (principals.Length > 1)
            {
                // TODO: Better exception message
                throw new InvalidOperationException("Multiple matching principals.");
            }

            return principals.FirstOrDefault();
        }

        public virtual IEnumerable<StateEntry> GetDependents([NotNull] StateEntry principalEntry, [NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(principalEntry, "principalEntry");
            Check.NotNull(foreignKey, "foreignKey");

            var principalKeyValue = principalEntry.GetPrincipalKeyValue(foreignKey);

            // TODO: Add additional indexes so that this isn't a linear lookup
            return StateEntries.Where(
                e => e.EntityType == foreignKey.EntityType
                     && principalKeyValue.Equals(e.GetDependentKeyValue(foreignKey)));
        }

        private EntityKey CreateKey(IEntityType entityType, IReadOnlyList<IProperty> properties, StateEntry entry)
        {
            return _keyFactorySource.GetKeyFactory(properties).Create(entityType, properties, entry);
        }

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var entriesToSave = StateEntries
                .Where(e => e.EntityState.IsDirty())
                .Select(e => e.PrepareToSave())
                .ToList();

            if (!entriesToSave.Any())
            {
                return 0;
            }

            try
            {
                var result = await _configuration.DataStore
                    .SaveChangesAsync(entriesToSave, Model, cancellationToken)
                    .ConfigureAwait(false);

                // TODO: When transactions supported, make it possible to commit/accept at end of all transactions
                foreach (var entry in entriesToSave)
                {
                    entry.AutoCommitSidecars();
                    entry.AcceptChanges();
                }

                return result;
            }
            catch
            {
                foreach (var entry in entriesToSave)
                {
                    entry.AutoRollbackSidecars();
                }
                throw;
            }
        }
    }
}
