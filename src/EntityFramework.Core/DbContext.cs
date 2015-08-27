// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity
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
    ///         other options) to be used for the context.
    ///     </para>
    ///     <para>
    ///         The model is discovered by running a set of conventions over the entity classes found in the
    ///         <see cref="DbSet{TEntity}" />
    ///         properties on the derived context. To further configure the model that is discovered by convention, you can
    ///         override the <see cref="OnModelCreating(ModelBuilder)" /> method.
    ///     </para>
    /// </remarks>
    public class DbContext : IDisposable, IAccessor<IServiceProvider>
    {
        private static readonly ThreadSafeDictionaryCache<Type, Type> _optionsTypes = new ThreadSafeDictionaryCache<Type, Type>();

        private LazyRef<IDbContextServices> _contextServices;
        private LazyRef<IDbSetInitializer> _setInitializer;
        private LazyRef<ChangeTracker> _changeTracker;
        private ILogger _logger;
        private ILoggerFactory _loggerFactory;

        private bool _initializing;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContext" /> class. The
        ///     <see cref="OnConfiguring(DbContextOptionsBuilder)" />
        ///     method will be called to configure the database (and other options) to be used for this context.
        /// </summary>
        protected DbContext()
        {
            var serviceProvider = DbContextActivator.ServiceProvider;

            Initialize(serviceProvider, GetOptions(serviceProvider));
        }

        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="DbContext" /> class using an <see cref="IServiceProvider" />.
        ///     </para>
        ///     <para>
        ///         The service provider must contain all the services required by Entity Framework (and the database being
        ///         used).
        ///         The Entity Framework services can be registered using the
        ///         <see cref="EntityFrameworkServiceCollectionExtensions.AddEntityFramework" /> method.
        ///         Most databases also provide an extension method on <see cref="IServiceCollection" /> to register the services
        ///         required.
        ///     </para>
        ///     <para>
        ///         If the <see cref="IServiceProvider" /> has a <see cref="DbContextOptions" /> or
        ///         <see cref="DbContextOptions{TContext}" />
        ///         registered, then this will be used as the options for this context instance. The <see cref="OnConfiguring" />
        ///         method
        ///         will still be called to allow further configuration of the options.
        ///     </para>
        /// </summary>
        /// <param name="serviceProvider">The service provider to be used.</param>
        public DbContext([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));

            Initialize(serviceProvider, GetOptions(serviceProvider));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContext" /> with the specified options. The
        ///     <see cref="OnConfiguring(DbContextOptionsBuilder)" /> method will still be called to allow further
        ///     configuration of the options.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public DbContext([NotNull] DbContextOptions options)
        {
            Check.NotNull(options, nameof(options));

            Initialize(DbContextActivator.ServiceProvider, options);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContext" /> class using an <see cref="IServiceProvider" />
        ///     and the specified options.
        ///     <para>
        ///         The <see cref="OnConfiguring(DbContextOptionsBuilder)" /> method will still be called to allow further
        ///         configuration of the options.
        ///     </para>
        ///     <para>
        ///         The service provider must contain all the services required by Entity Framework (and the databases being
        ///         used).
        ///         The Entity Framework services can be registered using the
        ///         <see cref="EntityFrameworkServiceCollectionExtensions.AddEntityFramework" /> method.
        ///         Most databases also provide an extension method on <see cref="IServiceCollection" /> to register the services
        ///         required.
        ///     </para>
        ///     <para>
        ///         If the <see cref="IServiceProvider" /> has a <see cref="DbContextOptions" /> or
        ///         <see cref="DbContextOptions{TContext}" />
        ///         registered, then this will be used as the options for this context instance. The <see cref="OnConfiguring" />
        ///         method
        ///         will still be called to allow further configuration of the options.
        ///     </para>
        /// </summary>
        /// <param name="serviceProvider">The service provider to be used.</param>
        /// <param name="options">The options for this context.</param>
        public DbContext([NotNull] IServiceProvider serviceProvider, [NotNull] DbContextOptions options)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));
            Check.NotNull(options, nameof(options));

            Initialize(serviceProvider, options);
        }

        private void Initialize(IServiceProvider serviceProvider, DbContextOptions options)
        {
            InitializeSets(serviceProvider, options);
            _contextServices = new LazyRef<IDbContextServices>(() => InitializeServices(serviceProvider, options));
            _setInitializer = new LazyRef<IDbSetInitializer>(() => ServiceProvider.GetRequiredService<IDbSetInitializer>());
            _changeTracker = new LazyRef<ChangeTracker>(() => ServiceProvider.GetRequiredService<IChangeTrackerFactory>().Create());
        }

        private DbContextOptions GetOptions(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                return new DbContextOptions<DbContext>();
            }

            var genericOptions = _optionsTypes.GetOrAdd(GetType(), t => typeof(DbContextOptions<>).MakeGenericType(t));

            var options = (DbContextOptions)serviceProvider.GetService(genericOptions)
                          ?? serviceProvider.GetService<DbContextOptions>();

            if (options != null
                && options.GetType() != genericOptions)
            {
                throw new InvalidOperationException(Strings.NonGenericOptions);
            }

            return options ?? new DbContextOptions<DbContext>();
        }

        private IChangeDetector GetChangeDetector() => ServiceProvider.GetRequiredService<IChangeDetector>();

        private IStateManager GetStateManager() => ServiceProvider.GetRequiredService<IStateManager>();

        private IServiceProvider ServiceProvider => _contextServices.Value.ServiceProvider;

        private IDbContextServices InitializeServices(IServiceProvider serviceProvider, DbContextOptions options)
        {
            if (_initializing)
            {
                throw new InvalidOperationException(Strings.RecursiveOnConfiguring);
            }

            try
            {
                _initializing = true;

                var optionsBuilder = new DbContextOptionsBuilder(options);
                OnConfiguring(optionsBuilder);

                var providerSource = serviceProvider != null ? ServiceProviderSource.Explicit : ServiceProviderSource.Implicit;

                serviceProvider = serviceProvider ?? ServiceProviderCache.Instance.GetOrAdd(optionsBuilder.Options);

                _loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                _logger = _loggerFactory.CreateLogger<DbContext>();
                _logger.LogDebug(Strings.DebugLogWarning(nameof(LogLevel.Debug), nameof(ILoggerFactory) + "." + nameof(ILoggerFactory.MinimumLevel), nameof(LogLevel) + "." + nameof(LogLevel.Verbose)));

                var scopedServiceProvider = serviceProvider
                    .GetRequiredService<IServiceScopeFactory>()
                    .CreateScope()
                    .ServiceProvider;

                return scopedServiceProvider
                    .GetRequiredService<IDbContextServices>()
                    .Initialize(scopedServiceProvider, optionsBuilder.Options, this, providerSource);
            }
            finally
            {
                _initializing = false;
            }
        }

        private void InitializeSets(IServiceProvider serviceProvider, DbContextOptions options)
        {
            serviceProvider = serviceProvider ?? ServiceProviderCache.Instance.GetOrAdd(options);

            serviceProvider.GetRequiredService<IDbSetInitializer>().InitializeSets(this);
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
        IServiceProvider IAccessor<IServiceProvider>.Service => ServiceProvider;

        /// <summary>
        ///     Override this method to configure the database (and other options) to be used for this context.
        ///     This method is called for each instance of the context that is created.
        /// </summary>
        /// <remarks>
        ///     If you passed an instance of <see cref="DbContextOptions" /> to the constructor of the context (or
        ///     provided an <see cref="IServiceProvider" /> with <see cref="DbContextOptions" /> registered) then
        ///     it is cloned before being passed to this method. This allows the options to be altered without
        ///     affecting other context instances that are constructed with the same <see cref="DbContextOptions" />
        ///     instance.
        /// </remarks>
        /// <param name="optionsBuilder">
        ///     A builder used to create or modify options for this context. Databases (and other extensions)
        ///     typically define extension methods on this object that allow you to configure the context.
        /// </param>
        protected internal virtual void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        /// <summary>
        ///     Override this method to further configure the model that was discovered by convention from the entity types
        ///     exposed in <see cref="DbSet{TEntity}" /> properties on your derived context. The resulting model may be cached
        ///     and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">
        ///     The builder being used to construct the model for this context. Databases (and other extensions) typically
        ///     define extension methods on this object that allow you to configure aspects of the model that are specific
        ///     to a given database.
        /// </param>
        protected internal virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        /// <summary>
        ///     Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <remarks>
        ///     This method will automatically call <see cref="ChangeTracker.DetectChanges" /> to discover any changes
        ///     to entity instances before saving to the underlying database. This can be disabled via
        ///     <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
        /// </remarks>
        /// <returns>
        ///     The number of state entries written to the underlying database.
        /// </returns>
        [DebuggerStepThrough]
        public virtual int SaveChanges() => SaveChanges(acceptAllChangesOnSuccess: true);

        /// <summary>
        ///     Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">
        ///     Indicates whether <see cref="ChangeTracker.AcceptAllChanges" /> is called after the changes have been
        ///     sent succesfully to the database.
        /// </param>
        /// <remarks>
        ///     This method will automatically call <see cref="ChangeTracker.DetectChanges" /> to discover any changes
        ///     to entity instances before saving to the underlying database. This can be disabled via
        ///     <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
        /// </remarks>
        /// <returns>
        ///     The number of state entries written to the underlying database.
        /// </returns>
        [DebuggerStepThrough]
        public virtual int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var stateManager = GetStateManager();

            TryDetectChanges(stateManager);

            try
            {
                return stateManager.SaveChanges(acceptAllChangesOnSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    new DatabaseErrorLogState(GetType()),
                    ex,
                    (state, exception) =>
                        Strings.LogExceptionDuringSaveChanges(Environment.NewLine, exception));

                throw;
            }
        }

        private void TryDetectChanges(IStateManager stateManager)
        {
            if (ChangeTracker.AutoDetectChangesEnabled)
            {
                GetChangeDetector().DetectChanges(stateManager);
            }
        }

        /// <summary>
        ///     Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This method will automatically call <see cref="ChangeTracker.DetectChanges" /> to discover any changes
        ///         to entity instances before saving to the underlying database. This can be disabled via
        ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
        ///     </para>
        ///     <para>
        ///         Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///         that any asynchronous operations have completed before calling another method on this context.
        ///     </para>
        /// </remarks>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains the
        ///     number of state entries written to the underlying database.
        /// </returns>
        public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken: cancellationToken);
        }

        /// <summary>
        ///     Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">
        ///     Indicates whether <see cref="ChangeTracker.AcceptAllChanges" /> is called after the changes have been
        ///     sent succesfully to the database.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         This method will automatically call <see cref="ChangeTracker.DetectChanges" /> to discover any changes
        ///         to entity instances before saving to the underlying database. This can be disabled via
        ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
        ///     </para>
        ///     <para>
        ///         Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///         that any asynchronous operations have completed before calling another method on this context.
        ///     </para>
        /// </remarks>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains the
        ///     number of state entries written to the underlying database.
        /// </returns>
        public virtual async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var stateManager = GetStateManager();

            if (ChangeTracker.AutoDetectChangesEnabled)
            {
                GetChangeDetector().DetectChanges(stateManager);
            }

            try
            {
                return await stateManager.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    new DatabaseErrorLogState(GetType()),
                    ex,
                    (state, exception) =>
                        Strings.LogExceptionDuringSaveChanges(Environment.NewLine, exception));

                throw;
            }
        }

        /// <summary>
        ///     Releases the allocated resources for this context.
        /// </summary>
        public virtual void Dispose()
        {
            if (_contextServices.HasValue)
            {
                _contextServices.Value.Dispose();
            }
        }

        /// <summary>
        ///     Gets an <see cref="EntityEntry{TEntity}" /> for the given entity providing access to
        ///     information the context is tracking for the given the entity and the ability
        ///     to perform actions on the entity.
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity to get the entry for. </param>
        /// <returns> The entry for the given entity. </returns>
        public virtual EntityEntry<TEntity> Entry<TEntity>([NotNull] TEntity entity) where TEntity : class
        {
            Check.NotNull(entity, nameof(entity));

            TryDetectChanges(GetStateManager());

            return EntryWithoutDetectChanges(entity);
        }

        private EntityEntry<TEntity> EntryWithoutDetectChanges<TEntity>([NotNull] TEntity entity) where TEntity : class
        {
            Check.NotNull(entity, nameof(entity));

            return new EntityEntry<TEntity>(this, GetStateManager().GetOrCreateEntry(entity));
        }

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
        public virtual EntityEntry Entry([NotNull] object entity)
        {
            Check.NotNull(entity, nameof(entity));

            TryDetectChanges(GetStateManager());

            return EntryWithoutDetectChanges(entity);
        }

        private EntityEntry EntryWithoutDetectChanges([NotNull] object entity)
        {
            Check.NotNull(entity, nameof(entity));

            return new EntityEntry(this, GetStateManager().GetOrCreateEntry(entity));
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Added" /> state such that it will
        ///     be inserted into the database when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity to add. </param>
        /// <returns>
        ///     The <see cref="EntityEntry{TEntity}" /> for the entity. This entry provides access to
        ///     information the context is tracking for the entity and the ability to perform
        ///     actions on the entity.
        /// </returns>
        public virtual EntityEntry<TEntity> Add<TEntity>([NotNull] TEntity entity) where TEntity : class
        {
            Check.NotNull(entity, nameof(entity));

            return SetEntityState(entity, EntityState.Added);
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Unchanged" /> state such that no
        ///     operation will be performed when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity to attach. </param>
        /// <returns>
        ///     The <see cref="EntityEntry{TEntity}" /> for the entity. This entry provides access to
        ///     information the context is tracking for the entity and the ability to perform
        ///     actions on the entity.
        /// </returns>
        public virtual EntityEntry<TEntity> Attach<TEntity>([NotNull] TEntity entity) where TEntity : class
        {
            Check.NotNull(entity, nameof(entity));

            var entry = EntryWithoutDetectChanges(entity);
            var internalEntry = entry.GetService();
            internalEntry.SetEntityState(EntityState.Unchanged, acceptChanges: true);

            return new EntityEntry<TEntity>(entry.Context, internalEntry);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity in the <see cref="EntityState.Modified" /> state such that it will
        ///         be updated in the database when <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         All properties of the entity will be marked as modified. To mark only some properties as modified, use
        ///         <see cref="Attach{TEntity}(TEntity)" /> to begin tracking the entity in the
        ///         <see cref="EntityState.Unchanged" />
        ///         state and then use the returned <see cref="EntityEntry{TEntity}" /> to mark the desired properties as modified.
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity to update. </param>
        /// <returns>
        ///     The <see cref="EntityEntry{TEntity}" /> for the entity. This entry provides access to
        ///     information the context is tracking for the entity and the ability to perform
        ///     actions on the entity.
        /// </returns>
        public virtual EntityEntry<TEntity> Update<TEntity>([NotNull] TEntity entity) where TEntity : class
        {
            Check.NotNull(entity, nameof(entity));

            return SetEntityState(entity, EntityState.Modified);
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     If the entity is already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///     stop tracking the entity (rather than marking it as <see cref="EntityState.Deleted" />) since the
        ///     entity was previously added to the context and does not exist in the database.
        /// </remarks>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity to remove. </param>
        /// <returns>
        ///     The <see cref="EntityEntry{TEntity}" /> for the entity. This entry provides access to
        ///     information the context is tracking for the entity and the ability to perform
        ///     actions on the entity.
        /// </returns>
        public virtual EntityEntry<TEntity> Remove<TEntity>([NotNull] TEntity entity) where TEntity : class
        {
            Check.NotNull(entity, nameof(entity));

            // An Added entity does not yet exist in the database. If it is then marked as deleted there is
            // nothing to delete because it was not yet inserted, so just make sure it doesn't get inserted.
            return SetEntityState(
                entity, EntryWithoutDetectChanges(entity).State == EntityState.Added
                    ? EntityState.Detached
                    : EntityState.Deleted);
        }

        private EntityEntry<TEntity> SetEntityState<TEntity>(TEntity entity, EntityState entityState) where TEntity : class
        {
            var entry = EntryWithoutDetectChanges(entity);

            entry.State = entityState;

            return entry;
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Added" /> state such that it will
        ///     be inserted into the database when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <param name="entity"> The entity to add. </param>
        /// <returns>
        ///     The <see cref="EntityEntry" /> for the entity. This entry provides access to
        ///     information the context is tracking for the entity and the ability to perform
        ///     actions on the entity.
        /// </returns>
        public virtual EntityEntry Add([NotNull] object entity)
        {
            Check.NotNull(entity, nameof(entity));

            return SetEntityState(entity, EntityState.Added);
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Unchanged" /> state such that no
        ///     operation will be performed when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <param name="entity"> The entity to attach. </param>
        /// <returns>
        ///     The <see cref="EntityEntry" /> for the entity. This entry provides access to
        ///     information the context is tracking for the entity and the ability to perform
        ///     actions on the entity.
        /// </returns>
        public virtual EntityEntry Attach([NotNull] object entity)
        {
            Check.NotNull(entity, nameof(entity));

            return SetEntityState(entity, EntityState.Unchanged);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity in the <see cref="EntityState.Modified" /> state such that it will
        ///         be updated in the database when <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         All properties of the entity will be marked as modified. To mark only some properties as modified, use
        ///         <see cref="Attach(object)" /> to begin tracking the entity in the <see cref="EntityState.Unchanged" />
        ///         state and then use the returned <see cref="EntityEntry" /> to mark the desired properties as modified.
        ///     </para>
        /// </summary>
        /// <param name="entity"> The entity to update. </param>
        /// <returns>
        ///     The <see cref="EntityEntry" /> for the entity. This entry provides access to
        ///     information the context is tracking for the entity and the ability to perform
        ///     actions on the entity.
        /// </returns>
        public virtual EntityEntry Update([NotNull] object entity)
        {
            Check.NotNull(entity, nameof(entity));

            return SetEntityState(entity, EntityState.Modified);
        }

        /// <summary>
        ///     Begins tracking the given entity in the <see cref="EntityState.Deleted" /> state such that it will
        ///     be removed from the database when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     If the entity is already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///     stop tracking the entity (rather than marking it as <see cref="EntityState.Deleted" />) since the
        ///     entity was previously added to the context and does not exist in the database.
        /// </remarks>
        /// <param name="entity"> The entity to remove. </param>
        /// <returns>
        ///     The <see cref="EntityEntry" /> for the entity. This entry provides access to
        ///     information the context is tracking for the entity and the ability to perform
        ///     actions on the entity.
        /// </returns>
        public virtual EntityEntry Remove([NotNull] object entity)
        {
            Check.NotNull(entity, nameof(entity));

            // An Added entity does not yet exist in the database. If it is then marked as deleted there is
            // nothing to delete because it was not yet inserted, so just make sure it doesn't get inserted.
            return SetEntityState(
                entity, EntryWithoutDetectChanges(entity).State == EntityState.Added
                    ? EntityState.Detached
                    : EntityState.Deleted);
        }

        private EntityEntry SetEntityState(object entity, EntityState entityState)
        {
            var entry = EntryWithoutDetectChanges(entity);

            entry.State = entityState;

            return entry;
        }

        private void SetEntityStates(object[] entities, EntityState entityState)
        {
            var stateManager = GetStateManager();

            foreach (var entity in entities)
            {
                stateManager.GetOrCreateEntry(entity).SetEntityState(entityState);
            }
        }

        /// <summary>
        ///     Begins tracking the given entities in the <see cref="EntityState.Added" /> state such that they will
        ///     be inserted into the database when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <param name="entities"> The entities to add. </param>
        public virtual void AddRange([NotNull] params object[] entities)
        {
            Check.NotNull(entities, nameof(entities));

            SetEntityStates(entities, EntityState.Added);
        }

        /// <summary>
        ///     Begins tracking the given entities in the <see cref="EntityState.Unchanged" /> state such that no
        ///     operation will be performed when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <param name="entities"> The entities to attach. </param>
        public virtual void AttachRange([NotNull] params object[] entities)
        {
            Check.NotNull(entities, nameof(entities));

            SetEntityStates(entities, EntityState.Unchanged, acceptChanges: true);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entities in the <see cref="EntityState.Modified" /> state such that they will
        ///         be updated in the database when <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         All properties of the entities will be marked as modified. To mark only some properties as modified, use
        ///         <see cref="Attach(object)" /> to begin tracking each entity in the <see cref="EntityState.Unchanged" />
        ///         state and then use the returned <see cref="EntityEntry" /> to mark the desired properties as modified.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to update. </param>
        public virtual void UpdateRange([NotNull] params object[] entities)
        {
            Check.NotNull(entities, nameof(entities));

            SetEntityStates(entities, EntityState.Modified);
        }

        /// <summary>
        ///     Begins tracking the given entities in the <see cref="EntityState.Deleted" /> state such that they will
        ///     be removed from the database when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     If any of the entities are already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///     stop tracking those entities (rather than marking them as <see cref="EntityState.Deleted" />) since those
        ///     entities were previously added to the context and do not exist in the database.
        /// </remarks>
        /// <param name="entities"> The entities to remove. </param>
        public virtual void RemoveRange([NotNull] params object[] entities)
        {
            Check.NotNull(entities, nameof(entities));

            RemoveRange((IEnumerable<object>)entities);
        }

        private void SetEntityStates(IEnumerable<object> entities, EntityState entityState, bool acceptChanges = false)
        {
            var stateManager = GetStateManager();

            foreach (var entity in entities)
            {
                stateManager.GetOrCreateEntry(entity).SetEntityState(entityState, acceptChanges);
            }
        }

        /// <summary>
        ///     Begins tracking the given entities in the <see cref="EntityState.Added" /> state such that they will
        ///     be inserted into the database when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <param name="entities"> The entities to add. </param>
        public virtual void AddRange([NotNull] IEnumerable<object> entities)
        {
            Check.NotNull(entities, nameof(entities));

            SetEntityStates(entities, EntityState.Added);
        }

        /// <summary>
        ///     Begins tracking the given entities in the <see cref="EntityState.Unchanged" /> state such that no
        ///     operation will be performed when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <param name="entities"> The entities to attach. </param>
        public virtual void AttachRange([NotNull] IEnumerable<object> entities)
        {
            Check.NotNull(entities, nameof(entities));

            SetEntityStates(entities, EntityState.Unchanged, acceptChanges: true);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entities in the <see cref="EntityState.Modified" /> state such that they will
        ///         be updated in the database when <see cref="SaveChanges()" /> is called.
        ///     </para>
        ///     <para>
        ///         All properties of the entities will be marked as modified. To mark only some properties as modified, use
        ///         <see cref="Attach(object)" /> to begin tracking each entity in the <see cref="EntityState.Unchanged" />
        ///         state and then use the returned <see cref="EntityEntry" /> to mark the desired properties as modified.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to update. </param>
        public virtual void UpdateRange([NotNull] IEnumerable<object> entities)
        {
            Check.NotNull(entities, nameof(entities));

            SetEntityStates(entities, EntityState.Modified);
        }

        /// <summary>
        ///     Begins tracking the given entities in the <see cref="EntityState.Deleted" /> state such that they will
        ///     be removed from the database when <see cref="SaveChanges()" /> is called.
        /// </summary>
        /// <remarks>
        ///     If any of the entities are already tracked in the <see cref="EntityState.Added" /> state then the context will
        ///     stop tracking those entities (rather than marking them as <see cref="EntityState.Deleted" />) since those
        ///     entities were previously added to the context and do not exist in the database.
        /// </remarks>
        /// <param name="entities"> The entities to remove. </param>
        public virtual void RemoveRange([NotNull] IEnumerable<object> entities)
        {
            Check.NotNull(entities, nameof(entities));

            var stateManager = GetStateManager();

            // An Added entity does not yet exist in the database. If it is then marked as deleted there is
            // nothing to delete because it was not yet inserted, so just make sure it doesn't get inserted.
            foreach (var entity in entities)
            {
                var entry = stateManager.GetOrCreateEntry(entity);
                entry.SetEntityState(entry.EntityState == EntityState.Added
                    ? EntityState.Detached
                    : EntityState.Deleted);
            }
        }

        /// <summary>
        ///     Provides access to database related information and operations for this context.
        /// </summary>
        public virtual DatabaseFacade Database => new DatabaseFacade(this);

        /// <summary>
        ///     Provides access to information and operations for entity instances this context is tracking.
        /// </summary>
        public virtual ChangeTracker ChangeTracker => _changeTracker.Value;

        /// <summary>
        ///     The metadata about the shape of entities and relationships between them.
        /// </summary>
        public virtual IModel Model => ServiceProvider.GetRequiredService<IModel>();

        /// <summary>
        ///     Creates a set to perform operations for a given entity type in the model. LINQ queries against
        ///     <see cref="DbSet{TEntity}" /> will be translated into queries against the database.
        /// </summary>
        /// <typeparam name="TEntity"> The type of entity for which a set should be returned. </typeparam>
        /// <returns> A set for the given entity type. </returns>
        public virtual DbSet<TEntity> Set<TEntity>() where TEntity : class => _setInitializer.Value.CreateSet<TEntity>(this);
    }
}
