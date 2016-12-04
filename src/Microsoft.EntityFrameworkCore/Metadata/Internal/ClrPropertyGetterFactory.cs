// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
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

            return new ClrPropertyGetter<TEntity, TValue>(Expression.Lambda<Func<TEntity, TValue>>(
                Expression.MakeMemberAccess(entityParameter, memberInfo),
                entityParameter).Compile());
        }
    }
}
