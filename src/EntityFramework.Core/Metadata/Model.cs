// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Model : Annotatable, IModel
    {
        // TODO: Perf: use a mutable structure before the model is made readonly
        // Issue #868
        private ImmutableSortedSet<EntityType> _entities
            = ImmutableSortedSet<EntityType>.Empty.WithComparer(new EntityTypeNameComparer());

        public virtual EntityType AddEntityType([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            return AddEntityType(new EntityType(type, this));
        }

        public virtual EntityType AddEntityType([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return AddEntityType(new EntityType(name, this));
        }

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
        {
            Check.NotNull(type, nameof(type));

            return type.GetTypeInfo().IsClass ? FindEntityType(new EntityType(type, this)) : null;
        }

        public virtual EntityType FindEntityType([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return FindEntityType(new EntityType(name, this));
        }

        private EntityType FindEntityType(EntityType entityType)
            => _entities.TryGetValue(entityType, out entityType)
                ? entityType
                : null;

        public virtual EntityType GetEntityType([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            var entityType = FindEntityType(type);
            if (entityType == null)
            {
                throw new ModelItemNotFoundException(CoreStrings.EntityTypeNotFound(type.Name));
            }

            return entityType;
        }

        public virtual EntityType GetEntityType([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var entityType = FindEntityType(name);
            if (entityType == null)
            {
                throw new ModelItemNotFoundException(CoreStrings.EntityTypeNotFound(name));
            }

            return entityType;
        }

        public virtual EntityType RemoveEntityType([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            if (FindReferencingForeignKeys(entityType).Any())
            {
                throw new InvalidOperationException(CoreStrings.EntityTypeInUse(entityType.Name));
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

        public virtual IReadOnlyList<EntityType> EntityTypes => _entities;

        public virtual IEnumerable<ForeignKey> FindReferencingForeignKeys([NotNull] EntityType entityType)
            => ((IModel)this).FindReferencingForeignKeys(entityType).Cast<ForeignKey>();

        public virtual IEnumerable<ForeignKey> FindReferencingForeignKeys([NotNull] Key key)
            => ((IModel)this).FindReferencingForeignKeys(key).Cast<ForeignKey>();

        public virtual IEnumerable<ForeignKey> FindReferencingForeignKeys([NotNull] Property property)
            => ((IModel)this).FindReferencingForeignKeys(property).Cast<ForeignKey>();

        public virtual string StorageName { get; [param: CanBeNull] set; }

        IEntityType IModel.FindEntityType(Type type) => FindEntityType(type);

        IEntityType IModel.GetEntityType(Type type) => GetEntityType(type);

        IEntityType IModel.FindEntityType(string name) => FindEntityType(name);

        IEntityType IModel.GetEntityType(string name) => GetEntityType(name);

        IReadOnlyList<IEntityType> IModel.EntityTypes => EntityTypes;
    }
}
