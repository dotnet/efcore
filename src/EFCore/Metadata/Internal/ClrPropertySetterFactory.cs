// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;

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
        public override IClrPropertySetter Create(IPropertyBase property)
            => property as IClrPropertySetter ?? Create(property.GetMemberInfo(forMaterialization: false, forSet: true), property);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override IClrPropertySetter CreateGeneric<TEntity, TValue, TNonNullableEnumValue>(
            MemberInfo memberInfo,
            IPropertyBase propertyBase)
        {
            var entityParameter = Expression.Parameter(typeof(TEntity), "entity");
            var valueParameter = Expression.Parameter(typeof(TValue), "value");
            var memberType = memberInfo.GetMemberType();
            var convertedParameter = memberType == typeof(TValue)
                ? (Expression)valueParameter
                : Expression.Convert(valueParameter, memberType);

            Expression writeExpression;
            if (memberInfo.DeclaringType.IsAssignableFrom(typeof(TEntity)))
            {
                writeExpression = CreateMemberAssignment(entityParameter);
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
                            CreateMemberAssignment(converted))
                    });
            }

            var setter = Expression.Lambda<Action<TEntity, TValue>>(
                writeExpression,
                entityParameter,
                valueParameter).Compile();

            var propertyType = propertyBase?.ClrType ?? memberInfo.GetMemberType();

            return propertyType.IsNullableType()
                && propertyType.UnwrapNullableType().IsEnum
                    ? new NullableEnumClrPropertySetter<TEntity, TValue, TNonNullableEnumValue>(setter)
                    : (IClrPropertySetter)new ClrPropertySetter<TEntity, TValue>(setter);

            Expression CreateMemberAssignment(Expression parameter)
            {
                return propertyBase?.IsIndexerProperty() == true
                    ? Expression.Assign(
                        Expression.MakeIndex(
                            entityParameter, (PropertyInfo)memberInfo, new List<Expression> { Expression.Constant(propertyBase.Name) }),
                        convertedParameter)
                    : Expression.MakeMemberAccess(parameter, memberInfo).Assign(convertedParameter);
            }
        }
    }
}
