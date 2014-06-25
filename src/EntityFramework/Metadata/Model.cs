// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
                () => ImmutableSortedSet<EntityType>.Empty.WithComparer(new EntityTypeNameComparer()));

        public virtual void AddEntityType([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _entities.Value = _entities.Value.Add(entityType);
            entityType.Model = this;
        }

        public virtual void RemoveEntityType([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _entities.Value = _entities.Value.Remove(entityType);
            entityType.Model = null;
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

        public virtual IEnumerable<IForeignKey> GetReferencingForeignKeys(IEntityType entityType)
        {
            // TODO: Perf: Add additional indexes so that this isn't a linear lookup
            return EntityTypes.SelectMany(et => et.ForeignKeys).Where(fk => fk.ReferencedEntityType == entityType);
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
    }
}
