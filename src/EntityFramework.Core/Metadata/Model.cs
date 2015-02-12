// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Model : MetadataBase, IModel
    {
        // TODO: Perf: use a mutable structure before the model is made readonly
        // Issue #868
        private ImmutableSortedSet<EntityType> _entities
            = ImmutableSortedSet<EntityType>.Empty.WithComparer(new EntityTypeNameComparer());

        public virtual EntityType AddEntityType([NotNull] Type type)
        {
            Check.NotNull(type, "type");

            return AddEntityType(new EntityType(type, this));
        }

        public virtual EntityType AddEntityType([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            return AddEntityType(new EntityType(name, this));
        }

        private EntityType AddEntityType(EntityType entityType)
        {
            var previousLength = _entities.Count;
            _entities = _entities.Add(entityType);

            if (previousLength == _entities.Count)
            {
                throw new InvalidOperationException(Strings.DuplicateEntityType(entityType.Name));
            }

            return entityType;
        }

        public virtual EntityType GetOrAddEntityType([NotNull] Type type)
        {
            return TryGetEntityType(type) ?? AddEntityType(type);
        }

        public virtual EntityType GetOrAddEntityType([NotNull] string name)
        {
            return TryGetEntityType(name) ?? AddEntityType(name);
        }

        [CanBeNull]
        public virtual EntityType TryGetEntityType([NotNull] Type type)
        {
            Check.NotNull(type, "type");

            return type.GetTypeInfo().IsClass ? TryGetEntityType(new EntityType(type, this)) : null;
        }

        [CanBeNull]
        public virtual EntityType TryGetEntityType([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            return TryGetEntityType(new EntityType(name, this));
        }

        private EntityType TryGetEntityType(EntityType entityType)
        {
            return _entities.TryGetValue(entityType, out entityType)
                ? entityType
                : null;
        }

        public virtual EntityType GetEntityType([NotNull] Type type)
        {
            Check.NotNull(type, "type");

            var entityType = TryGetEntityType(type);
            if (entityType == null)
            {
                throw new ModelItemNotFoundException(Strings.EntityTypeNotFound(type.Name));
            }

            return entityType;
        }

        public virtual EntityType GetEntityType([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            var entityType = TryGetEntityType(name);
            if (entityType == null)
            {
                throw new ModelItemNotFoundException(Strings.EntityTypeNotFound(name));
            }

            return entityType;
        }

        public virtual EntityType RemoveEntityType([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            if (GetReferencingForeignKeys(entityType).Any())
            {
                throw new InvalidOperationException(Strings.EntityTypeInUse(entityType.Name));
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

        public virtual IReadOnlyList<EntityType> EntityTypes
        {
            get { return _entities; }
        }

        public virtual IReadOnlyList<ForeignKey> GetReferencingForeignKeys([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            // TODO: Perf: Add additional indexes so that this isn't a linear lookup
            // Issue #1179
            return EntityTypes.SelectMany(et => et.ForeignKeys).Where(fk => fk.ReferencedEntityType == entityType).ToList();
        }

        public virtual IReadOnlyList<ForeignKey> GetReferencingForeignKeys([NotNull] IKey key)
        {
            Check.NotNull(key, "key");

            // TODO: Perf: Add additional indexes so that this isn't a linear lookup
            // Issue #1179
            return EntityTypes.SelectMany(e => e.ForeignKeys).Where(fk => fk.ReferencedKey == key).ToList();
        }

        public virtual IReadOnlyList<ForeignKey> GetReferencingForeignKeys([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            // TODO: Perf: Add additional indexes so that this isn't a linear lookup
            // Issue #1179
            return EntityTypes.SelectMany(e => e.ForeignKeys.Where(f => f.ReferencedProperties.Contains(property))).ToList();
        }

        public virtual string StorageName { get; [param: CanBeNull] set; }

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

        IEnumerable<IForeignKey> IModel.GetReferencingForeignKeys(IEntityType entityType)
        {
            return GetReferencingForeignKeys(entityType);
        }

        IEnumerable<IForeignKey> IModel.GetReferencingForeignKeys(IKey key)
        {
            return GetReferencingForeignKeys(key);
        }

        IEnumerable<IForeignKey> IModel.GetReferencingForeignKeys(IProperty property)
        {
            return GetReferencingForeignKeys(property);
        }
    }
}
