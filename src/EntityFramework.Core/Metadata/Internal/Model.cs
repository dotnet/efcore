// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class Model : ConventionalAnnotatable, IMutableModel
    {
        private readonly SortedDictionary<string, EntityType> _entities = new SortedDictionary<string, EntityType>();

        public virtual EntityType AddEntityType([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var entityType = new EntityType(name, this);
            var previousLength = _entities.Count;
            _entities[name] = entityType;

            if (previousLength == _entities.Count)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateEntityType(entityType.Name));
            }

            return entityType;
        }

        public virtual EntityType AddEntityType([NotNull] Type type) => (EntityType)((IMutableModel)this).AddEntityType(type);

        public virtual EntityType GetOrAddEntityType([NotNull] Type type)
            => FindEntityType(type) ?? AddEntityType(type);

        public virtual EntityType GetOrAddEntityType([NotNull] string name)
            => FindEntityType(name) ?? AddEntityType(name);

        public virtual EntityType FindEntityType([NotNull] Type type)
            => (EntityType)((IMutableModel)this).FindEntityType(type);

        public virtual EntityType FindEntityType([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            EntityType entityType;
            return _entities.TryGetValue(name, out entityType)
                ? entityType
                : null;
        }

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

            var removed = _entities.Remove(entityType.Name);
            Debug.Assert(removed);

            return entityType;
        }

        public virtual IEnumerable<EntityType> GetEntityTypes() => _entities.Values;

        IEntityType IModel.FindEntityType(string name) => FindEntityType(name);
        IEnumerable<IEntityType> IModel.GetEntityTypes() => GetEntityTypes();

        IMutableEntityType IMutableModel.AddEntityType(string name) => AddEntityType(name);
        IEnumerable<IMutableEntityType> IMutableModel.GetEntityTypes() => GetEntityTypes();
        IMutableEntityType IMutableModel.FindEntityType(string name) => FindEntityType(name);
        IMutableEntityType IMutableModel.RemoveEntityType(string name) => RemoveEntityType(name);
    }
}
