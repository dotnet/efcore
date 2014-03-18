// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class ContextEntitySets
    {
        private readonly EntityContext _context;
        private readonly EntitySetSource _source;
        private readonly Dictionary<Type, EntitySet> _sets = new Dictionary<Type, EntitySet>();

        public ContextEntitySets([NotNull] EntityContext context, [NotNull] EntitySetSource source)
        {
            Check.NotNull(context, "context");
            Check.NotNull(source, "source");

            _context = context;
            _source = source;
        }

        public virtual EntitySet GetEntitySet([NotNull] Type entityType)
        {
            Check.NotNull(entityType, "entityType");

            EntitySet entitySet;
            if (!_sets.TryGetValue(entityType, out entitySet))
            {
                entitySet = _source.Create(_context, entityType);
                _sets.Add(entityType, entitySet);
            }
            return entitySet;
        }

        public virtual EntitySet<TEntity> GetEntitySet<TEntity>() where TEntity : class
        {
            EntitySet entitySet;
            if (!_sets.TryGetValue(typeof(TEntity), out entitySet))
            {
                entitySet = new EntitySet<TEntity>(_context);
                _sets.Add(typeof(TEntity), entitySet);
            }
            return (EntitySet<TEntity>)entitySet;
        }
    }
}
