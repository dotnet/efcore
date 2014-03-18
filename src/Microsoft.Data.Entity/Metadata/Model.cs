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
        private readonly LazyRef<ImmutableSortedSet<EntityType>> _entities
            = new LazyRef<ImmutableSortedSet<EntityType>>(
                () => ImmutableSortedSet<EntityType>.Empty.WithComparer(new EntityTypeComparer()));

        public virtual void AddEntityType([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _entities.Value = _entities.Value.Add(entityType);
        }

        public virtual void RemoveEntityType([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _entities.Value = _entities.Value.Remove(entityType);
        }

        [CanBeNull]
        public virtual EntityType TryGetEntityType([NotNull] Type type)
        {
            Check.NotNull(type, "type");

            // TODO: with the same CLR type name in the same model
            EntityType entityType;
            return _entities.Value.TryGetValue(new EntityType(type), out entityType)
                ? entityType
                : null;
        }

        [NotNull]
        public virtual EntityType GetEntityType([NotNull] Type type)
        {
            Check.NotNull(type, "type");

            var entityType = TryGetEntityType(type);

            if (entityType == null)
            {
                throw new ModelItemNotFoundException(Strings.FormatEntityTypeNotFound(type.Name));
            }

            return entityType;
        }

        [CanBeNull]
        public virtual EntityType TryGetEntityType([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            EntityType entityType;
            return _entities.Value.TryGetValue(new EntityType(name), out entityType)
                ? entityType
                : null;
        }

        [NotNull]
        public virtual EntityType GetEntityType([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            var entityType = TryGetEntityType(name);

            if (entityType == null)
            {
                throw new ModelItemNotFoundException(Strings.FormatEntityTypeNotFound(name));
            }

            return entityType;
        }

        public virtual IReadOnlyList<EntityType> EntityTypes
        {
            get { return _entities.HasValue ? (IReadOnlyList<EntityType>)_entities.Value : ImmutableList<EntityType>.Empty; }
        }

        public virtual IReadOnlyList<IEntityType> TopologicalSort()
        {
            if (!_entities.HasValue)
            {
                return ImmutableList<EntityType>.Empty;
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
                    Strings.FormatCircularDependency(
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

        IEntityType IModel.TryGetEntityType(Type type)
        {
            return TryGetEntityType(type);
        }

        IEntityType IModel.GetEntityType(Type type)
        {
            return GetEntityType(type);
        }

        IEntityType IModel.TryGetEntityType(string name)
        {
            return TryGetEntityType(name);
        }

        IEntityType IModel.GetEntityType(string name)
        {
            return GetEntityType(name);
        }

        IReadOnlyList<IEntityType> IModel.EntityTypes
        {
            get { return EntityTypes; }
        }

        private class EntityTypeComparer : IComparer<EntityType>
        {
            public int Compare(EntityType x, EntityType y)
            {
                return StringComparer.Ordinal.Compare(x.Name, y.Name);
            }
        }
    }
}
