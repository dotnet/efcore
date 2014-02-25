// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Model : MetadataBase, IModel
    {
        private readonly LazyRef<ImmutableDictionary<Type, EntityType>> _entities
            = new LazyRef<ImmutableDictionary<Type, EntityType>>(() => ImmutableDictionary<Type, EntityType>.Empty);

        public virtual void AddEntityType([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _entities.ExchangeValue(d => d.Add(entityType.Type, entityType));
        }

        public virtual void RemoveEntityType([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _entities.ExchangeValue(l => l.Remove(entityType.Type));
        }

        public virtual EntityType EntityType([NotNull] object instance)
        {
            Check.NotNull(instance, "instance");

            return EntityType(instance.GetType());
        }

        public virtual EntityType EntityType([NotNull] Type type)
        {
            Check.NotNull(type, "type");

            EntityType value;
            return _entities.HasValue
                   && _entities.Value.TryGetValue(type, out value)
                ? value
                : null;
        }

        public virtual IEnumerable<EntityType> EntityTypes
        {
            get
            {
                return _entities.HasValue
                    ? _entities.Value.Values.OrderByOrdinal(e => e.Name)
                    : Enumerable.Empty<EntityType>();
            }
        }

        public virtual IEnumerable<IEntityType> TopologicalSort()
        {
            if (!_entities.HasValue)
            {
                return Enumerable.Empty<IEntityType>();
            }

            var sorted = new List<IEntityType>();
            var visiting = new HashSet<IEntityType>();
            var visited = new HashSet<IEntityType>();

            foreach (var entityType in EntityTypes)
            {
                TopologicalSortVisit(entityType, sorted, visiting, visited);
            }

            return sorted;
        }

        private static void TopologicalSortVisit(
            IEntityType entityType,
            ICollection<IEntityType> sorted,
            ISet<IEntityType> visiting,
            ISet<IEntityType> visited)
        {
            if (visiting.Contains(entityType)) // TODO: Support cycle-breaking in UP?
            {
                throw new InvalidOperationException(
                    Strings.CircularDependency(
                        visiting
                            .Concat(new[] { entityType })
                            .Select(et => et.Name).Join(" -> ")));
            }

            if (visited.Contains(entityType))
            {
                return;
            }

            visited.Add(entityType);
            visiting.Add(entityType);

            foreach (var predecessor in entityType.ForeignKeys.Select(fk => fk.ReferencedEntityType))
            {
                TopologicalSortVisit(predecessor, sorted, visiting, visited);
            }

            visiting.Remove(entityType);
            sorted.Add(entityType);
        }

        IEntityType IModel.EntityType(object instance)
        {
            return EntityType(instance);
        }

        IEntityType IModel.EntityType(Type type)
        {
            return EntityType(type);
        }

        IEnumerable<IEntityType> IModel.EntityTypes
        {
            get { return EntityTypes; }
        }
    }
}
