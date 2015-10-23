// Copyright (c) .NET Foundation. All rights reserved.
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
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Utilities;

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
                    () => new EntityQueryable<TEntity>(_context.GetService<IAsyncQueryProvider>()));
        }

        public InternalDbSet([NotNull] IQueryable<TEntity> source, [NotNull] DbSet<TEntity> dbSet)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(dbSet, nameof(dbSet));

            _context = ((InternalDbSet<TEntity>)dbSet)._context;
            _entityQueryable = new LazyRef<EntityQueryable<TEntity>>(() => (EntityQueryable<TEntity>)source);
        }

        public override EntityEntry<TEntity> Add(TEntity entity, GraphBehavior behavior = GraphBehavior.IncludeDependents) 
            => _context.Add(Check.NotNull(entity, nameof(entity)), behavior);

        public override EntityEntry<TEntity> Attach(TEntity entity, GraphBehavior behavior = GraphBehavior.IncludeDependents) 
            => _context.Attach(Check.NotNull(entity, nameof(entity)), behavior);

        public override EntityEntry<TEntity> Remove(TEntity entity) 
            => _context.Remove(Check.NotNull(entity, nameof(entity)));

        public override EntityEntry<TEntity> Update(TEntity entity, GraphBehavior behavior = GraphBehavior.IncludeDependents) 
            => _context.Update(Check.NotNull(entity, nameof(entity)), behavior);

        public override void AddRange(params TEntity[] entities) 
            => _context.AddRange(Check.NotNull(entities, nameof(entities)));

        public override void AttachRange(params TEntity[] entities) 
            => _context.AttachRange(Check.NotNull(entities, nameof(entities)));

        public override void RemoveRange(params TEntity[] entities) 
            => _context.RemoveRange(Check.NotNull(entities, nameof(entities)));

        public override void UpdateRange(params TEntity[] entities) 
            => _context.UpdateRange(Check.NotNull(entities, nameof(entities)));

        public override void AddRange(IEnumerable<TEntity> entities, GraphBehavior behavior = GraphBehavior.IncludeDependents) 
            => _context.AddRange(Check.NotNull(entities, nameof(entities)), behavior);

        public override void AttachRange(IEnumerable<TEntity> entities, GraphBehavior behavior = GraphBehavior.IncludeDependents) 
            => _context.AttachRange(Check.NotNull(entities, nameof(entities)), behavior);

        public override void RemoveRange(IEnumerable<TEntity> entities) 
            => _context.RemoveRange(Check.NotNull(entities, nameof(entities)));

        public override void UpdateRange(IEnumerable<TEntity> entities, GraphBehavior behavior = GraphBehavior.IncludeDependents) 
            => _context.UpdateRange(Check.NotNull(entities, nameof(entities)), behavior);

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() => _entityQueryable.Value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _entityQueryable.Value.GetEnumerator();

        IAsyncEnumerable<TEntity> IAsyncEnumerableAccessor<TEntity>.AsyncEnumerable => _entityQueryable.Value;

        Type IQueryable.ElementType => _entityQueryable.Value.ElementType;

        Expression IQueryable.Expression => _entityQueryable.Value.Expression;

        IQueryProvider IQueryable.Provider => _entityQueryable.Value.Provider;

        IServiceProvider IAccessor<IServiceProvider>.Service => ((IAccessor<IServiceProvider>)_context).Service;
    }
}
