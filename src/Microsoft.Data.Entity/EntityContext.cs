// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntityContext : IDisposable
    {
        private ContextConfiguration _configuration;
        private ContextEntitySets _sets;

        protected EntityContext()
        {
            Initialize(EntityConfigurationCache.Instance.GetOrAddConfiguration(this));
        }

        public EntityContext([NotNull] EntityConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            Initialize(configuration);
        }

        private void Initialize(EntityConfiguration configuration)
        {
            var scopedProvider = configuration
                .ServiceProvider.GetService<IServiceScopeFactory>()
                .CreateScope().ServiceProvider;

            _configuration = scopedProvider
                .GetService<ContextConfiguration>()
                .Initialize(scopedProvider, this);

            _sets = _configuration.ContextEntitySets;
            _sets.InitializeSets(this);
        }

        public virtual ContextConfiguration Configuration
        {
            get { return _configuration; }
        }

        protected internal virtual void OnConfiguring([NotNull] EntityConfigurationBuilder builder)
        {
        }

        protected internal virtual void OnModelCreating([NotNull] ModelBuilder builder)
        {
        }

        public virtual int SaveChanges()
        {
            // TODO: May need a parallel code path :-(
            return SaveChangesAsync().Result;
        }

        public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var stateManager = _configuration.StateManager;

            // TODO: Allow auto-detect changes to be switched off
            stateManager.DetectChanges();

            return stateManager.SaveChangesAsync(_configuration.DataStore, cancellationToken);
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

        public virtual async Task<TEntity> AddAsync<TEntity>(
            [NotNull] TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(entity, "entity");

            await _configuration.StateManager.GetOrCreateEntry(entity).SetEntityStateAsync(EntityState.Added, cancellationToken).ConfigureAwait(false);

            return entity;
        }

        public virtual TEntity Update<TEntity>([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            ChangeTracker.Entry(entity).State = EntityState.Modified;

            return entity;
        }

        public virtual Task<TEntity> UpdateAsync<TEntity>([NotNull] TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(entity, "entity");

            return Task.FromResult(Update(entity));
        }

        public virtual TEntity Delete<TEntity>([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            ChangeTracker.Entry(entity).State = EntityState.Deleted;

            return entity;
        }

        public virtual Database Database
        {
            get { return new Database(); }
        }

        public virtual ChangeTracker ChangeTracker
        {
            get { return new ChangeTracker(_configuration.StateManager); }
        }

        public virtual IModel Model
        {
            get { return _configuration.Model; }
        }

        public virtual EntitySet Set([NotNull] Type entityType)
        {
            Check.NotNull(entityType, "entityType");

            // Note: Creating sets needs to be fast because it is done eagerly when a context instance
            // is created so we avoid loading metadata to validate the type here.
            return _sets.GetEntitySet(this, entityType);
        }

        public virtual EntitySet<TEntity> Set<TEntity>()
            where TEntity : class
        {
            // Note: Creating sets needs to be fast because it is done eagerly when a context instance
            // is created so we avoid loading metadata to validate the type here.
            return _sets.GetEntitySet<TEntity>(this);
        }
    }
}
