// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    // This is lower-level change tracking services used by the ChangeTracker and other parts of the system
    public class StateManager : IStateManager
    {
        private readonly Dictionary<object, InternalEntityEntry> _entityReferenceMap
            = new Dictionary<object, InternalEntityEntry>(ReferenceEqualityComparer.Instance);

        private readonly Dictionary<object, WeakReference<InternalEntityEntry>> _detachedEntityReferenceMap
            = new Dictionary<object, WeakReference<InternalEntityEntry>>(ReferenceEqualityComparer.Instance);

        private readonly Dictionary<IKeyValue, InternalEntityEntry> _identityMap
            = new Dictionary<IKeyValue, InternalEntityEntry>();

        private readonly Dictionary<IForeignKey, Dictionary<IKeyValue, HashSet<InternalEntityEntry>>> _dependentsMap
            = new Dictionary<IForeignKey, Dictionary<IKeyValue, HashSet<InternalEntityEntry>>>();

        private readonly IInternalEntityEntryFactory _factory;
        private readonly IInternalEntityEntrySubscriber _subscriber;
        private readonly IKeyValueFactorySource _keyValueFactorySource;
        private readonly IModel _model;
        private readonly IDatabase _database;

        public StateManager(
            [NotNull] IInternalEntityEntryFactory factory,
            [NotNull] IInternalEntityEntrySubscriber subscriber,
            [NotNull] IInternalEntityEntryNotifier notifier,
            [NotNull] IValueGenerationManager valueGeneration,
            [NotNull] IKeyValueFactorySource keyValueFactorySource,
            [NotNull] IModel model,
            [NotNull] IDatabase database,
            [NotNull] DbContext context)
        {
            _factory = factory;
            _subscriber = subscriber;
            _keyValueFactorySource = keyValueFactorySource;
            Notify = notifier;
            ValueGeneration = valueGeneration;
            _model = model;
            _database = database;
            Context = context;
        }

        public virtual bool? SingleQueryMode { get; set; }

        public virtual IInternalEntityEntryNotifier Notify { get; }

        public virtual IValueGenerationManager ValueGeneration { get; }

        public virtual IKeyValue CreateKey(IKey key, object value)
            => _keyValueFactorySource.GetKeyFactory(key).Create(value);

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

                SingleQueryMode = false;

                var entityType = _model.FindEntityType(entity.GetType());

                if (entityType == null)
                {
                    throw new InvalidOperationException(CoreStrings.EntityTypeNotFound(entity.GetType().DisplayName(false)));
                }

                entry = _subscriber.SnapshotAndSubscribe(_factory.Create(this, entityType, entity), null);

                _detachedEntityReferenceMap[entity] = new WeakReference<InternalEntityEntry>(entry);
            }
            return entry;
        }

        public virtual void BeginTrackingQuery() => SingleQueryMode = SingleQueryMode == null;

        public virtual InternalEntityEntry StartTracking(
            IEntityType entityType,
            IKeyValue keyValue,
            object entity,
            ValueBuffer valueBuffer)
        {
            if (keyValue.IsInvalid)
            {
                throw new InvalidOperationException(CoreStrings.InvalidPrimaryKey(entityType.DisplayName()));
            }

            var existingEntry = TryGetEntry(keyValue);
            if (existingEntry != null)
            {
                if (existingEntry.Entity != entity)
                {
                    throw new InvalidOperationException(CoreStrings.IdentityConflict(entityType.DisplayName()));
                }

                return existingEntry;
            }

            var newEntry = _subscriber.SnapshotAndSubscribe(_factory.Create(this, entityType, entity, valueBuffer), valueBuffer);

            AddToIdentityMap(entityType, keyValue, newEntry);

            _entityReferenceMap[entity] = newEntry;
            _detachedEntityReferenceMap.Remove(entity);

            newEntry.SetEntityState(EntityState.Unchanged);

            return newEntry;
        }

        private void AddToIdentityMap(IEntityType entityType, IKeyValue keyValue, InternalEntityEntry newEntry)
        {
            _identityMap.Add(keyValue, newEntry);
            foreach (var key in entityType.GetKeys().Where(k => k != keyValue.Key))
            {
                var principalKeyValue = newEntry.GetPrincipalKeyValue(key);

                if (!principalKeyValue.IsInvalid)
                {
                    _identityMap[principalKeyValue] = newEntry;
                }
            }

            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                var dependentKey = newEntry.GetDependentKeyValue(foreignKey);
                if (dependentKey.IsInvalid)
                {
                    continue;
                }

                Dictionary<IKeyValue, HashSet<InternalEntityEntry>> fkMap;
                if (!_dependentsMap.TryGetValue(foreignKey, out fkMap))
                {
                    fkMap = new Dictionary<IKeyValue, HashSet<InternalEntityEntry>>();
                    _dependentsMap[foreignKey] = fkMap;
                }

                HashSet<InternalEntityEntry> dependents;
                if (!fkMap.TryGetValue(dependentKey, out dependents))
                {
                    dependents = new HashSet<InternalEntityEntry>();
                    fkMap[dependentKey] = dependents;
                }

                dependents.Add(newEntry);
            }
        }

        public virtual InternalEntityEntry TryGetEntry(IKeyValue keyValueValue)
        {
            InternalEntityEntry entry;
            _identityMap.TryGetValue(keyValueValue, out entry);
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
                throw new InvalidOperationException(CoreStrings.WrongStateManager(entityType.Name));
            }

            var mapKey = entry.Entity ?? entry;
            var existingEntry = TryGetEntry(mapKey);

            if ((existingEntry == null)
                || (existingEntry == entry))
            {
                _entityReferenceMap[mapKey] = entry;
                _detachedEntityReferenceMap.Remove(mapKey);
            }
            else
            {
                throw new InvalidOperationException(CoreStrings.MultipleEntries(entityType.Name));
            }

            var keyValue = GetKeyValueChecked(entityType.FindPrimaryKey(), entry);

            if (_identityMap.TryGetValue(keyValue, out existingEntry))
            {
                if (existingEntry != entry)
                {
                    // TODO: Consider specialized exception types
                    // Issue #611
                    throw new InvalidOperationException(CoreStrings.IdentityConflict(entityType.Name));
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
                    && (existingEntry == entry))
                {
                    _identityMap.Remove(keyValue);
                }
            }

            foreach (var foreignKey in entry.EntityType.GetForeignKeys())
            {
                var dependentKey = entry.GetDependentKeyValue(foreignKey);

                Dictionary<IKeyValue, HashSet<InternalEntityEntry>> fkMap;
                HashSet<InternalEntityEntry> dependents;
                if (!dependentKey.IsInvalid
                    && _dependentsMap.TryGetValue(foreignKey, out fkMap)
                    && fkMap.TryGetValue(dependentKey, out dependents))
                {
                    dependents.Remove(entry);

                    if (dependents.Count == 0)
                    {
                        fkMap.Remove(dependentKey);

                        if (fkMap.Count == 0)
                        {
                            _dependentsMap.Remove(foreignKey);
                        }
                    }
                }
            }
        }

        public virtual InternalEntityEntry GetPrincipal(InternalEntityEntry dependentEntry, IForeignKey foreignKey, ValueSource valueSource)
        {
            var dependentKeyValue = dependentEntry.GetDependentKeyValue(foreignKey, valueSource);
            if (dependentKeyValue.IsInvalid)
            {
                return null;
            }

            InternalEntityEntry principalEntry;
            _identityMap.TryGetValue(dependentKeyValue, out principalEntry);

            return principalEntry;
        }

        public virtual void UpdateIdentityMap(InternalEntityEntry entry, IKeyValue oldKeyValue, IKey principalKey)
        {
            if (entry.EntityState == EntityState.Detached)
            {
                return;
            }

            var newKey = GetKeyValueChecked(principalKey, entry);

            if (oldKeyValue.Equals(newKey))
            {
                return;
            }

            InternalEntityEntry existingEntry;
            if (_identityMap.TryGetValue(newKey, out existingEntry)
                && (existingEntry != entry))
            {
                throw new InvalidOperationException(CoreStrings.IdentityConflict(entry.EntityType.Name));
            }

            _identityMap.Remove(oldKeyValue);

            if (!newKey.IsInvalid)
            {
                _identityMap[newKey] = entry;
            }
        }

        public virtual void UpdateDependentMap(InternalEntityEntry entry, IKeyValue oldKeyValue, IForeignKey foreignKey)
        {
            if (entry.EntityState == EntityState.Detached)
            {
                return;
            }

            var newKey = entry.GetDependentKeyValue(foreignKey);

            if (oldKeyValue.Equals(newKey))
            {
                return;
            }

            Dictionary<IKeyValue, HashSet<InternalEntityEntry>> fkMap;
            if (_dependentsMap.TryGetValue(foreignKey, out fkMap))
            {
                HashSet<InternalEntityEntry> dependents;

                if (!oldKeyValue.IsInvalid
                    && fkMap.TryGetValue(oldKeyValue, out dependents))
                {
                    dependents.Remove(entry);

                    if (dependents.Count == 0)
                    {
                        fkMap.Remove(oldKeyValue);
                    }
                }

                if (newKey.IsInvalid)
                {
                    if (fkMap.Count == 0)
                    {
                        _dependentsMap.Remove(foreignKey);
                    }
                }
                else
                {
                    if (!fkMap.TryGetValue(newKey, out dependents))
                    {
                        dependents = new HashSet<InternalEntityEntry>();
                        fkMap[newKey] = dependents;
                    }

                    dependents.Add(entry);
                }
            }
        }

        private static IKeyValue GetKeyValueChecked(IKey key, InternalEntityEntry entry)
        {
            var keyValue = entry.GetPrincipalKeyValue(key);

            if (keyValue.IsInvalid)
            {
                // TODO: Check message text here
                throw new InvalidOperationException(CoreStrings.InvalidPrimaryKey(entry.EntityType.Name));
            }

            return keyValue;
        }

        public virtual IEnumerable<InternalEntityEntry> GetDependents(InternalEntityEntry principalEntry, IForeignKey foreignKey)
        {
            var keyValue = principalEntry.GetPrincipalKeyValue(foreignKey);

            Dictionary<IKeyValue, HashSet<InternalEntityEntry>> fkMap;
            HashSet<InternalEntityEntry> dependents;
            return !keyValue.IsInvalid
                   && _dependentsMap.TryGetValue(foreignKey, out fkMap)
                   && fkMap.TryGetValue(keyValue, out dependents)
                ? dependents
                : Enumerable.Empty<InternalEntityEntry>();
        }

        public virtual int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var entriesToSave = GetEntriesToSave();
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
                    entry.DiscardStoreGeneratedValues();
                }
                throw;
            }
        }

        private List<InternalEntityEntry> GetEntriesToSave()
        {
            foreach (var entry in Entries.Where(e => e.HasConceptualNull).ToList())
            {
                entry.HandleConceptualNulls();
            }

            foreach (var entry in Entries.Where(e => e.EntityState == EntityState.Deleted).ToList())
            {
                entry.CascadeDelete();
            }

            return Entries
                .Where(e => (e.EntityState == EntityState.Added)
                            || (e.EntityState == EntityState.Modified)
                            || (e.EntityState == EntityState.Deleted))
                .Select(e => e.PrepareToSave())
                .ToList();
        }

        public virtual async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entriesToSave = GetEntriesToSave();
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
                    entry.DiscardStoreGeneratedValues();
                }
                throw;
            }
        }

        protected virtual int SaveChanges(
            [NotNull] IReadOnlyList<InternalEntityEntry> entriesToSave)
            => _database.SaveChanges(entriesToSave);

        protected virtual Task<int> SaveChangesAsync(
            [NotNull] IReadOnlyList<InternalEntityEntry> entriesToSave,
            CancellationToken cancellationToken = default(CancellationToken))
            => _database.SaveChangesAsync(entriesToSave, cancellationToken);

        public virtual void AcceptAllChanges()
        {
            var changedEntries = Entries
                .Where(e => (e.EntityState == EntityState.Added)
                            || (e.EntityState == EntityState.Modified)
                            || (e.EntityState == EntityState.Deleted))
                .ToList();

            AcceptAllChanges(changedEntries);
        }

        private static void AcceptAllChanges(IEnumerable<InternalEntityEntry> changedEntries)
        {
            foreach (var entry in changedEntries)
            {
                entry.AcceptChanges();
            }
        }

        public virtual DbContext Context { get; }
    }
}
