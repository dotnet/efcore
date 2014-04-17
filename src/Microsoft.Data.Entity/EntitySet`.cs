// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
    public class EntitySet<TEntity> : EntitySet, IOrderedQueryable<TEntity>, IAsyncEnumerable<TEntity>
        where TEntity : class
    {
        private readonly EntityQueryable<TEntity> _entityQueryable;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected EntitySet()
        {
        }

        public EntitySet([NotNull] EntityContext context)
            : base(Check.NotNull(context, "context"))
        {
            _entityQueryable
                = new EntityQueryable<TEntity>(new EntityQueryExecutor(context));
        }

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator()
        {
            return _entityQueryable.GetAsyncEnumerator();
        }

        public virtual IEnumerator<TEntity> GetEnumerator()
        {
            return _entityQueryable.GetEnumerator();
        }

        IAsyncEnumerator IAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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

        public virtual TEntity Add([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return Context.Add(entity);
        }

        public virtual Task<TEntity> AddAsync(
            [NotNull] TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(entity, "entity");

            return Context.AddAsync(entity, cancellationToken);
        }

        public virtual TEntity Remove([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return Context.Delete(entity);
        }

        public virtual TEntity Update([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return Context.Update(entity);
        }

        public virtual Task<TEntity> UpdateAsync(
            [NotNull] TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(entity, "entity");

            return Context.UpdateAsync(entity, cancellationToken);
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
    }
}
