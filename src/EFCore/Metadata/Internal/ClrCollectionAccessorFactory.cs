// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ClrCollectionAccessorFactory
    {
        private static readonly MethodInfo _genericCreate
            = typeof(ClrCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateGeneric));

        private static readonly MethodInfo _createAndSet
            = typeof(ClrCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateAndSet));

        private static readonly MethodInfo _create
            = typeof(ClrCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateCollection));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IClrCollectionAccessor Create([NotNull] INavigation navigation)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var accessor = navigation as IClrCollectionAccessor;
            if (accessor != null)
            {
                return accessor;
            }

            var property = navigation.PropertyInfo;
            var elementType = property.PropertyType.TryGetElementType(typeof(IEnumerable<>));

            // TODO: Only ICollections supported; add support for enumerables with add/remove methods
            // Issue #752
            if (elementType == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NavigationBadType(
                        navigation.Name,
                        navigation.DeclaringEntityType.DisplayName(),
                        property.PropertyType.ShortDisplayName(),
                        navigation.GetTargetType().DisplayName()));
            }

            if (property.PropertyType.IsArray)
            {
                throw new InvalidOperationException(
                    CoreStrings.NavigationArray(
                        navigation.Name,
                        navigation.DeclaringEntityType.DisplayName(),
                        property.PropertyType.ShortDisplayName()));
            }

            var boundMethod = _genericCreate.MakeGenericMethod(
                property.DeclaringType, property.PropertyType, elementType);

            var memberInfo = navigation.GetMemberInfo(forConstruction: false, forSet: false);

            return (IClrCollectionAccessor)boundMethod.Invoke(null, new object[] { navigation, memberInfo });
        }

        [UsedImplicitly]
        private static IClrCollectionAccessor CreateGeneric<TEntity, TCollection, TElement>(INavigation navigation, MemberInfo memberInfo)
            where TEntity : class
            where TCollection : class, IEnumerable<TElement>
        {
            var entityParameter = Expression.Parameter(typeof(TEntity), "entity");
            var valueParameter = Expression.Parameter(typeof(TCollection), "collection");

            var getterDelegate = Expression.Lambda<Func<TEntity, TCollection>>(
                Expression.MakeMemberAccess(
                    entityParameter,
                    memberInfo),
                entityParameter).Compile();

            Action<TEntity, TCollection> setterDelegate = null;
            Func<TEntity, Action<TEntity, TCollection>, TCollection> createAndSetDelegate = null;
            Func<TCollection> createDelegate = null;

            var setterMemberInfo = navigation.GetMemberInfo(forConstruction: false, forSet: true);
            if (setterMemberInfo != null)
            {
                setterDelegate = Expression.Lambda<Action<TEntity, TCollection>>(
                    Expression.Assign(
                        Expression.MakeMemberAccess(
                            entityParameter,
                            setterMemberInfo),
                        Expression.Convert(
                            valueParameter,
                            setterMemberInfo.GetMemberType())),
                    entityParameter,
                    valueParameter).Compile();
            }

            if (setterDelegate != null)
            {
                var concreteType = new CollectionTypeFactory().TryFindTypeToInstantiate(typeof(TEntity), typeof(TCollection));

                if (concreteType != null)
                {
                    createAndSetDelegate = (Func<TEntity, Action<TEntity, TCollection>, TCollection>)_createAndSet
                        .MakeGenericMethod(typeof(TEntity), typeof(TCollection), concreteType)
                        .CreateDelegate(typeof(Func<TEntity, Action<TEntity, TCollection>, TCollection>));

                    createDelegate = (Func<TCollection>)_create
                        .MakeGenericMethod(typeof(TCollection), concreteType)
                        .CreateDelegate(typeof(Func<TCollection>));
                }
            }

            return new ClrICollectionAccessor<TEntity, TCollection, TElement>(
                navigation.Name, getterDelegate, setterDelegate, createAndSetDelegate, createDelegate);
        }

        [UsedImplicitly]
        private static TCollection CreateAndSet<TEntity, TCollection, TConcreteCollection>(
            TEntity entity,
            Action<TEntity, TCollection> setterDelegate)
            where TEntity : class
            where TCollection : class
            where TConcreteCollection : TCollection, new()
        {
            var collection = new TConcreteCollection();
            setterDelegate(entity, collection);
            return collection;
        }

        [UsedImplicitly]
        private static TCollection CreateCollection<TCollection, TConcreteCollection>()
            where TCollection : class
            where TConcreteCollection : TCollection, new()
            => new TConcreteCollection();
    }
}
