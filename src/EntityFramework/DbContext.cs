// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity
{
    public class DbContext : IDisposable
    {
        private static readonly ThreadSafeDictionaryCache<Type, Type> _optionsTypes = new ThreadSafeDictionaryCache<Type, Type>();

        private readonly LazyRef<DbContextConfiguration> _configuration;
        private readonly ContextSets _sets = new ContextSets();
        private readonly LazyRef<ILogger> _logger;

        private IServiceProvider _scopedServiceProvider;

        protected DbContext()
        {
            var options = new DbContextOptions();
            InitializeSets(null, options);
            _configuration = new LazyRef<DbContextConfiguration>(() => Initialize(null, options));
            _logger = new LazyRef<ILogger>(() => _configuration.Value.LoggerFactory.Create("DbContext"));
        }

        public DbContext([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            InitializeSets(serviceProvider, null);
            _configuration = new LazyRef<DbContextConfiguration>(
                () => Initialize(serviceProvider, GetOptions(serviceProvider)));

            _logger = new LazyRef<ILogger>(() => _configuration.Value.LoggerFactory.Create("DbContext"));
        }

        private DbContextOptions GetOptions(IServiceProvider serviceProvider)
        {
            var genericOptions = _optionsTypes.GetOrAdd(GetType(), t => typeof(DbContextOptions<>).MakeGenericType(t));

            return (DbContextOptions)serviceProvider.TryGetService(genericOptions)
                   ?? serviceProvider.TryGetService<DbContextOptions>()
                   ?? new DbContextOptions();
        }

        public DbContext([NotNull] DbContextOptions options)
        {
            Check.NotNull(options, "options");

            InitializeSets(null, options);
            _configuration = new LazyRef<DbContextConfiguration>(() => Initialize(null, options));
            _logger = new LazyRef<ILogger>(() => _configuration.Value.LoggerFactory.Create("DbContext"));
        }

        // TODO: Consider removing this constructor if DbContextOptions should be obtained from serviceProvider
        // Issue #192
        public DbContext([NotNull] IServiceProvider serviceProvider, [NotNull] DbContextOptions options)
        {
            Check.NotNull(serviceProvider, "serviceProvider");
            Check.NotNull(options, "options");

            InitializeSets(serviceProvider, options);
            _configuration = new LazyRef<DbContextConfiguration>(() => Initialize(serviceProvider, options));
            _logger = new LazyRef<ILogger>(() => _configuration.Value.LoggerFactory.Create("DbContext"));
        }

        private DbContextConfiguration Initialize(IServiceProvider serviceProvider, DbContextOptions options)
        {
            if (!options.IsLocked)
            {
                OnConfiguring(options);
            }
            options.Lock();

            var providerSource = serviceProvider != null
                ? DbContextConfiguration.ServiceProviderSource.Explicit
                : DbContextConfiguration.ServiceProviderSource.Implicit;

            serviceProvider = serviceProvider ?? ServiceProviderCache.Instance.GetOrAdd(options);

            _scopedServiceProvider = serviceProvider
                .GetService<IServiceScopeFactory>()
                .CreateScope()
                .ServiceProvider;

            return _scopedServiceProvider
                .GetService<DbContextConfiguration>()
                .Initialize(serviceProvider, _scopedServiceProvider, options, this, providerSource);
        }

        private void InitializeSets(IServiceProvider serviceProvider, DbContextOptions options)
        {
            serviceProvider = serviceProvider ?? ServiceProviderCache.Instance.GetOrAdd(options);

            serviceProvider.GetService<DbSetInitializer>().InitializeSets(this);
        }

        public virtual DbContextConfiguration Configuration
        {
            get { return _configuration.Value; }
        }

        protected internal virtual void OnConfiguring([NotNull] DbContextOptions options)
        {
        }

        protected internal virtual void OnModelCreating([NotNull] ModelBuilder modelBuilder)
        {
        }

        public virtual int SaveChanges()
        {
            var stateManager = Configuration.StateManager;

            // TODO: Allow auto-detect changes to be switched off
            // Issue #745
            Configuration.Services.ChangeDetector.DetectChanges(stateManager);

            try
            {
                return stateManager.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Value.Write(
                    TraceType.Error,
                    0,
                    new DataStoreErrorLogState(GetType()),
                    ex,
                    (state, exception) => string.Format("{0}" + Environment.NewLine + "{1}", Strings.LogExceptionDuringSaveChanges, exception.ToString()));

                throw;
            }
        }

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var stateManager = Configuration.StateManager;

            // TODO: Allow auto-detect changes to be switched off
            // Issue #745
            Configuration.Services.ChangeDetector.DetectChanges(stateManager);

            try
            {
                return await stateManager.SaveChangesAsync(cancellationToken).WithCurrentCulture();
            }
            catch (Exception ex)
            {
                _logger.Value.Write(
                    TraceType.Error, 
                    0, 
                    new DataStoreErrorLogState(GetType()), 
                    ex,
                    (state, exception) => string.Format("{0}" + Environment.NewLine + "{1}", Strings.LogExceptionDuringSaveChanges, exception.ToString()));

                throw;
            }
        }

        // TODO: Consider Framework Guidelines recommended dispose pattern
        // Issue #746
        public virtual void Dispose()
        {
            var disposableServiceProvider = _scopedServiceProvider as IDisposable;

            if (disposableServiceProvider != null)
            {
                disposableServiceProvider.Dispose();
            }
        }

        public virtual TEntity Add<TEntity>([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            Configuration.StateManager
                .GetOrCreateEntry(entity)
                .EntityState = EntityState.Added;

            return entity;
        }

        public virtual async Task<TEntity> AddAsync<TEntity>(
            [NotNull] TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(entity, "entity");

            await Configuration.StateManager
                .GetOrCreateEntry(entity)
                .SetEntityStateAsync(EntityState.Added, cancellationToken)
                .WithCurrentCulture();

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
            get { return Configuration.Database; }
        }

        public virtual ChangeTracker ChangeTracker
        {
            get { return new ChangeTracker(Configuration.StateManager, Configuration.Services.ChangeDetector); }
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
