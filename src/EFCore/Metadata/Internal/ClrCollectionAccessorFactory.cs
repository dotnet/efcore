// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

        private static readonly MethodInfo _createAndSetHashSet
            = typeof(ClrCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateAndSetHashSet));

        private static readonly MethodInfo _createHashSet
            = typeof(ClrCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateHashSet));

        private static readonly MethodInfo _createAndSetObservableHashSet
            = typeof(ClrCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateAndSetObservableHashSet));

        private static readonly MethodInfo _createObservableHashSet
            = typeof(ClrCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateObservableHashSet));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IClrCollectionAccessor Create([NotNull] INavigation navigation)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (navigation is IClrCollectionAccessor accessor)
            {
                return accessor;
            }

            var property = navigation.GetIdentifyingMemberInfo();
            var propertyType = property.GetMemberType();
            var elementType = propertyType.TryGetElementType(typeof(IEnumerable<>));

            if (elementType == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NavigationBadType(
                        navigation.Name,
                        navigation.DeclaringEntityType.DisplayName(),
                        propertyType.ShortDisplayName(),
                        navigation.GetTargetType().DisplayName()));
            }

            if (propertyType.IsArray)
            {
                throw new InvalidOperationException(
                    CoreStrings.NavigationArray(
                        navigation.Name,
                        navigation.DeclaringEntityType.DisplayName(),
                        propertyType.ShortDisplayName()));
            }

            var boundMethod = _genericCreate.MakeGenericMethod(
                property.DeclaringType, propertyType, elementType);

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

            var memberAccess = (Expression)Expression.MakeMemberAccess(entityParameter, memberInfo);
            if (memberAccess.Type != typeof(TCollection))
            {
                memberAccess = Expression.Convert(memberAccess, typeof(TCollection));
            }

            var getterDelegate = Expression.Lambda<Func<TEntity, TCollection>>(
                memberAccess,
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

            var concreteType = new CollectionTypeFactory().TryFindTypeToInstantiate(typeof(TEntity), typeof(TCollection));
            if (concreteType != null)
            {
                var isHashSet = concreteType.IsGenericType && concreteType.GetGenericTypeDefinition() == typeof(HashSet<>);
                if (setterDelegate != null)
                {
                    if (isHashSet)
                    {
                        createAndSetDelegate = (Func<TEntity, Action<TEntity, TCollection>, TCollection>)_createAndSetHashSet
                            .MakeGenericMethod(typeof(TEntity), typeof(TCollection), typeof(TElement))
                            .CreateDelegate(typeof(Func<TEntity, Action<TEntity, TCollection>, TCollection>));
                    }
                    else if (IsObservableHashSet(concreteType))
                    {
                        createAndSetDelegate = (Func<TEntity, Action<TEntity, TCollection>, TCollection>)_createAndSetObservableHashSet
                            .MakeGenericMethod(typeof(TEntity), typeof(TCollection), typeof(TElement))
                            .CreateDelegate(typeof(Func<TEntity, Action<TEntity, TCollection>, TCollection>));
                    }
                    else
                    {
                        createAndSetDelegate = (Func<TEntity, Action<TEntity, TCollection>, TCollection>)_createAndSet
                            .MakeGenericMethod(typeof(TEntity), typeof(TCollection), concreteType)
                            .CreateDelegate(typeof(Func<TEntity, Action<TEntity, TCollection>, TCollection>));
                    }
                }

                if (isHashSet)
                {
                    createDelegate = (Func<TCollection>)_createHashSet
                        .MakeGenericMethod(typeof(TCollection), typeof(TElement))
                        .CreateDelegate(typeof(Func<TCollection>));
                }
                else if (IsObservableHashSet(concreteType))
                {
                    createDelegate = (Func<TCollection>)_createObservableHashSet
                        .MakeGenericMethod(typeof(TCollection), typeof(TElement))
                        .CreateDelegate(typeof(Func<TCollection>));
                }
                else
                {
                    createDelegate = (Func<TCollection>)_create
                        .MakeGenericMethod(typeof(TCollection), concreteType)
                        .CreateDelegate(typeof(Func<TCollection>));
                }
            }

            return new ClrICollectionAccessor<TEntity, TCollection, TElement>(
                navigation.Name, getterDelegate, setterDelegate, createAndSetDelegate, createDelegate);
        }

        private static bool IsObservableHashSet(Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ObservableHashSet<>);

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

        [UsedImplicitly]
        private static TCollection CreateAndSetHashSet<TEntity, TCollection, TElement>(
            TEntity entity,
            Action<TEntity, TCollection> setterDelegate)
            where TEntity : class
            where TCollection : class
            where TElement : class
        {
            var collection = (TCollection)(ICollection<TElement>)new HashSet<TElement>(ReferenceEqualityComparer.Instance);
            setterDelegate(entity, collection);
            return collection;
        }

        [UsedImplicitly]
        private static TCollection CreateHashSet<TCollection, TElement>()
            where TCollection : class
            where TElement : class
            => (TCollection)(ICollection<TElement>)new HashSet<TElement>(ReferenceEqualityComparer.Instance);

        [UsedImplicitly]
        private static TCollection CreateAndSetObservableHashSet<TEntity, TCollection, TElement>(
            TEntity entity,
            Action<TEntity, TCollection> setterDelegate)
            where TEntity : class
            where TCollection : class
            where TElement : class
        {
            var collection = (TCollection)(ICollection<TElement>)new ObservableHashSet<TElement>(ReferenceEqualityComparer.Instance);
            setterDelegate(entity, collection);
            return collection;
        }

        [UsedImplicitly]
        private static TCollection CreateObservableHashSet<TCollection, TElement>()
            where TCollection : class
            where TElement : class
            => (TCollection)(ICollection<TElement>)new ObservableHashSet<TElement>(ReferenceEqualityComparer.Instance);
    }
}
