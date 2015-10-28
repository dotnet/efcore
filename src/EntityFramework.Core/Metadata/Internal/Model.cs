// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class Model : Annotatable, IMutableModel
    {
        private ImmutableSortedSet<EntityType> _entities
            = ImmutableSortedSet<EntityType>.Empty.WithComparer(new EntityTypeNameComparer());

        public virtual EntityType AddEntityType([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return AddEntityType(new EntityType(name, this));
        }

        public virtual EntityType AddEntityType([NotNull] Type type) => (EntityType)((IMutableModel)this).AddEntityType(type);

        private EntityType AddEntityType(EntityType entityType)
        {
            var previousLength = _entities.Count;
            _entities = _entities.Add(entityType);

            if (previousLength == _entities.Count)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateEntityType(entityType.Name));
            }

            return entityType;
        }

        public virtual EntityType GetOrAddEntityType([NotNull] Type type)
            => FindEntityType(type) ?? AddEntityType(type);

        public virtual EntityType GetOrAddEntityType([NotNull] string name)
            => FindEntityType(name) ?? AddEntityType(name);

        public virtual EntityType FindEntityType([NotNull] Type type)
            => (EntityType)((IMutableModel)this).FindEntityType(type);

        public virtual EntityType FindEntityType([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return FindEntityType(new EntityType(name, this));
        }

        private EntityType FindEntityType(EntityType entityType)
            => _entities.TryGetValue(entityType, out entityType)
                ? entityType
                : null;

        public virtual EntityType RemoveEntityType([NotNull] Type type)
        {
            var entityType = FindEntityType(type);
            return entityType == null
                ? null
                : RemoveEntityType(entityType);
        }

        public virtual EntityType RemoveEntityType([NotNull] string name)
        {
            var entityType = FindEntityType(name);
            return entityType == null
                ? null
                : RemoveEntityType(entityType);
        }

        private EntityType RemoveEntityType([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var referencingForeignKey = entityType.GetDeclaredReferencingForeignKeys().FirstOrDefault();
            if (referencingForeignKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeInUseByForeignKey(
                        entityType.DisplayName(),
                        Property.Format(referencingForeignKey.Properties),
                        referencingForeignKey.DeclaringEntityType.DisplayName()));
            }

            var derivedEntityType = entityType.GetDirectlyDerivedTypes().FirstOrDefault();
            if (derivedEntityType != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeInUseByDerived(
                        entityType.DisplayName(),
                        derivedEntityType.DisplayName()));
            }

            var previousEntities = _entities;
            _entities = _entities.Remove(entityType);

            EntityType removedEntityType = null;
            if (previousEntities.Count != _entities.Count)
            {
                previousEntities.TryGetValue(entityType, out removedEntityType);
            }

            return removedEntityType;
        }

        public virtual IReadOnlyList<EntityType> GetEntityTypes() => _entities;

        IEntityType IModel.FindEntityType(string name) => FindEntityType(name);
        IEnumerable<IEntityType> IModel.GetEntityTypes() => GetEntityTypes();

        IMutableEntityType IMutableModel.AddEntityType(string name) => AddEntityType(name);
        IReadOnlyList<IMutableEntityType> IMutableModel.GetEntityTypes() => GetEntityTypes();
        IMutableEntityType IMutableModel.FindEntityType(string name) => FindEntityType(name);
        IMutableEntityType IMutableModel.RemoveEntityType(string name) => RemoveEntityType(name);
    }
}
