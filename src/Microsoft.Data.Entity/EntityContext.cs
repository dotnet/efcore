// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntityContext : IDisposable
    {
        private readonly EntityConfiguration _entityConfiguration;

        public EntityContext([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, "entityConfiguration");

            _entityConfiguration = entityConfiguration;
        }

        public virtual int SaveChanges()
        {
            // TODO
            return 0;
        }

        public virtual Task<int> SaveChangesAsync()
        {
            return SaveChangesAsync(CancellationToken.None);
        }

        public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            // TODO
            return Task.FromResult(0);
        }

        public void Dispose()
        {
            // TODO
        }

        public virtual TEntity Add<TEntity>([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            // TODO: Somewhere down this call path needs to do key generation
            ChangeTracker.Entry(entity).State = EntityState.Added;

            return entity;
        }

        public virtual Task<TEntity> AddAsync<TEntity>([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return AddAsync(entity, CancellationToken.None);
        }

        public virtual Task<TEntity> AddAsync<TEntity>([NotNull] TEntity entity, CancellationToken cancellationToken)
        {
            Check.NotNull(entity, "entity");

            // TODO: When key gen exists this will need to be really async
            return Task.FromResult(Add(entity));
        }

        public virtual TEntity Update<TEntity>([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            ChangeTracker.Entry(entity).State = EntityState.Modified;

            return entity;
        }

        public virtual Task<TEntity> UpdateAsync<TEntity>([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return UpdateAsync(entity, CancellationToken.None);
        }

        public virtual Task<TEntity> UpdateAsync<TEntity>([NotNull] TEntity entity, CancellationToken cancellationToken)
        {
            Check.NotNull(entity, "entity");

            return Task.FromResult(Update(entity));
        }

        public virtual Database Database
        {
            get { return new Database(); }
        }

        public virtual ChangeTracker ChangeTracker
        {
            get { return _entityConfiguration.ChangeTracker; }
        }

        public virtual IModel Model
        {
            get { return _entityConfiguration.Model; }
        }
    }
}
