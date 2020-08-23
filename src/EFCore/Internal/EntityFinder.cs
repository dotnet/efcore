// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class EntityFinder<TEntity> : IEntityFinder<TEntity>
        where TEntity : class
    {
        private readonly IStateManager _stateManager;
        private readonly IDbSetSource _setSource;
        private readonly IDbSetCache _setCache;
        private readonly IEntityType _entityType;
        private readonly IQueryable<TEntity> _queryRoot;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public EntityFinder(
            [NotNull] IStateManager stateManager,
            [NotNull] IDbSetSource setSource,
            [NotNull] IDbSetCache setCache,
            [NotNull] IEntityType entityType)
        {
            _stateManager = stateManager;
            _setSource = setSource;
            _setCache = setCache;
            _entityType = entityType;
            _queryRoot = (IQueryable<TEntity>)BuildQueryRoot(entityType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TEntity Find(object[] keyValues)
            => keyValues == null || keyValues.Any(v => v == null)
                ? null
                : (FindTracked(keyValues, out var keyProperties)
                    ?? _queryRoot.FirstOrDefault(BuildLambda(keyProperties, new ValueBuffer(keyValues))));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        object IEntityFinder.Find(object[] keyValues)
            => Find(keyValues);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ValueTask<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken = default)
        {
            if (keyValues == null
                || keyValues.Any(v => v == null))
            {
                return new ValueTask<TEntity>((TEntity)null);
            }

            var tracked = FindTracked(keyValues, out var keyProperties);
            return tracked != null
                ? new ValueTask<TEntity>(tracked)
                : new ValueTask<TEntity>(
                    _queryRoot.FirstOrDefaultAsync(BuildLambda(keyProperties, new ValueBuffer(keyValues)), cancellationToken));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        ValueTask<object> IEntityFinder.FindAsync(object[] keyValues, CancellationToken cancellationToken)
        {
            if (keyValues == null
                || keyValues.Any(v => v == null))
            {
                return new ValueTask<object>((object)null);
            }

            var tracked = FindTracked(keyValues, out var keyProperties);
            return tracked != null
                ? new ValueTask<object>(tracked)
                : new ValueTask<object>(
                    _queryRoot.FirstOrDefaultAsync(
                        BuildObjectLambda(keyProperties, new ValueBuffer(keyValues)), cancellationToken));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Load(INavigation navigation, InternalEntityEntry entry)
        {
            if (entry.EntityState == EntityState.Detached)
            {
                throw new InvalidOperationException(CoreStrings.CannotLoadDetached(navigation.Name, entry.EntityType.DisplayName()));
            }

            var keyValues = GetLoadValues(navigation, entry);
            // Short-circuit for any null key values for perf and because of #6129
            if (keyValues != null)
            {
                Query(navigation, keyValues).Load();
            }

            entry.SetIsLoaded(navigation);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task LoadAsync(
            INavigation navigation,
            InternalEntityEntry entry,
            CancellationToken cancellationToken = default)
        {
            if (entry.EntityState == EntityState.Detached)
            {
                throw new InvalidOperationException(CoreStrings.CannotLoadDetached(navigation.Name, entry.EntityType.DisplayName()));
            }

            // Short-circuit for any null key values for perf and because of #6129
            var keyValues = GetLoadValues(navigation, entry);
            if (keyValues != null)
            {
                await Query(navigation, keyValues).LoadAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            entry.SetIsLoaded(navigation);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IQueryable<TEntity> Query(INavigation navigation, InternalEntityEntry entry)
        {
            if (entry.EntityState == EntityState.Detached)
            {
                throw new InvalidOperationException(CoreStrings.CannotLoadDetached(navigation.Name, entry.EntityType.DisplayName()));
            }

            var keyValues = GetLoadValues(navigation, entry);
            // Short-circuit for any null key values for perf and because of #6129
            if (keyValues == null)
            {
                // Creates an empty Queryable that works with Async. Has to be an EF query because it
                // could be used in a composition.
                return _queryRoot.Where(e => false);
            }

            return Query(navigation, keyValues);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual object[] GetDatabaseValues(InternalEntityEntry entry)
            => GetDatabaseValuesQuery(entry)?.FirstOrDefault();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Task<object[]> GetDatabaseValuesAsync(
            InternalEntityEntry entry,
            CancellationToken cancellationToken = default)
            => GetDatabaseValuesQuery(entry)?.FirstOrDefaultAsync(cancellationToken);

        private IQueryable<object[]> GetDatabaseValuesQuery(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;
            var properties = entityType.FindPrimaryKey().Properties;

            var keyValues = new object[properties.Count];
            for (var i = 0; i < keyValues.Length; i++)
            {
                var keyValue = entry[properties[i]];
                if (keyValue == null)
                {
                    return null;
                }

                keyValues[i] = keyValue;
            }

            return _queryRoot.AsNoTracking().IgnoreQueryFilters()
                .Where(BuildObjectLambda(properties, new ValueBuffer(keyValues)))
                .Select(BuildProjection(entityType));
        }

        private IQueryable<TEntity> Query(INavigation navigation, object[] keyValues)
            => _queryRoot.Where(BuildLambda(GetLoadProperties(navigation), new ValueBuffer(keyValues))).AsTracking();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IQueryable IEntityFinder.Query(INavigation navigation, InternalEntityEntry entry)
            => Query(navigation, entry);

        private static object[] GetLoadValues(INavigation navigation, InternalEntityEntry entry)
        {
            var properties = navigation.IsOnDependent
                ? navigation.ForeignKey.Properties
                : navigation.ForeignKey.PrincipalKey.Properties;

            var values = new object[properties.Count];

            for (var i = 0; i < values.Length; i++)
            {
                var value = entry[properties[i]];
                if (value == null)
                {
                    return null;
                }

                values[i] = value;
            }

            return values;
        }

        private static IReadOnlyList<IProperty> GetLoadProperties(INavigation navigation)
            => navigation.IsOnDependent
                ? navigation.ForeignKey.PrincipalKey.Properties
                : navigation.ForeignKey.Properties;

        private TEntity FindTracked(object[] keyValues, out IReadOnlyList<IProperty> keyProperties)
        {
            var key = _entityType.FindPrimaryKey();
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
                var valueType = keyValues[i].GetType();
                var propertyType = keyProperties[i].ClrType;
                if (valueType != propertyType.UnwrapNullableType())
                {
                    throw new ArgumentException(
                        CoreStrings.FindValueTypeMismatch(
                            i, typeof(TEntity).ShortDisplayName(), valueType.ShortDisplayName(), propertyType.ShortDisplayName()));
                }
            }

            return _stateManager.TryGetEntry(key, keyValues)?.Entity as TEntity;
        }

        private static Expression<Func<TEntity, bool>> BuildLambda(IReadOnlyList<IProperty> keyProperties, ValueBuffer keyValues)
        {
            var entityParameter = Expression.Parameter(typeof(TEntity), "e");

            return Expression.Lambda<Func<TEntity, bool>>(
                ExpressionExtensions.BuildPredicate(keyProperties, keyValues, entityParameter), entityParameter);
        }

        private static Expression<Func<object, bool>> BuildObjectLambda(IReadOnlyList<IProperty> keyProperties, ValueBuffer keyValues)
        {
            var entityParameter = Expression.Parameter(typeof(object), "e");

            return Expression.Lambda<Func<object, bool>>(
                ExpressionExtensions.BuildPredicate(keyProperties, keyValues, entityParameter), entityParameter);
        }

        private IQueryable BuildQueryRoot(IEntityType entityType)
        {
            return entityType.DefiningEntityType is IEntityType definingEntityType
                ? BuildQueryRoot(definingEntityType, entityType, entityType.DefiningNavigationName)
                : entityType.FindOwnership() is IForeignKey ownership
                    ? BuildQueryRoot(ownership.PrincipalEntityType, entityType, ownership.PrincipalToDependent.Name)
                    : entityType.HasSharedClrType
                        ? (IQueryable)_setCache.GetOrAddSet(_setSource, entityType.Name, entityType.ClrType)
                        : (IQueryable)_setCache.GetOrAddSet(_setSource, entityType.ClrType);
        }

        private IQueryable BuildQueryRoot(IEntityType ownerOrDefiningEntityType, IEntityType entityType, string navigationName)
        {
            var queryRoot = BuildQueryRoot(ownerOrDefiningEntityType);
            var collectionNavigation = ownerOrDefiningEntityType.FindNavigation(navigationName).IsCollection;

            return (IQueryable)(collectionNavigation ? _selectManyMethod : _selectMethod)
                .MakeGenericMethod(ownerOrDefiningEntityType.ClrType, entityType.ClrType)
                .Invoke(null, new object[] { queryRoot, navigationName });
        }

        private static readonly MethodInfo _selectMethod
            = typeof(EntityFinder<TEntity>).GetTypeInfo().GetDeclaredMethods(nameof(Select)).Single(mi => mi.IsGenericMethodDefinition);

        private static IQueryable<TResult> Select<TSource, TResult>(
            [NotNull] IQueryable<TSource> source,
            [NotNull] string propertyName)
            where TResult : class
            where TSource : class
        {
            var parameter = Expression.Parameter(typeof(TSource), "e");
            return source.Select(
                Expression.Lambda<Func<TSource, TResult>>(
                    Expression.MakeMemberAccess(parameter, typeof(TSource).GetAnyProperty(propertyName)),
                    parameter));
        }

        private static readonly MethodInfo _selectManyMethod
            = typeof(EntityFinder<TEntity>).GetTypeInfo().GetDeclaredMethods(nameof(SelectMany)).Single(mi => mi.IsGenericMethodDefinition);

        private static IQueryable<TResult> SelectMany<TSource, TResult>(
            [NotNull] IQueryable<TSource> source,
            [NotNull] string propertyName)
            where TResult : class
            where TSource : class
        {
            var parameter = Expression.Parameter(typeof(TSource), "e");
            return source.SelectMany(
                Expression.Lambda<Func<TSource, IEnumerable<TResult>>>(
                    Expression.MakeMemberAccess(parameter, typeof(TSource).GetAnyProperty(propertyName)),
                    parameter));
        }

        private static Expression<Func<object, object[]>> BuildProjection(IEntityType entityType)
        {
            var entityParameter = Expression.Parameter(typeof(object), "e");

            var projections = new List<Expression>();
            foreach (var property in entityType.GetProperties())
            {
                projections.Add(
                    Expression.Convert(
                        Expression.Convert(
                            Expression.Call(
                                EF.PropertyMethod.MakeGenericMethod(property.ClrType),
                                entityParameter,
                                Expression.Constant(property.Name, typeof(string))),
                            property.ClrType),
                        typeof(object)));
            }

            return Expression.Lambda<Func<object, object[]>>(
                Expression.NewArrayInit(typeof(object), projections),
                entityParameter);
        }
    }
}
