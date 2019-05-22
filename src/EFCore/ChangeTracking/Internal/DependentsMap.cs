// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class DependentsMap<TKey> : IDependentsMap
    {
        private readonly IForeignKey _foreignKey;
        private readonly IPrincipalKeyValueFactory<TKey> _principalKeyValueFactory;
        private readonly IDependentKeyValueFactory<TKey> _dependentKeyValueFactory;
        private readonly Dictionary<TKey, HashSet<InternalEntityEntry>> _map;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<InternalEntityEntry> GetDependents(InternalEntityEntry principalEntry)
        {
            return _map.TryGetValue(_principalKeyValueFactory.CreateFromCurrentValues(principalEntry), out var dependents)
                ? dependents
                : Enumerable.Empty<InternalEntityEntry>();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<InternalEntityEntry> GetDependentsUsingRelationshipSnapshot(InternalEntityEntry principalEntry)
        {
            return _map.TryGetValue(_principalKeyValueFactory.CreateFromRelationshipSnapshot(principalEntry), out var dependents)
                ? dependents
                : Enumerable.Empty<InternalEntityEntry>();
        }
    }
}
