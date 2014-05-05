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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class DbSet<TEntity> : DbSet, IOrderedQueryable<TEntity>
        where TEntity : class
    {
        private readonly EntityQueryable<TEntity> _entityQueryable;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected DbSet()
        {
        }

        public DbSet([NotNull] DbContext context)
            : base(Check.NotNull(context, "context"))
        {
            _entityQueryable
                = new EntityQueryable<TEntity>(new EntityQueryExecutor(context));
        }

        public virtual TEntity Add([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return Configuration.Context.Add(entity);
        }

        public virtual Task<TEntity> AddAsync(
            [NotNull] TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(entity, "entity");

            return Configuration.Context.AddAsync(entity, cancellationToken);
        }

        public virtual TEntity Remove([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return Configuration.Context.Delete(entity);
        }

        public virtual TEntity Update([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return Configuration.Context.Update(entity);
        }

        public virtual Task<TEntity> UpdateAsync(
            [NotNull] TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(entity, "entity");

            return Configuration.Context.UpdateAsync(entity, cancellationToken);
        }

        public virtual void AddRange([NotNull] IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities, "entities");

            foreach (var entity in entities)
            {
                Add(entity);
            }
        }

        public virtual void RemoveRange([NotNull] IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities, "entities");

            foreach (var entity in entities)
            {
                Remove(entity);
            }
        }

        public virtual void UpdateRange([NotNull] IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities, "entities");

            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return _entityQueryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _entityQueryable.GetEnumerator();
        }

        public override Type ElementType
        {
            get { return _entityQueryable.ElementType; }
        }

        public override Expression Expression
        {
            get { return _entityQueryable.Expression; }
        }

        public override IQueryProvider Provider
        {
            get { return _entityQueryable.Provider; }
        }
    }
}
