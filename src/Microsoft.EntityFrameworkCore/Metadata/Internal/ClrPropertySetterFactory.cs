// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
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
        protected override IClrPropertySetter CreateGeneric<TEntity, TValue, TNonNullableEnumValue>([NotNull] PropertyInfo property)
        {
            var setterProperty = property;
            while (setterProperty.SetMethod == null)
            {
                var declaringType = setterProperty.DeclaringType;
                Debug.Assert(declaringType != null);
                setterProperty = declaringType.GetProperty(property.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (setterProperty.SetMethod != null)
                {
                    break;
                }
                setterProperty = declaringType.GetTypeInfo().BaseType?.GetProperty(property.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (setterProperty == null)
                {
                    throw new InvalidOperationException(CoreStrings.NoClrSetter(property.Name));
                }
            }

            var setter = (Action<TEntity, TValue>)setterProperty.SetMethod.CreateDelegate(typeof(Action<TEntity, TValue>));

            return property.PropertyType.IsNullableType() && property.PropertyType.UnwrapNullableType().GetTypeInfo().IsEnum
                ? new NullableEnumClrPropertySetter<TEntity, TValue, TNonNullableEnumValue>(setter)
                : (IClrPropertySetter)new ClrPropertySetter<TEntity, TValue>(setter);
        }
    }
}
