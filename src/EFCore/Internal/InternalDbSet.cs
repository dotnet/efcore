// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InternalDbSet<TEntity> :
        DbSet<TEntity>,
        IQueryable<TEntity>,
        IAsyncEnumerable<TEntity>,
        IInfrastructure<IServiceProvider>,
        IResettableService
        where TEntity : class
    {
        private readonly DbContext _context;
        private readonly string _entityTypeName;
        private IEntityType _entityType;
        private EntityQueryable<TEntity> _entityQueryable;
        private LocalView<TEntity> _localView;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalDbSet([NotNull] DbContext context, [CanBeNull] string entityTypeName)
        {
            Check.NotNull(context, nameof(context));

            // Just storing context/service locator here so that the context will be initialized by the time the
            // set is used and services will be obtained from the correctly scoped container when this happens.
            _context = context;
            _entityTypeName = entityTypeName;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IEntityType EntityType
        {
            get
            {
                if (_entityType != null)
                {
                    return _entityType;
                }

                _entityType = _entityTypeName != null
                    ? _context.Model.FindEntityType(_entityTypeName)
                    : _context.Model.FindEntityType(typeof(TEntity));

                if (_entityType == null)
                {
                    if (_context.Model.HasEntityTypeWithDefiningNavigation(typeof(TEntity)))
                    {
                        throw new InvalidOperationException(CoreStrings.InvalidSetTypeWeak(typeof(TEntity).ShortDisplayName()));
                    }

                    if (_context.Model.IsShared(typeof(TEntity)))
                    {
                        throw new InvalidOperationException(CoreStrings.InvalidSetSharedType(typeof(TEntity).ShortDisplayName()));
                    }

                    throw new InvalidOperationException(CoreStrings.InvalidSetType(typeof(TEntity).ShortDisplayName()));
                }

                if (_entityType.IsOwned())
                {
                    _entityType = null;

                    throw new InvalidOperationException(CoreStrings.InvalidSetTypeOwned(typeof(TEntity).ShortDisplayName()));
                }

                if (_entityType.ClrType != typeof(TEntity))
                {
                    var message = CoreStrings.DbSetIncorrectGenericType(
                        _entityType.ShortName(), _entityType.ClrType.ShortDisplayName(), typeof(TEntity).ShortDisplayName());
                    _entityType = null;

                    throw new InvalidOperationException(message);
                }

                return _entityType;
            }
        }

        private void CheckState()
        {
            // ReSharper disable once AssignmentIsFullyDiscarded
            _ = EntityType;
        }

        private void CheckKey()
        {
            if (EntityType.FindPrimaryKey() == null)
            {
                throw new InvalidOperationException(CoreStrings.InvalidSetKeylessOperation(typeof(TEntity).ShortDisplayName()));
            }
        }

        private EntityQueryable<TEntity> EntityQueryable
        {
            get
            {
                CheckState();

                return NonCapturingLazyInitializer.EnsureInitialized(
                    ref _entityQueryable,
                    this,
                    internalSet => internalSet.CreateEntityQueryable());
            }
        }

        private EntityQueryable<TEntity> CreateEntityQueryable()
            => new EntityQueryable<TEntity>(_context.GetDependencies().QueryProvider, EntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override LocalView<TEntity> Local
        {
            get
            {
                CheckKey();

                if (_context.ChangeTracker.AutoDetectChangesEnabled)
                {
                    _context.ChangeTracker.DetectChanges();
                }

                return _localView ??= new LocalView<TEntity>(this);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override TEntity Find(params object[] keyValues)
            => Finder.Find(keyValues);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override ValueTask<TEntity> FindAsync(params object[] keyValues)
            => Finder.FindAsync(keyValues);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override ValueTask<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken)
            => Finder.FindAsync(keyValues, cancellationToken);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override EntityEntry<TEntity> Add(TEntity entity)
        {
            var entry = EntryWithoutDetectChanges(entity);

            SetEntityState(entry.GetInfrastructure(), EntityState.Added);

            return entry;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override async ValueTask<EntityEntry<TEntity>> AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
        {
            var entry = EntryWithoutDetectChanges(Check.NotNull(entity, nameof(entity)));

            await SetEntityStateAsync(entry.GetInfrastructure(), EntityState.Added, cancellationToken)
                .ConfigureAwait(false);

            return entry;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override EntityEntry<TEntity> Attach(TEntity entity)
        {
            var entry = EntryWithoutDetectChanges(entity);

            SetEntityState(entry.GetInfrastructure(), EntityState.Unchanged);

            return entry;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override EntityEntry<TEntity> Remove(TEntity entity)
        {
            Check.NotNull(entity, nameof(entity));

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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override EntityEntry<TEntity> Update(TEntity entity)
        {
            var entry = EntryWithoutDetectChanges(entity);

            SetEntityState(entry.GetInfrastructure(), EntityState.Modified);

            return entry;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void AddRange(params TEntity[] entities)
            => SetEntityStates(entities, EntityState.Added);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override async Task AddRangeAsync(params TEntity[] entities)
        {
            var stateManager = _context.GetDependencies().StateManager;

            foreach (var entity in entities)
            {
                await SetEntityStateAsync(
                        stateManager.GetOrCreateEntry(entity, EntityType),
                        EntityState.Added,
                        default)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void AttachRange(params TEntity[] entities)
            => SetEntityStates(Check.NotNull(entities, nameof(entities)), EntityState.Unchanged);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void RemoveRange(params TEntity[] entities)
        {
            Check.NotNull(entities, nameof(entities));

            var stateManager = _context.GetDependencies().StateManager;

            // An Added entity does not yet exist in the database. If it is then marked as deleted there is
            // nothing to delete because it was not yet inserted, so just make sure it doesn't get inserted.
            foreach (var entity in entities)
            {
                var entry = stateManager.GetOrCreateEntry(entity, EntityType);

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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void UpdateRange(params TEntity[] entities)
            => SetEntityStates(Check.NotNull(entities, nameof(entities)), EntityState.Modified);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void AddRange(IEnumerable<TEntity> entities)
            => SetEntityStates(Check.NotNull(entities, nameof(entities)), EntityState.Added);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override async Task AddRangeAsync(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default)
        {
            var stateManager = _context.GetDependencies().StateManager;

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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void AttachRange(IEnumerable<TEntity> entities)
            => SetEntityStates(Check.NotNull(entities, nameof(entities)), EntityState.Unchanged);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void RemoveRange(IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities, nameof(entities));

            var stateManager = _context.GetDependencies().StateManager;

            // An Added entity does not yet exist in the database. If it is then marked as deleted there is
            // nothing to delete because it was not yet inserted, so just make sure it doesn't get inserted.
            foreach (var entity in entities)
            {
                var entry = stateManager.GetOrCreateEntry(entity, EntityType);

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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void UpdateRange(IEnumerable<TEntity> entities)
            => SetEntityStates(Check.NotNull(entities, nameof(entities)), EntityState.Modified);

        private IEntityFinder<TEntity> Finder
            => (IEntityFinder<TEntity>)_context.GetDependencies().EntityFinderFactory.Create(EntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
            => EntityQueryable.GetEnumerator();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
            => EntityQueryable.GetEnumerator();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetAsyncEnumerator(CancellationToken cancellationToken)
            => EntityQueryable.GetAsyncEnumerator(cancellationToken);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        Type IQueryable.ElementType
            => EntityQueryable.ElementType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        Expression IQueryable.Expression
            => EntityQueryable.Expression;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IQueryProvider IQueryable.Provider
            => EntityQueryable.Provider;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IServiceProvider IInfrastructure<IServiceProvider>.Instance
            => _context.GetInfrastructure();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IResettableService.ResetState()
            => _localView = null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        Task IResettableService.ResetStateAsync(CancellationToken cancellationToken)
        {
            ((IResettableService)this).ResetState();

            return Task.CompletedTask;
        }

        private EntityEntry<TEntity> EntryWithoutDetectChanges(TEntity entity)
            => new EntityEntry<TEntity>(_context.GetDependencies().StateManager.GetOrCreateEntry(entity, EntityType));

        private void SetEntityStates(IEnumerable<TEntity> entities, EntityState entityState)
        {
            var stateManager = _context.GetDependencies().StateManager;

            foreach (var entity in entities)
            {
                SetEntityState(stateManager.GetOrCreateEntry(entity, EntityType), entityState);
            }
        }

        private void SetEntityState(InternalEntityEntry entry, EntityState entityState)
        {
            if (entry.EntityState == EntityState.Detached)
            {
                _context.GetDependencies().EntityGraphAttacher.AttachGraph(
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
                ? _context.GetDependencies().EntityGraphAttacher.AttachGraphAsync(
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
    }
}
