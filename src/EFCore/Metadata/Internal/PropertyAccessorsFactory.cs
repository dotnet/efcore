// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
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
        public virtual PropertyAccessors Create([NotNull] IPropertyBase propertyBase)
            => (PropertyAccessors)_genericCreate
                .MakeGenericMethod(propertyBase.ClrType)
                .Invoke(
                    null, new object[] { propertyBase });

        private static readonly MethodInfo _genericCreate
            = typeof(PropertyAccessorsFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateGeneric));

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
            IPropertyBase propertyBase, bool useStoreGeneratedValues)
        {
            var entityClrType = propertyBase.DeclaringType.ClrType;
            var entryParameter = Expression.Parameter(typeof(InternalEntityEntry), "entry");

            var shadowIndex = propertyBase.GetShadowIndex();
            Expression currentValueExpression;
            if (shadowIndex >= 0)
            {
                currentValueExpression = Expression.Call(
                    entryParameter,
                    InternalEntityEntry.ReadShadowValueMethod.MakeGenericMethod(typeof(TProperty)),
                    Expression.Constant(shadowIndex));
            }
            else
            {
                var convertedExpression = Expression.Convert(
                    Expression.Property(entryParameter, "Entity"),
                    entityClrType);

                currentValueExpression = CreateMemberAccess(
                    convertedExpression,
                    propertyBase.GetMemberInfo(forMaterialization: false, forSet: false));

                if (currentValueExpression.Type != typeof(TProperty))
                {
                    currentValueExpression = Expression.Convert(currentValueExpression, typeof(TProperty));
                }
            }

            var storeGeneratedIndex = propertyBase.GetStoreGeneratedIndex();
            if (storeGeneratedIndex >= 0)
            {
                if (useStoreGeneratedValues)
                {
                    currentValueExpression = Expression.Condition(
                        Expression.Equal(
                            currentValueExpression,
                            Expression.Constant(default(TProperty), typeof(TProperty))),
                        Expression.Call(
                            entryParameter,
                            InternalEntityEntry.ReadStoreGeneratedValueMethod.MakeGenericMethod(typeof(TProperty)),
                            Expression.Constant(storeGeneratedIndex)),
                        currentValueExpression);
                }

                currentValueExpression = Expression.Condition(
                    Expression.Equal(
                        currentValueExpression,
                        Expression.Constant(default(TProperty), typeof(TProperty))),
                    Expression.Call(
                        entryParameter,
                        InternalEntityEntry.ReadTemporaryValueMethod.MakeGenericMethod(typeof(TProperty)),
                        Expression.Constant(storeGeneratedIndex)),
                    currentValueExpression);
            }

            return Expression.Lambda<Func<InternalEntityEntry, TProperty>>(
                    currentValueExpression,
                    entryParameter)
                .Compile();

            Expression CreateMemberAccess(Expression parameter, MemberInfo memberInfo)
            {
                return propertyBase?.IsIndexerProperty() == true
                    ? Expression.MakeIndex(
                        parameter, (PropertyInfo)memberInfo, new List<Expression>() { Expression.Constant(propertyBase.Name) })
                    : (Expression)Expression.MakeMemberAccess(parameter, memberInfo);
            }
        }

        private static Func<InternalEntityEntry, TProperty> CreateOriginalValueGetter<TProperty>(IProperty property)
        {
            var entryParameter = Expression.Parameter(typeof(InternalEntityEntry), "entry");
            var originalValuesIndex = property.GetOriginalValueIndex();

            return Expression.Lambda<Func<InternalEntityEntry, TProperty>>(
                    originalValuesIndex >= 0
                        ? (Expression)Expression.Call(
                            entryParameter,
                            InternalEntityEntry.ReadOriginalValueMethod.MakeGenericMethod(typeof(TProperty)),
                            Expression.Constant(property),
                            Expression.Constant(originalValuesIndex))
                        : Expression.Block(
                            Expression.Throw(
                                Expression.Constant(
                                    new InvalidOperationException(
                                        CoreStrings.OriginalValueNotTracked(property.Name, property.DeclaringEntityType.DisplayName())))),
#pragma warning disable IDE0034 // Simplify 'default' expression - default infer to default(object) instead of default(TProperty)
                            Expression.Constant(default(TProperty), typeof(TProperty))),
#pragma warning restore IDE0034 // Simplify 'default' expression
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
                            InternalEntityEntry.ReadRelationshipSnapshotValueMethod.MakeGenericMethod(typeof(TProperty)),
                            Expression.Constant(propertyBase),
                            Expression.Constant(relationshipIndex))
                        : Expression.Call(
                            entryParameter,
                            InternalEntityEntry.GetCurrentValueMethod.MakeGenericMethod(typeof(TProperty)),
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
}
