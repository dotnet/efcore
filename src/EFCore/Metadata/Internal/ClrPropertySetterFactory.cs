// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ClrPropertySetterFactory : ClrAccessorFactory<IClrPropertySetter>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override IClrPropertySetter CreateGeneric<TEntity, TValue, TNonNullableEnumValue>(
            PropertyInfo propertyInfo, IPropertyBase propertyBase)
        {
            var memberInfo = propertyBase?.GetMemberInfo(forConstruction: false, forSet: true)
                             ?? propertyInfo.FindGetterProperty();

            if (memberInfo == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NoSetter(propertyInfo.Name, propertyInfo.DeclaringType.ShortDisplayName(), nameof(PropertyAccessMode)));
            }

            var entityParameter = Expression.Parameter(typeof(TEntity), "entity");
            var valueParameter = Expression.Parameter(typeof(TValue), "value");
            var memberType = memberInfo.GetMemberType();
            var convertedParameter = memberType == typeof(TValue)
                ? (Expression)valueParameter
                : Expression.Convert(valueParameter, memberType);

            Expression writeExpression;
            if (memberInfo.DeclaringType.GetTypeInfo().IsAssignableFrom(typeof(TEntity).GetTypeInfo()))
            {
                writeExpression = Expression.Assign(
                    Expression.MakeMemberAccess(entityParameter, memberInfo),
                    convertedParameter);
            }
            else
            {
                // This path handles properties that exist only on proxy types and so only exist if the instance is a proxy
                var converted = Expression.Variable(memberInfo.DeclaringType, "converted");

                writeExpression = Expression.Block(
                    new[] { converted },
                    new List<Expression>
                    {
                        Expression.Assign(
                            converted,
                            Expression.TypeAs(entityParameter, memberInfo.DeclaringType)),
                        Expression.IfThen(
                            Expression.ReferenceNotEqual(converted, Expression.Constant(null)),
                            Expression.Assign(
                                Expression.MakeMemberAccess(converted, memberInfo),
                                convertedParameter))
                    });
            }

            var setter = Expression.Lambda<Action<TEntity, TValue>>(
                writeExpression,
                entityParameter,
                valueParameter).Compile();

            var propertyType = propertyBase?.ClrType ?? propertyInfo?.PropertyType;

            return propertyType.IsNullableType()
                   && propertyType.UnwrapNullableType().GetTypeInfo().IsEnum
                ? new NullableEnumClrPropertySetter<TEntity, TValue, TNonNullableEnumValue>(setter)
                : (IClrPropertySetter)new ClrPropertySetter<TEntity, TValue>(setter);
        }
    }
}
