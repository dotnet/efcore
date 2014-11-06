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
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity
{
    public class DbSet<TEntity> : DbSet, IOrderedQueryable<TEntity>, IAsyncEnumerableAccessor<TEntity>
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
            // TODO: Decouple from DbContextConfiguration (Issue #641)
            _entityQueryable
                = new EntityQueryable<TEntity>(new EntityQueryExecutor(
                    context, 
                    new LazyRef<ILoggerFactory>(() => context.Configuration.Services.ServiceProvider.GetRequiredServiceChecked<ILoggerFactory>())));
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

        IAsyncEnumerable<TEntity> IAsyncEnumerableAccessor<TEntity>.AsyncEnumerable
        {
            get { return _entityQueryable; }
        }
    }
}
