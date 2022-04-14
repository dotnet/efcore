// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ClrCollectionAccessorFactory
{
    private static readonly MethodInfo GenericCreate
        = typeof(ClrCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateGeneric))!;

    private static readonly MethodInfo CreateAndSetMethod
        = typeof(ClrCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateAndSet))!;

    private static readonly MethodInfo CreateMethod
        = typeof(ClrCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateCollection))!;

    private static readonly MethodInfo CreateAndSetHashSetMethod
        = typeof(ClrCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateAndSetHashSet))!;

    private static readonly MethodInfo CreateHashSetMethod
        = typeof(ClrCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateHashSet))!;

    private static readonly MethodInfo CreateAndSetObservableHashSetMethod
        = typeof(ClrCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateAndSetObservableHashSet))!;

    private static readonly MethodInfo CreateObservableHashSetMethod
        = typeof(ClrCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateObservableHashSet))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IClrCollectionAccessor? Create(INavigationBase navigation)
        => !navigation.IsCollection || navigation.IsShadowProperty() ? null : Create(navigation, navigation.TargetEntityType);

    private static IClrCollectionAccessor? Create(IPropertyBase navigation, IEntityType? targetType)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (navigation is IClrCollectionAccessor accessor)
        {
            return accessor;
        }

        if (targetType == null)
        {
            return null;
        }

        var memberInfo = GetMostDerivedMemberInfo();
        var propertyType = navigation.IsIndexerProperty() ? navigation.ClrType : memberInfo.GetMemberType();
        var elementType = propertyType.TryGetElementType(typeof(IEnumerable<>));

        if (elementType == null)
        {
            throw new InvalidOperationException(
                CoreStrings.NavigationBadType(
                    navigation.Name,
                    navigation.DeclaringType.DisplayName(),
                    propertyType.ShortDisplayName(),
                    targetType.DisplayName()));
        }

        if (propertyType.IsArray)
        {
            throw new InvalidOperationException(
                CoreStrings.NavigationArray(
                    navigation.Name,
                    navigation.DeclaringType.DisplayName(),
                    propertyType.ShortDisplayName()));
        }

        var boundMethod = GenericCreate.MakeGenericMethod(
            memberInfo.DeclaringType!, propertyType, elementType);

        try
        {
            return (IClrCollectionAccessor?)boundMethod.Invoke(
                null, new object[] { navigation });
        }
        catch (TargetInvocationException invocationException)
        {
            throw invocationException.InnerException!;
        }

        MemberInfo GetMostDerivedMemberInfo()
        {
            var propertyInfo = navigation.PropertyInfo;
            var fieldInfo = navigation.FieldInfo;

            return (fieldInfo == null
                ? propertyInfo
                : propertyInfo == null
                    ? fieldInfo
                    : fieldInfo.FieldType.IsAssignableFrom(propertyInfo.PropertyType)
                        ? propertyInfo
                        : fieldInfo)!;
        }
    }

    [UsedImplicitly]
    private static IClrCollectionAccessor CreateGeneric<TEntity, TCollection, TElement>(INavigationBase navigation)
        where TEntity : class
        where TCollection : class, IEnumerable<TElement>
        where TElement : class
    {
        var entityParameter = Expression.Parameter(typeof(TEntity), "entity");
        var valueParameter = Expression.Parameter(typeof(TCollection), "collection");

        var memberInfoForRead = navigation.GetMemberInfo(forMaterialization: false, forSet: false);
        navigation.TryGetMemberInfo(forConstruction: false, forSet: true, out var memberInfoForWrite, out _);
        navigation.TryGetMemberInfo(forConstruction: true, forSet: true, out var memberInfoForMaterialization, out _);

        var memberAccessForRead = (Expression)Expression.MakeMemberAccess(entityParameter, memberInfoForRead);
        if (memberAccessForRead.Type != typeof(TCollection))
        {
            memberAccessForRead = Expression.Convert(memberAccessForRead, typeof(TCollection));
        }

        var getterDelegate = Expression.Lambda<Func<TEntity, TCollection>>(
            memberAccessForRead,
            entityParameter).Compile();

        Action<TEntity, TCollection>? setterDelegate = null;
        Action<TEntity, TCollection>? setterDelegateForMaterialization = null;
        Func<TEntity, Action<TEntity, TCollection>, TCollection>? createAndSetDelegate = null;
        Func<TCollection>? createDelegate = null;

        if (memberInfoForWrite != null)
        {
            setterDelegate = CreateSetterDelegate(entityParameter, memberInfoForWrite, valueParameter);
        }

        if (memberInfoForMaterialization != null)
        {
            setterDelegateForMaterialization = CreateSetterDelegate(entityParameter, memberInfoForMaterialization, valueParameter);
        }

        var concreteType = new CollectionTypeFactory().TryFindTypeToInstantiate(
            typeof(TEntity),
            typeof(TCollection),
            navigation.DeclaringEntityType.Model[CoreAnnotationNames.FullChangeTrackingNotificationsRequired] != null);

        if (concreteType != null)
        {
            var isHashSet = concreteType.IsGenericType && concreteType.GetGenericTypeDefinition() == typeof(HashSet<>);
            if (setterDelegate != null
                || setterDelegateForMaterialization != null)
            {
                if (isHashSet)
                {
                    createAndSetDelegate = (Func<TEntity, Action<TEntity, TCollection>, TCollection>)CreateAndSetHashSetMethod
                        .MakeGenericMethod(typeof(TEntity), typeof(TCollection), typeof(TElement))
                        .CreateDelegate(typeof(Func<TEntity, Action<TEntity, TCollection>, TCollection>));
                }
                else if (IsObservableHashSet(concreteType))
                {
                    createAndSetDelegate = (Func<TEntity, Action<TEntity, TCollection>, TCollection>)CreateAndSetObservableHashSetMethod
                        .MakeGenericMethod(typeof(TEntity), typeof(TCollection), typeof(TElement))
                        .CreateDelegate(typeof(Func<TEntity, Action<TEntity, TCollection>, TCollection>));
                }
                else
                {
                    createAndSetDelegate = (Func<TEntity, Action<TEntity, TCollection>, TCollection>)CreateAndSetMethod
                        .MakeGenericMethod(typeof(TEntity), typeof(TCollection), concreteType)
                        .CreateDelegate(typeof(Func<TEntity, Action<TEntity, TCollection>, TCollection>));
                }
            }

            if (isHashSet)
            {
                createDelegate = (Func<TCollection>)CreateHashSetMethod
                    .MakeGenericMethod(typeof(TCollection), typeof(TElement))
                    .CreateDelegate(typeof(Func<TCollection>));
            }
            else if (IsObservableHashSet(concreteType))
            {
                createDelegate = (Func<TCollection>)CreateObservableHashSetMethod
                    .MakeGenericMethod(typeof(TCollection), typeof(TElement))
                    .CreateDelegate(typeof(Func<TCollection>));
            }
            else
            {
                createDelegate = (Func<TCollection>)CreateMethod
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

        static Action<TEntity, TCollection> CreateSetterDelegate(
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
        var collection = (TCollection)(ICollection<TElement>)new HashSet<TElement>(LegacyReferenceEqualityComparer.Instance);
        setterDelegate(entity, collection);
        return collection;
    }

    [UsedImplicitly]
    private static TCollection CreateHashSet<TCollection, TElement>()
        where TCollection : class
        where TElement : class
        => (TCollection)(ICollection<TElement>)new HashSet<TElement>(LegacyReferenceEqualityComparer.Instance);

    [UsedImplicitly]
    private static TCollection CreateAndSetObservableHashSet<TEntity, TCollection, TElement>(
        TEntity entity,
        Action<TEntity, TCollection> setterDelegate)
        where TEntity : class
        where TCollection : class
        where TElement : class
    {
        var collection = (TCollection)(ICollection<TElement>)new ObservableHashSet<TElement>(LegacyReferenceEqualityComparer.Instance);
        setterDelegate(entity, collection);
        return collection;
    }

    [UsedImplicitly]
    private static TCollection CreateObservableHashSet<TCollection, TElement>()
        where TCollection : class
        where TElement : class
        => (TCollection)(ICollection<TElement>)new ObservableHashSet<TElement>(LegacyReferenceEqualityComparer.Instance);
}
