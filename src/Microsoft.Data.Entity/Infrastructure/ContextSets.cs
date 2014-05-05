// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
