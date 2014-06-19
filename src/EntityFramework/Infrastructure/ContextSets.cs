// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class ContextSets
    {
        private static readonly DbSetSource _source = new DbSetSource();
        private readonly Dictionary<Type, DbSet> _sets = new Dictionary<Type, DbSet>();

        public virtual DbSet GetSet([NotNull] DbContext context, [NotNull] Type entityType)
        {
            Check.NotNull(context, "context");
            Check.NotNull(entityType, "entityType");

            DbSet set;
            if (!_sets.TryGetValue(entityType, out set))
            {
                set = _source.Create(context, entityType);
                _sets.Add(entityType, set);
            }
            return set;
        }

        public virtual DbSet<TEntity> GetSet<TEntity>([NotNull] DbContext context) where TEntity : class
        {
            Check.NotNull(context, "context");

            DbSet set;
            if (!_sets.TryGetValue(typeof(TEntity), out set))
            {
                set = _source.Create(context, typeof(TEntity));
                _sets.Add(typeof(TEntity), set);
            }
            return (DbSet<TEntity>)set;
        }
    }
}
