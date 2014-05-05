// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class DbContext : IDisposable
    {
        private readonly LazyRef<DbContextConfiguration> _configuration;
        private readonly ContextSets _sets = new ContextSets();

        private IServiceProvider _scopedServiceProvider;

        protected DbContext()
        {
            InitializeSets(null, new DbContextOptions().BuildConfiguration());
            _configuration = new LazyRef<DbContextConfiguration>(() => Initialize(null, new DbContextOptions()));
        }

        public DbContext([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            InitializeSets(serviceProvider, null);
            _configuration = new LazyRef<DbContextConfiguration>(() => Initialize(serviceProvider, new DbContextOptions()));
        }

        public DbContext([NotNull] ImmutableDbContextOptions configuration)
        {
            Check.NotNull(configuration, "configuration");

            InitializeSets(null, configuration);
            _configuration = new LazyRef<DbContextConfiguration>(() => Initialize(null, configuration));
        }

        public DbContext([NotNull] IServiceProvider serviceProvider, [NotNull] ImmutableDbContextOptions configuration)
        {
            Check.NotNull(serviceProvider, "serviceProvider");
            Check.NotNull(configuration, "configuration");

            InitializeSets(serviceProvider, configuration);
            _configuration = new LazyRef<DbContextConfiguration>(() => Initialize(serviceProvider, configuration));
        }

        private DbContextConfiguration Initialize(IServiceProvider serviceProvider, DbContextOptions builder)
        {
            OnConfiguring(builder);

            return Initialize(serviceProvider, builder.BuildConfiguration());
        }

        private DbContextConfiguration Initialize(IServiceProvider serviceProvider, ImmutableDbContextOptions contextOptions)
        {
            var providerSource = serviceProvider != null
                ? DbContextConfiguration.ServiceProviderSource.Explicit
                : DbContextConfiguration.ServiceProviderSource.Implicit;

            serviceProvider = serviceProvider ?? ServiceProviderCache.Instance.GetOrAdd(contextOptions);

            _scopedServiceProvider = serviceProvider
                .GetService<IServiceScopeFactory>()
                .CreateScope()
                .ServiceProvider;

            return _scopedServiceProvider
                .GetService<DbContextConfiguration>()
                .Initialize(serviceProvider, _scopedServiceProvider, contextOptions, this, providerSource);
        }

        private void InitializeSets(IServiceProvider serviceProvider, ImmutableDbContextOptions contextOptions)
        {
            serviceProvider = serviceProvider ?? ServiceProviderCache.Instance.GetOrAdd(contextOptions);

            serviceProvider.GetRequiredService<DbSetInitializer>().InitializeSets(this);
        }

        public virtual DbContextConfiguration Configuration
        {
            get { return _configuration.Value; }
        }

        protected internal virtual void OnConfiguring([NotNull] DbContextOptions builder)
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
            if (_scopedServiceProvider != null)
            {
                ((IDisposable)_scopedServiceProvider).Dispose();
            }
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
            get { return Configuration.Services.Database; }
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
