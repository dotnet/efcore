// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityFinder<TEntity> : IEntityFinder<TEntity>
        where TEntity : class
    {
        private readonly IModel _model;
        private readonly IStateManager _stateManager;
        private readonly DbSet<TEntity> _set;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityFinder([NotNull] DbContext context)
        {
            _model = context.Model;
            _stateManager = context.GetService<IStateManager>();
            _set = context.Set<TEntity>();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TEntity Find(object[] keyValues)
        {
            Check.NotNull(keyValues, nameof(keyValues));

            IReadOnlyList<IProperty> keyProperties;
            return FindTracked(keyValues, out keyProperties)
                   ?? _set.FirstOrDefault(BuildLambda(keyProperties, new ValueBuffer(keyValues)));
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
        public virtual Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(keyValues, nameof(keyValues));

            IReadOnlyList<IProperty> keyProperties;
            var tracked = FindTracked(keyValues, out keyProperties);
            return tracked != null
                ? Task.FromResult(tracked)
                : _set.FirstOrDefaultAsync(BuildLambda(keyProperties, new ValueBuffer(keyValues)), cancellationToken);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        Task<object> IEntityFinder.FindAsync(object[] keyValues, CancellationToken cancellationToken)
        {
            Check.NotNull(keyValues, nameof(keyValues));

            IReadOnlyList<IProperty> keyProperties;
            var tracked = FindTracked(keyValues, out keyProperties);
            return tracked != null
                ? Task.FromResult((object)tracked)
                : _set.FirstOrDefaultAsync(
                    BuildObjectLambda(keyProperties, new ValueBuffer(keyValues)), cancellationToken);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Load(INavigation navigation, InternalEntityEntry entry)
        {
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
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Short-circuit for any null key values for perf and because of #6129
            var keyValues = GetLoadValues(navigation, entry);
            if (keyValues != null)
            {
                // TODO: Replace with LoadAsync when Issue #6122 is fixed
                await Query(navigation, keyValues).ToListAsync(cancellationToken);
            }

            entry.SetIsLoaded(navigation);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IQueryable<TEntity> Query(INavigation navigation, InternalEntityEntry entry)
        {
            var keyValues = GetLoadValues(navigation, entry);
            // Short-circuit for any null key values for perf and because of #6129
            if (keyValues == null)
            {
                // Creates an empty Queryable that works with Async. Has to be an EF query because it
                // could be used in a composition.
                return _set.Where(e => false);
            }

            return Query(navigation, keyValues);
        }

        private IQueryable<TEntity> Query(INavigation navigation, object[] keyValues) 
            => _set.Where(BuildLambda(GetLoadProperties(navigation), new ValueBuffer(keyValues)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        IQueryable IEntityFinder.Query(INavigation navigation, InternalEntityEntry entry)
            => Query(navigation, entry);

        private object[] GetLoadValues(INavigation navigation, InternalEntityEntry entry)
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

        private IReadOnlyList<IProperty> GetLoadProperties(INavigation navigation)
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
    }
}
