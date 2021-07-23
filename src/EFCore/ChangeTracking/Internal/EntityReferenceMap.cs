// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;


namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
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
                if (_sharedTypeReferenceMap == null)
                {
                    _sharedTypeReferenceMap = new Dictionary<IEntityType, EntityReferenceMap>();
                }

                if (!_sharedTypeReferenceMap.TryGetValue(entityType, out var sharedMap))
                {
                    sharedMap = new EntityReferenceMap(hasSubMap: false);
                    _sharedTypeReferenceMap[entityType] = sharedMap;
                }

                sharedMap.Update(entry, state, oldState);
            }
            else
            {
                var mapKey = entry.Entity ?? entry;

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
                            _detachedReferenceMap ??= new Dictionary<object, InternalEntityEntry>(LegacyReferenceEqualityComparer.Instance);
                            _detachedReferenceMap[mapKey] = entry;
                            break;
                        case EntityState.Unchanged:
                            _unchangedReferenceMap ??=
                                new Dictionary<object, InternalEntityEntry>(LegacyReferenceEqualityComparer.Instance);
                            _unchangedReferenceMap[mapKey] = entry;
                            break;
                        case EntityState.Deleted:
                            _deletedReferenceMap ??= new Dictionary<object, InternalEntityEntry>(LegacyReferenceEqualityComparer.Instance);
                            _deletedReferenceMap[mapKey] = entry;
                            break;
                        case EntityState.Modified:
                            _modifiedReferenceMap ??= new Dictionary<object, InternalEntityEntry>(LegacyReferenceEqualityComparer.Instance);
                            _modifiedReferenceMap[mapKey] = entry;
                            break;
                        case EntityState.Added:
                            _addedReferenceMap ??= new Dictionary<object, InternalEntityEntry>(LegacyReferenceEqualityComparer.Instance);
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
                    foreach (var keyValue in _sharedTypeReferenceMap)
                    {
                        // ReSharper disable once CheckForReferenceEqualityInstead.2
                        if (keyValue.Key.ClrType.IsAssignableFrom(type)
                            && keyValue.Value.TryGet(entity, entityType, out var foundEntry, throwOnNonUniqueness))
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
            bool added = false,
            bool modified = false,
            bool deleted = false,
            bool unchanged = false)
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
                count += _deletedReferenceMap.Count;
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
                    count += map.Value.GetCountForState(added, modified, deleted, unchanged);
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
            bool added = false,
            bool modified = false,
            bool deleted = false,
            bool unchanged = false)
        {
            // Perf sensitive

            var returnAdded
                = added
                && _addedReferenceMap != null
                && _addedReferenceMap.Count > 0;

            var returnModified
                = modified
                && _modifiedReferenceMap != null
                && _modifiedReferenceMap.Count > 0;

            var returnDeleted
                = deleted
                && _deletedReferenceMap != null
                && _deletedReferenceMap.Count > 0;

            var returnUnchanged
                = unchanged
                && _unchangedReferenceMap != null
                && _unchangedReferenceMap.Count > 0;

            var hasSharedTypes
                = _sharedTypeReferenceMap != null
                && _sharedTypeReferenceMap.Count > 0;

            if (!hasSharedTypes)
            {
                var numberOfStates
                    = (returnAdded ? 1 : 0)
                    + (returnModified ? 1 : 0)
                    + (returnDeleted ? 1 : 0)
                    + (returnUnchanged ? 1 : 0);

                if (numberOfStates == 1)
                {
                    if (returnUnchanged)
                    {
                        return _unchangedReferenceMap!.Values;
                    }

                    if (returnAdded)
                    {
                        return _addedReferenceMap!.Values;
                    }

                    if (returnModified)
                    {
                        return _modifiedReferenceMap!.Values;
                    }

                    if (returnDeleted)
                    {
                        return _deletedReferenceMap!.Values;
                    }
                }

                if (numberOfStates == 0)
                {
                    return Enumerable.Empty<InternalEntityEntry>();
                }
            }

            return GetEntriesForState(
                added, modified, deleted, unchanged,
                hasSharedTypes,
                returnAdded, returnModified, returnDeleted, returnUnchanged);
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
            bool returnUnchanged)
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
                    yield return entry;
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
                    foreach (var entry in subMap.GetEntriesForState(added, modified, deleted, unchanged))
                    {
                        if ((entry.SharedIdentityEntry == null
                            || entry.EntityState != EntityState.Deleted))
                        {
                            yield return entry;
                        }
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

            if (_addedReferenceMap != null
                && _addedReferenceMap.Count > 0)
            {
                foreach (var entry in _addedReferenceMap.Values)
                {
                    if (entry.Entity is TEntity entity)
                    {
                        yield return entity;
                    }
                }
            }

            if (_modifiedReferenceMap != null
                && _modifiedReferenceMap.Count > 0)
            {
                foreach (var entry in _modifiedReferenceMap.Values)
                {
                    if (entry.Entity is TEntity entity)
                    {
                        yield return entity;
                    }
                }
            }

            if (_unchangedReferenceMap != null
                && _unchangedReferenceMap.Count > 0)
            {
                foreach (var entry in _unchangedReferenceMap.Values)
                {
                    if (entry.Entity is TEntity entity)
                    {
                        yield return entity;
                    }
                }
            }

            if (_sharedTypeReferenceMap != null
                && _sharedTypeReferenceMap.Count > 0)
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
}
