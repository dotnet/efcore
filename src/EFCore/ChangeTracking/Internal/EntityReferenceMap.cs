// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
        private Dictionary<object, InternalEntityEntry> _detachedReferenceMap;
        private Dictionary<object, InternalEntityEntry> _unchangedReferenceMap;
        private Dictionary<object, InternalEntityEntry> _addedReferenceMap;
        private Dictionary<object, InternalEntityEntry> _modifiedReferenceMap;
        private Dictionary<object, InternalEntityEntry> _deletedReferenceMap;
        private Dictionary<IEntityType, EntityReferenceMap> _dependentTypeReferenceMap;

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
            [NotNull] InternalEntityEntry entry,
            EntityState state,
            EntityState? oldState)
        {
            var mapKey = entry.Entity ?? entry;
            var entityType = entry.EntityType;
            if (_hasSubMap
                && entityType.HasDefiningNavigation())
            {
                if (_dependentTypeReferenceMap == null)
                {
                    _dependentTypeReferenceMap = new Dictionary<IEntityType, EntityReferenceMap>();
                }

                if (!_dependentTypeReferenceMap.TryGetValue(entityType, out var dependentMap))
                {
                    dependentMap = new EntityReferenceMap(hasSubMap: false);
                    _dependentTypeReferenceMap[entityType] = dependentMap;
                }

                dependentMap.Update(entry, state, oldState);
            }
            else
            {
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
                            if (_detachedReferenceMap == null)
                            {
                                _detachedReferenceMap =
                                    new Dictionary<object, InternalEntityEntry>(ReferenceEqualityComparer.Instance);
                            }

                            _detachedReferenceMap[mapKey] = entry;
                            break;
                        case EntityState.Unchanged:
                            if (_unchangedReferenceMap == null)
                            {
                                _unchangedReferenceMap =
                                    new Dictionary<object, InternalEntityEntry>(ReferenceEqualityComparer.Instance);
                            }

                            _unchangedReferenceMap[mapKey] = entry;
                            break;
                        case EntityState.Deleted:
                            if (_deletedReferenceMap == null)
                            {
                                _deletedReferenceMap =
                                    new Dictionary<object, InternalEntityEntry>(ReferenceEqualityComparer.Instance);
                            }

                            _deletedReferenceMap[mapKey] = entry;
                            break;
                        case EntityState.Modified:
                            if (_modifiedReferenceMap == null)
                            {
                                _modifiedReferenceMap =
                                    new Dictionary<object, InternalEntityEntry>(ReferenceEqualityComparer.Instance);
                            }

                            _modifiedReferenceMap[mapKey] = entry;
                            break;
                        case EntityState.Added:
                            if (_addedReferenceMap == null)
                            {
                                _addedReferenceMap =
                                    new Dictionary<object, InternalEntityEntry>(ReferenceEqualityComparer.Instance);
                            }

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
            [NotNull] object entity,
            [CanBeNull] IEntityType entityType,
            [CanBeNull] out InternalEntityEntry entry,
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
                && _dependentTypeReferenceMap != null)
            {
                if (entityType != null)
                {
                    if (_dependentTypeReferenceMap.TryGetValue(entityType, out var subMap))
                    {
                        return subMap.TryGet(entity, entityType, out entry, throwOnNonUniqueness);
                    }
                }
                else
                {
                    var type = entity.GetType();
                    foreach (var keyValue in _dependentTypeReferenceMap)
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

            if (_dependentTypeReferenceMap != null)
            {
                foreach (var map in _dependentTypeReferenceMap)
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

            var hasDependentTypes
                = _dependentTypeReferenceMap != null
                  && _dependentTypeReferenceMap.Count > 0;

            if (!hasDependentTypes)
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
                        return _unchangedReferenceMap.Values;
                    }
                    if (returnAdded)
                    {
                        return _addedReferenceMap.Values;
                    }
                    if (returnModified)
                    {
                        return _modifiedReferenceMap.Values;
                    }
                    if (returnDeleted)
                    {
                        return _deletedReferenceMap.Values;
                    }
                }

                if (numberOfStates == 0)
                {
                    return Enumerable.Empty<InternalEntityEntry>();
                }
            }

            return GetEntriesForState(
                added, modified, deleted, unchanged,
                hasDependentTypes,
                returnAdded, returnModified, returnDeleted, returnUnchanged);
        }

        private IEnumerable<InternalEntityEntry> GetEntriesForState(
            bool added,
            bool modified,
            bool deleted,
            bool unchanged,
            bool hasDependentTypes,
            bool returnAdded,
            bool returnModified,
            bool returnDeleted,
            bool returnUnchanged)
        {
            if (returnAdded)
            {
                foreach (var entry in _addedReferenceMap.Values)
                {
                    yield return entry;
                }
            }

            if (returnModified)
            {
                foreach (var entry in _modifiedReferenceMap.Values)
                {
                    yield return entry;
                }
            }

            if (returnDeleted)
            {
                foreach (var entry in _deletedReferenceMap.Values)
                {
                    yield return entry;
                }
            }

            if (returnUnchanged)
            {
                foreach (var entry in _unchangedReferenceMap.Values)
                {
                    yield return entry;
                }
            }

            if (hasDependentTypes)
            {
                foreach (var subMap in _dependentTypeReferenceMap.Values)
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
            if (_dependentTypeReferenceMap != null
                && entityType.HasDefiningNavigation())
            {
                _dependentTypeReferenceMap[entityType].Remove(entity, entityType, oldState);
            }
            else
            {
                switch (oldState)
                {
                    case EntityState.Detached:
                        _detachedReferenceMap?.Remove(entity);
                        break;
                    case EntityState.Unchanged:
                        _unchangedReferenceMap.Remove(entity);
                        break;
                    case EntityState.Deleted:
                        _deletedReferenceMap.Remove(entity);
                        break;
                    case EntityState.Modified:
                        _modifiedReferenceMap.Remove(entity);
                        break;
                    case EntityState.Added:
                        _addedReferenceMap.Remove(entity);
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
            _dependentTypeReferenceMap?.Clear();
            _dependentTypeReferenceMap = null;
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

            if (_dependentTypeReferenceMap != null
                && _dependentTypeReferenceMap.Count > 0)
            {
                foreach (var subMap in _dependentTypeReferenceMap.Values)
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
