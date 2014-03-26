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
    public class EntitySet<TEntity> : EntitySet, IQueryable<TEntity>
        where TEntity : class
    {
        private readonly IQueryProvider _provider;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected EntitySet()
        {
        }

        public EntitySet([NotNull] EntityContext context)
            : base(context)
        {
            _provider = new EnumerableQuery<TEntity>(this);
        }

        public virtual IEnumerator<TEntity> GetEnumerator()
        {
            // TODO Replace with real implementation
            return ExecuteQuery().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override Type ElementType
        {
            get { return typeof(TEntity); }
        }

        // TODO
        public override Expression Expression
        {
            get { return null; }
        }

        // TODO
        public override IQueryProvider Provider
        {
            get { return null; }
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

            // TODO
            return entity;
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

        Type IQueryable.ElementType
        {
            get
            {
                // TODO Replace with real implementation
                return typeof(TEntity);
            }
        }

        Expression IQueryable.Expression
        {
            get
            {
                // TODO Replace with real implementation
                return ExecuteQuery().AsQueryable().Expression;
            }
        }

        IQueryProvider IQueryable.Provider
        {
            get
            {
                // TODO Replace with real implementation
                return _provider;
            }
        }

        private IEnumerable<TEntity> ExecuteQuery()
        {
            return Context.Configuration.DataStore
                .Query<TEntity>(Context.Model, Context.Configuration.StateManager);
        }
    }
}
