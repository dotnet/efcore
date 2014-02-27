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
        private readonly EntityConfiguration _configuration;
        private readonly LazyRef<IModel> _model;
        private readonly LazyRef<ChangeTracker> _changeTracker;

        // Intended only for creation of test doubles
        internal EntityContext()
        {
        }

        public EntityContext([NotNull] EntityConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            _configuration = configuration;
            _model = new LazyRef<IModel>(() => _configuration.Model ?? _configuration.ModelSource.GetModel(this));
            _changeTracker = new LazyRef<ChangeTracker>(() => _configuration.ChangeTrackerFactory.Create(_model.Value));

            _configuration.EntitySetInitializer.InitializeSets(this);
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

            return AddAsync(entity, CancellationToken.None).Result;
        }

        public virtual Task<TEntity> AddAsync<TEntity>([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return AddAsync(entity, CancellationToken.None);
        }

        public virtual async Task<TEntity> AddAsync<TEntity>([NotNull] TEntity entity, CancellationToken cancellationToken)
        {
            Check.NotNull(entity, "entity");

            await ChangeTracker.Entry(entity).Entry.SetEntityStateAsync(EntityState.Added, cancellationToken).ConfigureAwait(false);

            return entity;
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
            get { return _changeTracker.Value; }
        }

        public virtual IModel Model
        {
            get { return _model.Value; }
        }

        public virtual EntitySet<TEntity> Set<TEntity>() where TEntity : class
        {
            // TODO: Check that the type is actually in the model
            return new EntitySet<TEntity>(this);
        }
    }
}
