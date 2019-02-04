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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InternalDbSet<TEntity> :
        DbSet<TEntity>, IQueryable<TEntity>, IAsyncEnumerableAccessor<TEntity>, IInfrastructure<IServiceProvider>, IResettableService
        where TEntity : class
    {
        private readonly DbContext _context;
        private IEntityType? _entityType;
        private EntityQueryable<TEntity>? _entityQueryable;
        private LocalView<TEntity>? _localView;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalDbSet([NotNull] DbContext context)
        {
            Check.NotNull(context, nameof(context));

            // Just storing context/service locator here so that the context will be initialized by the time the
            // set is used and services will be obtained from the correctly scoped container when this happens.
            _context = context;
        }

        private IEntityType EntityType
        {
            get
            {
                if (_entityType != null)
                {
                    return _entityType;
                }

                _entityType = _context.Model.FindEntityType(typeof(TEntity));

                if (_entityType == null)
                {
                    if (_context.Model.HasEntityTypeWithDefiningNavigation(typeof(TEntity)))
                    {
                        throw new InvalidOperationException(CoreStrings.InvalidSetTypeWeak(typeof(TEntity).ShortDisplayName()));
                    }

                    throw new InvalidOperationException(CoreStrings.InvalidSetType(typeof(TEntity).ShortDisplayName()));
                }

                if (_entityType.IsOwned())
                {
                    _entityType = null;

                    throw new InvalidOperationException(CoreStrings.InvalidSetTypeOwned(typeof(TEntity).ShortDisplayName()));
                }

                if (_entityType.IsQueryType)
                {
                    _entityType = null;

                    throw new InvalidOperationException(CoreStrings.InvalidSetTypeQuery(typeof(TEntity).ShortDisplayName()));
                }

                return _entityType;
            }
        }

        private void CheckState()
        {
            // ReSharper disable once AssignmentIsFullyDiscarded
            _ = EntityType;
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
            => new EntityQueryable<TEntity>(_context.GetDependencies().QueryProvider);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override LocalView<TEntity> Local
        {
            get
            {
                CheckState();

                return _localView ?? (_localView = new LocalView<TEntity>(this));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override TEntity? Find(params object[]? keyValues)
            => Finder.Find(keyValues);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Task<TEntity?> FindAsync(params object[]? keyValues)
            => Finder.FindAsync(keyValues);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Task<TEntity?> FindAsync(object[]? keyValues, CancellationToken cancellationToken)
            => Finder.FindAsync(keyValues, cancellationToken);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override EntityEntry<TEntity> Add(TEntity entity)
            => _context.Add(entity);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Task<EntityEntry<TEntity>> AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
            => _context.AddAsync(entity, cancellationToken);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override EntityEntry<TEntity> Attach(TEntity entity)
            => _context.Attach(entity);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override EntityEntry<TEntity> Remove(TEntity entity)
            => _context.Remove(entity);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override EntityEntry<TEntity> Update(TEntity entity)
            => _context.Update(entity);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void AddRange(params TEntity[] entities)
            // ReSharper disable once CoVariantArrayConversion
            => _context.AddRange(entities);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Task AddRangeAsync(params TEntity[] entities)
            // ReSharper disable once CoVariantArrayConversion
            => _context.AddRangeAsync(entities);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void AttachRange(params TEntity[] entities)
            // ReSharper disable once CoVariantArrayConversion
            => _context.AttachRange(entities);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void RemoveRange(params TEntity[] entities)
            // ReSharper disable once CoVariantArrayConversion
            => _context.RemoveRange(entities);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void UpdateRange(params TEntity[] entities)
            // ReSharper disable once CoVariantArrayConversion
            => _context.UpdateRange(entities);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void AddRange(IEnumerable<TEntity> entities)
            => _context.AddRange(entities);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Task AddRangeAsync(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default)
            => _context.AddRangeAsync(entities, cancellationToken);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void AttachRange(IEnumerable<TEntity> entities)
            => _context.AttachRange(entities);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void RemoveRange(IEnumerable<TEntity> entities)
            => _context.RemoveRange(entities);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void UpdateRange(IEnumerable<TEntity> entities)
            => _context.UpdateRange(entities);

        private IEntityFinder<TEntity> Finder
            => (IEntityFinder<TEntity>)_context.GetDependencies().EntityFinderFactory.Create(EntityType);

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() => EntityQueryable.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => EntityQueryable.GetEnumerator();

        IAsyncEnumerable<TEntity> IAsyncEnumerableAccessor<TEntity>.AsyncEnumerable => EntityQueryable;

        Type IQueryable.ElementType => EntityQueryable.ElementType;

        Expression IQueryable.Expression => EntityQueryable.Expression;

        IQueryProvider IQueryable.Provider => EntityQueryable.Provider;

        IServiceProvider IInfrastructure<IServiceProvider>.Instance
            => _context.GetInfrastructure();

        void IResettableService.ResetState() => _localView = null;
    }
}
