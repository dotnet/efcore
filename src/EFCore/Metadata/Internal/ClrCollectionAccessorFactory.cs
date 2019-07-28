// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IClrCollectionAccessor Create([NotNull] INavigation navigation)
        {
            MemberInfo GetMostDerivedMemberInfo()
            {
                var propertyInfo = navigation.PropertyInfo;
                var fieldInfo = navigation.FieldInfo;

                return fieldInfo == null
                    ? propertyInfo
                    : propertyInfo == null
                        ? fieldInfo
                        : fieldInfo.FieldType.IsAssignableFrom(propertyInfo.PropertyType)
                            ? (MemberInfo)propertyInfo
                            : fieldInfo;
            }

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (navigation is IClrCollectionAccessor accessor)
            {
                return accessor;
            }

            var memberInfo = GetMostDerivedMemberInfo();
            var propertyType = memberInfo.GetMemberType();
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
                memberInfo.DeclaringType, propertyType, elementType);

            try
            {
                return (IClrCollectionAccessor)boundMethod.Invoke(
                    null, new object[]
                    {
                        navigation
                    });
            }
            catch (TargetInvocationException invocationException)
            {
                throw invocationException.InnerException;
            }
        }

        [UsedImplicitly]
        private static IClrCollectionAccessor CreateGeneric<TEntity, TCollection, TElement>(INavigation navigation)
            where TEntity : class
            where TCollection : class, IEnumerable<TElement>
            where TElement : class
        {
            Action<TEntity, TCollection> CreateSetterDelegate(
                ParameterExpression parameterExpression,
                MemberInfo memberInfo,
                ParameterExpression valueParameter1)
                => Expression.Lambda<Action<TEntity, TCollection>>(
                    Expression.MakeMemberAccess(
                        parameterExpression,
                        memberInfo).Assign(
                        Expression.Convert(
                            valueParameter1,
                            memberInfo.GetMemberType())),
                    parameterExpression,
                    valueParameter1).Compile();

            var entityParameter = Expression.Parameter(typeof(TEntity), "entity");
            var valueParameter = Expression.Parameter(typeof(TCollection), "collection");

            var memberInfoForRead = navigation.GetMemberInfo(forMaterialization: false, forSet: false);
            var memberInfoForWrite = navigation.GetMemberInfo(forMaterialization: false, forSet: true);
            var memberInfoForMaterialization = navigation.GetMemberInfo(forMaterialization: true, forSet: true);

            var memberAccessForRead = (Expression)Expression.MakeMemberAccess(entityParameter, memberInfoForRead);
            if (memberAccessForRead.Type != typeof(TCollection))
            {
                memberAccessForRead = Expression.Convert(memberAccessForRead, typeof(TCollection));
            }

            var getterDelegate = Expression.Lambda<Func<TEntity, TCollection>>(
                memberAccessForRead,
                entityParameter).Compile();

            Action<TEntity, TCollection> setterDelegate = null;
            Action<TEntity, TCollection> setterDelegateForMaterialization = null;
            Func<TEntity, Action<TEntity, TCollection>, TCollection> createAndSetDelegate = null;
            Func<TCollection> createDelegate = null;

            if (memberInfoForWrite != null)
            {
                setterDelegate = CreateSetterDelegate(entityParameter, memberInfoForWrite, valueParameter);
            }

            if (memberInfoForMaterialization != null)
            {
                setterDelegateForMaterialization = CreateSetterDelegate(entityParameter, memberInfoForMaterialization, valueParameter);
            }

            var concreteType = new CollectionTypeFactory().TryFindTypeToInstantiate(typeof(TEntity), typeof(TCollection));
            if (concreteType != null)
            {
                var isHashSet = concreteType.IsGenericType && concreteType.GetGenericTypeDefinition() == typeof(HashSet<>);
                if (setterDelegate != null
                    || setterDelegateForMaterialization != null)
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
                navigation.Name,
                getterDelegate,
                setterDelegate,
                setterDelegateForMaterialization,
                createAndSetDelegate,
                createDelegate);
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
