// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class IdentityMap<TKey> : IIdentityMap
    {
        private readonly Dictionary<TKey, InternalEntityEntry> _identityMap;
        private readonly IList<IForeignKey> _foreignKeys;
        private Dictionary<IForeignKey, IDependentsMap> _dependentMaps;

        public IdentityMap(
            [NotNull] IKey key,
            [NotNull] IPrincipalKeyValueFactory<TKey> principalKeyValueFactory)
        {
            Key = key;
            PrincipalKeyValueFactory = principalKeyValueFactory;
            _identityMap = new Dictionary<TKey, InternalEntityEntry>(principalKeyValueFactory.EqualityComparer);

            if (key.IsPrimaryKey())
            {
                _foreignKeys = key.DeclaringEntityType
                    .GetDerivedTypesInclusive()
                    .SelectMany(e => e.GetForeignKeys())
                    .Distinct()
                    .ToList();
            }
        }

        protected virtual IPrincipalKeyValueFactory<TKey> PrincipalKeyValueFactory { get; }

        public virtual IKey Key { get; }

        public virtual bool Contains(ValueBuffer valueBuffer)
        {
            var key = PrincipalKeyValueFactory.CreateFromBuffer(valueBuffer);
            return key != null && _identityMap.ContainsKey((TKey)key);
        }

        public virtual bool Contains(IForeignKey foreignKey, ValueBuffer valueBuffer)
        {
            TKey key;
            return foreignKey.GetDependentKeyValueFactory<TKey>().TryCreateFromBuffer(valueBuffer, out key)
                   && _identityMap.ContainsKey(key);
        }

        public virtual InternalEntityEntry TryGetEntry(ValueBuffer valueBuffer, bool throwOnNullKey)
        {
            InternalEntityEntry entry;
            var key = PrincipalKeyValueFactory.CreateFromBuffer(valueBuffer);
            if (key == null
                && throwOnNullKey)
            {
                throw new InvalidOperationException(CoreStrings.InvalidKeyValue(Key.DeclaringEntityType.DisplayName()));
            }
            return key != null && _identityMap.TryGetValue((TKey)key, out entry) ? entry : null;
        }

        public virtual InternalEntityEntry TryGetEntry(IForeignKey foreignKey, InternalEntityEntry dependentEntry)
        {
            TKey key;
            InternalEntityEntry entry;
            return foreignKey.GetDependentKeyValueFactory<TKey>().TryCreateFromCurrentValues(dependentEntry, out key)
                   && _identityMap.TryGetValue(key, out entry)
                ? entry
                : null;
        }

        public virtual InternalEntityEntry TryGetEntryUsingRelationshipSnapshot(IForeignKey foreignKey, InternalEntityEntry dependentEntry)
        {
            TKey key;
            InternalEntityEntry entry;
            return foreignKey.GetDependentKeyValueFactory<TKey>().TryCreateFromRelationshipSnapshot(dependentEntry, out key)
                   && _identityMap.TryGetValue(key, out entry)
                ? entry
                : null;
        }

        public virtual void AddOrUpdate(InternalEntityEntry entry)
            => AddInternal(PrincipalKeyValueFactory.CreateFromCurrentValues(entry), entry);

        public virtual void Add(InternalEntityEntry entry)
            => Add(PrincipalKeyValueFactory.CreateFromCurrentValues(entry), entry);

        protected virtual void Add([NotNull] TKey key, [NotNull] InternalEntityEntry entry)
        {
            InternalEntityEntry existingEntry;
            if (_identityMap.TryGetValue(key, out existingEntry))
            {
                if (existingEntry != entry)
                {
                    throw new InvalidOperationException(CoreStrings.IdentityConflict(entry.EntityType.DisplayName()));
                }
            }
            else
            {
                AddInternal(key, entry);
            }
        }

        private void AddInternal(TKey key, InternalEntityEntry entry)
        {
            _identityMap[key] = entry;

            if (_dependentMaps != null
                && _foreignKeys != null)
            {
                foreach (var foreignKey in _foreignKeys)
                {
                    IDependentsMap map;
                    if (_dependentMaps.TryGetValue(foreignKey, out map))
                    {
                        map.Add(entry);
                    }
                }
            }
        }

        public virtual IDependentsMap GetDependentsMap(IForeignKey foreignKey)
        {
            if (_dependentMaps == null)
            {
                _dependentMaps = new Dictionary<IForeignKey, IDependentsMap>(ReferenceEqualityComparer.Instance);
            }

            IDependentsMap map;
            if (!_dependentMaps.TryGetValue(foreignKey, out map))
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

        public virtual IDependentsMap FindDependentsMap(IForeignKey foreignKey)
        {
            IDependentsMap map;
            return _dependentMaps != null
                   && _dependentMaps.TryGetValue(foreignKey, out map) 
                   ? map 
                   : null;
        }

        public virtual void Remove(InternalEntityEntry entry)
            => Remove(PrincipalKeyValueFactory.CreateFromCurrentValues(entry), entry);

        public virtual void RemoveUsingRelationshipSnapshot(InternalEntityEntry entry)
            => Remove(PrincipalKeyValueFactory.CreateFromRelationshipSnapshot(entry), entry);

        protected virtual void Remove([NotNull] TKey key, [NotNull] InternalEntityEntry entry)
        {
            _identityMap.Remove(key);

            if (_dependentMaps != null
                && _foreignKeys != null)
            {
                foreach (var foreignKey in _foreignKeys)
                {
                    IDependentsMap map;
                    if (_dependentMaps.TryGetValue(foreignKey, out map))
                    {
                        map.Remove(entry);
                    }
                }
            }
        }
    }
}
