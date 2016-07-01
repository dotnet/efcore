// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InternalDbSet<TEntity>
        : DbSet<TEntity>, IQueryable<TEntity>, IAsyncEnumerableAccessor<TEntity>, IInfrastructure<IServiceProvider>
        where TEntity : class
    {
        private readonly DbContext _context;
        private IStateManager _stateManager;
        private readonly LazyRef<EntityQueryable<TEntity>> _entityQueryable;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalDbSet([NotNull] DbContext context)
        {
            Check.NotNull(context, nameof(context));

            _context = context;

            // Using context/service locator here so that the context will be initialized the first time the
            // set is used and services will be obtained from the correctly scoped container when this happens.
            _entityQueryable
                = new LazyRef<EntityQueryable<TEntity>>(
                    () => new EntityQueryable<TEntity>(_context.QueryProvider));
        }

        /// <summary>
        ///     Finds an entity with the given primary key values. If an entity with the given primary key values
        ///     is being tracked by the context, then it is returned immediately without making a request to the
        ///     database. Otherwise, a query is made to the dataabse for an entity with the given primary key values
        ///     and this entity, if found, is attached to the context and returned. If no entity is found, then
        ///     null is returned.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>The entity found, or null.</returns>
        public override TEntity Find(params object[] keyValues)
        {
            Check.NotNull(keyValues, nameof(keyValues));

            IReadOnlyList<IProperty> keyProperties;
            return FindTracked(keyValues, out keyProperties)
                   ?? this.FirstOrDefault(BuildPredicate(keyProperties, keyValues));
        }

        /// <summary>
        ///     Finds an entity with the given primary key values. If an entity with the given primary key values
        ///     is being tracked by the context, then it is returned immediately without making a request to the
        ///     database. Otherwise, a query is made to the dataabse for an entity with the given primary key values
        ///     and this entity, if found, is attached to the context and returned. If no entity is found, then
        ///     null is returned.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>The entity found, or null.</returns>
        public override Task<TEntity> FindAsync(params object[] keyValues)
            => FindAsync(keyValues, default(CancellationToken));

        /// <summary>
        ///     Finds an entity with the given primary key values. If an entity with the given primary key values
        ///     is being tracked by the context, then it is returned immediately without making a request to the
        ///     database. Otherwise, a query is made to the dataabse for an entity with the given primary key values
        ///     and this entity, if found, is attached to the context and returned. If no entity is found, then
        ///     null is returned.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>The entity found, or null.</returns>
        public override Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken)
        {
            Check.NotNull(keyValues, nameof(keyValues));

            IReadOnlyList<IProperty> keyProperties;
            var tracked = FindTracked(keyValues, out keyProperties);
            return tracked != null
                ? Task.FromResult(tracked)
                : this.FirstOrDefaultAsync(BuildPredicate(keyProperties, keyValues), cancellationToken);
        }

        private TEntity FindTracked(object[] keyValues, out IReadOnlyList<IProperty> keyProperties)
        {
            var key = _context.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey();
            keyProperties = key.Properties;

            if (keyProperties.Count != keyValues.Length)
            {
                if (keyProperties.Count == 1)
                {
                    throw new ArgumentException(
                        CoreStrings.FindNotCompositeKey(typeof(TEntity).ShortDisplayName(), keyValues.Length));
                }
                throw new ArgumentException(
                    CoreStrings.FindValueCountMismatch(typeof(TEntity).ShortDisplayName(), keyProperties.Count, keyValues.Length));
            }

            for (var i = 0; i < keyValues.Length; i++)
            {
                if (keyValues[i] == null)
                {
                    throw new ArgumentNullException(nameof(keyValues));
                }

                var valueType = keyValues[i].GetType();
                var propertyType = keyProperties[i].ClrType;
                if (valueType != propertyType)
                {
                    throw new ArgumentException(
                        CoreStrings.FindValueTypeMismatch(
                            i, typeof(TEntity).ShortDisplayName(), valueType.ShortDisplayName(), propertyType.ShortDisplayName()));
                }
            }

            return StateManager.TryGetEntry(key, keyValues)?.Entity as TEntity;
        }

        private IStateManager StateManager => _stateManager ?? (_stateManager = _context.GetService<IStateManager>());

        private static readonly MethodInfo _efPropertyMethod
            = typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(EF.Property));

        private static Expression<Func<TEntity, bool>> BuildPredicate(IReadOnlyList<IProperty> keyProperties, object[] keyValues)
        {
            var entityParameter = Expression.Parameter(typeof(TEntity), "e");

            BinaryExpression predicate = null;
            for (var i = 0; i < keyValues.Length; i++)
            {
                var property = keyProperties[i];
                var equals =
                    Expression.Equal(
                        Expression.Call(
                            _efPropertyMethod.MakeGenericMethod(property.ClrType),
                            entityParameter,
                            Expression.Constant(property.Name, typeof(string))),
                        Expression.Constant(
                            keyValues[i], property.ClrType));

                predicate = predicate == null ? equals : Expression.AndAlso(predicate, equals);
            }

            return Expression.Lambda<Func<TEntity, bool>>(predicate, entityParameter);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override EntityEntry<TEntity> Add(TEntity entity)
            => _context.Add(Check.NotNull(entity, nameof(entity)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override EntityEntry<TEntity> Attach(TEntity entity)
            => _context.Attach(Check.NotNull(entity, nameof(entity)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override EntityEntry<TEntity> Remove(TEntity entity)
            => _context.Remove(Check.NotNull(entity, nameof(entity)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override EntityEntry<TEntity> Update(TEntity entity)
            => _context.Update(Check.NotNull(entity, nameof(entity)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void AddRange(params TEntity[] entities)
            // ReSharper disable once CoVariantArrayConversion
            => _context.AddRange(Check.NotNull(entities, nameof(entities)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void AttachRange(params TEntity[] entities)
            // ReSharper disable once CoVariantArrayConversion
            => _context.AttachRange(Check.NotNull(entities, nameof(entities)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void RemoveRange(params TEntity[] entities)
            // ReSharper disable once CoVariantArrayConversion
            => _context.RemoveRange(Check.NotNull(entities, nameof(entities)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void UpdateRange(params TEntity[] entities)
            // ReSharper disable once CoVariantArrayConversion
            => _context.UpdateRange(Check.NotNull(entities, nameof(entities)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void AddRange(IEnumerable<TEntity> entities)
            => _context.AddRange(Check.NotNull(entities, nameof(entities)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void AttachRange(IEnumerable<TEntity> entities)
            => _context.AttachRange(Check.NotNull(entities, nameof(entities)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void RemoveRange(IEnumerable<TEntity> entities)
            => _context.RemoveRange(Check.NotNull(entities, nameof(entities)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void UpdateRange(IEnumerable<TEntity> entities)
            => _context.UpdateRange(Check.NotNull(entities, nameof(entities)));

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() => _entityQueryable.Value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _entityQueryable.Value.GetEnumerator();

        IAsyncEnumerable<TEntity> IAsyncEnumerableAccessor<TEntity>.AsyncEnumerable => _entityQueryable.Value;

        Type IQueryable.ElementType => _entityQueryable.Value.ElementType;

        Expression IQueryable.Expression => _entityQueryable.Value.Expression;

        IQueryProvider IQueryable.Provider => _entityQueryable.Value.Provider;

        IServiceProvider IInfrastructure<IServiceProvider>.Instance => ((IInfrastructure<IServiceProvider>)_context).Instance;
    }
}
