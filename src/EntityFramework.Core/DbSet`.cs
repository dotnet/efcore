// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity
{
    public class DbSet<TEntity> : IOrderedQueryable<TEntity>, IAsyncEnumerableAccessor<TEntity>
        where TEntity : class
    {
        private readonly DbContext _context;
        private readonly LazyRef<EntityQueryable<TEntity>> _entityQueryable;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected DbSet()
        {
        }

        public DbSet([NotNull] DbContext context)
        {
            Check.NotNull(context, "context");

            _context = context;
            // Using context/service locator here so that the context will be initialized the first time the
            // set is used and services will be obtained from the correctly scoped container when this happens.
            _entityQueryable = new LazyRef<EntityQueryable<TEntity>>(
                () => new EntityQueryable<TEntity>(
                    ((IDbContextServices)_context).ScopedServiceProvider.GetRequiredServiceChecked<EntityQueryExecutor>()));
        }

        public virtual EntityEntry<TEntity> Add([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return _context.Add(entity);
        }

        public virtual Task<EntityEntry<TEntity>> AddAsync(
            [NotNull] TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(entity, "entity");

            return _context.AddAsync(entity, cancellationToken);
        }

        public virtual EntityEntry<TEntity> Attach([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return _context.Attach(entity);
        }

        public virtual EntityEntry<TEntity> Remove([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return _context.Remove(entity);
        }

        public virtual EntityEntry<TEntity> Update([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return _context.Update(entity);
        }

        public virtual IReadOnlyList<EntityEntry<TEntity>> Add([NotNull] params TEntity[] entities)
        {
            Check.NotNull(entities, "entities");

            return _context.Add(entities);
        }

        public virtual Task<IReadOnlyList<EntityEntry<TEntity>>> AddAsync([NotNull] params TEntity[] entities)
        {
            Check.NotNull(entities, "entities");

            return _context.AddAsync(entities);
        }

        public virtual Task<IReadOnlyList<EntityEntry<TEntity>>> AddAsync(
            CancellationToken cancellationToken, [NotNull] params TEntity[] entities)
        {
            Check.NotNull(entities, "entities");

            return _context.AddAsync(entities, cancellationToken);
        }

        public virtual IReadOnlyList<EntityEntry<TEntity>> Attach([NotNull] params TEntity[] entities)
        {
            Check.NotNull(entities, "entities");

            return _context.Attach(entities);
        }

        public virtual IReadOnlyList<EntityEntry<TEntity>> Remove([NotNull] params TEntity[] entities)
        {
            Check.NotNull(entities, "entities");

            return _context.Remove(entities);
        }

        public virtual IReadOnlyList<EntityEntry<TEntity>> Update([NotNull] params TEntity[] entities)
        {
            Check.NotNull(entities, "entities");

            return _context.Update(entities);
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return _entityQueryable.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _entityQueryable.Value.GetEnumerator();
        }

        public virtual Type ElementType
        {
            get { return _entityQueryable.Value.ElementType; }
        }

        public virtual Expression Expression
        {
            get { return _entityQueryable.Value.Expression; }
        }

        public virtual IQueryProvider Provider
        {
            get { return _entityQueryable.Value.Provider; }
        }

        IAsyncEnumerable<TEntity> IAsyncEnumerableAccessor<TEntity>.AsyncEnumerable
        {
            get { return _entityQueryable.Value; }
        }
    }
}
