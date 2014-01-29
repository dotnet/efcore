// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class EntitySet<TEntity> : IQueryable<TEntity>
        where TEntity : class
    {
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
        public Type ElementType { get; private set; }

        // TODO
        public Expression Expression { get; private set; }

        // TODO
        public IQueryProvider Provider { get; private set; }

        public virtual TEntity Add(TEntity entity)
        {
            // TODO
            return entity;
        }

        public virtual TEntity Remove(TEntity entity)
        {
            // TODO
            return entity;
        }

        public virtual TEntity Update(TEntity entity)
        {
            // TODO
            return entity;
        }

        public virtual IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities)
        {
            // TODO
            return entities;
        }

        public virtual IEnumerable<TEntity> RemoveRange(IEnumerable<TEntity> entities)
        {
            // TODO
            return entities;
        }

        public virtual IEnumerable<TEntity> UpdateRange(IEnumerable<TEntity> entities)
        {
            // TODO
            return entities;
        }
    }
}
