// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public abstract class SnapshotFactoryFactory
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<ISnapshot> CreateEmpty(IRuntimeTypeBase structuralType)
        => CreateEmptyExpression(structuralType).Compile();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression<Func<ISnapshot>> CreateEmptyExpression(IRuntimeTypeBase structuralType)
        => Expression.Lambda<Func<ISnapshot>>(CreateConstructorExpression(structuralType, null));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression CreateConstructorExpression(
        IRuntimeTypeBase structuralType,
        Expression? parameter)
    {
        var count = GetPropertyCount(structuralType);
        if (count == 0)
        {
            return Expression.MakeMemberAccess(null, Snapshot.EmptyField);
        }

        var types = new Type[count];
        var propertyBases = new IPropertyBase?[count];

        var actualCount = 0;
        foreach (var propertyBase in structuralType.GetSnapshottableMembers())
        {
            var index = GetPropertyIndex(propertyBase);
            if (index >= 0)
            {
                Check.DebugAssert(propertyBases[index] == null, $"Both {propertyBase.Name} and {propertyBases[index]?.Name} have the same index {index}.");

                types[index] = (propertyBase as IProperty)?.ClrType ?? typeof(object);
                propertyBases[index] = propertyBase;
                actualCount++;
            }
        }

        Check.DebugAssert(actualCount == count,
            $"Count of snapshottable properties {actualCount} for {structuralType.DisplayName()} does not match expected count {count}.");

        Expression constructorExpression;
        if (count > Snapshot.MaxGenericTypes)
        {
            var snapshotExpressions = new List<Expression>();

            for (var i = 0; i < count; i += Snapshot.MaxGenericTypes)
            {
                snapshotExpressions.Add(
                    CreateSnapshotExpression(
                        structuralType.ClrType,
                        parameter,
                        [.. types.Skip(i).Take(Snapshot.MaxGenericTypes)],
                        [.. propertyBases.Skip(i).Take(Snapshot.MaxGenericTypes)]));
            }

            constructorExpression =
                Expression.Convert(
                    Expression.New(
                        MultiSnapshot.Constructor,
                        Expression.NewArrayInit(typeof(ISnapshot), snapshotExpressions)),
                    typeof(ISnapshot));
        }
        else
        {
            constructorExpression = CreateSnapshotExpression(structuralType.ClrType, parameter, types, propertyBases);
        }

        return constructorExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression CreateSnapshotExpression(
        Type? clrType,
        Expression? parameter,
        Type[] types,
        IList<IPropertyBase?> propertyBases)
    {
        var count = types.Length;
        var arguments = new Expression[count];

        var structuralTypeVariable = clrType == null
            ? null
            : Expression.Variable(clrType, "structuralType");

        Check.DebugAssert(structuralTypeVariable != null || count == 0,
            "If there are any properties then the entity parameter must be used");
        var indicesExpression = parameter == null || !parameter.Type.IsAssignableTo(typeof(IInternalEntry))
            ? (Expression)Expression.Property(null, typeof(ReadOnlySpan<int>), nameof(ReadOnlySpan<>.Empty))
            : Expression.Call(parameter, PropertyAccessorsFactory.GetOrdinalsMethod);

        for (var i = 0; i < count; i++)
        {
            var propertyBase = propertyBases[i];

            switch (propertyBase)
            {
                case null:
                    arguments[i] = Expression.Constant(null);
                    types[i] = typeof(object);
                    continue;

                case IProperty property:
                    arguments[i] = CreateSnapshotValueExpression(CreateReadValueExpression(parameter, property), property);
                    continue;

                case var _ when propertyBase.IsShadowProperty():
                    arguments[i] = CreateSnapshotValueExpression(CreateReadShadowValueExpression(parameter, propertyBase), propertyBase);
                    continue;
            }

            arguments[i] = CreateSnapshotValueExpression(CreateReadValueExpression(parameter, propertyBase), propertyBase);
        }

        var constructorExpression = Expression.Convert(
            Expression.New(
                Snapshot.CreateSnapshotType(types).GetDeclaredConstructor(types)!,
                arguments),
            typeof(ISnapshot));

        Check.DebugAssert(
            !UseEntityVariable || structuralTypeVariable == null || parameter != null,
            "Parameter can only be null when not using entity variable.");

        return UseEntityVariable
            && structuralTypeVariable != null
                ? Expression.Block(
                    new List<ParameterExpression> { structuralTypeVariable },
                    new List<Expression>
                    {
                        Expression.Assign(
                            structuralTypeVariable,
                            propertyBases[0]!.DeclaringType is IComplexType declaringComplexType && declaringComplexType.ComplexProperty.IsCollection
                            ? PropertyAccessorsFactory.CreateComplexCollectionElementAccess(
                                declaringComplexType.ComplexProperty,
                                Expression.Convert(
                                    Expression.Property(parameter!, nameof(IInternalEntry.Entity)),
                                    declaringComplexType.ComplexProperty.DeclaringType.ContainingEntityType.ClrType),
                                indicesExpression,
                                fromDeclaringType: false,
                                fromEntity: true)
                            : propertyBases[0]!.DeclaringType.ContainingType is IComplexType collectionComplexType
                                ? PropertyAccessorsFactory.CreateComplexCollectionElementAccess(
                                    collectionComplexType.ComplexProperty,
                                    Expression.Convert(
                                        Expression.Property(parameter!, nameof(IInternalEntry.Entity)),
                                        collectionComplexType.ComplexProperty.DeclaringType.ContainingEntityType.ClrType),
                                    indicesExpression,
                                    fromDeclaringType: false,
                                    fromEntity: true)
                                : Expression.Convert(
                                    Expression.Property(parameter!, nameof(IInternalEntry.Entity)),
                                    structuralTypeVariable.Type)),
                        constructorExpression
                    })
                : constructorExpression;
    }

    private Expression CreateSnapshotValueExpression(Expression expression, IPropertyBase propertyBase)
    {
        if (propertyBase is not IProperty property)
        {
            if (propertyBase.IsCollection)
            {
                if (expression.Type != typeof(IEnumerable))
                {
                    expression = Expression.Convert(expression, typeof(IEnumerable));
                }

                expression = propertyBase is IComplexProperty complexProperty
                    ? Expression.Call(
                        null,
                        SnapshotComplexCollectionMethod,
                        expression)
                    : Expression.Call(
                        null,
                        SnapshotCollectionMethod,
                        expression);
            }
            return expression;
        }

        if (GetValueComparer(property) is not ValueComparer comparer)
        {
            return expression;
        }

        if (expression.Type != comparer.Type)
        {
            expression = Expression.Convert(expression, comparer.Type);
        }

        var comparerExpression = Expression.Convert(
            Expression.Call(
                Expression.Constant(property),
                GetValueComparerMethod()!),
            typeof(ValueComparer<>).MakeGenericType(comparer.Type));

        Expression snapshotExpression = Expression.Call(
            comparerExpression,
            ValueComparer.GetGenericSnapshotMethod(comparer.Type),
            expression);

        if (snapshotExpression.Type != propertyBase.ClrType)
        {
            snapshotExpression = Expression.Convert(snapshotExpression, propertyBase.ClrType);
        }

        expression = propertyBase.ClrType.IsNullableType()
            ? Expression.Condition(
                Expression.Equal(expression, Expression.Constant(null, propertyBase.ClrType)),
                Expression.Constant(null, propertyBase.ClrType),
                snapshotExpression)
            : snapshotExpression;

        return expression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected abstract ValueComparer? GetValueComparer(IProperty property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected abstract MethodInfo? GetValueComparerMethod();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression CreateReadShadowValueExpression(
        Expression? parameter,
        IPropertyBase property)
        => Expression.Call(
            parameter,
            InternalEntryBase.MakeReadShadowValueMethod((property as IProperty)?.ClrType ?? typeof(object)),
            Expression.Constant(property.GetShadowIndex()));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression CreateReadValueExpression(
        Expression? parameter,
        IPropertyBase property)
        => Expression.Call(
            parameter,
            InternalEntryBase.MakeGetCurrentValueMethod(property.ClrType),
            Expression.Constant(property, typeof(IPropertyBase)));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected abstract int GetPropertyIndex(IPropertyBase propertyBase);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected abstract int GetPropertyCount(IRuntimeTypeBase structuralType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual bool UseEntityVariable
        => true;

    private static readonly MethodInfo SnapshotCollectionMethod
        = typeof(SnapshotFactoryFactory).GetTypeInfo().GetDeclaredMethod(nameof(SnapshotCollection))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static HashSet<object>? SnapshotCollection(IEnumerable? collection)
    {
        if (collection is null)
        {
            return null;
        }
        var snapshot = new HashSet<object>(ReferenceEqualityComparer.Instance);
        foreach (var item in collection)
        {
            snapshot.Add(item);
        }
        return snapshot;
    }

    private static readonly MethodInfo SnapshotComplexCollectionMethod
        = typeof(SnapshotFactoryFactory).GetTypeInfo().GetDeclaredMethod(nameof(SnapshotComplexCollection))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static List<object>? SnapshotComplexCollection(IEnumerable? collection)
    {
        if (collection == null)
        {
            return null;
        }

        var snapshot = new List<object>();
        foreach (var item in collection)
        {
            snapshot.Add(item);
        }
        return snapshot;
    }
}
