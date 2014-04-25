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
    public class DbContext : IDisposable
    {
        private readonly LazyRef<ContextConfiguration> _configuration;
        private readonly ContextSets _sets = new ContextSets();

        protected DbContext()
        {
            InitializeSets(null, new EntityConfigurationBuilder().BuildConfiguration());
            _configuration = new LazyRef<ContextConfiguration>(() => Initialize(null, new EntityConfigurationBuilder()));
        }

        public DbContext([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            InitializeSets(serviceProvider, null);
            _configuration = new LazyRef<ContextConfiguration>(() => Initialize(serviceProvider, new EntityConfigurationBuilder()));
        }

        public DbContext([NotNull] EntityConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            InitializeSets(null, configuration);
            _configuration = new LazyRef<ContextConfiguration>(() => Initialize(null, configuration));
        }

        public DbContext([NotNull] IServiceProvider serviceProvider, [NotNull] EntityConfiguration configuration)
        {
            Check.NotNull(serviceProvider, "serviceProvider");
            Check.NotNull(configuration, "configuration");

            InitializeSets(serviceProvider, configuration);
            _configuration = new LazyRef<ContextConfiguration>(() => Initialize(serviceProvider, configuration));
        }

        private ContextConfiguration Initialize(IServiceProvider serviceProvider, EntityConfigurationBuilder builder)
        {
            OnConfiguring(builder);

            return Initialize(serviceProvider, builder.BuildConfiguration());
        }

        private ContextConfiguration Initialize(IServiceProvider serviceProvider, EntityConfiguration entityConfiguration)
        {
            var providerSource = serviceProvider != null 
                ? ContextConfiguration.ServiceProviderSource.Explicit 
                : ContextConfiguration.ServiceProviderSource.Implicit;

            serviceProvider = serviceProvider ?? ServiceProviderCache.Instance.GetOrAdd(entityConfiguration);

            var scopedProvider = serviceProvider
                .GetService<IServiceScopeFactory>()
                .CreateScope()
                .ServiceProvider;

            return scopedProvider
                .GetService<ContextConfiguration>()
                .Initialize(serviceProvider, scopedProvider, entityConfiguration, this, providerSource);
        }

        private void InitializeSets(IServiceProvider serviceProvider, EntityConfiguration entityConfiguration)
        {
            serviceProvider = serviceProvider ?? ServiceProviderCache.Instance.GetOrAdd(entityConfiguration);

            serviceProvider.GetRequiredService<DbSetInitializer>().InitializeSets(this);
        }

        public virtual ContextConfiguration Configuration
        {
            get { return _configuration.Value; }
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
            var stateManager = Configuration.Services.StateManager;

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

            await Configuration.Services.StateManager.GetOrCreateEntry(entity).SetEntityStateAsync(EntityState.Added, cancellationToken).ConfigureAwait(false);

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
            get { return new ChangeTracker(Configuration.Services.StateManager); }
        }

        public virtual IModel Model
        {
            get { return Configuration.Model; }
        }

        public virtual DbSet Set([NotNull] Type entityType)
        {
            Check.NotNull(entityType, "entityType");

            // Note: Creating sets needs to be fast because it is done eagerly when a context instance
            // is created so we avoid loading metadata to validate the type here.
            return _sets.GetSet(this, entityType);
        }

        public virtual DbSet<TEntity> Set<TEntity>()
            where TEntity : class
        {
            // Note: Creating sets needs to be fast because it is done eagerly when a context instance
            // is created so we avoid loading metadata to validate the type here.
            return _sets.GetSet<TEntity>(this);
        }
    }
}
