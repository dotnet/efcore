// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class EntityReferenceMap
{
    private readonly bool _hasSubMap;
    private Dictionary<object, InternalEntityEntry>? _detachedReferenceMap;
    private Dictionary<object, InternalEntityEntry>? _unchangedReferenceMap;
    private Dictionary<object, InternalEntityEntry>? _addedReferenceMap;
    private Dictionary<object, InternalEntityEntry>? _modifiedReferenceMap;
    private Dictionary<object, InternalEntityEntry>? _deletedReferenceMap;
    private Dictionary<IEntityType, EntityReferenceMap>? _sharedTypeReferenceMap;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EntityReferenceMap(bool hasSubMap)
    {
        _hasSubMap = hasSubMap;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Update(
        InternalEntityEntry entry,
        EntityState state,
        EntityState? oldState)
    {
        var entityType = entry.EntityType;
        if (_hasSubMap
            && entityType.HasSharedClrType)
        {
            _sharedTypeReferenceMap ??= new Dictionary<IEntityType, EntityReferenceMap>();

            if (!_sharedTypeReferenceMap.TryGetValue(entityType, out var sharedMap))
            {
                sharedMap = new EntityReferenceMap(hasSubMap: false);
                _sharedTypeReferenceMap[entityType] = sharedMap;
            }

            sharedMap.Update(entry, state, oldState);
        }
        else
        {
            var mapKey = entry.Entity;

            if (oldState.HasValue)
            {
                Remove(mapKey, entityType, oldState.Value);
            }

            if (!oldState.HasValue
                || state != EntityState.Detached)
            {
                switch (state)
                {
                    case EntityState.Detached:
                        _detachedReferenceMap ??= new Dictionary<object, InternalEntityEntry>(ReferenceEqualityComparer.Instance);
                        _detachedReferenceMap[mapKey] = entry;
                        break;
                    case EntityState.Unchanged:
                        _unchangedReferenceMap ??=
                            new Dictionary<object, InternalEntityEntry>(ReferenceEqualityComparer.Instance);
                        _unchangedReferenceMap[mapKey] = entry;
                        break;
                    case EntityState.Deleted:
                        _deletedReferenceMap ??= new Dictionary<object, InternalEntityEntry>(ReferenceEqualityComparer.Instance);
                        _deletedReferenceMap[mapKey] = entry;
                        break;
                    case EntityState.Modified:
                        _modifiedReferenceMap ??= new Dictionary<object, InternalEntityEntry>(ReferenceEqualityComparer.Instance);
                        _modifiedReferenceMap[mapKey] = entry;
                        break;
                    case EntityState.Added:
                        _addedReferenceMap ??= new Dictionary<object, InternalEntityEntry>(ReferenceEqualityComparer.Instance);
                        _addedReferenceMap[mapKey] = entry;
                        break;
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
    public virtual bool TryGet(
        object entity,
        IEntityType? entityType,
        [NotNullWhen(true)] out InternalEntityEntry? entry,
        bool throwOnNonUniqueness)
    {
        entry = null;
        var found = _unchangedReferenceMap?.TryGetValue(entity, out entry) == true
            || _modifiedReferenceMap?.TryGetValue(entity, out entry) == true
            || _addedReferenceMap?.TryGetValue(entity, out entry) == true
            || _deletedReferenceMap?.TryGetValue(entity, out entry) == true
            || _detachedReferenceMap?.TryGetValue(entity, out entry) == true;

        if (!found
            && _hasSubMap
            && _sharedTypeReferenceMap != null)
        {
            if (entityType != null)
            {
                if (_sharedTypeReferenceMap.TryGetValue(entityType, out var subMap))
                {
                    return subMap.TryGet(entity, entityType, out entry, throwOnNonUniqueness);
                }
            }
            else
            {
                var type = entity.GetType();
                foreach (var (key, entityReferenceMap) in _sharedTypeReferenceMap)
                {
                    // ReSharper disable once CheckForReferenceEqualityInstead.2
                    if (key.ClrType.IsAssignableFrom(type)
                        && entityReferenceMap.TryGet(entity, entityType, out var foundEntry, throwOnNonUniqueness))
                    {
                        if (found)
                        {
                            if (!throwOnNonUniqueness)
                            {
                                entry = null;
                                return false;
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
            }
        }

        return found;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int GetCountForState(
        bool added,
        bool modified,
        bool deleted,
        bool unchanged,
        bool countDeletedSharedIdentity)
    {
        var count = 0;

        if (added
            && _addedReferenceMap != null)
        {
            count = _addedReferenceMap.Count;
        }

        if (modified
            && _modifiedReferenceMap != null)
        {
            count += _modifiedReferenceMap.Count;
        }

        if (deleted
            && _deletedReferenceMap != null)
        {
            count += countDeletedSharedIdentity
                ? _deletedReferenceMap.Count
                : _deletedReferenceMap.Count(p => p.Value.SharedIdentityEntry == null);
        }

        if (unchanged
            && _unchangedReferenceMap != null)
        {
            count += _unchangedReferenceMap.Count;
        }

        if (_sharedTypeReferenceMap != null)
        {
            foreach (var map in _sharedTypeReferenceMap)
            {
                count += map.Value.GetCountForState(added, modified, deleted, unchanged, countDeletedSharedIdentity);
            }
        }

        return count;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<InternalEntityEntry> GetEntriesForState(
        bool added,
        bool modified,
        bool deleted,
        bool unchanged,
        bool returnDeletedSharedIdentity)
    {
        // Perf sensitive

        var returnAdded = added && _addedReferenceMap is { Count: > 0 };
        var returnModified = modified && _modifiedReferenceMap is { Count: > 0 };
        var returnDeleted = deleted && _deletedReferenceMap is { Count: > 0 };
        var returnUnchanged = unchanged && _unchangedReferenceMap is { Count: > 0 };
        var hasSharedTypes = _sharedTypeReferenceMap is { Count: > 0 };

        if (!hasSharedTypes)
        {
            var numberOfStates
                = (returnAdded ? 1 : 0)
                + (returnModified ? 1 : 0)
                + (returnDeleted ? 1 : 0)
                + (returnUnchanged ? 1 : 0);

            switch (numberOfStates)
            {
                case 1 when returnUnchanged:
                    return _unchangedReferenceMap!.Values;
                case 1 when returnAdded:
                    return _addedReferenceMap!.Values;
                case 1 when returnModified:
                    return _modifiedReferenceMap!.Values;
                case 1 when returnDeleted:
                    return _deletedReferenceMap!.Values;
                case 0:
                    return Enumerable.Empty<InternalEntityEntry>();
            }
        }

        return GetEntriesForState(
            added, modified, deleted, unchanged,
            hasSharedTypes,
            returnAdded, returnModified, returnDeleted, returnUnchanged,
            returnDeletedSharedIdentity);
    }

    private IEnumerable<InternalEntityEntry> GetEntriesForState(
        bool added,
        bool modified,
        bool deleted,
        bool unchanged,
        bool hasSharedTypes,
        bool returnAdded,
        bool returnModified,
        bool returnDeleted,
        bool returnUnchanged,
        bool returnSharedIdentity)
    {
        if (returnAdded)
        {
            foreach (var entry in _addedReferenceMap!.Values)
            {
                yield return entry;
            }
        }

        if (returnModified)
        {
            foreach (var entry in _modifiedReferenceMap!.Values)
            {
                yield return entry;
            }
        }

        if (returnDeleted)
        {
            foreach (var entry in _deletedReferenceMap!.Values)
            {
                if (entry.SharedIdentityEntry == null
                    || returnSharedIdentity)
                {
                    yield return entry;
                }
            }
        }

        if (returnUnchanged)
        {
            foreach (var entry in _unchangedReferenceMap!.Values)
            {
                yield return entry;
            }
        }

        if (hasSharedTypes)
        {
            foreach (var subMap in _sharedTypeReferenceMap!.Values)
            {
                foreach (var entry in subMap.GetEntriesForState(added, modified, deleted, unchanged, returnSharedIdentity))
                {
                    yield return entry;
                }
            }
        }
    }

    private void Remove(
        object entity,
        IEntityType entityType,
        EntityState oldState)
    {
        if (_sharedTypeReferenceMap != null
            && entityType.HasSharedClrType)
        {
            _sharedTypeReferenceMap[entityType].Remove(entity, entityType, oldState);
        }
        else
        {
            switch (oldState)
            {
                case EntityState.Detached:
                    _detachedReferenceMap?.Remove(entity);
                    break;
                case EntityState.Unchanged:
                    _unchangedReferenceMap?.Remove(entity);
                    break;
                case EntityState.Deleted:
                    _deletedReferenceMap?.Remove(entity);
                    break;
                case EntityState.Modified:
                    _modifiedReferenceMap?.Remove(entity);
                    break;
                case EntityState.Added:
                    _addedReferenceMap?.Remove(entity);
                    break;
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Clear()
    {
        _unchangedReferenceMap = null;
        _detachedReferenceMap = null;
        _deletedReferenceMap = null;
        _addedReferenceMap = null;
        _modifiedReferenceMap = null;
        _sharedTypeReferenceMap?.Clear();
        _sharedTypeReferenceMap = null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<TEntity> GetNonDeletedEntities<TEntity>()
        where TEntity : class
    {
        // Perf sensitive

        if (_addedReferenceMap is { Count: > 0 })
        {
            foreach (var entry in _addedReferenceMap.Values)
            {
                if (entry.Entity is TEntity entity)
                {
                    yield return entity;
                }
            }
        }

        if (_modifiedReferenceMap is { Count: > 0 })
        {
            foreach (var entry in _modifiedReferenceMap.Values)
            {
                if (entry.Entity is TEntity entity)
                {
                    yield return entity;
                }
            }
        }

        if (_unchangedReferenceMap is { Count: > 0 })
        {
            foreach (var entry in _unchangedReferenceMap.Values)
            {
                if (entry.Entity is TEntity entity)
                {
                    yield return entity;
                }
            }
        }

        if (_sharedTypeReferenceMap is { Count: > 0 })
        {
            foreach (var subMap in _sharedTypeReferenceMap.Values)
            {
                foreach (var entity in subMap.GetNonDeletedEntities<TEntity>())
                {
                    yield return entity;
                }
            }
        }
    }
}
