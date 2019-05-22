// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ClrPropertySetterFactory : ClrAccessorFactory<IClrPropertySetter>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
                writeExpression = Expression.MakeMemberAccess(entityParameter, memberInfo)
                    .Assign(convertedParameter);
            }
            else
            {
                // This path handles properties that exist only on proxy types and so only exist if the instance is a proxy
                var converted = Expression.Variable(memberInfo.DeclaringType, "converted");

                writeExpression = Expression.Block(
                    new[]
                    {
                        converted
                    },
                    new List<Expression>
                    {
                        Expression.Assign(
                            converted,
                            Expression.TypeAs(entityParameter, memberInfo.DeclaringType)),
                        Expression.IfThen(
                            Expression.ReferenceNotEqual(converted, Expression.Constant(null)),
                            Expression.MakeMemberAccess(converted, memberInfo)
                                .Assign(convertedParameter))
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
