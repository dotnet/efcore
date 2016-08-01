// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
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
            PropertyInfo property, IPropertyBase propertyBase)
        {
            var setterProperty = property.DeclaringType
                .GetPropertiesInHierarchy(property.Name)
                .FirstOrDefault(p => p.SetMethod != null);

            Action<TEntity, TValue> setter = null;

            if (setterProperty != null)
            {
                setter = (Action<TEntity, TValue>)setterProperty.SetMethod.CreateDelegate(typeof(Action<TEntity, TValue>));
            }
            else
            {
                var fieldInfo = propertyBase?.DeclaringEntityType.Model.GetMemberMapper().FindBackingField(propertyBase);
                if (fieldInfo != null)
                {
                    var entityParameter = Expression.Parameter(typeof(TEntity), "entity");
                    var valueParameter = Expression.Parameter(typeof(TValue), "value");

                    setter = Expression.Lambda<Action<TEntity, TValue>>(
                        Expression.Assign(
                            Expression.Field(entityParameter, fieldInfo),
                            valueParameter),
                        entityParameter,
                        valueParameter).Compile();
                }
            }

            if (setter == null)
            {
                throw new InvalidOperationException(CoreStrings.NoClrSetter(property.Name));
            }

            return property.PropertyType.IsNullableType() && property.PropertyType.UnwrapNullableType().GetTypeInfo().IsEnum
                ? new NullableEnumClrPropertySetter<TEntity, TValue, TNonNullableEnumValue>(setter)
                : (IClrPropertySetter)new ClrPropertySetter<TEntity, TValue>(setter);
        }
    }
}
