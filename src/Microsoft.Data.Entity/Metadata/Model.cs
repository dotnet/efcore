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
        private readonly LazyRef<ImmutableDictionary<Type, IEntityType>> _entities
            = new LazyRef<ImmutableDictionary<Type, IEntityType>>(() => ImmutableDictionary<Type, IEntityType>.Empty);

        public virtual void AddEntity([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _entities.ExchangeValue(d => d.Add(entityType.Type, entityType));
        }

        public virtual void RemoveEntity([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _entities.ExchangeValue(l => l.Remove(entityType.Type));
        }

        public virtual IEntityType Entity(object instance)
        {
            Check.NotNull(instance, "instance");

            return Entity(instance.GetType());
        }

        public virtual IEntityType Entity(Type type)
        {
            Check.NotNull(type, "type");

            IEntityType value;
            return _entities.HasValue
                   && _entities.Value.TryGetValue(type, out value)
                ? value
                : null;
        }

        public virtual IEnumerable<IEntityType> Entities
        {
            get
            {
                return _entities.HasValue
                    ? _entities.Value.Values.OrderByOrdinal(e => e.Name)
                    : Enumerable.Empty<IEntityType>();
            }
        }
    }
}
