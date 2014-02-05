// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Model : MetadataBase
    {
        private readonly LazyRef<ImmutableDictionary<Type, Entity>> _entities
            = new LazyRef<ImmutableDictionary<Type, Entity>>(() => ImmutableDictionary<Type, Entity>.Empty);

        public virtual void AddEntity(Entity entity)
        {
            Check.NotNull(entity, "entity");

            _entities.ExchangeValue(d => d.Add(entity.Type, entity));
        }

        public virtual void RemoveEntity(Entity entity)
        {
            Check.NotNull(entity, "entity");

            _entities.ExchangeValue(l => l.Remove(entity.Type));
        }

        public virtual Entity Entity(object @object)
        {
            Check.NotNull(@object, "object");

            return Entity(@object.GetType());
        }

        public virtual Entity Entity(Type type)
        {
            Check.NotNull(type, "type");

            Entity value;
            return _entities.HasValue
                   && _entities.Value.TryGetValue(type, out value)
                ? value
                : null;
        }

        public virtual IEnumerable<Entity> Entities
        {
            get
            {
                return _entities.HasValue
                    ? _entities.Value.Values.OrderByOrdinal(e => e.Name)
                    : Enumerable.Empty<Entity>();
            }
        }
    }
}
