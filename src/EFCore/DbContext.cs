// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     A DbContext instance represents a session with the database and can be used to query and save
    ///     instances of your entities. DbContext is a combination of the Unit Of Work and Repository patterns.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Typically you create a class that derives from DbContext and contains <see cref="DbSet{TEntity}" />
    ///         properties for each entity in the model. If the <see cref="DbSet{TEntity}" /> properties have a public setter,
    ///         they are automatically initialized when the instance of the derived context is created.
    ///     </para>
    ///     <para>
    ///         Override the <see cref="OnConfiguring(DbContextOptionsBuilder)" /> method to configure the database (and
    ///         other options) to be used for the context. Alternatively, if you would rather perform configuration externally
    ///         instead of inline in your context, you can use <see cref="DbContextOptionsBuilder{TContext}" />
    ///         (or <see cref="DbContextOptionsBuilder" />) to externally create an instance of <see cref="DbContextOptions{TContext}" />
    ///         (or <see cref="DbContextOptions" />) and pass it to a base constructor of <see cref="DbContext" />.
    ///     </para>
    ///     <para>
    ///         The model is discovered by running a set of conventions over the entity classes found in the
    ///         <see cref="DbSet{TEntity}" /> properties on the derived context. To further configure the model that
    ///         is discovered by convention, you can override the <see cref="OnModelCreating(ModelBuilder)" /> method.
    ///     </para>
    /// </remarks>
    public class DbContext :
        IDisposable,
        IAsyncDisposable,
        IInfrastructure<IServiceProvider>,
        IDbContextDependencies,
        IDbSetCache,
        IDbContextPoolable
    {
        private readonly DbContextOptions _options;

        private IDictionary<(Type Type, string? Name), object>? _sets;
        private IDbContextServices? _contextServices;
        private IDbContextDependencies? _dbContextDependencies;
        private DatabaseFacade? _database;
        private ChangeTracker? _changeTracker;

        private IServiceScope? _serviceScope;
        private DbContextLease _lease = DbContextLease.InactiveLease;
        private DbContextPoolConfigurationSnapshot? _configurationSnapshot;
        private List<IResettableService>? _cachedResettableServices;
        private bool _initializing;
        private bool _disposed;

        private readonly Guid _contextId = Guid.NewGuid();
        private int _leaseCount;

        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="DbContext" /> class. The
        ///         <see cref="OnConfiguring(DbContextOptionsBuilder)" />
        ///         method will be called to configure the database (and other options) to be used for this context.
        ///     </para>
        /// </summary>
        protected DbContext()
            : this(new DbContextOptions<DbContext>())
        {
        }

        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="DbContext" /> class using the specified options.
        ///         The <see cref="OnConfiguring(DbContextOptionsBuilder)" /> method will still be called to allow further
        ///         configuration of the options.
        ///     </para>
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public DbContext(DbContextOptions options)
        {
            Check.NotNull(options, nameof(options));

            if (!options.ContextType.IsAssignableFrom(GetType()))
            {
                throw new InvalidOperationException(CoreStrings.NonGenericOptions(GetType().ShortDisplayName()));
            }

            _options = options;

            // This service is not stored in _setInitializer as this may not be the service provider that will be used
            // as the internal service provider going forward, because at this time OnConfiguring has not yet been called.
            // Mostly that isn't a problem because set initialization is done by our internal services, but in the case
            // where some of those services are replaced, this could initialize set using non-replaced services.
            // In this rare case if this is a problem for the app, then the app can just not use this mechanism to create
            // DbSet instances, and this code becomes a no-op. However, if this set initializer is then saved and used later
            // for the Set method, then it makes the problem bigger because now an app is using the non-replaced services
            // even when it doesn't need to.
            ServiceProviderCache.Instance.GetOrAdd(options, providerRequired: false)
                .GetRequiredService<IDbSetInitializer>()
                .InitializeSets(this);

            EntityFrameworkEventSource.Log.DbContextInitializing();
        }

        /// <summary>
        ///     Provides access to database related information and operations for this context.
        /// </summary>
        public virtual DatabaseFacade Database
        {
            get
            {
                CheckDisposed();

                return _database ??= new DatabaseFacade(this);
            }
        }

        /// <summary>
        ///     Provides access to information and operations for entity instances this context is tracking.
        /// </summary>
        public virtual ChangeTracker ChangeTracker
            => _changeTracker ??= InternalServiceProvider.GetRequiredService<IChangeTrackerFactory>().Create();

        /// <summary>
        ///     The metadata about the shape of entities, the relationships between them, and how they map to the database.
        ///     May not include all the information necessary to initialize the database.
        /// </summary>
        public virtual IModel Model
        {
            [DebuggerStepThrough]
            get => ContextServices.Model;
        }

        /// <summary>
        ///     <para>
        ///         A unique identifier for the context instance and pool lease, if any.
        ///     </para>
        ///     <para>
        ///         This identifier is primarily intended as a correlation ID for logging and debugging such
        ///         that it is easy to identify that multiple events are using the same or different context instances.
        ///     </para>
        /// </summary>
        public virtual DbContextId ContextId
            => new(_contextId, _leaseCount);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        IDbSetSource IDbContextDependencies.SetSource
            => DbContextDependencies.SetSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        IEntityFinderFactory IDbContextDependencies.EntityFinderFactory
            => DbContextDependencies.EntityFinderFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        IAsyncQueryProvider IDbContextDependencies.QueryProvider
            => DbContextDependencies.QueryProvider;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        IStateManager IDbContextDependencies.StateManager
            => DbContextDependencies.StateManager;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        IChangeDetector IDbContextDependencies.ChangeDetector
            => DbContextDependencies.ChangeDetector;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        IEntityGraphAttacher IDbContextDependencies.EntityGraphAttacher
            => DbContextDependencies.EntityGraphAttacher;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        IDiagnosticsLogger<DbLoggerCategory.Update> IDbContextDependencies.UpdateLogger
            => DbContextDependencies.UpdateLogger;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        IDiagnosticsLogger<DbLoggerCategory.Infrastructure> IDbContextDependencies.InfrastructureLogger
            => DbContextDependencies.InfrastructureLogger;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        object IDbSetCache.GetOrAddSet(IDbSetSource source, Type type)
        {
            CheckDisposed();

            _sets ??= new Dictionary<(Type Type, string? Name), object>();

            if (!_sets.TryGetValue((type, null), out var set))
            {
                set = source.Create(this, type);
                _sets[(type, null)] = set;
                _cachedResettableServices = null;
            }

            return set;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        object IDbSetCache.GetOrAddSet(IDbSetSource source, string entityTypeName, Type type)
        {
            CheckDisposed();

            _sets ??= new Dictionary<(Type Type, string? Name), object>();

            if (!_sets.TryGetValue((type, entityTypeName), out var set))
            {
                set = source.Create(this, entityTypeName, type);
                _sets[(type, entityTypeName)] = set;
                _cachedResettableServices = null;
            }

            return set;
        }

        /// <summary>
        ///     Creates a <see cref="DbSet{TEntity}" /> that can be used to query and save instances of <typeparamref name="TEntity" />.
        /// </summary>
        /// <typeparam name="TEntity"> The type of entity for which a set should be returned. </typeparam>
        /// <returns> A set for the given entity type. </returns>
        public virtual DbSet<TEntity> Set<TEntity>()
            where TEntity : class
            => (DbSet<TEntity>)((IDbSetCache)this).GetOrAddSet(DbContextDependencies.SetSource, typeof(TEntity));

        /// <summary>
        ///     <para>
        ///         Creates a <see cref="DbSet{TEntity}" /> for a shared-type entity type that can be used to query and save
        ///         instances of <typeparamref name="TEntity" />.
        ///     </para>
        ///     <para>
        ///         Shared-type entity types are typically used for the join entity in many-to-many relationships.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name for the shared-type entity type to use. </param>
        /// <typeparam name="TEntity"> The type of entity for which a set should be returned. </typeparam>
        /// <returns> A set for the given entity type. </returns>
        public virtual DbSet<TEntity> Set<TEntity>(string name)
            where TEntity : class
            => (DbSet<TEntity>)((IDbSetCache)this).GetOrAddSet(DbContextDependencies.SetSource, name, typeof(TEntity));

        private IEntityFinder Finder(Type type)
        {
            var entityType = Model.FindEntityType(type);
            if (entityType == null)
            {
                if (Model.IsShared(type))
                {
                    throw new InvalidOperationException(CoreStrings.InvalidSetSharedType(type.ShortDisplayName()));
                }

                var findSameTypeName = Model.FindSameTypeNameWithDifferentNamespace(type);
                //if the same name exists in your entity types we will show you the full namespace of the type
                if (!string.IsNullOrEmpty(findSameTypeName))
                {
                    throw new InvalidOperationException(
                        CoreStrings.InvalidSetSameTypeWithDifferentNamespace(type.DisplayName(), findSameTypeName));
                }

                throw new InvalidOperationException(CoreStrings.InvalidSetType(type.ShortDisplayName()));
            }

            if (entityType.FindPrimaryKey() == null)
            {
                throw new InvalidOperationException(CoreStrings.InvalidSetKeylessOperation(type.ShortDisplayName()));
            }

            return DbContextDependencies.EntityFinderFactory.Create(entityType);
        }

        private IServiceProvider InternalServiceProvider
            => ContextServices.InternalServiceProvider;

        private IDbContextServices ContextServices
        {
            get
            {
                CheckDisposed();

                if (_contextServices != null)
                {
                    return _contextServices;
                }

                if (_initializing)
                {
                    throw new InvalidOperationException(CoreStrings.RecursiveOnConfiguring);
                }

                try
                {
                    _initializing = true;

                    var optionsBuilder = new DbContextOptionsBuilder(_options);

                    OnConfiguring(optionsBuilder);

                    if (_options.IsFrozen
                        && !ReferenceEquals(_options, optionsBuilder.Options))
                    {
                        throw new InvalidOperationException(CoreStrings.PoolingOptionsModified);
                    }

                    var options = optionsBuilder.Options;

                    _serviceScope = ServiceProviderCache.Instance.GetOrAdd(options, providerRequired: true)
                        .GetRequiredService<IServiceScopeFactory>()
                        .CreateScope();

                    var scopedServiceProvider = _serviceScope.ServiceProvider;

                    var contextServices = scopedServiceProvider.GetRequiredService<IDbContextServices>();

                    contextServices.Initialize(scopedServiceProvider, options, this);

                    _contextServices = contextServices;

                    DbContextDependencies.InfrastructureLogger.ContextInitialized(this, options);
                }
                finally
                {
                    _initializing = false;
                }

                return _contextServices;
            }
        }

        private IDbContextDependencies DbContextDependencies
        {
            [DebuggerStepThrough]
            get
            {
                CheckDisposed();

                return _dbContextDependencies ??= InternalServiceProvider.GetRequiredService<IDbContextDependencies>();
            }
        }

        [DebuggerStepThrough]
        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().ShortDisplayName(), CoreStrings.ContextDisposed);
            }
        }

        /// <summary>
        ///     <para>
        ///         Override this method to configure the database (and other options) to be used for this context.
        ///         This method is called for each instance of the context that is created.
        ///         The base implementation does nothing.
        ///     </para>
        ///     <para>
        ///         In situations where an instance of <see cref="DbContextOptions" /> may or may not have been passed
        ///         to the constructor, you can use <see cref="DbContextOptionsBuilder.IsConfigured" /> to determine if
        ///         the options have already been set, and skip some or all of the logic in
        ///         <see cref="OnConfiguring(DbContextOptionsBuilder)" />.
        ///     </para>
        /// </summary>
        /// <param name="optionsBuilder">
        ///     A builder used to create or modify options for this context. Databases (and other extensions)
        ///     typically define extension methods on this object that allow you to configure the context.
        /// </param>
        protected internal virtual void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        /// <summary>
        ///     Override this method to set defaults and configure conventions before they run. This method is invoked before
        ///     <see cref="OnModelCreating"/>.
        /// </summary>
        /// <remarks>
        ///     If a model is explicitly set on the options for this context (via <see cref="DbContextOptionsBuilder.UseModel(IModel)" />)
        ///     then this method will not be run.
        /// </remarks>
        /// <param name="configurationBuilder">
        ///     The builder being used to set defaults and configure conventions that will be used to build the model for this context.
        /// </param>
        protected internal virtual void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
        }

        /// <summary>
        ///     Override this method to further configure the model that was discovered by convention from the entity types
        ///     exposed in <see cref="DbSet{TEntity}" /> properties on your derived context. The resulting model may be cached
        ///     and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <remarks>
        ///     If a model is explicitly set on the options for this context (via <see cref="DbContextOptionsBuilder.UseModel(IModel)" />)
        ///     then this method will not be run.
        /// </remarks>
        /// <param name="modelBuilder">
        ///     The builder being used to construct the model for this context. Databases (and other extensions) typically
        ///     define extension methods on this object that allow you to configure aspects of the model that are specific
        ///     to a given database.
        /// </param>
        protected internal virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        /// <summary>
        ///     <para>
        ///         Saves all changes made in this context to the database.
        ///     </para>
        ///     <para>
        ///         This method will automatically call <see cref="ChangeTracker.DetectChanges" /> to discover any
        ///         changes to entity instances before saving to the underlying database. This can be disabled via
        ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     The number of state entries written to the database.
        /// </returns>
        /// <exception cref="DbUpdateException">
        ///     An error is encountered while saving to the database.
        /// </exception>
        /// <exception cref="DbUpdateConcurrencyException">
        ///     A concurrency violation is encountered while saving to the database.
        ///     A concurrency violation occurs when an unexpected number of rows are affected during save.
        ///     This is usually because the data in the database has been modified since it was loaded into memory.
        /// </exception>
        public virtual int SaveChanges()
            => SaveChanges(acceptAllChangesOnSuccess: true);

        /// <summary>
        ///     <para>
        ///         Saves all changes made in this context to the database.
        ///     </para>
        ///     <para>
        ///         This method will automatically call <see cref="ChangeTracker.DetectChanges" /> to discover any
        ///         changes to entity instances before saving to the underlying database. This can be disabled via
        ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
        ///     </para>
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">
        ///     Indicates whether <see cref="ChangeTracker.AcceptAllChanges" /> is called after the changes have
        ///     been sent successfully to the database.
        /// </param>
        /// <returns>
        ///     The number of state entries written to the database.
        /// </returns>
        /// <exception cref="DbUpdateException">
        ///     An error is encountered while saving to the database.
        /// </exception>
        /// <exception cref="DbUpdateConcurrencyException">
        ///     A concurrency violation is encountered while saving to the database.
        ///     A concurrency violation occurs when an unexpected number of rows are affected during save.
        ///     This is usually because the data in the database has been modified since it was loaded into memory.
        /// </exception>
        public virtual int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            CheckDisposed();

            SavingChanges?.Invoke(this, new SavingChangesEventArgs(acceptAllChangesOnSuccess));

            var interceptionResult = DbContextDependencies.UpdateLogger.SaveChangesStarting(this);

            TryDetectChanges();

            try
            {
                var entitiesSaved = interceptionResult.HasResult
                    ? interceptionResult.Result
                    : DbContextDependencies.StateManager.SaveChanges(acceptAllChangesOnSuccess);

                var result = DbContextDependencies.UpdateLogger.SaveChangesCompleted(this, entitiesSaved);

                SavedChanges?.Invoke(this, new SavedChangesEventArgs(acceptAllChangesOnSuccess, result));

                return result;
            }
            catch (DbUpdateConcurrencyException exception)
            {
                EntityFrameworkEventSource.Log.OptimisticConcurrencyFailure();

                DbContextDependencies.UpdateLogger.OptimisticConcurrencyException(this, exception);

                SaveChangesFailed?.Invoke(this, new SaveChangesFailedEventArgs(acceptAllChangesOnSuccess, exception));

                throw;
            }
            catch (Exception exception)
            {
                DbContextDependencies.UpdateLogger.SaveChangesFailed(this, exception);

                SaveChangesFailed?.Invoke(this, new SaveChangesFailedEventArgs(acceptAllChangesOnSuccess, exception));

                throw;
            }
        }

        private void TryDetectChanges()
        {
            if (ChangeTracker.AutoDetectChangesEnabled)
            {
                ChangeTracker.DetectChanges();
            }
        }

        private void TryDetectChanges(EntityEntry entry)
        {
            if (ChangeTracker.AutoDetectChangesEnabled)
            {
                entry.DetectChanges();
            }
        }

        /// <summary>
        ///     <para>
        ///         Saves all changes made in this context to the database.
        ///     </para>
        ///     <para>
        ///         This method will automatically call <see cref="ChangeTracker.DetectChanges" /> to discover any
        ///         changes to entity instances before saving to the underlying database. This can be disabled via
        ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
        ///     </para>
        ///     <para>
        ///         Multiple active operations on the same context instance are not supported.  Use <see langword="await" /> to ensure
        ///         that any asynchronous operations have completed before calling another method on this context.
        ///     </para>
        /// </summary>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains the
        ///     number of state entries written to the database.
        /// </returns>
        /// <exception cref="DbUpdateException">
        ///     An error is encountered while saving to the database.
        /// </exception>
        /// <exception cref="DbUpdateConcurrencyException">
        ///     A concurrency violation is encountered while saving to the database.
        ///     A concurrency violation occurs when an unexpected number of rows are affected during save.
        ///     This is usually because the data in the database has been modified since it was loaded into memory.
        /// </exception>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken" /> is canceled. </exception>
        public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken: cancellationToken);

        /// <summary>
        ///     <para>
        ///         Saves all changes made in this context to the database.
        ///     </para>
        ///     <para>
        ///         This method will automatically call <see cref="ChangeTracker.DetectChanges" /> to discover any
        ///         changes to entity instances before saving to the underlying database. This can be disabled via
        ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
        ///     </para>
        ///     <para>
        ///         Multiple active operations on the same context instance are not supported.  Use <see langword="await" /> to ensure
        ///         that any asynchronous operations have completed before calling another method on this context.
        ///     </para>
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">
        ///     Indicates whether <see cref="ChangeTracker.AcceptAllChanges" /> is called after the changes have
        ///     been sent successfully to the database.
        /// </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains the
        ///     number of state entries written to the database.
        /// </returns>
        /// <exception cref="DbUpdateException">
        ///     An error is encountered while saving to the database.
        /// </exception>
        /// <exception cref="DbUpdateConcurrencyException">
        ///     A concurrency violation is encountered while saving to the database.
        ///     A concurrency violation occurs when an unexpected number of rows are affected during save.
        ///     This is usually because the data in the database has been modified since it was loaded into memory.
        /// </exception>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken" /> is canceled. </exception>
        public virtual async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();

            SavingChanges?.Invoke(this, new SavingChangesEventArgs(acceptAllChangesOnSuccess));

            var interceptionResult = await DbContextDependencies.UpdateLogger
                .SaveChangesStartingAsync(this, cancellationToken).ConfigureAwait(acceptAllChangesOnSuccess);

            TryDetectChanges();

            try
            {
                var entitiesSaved = interceptionResult.HasResult
                    ? interceptionResult.Result
                    : await DbContextDependencies.StateManager
                        .SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken)
                        .ConfigureAwait(false);

                var result = await DbContextDependencies.UpdateLogger
                    .SaveChangesCompletedAsync(this, entitiesSaved, cancellationToken)
                    .ConfigureAwait(false);

                SavedChanges?.Invoke(this, new SavedChangesEventArgs(acceptAllChangesOnSuccess, result));

                return result;
            }
            catch (DbUpdateConcurrencyException exception)
            {
                EntityFrameworkEventSource.Log.OptimisticConcurrencyFailure();

                await DbContextDependencies.UpdateLogger.OptimisticConcurrencyExceptionAsync(this, exception, cancellationToken)
                    .ConfigureAwait(false);

                SaveChangesFailed?.Invoke(this, new SaveChangesFailedEventArgs(acceptAllChangesOnSuccess, exception));

                throw;
            }
            catch (Exception exception)
            {
                await DbContextDependencies.UpdateLogger.SaveChangesFailedAsync(this, exception, cancellationToken).ConfigureAwait(false);

                SaveChangesFailed?.Invoke(this, new SaveChangesFailedEventArgs(acceptAllChangesOnSuccess, exception));

                throw;
            }
        }

        /// <summary>
        ///     An event fired at the beginning of a call to <see cref="M:SaveChanges" /> or <see cref="M:SaveChangesAsync" />
        /// </summary>
        public event EventHandler<SavingChangesEventArgs>? SavingChanges;

        /// <summary>
        ///     An event fired at the end of a call to <see cref="M:SaveChanges" /> or <see cref="M:SaveChangesAsync" />
        /// </summary>
        public event EventHandler<SavedChangesEventArgs>? SavedChanges;

        /// <summary>
        ///     An event fired if a call to <see cref="M:SaveChanges" /> or <see cref="M:SaveChangesAsync" /> fails with an exception.
        /// </summary>
        public event EventHandler<SaveChangesFailedEventArgs>? SaveChangesFailed;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        void IDbContextPoolable.ClearLease()
            => _lease = DbContextLease.InactiveLease;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        void IDbContextPoolable.SetLease(DbContextLease lease)
        {
            _lease = lease;
            _disposed = false;
            ++_leaseCount;

            if (_configurationSnapshot?.AutoDetectChangesEnabled != null)
            {
                Check.DebugAssert(
                    _configurationSnapshot.QueryTrackingBehavior.HasValue, "!configurationSnapshot.QueryTrackingBehavior.HasValue");
                Check.DebugAssert(_configurationSnapshot.LazyLoadingEnabled.HasValue, "!configurationSnapshot.LazyLoadingEnabled.HasValue");
                Check.DebugAssert(
                    _configurationSnapshot.CascadeDeleteTiming.HasValue, "!configurationSnapshot.CascadeDeleteTiming.HasValue");
                Check.DebugAssert(
                    _configurationSnapshot.DeleteOrphansTiming.HasValue, "!configurationSnapshot.DeleteOrphansTiming.HasValue");

                var changeTracker = ChangeTracker;
                changeTracker.AutoDetectChangesEnabled = _configurationSnapshot.AutoDetectChangesEnabled.Value;
                changeTracker.QueryTrackingBehavior = _configurationSnapshot.QueryTrackingBehavior.Value;
                changeTracker.LazyLoadingEnabled = _configurationSnapshot.LazyLoadingEnabled.Value;
                changeTracker.CascadeDeleteTiming = _configurationSnapshot.CascadeDeleteTiming.Value;
                changeTracker.DeleteOrphansTiming = _configurationSnapshot.DeleteOrphansTiming.Value;
            }
            else
            {
                ((IResettableService?)_changeTracker)?.ResetState();
            }

            if (_database != null)
            {
                _database.AutoTransactionsEnabled
                    = _configurationSnapshot?.AutoTransactionsEnabled == null
                    || _configurationSnapshot.AutoTransactionsEnabled.Value;

                _database.AutoSavepointsEnabled
                    = _configurationSnapshot?.AutoSavepointsEnabled == null
                    || _configurationSnapshot.AutoSavepointsEnabled.Value;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        void IDbContextPoolable.SnapshotConfiguration()
            => _configurationSnapshot = new DbContextPoolConfigurationSnapshot(
                _changeTracker?.AutoDetectChangesEnabled,
                _changeTracker?.QueryTrackingBehavior,
                _database?.AutoTransactionsEnabled,
                _database?.AutoSavepointsEnabled,
                _changeTracker?.LazyLoadingEnabled,
                _changeTracker?.CascadeDeleteTiming,
                _changeTracker?.DeleteOrphansTiming);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        void IResettableService.ResetState()
        {
            foreach (var service in GetResettableServices())
            {
                service.ResetState();
            }

            ClearEvents();

            _disposed = true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        async Task IResettableService.ResetStateAsync(CancellationToken cancellationToken)
        {
            foreach (var service in GetResettableServices())
            {
                await service.ResetStateAsync(cancellationToken).ConfigureAwait(false);
            }

            ClearEvents();

            _disposed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<IResettableService> GetResettableServices()
        {
            if (_cachedResettableServices is not null)
            {
                return _cachedResettableServices;
            }

            var resettableServices = new List<IResettableService>();

            var services = _contextServices?.InternalServiceProvider.GetService<IEnumerable<IResettableService>>();
            if (services is not null)
            {
                resettableServices.AddRange(services);

                // Note that if the context hasn't been initialized yet, we don't cache the resettable services
                // (since some services haven't been added yet).
                _cachedResettableServices = resettableServices;
            }

            if (_sets is not null)
            {
                resettableServices.AddRange(_sets.Values.OfType<IResettableService>());
            }

            return resettableServices;
        }

        /// <summary>
        ///     Releases the allocated resources for this context.
        /// </summary>
        public virtual void Dispose()
        {
            if (DisposeSync())
            {
                _serviceScope?.Dispose();
            }
        }

        private bool DisposeSync()
        {
            if (_lease.IsActive)
            {
                if (_lease.ContextDisposed())
                {
                    _disposed = true;

                    ClearEvents();

                    _lease = DbContextLease.InactiveLease;
                }
            }
            else if (!_disposed)
            {
                EntityFrameworkEventSource.Log.DbContextDisposing();

                _dbContextDependencies?.InfrastructureLogger.ContextDisposed(this);

                _disposed = true;

                _dbContextDependencies?.StateManager.Unsubscribe();

                _dbContextDependencies = null;
                _changeTracker = null;
                _database = null;

                ClearEvents();

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Releases the allocated resources for this context.
        /// </summary>
        public virtual ValueTask DisposeAsync()
            => DisposeSync() ? _serviceScope.DisposeAsyncIfAvailable() : default;

        private void ClearEvents()
        {
            SavingChanges = null;
            SavedChanges = null;
            SaveChangesFailed = null;
        }

        /// <summary>
        ///     Gets an <see cref="EntityEntry{TEntity}" /> for the given entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity to get the entry for. </param>
        /// <returns> The entry for the given entity. </returns>
        public virtual EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
            where TEntity : class
        {
            Check.NotNull(entity, nameof(entity));
            CheckDisposed();

            var entry = EntryWithoutDetectChanges(entity);

            TryDetectChanges(entry);

            return entry;
        }

        private EntityEntry<TEntity> EntryWithoutDetectChanges<TEntity>(TEntity entity)
            where TEntity : class
            => new(DbContextDependencies.StateManager.GetOrCreateEntry(entity));

        /// <summary>
        ///     <para>
        ///         Gets an <see cref="EntityEntry" /> for the given entity. The entry provides
        ///         access to change tracking information and operations for the entity.
        ///     </para>
        ///     <para>
        ///         This method may be called on an entity that is not tracked. You can then
        ///         set the <see cref="EntityEntry.State" /> property on the returned entry
        ///         to have the context begin tracking the entity in the specified state.
        ///     </para>
        /// </summary>
        /// <param name="entity"> The entity to get the entry for. </param>
        /// <returns> The entry for the given entity. </returns>
        public virtual EntityEntry Entry(object entity)
        {
            Check.NotNull(entity, nameof(entity));
            CheckDisposed();

            var entry = EntryWithoutDetectChanges(entity);

            TryDetectChanges(entry);

            return entry;
        }

        private EntityEntry EntryWithoutDetectChanges(object entity)
            => new(DbContextDependencies.StateManager.GetOrCreateEntry(entity));

        private void SetEntityState(InternalEntityEntry entry, EntityState entityState)
        {
            if (entry.EntityState == EntityState.Detached)
            {
                DbContextDependencies.EntityGraphAttacher.AttachGraph(
                    entry,
                    entityState,
                    entityState,
                    forceStateWhenUnknownKey: true);
            }
            else
            {
                entry.SetEntityState(
                    entityState,
                    acceptChanges: true,
                    forceStateWhenUnknownKey: entityState);
            }
        }

        private Task SetEntityStateAsync(
            InternalEntityEntry entry,
            EntityState entityState,
            CancellationToken cancellationToken)
        {
            return entry.EntityState == EntityState.Detached
                ? DbContextDependencies.EntityGraphAttacher.AttachGraphAsync(
                    entry,
                    entityState,
                    entityState,
                    forceStateWhenUnknownKey: true,
                    cancellationToken: cancellationToken)
                : entry.SetEntityStateAsync(
                    entityState,
                    acceptChanges: true,
                    forceStateWhenUnknownKey: entityState,
                    cancellationToken: cancellationToken);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity, and any other reachable entities that are
        ///         not already being tracked, in the <see cref="EntityState.Added" /> state such that
        ///         they will be inserted into the database when <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity to add. </param>
        /// <returns>
        ///     The <see cref="EntityEntry{TEntity}" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public virtual EntityEntry<TEntity> Add<TEntity>(TEntity entity)
            where TEntity : class
        {
            CheckDisposed();

            return SetEntityState(Check.NotNull(entity, nameof(entity)), EntityState.Added);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity, and any other reachable entities that are
        ///         not already being tracked, in the <see cref="EntityState.Added" /> state such that they will
        ///         be inserted into the database when <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         This method is async only to allow special value generators, such as the one used by
        ///         'Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.SequenceHiLo',
        ///         to access the database asynchronously. For all other cases the non async method should be used.
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity to add. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous Add operation. The task result contains the
        ///     <see cref="EntityEntry{TEntity}" /> for the entity. The entry provides access to change tracking
        ///     information and operations for the entity.
        /// </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken" /> is canceled. </exception>
        public virtual async ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : class
        {
            CheckDisposed();

            var entry = EntryWithoutDetectChanges(Check.NotNull(entity, nameof(entity)));

            await SetEntityStateAsync(entry.GetInfrastructure(), EntityState.Added, cancellationToken)
                .ConfigureAwait(false);

            return entry;
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity and entries reachable from the given entity using
        ///         the <see cref="EntityState.Unchanged" /> state by default, but see below for cases
        ///         when a different state will be used.
        ///     </para>
        ///     <para>
        ///         Generally, no database interaction will be performed until <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. All entities found will be tracked
        ///         by the context.
        ///     </para>
        ///     <para>
        ///         For entity types with generated keys if an entity has its primary key value set
        ///         then it will be tracked in the <see cref="EntityState.Unchanged" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="EntityState.Added" /> state.
        ///         This helps ensure only new entities will be inserted.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        ///     <para>
        ///         For entity types without generated keys, the state set is always <see cref="EntityState.Unchanged" />.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity to attach. </param>
        /// <returns>
        ///     The <see cref="EntityEntry{TEntity}" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public virtual EntityEntry<TEntity> Attach<TEntity>(TEntity entity)
            where TEntity : class
        {
            CheckDisposed();

            return SetEntityState(Check.NotNull(entity, nameof(entity)), EntityState.Unchanged);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity and entries reachable from the given entity using
        ///         the <see cref="EntityState.Modified" /> state by default, but see below for cases
        ///         when a different state will be used.
        ///     </para>
        ///     <para>
        ///         Generally, no database interaction will be performed until <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. All entities found will be tracked
        ///         by the context.
        ///     </para>
        ///     <para>
        ///         For entity types with generated keys if an entity has its primary key value set
        ///         then it will be tracked in the <see cref="EntityState.Modified" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="EntityState.Added" /> state.
        ///         This helps ensure new entities will be inserted, while existing entities will be updated.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        ///     <para>
        ///         For entity types without generated keys, the state set is always <see cref="EntityState.Modified" />.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity to update. </param>
        /// <returns>
        ///     The <see cref="EntityEntry{TEntity}" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public virtual EntityEntry<TEntity> Update<TEntity>(TEntity entity)
            where TEntity : class
        {
            CheckDisposed();

            return SetEntityState(Check.NotNull(entity, nameof(entity)), EntityState.Modified);
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If the entity is already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking the entity (rather than marking it as <see cref="EntityState.Deleted" />) since the
        ///         entity was previously added to the context and does not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="Attach{TEntity}(TEntity)" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </remarks>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity to remove. </param>
        /// <returns>
        ///     The <see cref="EntityEntry{TEntity}" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public virtual EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
            where TEntity : class
        {
            Check.NotNull(entity, nameof(entity));
            CheckDisposed();

            var entry = EntryWithoutDetectChanges(entity);

            var initialState = entry.State;
            if (initialState == EntityState.Detached)
            {
                SetEntityState(entry.GetInfrastructure(), EntityState.Unchanged);
            }

            // An Added entity does not yet exist in the database. If it is then marked as deleted there is
            // nothing to delete because it was not yet inserted, so just make sure it doesn't get inserted.
            entry.State =
                initialState == EntityState.Added
                    ? EntityState.Detached
                    : EntityState.Deleted;

            return entry;
        }

        private EntityEntry<TEntity> SetEntityState<TEntity>(
            TEntity entity,
            EntityState entityState)
            where TEntity : class
        {
            var entry = EntryWithoutDetectChanges(entity);

            SetEntityState(entry.GetInfrastructure(), entityState);

            return entry;
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity, and any other reachable entities that are
        ///         not already being tracked, in the <see cref="EntityState.Added" /> state such that they will
        ///         be inserted into the database when <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <param name="entity"> The entity to add. </param>
        /// <returns>
        ///     The <see cref="EntityEntry" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public virtual EntityEntry Add(object entity)
        {
            CheckDisposed();

            return SetEntityState(Check.NotNull(entity, nameof(entity)), EntityState.Added);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity, and any other reachable entities that are
        ///         not already being tracked, in the <see cref="EntityState.Added" /> state such that they will
        ///         be inserted into the database when <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        ///     <para>
        ///         This method is async only to allow special value generators, such as the one used by
        ///         'Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.SequenceHiLo',
        ///         to access the database asynchronously. For all other cases the non async method should be used.
        ///     </para>
        /// </summary>
        /// <param name="entity"> The entity to add. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous Add operation. The task result contains the
        ///     <see cref="EntityEntry" /> for the entity. The entry provides access to change tracking
        ///     information and operations for the entity.
        /// </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken" /> is canceled. </exception>
        public virtual async ValueTask<EntityEntry> AddAsync(
            object entity,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();

            var entry = EntryWithoutDetectChanges(Check.NotNull(entity, nameof(entity)));

            await SetEntityStateAsync(entry.GetInfrastructure(), EntityState.Added, cancellationToken)
                .ConfigureAwait(false);

            return entry;
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity and entries reachable from the given entity using
        ///         the <see cref="EntityState.Unchanged" /> state by default, but see below for cases
        ///         when a different state will be used.
        ///     </para>
        ///     <para>
        ///         Generally, no database interaction will be performed until <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. All entities found will be tracked
        ///         by the context.
        ///     </para>
        ///     <para>
        ///         For entity types with generated keys if an entity has its primary key value set
        ///         then it will be tracked in the <see cref="EntityState.Unchanged" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="EntityState.Added" /> state.
        ///         This helps ensure only new entities will be inserted.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        ///     <para>
        ///         For entity types without generated keys, the state set is always <see cref="EntityState.Unchanged" />.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <param name="entity"> The entity to attach. </param>
        /// <returns>
        ///     The <see cref="EntityEntry" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public virtual EntityEntry Attach(object entity)
        {
            CheckDisposed();

            return SetEntityState(Check.NotNull(entity, nameof(entity)), EntityState.Unchanged);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity and entries reachable from the given entity using
        ///         the <see cref="EntityState.Modified" /> state by default, but see below for cases
        ///         when a different state will be used.
        ///     </para>
        ///     <para>
        ///         Generally, no database interaction will be performed until <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. All entities found will be tracked
        ///         by the context.
        ///     </para>
        ///     <para>
        ///         For entity types with generated keys if an entity has its primary key value set
        ///         then it will be tracked in the <see cref="EntityState.Modified" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="EntityState.Added" /> state.
        ///         This helps ensure new entities will be inserted, while existing entities will be updated.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        ///     <para>
        ///         For entity types without generated keys, the state set is always <see cref="EntityState.Modified" />.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <param name="entity"> The entity to update. </param>
        /// <returns>
        ///     The <see cref="EntityEntry" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public virtual EntityEntry Update(object entity)
        {
            CheckDisposed();

            return SetEntityState(Check.NotNull(entity, nameof(entity)), EntityState.Modified);
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If the entity is already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking the entity (rather than marking it as <see cref="EntityState.Deleted" />) since the
        ///         entity was previously added to the context and does not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="Attach(object)" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </remarks>
        /// <param name="entity"> The entity to remove. </param>
        /// <returns>
        ///     The <see cref="EntityEntry" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public virtual EntityEntry Remove(object entity)
        {
            Check.NotNull(entity, nameof(entity));
            CheckDisposed();

            var entry = EntryWithoutDetectChanges(entity);

            var initialState = entry.State;
            if (initialState == EntityState.Detached)
            {
                SetEntityState(entry.GetInfrastructure(), EntityState.Unchanged);
            }

            // An Added entity does not yet exist in the database. If it is then marked as deleted there is
            // nothing to delete because it was not yet inserted, so just make sure it doesn't get inserted.
            entry.State =
                initialState == EntityState.Added
                    ? EntityState.Detached
                    : EntityState.Deleted;

            return entry;
        }

        private EntityEntry SetEntityState(object entity, EntityState entityState)
        {
            var entry = EntryWithoutDetectChanges(entity);

            SetEntityState(entry.GetInfrastructure(), entityState);

            return entry;
        }

        /// <summary>
        ///     Begins tracking the given entities, and any other reachable entities that are
        ///     not already being tracked, in the <see cref="EntityState.Added" /> state such that they will
        ///     be inserted into the database when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <param name="entities"> The entities to add. </param>
        public virtual void AddRange(params object[] entities)
        {
            CheckDisposed();

            AddRange((IEnumerable<object>)entities);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity, and any other reachable entities that are
        ///         not already being tracked, in the <see cref="EntityState.Added" /> state such that they will
        ///         be inserted into the database when <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         This method is async only to allow special value generators, such as the one used by
        ///         'Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.SequenceHiLo',
        ///         to access the database asynchronously. For all other cases the non async method should be used.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to add. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public virtual Task AddRangeAsync(params object[] entities)
        {
            CheckDisposed();

            return AddRangeAsync((IEnumerable<object>)entities);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entities and entries reachable from the given entities using
        ///         the <see cref="EntityState.Unchanged" /> state by default, but see below for cases
        ///         when a different state will be used.
        ///     </para>
        ///     <para>
        ///         Generally, no database interaction will be performed until <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. All entities found will be tracked
        ///         by the context.
        ///     </para>
        ///     <para>
        ///         For entity types with generated keys if an entity has its primary key value set
        ///         then it will be tracked in the <see cref="EntityState.Unchanged" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="EntityState.Added" /> state.
        ///         This helps ensure only new entities will be inserted.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        ///     <para>
        ///         For entity types without generated keys, the state set is always <see cref="EntityState.Unchanged" />.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to attach. </param>
        public virtual void AttachRange(params object[] entities)
        {
            CheckDisposed();

            AttachRange((IEnumerable<object>)entities);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entities and entries reachable from the given entities using
        ///         the <see cref="EntityState.Modified" /> state by default, but see below for cases
        ///         when a different state will be used.
        ///     </para>
        ///     <para>
        ///         Generally, no database interaction will be performed until <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. All entities found will be tracked
        ///         by the context.
        ///     </para>
        ///     <para>
        ///         For entity types with generated keys if an entity has its primary key value set
        ///         then it will be tracked in the <see cref="EntityState.Modified" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="EntityState.Added" /> state.
        ///         This helps ensure new entities will be inserted, while existing entities will be updated.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        ///     <para>
        ///         For entity types without generated keys, the state set is always <see cref="EntityState.Modified" />.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to update. </param>
        public virtual void UpdateRange(params object[] entities)
        {
            CheckDisposed();

            UpdateRange((IEnumerable<object>)entities);
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If any of the entities are already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking those entities (rather than marking them as <see cref="EntityState.Deleted" />) since those
        ///         entities were previously added to the context and do not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="AttachRange(object[])" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="SaveChanges()" /> is called.
        ///     </para>
        /// </remarks>
        /// <param name="entities"> The entities to remove. </param>
        public virtual void RemoveRange(params object[] entities)
        {
            CheckDisposed();

            RemoveRange((IEnumerable<object>)entities);
        }

        private void SetEntityStates(IEnumerable<object> entities, EntityState entityState)
        {
            var stateManager = DbContextDependencies.StateManager;

            foreach (var entity in entities)
            {
                SetEntityState(stateManager.GetOrCreateEntry(entity), entityState);
            }
        }

        /// <summary>
        ///     Begins tracking the given entities, and any other reachable entities that are
        ///     not already being tracked, in the <see cref="EntityState.Added" /> state such that they will
        ///     be inserted into the database when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <param name="entities"> The entities to add. </param>
        public virtual void AddRange(IEnumerable<object> entities)
        {
            CheckDisposed();

            SetEntityStates(Check.NotNull(entities, nameof(entities)), EntityState.Added);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity, and any other reachable entities that are
        ///         not already being tracked, in the <see cref="EntityState.Added" /> state such that they will
        ///         be inserted into the database when <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         This method is async only to allow special value generators, such as the one used by
        ///         'Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.SequenceHiLo',
        ///         to access the database asynchronously. For all other cases the non async method should be used.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to add. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken" /> is canceled. </exception>
        public virtual async Task AddRangeAsync(
            IEnumerable<object> entities,
            CancellationToken cancellationToken = default)
        {
            CheckDisposed();

            var stateManager = DbContextDependencies.StateManager;

            foreach (var entity in entities)
            {
                await SetEntityStateAsync(
                        stateManager.GetOrCreateEntry(entity),
                        EntityState.Added,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entities and entries reachable from the given entities using
        ///         the <see cref="EntityState.Unchanged" /> state by default, but see below for cases
        ///         when a different state will be used.
        ///     </para>
        ///     <para>
        ///         Generally, no database interaction will be performed until <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. All entities found will be tracked
        ///         by the context.
        ///     </para>
        ///     <para>
        ///         For entity types with generated keys if an entity has its primary key value set
        ///         then it will be tracked in the <see cref="EntityState.Unchanged" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="EntityState.Added" /> state.
        ///         This helps ensure only new entities will be inserted.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        ///     <para>
        ///         For entity types without generated keys, the state set is always <see cref="EntityState.Unchanged" />.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to attach. </param>
        public virtual void AttachRange(IEnumerable<object> entities)
        {
            CheckDisposed();

            SetEntityStates(Check.NotNull(entities, nameof(entities)), EntityState.Unchanged);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entities and entries reachable from the given entities using
        ///         the <see cref="EntityState.Modified" /> state by default, but see below for cases
        ///         when a different state will be used.
        ///     </para>
        ///     <para>
        ///         Generally, no database interaction will be performed until <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. All entities found will be tracked
        ///         by the context.
        ///     </para>
        ///     <para>
        ///         For entity types with generated keys if an entity has its primary key value set
        ///         then it will be tracked in the <see cref="EntityState.Modified" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="EntityState.Added" /> state.
        ///         This helps ensure new entities will be inserted, while existing entities will be updated.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        ///     <para>
        ///         For entity types without generated keys, the state set is always <see cref="EntityState.Modified" />.
        ///     </para>
        ///     <para>
        ///         Use <see cref="EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to update. </param>
        public virtual void UpdateRange(IEnumerable<object> entities)
        {
            CheckDisposed();

            SetEntityStates(Check.NotNull(entities, nameof(entities)), EntityState.Modified);
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If any of the entities are already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///         stop tracking those entities (rather than marking them as <see cref="EntityState.Deleted" />) since those
        ///         entities were previously added to the context and do not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="AttachRange(IEnumerable{object})" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="SaveChanges()" /> is called.
        ///     </para>
        /// </remarks>
        /// <param name="entities"> The entities to remove. </param>
        public virtual void RemoveRange(IEnumerable<object> entities)
        {
            Check.NotNull(entities, nameof(entities));
            CheckDisposed();

            var stateManager = DbContextDependencies.StateManager;

            // An Added entity does not yet exist in the database. If it is then marked as deleted there is
            // nothing to delete because it was not yet inserted, so just make sure it doesn't get inserted.
            foreach (var entity in entities)
            {
                var entry = stateManager.GetOrCreateEntry(entity);

                var initialState = entry.EntityState;
                if (initialState == EntityState.Detached)
                {
                    SetEntityState(entry, EntityState.Unchanged);
                }

                entry.SetEntityState(
                    initialState == EntityState.Added
                        ? EntityState.Detached
                        : EntityState.Deleted);
            }
        }

        /// <summary>
        ///     Finds an entity with the given primary key values. If an entity with the given primary key values
        ///     is being tracked by the context, then it is returned immediately without making a request to the
        ///     database. Otherwise, a query is made to the database for an entity with the given primary key values
        ///     and this entity, if found, is attached to the context and returned. If no entity is found, then
        ///     null is returned.
        /// </summary>
        /// <param name="entityType"> The type of entity to find. </param>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>The entity found, or <see langword="null" />.</returns>
        public virtual object? Find(Type entityType, params object?[]? keyValues)
        {
            CheckDisposed();

            return Finder(entityType).Find(keyValues);
        }

        /// <summary>
        ///     Finds an entity with the given primary key values. If an entity with the given primary key values
        ///     is being tracked by the context, then it is returned immediately without making a request to the
        ///     database. Otherwise, a query is made to the database for an entity with the given primary key values
        ///     and this entity, if found, is attached to the context and returned. If no entity is found, then
        ///     null is returned.
        /// </summary>
        /// <param name="entityType"> The type of entity to find. </param>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>The entity found, or <see langword="null" />.</returns>
        public virtual ValueTask<object?> FindAsync(Type entityType, params object?[]? keyValues)
        {
            CheckDisposed();

            return Finder(entityType).FindAsync(keyValues);
        }

        /// <summary>
        ///     Finds an entity with the given primary key values. If an entity with the given primary key values
        ///     is being tracked by the context, then it is returned immediately without making a request to the
        ///     database. Otherwise, a query is made to the database for an entity with the given primary key values
        ///     and this entity, if found, is attached to the context and returned. If no entity is found, then
        ///     null is returned.
        /// </summary>
        /// <param name="entityType"> The type of entity to find. </param>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>The entity found, or <see langword="null" />.</returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken" /> is canceled. </exception>
        public virtual ValueTask<object?> FindAsync(
            Type entityType,
            object?[]? keyValues,
            CancellationToken cancellationToken)
        {
            CheckDisposed();

            return Finder(entityType).FindAsync(keyValues, cancellationToken);
        }

        /// <summary>
        ///     Finds an entity with the given primary key values. If an entity with the given primary key values
        ///     is being tracked by the context, then it is returned immediately without making a request to the
        ///     database. Otherwise, a query is made to the database for an entity with the given primary key values
        ///     and this entity, if found, is attached to the context and returned. If no entity is found, then
        ///     null is returned.
        /// </summary>
        /// <typeparam name="TEntity"> The type of entity to find. </typeparam>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>The entity found, or <see langword="null" />.</returns>
        public virtual TEntity? Find<TEntity>(params object?[]? keyValues)
            where TEntity : class
        {
            CheckDisposed();

            return ((IEntityFinder<TEntity>)Finder(typeof(TEntity))).Find(keyValues);
        }

        /// <summary>
        ///     Finds an entity with the given primary key values. If an entity with the given primary key values
        ///     is being tracked by the context, then it is returned immediately without making a request to the
        ///     database. Otherwise, a query is made to the database for an entity with the given primary key values
        ///     and this entity, if found, is attached to the context and returned. If no entity is found, then
        ///     null is returned.
        /// </summary>
        /// <typeparam name="TEntity"> The type of entity to find. </typeparam>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>The entity found, or <see langword="null" />.</returns>
        public virtual ValueTask<TEntity?> FindAsync<TEntity>(params object?[]? keyValues)
            where TEntity : class
        {
            CheckDisposed();

            return ((IEntityFinder<TEntity>)Finder(typeof(TEntity))).FindAsync(keyValues);
        }

        /// <summary>
        ///     Finds an entity with the given primary key values. If an entity with the given primary key values
        ///     is being tracked by the context, then it is returned immediately without making a request to the
        ///     database. Otherwise, a query is made to the database for an entity with the given primary key values
        ///     and this entity, if found, is attached to the context and returned. If no entity is found, then
        ///     null is returned.
        /// </summary>
        /// <typeparam name="TEntity"> The type of entity to find. </typeparam>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>The entity found, or <see langword="null" />.</returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken" /> is canceled. </exception>
        public virtual ValueTask<TEntity?> FindAsync<TEntity>(object?[]? keyValues, CancellationToken cancellationToken)
            where TEntity : class
        {
            CheckDisposed();

            return ((IEntityFinder<TEntity>)Finder(typeof(TEntity))).FindAsync(keyValues, cancellationToken);
        }

        /// <summary>
        ///     <para>
        ///         Gets the scoped <see cref="IServiceProvider" /> being used to resolve services.
        ///     </para>
        ///     <para>
        ///         This property is intended for use by extension methods that need to make use of services
        ///         not directly exposed in the public API surface.
        ///     </para>
        /// </summary>
        IServiceProvider IInfrastructure<IServiceProvider>.Instance
            => InternalServiceProvider;

        /// <summary>
        ///     Creates a queryable for given query expression.
        /// </summary>
        /// <typeparam name="TResult"> The result type of the query expression. </typeparam>
        /// <param name="expression"> The query expression to create. </param>
        /// <returns> An <see cref="IQueryable{T}" /> representing the query. </returns>
        public virtual IQueryable<TResult> FromExpression<TResult>(Expression<Func<IQueryable<TResult>>> expression)
        {
            Check.NotNull(expression, nameof(expression));

            return DbContextDependencies.QueryProvider.CreateQuery<TResult>(expression.Body);
        }

        #region Hidden System.Object members
        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string? ToString()
            => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj)
            => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
            => base.GetHashCode();
        #endregion
    }
}
