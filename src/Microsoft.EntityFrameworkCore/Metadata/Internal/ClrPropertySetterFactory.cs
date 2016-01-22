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
            // TODO: Handle case where there is not setter or setter is private on a base type
            // Issue #753

            var types = new[] { property.DeclaringType, property.PropertyType };

            return (IClrPropertySetter)Activator.CreateInstance(
                property.PropertyType.IsNullableType()
                && property.PropertyType.UnwrapNullableType().GetTypeInfo().IsEnum
                    ? typeof(NullableEnumClrPropertySetter<,,>).MakeGenericType(
                        property.DeclaringType, property.PropertyType, property.PropertyType.UnwrapNullableType())
                    : typeof(ClrPropertySetter<,>).MakeGenericType(types),
                property.SetMethod.CreateDelegate(typeof(Action<,>).MakeGenericType(types)));
        }
    }
}
