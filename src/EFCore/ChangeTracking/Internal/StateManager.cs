// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

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

        private readonly Dictionary<IEntityType, Dictionary<object, InternalEntityEntry>> _dependentTypeReferenceMap
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

        private readonly IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> _changeTrackingLogger;
        private readonly IInternalEntityEntryFactory _internalEntityEntryFactory;
        private readonly IInternalEntityEntrySubscriber _internalEntityEntrySubscriber;
        private readonly IModel _model;
        private readonly IDatabase _database;
        private readonly IConcurrencyDetector _concurrencyDetector;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public StateManager([NotNull] StateManagerDependencies dependencies)
        {
            _internalEntityEntryFactory = dependencies.InternalEntityEntryFactory;
            _internalEntityEntrySubscriber = dependencies.InternalEntityEntrySubscriber;
            InternalEntityEntryNotifier = dependencies.InternalEntityEntryNotifier;
            ValueGenerationManager = dependencies.ValueGenerationManager;
            _model = dependencies.Model;
            _database = dependencies.Database;
            _concurrencyDetector = dependencies.ConcurrencyDetector;
            Context = dependencies.CurrentContext.Context;
            EntityFinderFactory = new EntityFinderFactory(dependencies.EntityFinderSource, this, dependencies.SetSource, dependencies.CurrentContext.Context);
            EntityMaterializerSource = dependencies.EntityMaterializerSource;

            if (dependencies.LoggingOptions.IsSensitiveDataLoggingEnabled)
            {
                SensitiveLoggingEnabled = true;
            }

            UpdateLogger = dependencies.UpdateLogger;
            _changeTrackingLogger = dependencies.ChangeTrackingLogger;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool SensitiveLoggingEnabled { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IDiagnosticsLogger<DbLoggerCategory.Update> UpdateLogger { get; }

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
        public virtual IInternalEntityEntryNotifier InternalEntityEntryNotifier { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IValueGenerationManager ValueGenerationManager { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DbContext Context { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEntityFinderFactory EntityFinderFactory { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEntityMaterializerSource EntityMaterializerSource { get; }

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

                var entityType = _model.FindRuntimeEntityType(entity.GetType());
                if (entityType == null)
                {
                    if (_model.HasEntityTypeWithDefiningNavigation(entity.GetType()))
                    {
                        throw new InvalidOperationException(
                            CoreStrings.UntrackedDependentEntity(
                                entity.GetType().ShortDisplayName(),
                                "." + nameof(EntityEntry.Reference) + "()." + nameof(ReferenceEntry.TargetEntry)));
                    }

                    throw new InvalidOperationException(CoreStrings.EntityTypeNotFound(entity.GetType().ShortDisplayName()));
                }

                if (entityType.IsQueryType)
                {
                    throw new InvalidOperationException(CoreStrings.QueryTypeNotValid(entityType.DisplayName()));
                }

                entry = _internalEntityEntryFactory.Create(this, entityType, entity);

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
                _trackingQueryMode = TrackingQueryMode.Multiple;

                entry = _internalEntityEntryFactory.Create(this, entityType, entity);

                AddToReferenceMap(entry);
            }

            return entry;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry CreateEntry(IDictionary<string, object> values, IEntityType entityType)
        {
            _trackingQueryMode = TrackingQueryMode.Multiple;

            var i = 0;
            var valuesArray = new object[entityType.PropertyCount()];
            foreach (var property in entityType.GetProperties())
            {
                valuesArray[i++] = values.TryGetValue(property.Name, out var value)
                    ? value
                    : property.ClrType.GetDefaultValue();
            }

            var valueBuffer = new ValueBuffer(valuesArray);

            var entity = entityType.HasClrType()
                ? EntityMaterializerSource.GetMaterializer(entityType)(
                    new MaterializationContext(valueBuffer, Context))
                : null;

            var entry = _internalEntityEntryFactory.Create(this, entityType, entity, valueBuffer);

            AddToReferenceMap(entry);

            return entry;
        }

        private void AddToReferenceMap(InternalEntityEntry entry)
        {
            var mapKey = entry.Entity ?? entry;
            var entityType = entry.EntityType;
            if (entityType.HasDefiningNavigation())
            {
                foreach (var otherType in _model.GetEntityTypes(entityType.Name)
                    .Where(et => et != entityType && TryGetEntry(mapKey, et) != null))
                {
                    UpdateLogger.DuplicateDependentEntityTypeInstanceWarning(entityType, otherType);
                }

                if (!_dependentTypeReferenceMap.TryGetValue(entityType, out var entries))
                {
                    entries = new Dictionary<object, InternalEntityEntry>(ReferenceEqualityComparer.Instance);
                    _dependentTypeReferenceMap[entityType] = entries;
                }

                entries[mapKey] = entry;
            }
            else
            {
                _entityReferenceMap[mapKey] = entry;
            }
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
            in ValueBuffer valueBuffer,
            ISet<IForeignKey> handledForeignKeys)
        {
            var existingEntry = TryGetEntry(entity);
            if (existingEntry != null)
            {
                return existingEntry;
            }

            var clrType = entity.GetType();
            var entityType = baseEntityType.ClrType == clrType
                || baseEntityType.HasDefiningNavigation()
                    ? baseEntityType
                    : _model.FindRuntimeEntityType(clrType);

            var newEntry = valueBuffer.IsEmpty
                ? _internalEntityEntryFactory.Create(this, entityType, entity)
                : _internalEntityEntryFactory.Create(this, entityType, entity, valueBuffer);

            foreach (var key in baseEntityType.GetKeys())
            {
                GetOrCreateIdentityMap(key).AddOrUpdate(newEntry);
            }

            AddToReferenceMap(newEntry);

            newEntry.MarkUnchangedFromQuery(handledForeignKeys);

            if (_internalEntityEntrySubscriber.SnapshotAndSubscribe(newEntry))
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
        public virtual InternalEntityEntry TryGetEntry(IKey key, in ValueBuffer valueBuffer, bool throwOnNullKey)
            => GetOrCreateIdentityMap(key).TryGetEntry(valueBuffer, throwOnNullKey);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry TryGetEntry(object entity, bool throwOnNonUniqueness = true)
        {
            if (_entityReferenceMap.TryGetValue(entity, out var entry))
            {
                return entry;
            }

            var type = entity.GetType();
            var found = false;
            foreach (var keyValue in _dependentTypeReferenceMap)
            {
                // ReSharper disable once CheckForReferenceEqualityInstead.2
                if (keyValue.Key.ClrType.IsAssignableFrom(type)
                    && keyValue.Value.TryGetValue(entity, out var foundEntry))
                {
                    if (found)
                    {
                        if (!throwOnNonUniqueness)
                        {
                            return null;
                        }

                        throw new InvalidOperationException(
                            CoreStrings.AmbiguousDependentEntity(
                                entity.GetType().ShortDisplayName(),
                                "." + nameof(EntityEntry.Reference) + "()." + nameof(ReferenceEntry.TargetEntry)));
                    }

                    entry = foundEntry;
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
            => _entityReferenceMap.TryGetValue(entity, out var entry)
                ? entry
                : _dependentTypeReferenceMap.TryGetValue(entityType, out var entries)
                  && entries.TryGetValue(entity, out entry)
                    ? entry
                    : null;

        private IIdentityMap GetOrCreateIdentityMap(IKey key)
        {
            if (_identityMap0 == null)
            {
                _identityMap0 = key.GetIdentityMapFactory()(SensitiveLoggingEnabled);
                return _identityMap0;
            }

            if (_identityMap0.Key == key)
            {
                return _identityMap0;
            }

            if (_identityMap1 == null)
            {
                _identityMap1 = key.GetIdentityMapFactory()(SensitiveLoggingEnabled);
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

            if (!_identityMaps.TryGetValue(key, out var identityMap))
            {
                identityMap = key.GetIdentityMapFactory()(SensitiveLoggingEnabled);
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

            return _identityMaps == null
                || !_identityMaps.TryGetValue(key, out var identityMap)
                ? null
                : identityMap;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<InternalEntityEntry> Entries => _entityReferenceMap.Values
            .Concat(_dependentTypeReferenceMap.Values.SelectMany(e => e.Values))
            .Where(
                e => e.EntityState != EntityState.Detached
                     && (e.SharedIdentityEntry == null || e.EntityState != EntityState.Deleted));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry StartTracking(InternalEntityEntry entry)
        {
            var entityType = (EntityType)entry.EntityType;

            if (entry.StateManager != this)
            {
                throw new InvalidOperationException(CoreStrings.WrongStateManager(entityType.DisplayName()));
            }

            var mapKey = entry.Entity ?? entry;
            var existingEntry = TryGetEntry(mapKey, entityType);

            if (existingEntry == null)
            {
                AddToReferenceMap(entry);
            }
            else if (existingEntry != entry)
            {
                throw new InvalidOperationException(CoreStrings.MultipleEntries(entityType.DisplayName()));
            }

            foreach (var key in entityType.GetKeys())
            {
                GetOrCreateIdentityMap(key).Add(entry);
            }

            if (_internalEntityEntrySubscriber.SnapshotAndSubscribe(entry))
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
                _internalEntityEntrySubscriber.Unsubscribe(entry);
            }

            var entityType = entry.EntityType;
            var mapKey = entry.Entity ?? entry;

            if (entityType.HasDefiningNavigation())
            {
                var entries = _dependentTypeReferenceMap[entityType];
                entries.Remove(mapKey);
                if (entries.Count == 0)
                {
                    _dependentTypeReferenceMap.Remove(entityType);
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
                    var untrackedEntityType = _model.FindRuntimeEntityType(keyValuePair.Key.GetType());
                    if (navigations.Any(n => n.GetTargetType().IsAssignableFrom(untrackedEntityType))
                        || untrackedEntityType.GetNavigations().Any(n => n.GetTargetType().IsAssignableFrom(entityType)))
                    {
                        _referencedUntrackedEntities.Value.Remove(keyValuePair.Key);

                        var newList = keyValuePair.Value.Where(tuple => tuple.Item2 != entry).ToList();

                        if (newList.Count > 0)
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
                    _internalEntityEntrySubscriber.Unsubscribe(entry);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void ResetState()
        {
            Unsubscribe();
            ChangedCount = 0;
            _entityReferenceMap.Clear();
            _dependentTypeReferenceMap.Clear();

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

            Tracked = null;
            StateChanged = null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void RecordReferencedUntrackedEntity(
            object referencedEntity, INavigation navigation, InternalEntityEntry referencedFromEntry)
        {
            if (!_referencedUntrackedEntities.Value.TryGetValue(referencedEntity, out var danglers))
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
        public virtual IEnumerable<Tuple<INavigation, InternalEntityEntry>> GetRecordedReferrers(object referencedEntity, bool clear)
        {
            if (_referencedUntrackedEntities.HasValue
                && _referencedUntrackedEntities.Value.TryGetValue(referencedEntity, out var danglers))
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

            return ((IEnumerable<object>)navigationValue).Select(v => TryGetEntry(v)).Where(e => e != null);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEntityFinder CreateEntityFinder(IEntityType entityType)
            => EntityFinderFactory.Create(entityType);

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

            var entriesToSave = GetInternalEntriesToSave();
            if (entriesToSave.Count == 0)
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<IUpdateEntry> GetEntriesToSave()
            => GetInternalEntriesToSave();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<InternalEntityEntry> GetInternalEntriesToSave()
        {
            foreach (var entry in Entries.Where(
                e => (e.EntityState == EntityState.Modified
                      || e.EntityState == EntityState.Added)
                     && e.HasConceptualNull).ToList())
            {
                entry.HandleConceptualNulls(SensitiveLoggingEnabled);
            }

            foreach (var entry in Entries.Where(e => e.EntityState == EntityState.Deleted).ToList())
            {
                CascadeDelete(entry);
            }

            return Entries
                .Where(
                    e => e.EntityState == EntityState.Added
                         || e.EntityState == EntityState.Modified
                         || e.EntityState == EntityState.Deleted)
                .Select(e => e.PrepareToSave())
                .ToList();
        }

        private void CascadeDelete(InternalEntityEntry entry)
        {
            foreach (var fk in entry.EntityType.GetReferencingForeignKeys())
            {
                foreach (var dependent in (GetDependentsFromNavigation(entry, fk)
                                           ?? GetDependents(entry, fk)).ToList())
                {
                    if (dependent.EntityState != EntityState.Deleted
                        && dependent.EntityState != EntityState.Detached
                        && fk.DeleteBehavior != DeleteBehavior.Restrict
                        && (dependent.EntityState == EntityState.Added
                            || KeysEqual(entry, fk, dependent)))
                    {
                        if (fk.DeleteBehavior == DeleteBehavior.Cascade)
                        {
                            var cascadeState = dependent.EntityState == EntityState.Added
                                ? EntityState.Detached
                                : EntityState.Deleted;

                            if (SensitiveLoggingEnabled)
                            {
                                UpdateLogger.CascadeDeleteSensitive(dependent, entry, cascadeState);
                            }
                            else
                            {
                                UpdateLogger.CascadeDelete(dependent, entry, cascadeState);
                            }

                            dependent.SetEntityState(cascadeState);

                            CascadeDelete(dependent);
                        }
                        else
                        {
                            foreach (var dependentProperty in fk.Properties)
                            {
                                dependent[dependentProperty] = null;
                            }

                            if (dependent.HasConceptualNull)
                            {
                                dependent.HandleConceptualNulls(SensitiveLoggingEnabled);
                            }
                        }
                    }
                }
            }
        }

        private static bool KeysEqual(InternalEntityEntry entry, IForeignKey fk, InternalEntityEntry dependent)
        {
            for (var i = 0; i < fk.Properties.Count; i++)
            {
                var principalProperty = fk.PrincipalKey.Properties[i];
                var dependentProperty = fk.Properties[i];

                if (!KeyValuesEqual(
                    principalProperty,
                    entry[principalProperty],
                    dependent[dependentProperty]))
                {
                    //dependent[dependentProperty] = null;
                    return false;
                }
            }

            return true;
        }

        private static bool KeyValuesEqual(IProperty property, object value, object currentValue)
            => (property.GetKeyValueComparer()
                ?? property.FindMapping()?.KeyComparer)
               ?.Equals(currentValue, value)
               ?? Equals(currentValue, value);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            if (ChangedCount == 0)
            {
                return 0;
            }

            var entriesToSave = GetInternalEntriesToSave();
            if (entriesToSave.Count == 0)
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
            CancellationToken cancellationToken = default)
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
                .Where(
                    e => e.EntityState == EntityState.Added
                         || e.EntityState == EntityState.Modified
                         || e.EntityState == EntityState.Deleted)
                .ToList();

            AcceptAllChanges(changedEntries);
        }

        private static void AcceptAllChanges(IReadOnlyList<InternalEntityEntry> changedEntries)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var entryIndex = 0; entryIndex < changedEntries.Count; entryIndex++)
            {
                changedEntries[entryIndex].AcceptChanges();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public event EventHandler<EntityTrackedEventArgs> Tracked;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void OnTracked(InternalEntityEntry internalEntityEntry, bool fromQuery)
        {
            var @event = Tracked;

            if (SensitiveLoggingEnabled)
            {
                _changeTrackingLogger.StartedTrackingSensitive(internalEntityEntry);
            }
            else
            {
                _changeTrackingLogger.StartedTracking(internalEntityEntry);
            }

            @event?.Invoke(Context.ChangeTracker, new EntityTrackedEventArgs(internalEntityEntry, fromQuery));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public event EventHandler<EntityStateChangedEventArgs> StateChanged;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void OnStateChanged(InternalEntityEntry internalEntityEntry, EntityState oldState)
        {
            var @event = StateChanged;
            var newState = internalEntityEntry.EntityState;

            if (SensitiveLoggingEnabled)
            {
                _changeTrackingLogger.StateChangedSensitive(internalEntityEntry, oldState, newState);
            }
            else
            {
                _changeTrackingLogger.StateChanged(internalEntityEntry, oldState, newState);
            }

            @event?.Invoke(Context.ChangeTracker, new EntityStateChangedEventArgs(internalEntityEntry, oldState, newState));
        }
    }
}
