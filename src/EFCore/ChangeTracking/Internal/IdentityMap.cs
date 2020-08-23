// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class IdentityMap<TKey> : IIdentityMap
    {
        private readonly bool _sensitiveLoggingEnabled;
        private readonly Dictionary<TKey, InternalEntityEntry> _identityMap;
        private readonly IForeignKey[] _foreignKeys;
        private Dictionary<IForeignKey, IDependentsMap> _dependentMaps;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IdentityMap(
            [NotNull] IKey key,
            [NotNull] IPrincipalKeyValueFactory<TKey> principalKeyValueFactory,
            bool sensitiveLoggingEnabled)
        {
            _sensitiveLoggingEnabled = sensitiveLoggingEnabled;
            Key = key;
            PrincipalKeyValueFactory = principalKeyValueFactory;
            _identityMap = new Dictionary<TKey, InternalEntityEntry>(principalKeyValueFactory.EqualityComparer);

            if (key.IsPrimaryKey())
            {
                _foreignKeys = key.DeclaringEntityType
                    .GetDerivedTypesInclusive()
                    .SelectMany(EntityTypeExtensions.GetDeclaredForeignKeys)
                    .ToArray();
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IPrincipalKeyValueFactory<TKey> PrincipalKeyValueFactory { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IKey Key { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Contains(in ValueBuffer valueBuffer)
        {
            var key = PrincipalKeyValueFactory.CreateFromBuffer(valueBuffer);
            return key != null && _identityMap.ContainsKey((TKey)key);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Contains(IForeignKey foreignKey, in ValueBuffer valueBuffer)
            => foreignKey.GetDependentKeyValueFactory<TKey>().TryCreateFromBuffer(valueBuffer, out var key)
                && _identityMap.ContainsKey(key);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityEntry TryGetEntry(object[] keyValues)
        {
            var key = PrincipalKeyValueFactory.CreateFromKeyValues(keyValues);
            return key != null && _identityMap.TryGetValue((TKey)key, out var entry) ? entry : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityEntry TryGetEntry(object[] keyValues, bool throwOnNullKey, out bool hasNullKey)
        {
            var key = PrincipalKeyValueFactory.CreateFromKeyValues(keyValues);

            if (key == null)
            {
                if (throwOnNullKey)
                {
                    if (Key.IsPrimaryKey())
                    {
                        throw new InvalidOperationException(
                            CoreStrings.InvalidKeyValue(
                                Key.DeclaringEntityType.DisplayName(),
                                PrincipalKeyValueFactory.FindNullPropertyInKeyValues(keyValues).Name));
                    }

                    throw new InvalidOperationException(
                        CoreStrings.InvalidAlternateKeyValue(
                            Key.DeclaringEntityType.DisplayName(),
                            PrincipalKeyValueFactory.FindNullPropertyInKeyValues(keyValues).Name));
                }

                hasNullKey = true;

                return null;
            }

            hasNullKey = false;

            try
            {
                return _identityMap.TryGetValue((TKey)key, out var entry)
                    ? entry
                    : null;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(
                    // ReSharper disable once PossibleNullReferenceException
                    CoreStrings.ErrorMaterializingPropertyInvalidCast(
                        Key.DeclaringEntityType.DisplayName(),
                        Key.Properties.First().Name,
                        typeof(TKey),
                        key.GetType()),
                    e);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityEntry TryGetEntry(IForeignKey foreignKey, InternalEntityEntry dependentEntry)
            => foreignKey.GetDependentKeyValueFactory<TKey>().TryCreateFromCurrentValues(dependentEntry, out var key)
                && _identityMap.TryGetValue(key, out var entry)
                    ? entry
                    : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityEntry TryGetEntryUsingPreStoreGeneratedValues(
            IForeignKey foreignKey,
            InternalEntityEntry dependentEntry)
            => foreignKey.GetDependentKeyValueFactory<TKey>().TryCreateFromPreStoreGeneratedCurrentValues(dependentEntry, out var key)
                && _identityMap.TryGetValue(key, out var entry)
                    ? entry
                    : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityEntry TryGetEntryUsingRelationshipSnapshot(IForeignKey foreignKey, InternalEntityEntry dependentEntry)
            => foreignKey.GetDependentKeyValueFactory<TKey>().TryCreateFromRelationshipSnapshot(dependentEntry, out var key)
                && _identityMap.TryGetValue(key, out var entry)
                    ? entry
                    : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddOrUpdate(InternalEntityEntry entry)
            => Add(PrincipalKeyValueFactory.CreateFromCurrentValues(entry), entry, updateDuplicate: true);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Add(InternalEntityEntry entry)
            => Add(PrincipalKeyValueFactory.CreateFromCurrentValues(entry), entry);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Add(object[] keyValues, InternalEntityEntry entry)
            => Add((TKey)PrincipalKeyValueFactory.CreateFromKeyValues(keyValues), entry);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void Add([NotNull] TKey key, [NotNull] InternalEntityEntry entry)
            => Add(key, entry, updateDuplicate: false);

        private void ThrowIdentityConflict(InternalEntityEntry entry)
        {
            if (entry.EntityType.IsOwned())
            {
                if (_sensitiveLoggingEnabled)
                {
                    throw new InvalidOperationException(
                        CoreStrings.IdentityConflictOwnedSensitive(
                            entry.EntityType.DisplayName(),
                            entry.BuildCurrentValuesString(Key.Properties)));
                }

                throw new InvalidOperationException(
                    CoreStrings.IdentityConflictOwned(
                        entry.EntityType.DisplayName(),
                        Key.Properties.Format()));
            }

            if (_sensitiveLoggingEnabled)
            {
                throw new InvalidOperationException(
                    CoreStrings.IdentityConflictSensitive(
                        entry.EntityType.DisplayName(),
                        entry.BuildCurrentValuesString(Key.Properties)));
            }

            throw new InvalidOperationException(
                CoreStrings.IdentityConflict(
                    entry.EntityType.DisplayName(),
                    Key.Properties.Format()));
        }

        private void Add(TKey key, InternalEntityEntry entry, bool updateDuplicate)
        {
            if (_identityMap.TryGetValue(key, out var existingEntry))
            {
                var bothStatesEquivalent = (entry.EntityState == EntityState.Deleted) == (existingEntry.EntityState == EntityState.Deleted);
                if (!updateDuplicate)
                {
                    if (existingEntry == entry)
                    {
                        return;
                    }

                    if (bothStatesEquivalent)
                    {
                        ThrowIdentityConflict(entry);
                    }

                    if (existingEntry.SharedIdentityEntry != null)
                    {
                        if (existingEntry.SharedIdentityEntry == entry)
                        {
                            return;
                        }

                        ThrowIdentityConflict(entry);
                    }
                }

                if (!bothStatesEquivalent
                    && Key.IsPrimaryKey())
                {
                    entry.SharedIdentityEntry = existingEntry;
                    existingEntry.SharedIdentityEntry = entry;
                    if (existingEntry.EntityState != EntityState.Deleted)
                    {
                        return;
                    }
                }
            }

            _identityMap[key] = entry;

            if (_dependentMaps != null
                && _foreignKeys != null)
            {
                foreach (var foreignKey in _foreignKeys)
                {
                    if (_dependentMaps.TryGetValue(foreignKey, out var map))
                    {
                        if (existingEntry != null)
                        {
                            map.Remove(existingEntry);
                        }

                        map.Add(entry);
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
        public virtual IDependentsMap GetDependentsMap(IForeignKey foreignKey)
        {
            if (_dependentMaps == null)
            {
                _dependentMaps = new Dictionary<IForeignKey, IDependentsMap>(LegacyReferenceEqualityComparer.Instance);
            }

            if (!_dependentMaps.TryGetValue(foreignKey, out var map))
            {
                map = foreignKey.CreateDependentsMapFactory();

                foreach (var value in _identityMap.Values)
                {
                    map.Add(value);
                }

                _dependentMaps[foreignKey] = map;
            }

            return map;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IDependentsMap FindDependentsMap(IForeignKey foreignKey)
            => _dependentMaps != null
                && _dependentMaps.TryGetValue(foreignKey, out var map)
                    ? map
                    : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Clear()
        {
            _identityMap?.Clear();
            _dependentMaps?.Clear();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Remove(InternalEntityEntry entry)
            => Remove(PrincipalKeyValueFactory.CreateFromCurrentValues(entry), entry);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void RemoveUsingRelationshipSnapshot(InternalEntityEntry entry)
            => Remove(PrincipalKeyValueFactory.CreateFromRelationshipSnapshot(entry), entry);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void Remove([NotNull] TKey key, [NotNull] InternalEntityEntry entry)
        {
            InternalEntityEntry otherEntry = null;
            if (entry.SharedIdentityEntry != null)
            {
                otherEntry = entry.SharedIdentityEntry;
                otherEntry.SharedIdentityEntry = null;
                entry.SharedIdentityEntry = null;

                if (otherEntry.EntityState != EntityState.Deleted)
                {
                    return;
                }
            }

            if (otherEntry == null)
            {
                if (_identityMap.TryGetValue(key, out var existingEntry)
                    && existingEntry == entry)
                {
                    _identityMap.Remove(key);
                }
            }
            else
            {
                _identityMap[key] = otherEntry;
            }

            if (_dependentMaps != null
                && _foreignKeys != null)
            {
                foreach (var foreignKey in _foreignKeys)
                {
                    if (_dependentMaps.TryGetValue(foreignKey, out var map))
                    {
                        map.Remove(entry);
                        if (otherEntry != null)
                        {
                            map.Add(otherEntry);
                        }
                    }
                }
            }
        }
    }
}
