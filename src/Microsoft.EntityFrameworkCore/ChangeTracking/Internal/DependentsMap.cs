// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class DependentsMap<TKey> : IDependentsMap
    {
        private readonly IEntityType _entityType;
        private readonly IPrincipalKeyValueFactory<TKey> _principalKeyValueFactory;
        private readonly IDependentKeyValueFactory<TKey> _dependentKeyValueFactory;
        private readonly Dictionary<TKey, List<InternalEntityEntry>> _map;

        public DependentsMap(
            [NotNull] IForeignKey foreignKey,
            [NotNull] IPrincipalKeyValueFactory<TKey> principalKeyValueFactory,
            [NotNull] IDependentKeyValueFactory<TKey> dependentKeyValueFactory)
        {
            _entityType = foreignKey.DeclaringEntityType;
            _principalKeyValueFactory = principalKeyValueFactory;
            _dependentKeyValueFactory = dependentKeyValueFactory;
            _map = new Dictionary<TKey, List<InternalEntityEntry>>(principalKeyValueFactory.EqualityComparer);
        }

        public virtual void Add(InternalEntityEntry entry)
        {
            TKey key;
            if (_entityType.IsAssignableFrom(entry.EntityType)
                && _dependentKeyValueFactory.TryCreateFromCurrentValues(entry, out key))
            {
                List<InternalEntityEntry> dependents;
                if (!_map.TryGetValue(key, out dependents))
                {
                    dependents = new List<InternalEntityEntry>();
                    _map[key] = dependents;
                }
                dependents.Add(entry);
            }
        }

        public virtual void Remove(InternalEntityEntry entry)
        {
            TKey key;
            if (_entityType.IsAssignableFrom(entry.EntityType)
                && _dependentKeyValueFactory.TryCreateFromCurrentValues(entry, out key))
            {
                List<InternalEntityEntry> dependents;
                if (_map.TryGetValue(key, out dependents))
                {
                    dependents.Remove(entry);
                }
            }
        }

        public virtual void Update(InternalEntityEntry entry)
        {
            if (_entityType.IsAssignableFrom(entry.EntityType))
            {
                TKey key;
                List<InternalEntityEntry> dependents;
                if (_dependentKeyValueFactory.TryCreateFromRelationshipSnapshot(entry, out key)
                    && _map.TryGetValue(key, out dependents))
                {
                    dependents.Remove(entry);
                }

                if (_dependentKeyValueFactory.TryCreateFromCurrentValues(entry, out key))
                {
                    if (!_map.TryGetValue(key, out dependents))
                    {
                        dependents = new List<InternalEntityEntry>();
                        _map[key] = dependents;
                    }
                    dependents.Add(entry);
                }
            }
        }

        public virtual IEnumerable<InternalEntityEntry> GetDependents(InternalEntityEntry principalEntry)
        {
            List<InternalEntityEntry> dependents;
            return _map.TryGetValue(_principalKeyValueFactory.CreateFromCurrentValues(principalEntry), out dependents)
                ? dependents
                : Enumerable.Empty<InternalEntityEntry>();
        }

        public virtual IEnumerable<InternalEntityEntry> GetDependentsUsingRelationshipSnapshot(InternalEntityEntry principalEntry)
        {
            List<InternalEntityEntry> dependents;
            return _map.TryGetValue(_principalKeyValueFactory.CreateFromRelationshipSnapshot(principalEntry), out dependents)
                ? dependents
                : Enumerable.Empty<InternalEntityEntry>();
        }
    }
}
