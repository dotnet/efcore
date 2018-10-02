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
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityFinder<TEntity> : IEntityFinder<TEntity>
        where TEntity : class
    {
        private readonly IStateManager _stateManager;
        private readonly IDbSetSource _setSource;
        private readonly IDbSetCache _setCache;
        private readonly IModel _model;
        private readonly IQueryable<TEntity> _queryRoot;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
            _model = entityType.Model;
            _queryRoot = (IQueryable<TEntity>)BuildQueryRoot(entityType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TEntity Find(object[] keyValues)
        {
            return keyValues?.Any(v => v == null) != false
                ? null
                : FindTracked(keyValues, out var keyProperties)
                   ?? _queryRoot.FirstOrDefault(BuildLambda(keyProperties, new ValueBuffer(keyValues)));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        object IEntityFinder.Find(object[] keyValues)
            => Find(keyValues);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken = default)
        {
            if (keyValues?.Any(v => v == null) != false)
            {
                return Task.FromResult<TEntity>(null);
            }

            var tracked = FindTracked(keyValues, out var keyProperties);
            return tracked != null
                ? Task.FromResult(tracked)
                : _queryRoot.FirstOrDefaultAsync(BuildLambda(keyProperties, new ValueBuffer(keyValues)), cancellationToken);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        Task<object> IEntityFinder.FindAsync(object[] keyValues, CancellationToken cancellationToken)
        {
            if (keyValues?.Any(v => v == null) != false)
            {
                return Task.FromResult<object>(null);
            }

            var tracked = FindTracked(keyValues, out var keyProperties);
            return tracked != null
                ? Task.FromResult((object)tracked)
                : _queryRoot.FirstOrDefaultAsync(
                    BuildObjectLambda(keyProperties, new ValueBuffer(keyValues)), cancellationToken);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
                await Query(navigation, keyValues).LoadAsync(cancellationToken);
            }

            entry.SetIsLoaded(navigation);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object[] GetDatabaseValues(InternalEntityEntry entry)
            => GetDatabaseValuesQuery(entry)?.FirstOrDefault();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
#pragma warning disable RCS1210 // Return Task.FromResult instead of returning null.
        public virtual Task<object[]> GetDatabaseValuesAsync(
            InternalEntityEntry entry, CancellationToken cancellationToken = default)
            => GetDatabaseValuesQuery(entry)?.FirstOrDefaultAsync(cancellationToken);
#pragma warning restore RCS1210 // Return Task.FromResult instead of returning null.

        private IQueryable<object[]> GetDatabaseValuesQuery(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;
            var properties = entityType.FindPrimaryKey().Properties;

            var keyValues = new object[properties.Count];
            for (var i = 0; i < keyValues.Length; i++)
            {
                keyValues[i] = entry[properties[i]];
                if (keyValues[i] == null)
                {
                    return null;
                }
            }

            return _queryRoot.AsNoTracking().IgnoreQueryFilters()
                .Where(BuildObjectLambda(properties, new ValueBuffer(keyValues)))
                .Select(BuildProjection(entityType));
        }

        private IQueryable<TEntity> Query(INavigation navigation, object[] keyValues)
            => _queryRoot.Where(BuildLambda(GetLoadProperties(navigation), new ValueBuffer(keyValues))).AsTracking();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        IQueryable IEntityFinder.Query(INavigation navigation, InternalEntityEntry entry)
            => Query(navigation, entry);

        private static object[] GetLoadValues(INavigation navigation, InternalEntityEntry entry)
        {
            var properties = navigation.IsDependentToPrincipal()
                ? navigation.ForeignKey.Properties
                : navigation.ForeignKey.PrincipalKey.Properties;

            var values = new object[properties.Count];

            for (var i = 0; i < values.Length; i++)
            {
                values[i] = entry[properties[i]];
                if (values[i] == null)
                {
                    return null;
                }
            }

            return values;
        }

        private static IReadOnlyList<IProperty> GetLoadProperties(INavigation navigation)
            => navigation.IsDependentToPrincipal()
                ? navigation.ForeignKey.PrincipalKey.Properties
                : navigation.ForeignKey.Properties;

        private TEntity FindTracked(object[] keyValues, out IReadOnlyList<IProperty> keyProperties)
        {
            var key = _model.FindEntityType(typeof(TEntity)).FindPrimaryKey();
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
                BuildPredicate(keyProperties, keyValues, entityParameter), entityParameter);
        }

        private static Expression<Func<object, bool>> BuildObjectLambda(IReadOnlyList<IProperty> keyProperties, ValueBuffer keyValues)
        {
            var entityParameter = Expression.Parameter(typeof(object), "e");

            return Expression.Lambda<Func<object, bool>>(
                BuildPredicate(keyProperties, keyValues, entityParameter), entityParameter);
        }

        private IQueryable BuildQueryRoot(IEntityType entityType)
        {
            var definingEntityType = entityType.DefiningEntityType;
            return definingEntityType == null
                ? (IQueryable)_setCache.GetOrAddSet(_setSource, entityType.ClrType)
                : BuildQueryRoot(definingEntityType, entityType);
        }

        private IQueryable BuildQueryRoot(IEntityType definingEntityType, IEntityType entityType)
        {
            var queryRoot = BuildQueryRoot(definingEntityType);

            return (IQueryable)_selectMethod.MakeGenericMethod(definingEntityType.ClrType, entityType.ClrType)
                .Invoke(null, new object[] { queryRoot, entityType.DefiningNavigationName });
        }

        private static readonly MethodInfo _selectMethod
            = typeof(EntityFinder<TEntity>).GetTypeInfo().GetDeclaredMethods(nameof(Select)).Single(mi => mi.IsGenericMethodDefinition);

        private static IQueryable<TResult> Select<TSource, TResult>(
            [NotNull] IQueryable<TSource> source, [NotNull] string propertyName)
            where TResult : class
            where TSource : class
        {
            var parameter = Expression.Parameter(typeof(TSource), "e");
            return source.Select(
                Expression.Lambda<Func<TSource, TResult>>(
                    Expression.MakeMemberAccess(parameter, typeof(TSource).GetAnyProperty(propertyName)),
                    parameter));
        }

        private static BinaryExpression BuildPredicate(
            IReadOnlyList<IProperty> keyProperties,
            ValueBuffer keyValues,
            ParameterExpression entityParameter)
        {
            var keyValuesConstant = Expression.Constant(keyValues);

            BinaryExpression predicate = null;
            for (var i = 0; i < keyProperties.Count; i++)
            {
                var property = keyProperties[i];
                var equalsExpression =
                    Expression.Equal(
                        Expression.Call(
                            EF.PropertyMethod.MakeGenericMethod(property.ClrType),
                            entityParameter,
                            Expression.Constant(property.Name, typeof(string))),
                        Expression.Convert(
                            Expression.Call(
                                keyValuesConstant,
                                ValueBuffer.GetValueMethod,
                                Expression.Constant(i)),
                            property.ClrType));

                predicate = predicate == null ? equalsExpression : Expression.AndAlso(predicate, equalsExpression);
            }

            return predicate;
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
