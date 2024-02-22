// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore.Internal;
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
    public virtual Func<ISnapshot> CreateEmpty(IRuntimeEntityType entityType)
        => CreateEmptyExpression(entityType).Compile();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression<Func<ISnapshot>> CreateEmptyExpression(IRuntimeEntityType entityType)
        => Expression.Lambda<Func<ISnapshot>>(CreateConstructorExpression(entityType, null));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression CreateConstructorExpression(
        IRuntimeEntityType entityType,
        Expression? parameter)
    {
        var count = GetPropertyCount(entityType);
        if (count == 0)
        {
            return Expression.MakeMemberAccess(null, Snapshot.EmptyField);
        }

        var types = new Type[count];
        var propertyBases = new IPropertyBase?[count];

        foreach (var propertyBase in entityType.GetSnapshottableMembers())
        {
            var index = GetPropertyIndex(propertyBase);
            if (index >= 0)
            {
                types[index] = (propertyBase as IProperty)?.ClrType ?? typeof(object);
                propertyBases[index] = propertyBase;
            }
        }

        Expression constructorExpression;
        if (count > Snapshot.MaxGenericTypes)
        {
            var snapshotExpressions = new List<Expression>();

            for (var i = 0; i < count; i += Snapshot.MaxGenericTypes)
            {
                snapshotExpressions.Add(
                    CreateSnapshotExpression(
                        entityType.ClrType,
                        parameter,
                        types.Skip(i).Take(Snapshot.MaxGenericTypes).ToArray(),
                        propertyBases.Skip(i).Take(Snapshot.MaxGenericTypes).ToList()));
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
            constructorExpression = CreateSnapshotExpression(entityType.ClrType, parameter, types, propertyBases);
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
        Type? entityType,
        Expression? parameter,
        Type[] types,
        IList<IPropertyBase?> propertyBases)
    {
        var count = types.Length;

        var arguments = new Expression[count];

        var entityVariable = entityType == null
            ? null
            : Expression.Variable(entityType, "entity");

        for (var i = 0; i < count; i++)
        {
            var propertyBase = propertyBases[i];
            if (propertyBase == null)
            {
                arguments[i] = Expression.Constant(null);
                types[i] = typeof(object);
                continue;
            }

            if (propertyBase is IProperty property)
            {
                arguments[i] = CreateSnapshotValueExpression(CreateReadValueExpression(parameter, property), property);
                continue;
            }

            if (propertyBase is IComplexProperty complexProperty)
            {
                arguments[i] = CreateSnapshotValueExpression(CreateReadValueExpression(parameter, complexProperty), complexProperty);
                continue;
            }

            if (propertyBase.IsShadowProperty())
            {
                arguments[i] = CreateSnapshotValueExpression(CreateReadShadowValueExpression(parameter, propertyBase), propertyBase);
                continue;
            }

            var memberInfo = propertyBase.GetMemberInfo(forMaterialization: false, forSet: false);
            var memberAccess = PropertyAccessorsFactory.CreateMemberAccess(propertyBase, entityVariable!, memberInfo, fromContainingType: false);

            if (memberAccess.Type != propertyBase.ClrType)
            {
                var hasDefaultValueExpression = memberAccess.MakeHasSentinel(propertyBase);

                memberAccess = Expression.Condition(
                    hasDefaultValueExpression,
                    propertyBase.ClrType.GetDefaultValueConstant(),
                    Expression.Convert(memberAccess, propertyBase.ClrType));
            }

            arguments[i] = (propertyBase as INavigation)?.IsCollection ?? false
                ? Expression.Call(
                    null,
                    SnapshotCollectionMethod,
                    memberAccess)
                : CreateSnapshotValueExpression(memberAccess, propertyBase);
        }

        var constructorExpression = Expression.Convert(
            Expression.New(
                Snapshot.CreateSnapshotType(types).GetDeclaredConstructor(types)!,
                arguments),
            typeof(ISnapshot));

        Check.DebugAssert(
            !UseEntityVariable || entityVariable == null || parameter != null,
            "Parameter can only be null when not using entity variable.");

        return UseEntityVariable
            && entityVariable != null
                ? Expression.Block(
                    new List<ParameterExpression> { entityVariable },
                    new List<Expression>
                    {
                        Expression.Assign(
                            entityVariable,
                            Expression.Convert(
                                Expression.Property(parameter!, nameof(InternalEntityEntry.Entity)),
                                entityType!)),
                        constructorExpression
                    })
                : constructorExpression;
    }

    private Expression CreateSnapshotValueExpression(Expression expression, IPropertyBase propertyBase)
    {
        if (propertyBase is not IProperty property
            || GetValueComparer(property) is not ValueComparer comparer)
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
            InternalEntityEntry.MakeReadShadowValueMethod((property as IProperty)?.ClrType ?? typeof(object)),
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
            InternalEntityEntry.MakeGetCurrentValueMethod(property.ClrType),
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
    protected abstract int GetPropertyCount(IRuntimeEntityType entityType);

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
    public static HashSet<object>? SnapshotCollection(IEnumerable<object>? collection)
        => collection == null
            ? null
            : new HashSet<object>(collection, ReferenceEqualityComparer.Instance);
}
