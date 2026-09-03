// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Runtime.ExceptionServices;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ClrIndexedCollectionAccessorFactory
{
    private static readonly MethodInfo GenericCreate
        = typeof(ClrIndexedCollectionAccessorFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateGeneric))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly ClrIndexedCollectionAccessorFactory Instance = new();

    private ClrIndexedCollectionAccessorFactory()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IClrIndexedCollectionAccessor? Create(IPropertyBase collection)
    {
        if (!collection.IsCollection
            || collection.IsShadowProperty())
        {
            return null;
        }

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (collection is IClrIndexedCollectionAccessor accessor)
        {
            return accessor;
        }

        var memberInfo = GetMostDerivedMemberInfo(collection);
        Check.DebugAssert(
            memberInfo == null || memberInfo.GetMemberType() == collection.ClrType,
            $"The member '{memberInfo?.Name}' is not of the expected type '{collection.ClrType.ShortDisplayName()}'.");

        var propertyType = collection.IsIndexerProperty() || collection.IsShadowProperty()
            ? collection.ClrType
            : memberInfo!.GetMemberType();
        var elementType = propertyType.TryGetElementType(typeof(IList<>));
        Check.DebugAssert(
            elementType != null,
            $"The type of navigation '{collection.DeclaringType.DisplayName()}.{collection.Name}' is '{propertyType.ShortDisplayName()}' which does not implement 'IList<>'. Collection properties must implement 'IList<>' of the target type.");

        var boundMethod = GenericCreate.MakeGenericMethod(
            memberInfo?.DeclaringType ?? collection.DeclaringType.ClrType, propertyType, elementType);

        try
        {
            return (IClrIndexedCollectionAccessor?)boundMethod.Invoke(null, [collection]);
        }
        catch (TargetInvocationException invocationException)
        {
            throw invocationException.InnerException!;
        }
    }

    [UsedImplicitly]
    private static IClrIndexedCollectionAccessor CreateGeneric<TStructural, TCollection, TElement>(IPropertyBase collection)
        where TCollection : class, IList<TElement>
    {
        CreateExpressions<TStructural, TCollection, TElement>(
            collection,
            out var get,
            out var set,
            out var setForMaterialization,
            out var createCollectionExpression);

        return new ClrIndexedCollectionAccessor<TStructural, TCollection, TElement>(
            collection.Name,
            collection.IsShadowProperty(),
            get?.Compile(),
            set?.Compile(),
            setForMaterialization?.Compile(),
            createCollectionExpression.Compile());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Create(
        IPropertyBase collection,
        out Type entityType,
        out Type propertyType,
        out Type elementType,
        out Expression? get,
        out Expression? set,
        out Expression? setForMaterialization,
        out Expression createCollection)
    {
        var memberInfo = GetMostDerivedMemberInfo(collection);
        entityType = memberInfo?.DeclaringType ?? collection.DeclaringType.ClrType;
        propertyType = collection.IsIndexerProperty() || collection.IsShadowProperty()
            ? collection.ClrType
            : memberInfo!.GetMemberType();

        elementType = propertyType.TryGetElementType(typeof(IEnumerable<>))!;

        var boundMethod = GenericCreateExpressions.MakeGenericMethod(entityType, propertyType, elementType);

        try
        {
            var parameters = new object?[] { collection, null, null, null, null };
            boundMethod.Invoke(this, parameters);
            get = (Expression?)parameters[1];
            set = (Expression?)parameters[2];
            setForMaterialization = (Expression?)parameters[3];
            createCollection = (Expression)parameters[4]!;
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

    private static readonly MethodInfo GenericCreateExpressions
        = typeof(ClrIndexedCollectionAccessorFactory).GetMethod(nameof(CreateExpressions), BindingFlags.Static | BindingFlags.NonPublic)!;

    [UsedImplicitly]
    private static void CreateExpressions<TStructural, TCollection, TElement>(
        IPropertyBase collection,
        out Expression<Func<TStructural, int, TElement>>? get,
        out Expression<Action<TStructural, int, TElement?>>? set,
        out Expression<Action<TStructural, int, TElement?>>? setForMaterialization,
        out Expression<Func<int, TCollection>> createCollection)
        where TCollection : class, IList<TElement>
    {
        get = null;
        set = null;
        setForMaterialization = null;

        var capacityParameter = Expression.Parameter(typeof(int), "capacity");
        var collectionType = typeof(TCollection);

        Expression createCollectionBody;
        if (collectionType.IsArray)
        {
            createCollectionBody = Expression.NewArrayBounds(typeof(TElement), capacityParameter);
        }
        else
        {
            var concreteType = collectionType;
            if (collectionType.IsInterface)
            {
                if (collectionType == typeof(IList)
                    || (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(IList<>)))
                {
                    concreteType = typeof(List<TElement>);
                }
            }

            var capacityConstructor = concreteType.GetConstructor([typeof(int)]);
            var defaultConstructor = concreteType.GetConstructor(Type.EmptyTypes);

            if (capacityConstructor != null)
            {
                createCollectionBody = Expression.New(capacityConstructor, capacityParameter);
            }
            else if (defaultConstructor != null)
            {
                createCollectionBody = Expression.New(defaultConstructor);
            }
            else
            {
                throw new InvalidOperationException(
                    $"The type '{collectionType.FullName}' does not have a constructor that takes an integer parameter or a parameterless constructor.");
            }
        }

        createCollection = Expression.Lambda<Func<int, TCollection>>(createCollectionBody, capacityParameter);

        var objectParameter = Expression.Parameter(typeof(TStructural), "structuralObject");
        var indexParameter = Expression.Parameter(typeof(int), "index");
        var valueParameter = Expression.Parameter(typeof(TElement?), "value");

        if (collection.IsShadowProperty())
        {
            return;
        }

        var readMemberInfo = collection.GetMemberInfo(forMaterialization: false, forSet: false);
        collection.TryGetMemberInfo(forMaterialization: true, forSet: false, out var memberInfoForMaterialization, out _);
        var memberAccessForRead = (Expression)Expression.MakeMemberAccess(objectParameter, readMemberInfo);
        if (memberAccessForRead.Type != typeof(TCollection))
        {
            memberAccessForRead = Expression.Convert(memberAccessForRead, typeof(TCollection));
        }

        var indexer = typeof(IList<TElement>).GetRuntimeProperties().Single(p => p.GetIndexParameters().Length > 0);

        var index = Expression.MakeIndex(memberAccessForRead, indexer, [indexParameter]);
        get = Expression.Lambda<Func<TStructural, int, TElement>>(index, objectParameter, indexParameter);

        set = Expression.Lambda<Action<TStructural, int, TElement?>>(
            Expression.Assign(index, valueParameter),
            objectParameter, indexParameter, valueParameter);

        if (memberInfoForMaterialization != null)
        {
            var memberAccessForMaterialization = (Expression)Expression.MakeMemberAccess(objectParameter, memberInfoForMaterialization);
            if (memberAccessForMaterialization.Type != typeof(TCollection))
            {
                memberAccessForMaterialization = Expression.Convert(memberAccessForMaterialization, typeof(TCollection));
            }

            index = Expression.MakeIndex(memberAccessForMaterialization, indexer, [indexParameter]);

            setForMaterialization = Expression.Lambda<Action<TStructural, int, TElement?>>(
                Expression.Assign(index, valueParameter),
                objectParameter, indexParameter, valueParameter);
        }
    }

    private static MemberInfo? GetMostDerivedMemberInfo(IPropertyBase collection)
    {
        var propertyInfo = collection.PropertyInfo;
        var fieldInfo = collection.FieldInfo;

        return fieldInfo == null
            ? propertyInfo
            : propertyInfo == null
                ? fieldInfo
                : fieldInfo.FieldType.IsAssignableFrom(propertyInfo.PropertyType)
                    ? propertyInfo
                    : fieldInfo;
    }
}
