// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntitySet<TEntity> : IQueryable<TEntity>
        where TEntity : class
    {
        private readonly EntityContext _context;

        // Intended only for creation of test doubles
        internal EntitySet()
        {
        }

        public EntitySet([NotNull] EntityContext context)
        {
            Check.NotNull(context, "context");

            _context = context;
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            // TODO
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // TODO
        public Type ElementType
        {
            get { return null; }
        }

        // TODO
        public Expression Expression
        {
            get { return null; }
        }

        // TODO
        public IQueryProvider Provider
        {
            get { return null; }
        }

        public virtual TEntity Add([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return _context.Add(entity);
        }

        public virtual Task<TEntity> AddAsync([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return _context.AddAsync(entity, CancellationToken.None);
        }

        public virtual Task<TEntity> AddAsync([NotNull] TEntity entity, CancellationToken cancellationToken)
        {
            Check.NotNull(entity, "entity");

            return _context.AddAsync(entity, cancellationToken);
        }

        public virtual TEntity Remove([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            // TODO
            return entity;
        }

        public virtual TEntity Update([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return _context.Update(entity);
        }

        public virtual Task<TEntity> UpdateAsync([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return _context.UpdateAsync(entity, CancellationToken.None);
        }

        public virtual Task<TEntity> UpdateAsync([NotNull] TEntity entity, CancellationToken cancellationToken)
        {
            Check.NotNull(entity, "entity");

            return _context.UpdateAsync(entity, cancellationToken);
        }

        public virtual IEnumerable<TEntity> AddRange([NotNull] IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities, "entities");

            // TODO
            return entities;
        }

        public virtual IEnumerable<TEntity> RemoveRange([NotNull] IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities, "entities");

            // TODO
            return entities;
        }

        public virtual IEnumerable<TEntity> UpdateRange([NotNull] IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities, "entities");

            // TODO
            return entities;
        }
    }
}
