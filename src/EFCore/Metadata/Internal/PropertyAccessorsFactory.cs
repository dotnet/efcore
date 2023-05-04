// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertyAccessors Create(IPropertyBase propertyBase)
        => (PropertyAccessors)GenericCreate
            .MakeGenericMethod(propertyBase.ClrType)
            .Invoke(null, new object[] { propertyBase })!;

    private static readonly MethodInfo GenericCreate
        = typeof(PropertyAccessorsFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateGeneric))!;

    [UsedImplicitly]
    private static PropertyAccessors CreateGeneric<TProperty>(IPropertyBase propertyBase)
    {
        var property = propertyBase as IProperty;

        return new PropertyAccessors(
            CreateCurrentValueGetter<TProperty>(propertyBase, useStoreGeneratedValues: true),
            CreateCurrentValueGetter<TProperty>(propertyBase, useStoreGeneratedValues: false),
            property == null ? null : CreateOriginalValueGetter<TProperty>(property),
            CreateRelationshipSnapshotGetter<TProperty>(propertyBase),
            property == null ? null : CreateValueBufferGetter(property));
    }

    private static Func<InternalEntityEntry, TProperty> CreateCurrentValueGetter<TProperty>(
        IPropertyBase propertyBase,
        bool useStoreGeneratedValues)
    {
        var entityClrType = propertyBase.DeclaringType.ClrType;
        var entryParameter = Expression.Parameter(typeof(InternalEntityEntry), "entry");
        var property = propertyBase as IProperty;
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

            hasSentinelValueExpression = currentValueExpression.MakeHasSentinelValue(propertyBase);
        }
        else
        {
            var convertedExpression = Expression.Convert(
                Expression.Property(entryParameter, "Entity"),
                entityClrType);

            var memberInfo = propertyBase.GetMemberInfo(forMaterialization: false, forSet: false);

            currentValueExpression = PropertyBase.CreateMemberAccess(propertyBase, convertedExpression, memberInfo);
            hasSentinelValueExpression = currentValueExpression.MakeHasSentinelValue(propertyBase);

            if (currentValueExpression.Type != typeof(TProperty))
            {
                if (currentValueExpression.Type.IsNullableType()
                    && !typeof(TProperty).IsNullableType())
                {
                    var nullableValue = Expression.Variable(currentValueExpression.Type, "nullableValue");

                    currentValueExpression = Expression.Block(
                        new[] { nullableValue },
                        new List<Expression>
                        {
                            Expression.Assign(
                                nullableValue,
                                currentValueExpression),
                            currentValueExpression.Type.IsValueType
                                ? Expression.Condition(
                                    Expression.Call(
                                        nullableValue,
                                        nullableValue.Type.GetMethod("get_HasValue")!),
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
                    typeof(InternalEntityEntry).GetMethod(nameof(InternalEntityEntry.FlaggedAsStoreGenerated))!,
                    Expression.Constant(propertyIndex)),
                Expression.Call(
                    entryParameter,
                    InternalEntityEntry.MakeReadStoreGeneratedValueMethod(typeof(TProperty)),
                    Expression.Constant(storeGeneratedIndex)),
                Expression.Condition(
                    Expression.AndAlso(
                        Expression.Call(
                            entryParameter,
                            typeof(InternalEntityEntry).GetMethod(nameof(InternalEntityEntry.FlaggedAsTemporary))!,
                            Expression.Constant(propertyIndex)),
                        hasSentinelValueExpression),
                    Expression.Call(
                        entryParameter,
                        InternalEntityEntry.MakeReadTemporaryValueMethod(typeof(TProperty)),
                        Expression.Constant(storeGeneratedIndex)),
                    currentValueExpression));
        }

        return Expression.Lambda<Func<InternalEntityEntry, TProperty>>(
                currentValueExpression,
                entryParameter)
            .Compile();
    }

    private static Func<InternalEntityEntry, TProperty> CreateOriginalValueGetter<TProperty>(IProperty property)
    {
        var entryParameter = Expression.Parameter(typeof(InternalEntityEntry), "entry");
        var originalValuesIndex = property.GetOriginalValueIndex();

        return Expression.Lambda<Func<InternalEntityEntry, TProperty>>(
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
                                    CoreStrings.OriginalValueNotTracked(property.Name, property.DeclaringEntityType.DisplayName())))),
                        Expression.Constant(default(TProperty), typeof(TProperty))),
                entryParameter)
            .Compile();
    }

    private static Func<InternalEntityEntry, TProperty> CreateRelationshipSnapshotGetter<TProperty>(IPropertyBase propertyBase)
    {
        var entryParameter = Expression.Parameter(typeof(InternalEntityEntry), "entry");
        var relationshipIndex = (propertyBase as IProperty)?.GetRelationshipIndex() ?? -1;

        return Expression.Lambda<Func<InternalEntityEntry, TProperty>>(
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
                entryParameter)
            .Compile();
    }

    private static Func<ValueBuffer, object> CreateValueBufferGetter(IProperty property)
    {
        var valueBufferParameter = Expression.Parameter(typeof(ValueBuffer), "valueBuffer");

        return Expression.Lambda<Func<ValueBuffer, object>>(
                Expression.Call(
                    valueBufferParameter,
                    ValueBuffer.GetValueMethod,
                    Expression.Constant(property.GetIndex())),
                valueBufferParameter)
            .Compile();
    }
}
