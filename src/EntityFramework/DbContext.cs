// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.Data.Entity
{
    public class DbContext : IDisposable, IDbContextServices
    {
        private static readonly ThreadSafeDictionaryCache<Type, Type> _optionsTypes = new ThreadSafeDictionaryCache<Type, Type>();

        private readonly LazyRef<ContextServices> _configuration;
        private readonly LazyRef<ILogger> _logger;
        private readonly LazyRef<DbSetInitializer> _setInitializer;

        private bool _initializing;

        protected DbContext()
        {
            var serviceProvider = DbContextActivator.ServiceProvider;
            var options = GetOptions(serviceProvider);

            InitializeSets(serviceProvider, options);
            _configuration = new LazyRef<ContextServices>(() => Initialize(serviceProvider, options));
            _logger = new LazyRef<ILogger>(CreateLogger);
            _setInitializer = new LazyRef<DbSetInitializer>(GetSetInitializer);
        }

        public DbContext([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            var options = GetOptions(serviceProvider);

            InitializeSets(serviceProvider, options);
            _configuration = new LazyRef<ContextServices>(
                () => Initialize(serviceProvider, options));

            _logger = new LazyRef<ILogger>(CreateLogger);
            _setInitializer = new LazyRef<DbSetInitializer>(GetSetInitializer);
        }

        private DbContextOptions GetOptions(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                return new DbContextOptions();
            }

            var genericOptions = _optionsTypes.GetOrAdd(GetType(), t => typeof(DbContextOptions<>).MakeGenericType(t));

            var optionsAccessor = (IOptions<DbContextOptions>)serviceProvider.TryGetService(
                typeof(IOptions<>).MakeGenericType(genericOptions));
            if (optionsAccessor != null)
            {
                return optionsAccessor.Options;
            }

            optionsAccessor = serviceProvider.TryGetService<IOptions<DbContextOptions>>();
            if (optionsAccessor != null)
            {
                return optionsAccessor.Options;
            }

            var options = (DbContextOptions)serviceProvider.TryGetService(genericOptions);
            if (options != null)
            {
                return options;
            }

            options = serviceProvider.TryGetService<DbContextOptions>();
            if (options != null)
            {
                return options;
            }

            return new DbContextOptions();
        }

        public DbContext([NotNull] DbContextOptions options)
        {
            Check.NotNull(options, "options");

            var serviceProvider = DbContextActivator.ServiceProvider;

            InitializeSets(serviceProvider, options);
            _configuration = new LazyRef<ContextServices>(() => Initialize(serviceProvider, options));
            _logger = new LazyRef<ILogger>(CreateLogger);
            _setInitializer = new LazyRef<DbSetInitializer>(GetSetInitializer);
        }

        // TODO: Consider removing this constructor if DbContextOptions should be obtained from serviceProvider
        // Issue #192
        public DbContext([NotNull] IServiceProvider serviceProvider, [NotNull] DbContextOptions options)
        {
            Check.NotNull(serviceProvider, "serviceProvider");
            Check.NotNull(options, "options");

            InitializeSets(serviceProvider, options);
            _configuration = new LazyRef<ContextServices>(() => Initialize(serviceProvider, options));
            _logger = new LazyRef<ILogger>(CreateLogger);
            _setInitializer = new LazyRef<DbSetInitializer>(GetSetInitializer);
        }

        private ILogger CreateLogger()
        {
            return _configuration.Value.ScopedServiceProvider.GetRequiredServiceChecked<ILoggerFactory>().Create<DbContext>();
        }

        private DbSetInitializer GetSetInitializer()
        {
            return _configuration.Value.ScopedServiceProvider.GetRequiredServiceChecked<DbSetInitializer>();
        }

        private ChangeDetector GetChangeDetector()
        {
            return _configuration.Value.ScopedServiceProvider.GetRequiredServiceChecked<ChangeDetector>();
        }

        private StateManager GetStateManager()
        {
            return _configuration.Value.ScopedServiceProvider.GetRequiredServiceChecked<StateManager>();
        }

        private ContextServices Initialize(IServiceProvider serviceProvider, DbContextOptions options)
        {
            if (_initializing)
            {
                throw new InvalidOperationException(Strings.RecursiveOnConfiguring);
            }

            try
            {
                _initializing = true;

                options = options.Clone();

                OnConfiguring(options);

                var providerSource = serviceProvider != null
                    ? ContextServices.ServiceProviderSource.Explicit
                    : ContextServices.ServiceProviderSource.Implicit;

                serviceProvider = serviceProvider ?? ServiceProviderCache.Instance.GetOrAdd(options);

                var scopedServiceProvider = serviceProvider
                    .GetRequiredServiceChecked<IServiceScopeFactory>()
                    .CreateScope()
                    .ServiceProvider;

                return scopedServiceProvider
                    .GetRequiredServiceChecked<ContextServices>()
                    .Initialize(scopedServiceProvider, options, this, providerSource);
            }
            finally
            {
                _initializing = false;
            }
        }

        private void InitializeSets(IServiceProvider serviceProvider, DbContextOptions options)
        {
            serviceProvider = serviceProvider ?? ServiceProviderCache.Instance.GetOrAdd(options);

            serviceProvider.GetRequiredServiceChecked<DbSetInitializer>().InitializeSets(this);
        }

        IServiceProvider IDbContextServices.ScopedServiceProvider
        {
            get { return _configuration.Value.ScopedServiceProvider; }
        }

        protected internal virtual void OnConfiguring(DbContextOptions options)
        {
        }

        protected internal virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        public virtual int SaveChanges()
        {
            var stateManager = GetStateManager();

            // TODO: Allow auto-detect changes to be switched off
            // Issue #745
            GetChangeDetector().DetectChanges(stateManager);

            try
            {
                return stateManager.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Value.WriteError(
                    new DataStoreErrorLogState(GetType()),
                    ex,
                    (state, exception) =>
                        Strings.LogExceptionDuringSaveChanges(Environment.NewLine, exception));

                throw;
            }
        }

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var stateManager = GetStateManager();

            // TODO: Allow auto-detect changes to be switched off
            // Issue #745
            GetChangeDetector().DetectChanges(stateManager);

            try
            {
                return await stateManager.SaveChangesAsync(cancellationToken).WithCurrentCulture();
            }
            catch (Exception ex)
            {
                _logger.Value.WriteError(
                    new DataStoreErrorLogState(GetType()),
                    ex,
                    (state, exception) =>
                        Strings.LogExceptionDuringSaveChanges(Environment.NewLine, exception));

                throw;
            }
        }

        public virtual void Dispose()
        {
            if (_configuration.HasValue)
            {
                _configuration.Value.Dispose();
            }
        }

        public virtual TEntity Add<TEntity>([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            GetStateManager()
                .GetOrCreateEntry(entity)
                .EntityState = EntityState.Added;

            return entity;
        }

        public virtual async Task<TEntity> AddAsync<TEntity>(
            [NotNull] TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(entity, "entity");

            await GetStateManager()
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
            get { return _configuration.Value.ScopedServiceProvider.GetRequiredServiceChecked<ContextService<Database>>().Service; }
        }

        public virtual ChangeTracker ChangeTracker
        {
            get { return _configuration.Value.ScopedServiceProvider.GetRequiredServiceChecked<ChangeTracker>(); }
        }

        public virtual IModel Model
        {
            get { return _configuration.Value.ScopedServiceProvider.GetRequiredServiceChecked<ContextService<IModel>>().Service; }
        }

        public virtual DbSet<TEntity> Set<TEntity>()
            where TEntity : class
        {
            // Note: Creating sets needs to be fast because it is done eagerly when a context instance
            // is created so we avoid loading metadata to validate the type here.
            return _setInitializer.Value.CreateSet<TEntity>(this);
        }
    }
}
