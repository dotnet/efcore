// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class ClrPropertySetterFactory : ClrAccessorFactory<IClrPropertySetter>
    {
        public override IClrPropertySetter Create(PropertyInfo property)
        {
            var types = new[] { property.DeclaringType, property.PropertyType };

            var type = property.PropertyType.IsNullableType() && property.PropertyType.UnwrapNullableType().GetTypeInfo().IsEnum
                ? typeof(NullableEnumClrPropertySetter<,,>).MakeGenericType(property.DeclaringType, property.PropertyType, property.PropertyType.UnwrapNullableType())
                : typeof(ClrPropertySetter<,>).MakeGenericType(types);

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

            return (IClrPropertySetter)Activator.CreateInstance(type, setterProperty.SetMethod.CreateDelegate(typeof(Action<,>).MakeGenericType(types)));
        }
    }
}
