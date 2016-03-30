// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

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
                setterProperty = setterProperty.DeclaringType.GetProperty(property.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if(setterProperty.SetMethod != null)
                {
                    break;
                }
                else
                {
                    setterProperty = setterProperty.DeclaringType.GetTypeInfo().BaseType.GetProperty(property.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if(setterProperty == null)
                    {
                        throw new InvalidOperationException($"Could not find setter for property {property.Name}");
                    }
                }
            }

            return (IClrPropertySetter)Activator.CreateInstance(type, setterProperty.SetMethod.CreateDelegate(typeof(Action<,>).MakeGenericType(types)));
        }
    }
}