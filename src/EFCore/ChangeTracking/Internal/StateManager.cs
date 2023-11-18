// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

// This is lower-level change tracking services used by the ChangeTracker and other parts of the system
/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class StateManager : IStateManager
{
    private readonly EntityReferenceMap _entityReferenceMap = new(hasSubMap: true);

    private Dictionary<object, IList<Tuple<INavigationBase, InternalEntityEntry>>>? _referencedUntrackedEntities;
    private IIdentityMap? _identityMap0;
    private IIdentityMap? _identityMap1;
    private Dictionary<IKey, IIdentityMap>? _identityMaps;
    private bool _needsUnsubscribe;
    private bool _hasServiceProperties;
    private IChangeDetector? _changeDetector;

    private readonly IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> _changeTrackingLogger;
    private readonly IInternalEntityEntrySubscriber _internalEntityEntrySubscriber;
    private readonly IModel _model;
    private readonly IDatabase _database;
    private readonly IConcurrencyDetector? _concurrencyDetector;
    private readonly IIdentityResolutionInterceptor? _resolutionInterceptor;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public StateManager(StateManagerDependencies dependencies)
    {
        Dependencies = dependencies;

        _internalEntityEntrySubscriber = dependencies.InternalEntityEntrySubscriber;
        InternalEntityEntryNotifier = dependencies.InternalEntityEntryNotifier;
        ValueGenerationManager = dependencies.ValueGenerationManager;
        _model = dependencies.Model;
        _database = dependencies.Database;
        _concurrencyDetector = dependencies.CoreSingletonOptions.AreThreadSafetyChecksEnabled
            ? dependencies.ConcurrencyDetector
            : null;
        Context = dependencies.CurrentContext.Context;
        EntityFinderFactory = new EntityFinderFactory(
            dependencies.EntityFinderSource, this, dependencies.SetSource, dependencies.CurrentContext.Context);
        EntityMaterializerSource = dependencies.EntityMaterializerSource;

        if (dependencies.LoggingOptions.IsSensitiveDataLoggingEnabled)
        {
            SensitiveLoggingEnabled = true;
        }

        UpdateLogger = dependencies.UpdateLogger;
        _changeTrackingLogger = dependencies.ChangeTrackingLogger;

        _resolutionInterceptor = dependencies.Interceptors.Aggregate<IIdentityResolutionInterceptor>();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StateManagerDependencies Dependencies { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool SensitiveLoggingEnabled { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IDiagnosticsLogger<DbLoggerCategory.Update> UpdateLogger { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CascadeTiming DeleteOrphansTiming { get; set; } = CascadeTiming.Immediate;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CascadeTiming CascadeDeleteTiming { get; set; } = CascadeTiming.Immediate;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool SavingChanges { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IInternalEntityEntryNotifier InternalEntityEntryNotifier { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ChangingState(InternalEntityEntry entry, EntityState newState)
    {
        InternalEntityEntryNotifier.StateChanging(entry, newState);

        UpdateReferenceMaps(entry, newState, entry.EntityState);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IValueGenerationManager ValueGenerationManager { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DbContext Context { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IModel Model
        => _model;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEntityFinderFactory EntityFinderFactory { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEntityMaterializerSource EntityMaterializerSource { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IChangeDetector ChangeDetector
        => _changeDetector ??= Context.GetDependencies().ChangeDetector;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry GetOrCreateEntry(object entity)
    {
        var entry = TryGetEntry(entity);
        if (entry == null)
        {
            var entityType = _model.FindRuntimeEntityType(entity.GetType());
            if (entityType == null)
            {
                if (_model.IsShared(entity.GetType()))
                {
                    throw new InvalidOperationException(
                        CoreStrings.UntrackedDependentEntity(
                            entity.GetType().ShortDisplayName(),
                            "." + nameof(EntityEntry.Reference) + "()." + nameof(ReferenceEntry.TargetEntry),
                            "." + nameof(EntityEntry.Collection) + "()." + nameof(CollectionEntry.FindEntry) + "()"));
                }

                throw new InvalidOperationException(CoreStrings.EntityTypeNotFound(entity.GetType().ShortDisplayName()));
            }

            if (entityType.FindPrimaryKey() == null)
            {
                throw new InvalidOperationException(CoreStrings.KeylessTypeTracked(entityType.DisplayName()));
            }

            entry = new InternalEntityEntry(this, entityType, entity);

            UpdateReferenceMaps(entry, EntityState.Detached, null);
        }

        return entry;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry GetOrCreateEntry(object entity, IEntityType? entityType)
    {
        if (entityType == null)
        {
            return GetOrCreateEntry(entity);
        }

        var entry = TryGetEntry(entity, entityType);
        if (entry == null)
        {
            var runtimeEntityType = _model.FindRuntimeEntityType(entity.GetType());
            if (runtimeEntityType != null)
            {
                if (!entityType.IsAssignableFrom(runtimeEntityType))
                {
                    throw new InvalidOperationException(
                        CoreStrings.TrackingTypeMismatch(
                            runtimeEntityType.DisplayName(), entityType.DisplayName()));
                }

                entityType = runtimeEntityType;
            }

            if (entityType.FindPrimaryKey() == null)
            {
                throw new InvalidOperationException(CoreStrings.KeylessTypeTracked(entityType.DisplayName()));
            }

            entry = new InternalEntityEntry(this, entityType, entity);

            UpdateReferenceMaps(entry, EntityState.Detached, null);
        }

        return entry;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry CreateEntry(IDictionary<string, object?> values, IEntityType entityType)
    {
        var entry = new InternalEntityEntry(this, entityType, values, EntityMaterializerSource);

        UpdateReferenceMaps(entry, EntityState.Detached, null);

        return entry;
    }

    private void UpdateReferenceMaps(
        InternalEntityEntry entry,
        EntityState state,
        EntityState? oldState)
    {
        var entityType = entry.EntityType;
        if (entityType.HasSharedClrType)
        {
            var mapKey = entry.Entity;
            foreach (var otherType in _model.FindEntityTypes(entityType.ClrType)
                         .Where(et => et != entityType && TryGetEntry(mapKey, et) != null))
            {
                UpdateLogger.DuplicateDependentEntityTypeInstanceWarning(entityType, otherType);
            }
        }

        _entityReferenceMap.Update(entry, state, oldState);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry StartTrackingFromQuery(
        IEntityType baseEntityType,
        object entity,
        in ISnapshot snapshot)
    {
        var existingEntry = TryGetEntry(entity);
        if (existingEntry != null)
        {
            return existingEntry;
        }

        var clrType = entity.GetType();
        var entityType = baseEntityType.HasSharedClrType
            || baseEntityType.ClrType == clrType
                ? baseEntityType
                : _model.FindRuntimeEntityType(clrType)!;

        var newEntry = snapshot.IsEmpty
            ? new InternalEntityEntry(this, entityType, entity)
            : new InternalEntityEntry(this, entityType, entity, snapshot);

        foreach (var key in baseEntityType.GetKeys())
        {
            GetOrCreateIdentityMap(key).AddOrUpdate(newEntry);
        }

        UpdateReferenceMaps(newEntry, EntityState.Unchanged, null);

        newEntry.MarkUnchangedFromQuery();

        if (_internalEntityEntrySubscriber.SnapshotAndSubscribe(newEntry))
        {
            _needsUnsubscribe = true;
        }

        if (!_hasServiceProperties && newEntry.EntityType.HasServiceProperties())
        {
            _hasServiceProperties = true;
        }

        return newEntry;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry? TryGetEntry(IKey key, IReadOnlyList<object?> keyValues)
        => FindIdentityMap(key)?.TryGetEntry(keyValues);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry? TryGetEntryTyped<TKey>(IKey key, TKey keyValue)
        => ((IIdentityMap<TKey>?)FindIdentityMap(key))?.TryGetEntryTyped(keyValue);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry? TryGetEntry(IKey key, object?[] keyValues, bool throwOnNullKey, out bool hasNullKey)
        => GetOrCreateIdentityMap(key).TryGetEntry(keyValues, throwOnNullKey, out hasNullKey);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry? TryGetEntry(object entity, bool throwOnNonUniqueness = true)
        => _entityReferenceMap.TryGet(entity, null, out var entry, throwOnNonUniqueness)
            ? entry
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry? TryGetEntry(object entity, IEntityType entityType, bool throwOnTypeMismatch = true)
        => _entityReferenceMap.TryGet(entity, entityType, out var entry, throwOnNonUniqueness: false)
            ? !entityType.IsAssignableFrom(entry.EntityType)
                ? throwOnTypeMismatch
                    ? throw new InvalidOperationException(
                        CoreStrings.TrackingTypeMismatch(entry.EntityType.DisplayName(), entityType.DisplayName()))
                    : null
                : entry
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry? TryGetExistingEntry(object entity, IKey key)
    {
        var keyValues = GetKeyValues();
        return keyValues == null ? null : TryGetEntry(key, keyValues);

        object[]? GetKeyValues()
        {
            var entry = GetOrCreateEntry(entity);
            var properties = key.Properties;
            var propertyValues = new object[properties.Count];
            for (var i = 0; i < propertyValues.Length; i++)
            {
                var propertyValue = entry[properties[i]];
                if (propertyValue == null)
                {
                    return null;
                }

                propertyValues[i] = propertyValue;
            }

            return propertyValues;
        }
    }

    private IIdentityMap GetOrCreateIdentityMap(IKey key)
    {
        if (_identityMap0 == null)
        {
            _identityMap0 = ((IRuntimeKey)key).GetIdentityMapFactory()(SensitiveLoggingEnabled);
            return _identityMap0;
        }

        if (_identityMap0.Key == key)
        {
            return _identityMap0;
        }

        if (_identityMap1 == null)
        {
            _identityMap1 = ((IRuntimeKey)key).GetIdentityMapFactory()(SensitiveLoggingEnabled);
            return _identityMap1;
        }

        if (_identityMap1.Key == key)
        {
            return _identityMap1;
        }

        _identityMaps ??= new Dictionary<IKey, IIdentityMap>();

        if (!_identityMaps.TryGetValue(key, out var identityMap))
        {
            identityMap = ((IRuntimeKey)key).GetIdentityMapFactory()(SensitiveLoggingEnabled);
            _identityMaps[key] = identityMap;
        }

        return identityMap;
    }

    private IIdentityMap? FindIdentityMap(IKey? key)
    {
        if (_identityMap0 == null
            || key == null)
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int GetCountForState(
        bool added = false,
        bool modified = false,
        bool deleted = false,
        bool unchanged = false,
        bool countDeletedSharedIdentity = false)
        => _entityReferenceMap.GetCountForState(added, modified, deleted, unchanged, countDeletedSharedIdentity);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int Count
        => GetCountForState(added: true, modified: true, deleted: true, unchanged: true, countDeletedSharedIdentity: true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<InternalEntityEntry> GetEntriesForState(
        bool added = false,
        bool modified = false,
        bool deleted = false,
        bool unchanged = false,
        bool returnDeletedSharedIdentity = false)
        => _entityReferenceMap.GetEntriesForState(added, modified, deleted, unchanged, returnDeletedSharedIdentity);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<InternalEntityEntry> Entries
        => GetEntriesForState(added: true, modified: true, deleted: true, unchanged: true, returnDeletedSharedIdentity: true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<TEntity> GetNonDeletedEntities<TEntity>()
        where TEntity : class
        => _entityReferenceMap.GetNonDeletedEntities<TEntity>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry StartTracking(InternalEntityEntry entry)
    {
        var entityType = entry.EntityType;

        if (entry.StateManager != this)
        {
            throw new InvalidOperationException(CoreStrings.WrongStateManager(entityType.DisplayName()));
        }

#if DEBUG
        var existingEntry = TryGetEntry(entry.Entity, entityType);

        Check.DebugAssert(existingEntry == null || existingEntry == entry, "Duplicate InternalEntityEntry");
#endif

        foreach (var key in entityType.GetKeys())
        {
            GetOrCreateIdentityMap(key).Add(entry);
        }

        if (_internalEntityEntrySubscriber.SnapshotAndSubscribe(entry))
        {
            _needsUnsubscribe = true;
        }

        if (!_hasServiceProperties && entry.EntityType.HasServiceProperties())
        {
            _hasServiceProperties = true;
        }

        return entry;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void StopTracking(InternalEntityEntry entry, EntityState oldState)
    {
        if (_needsUnsubscribe)
        {
            _internalEntityEntrySubscriber.Unsubscribe(entry);
        }

        foreach (var key in entry.EntityType.GetKeys())
        {
            foreach (var foreignKey in key.GetReferencingForeignKeys())
            {
                var dependentToPrincipal = foreignKey.DependentToPrincipal;
                if (dependentToPrincipal != null)
                {
                    foreach (InternalEntityEntry dependentEntry in GetDependents(entry, foreignKey))
                    {
                        if (dependentEntry[dependentToPrincipal] == entry.Entity)
                        {
                            RecordReferencedUntrackedEntity(entry.Entity, dependentToPrincipal, dependentEntry);
                        }
                    }
                }
            }

            FindIdentityMap(key)?.Remove(entry);
        }

        if (_referencedUntrackedEntities != null)
        {
            foreach (var (key, value) in _referencedUntrackedEntities.ToList())
            {
                if (value.Any(t => t.Item2 == entry))
                {
                    _referencedUntrackedEntities.Remove(key);

                    var newList = value.Where(tuple => tuple.Item2 != entry).ToList();

                    if (newList.Count > 0)
                    {
                        _referencedUntrackedEntities.Add(key, newList);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Unsubscribe(bool resetting)
    {
        if (_needsUnsubscribe)
        {
            foreach (var entry in Entries)
            {
                _internalEntityEntrySubscriber.Unsubscribe(entry);
            }
        }

        if (_hasServiceProperties)
        {
            foreach (var entry in Entries)
            {
                foreach (var serviceProperty in entry.EntityType.GetServiceProperties())
                {
                    var service = entry[serviceProperty];
                    if (resetting
                        && service is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                    else if (resetting
                             && service is ILazyLoader lazyLoader)
                    {
                        lazyLoader.Dispose();
                    }
                    else if (service is not IInjectableService detachable
                             || detachable.Detaching(Context, entry.Entity))
                    {
                        entry[serviceProperty] = null;
                    }
                }
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ResetState()
    {
        Clear(resetting: true);
        Dependencies.NavigationFixer.AbortDelayedFixup();
        _changeDetector?.ResetState();

        Tracking = null;
        Tracked = null;
        StateChanging = null;
        StateChanged = null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Clear(bool resetting)
    {
        Unsubscribe(resetting);
        ChangedCount = 0;
        _entityReferenceMap.Clear();
        _referencedUntrackedEntities = null;

        _identityMaps?.Clear();
        _identityMap0?.Clear();
        _identityMap1?.Clear();

        _needsUnsubscribe = false;
        _hasServiceProperties = false;

        SavingChanges = false;

        foreach (IResettableService set in ((IDbSetCache)Context).GetSets())
        {
            set.ResetState();
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual Task ResetStateAsync(CancellationToken cancellationToken = default)
    {
        ResetState();

        return Task.CompletedTask;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void BeginAttachGraph()
        => Dependencies.NavigationFixer.BeginDelayedFixup();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void CompleteAttachGraph()
        => Dependencies.NavigationFixer.CompleteDelayedFixup();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AbortAttachGraph()
        => Dependencies.NavigationFixer.AbortDelayedFixup();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void RecordReferencedUntrackedEntity(
        object referencedEntity,
        INavigationBase navigation,
        InternalEntityEntry referencedFromEntry)
    {
        _referencedUntrackedEntities ??=
            new Dictionary<object, IList<Tuple<INavigationBase, InternalEntityEntry>>>(ReferenceEqualityComparer.Instance);

        if (!_referencedUntrackedEntities.TryGetValue(referencedEntity, out var danglers))
        {
            danglers = new List<Tuple<INavigationBase, InternalEntityEntry>>();
            _referencedUntrackedEntities.Add(referencedEntity, danglers);
        }

        danglers.Add(Tuple.Create(navigation, referencedFromEntry));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void UpdateReferencedUntrackedEntity(
        object referencedEntity,
        object newReferencedEntity,
        INavigationBase navigation,
        InternalEntityEntry referencedFromEntry)
    {
        if (_referencedUntrackedEntities != null
            && _referencedUntrackedEntities.TryGetValue(referencedEntity, out var danglers))
        {
            _referencedUntrackedEntities.Remove(referencedEntity);

            if (!_referencedUntrackedEntities.TryGetValue(newReferencedEntity, out var newDanglers))
            {
                newDanglers = new List<Tuple<INavigationBase, InternalEntityEntry>>();
                _referencedUntrackedEntities.Add(newReferencedEntity, newDanglers);
            }

            foreach (var dangler in danglers)
            {
                newDanglers.Add(dangler);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool ResolveToExistingEntry(
        InternalEntityEntry newEntry,
        INavigationBase? navigation,
        InternalEntityEntry? referencedFromEntry)
    {
        if (_resolutionInterceptor != null)
        {
            var interceptionData = new IdentityResolutionInterceptionData(Context);
            var needsTracking = false;
            foreach (var key in newEntry.EntityType.GetKeys())
            {
                var existingEntry = FindIdentityMap(key)?.TryGetEntry(newEntry);
                if (existingEntry != null)
                {
                    _resolutionInterceptor.UpdateTrackedInstance(
                        interceptionData,
                        new EntityEntry(existingEntry),
                        newEntry.Entity);

                    if (navigation != null)
                    {
                        UpdateReferencedUntrackedEntity(
                            newEntry.Entity,
                            existingEntry.Entity,
                            navigation,
                            referencedFromEntry!);

                        var navigationValue = referencedFromEntry![navigation];
                        if (navigationValue != null && navigation.IsCollection)
                        {
                            referencedFromEntry.RemoveFromCollection(navigation, newEntry.Entity);
                        }
                    }

                    InternalEntityEntryNotifier.FixupResolved(existingEntry, newEntry);
                }
                else
                {
                    needsTracking = true;
                }
            }

            return !needsTracking;
        }

        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Tuple<INavigationBase, InternalEntityEntry>> GetRecordedReferrers(object referencedEntity, bool clear)
    {
        if (_referencedUntrackedEntities != null
            && _referencedUntrackedEntities.TryGetValue(referencedEntity, out var danglers))
        {
            if (clear)
            {
                _referencedUntrackedEntities.Remove(referencedEntity);
            }

            return danglers;
        }

        return Enumerable.Empty<Tuple<INavigationBase, InternalEntityEntry>>();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry? FindPrincipal(
        InternalEntityEntry dependentEntry,
        IForeignKey foreignKey)
        => FilterIncompatiblePrincipal(
            foreignKey,
            FindIdentityMap(foreignKey.PrincipalKey)
                ?.TryGetEntry(foreignKey, dependentEntry));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry? FindPrincipalUsingPreStoreGeneratedValues(
        InternalEntityEntry dependentEntry,
        IForeignKey foreignKey)
        => FilterIncompatiblePrincipal(
            foreignKey,
            FindIdentityMap(foreignKey.PrincipalKey)
                ?.TryGetEntryUsingPreStoreGeneratedValues(foreignKey, dependentEntry));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry? FindPrincipalUsingRelationshipSnapshot(
        InternalEntityEntry dependentEntry,
        IForeignKey foreignKey)
        => FilterIncompatiblePrincipal(
            foreignKey,
            FindIdentityMap(foreignKey.PrincipalKey)
                ?.TryGetEntryUsingRelationshipSnapshot(foreignKey, dependentEntry));

    private static InternalEntityEntry? FilterIncompatiblePrincipal(
        IForeignKey foreignKey,
        InternalEntityEntry? principalEntry)
        => principalEntry != null
            && foreignKey.PrincipalEntityType.IsAssignableFrom(principalEntry.EntityType)
                ? principalEntry
                : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<IUpdateEntry> GetDependents(
        IUpdateEntry principalEntry,
        IForeignKey foreignKey)
    {
        var dependentIdentityMap = FindIdentityMap(foreignKey.DeclaringEntityType.FindPrimaryKey());
        return dependentIdentityMap != null && foreignKey.PrincipalEntityType.IsAssignableFrom(principalEntry.EntityType)
            ? dependentIdentityMap.GetDependentsMap(foreignKey).GetDependents(principalEntry)
            : Enumerable.Empty<IUpdateEntry>();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<InternalEntityEntry> GetEntries(IKey key)
    {
        var identityMap = FindIdentityMap(key);
        return identityMap == null
            ? Enumerable.Empty<InternalEntityEntry>()
            : identityMap.All();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<IUpdateEntry> GetDependents(
        IReadOnlyList<object?> keyValues,
        IForeignKey foreignKey)
    {
        GetOrCreateIdentityMap(foreignKey.PrincipalKey); // Ensure the identity map is created even if principal not tracked.
        return GetOrCreateIdentityMap(foreignKey.DeclaringEntityType.FindPrimaryKey()!)
            .GetDependentsMap(foreignKey).GetDependents(keyValues);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<IUpdateEntry> GetDependentsUsingRelationshipSnapshot(
        IUpdateEntry principalEntry,
        IForeignKey foreignKey)
    {
        var dependentIdentityMap = FindIdentityMap(foreignKey.DeclaringEntityType.FindPrimaryKey());
        return dependentIdentityMap != null
            ? dependentIdentityMap.GetDependentsMap(foreignKey).GetDependentsUsingRelationshipSnapshot(principalEntry)
            : Enumerable.Empty<IUpdateEntry>();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<IUpdateEntry>? GetDependentsFromNavigation(
        IUpdateEntry principalEntry,
        IForeignKey foreignKey)
    {
        var navigation = foreignKey.PrincipalToDependent;
        if (navigation == null
            || navigation.IsShadowProperty())
        {
            return null;
        }

        var navigationValue = ((InternalEntityEntry)principalEntry)[navigation];
        if (navigationValue == null)
        {
            return Enumerable.Empty<InternalEntityEntry>();
        }

        if (foreignKey.IsUnique)
        {
            var dependentEntry = TryGetEntry(navigationValue, foreignKey.DeclaringEntityType);

            return dependentEntry != null
                ? new[] { dependentEntry }
                : Enumerable.Empty<InternalEntityEntry>();
        }

        return ((IEnumerable<object>)navigationValue)
            // ReSharper disable once RedundantEnumerableCastCall
            .Select(v => TryGetEntry(v, foreignKey.DeclaringEntityType)).Where(e => e != null).Cast<IUpdateEntry>();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEntityFinder CreateEntityFinder(IEntityType entityType)
        => EntityFinderFactory.Create(entityType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int ChangedCount { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IList<IUpdateEntry> GetEntriesToSave(bool cascadeChanges)
    {
        if (cascadeChanges)
        {
            CascadeChanges(force: false);
        }

        var toSave = new List<IUpdateEntry>(GetCountForState(added: true, modified: true, deleted: true));

        // Perf sensitive

        foreach (var entry in GetEntriesForState(added: true, modified: true, deleted: true))
        {
            toSave.Add(entry.PrepareToSave());
        }

        return toSave;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void CascadeChanges(bool force)
    {
        // Perf sensitive

        var toHandle = new List<InternalEntityEntry>();

        foreach (var entry in GetEntriesForState(modified: true, added: true))
        {
            if (entry.HasConceptualNull)
            {
                toHandle.Add(entry);
            }
        }

        foreach (var entry in toHandle)
        {
            entry.HandleConceptualNulls(SensitiveLoggingEnabled, force, isCascadeDelete: false);
        }

        foreach (var entry in this.ToListForState(deleted: true))
        {
            CascadeDelete(entry, force);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void CascadeDelete(InternalEntityEntry entry, bool force, IEnumerable<IForeignKey>? foreignKeys = null)
    {
        var doCascadeDelete = force || CascadeDeleteTiming != CascadeTiming.Never;
        var principalIsDetached = entry.EntityState == EntityState.Detached;

        var detectChangesEnabled = Context.ChangeTracker.AutoDetectChangesEnabled
            && !((IRuntimeModel)Model).SkipDetectChanges;

        foreignKeys ??= entry.EntityType.GetReferencingForeignKeys();
        foreach (var fk in foreignKeys)
        {
            if (fk.DeleteBehavior == DeleteBehavior.ClientNoAction)
            {
                continue;
            }

            foreach (InternalEntityEntry dependent in (GetDependentsFromNavigation(entry, fk)
                         ?? GetDependents(entry, fk)).ToList())
            {
                if (dependent.SharedIdentityEntry == entry)
                {
                    continue;
                }

                if (detectChangesEnabled)
                {
                    ChangeDetector.DetectChanges(dependent);
                }

                if (dependent.EntityState != EntityState.Deleted
                    && dependent.EntityState != EntityState.Detached
                    && (dependent.EntityState == EntityState.Added
                        || KeysEqual(entry, fk, dependent)))
                {
                    if (fk.DeleteBehavior is DeleteBehavior.Cascade or DeleteBehavior.ClientCascade
                        && doCascadeDelete)
                    {
                        var cascadeState = principalIsDetached
                            || dependent.EntityState == EntityState.Added
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

                        CascadeDelete(dependent, force);
                    }
                    else if (!principalIsDetached)
                    {
                        fk.GetPropertiesWithMinimalOverlapIfPossible(out var fkProperties, out _);

                        foreach (var dependentProperty in fkProperties)
                        {
                            dependent.SetProperty(
                                dependentProperty, null, isMaterialization: false, setModified: true, isCascadeDelete: true);
                        }

                        if (dependent.HasConceptualNull)
                        {
                            dependent.HandleConceptualNulls(SensitiveLoggingEnabled, force, isCascadeDelete: true);
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
                return false;
            }
        }

        return true;
    }

    private static bool KeyValuesEqual(IProperty property, object? value, object? currentValue)
        => property.GetKeyValueComparer().Equals(currentValue, value);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual int SaveChanges(IList<IUpdateEntry> entriesToSave)
    {
        _concurrencyDetector?.EnterCriticalSection();

        try
        {
            EntityFrameworkEventSource.Log.SavingChanges();

            return _database.SaveChanges(entriesToSave);
        }
        finally
        {
            _concurrencyDetector?.ExitCriticalSection();
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual async Task<int> SaveChangesAsync(
        IList<IUpdateEntry> entriesToSave,
        CancellationToken cancellationToken = default)
    {
        _concurrencyDetector?.EnterCriticalSection();

        try
        {
            EntityFrameworkEventSource.Log.SavingChanges();

            return await _database.SaveChangesAsync(entriesToSave, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _concurrencyDetector?.ExitCriticalSection();
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int SaveChanges(bool acceptAllChangesOnSuccess)
        => Context.Database.AutoTransactionBehavior == AutoTransactionBehavior.Never
            ? SaveChanges(this, acceptAllChangesOnSuccess)
            : Dependencies.ExecutionStrategy.Execute(
                (StateManager: this, AcceptAllChangesOnSuccess: acceptAllChangesOnSuccess),
                static (_, t) => SaveChanges(t.StateManager, t.AcceptAllChangesOnSuccess),
                null);

    private static int SaveChanges(StateManager stateManager, bool acceptAllChangesOnSuccess)
    {
        if (stateManager.ChangedCount == 0)
        {
            return 0;
        }

        var entriesToSave = stateManager.GetEntriesToSave(cascadeChanges: true);
        if (entriesToSave.Count == 0)
        {
            return 0;
        }

        try
        {
            stateManager.SavingChanges = true;
            var result = stateManager.SaveChanges(entriesToSave);

            if (acceptAllChangesOnSuccess)
            {
                AcceptAllChanges((IReadOnlyList<IUpdateEntry>)entriesToSave);
            }

            return result;
        }
        catch
        {
            foreach (var entry in entriesToSave)
            {
                ((InternalEntityEntry)entry).DiscardStoreGeneratedValues();
            }

            throw;
        }
        finally
        {
            stateManager.SavingChanges = false;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
        => Context.Database.AutoTransactionBehavior == AutoTransactionBehavior.Never
            ? SaveChangesAsync(this, acceptAllChangesOnSuccess, cancellationToken)
            : Dependencies.ExecutionStrategy.ExecuteAsync(
                (StateManager: this, AcceptAllChangesOnSuccess: acceptAllChangesOnSuccess),
                static (_, t, cancellationToken) => SaveChangesAsync(t.StateManager, t.AcceptAllChangesOnSuccess, cancellationToken),
                null,
                cancellationToken);

    private static async Task<int> SaveChangesAsync(
        StateManager stateManager,
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken)
    {
        if (stateManager.ChangedCount == 0)
        {
            return 0;
        }

        var entriesToSave = stateManager.GetEntriesToSave(cascadeChanges: true);
        if (entriesToSave.Count == 0)
        {
            return 0;
        }

        try
        {
            stateManager.SavingChanges = true;
            var result = await stateManager.SaveChangesAsync(entriesToSave, cancellationToken)
                .ConfigureAwait(acceptAllChangesOnSuccess);

            if (acceptAllChangesOnSuccess)
            {
                AcceptAllChanges((IReadOnlyList<IUpdateEntry>)entriesToSave);
            }

            return result;
        }
        catch
        {
            foreach (var entry in entriesToSave)
            {
                ((InternalEntityEntry)entry).DiscardStoreGeneratedValues();
            }

            throw;
        }
        finally
        {
            stateManager.SavingChanges = false;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AcceptAllChanges()
        => AcceptAllChanges(this.ToListForState(added: true, modified: true, deleted: true));

    private static void AcceptAllChanges(IReadOnlyList<IUpdateEntry> changedEntries)
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var entryIndex = 0; entryIndex < changedEntries.Count; entryIndex++)
        {
            ((InternalEntityEntry)changedEntries[entryIndex]).AcceptChanges();
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual (
        EventHandler<EntityTrackingEventArgs>? Tracking,
        EventHandler<EntityTrackedEventArgs>? Tracked,
        EventHandler<EntityStateChangingEventArgs>? StateChanging,
        EventHandler<EntityStateChangedEventArgs>? StateChanged) CaptureEvents()
        => (Tracking, Tracked, StateChanging, StateChanged);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetEvents(
        EventHandler<EntityTrackingEventArgs>? tracking,
        EventHandler<EntityTrackedEventArgs>? tracked,
        EventHandler<EntityStateChangingEventArgs>? stateChanging,
        EventHandler<EntityStateChangedEventArgs>? stateChanged)
    {
        Tracking = tracking;
        Tracked = tracked;
        StateChanging = stateChanging;
        StateChanged = stateChanged;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public event EventHandler<EntityTrackingEventArgs>? Tracking;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void OnTracking(InternalEntityEntry internalEntityEntry, EntityState state, bool fromQuery)
    {
        var @event = Tracking;

        @event?.Invoke(Context.ChangeTracker, new EntityTrackingEventArgs(internalEntityEntry, state, fromQuery));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public event EventHandler<EntityTrackedEventArgs>? Tracked;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public event EventHandler<EntityStateChangingEventArgs>? StateChanging;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void OnStateChanging(InternalEntityEntry internalEntityEntry, EntityState newState)
    {
        var @event = StateChanging;
        var oldState = internalEntityEntry.EntityState;

        @event?.Invoke(Context.ChangeTracker, new EntityStateChangingEventArgs(internalEntityEntry, oldState, newState));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public event EventHandler<EntityStateChangedEventArgs>? StateChanged;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
