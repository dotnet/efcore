// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ClrPropertyGetterFactory : ClrAccessorFactory<IClrPropertyGetter>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override IClrPropertyGetter CreateGeneric<TEntity, TValue, TNonNullableEnumValue>(
            PropertyInfo propertyInfo, IPropertyBase propertyBase)
        {
            var memberInfo = propertyBase?.GetMemberInfo(forConstruction: false, forSet: false)
                             ?? propertyInfo.FindGetterProperty();

            if (memberInfo == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NoGetter(propertyInfo.Name, propertyInfo.DeclaringType.ShortDisplayName(), nameof(PropertyAccessMode)));
            }

            var entityParameter = Expression.Parameter(typeof(TEntity), "entity");

            Expression readExpression;
            if (memberInfo.DeclaringType.GetTypeInfo().IsAssignableFrom(typeof(TEntity).GetTypeInfo()))
            {
                readExpression = Expression.MakeMemberAccess(entityParameter, memberInfo);
            }
            else
            {
                // This path handles properties that exist only on proxy types and so only exist if the instance is a proxy
                var converted = Expression.Variable(memberInfo.DeclaringType, "converted");

                readExpression = Expression.Block(
                    new[] { converted },
                    new List<Expression>
                    {
                        Expression.Assign(
                            converted,
                            Expression.TypeAs(entityParameter, memberInfo.DeclaringType)),
                        Expression.Condition(
                            Expression.ReferenceEqual(converted, Expression.Constant(null)),
                            Expression.Default(memberInfo.GetMemberType()),
                            Expression.MakeMemberAccess(converted, memberInfo))
                    });
            }

            if (readExpression.Type != typeof(TValue))
            {
                readExpression = Expression.Convert(readExpression, typeof(TValue));
            }

            Expression hasDefaultValueExpression;

            if (!readExpression.Type.IsValueType)
            {
                hasDefaultValueExpression
                    = Expression.ReferenceEqual(
                        readExpression,
                        Expression.Constant(null, readExpression.Type));
            }
            else if (readExpression.Type.IsGenericType
                     && readExpression.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                hasDefaultValueExpression
                    = Expression.Not(
                        Expression.Call(
                            readExpression,
                            readExpression.Type.GetMethod("get_HasValue")));
            }
            else
            {
                var property = propertyBase as IProperty;
                var comparer = property?.GetValueComparer()
                               ?? property?.FindMapping()?.Comparer
                               ?? (ValueComparer)Activator.CreateInstance(
                                   typeof(ValueComparer<>).MakeGenericType(typeof(TValue)),
                                   new object[] { false });

                hasDefaultValueExpression = comparer.ExtractEqualsBody(
                    comparer.Type != typeof(TValue)
                        ? Expression.Convert(readExpression, comparer.Type)
                        : readExpression,
                    Expression.Default(comparer.Type));
            }

            return new ClrPropertyGetter<TEntity, TValue>(
                Expression.Lambda<Func<TEntity, TValue>>(readExpression, entityParameter).Compile(),
                Expression.Lambda<Func<TEntity, bool>>(hasDefaultValueExpression, entityParameter).Compile());
        }
    }
}
