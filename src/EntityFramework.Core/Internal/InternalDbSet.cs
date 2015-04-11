// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Internal
{
    public class InternalDbSet<TEntity>
        : DbSet<TEntity>, IOrderedQueryable<TEntity>, IAsyncEnumerableAccessor<TEntity>, IAccessor<IServiceProvider>
        where TEntity : class
    {
        private readonly DbContext _context;
        private readonly LazyRef<EntityQueryable<TEntity>> _entityQueryable;

        public InternalDbSet([NotNull] DbContext context)
        {
            Check.NotNull(context, nameof(context));

            _context = context;

            // Using context/service locator here so that the context will be initialized the first time the
            // set is used and services will be obtained from the correctly scoped container when this happens.
            _entityQueryable
                = new LazyRef<EntityQueryable<TEntity>>(
                    () => new EntityQueryable<TEntity>(
                        ((IAccessor<IServiceProvider>)_context).Service.GetRequiredService<IEntityQueryProvider>()));
        }

        public override EntityEntry<TEntity> Add(TEntity entity)
        {
            Check.NotNull(entity, nameof(entity));

            return _context.Add(entity);
        }

        public override EntityEntry<TEntity> Attach(TEntity entity)
        {
            Check.NotNull(entity, nameof(entity));

            return _context.Attach(entity);
        }

        public override EntityEntry<TEntity> Remove(TEntity entity)
        {
            Check.NotNull(entity, nameof(entity));

            return _context.Remove(entity);
        }

        public override EntityEntry<TEntity> Update(TEntity entity)
        {
            Check.NotNull(entity, nameof(entity));

            return _context.Update(entity);
        }

        public override void AddRange(params TEntity[] entities)
        {
            Check.NotNull(entities, nameof(entities));

            _context.AddRange(entities);
        }

        public override void AttachRange(params TEntity[] entities)
        {
            Check.NotNull(entities, nameof(entities));

            _context.AttachRange(entities);
        }

        public override void RemoveRange(params TEntity[] entities)
        {
            Check.NotNull(entities, nameof(entities));

            _context.RemoveRange(entities);
        }

        public override void UpdateRange(params TEntity[] entities)
        {
            Check.NotNull(entities, nameof(entities));

            _context.UpdateRange(entities);
        }

        public override void AddRange(IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities, nameof(entities));

            _context.AddRange(entities);
        }

        public override void AttachRange(IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities, nameof(entities));

            _context.AttachRange(entities);
        }

        public override void RemoveRange(IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities, nameof(entities));

            _context.RemoveRange(entities);
        }

        public override void UpdateRange(IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities, nameof(entities));

            _context.UpdateRange(entities);
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() => _entityQueryable.Value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _entityQueryable.Value.GetEnumerator();

        IAsyncEnumerable<TEntity> IAsyncEnumerableAccessor<TEntity>.AsyncEnumerable => _entityQueryable.Value;

        Type IQueryable.ElementType => _entityQueryable.Value.ElementType;

        Expression IQueryable.Expression => _entityQueryable.Value.Expression;

        IQueryProvider IQueryable.Provider => _entityQueryable.Value.Provider;

        IServiceProvider IAccessor<IServiceProvider>.Service => ((IAccessor<IServiceProvider>)_context).Service;
    }
}
