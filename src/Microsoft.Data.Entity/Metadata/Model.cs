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

        public virtual void AddEntity([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _entities.ExchangeValue(d => d.Add(entityType.Type, entityType));
        }

        public virtual void RemoveEntity([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _entities.ExchangeValue(l => l.Remove(entityType.Type));
        }

        public virtual EntityType Entity([NotNull] object instance)
        {
            Check.NotNull(instance, "instance");

            return Entity(instance.GetType());
        }

        public virtual EntityType Entity([NotNull] Type type)
        {
            Check.NotNull(type, "type");

            EntityType value;
            return _entities.HasValue
                   && _entities.Value.TryGetValue(type, out value)
                ? value
                : null;
        }

        public virtual IEnumerable<EntityType> Entities
        {
            get
            {
                return _entities.HasValue
                    ? _entities.Value.Values.OrderByOrdinal(e => e.Name)
                    : Enumerable.Empty<EntityType>();
            }
        }

        IEntityType IModel.Entity(object instance)
        {
            return Entity(instance);
        }

        IEntityType IModel.Entity(Type type)
        {
            return Entity(type);
        }

        IEnumerable<IEntityType> IModel.Entities
        {
            get { return Entities; }
        }
    }
}
