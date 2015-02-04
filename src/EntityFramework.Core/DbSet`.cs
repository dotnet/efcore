// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Query;

namespace Microsoft.Data.Entity
{
    public abstract class DbSet<TEntity> : IOrderedQueryable<TEntity>, IAsyncEnumerableAccessor<TEntity>, IAccessor<IServiceProvider>
        where TEntity : class
    {
        public virtual EntityEntry<TEntity> Add([NotNull] TEntity entity)
        {
            throw new NotImplementedException();
        }

        public virtual EntityEntry<TEntity> Attach([NotNull] TEntity entity)
        {
            throw new NotImplementedException();
        }

        public virtual EntityEntry<TEntity> Remove([NotNull] TEntity entity)
        {
            throw new NotImplementedException();
        }

        public virtual EntityEntry<TEntity> Update([NotNull] TEntity entity)
        {
            throw new NotImplementedException();
        }

        public virtual void AddRange([NotNull] params TEntity[] entities)
        {
            throw new NotImplementedException();
        }

        public virtual void AttachRange([NotNull] params TEntity[] entities)
        {
            throw new NotImplementedException();
        }

        public virtual void RemoveRange([NotNull] params TEntity[] entities)
        {
            throw new NotImplementedException();
        }

        public virtual void UpdateRange([NotNull] params TEntity[] entities)
        {
            throw new NotImplementedException();
        }

        public virtual void AddRange([NotNull] IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        public virtual void AttachRange([NotNull] IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        public virtual void RemoveRange([NotNull] IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        public virtual void UpdateRange([NotNull] IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IAsyncEnumerable<TEntity> IAsyncEnumerableAccessor<TEntity>.AsyncEnumerable
        {
            get { throw new NotImplementedException(); }
        }

        Type IQueryable.ElementType
        {
            get { throw new NotImplementedException(); }
        }

        Expression IQueryable.Expression
        {
            get { throw new NotImplementedException(); }
        }

        IQueryProvider IQueryable.Provider
        {
            get { throw new NotImplementedException(); }
        }

        IServiceProvider IAccessor<IServiceProvider>.Service
        {
            get { throw new NotImplementedException(); }
        }
    }
}
