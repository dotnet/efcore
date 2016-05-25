// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class ClrPropertySetterFactory : ClrAccessorFactory<IClrPropertySetter>
    {
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
