// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DependentsMap<TKey> : IDependentsMap
    {
        private readonly IForeignKey _foreignKey;
        private readonly IPrincipalKeyValueFactory<TKey> _principalKeyValueFactory;
        private readonly IDependentKeyValueFactory<TKey> _dependentKeyValueFactory;
        private readonly Dictionary<TKey, HashSet<InternalEntityEntry>> _map;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DependentsMap(
            [NotNull] IForeignKey foreignKey,
            [NotNull] IPrincipalKeyValueFactory<TKey> principalKeyValueFactory,
            [NotNull] IDependentKeyValueFactory<TKey> dependentKeyValueFactory)
        {
            _foreignKey = foreignKey;
            _principalKeyValueFactory = principalKeyValueFactory;
            _dependentKeyValueFactory = dependentKeyValueFactory;
            _map = new Dictionary<TKey, HashSet<InternalEntityEntry>>(principalKeyValueFactory.EqualityComparer);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Add(InternalEntityEntry entry)
        {
            if (_foreignKey.DeclaringEntityType.IsAssignableFrom(entry.EntityType)
                && TryCreateFromCurrentValues(entry, out var key))
            {
                if (!_map.TryGetValue(key, out var dependents))
                {
                    dependents = new HashSet<InternalEntityEntry>();
                    _map[key] = dependents;
                }

                dependents.Add(entry);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Remove(InternalEntityEntry entry)
        {
            if (_foreignKey.DeclaringEntityType.IsAssignableFrom(entry.EntityType)
                && TryCreateFromCurrentValues(entry, out var key))
            {
                if (_map.TryGetValue(key, out var dependents))
                {
                    dependents.Remove(entry);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Update(InternalEntityEntry entry)
        {
            if (_foreignKey.DeclaringEntityType.IsAssignableFrom(entry.EntityType))
            {
                if (_dependentKeyValueFactory.TryCreateFromRelationshipSnapshot(entry, out var key)
                    && _map.TryGetValue(key, out var dependents))
                {
                    dependents.Remove(entry);
                }

                if (TryCreateFromCurrentValues(entry, out key))
                {
                    if (!_map.TryGetValue(key, out dependents))
                    {
                        dependents = new HashSet<InternalEntityEntry>();
                        _map[key] = dependents;
                    }

                    dependents.Add(entry);
                }
            }
        }

        private bool TryCreateFromCurrentValues(InternalEntityEntry entry, out TKey key)
        {
            // TODO: Move into delegate
            foreach (var property in _foreignKey.Properties)
            {
                if (entry.IsConceptualNull(property))
                {
                    key = default;
                    return false;
                }
            }

            return _dependentKeyValueFactory.TryCreateFromCurrentValues(entry, out key);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<InternalEntityEntry> GetDependents(InternalEntityEntry principalEntry)
        {
            return _map.TryGetValue(_principalKeyValueFactory.CreateFromCurrentValues(principalEntry), out var dependents)
                ? dependents
                : Enumerable.Empty<InternalEntityEntry>();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<InternalEntityEntry> GetDependentsUsingRelationshipSnapshot(InternalEntityEntry principalEntry)
        {
            return _map.TryGetValue(_principalKeyValueFactory.CreateFromRelationshipSnapshot(principalEntry), out var dependents)
                ? dependents
                : Enumerable.Empty<InternalEntityEntry>();
        }
    }
}
