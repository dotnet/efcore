// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.ExceptionServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class PropertyAccessorsFactory
{
    private PropertyAccessorsFactory()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly PropertyAccessorsFactory Instance = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertyAccessors Create(IPropertyBase propertyBase)
        => (PropertyAccessors)GenericCreate
            .MakeGenericMethod(propertyBase.ClrType)
            .Invoke(null, [propertyBase])!;

    private static readonly MethodInfo GenericCreate
        = typeof(PropertyAccessorsFactory).GetMethod(nameof(CreateGeneric), BindingFlags.Static | BindingFlags.NonPublic)!;

    [UsedImplicitly]
    private static PropertyAccessors CreateGeneric<TProperty>(IPropertyBase propertyBase)
    {
        CreateExpressions<TProperty>(
            propertyBase,
            out var currentValueGetter,
            out var preStoreGeneratedCurrentValueGetter,
            out var originalValueGetter,
            out var relationshipSnapshotGetter);

        return new PropertyAccessors(
            currentValueGetter.Compile(),
            preStoreGeneratedCurrentValueGetter.Compile(),
            originalValueGetter?.Compile(),
            relationshipSnapshotGetter.Compile());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Create(
        IPropertyBase propertyBase,
        out Expression currentValueGetter,
        out Expression preStoreGeneratedCurrentValueGetter,
        out Expression? originalValueGetter,
        out Expression relationshipSnapshotGetter)
    {
        var boundMethod = GenericCreateExpressions.MakeGenericMethod(propertyBase.ClrType);

        try
        {
            var parameters = new object?[] { propertyBase, null, null, null, null };
            boundMethod.Invoke(null, parameters);
            currentValueGetter = (Expression)parameters[1]!;
            preStoreGeneratedCurrentValueGetter = (Expression)parameters[2]!;
            originalValueGetter = (Expression?)parameters[3];
            relationshipSnapshotGetter = (Expression)parameters[4]!;
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

    private static readonly MethodInfo GenericCreateExpressions
        = typeof(PropertyAccessorsFactory).GetMethod(nameof(CreateExpressions), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static void CreateExpressions<TProperty>(
        IPropertyBase propertyBase,
        out Expression<Func<IInternalEntry, TProperty>> currentValueGetter,
        out Expression<Func<IInternalEntry, TProperty>> preStoreGeneratedCurrentValueGetter,
        out Expression<Func<IInternalEntry, TProperty>>? originalValueGetter,
        out Expression<Func<IInternalEntry, TProperty>> relationshipSnapshotGetter)
    {
        currentValueGetter = CreateCurrentValueGetter<TProperty>(propertyBase, useStoreGeneratedValues: true);
        preStoreGeneratedCurrentValueGetter = CreateCurrentValueGetter<TProperty>(propertyBase, useStoreGeneratedValues: false);
        originalValueGetter = propertyBase is not IProperty property ? null : CreateOriginalValueGetter<TProperty>(property);
        relationshipSnapshotGetter = CreateRelationshipSnapshotGetter<TProperty>(propertyBase);
    }

    private static Expression<Func<IInternalEntry, TProperty>> CreateCurrentValueGetter<TProperty>(
        IPropertyBase propertyBase,
        bool useStoreGeneratedValues)
    {
        var entityClrType = propertyBase.DeclaringType.GetPropertyAccessRoot().ClrType;
        var entryParameter = Expression.Parameter(typeof(IInternalEntry), "entry");
        var propertyIndex = propertyBase.GetIndex();
        var shadowIndex = propertyBase.GetShadowIndex();
        var storeGeneratedIndex = propertyBase.GetStoreGeneratedIndex();
        Expression currentValueExpression;
        Expression hasSentinelValueExpression;

        if (shadowIndex >= 0)
        {
            currentValueExpression = Expression.Call(
                entryParameter,
                InternalEntityEntry.MakeReadShadowValueMethod(typeof(TProperty)),
                Expression.Constant(shadowIndex));

            hasSentinelValueExpression = currentValueExpression.MakeHasSentinel(propertyBase);
        }
        else
        {
            var convertedExpression = Expression.Convert(
                Expression.Property(entryParameter, nameof(IInternalEntry.Object)),
                entityClrType);

            var memberInfo = propertyBase.GetMemberInfo(forMaterialization: false, forSet: false);

            currentValueExpression = CreateMemberAccess(
                propertyBase, convertedExpression, memberInfo, fromContainingType: false);
            hasSentinelValueExpression = currentValueExpression.MakeHasSentinel(propertyBase);

            if (currentValueExpression.Type != typeof(TProperty))
            {
                if (currentValueExpression.Type.IsNullableType()
                    && !typeof(TProperty).IsNullableType())
                {
                    var nullableValue = Expression.Variable(currentValueExpression.Type, "nullableValue");

                    currentValueExpression = Expression.Block(
                        [nullableValue],
                        new List<Expression>
                        {
                            Expression.Assign(
                                nullableValue,
                                currentValueExpression),
                            currentValueExpression.Type.IsValueType
                                ? Expression.Condition(
                                    Expression.MakeMemberAccess(
                                        nullableValue,
                                        nullableValue.Type.GetProperty("HasValue")!),
                                    Expression.Convert(nullableValue, typeof(TProperty)),
                                    Expression.Default(typeof(TProperty)))
                                : Expression.Condition(
                                    Expression.ReferenceEqual(nullableValue, Expression.Constant(null)),
                                    Expression.Default(typeof(TProperty)),
                                    Expression.Convert(nullableValue, typeof(TProperty)))
                        });
                }
                else
                {
                    currentValueExpression = Expression.Convert(currentValueExpression, typeof(TProperty));
                }
            }
        }

        if (useStoreGeneratedValues && storeGeneratedIndex >= 0)
        {
            currentValueExpression = Expression.Condition(
                Expression.Call(
                    entryParameter,
                    InternalEntityEntry.FlaggedAsStoreGeneratedMethod,
                    Expression.Constant(propertyIndex)),
                Expression.Call(
                    entryParameter,
                    InternalEntityEntry.MakeReadStoreGeneratedValueMethod(typeof(TProperty)),
                    Expression.Constant(storeGeneratedIndex)),
                Expression.Condition(
                    Expression.AndAlso(
                        Expression.Call(
                            entryParameter,
                            InternalEntityEntry.FlaggedAsTemporaryMethod,
                            Expression.Constant(propertyIndex)),
                        hasSentinelValueExpression),
                    Expression.Call(
                        entryParameter,
                        InternalEntityEntry.MakeReadTemporaryValueMethod(typeof(TProperty)),
                        Expression.Constant(storeGeneratedIndex)),
                    currentValueExpression));
        }

        return Expression.Lambda<Func<IInternalEntry, TProperty>>(
            currentValueExpression,
            entryParameter);
    }

    private static Expression<Func<IInternalEntry, TProperty>> CreateOriginalValueGetter<TProperty>(IProperty property)
    {
        var entryParameter = Expression.Parameter(typeof(IInternalEntry), "entry");
        var originalValuesIndex = property.GetOriginalValueIndex();

        return Expression.Lambda<Func<IInternalEntry, TProperty>>(
            originalValuesIndex >= 0
                ? Expression.Call(
                    entryParameter,
                    InternalEntityEntry.MakeReadOriginalValueMethod(typeof(TProperty)),
                    Expression.Constant(property),
                    Expression.Constant(originalValuesIndex))
                : Expression.Block(
                    Expression.Throw(
                        Expression.Constant(
                            new InvalidOperationException(
                                CoreStrings.OriginalValueNotTracked(property.Name, property.DeclaringType.DisplayName())))),
                    Expression.Constant(default(TProperty), typeof(TProperty))),
            entryParameter);
    }

    private static Expression<Func<IInternalEntry, TProperty>> CreateRelationshipSnapshotGetter<TProperty>(IPropertyBase propertyBase)
    {
        var entryParameter = Expression.Parameter(typeof(IInternalEntry), "entry");
        var relationshipIndex = (propertyBase as IProperty)?.GetRelationshipIndex() ?? -1;

        return Expression.Lambda<Func<IInternalEntry, TProperty>>(
            relationshipIndex >= 0
                ? Expression.Call(
                    entryParameter,
                    InternalEntityEntry.MakeReadRelationshipSnapshotValueMethod(typeof(TProperty)),
                    Expression.Constant(propertyBase),
                    Expression.Constant(relationshipIndex))
                : Expression.Call(
                    entryParameter,
                    InternalEntityEntry.MakeGetCurrentValueMethod(typeof(TProperty)),
                    Expression.Constant(propertyBase)),
            entryParameter);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly MethodInfo ContainsKeyMethod =
        typeof(IDictionary<string, object>).GetMethod(nameof(IDictionary<string, object>.ContainsKey), [typeof(string)])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Expression CreateMemberAccess(
        IPropertyBase? property,
        Expression instanceExpression,
        MemberInfo memberInfo,
        bool fromContainingType)
    {
        if (property?.IsIndexerProperty() == true)
        {
            Expression expression = Expression.MakeIndex(
                instanceExpression, (PropertyInfo)memberInfo, [Expression.Constant(property.Name)]);

            if (property.DeclaringType.IsPropertyBag)
            {
                expression = Expression.Condition(
                    Expression.Call(
                        instanceExpression, ContainsKeyMethod, [Expression.Constant(property.Name)]),
                    expression,
                    expression.Type.GetDefaultValueConstant());
            }

            return expression;
        }

        if (!fromContainingType
            && property?.DeclaringType is IComplexType complexType
            && !complexType.ComplexProperty.IsCollection)
        {
            instanceExpression = CreateMemberAccess(
                complexType.ComplexProperty,
                instanceExpression,
                complexType.ComplexProperty.GetMemberInfo(forMaterialization: false, forSet: false),
                fromContainingType);

            if (!instanceExpression.Type.IsValueType
                || instanceExpression.Type.IsNullableValueType())
            {
                return Expression.Condition(
                    Expression.Equal(instanceExpression, Expression.Constant(null)),
                    Expression.Default(memberInfo.GetMemberType()),
                    Expression.MakeMemberAccess(instanceExpression, memberInfo));
            }
        }

        return Expression.MakeMemberAccess(instanceExpression, memberInfo);
    }
}
