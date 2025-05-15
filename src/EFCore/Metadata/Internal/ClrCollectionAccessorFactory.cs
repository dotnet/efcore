// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.ExceptionServices;
using JetBrains.Annotations;

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

    private static readonly MethodInfo CreateAndSetHashSetMethod
        = typeof(ClrCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateAndSetHashSet))!;

    private static readonly MethodInfo CreateAndSetObservableHashSetMethod
        = typeof(ClrCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateAndSetObservableHashSet))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly ClrCollectionAccessorFactory Instance = new();

    private ClrCollectionAccessorFactory()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IClrCollectionAccessor? Create(INavigationBase navigation)
    {
        if (!navigation.IsCollection)
        {
            return null;
        }

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (navigation is IClrCollectionAccessor accessor)
        {
            return accessor;
        }

        var targetType = navigation.TargetEntityType;
        if (targetType == null)
        {
            return null;
        }

        var memberInfo = GetMostDerivedMemberInfo(navigation);
        var propertyType = navigation.IsIndexerProperty() || navigation.IsShadowProperty()
            ? navigation.ClrType
            : memberInfo!.GetMemberType();

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
            memberInfo?.DeclaringType ?? navigation.DeclaringType.ClrType, propertyType, elementType);

        try
        {
            return (IClrCollectionAccessor?)boundMethod.Invoke(null, [navigation]);
        }
        catch (TargetInvocationException invocationException)
        {
            throw invocationException.InnerException!;
        }
    }

    [UsedImplicitly]
    private static IClrCollectionAccessor CreateGeneric<TEntity, TCollection, TElement>(INavigationBase navigation)
        where TEntity : class
        where TCollection : class, IEnumerable<TElement>
        where TElement : class
    {
        CreateExpressions<TEntity, TCollection, TElement>(
            navigation,
            out var getCollection,
            out var setCollection,
            out var setCollectionForMaterialization,
            out var createAndSetCollection,
            out var createCollection);

        return new ClrICollectionAccessor<TEntity, TCollection, TElement>(
            navigation.Name,
            navigation.IsShadowProperty(),
            getCollection?.Compile(),
            setCollection?.Compile(),
            setCollectionForMaterialization?.Compile(),
            createAndSetCollection?.Compile(),
            createCollection?.Compile());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Create(
        INavigationBase navigation,
        out Type entityType,
        out Type propertyType,
        out Type elementType,
        out Expression? getCollection,
        out Expression? setCollection,
        out Expression? setCollectionForMaterialization,
        out Expression? createAndSetCollection,
        out Expression? createCollection)
    {
        var memberInfo = GetMostDerivedMemberInfo(navigation);
        entityType = memberInfo?.DeclaringType ?? navigation.DeclaringType.ClrType;
        propertyType = navigation.IsIndexerProperty() || navigation.IsShadowProperty()
            ? navigation.ClrType
            : memberInfo!.GetMemberType();

        elementType = propertyType.TryGetElementType(typeof(IEnumerable<>))!;

        var boundMethod = GenericCreateExpressions.MakeGenericMethod(entityType, propertyType, elementType);

        try
        {
            var parameters = new object?[] { navigation, null, null, null, null, null };
            boundMethod.Invoke(this, parameters);
            getCollection = (Expression)parameters[1]!;
            setCollection = (Expression)parameters[2]!;
            setCollectionForMaterialization = (Expression?)parameters[3];
            createAndSetCollection = (Expression)parameters[4]!;
            createCollection = (Expression?)parameters[5];
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

    private static readonly MethodInfo GenericCreateExpressions
        = typeof(ClrCollectionAccessorFactory).GetMethod(nameof(CreateExpressions), BindingFlags.Static | BindingFlags.NonPublic)!;

    [UsedImplicitly]
    private static void CreateExpressions<TEntity, TCollection, TElement>(
        INavigationBase navigation,
        out Expression<Func<TEntity, TCollection>>? getCollection,
        out Expression<Action<TEntity, TCollection>>? setCollection,
        out Expression<Action<TEntity, TCollection>>? setCollectionForMaterialization,
        out Expression<Func<TEntity, Action<TEntity, TCollection>, TCollection>>? createAndSetCollection,
        out Expression<Func<TCollection>>? createCollection)
        where TEntity : class
        where TCollection : class, IEnumerable<TElement>
        where TElement : class
    {
        getCollection = null;
        setCollection = null;
        setCollectionForMaterialization = null;
        createAndSetCollection = null;
        createCollection = null;

        var entityParameter = Expression.Parameter(typeof(TEntity), "entity");
        var valueParameter = Expression.Parameter(typeof(TCollection), "collection");

        if (!navigation.IsShadowProperty())
        {
            var memberInfoForRead = navigation.GetMemberInfo(forMaterialization: false, forSet: false);
            navigation.TryGetMemberInfo(forMaterialization: false, forSet: true, out var memberInfoForWrite, out _);
            navigation.TryGetMemberInfo(forMaterialization: true, forSet: true, out var memberInfoForMaterialization, out _);
            var memberAccessForRead = (Expression)Expression.MakeMemberAccess(entityParameter, memberInfoForRead);
            if (memberAccessForRead.Type != typeof(TCollection))
            {
                memberAccessForRead = Expression.Convert(memberAccessForRead, typeof(TCollection));
            }

            getCollection = Expression.Lambda<Func<TEntity, TCollection>>(
                memberAccessForRead,
                entityParameter);

            if (memberInfoForWrite != null)
            {
                setCollection = CreateSetterDelegate(entityParameter, memberInfoForWrite, valueParameter);
            }

            if (memberInfoForMaterialization != null)
            {
                setCollectionForMaterialization = CreateSetterDelegate(entityParameter, memberInfoForMaterialization, valueParameter);
            }
        }

        var concreteType = CollectionTypeFactory.Instance.TryFindTypeToInstantiate(
            typeof(TEntity),
            typeof(TCollection),
            navigation.DeclaringEntityType.Model[CoreAnnotationNames.FullChangeTrackingNotificationsRequired] != null);
        if (concreteType != null)
        {
            var isHashSet = concreteType.IsGenericType && concreteType.GetGenericTypeDefinition() == typeof(HashSet<>);
            if (setCollection != null
                || setCollectionForMaterialization != null)
            {
                var setterParameter = Expression.Parameter(typeof(Action<TEntity, TCollection>), "setter");

                var createAndSetCollectionMethod = isHashSet
                    ? CreateAndSetHashSetMethod
                        .MakeGenericMethod(typeof(TEntity), typeof(TCollection), typeof(TElement))
                    : IsObservableHashSet(concreteType)
                        ? CreateAndSetObservableHashSetMethod
                            .MakeGenericMethod(typeof(TEntity), typeof(TCollection), typeof(TElement))
                        : CreateAndSetMethod
                            .MakeGenericMethod(typeof(TEntity), typeof(TCollection), concreteType);

                createAndSetCollection = Expression.Lambda<Func<TEntity, Action<TEntity, TCollection>, TCollection>>(
                    Expression.Call(createAndSetCollectionMethod, entityParameter, setterParameter),
                    entityParameter,
                    setterParameter);
            }

            createCollection = isHashSet
                ? (() => (TCollection)(ICollection<TElement>)new HashSet<TElement>(ReferenceEqualityComparer.Instance))
                : IsObservableHashSet(concreteType)
                    ? (() => (TCollection)(ICollection<TElement>)new ObservableHashSet<TElement>(ReferenceEqualityComparer.Instance))
                    : Expression.Lambda<Func<TCollection>>(Expression.New(concreteType));
        }

        static Expression<Action<TEntity, TCollection>> CreateSetterDelegate(
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
                valueParameter1);
    }

    private static MemberInfo? GetMostDerivedMemberInfo(INavigationBase navigation)
    {
        var propertyInfo = navigation.PropertyInfo;
        var fieldInfo = navigation.FieldInfo;

        return fieldInfo == null
            ? propertyInfo
            : propertyInfo == null
                ? fieldInfo
                : fieldInfo.FieldType.IsAssignableFrom(propertyInfo.PropertyType)
                    ? propertyInfo
                    : fieldInfo;
    }

    private static bool IsObservableHashSet(Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ObservableHashSet<>);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static TCollection CreateAndSet<TEntity, TCollection, TConcreteCollection>(
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static TCollection CreateAndSetHashSet<TEntity, TCollection, TElement>(
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static TCollection CreateAndSetObservableHashSet<TEntity, TCollection, TElement>(
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
}
