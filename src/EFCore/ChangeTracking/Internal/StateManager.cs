// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    // This is lower-level change tracking services used by the ChangeTracker and other parts of the system
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class StateManager : IStateManager
    {
        private readonly Dictionary<object, InternalEntityEntry> _entityReferenceMap
            = new Dictionary<object, InternalEntityEntry>(ReferenceEqualityComparer.Instance);

        private readonly Dictionary<IEntityType, Dictionary<object, InternalEntityEntry>> _dietReferenceMap
            = new Dictionary<IEntityType, Dictionary<object, InternalEntityEntry>>();

        private readonly LazyRef<IDictionary<object, IList<Tuple<INavigation, InternalEntityEntry>>>> _referencedUntrackedEntities
            = new LazyRef<IDictionary<object, IList<Tuple<INavigation, InternalEntityEntry>>>>(
                () => new Dictionary<object, IList<Tuple<INavigation, InternalEntityEntry>>>(ReferenceEqualityComparer.Instance));

        private IIdentityMap _identityMap0;
        private IIdentityMap _identityMap1;
        private Dictionary<IKey, IIdentityMap> _identityMaps;
        private bool _needsUnsubscribe;
        private bool _queryIsTracked;
        private TrackingQueryMode _trackingQueryMode = TrackingQueryMode.Simple;
        private IEntityType _singleQueryModeEntityType;

        private readonly bool _sensitiveLoggingEnabled;
        private readonly IInternalEntityEntryFactory _factory;
        private readonly IInternalEntityEntrySubscriber _subscriber;
        private readonly IModel _model;
        private readonly IDatabase _database;
        private readonly IConcurrencyDetector _concurrencyDetector;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public StateManager(
            [NotNull] IInternalEntityEntryFactory factory,
            [NotNull] IInternalEntityEntrySubscriber subscriber,
            [NotNull] IInternalEntityEntryNotifier notifier,
            [NotNull] IValueGenerationManager valueGeneration,
            [NotNull] IModel model,
            [NotNull] IDatabase database,
            [NotNull] IConcurrencyDetector concurrencyDetector,
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] ILoggingOptions loggingOptions)
        {
            _factory = factory;
            _subscriber = subscriber;
            Notify = notifier;
            ValueGeneration = valueGeneration;
            _model = model;
            _database = database;
            _concurrencyDetector = concurrencyDetector;
            Context = currentContext.Context;

            if (loggingOptions.SensitiveDataLoggingEnabled)
            {
                _sensitiveLoggingEnabled = true;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TrackingQueryMode GetTrackingQueryMode(IEntityType entityType)
        {
            if (_trackingQueryMode == TrackingQueryMode.Simple
                && _singleQueryModeEntityType != entityType)
            {
                // Drop out if SQM for change of entity type or self-refs since query may not fix them up.
                if (_singleQueryModeEntityType != null
                    || entityType.GetNavigations().Any(n => entityType.IsSameHierarchy(n.GetTargetType())))
                {
                    _trackingQueryMode = TrackingQueryMode.Single;
                }

                _singleQueryModeEntityType = entityType;
            }

            return _trackingQueryMode;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void EndSingleQueryMode() => _trackingQueryMode = TrackingQueryMode.Multiple;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IInternalEntityEntryNotifier Notify { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IValueGenerationManager ValueGeneration { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry GetOrCreateEntry(object entity)
        {
            var entry = TryGetEntry(entity);
            if (entry == null)
            {
                _trackingQueryMode = TrackingQueryMode.Multiple;

                var entityType = _model.FindEntityType(entity.GetType());

                if (entityType == null)
                {
                    if (_model.IsDelegatedIdentityEntityType(entity.GetType()))
                    {
                        throw new InvalidOperationException(CoreStrings.UntrackedDelegatedIdentityEntity(
                            entity.GetType().ShortDisplayName(),
                            "." + nameof(EntityEntry.Reference) + "()." + nameof(ReferenceEntry.TargetEntry)));
                    }
                    else
                    {
                        throw new InvalidOperationException(CoreStrings.EntityTypeNotFound(entity.GetType().ShortDisplayName()));
                    }
                }

                entry = _factory.Create(this, entityType, entity);

                _entityReferenceMap[entity] = entry;
            }
            return entry;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry GetOrCreateEntry(object entity, IEntityType entityType)
        {
            var entry = TryGetEntry(entity, entityType);
            if (entry == null)
            {
                entry = _factory.Create(this, entityType, entity);

                if (entityType.HasDelegatedIdentity())
                {
                    if (!_dietReferenceMap.TryGetValue(entityType, out var entries))
                    {
                        entries = new Dictionary<object, InternalEntityEntry>();
                        _dietReferenceMap[entityType] = entries;
                    }

                    entries[entity] = entry;
                }
                else
                {
                    _entityReferenceMap[entity] = entry;
                }
            }
            return entry;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void BeginTrackingQuery()
        {
            if (_queryIsTracked)
            {
                _trackingQueryMode = TrackingQueryMode.Multiple;
            }
            else
            {
                _queryIsTracked = true;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry StartTrackingFromQuery(
            IEntityType baseEntityType,
            object entity,
            ValueBuffer valueBuffer,
            ISet<IForeignKey> handledForeignKeys)
        {
            var existingEntry = TryGetEntry(entity);
            if (existingEntry != null)
            {
                return existingEntry;
            }

            var clrType = entity.GetType();

            var newEntry = _factory.Create(this,
                baseEntityType.ClrType == clrType
                    ? baseEntityType
                    : _model.FindEntityType(clrType),
                entity, valueBuffer);

            foreach (var key in baseEntityType.GetKeys())
            {
                GetOrCreateIdentityMap(key).AddOrUpdate(newEntry);
            }

            _entityReferenceMap[entity] = newEntry;

            newEntry.MarkUnchangedFromQuery(handledForeignKeys);

            if (_subscriber.SnapshotAndSubscribe(newEntry))
            {
                _needsUnsubscribe = true;
            }

            return newEntry;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry TryGetEntry(IKey key, object[] keyValues)
            => FindIdentityMap(key)?.TryGetEntry(keyValues);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry TryGetEntry(IKey key, ValueBuffer valueBuffer, bool throwOnNullKey)
            => GetOrCreateIdentityMap(key).TryGetEntry(valueBuffer, throwOnNullKey);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry TryGetEntry(object entity)
        {
            if (_entityReferenceMap.TryGetValue(entity, out InternalEntityEntry entry))
            {
                return entry;
            }

            var type = entity.GetType();
            var found = false;
            foreach (var keyValue in _dietReferenceMap)
            {
                // ReSharper disable once CheckForReferenceEqualityInstead.2
                if (Equals(keyValue.Key.ClrType, type)
                    && keyValue.Value.TryGetValue(entity, out entry))
                {
                    if (found)
                    {
                        throw new InvalidOperationException(CoreStrings.AmbiguousDelegatedIdentityEntity(
                            entity.GetType().ShortDisplayName(),
                            "." + nameof(EntityEntry.Reference) + "()." + nameof(ReferenceEntry.TargetEntry)));
                    }
                    found = true;
                }
            }

            return entry;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry TryGetEntry(object entity, IEntityType entityType)
            => _entityReferenceMap.TryGetValue(entity, out InternalEntityEntry entry)
                ? entry
                : _dietReferenceMap.TryGetValue(entityType, out var entries)
                  && entries.TryGetValue(entity, out entry)
                    ? entry
                    : null;

        private IIdentityMap GetOrCreateIdentityMap(IKey key)
        {
            if (_identityMap0 == null)
            {
                _identityMap0 = key.GetIdentityMapFactory()(_sensitiveLoggingEnabled);
                return _identityMap0;
            }

            if (_identityMap0.Key == key)
            {
                return _identityMap0;
            }

            if (_identityMap1 == null)
            {
                _identityMap1 = key.GetIdentityMapFactory()(_sensitiveLoggingEnabled);
                return _identityMap1;
            }

            if (_identityMap1.Key == key)
            {
                return _identityMap1;
            }

            if (_identityMaps == null)
            {
                _identityMaps = new Dictionary<IKey, IIdentityMap>();
            }

            IIdentityMap identityMap;
            if (!_identityMaps.TryGetValue(key, out identityMap))
            {
                identityMap = key.GetIdentityMapFactory()(_sensitiveLoggingEnabled);
                _identityMaps[key] = identityMap;
            }
            return identityMap;
        }

        private IIdentityMap FindIdentityMap(IKey key)
        {
            if (_identityMap0 == null)
            {
                return null;
            }

            if (_identityMap0.Key == key)
            {
                return _identityMap0;
            }

            if (_identityMap1 == null)
            {
                return null;
            }

            if (_identityMap1.Key == key)
            {
                return _identityMap1;
            }

            IIdentityMap identityMap;
            if (_identityMaps == null
                || !_identityMaps.TryGetValue(key, out identityMap))
            {
                return null;
            }
            return identityMap;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<InternalEntityEntry> Entries => _entityReferenceMap.Values
            .Concat(_dietReferenceMap.Values.SelectMany(e => e.Values));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry StartTracking(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;

            if (entry.StateManager != this)
            {
                throw new InvalidOperationException(CoreStrings.WrongStateManager(entityType.DisplayName()));
            }

            var mapKey = entry.Entity ?? entry;
            var existingEntry = TryGetEntry(mapKey, entityType);

            if (existingEntry == null
                || existingEntry == entry)
            {
                if (entityType.HasDelegatedIdentity())
                {
                    _dietReferenceMap[entityType][mapKey] = entry;
                }
                else
                {
                    _entityReferenceMap[mapKey] = entry;
                }
            }
            else
            {
                throw new InvalidOperationException(CoreStrings.MultipleEntries(entityType.DisplayName()));
            }

            foreach (var key in entityType.GetKeys())
            {
                GetOrCreateIdentityMap(key).Add(entry);
            }

            if (_subscriber.SnapshotAndSubscribe(entry))
            {
                _needsUnsubscribe = true;
            }

            return entry;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void StopTracking(InternalEntityEntry entry)
        {
            if (_needsUnsubscribe)
            {
                _subscriber.Unsubscribe(entry);
            }

            var entityType = entry.EntityType;
            var mapKey = entry.Entity ?? entry;

            if (entityType.HasDelegatedIdentity())
            {
                var entries = _dietReferenceMap[entityType];
                entries.Remove(mapKey);
                if (entries.Count == 0)
                {
                    _dietReferenceMap.Remove(entityType);
                }
            }
            else
            {
                _entityReferenceMap.Remove(mapKey);
            }

            foreach (var key in entityType.GetKeys())
            {
                FindIdentityMap(key)?.Remove(entry);
            }

            if (_referencedUntrackedEntities.HasValue)
            {
                var navigations = entityType.GetNavigations().ToList();

                foreach (var keyValuePair in _referencedUntrackedEntities.Value.ToList())
                {
                    var untrackedEntityType = _model.FindEntityType(keyValuePair.Key.GetType());
                    if (navigations.Any(n => n.GetTargetType().IsAssignableFrom(untrackedEntityType))
                        || untrackedEntityType.GetNavigations().Any(n => n.GetTargetType().IsAssignableFrom(entityType)))
                    {
                        _referencedUntrackedEntities.Value.Remove(keyValuePair.Key);

                        var newList = keyValuePair.Value.Where(tuple => tuple.Item2 != entry).ToList();

                        if (newList.Any())
                        {
                            _referencedUntrackedEntities.Value.Add(keyValuePair.Key, newList);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Unsubscribe()
        {
            if (_needsUnsubscribe)
            {
                foreach (var entry in Entries)
                {
                    _subscriber.Unsubscribe(entry);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Reset()
        {
            Unsubscribe();
            _entityReferenceMap.Clear();
            _dietReferenceMap.Clear();

            if (_referencedUntrackedEntities.HasValue)
            {
                _referencedUntrackedEntities.Value.Clear();
            }

            _identityMaps?.Clear();
            _identityMap0?.Clear();
            _identityMap1?.Clear();

            _needsUnsubscribe = false;
            _queryIsTracked = false;
            _trackingQueryMode = TrackingQueryMode.Simple;
            _singleQueryModeEntityType = null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void RecordReferencedUntrackedEntity(
            object referencedEntity, INavigation navigation, InternalEntityEntry referencedFromEntry)
        {
            IList<Tuple<INavigation, InternalEntityEntry>> danglers;
            if (!_referencedUntrackedEntities.Value.TryGetValue(referencedEntity, out danglers))
            {
                danglers = new List<Tuple<INavigation, InternalEntityEntry>>();
                _referencedUntrackedEntities.Value.Add(referencedEntity, danglers);
            }
            danglers.Add(Tuple.Create(navigation, referencedFromEntry));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Tuple<INavigation, InternalEntityEntry>> GetRecordedReferers(object referencedEntity, bool clear)
        {
            IList<Tuple<INavigation, InternalEntityEntry>> danglers;
            if (_referencedUntrackedEntities.HasValue
                && _referencedUntrackedEntities.Value.TryGetValue(referencedEntity, out danglers))
            {
                if (clear)
                {
                    _referencedUntrackedEntities.Value.Remove(referencedEntity);
                }
                return danglers;
            }

            return Enumerable.Empty<Tuple<INavigation, InternalEntityEntry>>();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry GetPrincipal(InternalEntityEntry dependentEntry, IForeignKey foreignKey)
            => FindIdentityMap(foreignKey.PrincipalKey)?.TryGetEntry(foreignKey, dependentEntry);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry GetPrincipalUsingPreStoreGeneratedValues(InternalEntityEntry dependentEntry, IForeignKey foreignKey)
            => FindIdentityMap(foreignKey.PrincipalKey)?.TryGetEntryUsingPreStoreGeneratedValues(foreignKey, dependentEntry);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry GetPrincipalUsingRelationshipSnapshot(InternalEntityEntry dependentEntry, IForeignKey foreignKey)
            => FindIdentityMap(foreignKey.PrincipalKey)?.TryGetEntryUsingRelationshipSnapshot(foreignKey, dependentEntry);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void UpdateIdentityMap(InternalEntityEntry entry, IKey key)
        {
            if (entry.EntityState == EntityState.Detached)
            {
                return;
            }

            var identityMap = FindIdentityMap(key);
            if (identityMap == null)
            {
                return;
            }

            identityMap.RemoveUsingRelationshipSnapshot(entry);
            identityMap.Add(entry);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void UpdateDependentMap(InternalEntityEntry entry, IForeignKey foreignKey)
        {
            if (entry.EntityState == EntityState.Detached)
            {
                return;
            }

            FindIdentityMap(foreignKey.DeclaringEntityType.FindPrimaryKey())
                ?.FindDependentsMap(foreignKey)
                ?.Update(entry);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<InternalEntityEntry> GetDependents(
            InternalEntityEntry principalEntry, IForeignKey foreignKey)
        {
            var dependentIdentityMap = FindIdentityMap(foreignKey.DeclaringEntityType.FindPrimaryKey());
            return dependentIdentityMap != null
                ? dependentIdentityMap.GetDependentsMap(foreignKey).GetDependents(principalEntry)
                : Enumerable.Empty<InternalEntityEntry>();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<InternalEntityEntry> GetDependentsUsingRelationshipSnapshot(
            InternalEntityEntry principalEntry, IForeignKey foreignKey)
        {
            var dependentIdentityMap = FindIdentityMap(foreignKey.DeclaringEntityType.FindPrimaryKey());
            return dependentIdentityMap != null
                ? dependentIdentityMap.GetDependentsMap(foreignKey).GetDependentsUsingRelationshipSnapshot(principalEntry)
                : Enumerable.Empty<InternalEntityEntry>();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<InternalEntityEntry> GetDependentsFromNavigation(InternalEntityEntry principalEntry, IForeignKey foreignKey)
        {
            var navigation = foreignKey.PrincipalToDependent;
            if (navigation == null)
            {
                return null;
            }

            var navigationValue = principalEntry[navigation];
            if (navigationValue == null)
            {
                return Enumerable.Empty<InternalEntityEntry>();
            }

            if (foreignKey.IsUnique)
            {
                var dependentEntry = TryGetEntry(navigationValue);

                return dependentEntry != null
                    ? new[] { dependentEntry }
                    : Enumerable.Empty<InternalEntityEntry>();
            }

            return ((IEnumerable<object>)navigationValue).Select(TryGetEntry).Where(e => e != null);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual int ChangedCount { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            if (ChangedCount == 0)
            {
                return 0;
            }

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
            foreach (var entry in Entries.Where(
                e => (e.EntityState == EntityState.Modified
                      || e.EntityState == EntityState.Added)
                     && e.HasConceptualNull).ToList())
            {
                entry.HandleConceptualNulls();
            }

            foreach (var entry in Entries.Where(e => e.EntityState == EntityState.Deleted).ToList())
            {
                entry.CascadeDelete();
            }

            return Entries
                .Where(e => e.EntityState == EntityState.Added
                            || e.EntityState == EntityState.Modified
                            || e.EntityState == EntityState.Deleted)
                .Select(e => e.PrepareToSave())
                .ToList();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (ChangedCount == 0)
            {
                return 0;
            }

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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual int SaveChanges(
            [NotNull] IReadOnlyList<InternalEntityEntry> entriesToSave)
        {
            using (_concurrencyDetector.EnterCriticalSection())
            {
                return _database.SaveChanges(entriesToSave);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual async Task<int> SaveChangesAsync(
            [NotNull] IReadOnlyList<InternalEntityEntry> entriesToSave,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (_concurrencyDetector.EnterCriticalSection())
            {
                return await _database.SaveChangesAsync(entriesToSave, cancellationToken);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AcceptAllChanges()
        {
            var changedEntries = Entries
                .Where(e => e.EntityState == EntityState.Added
                            || e.EntityState == EntityState.Modified
                            || e.EntityState == EntityState.Deleted)
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DbContext Context { get; }
    }
}
