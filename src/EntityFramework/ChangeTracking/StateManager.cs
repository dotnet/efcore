// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
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
        private readonly DbContextConfiguration _configuration;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected StateManager()
        {
        }

        public StateManager(
            [NotNull] DbContextConfiguration configuration,
            [NotNull] StateEntryFactory factory,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] StateEntrySubscriber subscriber)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(factory, "factory");
            Check.NotNull(entityKeyFactorySource, "entityKeyFactorySource");
            Check.NotNull(subscriber, "subscriber");

            _configuration = configuration;
            _keyFactorySource = entityKeyFactorySource;
            _factory = factory;
            _subscriber = subscriber;
        }

        public virtual StateEntry CreateNewEntry([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            // TODO: Consider entities without parameterless constructor--use o/c mapping info?
            // Issue #240
            var entity = entityType.HasClrType ? Activator.CreateInstance(entityType.Type) : null;

            return _subscriber.SnapshotAndSubscribe(_factory.Create(entityType, entity));
        }

        public virtual StateEntry GetOrCreateEntry([NotNull] object entity)
        {
            Check.NotNull(entity, "entity");

            // TODO: Consider how to handle derived types that are not explicitly in the model
            // Issue #743
            StateEntry stateEntry;
            if (!_entityReferenceMap.TryGetValue(entity, out stateEntry))
            {
                var entityType = Model.GetEntityType(entity.GetType());

                stateEntry = _subscriber.SnapshotAndSubscribe(_factory.Create(entityType, entity));
                
                _entityReferenceMap[entity] = stateEntry;
            }
            return stateEntry;
        }

        public virtual StateEntry GetOrMaterializeEntry([NotNull] IEntityType entityType, [NotNull] IValueReader valueReader)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(valueReader, "valueReader");

            // TODO: Perf: Pre-compute this for speed
            var keyProperties = entityType.GetPrimaryKey().Properties;
            var keyValue = _keyFactorySource.GetKeyFactory(keyProperties).Create(entityType, keyProperties, valueReader);

            var existingEntry = TryGetEntry(keyValue);
            if (existingEntry != null)
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

        public virtual StateEntry TryGetEntry([NotNull] EntityKey keyValue)
        {
            Check.NotNull(keyValue, "keyValue");

            StateEntry entry;
            _identityMap.TryGetValue(keyValue, out entry);
            return entry;
        }

        public virtual StateEntry TryGetEntry([NotNull] object entity)
        {
            Check.NotNull(entity, "entity");

            StateEntry entry;
            _entityReferenceMap.TryGetValue(entity, out entry);
            return entry;
        }

        public virtual IEnumerable<StateEntry> StateEntries
        {
            get { return _identityMap.Values; }
        }

        public virtual StateEntry StartTracking([NotNull] StateEntry entry)
        {
            Check.NotNull(entry, "entry");

            var entityType = entry.EntityType;

            if (entry.Configuration.StateManager != this)
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

            var keyValue = GetPrimaryKeyValueChecked(entry);

            if (_identityMap.TryGetValue(keyValue, out existingEntry))
            {
                if (existingEntry != entry)
                {
                    // TODO: Consider specialized exception types
                    // Issue #611
                    throw new InvalidOperationException(Strings.FormatIdentityConflict(entityType.Name));
                }
            }
            else
            {
                _identityMap[keyValue] = entry;
            }

            return entry;
        }

        public virtual void StopTracking([NotNull] StateEntry entry)
        {
            Check.NotNull(entry, "entry");

            if (entry.Entity != null)
            {
                _entityReferenceMap.Remove(entry.Entity);
            }

            var keyValue = entry.GetPrimaryKeyValue();

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

        public virtual StateEntry GetPrincipal([NotNull] IPropertyBagEntry dependentEntry, [NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(dependentEntry, "dependentEntry");
            Check.NotNull(foreignKey, "foreignKey");

            var dependentKeyValue = dependentEntry.GetDependentKeyValue(foreignKey);

            if (dependentKeyValue == EntityKey.NullEntityKey)
            {
                return null;
            }

            // TODO: Perf: Add additional indexes so that this isn't a linear lookup
            var principals = StateEntries.Where(
                e => e.EntityType == foreignKey.ReferencedEntityType
                     && dependentKeyValue.Equals(e.GetPrincipalKeyValue(foreignKey))).ToArray();

            if (principals.Length > 1)
            {
                // TODO: Better exception message
                // Issue #739
                throw new InvalidOperationException("Multiple matching principals.");
            }

            return principals.FirstOrDefault();
        }

        public virtual void UpdateIdentityMap([NotNull] StateEntry entry, [NotNull] EntityKey oldKey)
        {
            Check.NotNull(entry, "entry");
            Check.NotNull(oldKey, "oldKey");

            if (entry.EntityState == EntityState.Unknown)
            {
                return;
            }

            var newKey = GetPrimaryKeyValueChecked(entry);

            if (oldKey.Equals(newKey))
            {
                return;
            }

            StateEntry existingEntry;
            if (_identityMap.TryGetValue(newKey, out existingEntry)
                && existingEntry != entry)
            {
                throw new InvalidOperationException(Strings.FormatIdentityConflict(entry.EntityType.Name));
            }

            _identityMap.Remove(oldKey);
            _identityMap[newKey] = entry;
        }

        private EntityKey GetPrimaryKeyValueChecked(StateEntry entry)
        {
            var keyValue = entry.GetPrimaryKeyValue();

            if (keyValue == EntityKey.NullEntityKey)
            {
                throw new InvalidOperationException(Strings.FormatNullPrimaryKey(entry.EntityType.Name));
            }

            return keyValue;
        }

        public virtual IEnumerable<StateEntry> GetDependents([NotNull] StateEntry principalEntry, [NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(principalEntry, "principalEntry");
            Check.NotNull(foreignKey, "foreignKey");

            var principalKeyValue = principalEntry.GetPrincipalKeyValue(foreignKey);

            // TODO: Perf: Add additional indexes so that this isn't a linear lookup
            return principalKeyValue == EntityKey.NullEntityKey
                ? Enumerable.Empty<StateEntry>()
                : StateEntries.Where(
                    e => e.EntityType == foreignKey.EntityType
                         && principalKeyValue.Equals(e.GetDependentKeyValue(foreignKey)));
        }

        public virtual int SaveChanges()
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
                var result = SaveChanges(entriesToSave);

                // TODO: When transactions supported, make it possible to commit/accept at end of all transactions
                // Issue #744
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
                var result
                    = await SaveChangesAsync(entriesToSave, cancellationToken)
                        .WithCurrentCulture();

                // TODO: When transactions supported, make it possible to commit/accept at end of all transactions
                // Issue #744
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

        protected virtual int SaveChanges(
            [NotNull] IReadOnlyList<StateEntry> entriesToSave)
        {
            Check.NotNull(entriesToSave, "entriesToSave");

            return _configuration.DataStore
                .SaveChanges(entriesToSave);
        }

        protected virtual async Task<int> SaveChangesAsync(
            [NotNull] IReadOnlyList<StateEntry> entriesToSave,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(entriesToSave, "entriesToSave");

            return await _configuration.DataStore
                .SaveChangesAsync(entriesToSave, cancellationToken)
                .WithCurrentCulture();
        }
    }
}
