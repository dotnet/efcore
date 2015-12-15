// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
        
        public IdentityMap(
            [NotNull] IKey key,
            [NotNull] IPrincipalKeyValueFactory<TKey> principalKeyValueFactory)
        {
            Key = key;
            PrincipalKeyValueFactory = principalKeyValueFactory;
            _identityMap = new Dictionary<TKey, InternalEntityEntry>(principalKeyValueFactory.EqualityComparer);
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
            return GetDependentKeyValueFactory(foreignKey).TryCreateFromBuffer(valueBuffer, out key)
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
            return GetDependentKeyValueFactory(foreignKey).TryCreateFromCurrentValues(dependentEntry, out key)
                   && _identityMap.TryGetValue(key, out entry)
                ? entry
                : null;
        }

        public virtual InternalEntityEntry TryGetEntryUsingRelationshipSnapshot(IForeignKey foreignKey, InternalEntityEntry dependentEntry)
        {
            TKey key;
            InternalEntityEntry entry;
            return GetDependentKeyValueFactory(foreignKey).TryCreateFromRelationshipSnapshot(dependentEntry, out key)
                   && _identityMap.TryGetValue(key, out entry)
                ? entry
                : null;
        }

        public virtual void AddOrUpdate(InternalEntityEntry entry)
            => _identityMap[PrincipalKeyValueFactory.CreateFromCurrentValues(entry)] = entry;

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
                _identityMap[key] = entry;
            }
        }

        public virtual void Remove(InternalEntityEntry entry)
            => _identityMap.Remove(PrincipalKeyValueFactory.CreateFromCurrentValues(entry));

        public virtual void RemoveUsingRelationshipSnapshot(InternalEntityEntry entry)
            => _identityMap.Remove(PrincipalKeyValueFactory.CreateFromRelationshipSnapshot(entry));

        protected virtual void Remove([NotNull] TKey key)
            => _identityMap.Remove(key);

        private static IDependentKeyValueFactory<TKey> GetDependentKeyValueFactory(IForeignKey foreignKey)
        {
            var factorySource = foreignKey as IDependentKeyValueFactorySource;

            return factorySource != null
                ? (IDependentKeyValueFactory<TKey>)factorySource.DependentKeyValueFactory
                : new DependentKeyValueFactoryFactory().Create<TKey>(foreignKey);
        }
    }
}
