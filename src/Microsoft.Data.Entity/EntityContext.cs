// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
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
            Initialize(null, new EntityConfigurationBuilder());
        }

        public EntityContext([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            Initialize(serviceProvider, new EntityConfigurationBuilder());
        }

        public EntityContext([NotNull] EntityConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            Initialize(null, configuration);
        }

        public EntityContext([NotNull] IServiceProvider serviceProvider, [NotNull] EntityConfiguration configuration)
        {
            Check.NotNull(serviceProvider, "serviceProvider");
            Check.NotNull(configuration, "configuration");

            Initialize(serviceProvider, configuration);
        }

        private void Initialize(IServiceProvider serviceProvider, EntityConfigurationBuilder builder)
        {
            // TODO: Make this lazy
            OnConfiguring(builder);

            Initialize(serviceProvider, builder.BuildConfiguration());
        }

        private void Initialize(IServiceProvider serviceProvider, EntityConfiguration entityConfiguration)
        {
            var providerSource = serviceProvider != null 
                ? ContextConfiguration.ServiceProviderSource.Explicit 
                : ContextConfiguration.ServiceProviderSource.Implicit;

            serviceProvider = serviceProvider ?? ServiceProviderCache.Instance.GetOrAdd(entityConfiguration);

            var scopedProvider = serviceProvider
                .GetService<IServiceScopeFactory>()
                .CreateScope()
                .ServiceProvider;

            _configuration = scopedProvider
                .GetService<ContextConfiguration>()
                .Initialize(scopedProvider, entityConfiguration, this, providerSource);

            // TODO: This bit not lazy
            _sets = _configuration.Services.ContextEntitySets;
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
            var stateManager = _configuration.Services.StateManager;

            // TODO: Allow auto-detect changes to be switched off
            stateManager.DetectChanges();

            // TODO: StateManager could get data store from config itself
            return stateManager.SaveChangesAsync(cancellationToken);
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

            await _configuration.Services.StateManager.GetOrCreateEntry(entity).SetEntityStateAsync(EntityState.Added, cancellationToken).ConfigureAwait(false);

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
            get { return new ChangeTracker(_configuration.Services.StateManager); }
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
