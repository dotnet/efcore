// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    // This is lower-level change tracking services used by the ChangeTracker and other parts of the system
    public class StateManager : IStateManager
    {
        private readonly Dictionary<object, InternalEntityEntry> _entityReferenceMap
            = new Dictionary<object, InternalEntityEntry>(ReferenceEqualityComparer.Instance);

        private readonly Dictionary<object, WeakReference<InternalEntityEntry>> _detachedEntityReferenceMap
            = new Dictionary<object, WeakReference<InternalEntityEntry>>(ReferenceEqualityComparer.Instance);

        private readonly Dictionary<EntityKey, InternalEntityEntry> _identityMap = new Dictionary<EntityKey, InternalEntityEntry>();
        private readonly IInternalEntityEntryFactory _factory;
        private readonly IInternalEntityEntrySubscriber _subscriber;
        private readonly IModel _model;
        private readonly IDatabase _database;

        public StateManager(
            [NotNull] IInternalEntityEntryFactory factory,
            [NotNull] IInternalEntityEntrySubscriber subscriber,
            [NotNull] IInternalEntityEntryNotifier notifier,
            [NotNull] IValueGenerationManager valueGeneration,
            [NotNull] IModel model,
            [NotNull] IDatabase database)
        {
            _factory = factory;
            _subscriber = subscriber;
            Notify = notifier;
            ValueGeneration = valueGeneration;
            _model = model;
            _database = database;
        }

        public virtual IInternalEntityEntryNotifier Notify { get; }

        public virtual IValueGenerationManager ValueGeneration { get; }

        public virtual InternalEntityEntry CreateNewEntry(IEntityType entityType)
        {
            // TODO: Consider entities without parameterless constructor--use o/c mapping info?
            // Issue #240
            var entity = entityType.HasClrType() ? Activator.CreateInstance(entityType.ClrType) : null;

            return _subscriber.SnapshotAndSubscribe(_factory.Create(this, entityType, entity));
        }

        public virtual InternalEntityEntry GetOrCreateEntry(object entity)
        {
            // TODO: Consider how to handle derived types that are not explicitly in the model
            // Issue #743
            var entry = TryGetEntry(entity);
            if (entry == null)
            {
                if (_detachedEntityReferenceMap.Count % 100 == 99)
                {
                    InternalEntityEntry _;
                    var deadKeys = _detachedEntityReferenceMap
                        .Where(e => !e.Value.TryGetTarget(out _))
                        .Select(e => e.Key)
                        .ToList();

                    foreach (var deadKey in deadKeys)
                    {
                        _detachedEntityReferenceMap.Remove(deadKey);
                    }
                }

                var entityType = _model.GetEntityType(entity.GetType());

                entry = _subscriber.SnapshotAndSubscribe(_factory.Create(this, entityType, entity));

                _detachedEntityReferenceMap[entity] = new WeakReference<InternalEntityEntry>(entry);
            }
            return entry;
        }

        public virtual InternalEntityEntry StartTracking(
            IEntityType entityType, EntityKey entityKey, object entity, ValueBuffer valueBuffer)
        {
            if (entityKey == EntityKey.InvalidEntityKey)
            {
                throw new InvalidOperationException(Strings.InvalidPrimaryKey(entityType.DisplayName()));
            }

            var existingEntry = TryGetEntry(entityKey);
            if (existingEntry != null)
            {
                if (existingEntry.Entity != entity)
                {
                    throw new InvalidOperationException(Strings.IdentityConflict(entityType.DisplayName()));
                }

                return existingEntry;
            }

            var newEntry = _subscriber.SnapshotAndSubscribe(_factory.Create(this, entityType, entity, valueBuffer));

            AddToIdentityMap(entityType, entityKey, newEntry);

            _entityReferenceMap[entity] = newEntry;
            _detachedEntityReferenceMap.Remove(entity);

            newEntry.SetEntityState(EntityState.Unchanged);

            return newEntry;
        }

        private void AddToIdentityMap(IEntityType entityType, EntityKey entityKey, InternalEntityEntry newEntry)
        {
            _identityMap.Add(entityKey, newEntry);
            foreach (var key in entityType.GetKeys().Where(k => k != entityKey.Key))
            {
                _identityMap[newEntry.GetPrincipalKeyValue(key)] = newEntry;
            }
        }

        public virtual InternalEntityEntry TryGetEntry(EntityKey keyValue)
        {
            InternalEntityEntry entry;
            _identityMap.TryGetValue(keyValue, out entry);
            return entry;
        }

        public virtual InternalEntityEntry TryGetEntry(object entity)
        {
            InternalEntityEntry entry;
            if (!_entityReferenceMap.TryGetValue(entity, out entry))
            {
                WeakReference<InternalEntityEntry> detachedEntry;

                if (!_detachedEntityReferenceMap.TryGetValue(entity, out detachedEntry)
                    || !detachedEntry.TryGetTarget(out entry))
                {
                    return null;
                }
            }

            return entry;
        }

        public virtual IEnumerable<InternalEntityEntry> Entries => _entityReferenceMap.Values;

        public virtual InternalEntityEntry StartTracking(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;

            if (entry.StateManager != this)
            {
                throw new InvalidOperationException(Strings.WrongStateManager(entityType.Name));
            }

            var mapKey = entry.Entity ?? entry;
            var existingEntry = TryGetEntry(mapKey);

            if (existingEntry == null || existingEntry == entry)
            {
                _entityReferenceMap[mapKey] = entry;
                _detachedEntityReferenceMap.Remove(mapKey);
            }
            else
            {
                throw new InvalidOperationException(Strings.MultipleEntries(entityType.Name));
            }

            var keyValue = GetPrimaryKeyValueChecked(entry);

            if (_identityMap.TryGetValue(keyValue, out existingEntry))
            {
                if (existingEntry != entry)
                {
                    // TODO: Consider specialized exception types
                    // Issue #611
                    throw new InvalidOperationException(Strings.IdentityConflict(entityType.Name));
                }
            }
            else
            {
                AddToIdentityMap(entityType, keyValue, entry);
            }

            return entry;
        }

        public virtual void StopTracking(InternalEntityEntry entry)
        {
            var mapKey = entry.Entity ?? entry;
            _entityReferenceMap.Remove(mapKey);
            _detachedEntityReferenceMap[mapKey] = new WeakReference<InternalEntityEntry>(entry);

            foreach (var key in entry.EntityType.GetKeys())
            {
                var keyValue = entry.GetPrincipalKeyValue(key);

                InternalEntityEntry existingEntry;
                if (_identityMap.TryGetValue(keyValue, out existingEntry)
                    && existingEntry == entry)
                {
                    _identityMap.Remove(keyValue);
                }
            }
        }

        public virtual InternalEntityEntry GetPrincipal(IPropertyAccessor dependentEntry, IForeignKey foreignKey)
        {
            var dependentKeyValue = dependentEntry.GetDependentKeyValue(foreignKey);
            if (dependentKeyValue == EntityKey.InvalidEntityKey)
            {
                return null;
            }

            InternalEntityEntry principalEntry;
            _identityMap.TryGetValue(dependentKeyValue, out principalEntry);

            return principalEntry;
        }

        public virtual void UpdateIdentityMap(InternalEntityEntry entry, EntityKey oldKey)
        {
            if (entry.EntityState == EntityState.Detached)
            {
                return;
            }

            var newKey = GetPrimaryKeyValueChecked(entry);

            if (oldKey.Equals(newKey))
            {
                return;
            }

            InternalEntityEntry existingEntry;
            if (_identityMap.TryGetValue(newKey, out existingEntry)
                && existingEntry != entry)
            {
                throw new InvalidOperationException(Strings.IdentityConflict(entry.EntityType.Name));
            }

            _identityMap.Remove(oldKey);
            _identityMap[newKey] = entry;
        }

        private EntityKey GetPrimaryKeyValueChecked(InternalEntityEntry entry)
        {
            var keyValue = entry.GetPrimaryKeyValue();

            if (keyValue == EntityKey.InvalidEntityKey)
            {
                // TODO: Check message text here
                throw new InvalidOperationException(Strings.InvalidPrimaryKey(entry.EntityType.Name));
            }

            return keyValue;
        }

        public virtual IEnumerable<InternalEntityEntry> GetDependents(InternalEntityEntry principalEntry, IForeignKey foreignKey)
        {
            var principalKeyValue = principalEntry.GetPrincipalKeyValue(foreignKey);

            // TODO: Perf: Add additional indexes so that this isn't a linear lookup
            return principalKeyValue == EntityKey.InvalidEntityKey
                ? Enumerable.Empty<InternalEntityEntry>()
                : Entries.Where(
                    e => foreignKey.DeclaringEntityType.IsAssignableFrom(e.EntityType)
                         && principalKeyValue.Equals(e.GetDependentKeyValue(foreignKey)));
        }

        [DebuggerStepThrough]
        public virtual int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var entriesToSave = Entries
                .Where(e => e.EntityState == EntityState.Added
                            || e.EntityState == EntityState.Modified
                            || e.EntityState == EntityState.Deleted)
                .Select(e => e.PrepareToSave())
                .ToList();

            if (!entriesToSave.Any())
            {
                return 0;
            }

            try
            {
                var result = SaveChanges(entriesToSave);

                if (acceptAllChangesOnSuccess)
                {
                    AcceptAllChanges(entriesToSave);
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

        public virtual async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entriesToSave = Entries
                .Where(e => e.EntityState == EntityState.Added
                            || e.EntityState == EntityState.Modified
                            || e.EntityState == EntityState.Deleted)
                .Select(e => e.PrepareToSave())
                .ToList();

            if (!entriesToSave.Any())
            {
                return 0;
            }

            try
            {
                var result = await SaveChangesAsync(entriesToSave, cancellationToken);

                if (acceptAllChangesOnSuccess)
                {
                    AcceptAllChanges(entriesToSave);
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
            [NotNull] IReadOnlyList<InternalEntityEntry> entriesToSave) => _database.SaveChanges(entriesToSave);

        protected virtual Task<int> SaveChangesAsync(
            [NotNull] IReadOnlyList<InternalEntityEntry> entriesToSave,
            CancellationToken cancellationToken = default(CancellationToken))
            => _database.SaveChangesAsync(entriesToSave, cancellationToken);

        public virtual void AcceptAllChanges()
        {
            var changedEntries = Entries
                .Where(e => e.EntityState == EntityState.Added
                            || e.EntityState == EntityState.Modified
                            || e.EntityState == EntityState.Deleted)
                .ToList();

            AcceptAllChanges(changedEntries);
        }

        private void AcceptAllChanges(IReadOnlyList<InternalEntityEntry> changedEntries)
        {
            foreach (var entry in changedEntries)
            {
                entry.AutoCommitSidecars();
                entry.AcceptChanges();
            }
        }
    }
}
