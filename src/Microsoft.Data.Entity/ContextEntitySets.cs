// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class ContextEntitySets
    {
        private static readonly EntitySetSource _source = new EntitySetSource();
        private readonly Dictionary<Type, EntitySet> _sets = new Dictionary<Type, EntitySet>();

        public virtual EntitySet GetEntitySet([NotNull] EntityContext context, [NotNull] Type entityType)
        {
            Check.NotNull(context, "context");
            Check.NotNull(entityType, "entityType");

            EntitySet entitySet;
            if (!_sets.TryGetValue(entityType, out entitySet))
            {
                entitySet = _source.Create(context, entityType);
                _sets.Add(entityType, entitySet);
            }
            return entitySet;
        }

        public virtual EntitySet<TEntity> GetEntitySet<TEntity>([NotNull] EntityContext context) where TEntity : class
        {
            Check.NotNull(context, "context");

            EntitySet entitySet;
            if (!_sets.TryGetValue(typeof(TEntity), out entitySet))
            {
                entitySet = _source.Create(context, typeof(TEntity));
                _sets.Add(typeof(TEntity), entitySet);
            }
            return (EntitySet<TEntity>)entitySet;
        }
    }
}
