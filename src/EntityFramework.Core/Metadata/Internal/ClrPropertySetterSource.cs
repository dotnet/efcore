// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class ClrPropertySetterSource : ClrAccessorSource<IClrPropertySetter>
    {
        protected override IClrPropertySetter Create([NotNull] PropertyInfo property)
        {
            // TODO: Handle case where there is not setter or setter is private on a base type
            // Issue #753

            var types = new[] { property.DeclaringType, property.PropertyType };
            var actionType = typeof(Action<,>).MakeGenericType(types);

            Type setterType;
            if (property.PropertyType.IsNullableType()
                    && property.PropertyType.UnwrapNullableType().GetTypeInfo().IsEnum)
            {
                setterType = typeof(NullableEnumClrPropertySetter<,,>).MakeGenericType(property.DeclaringType, property.PropertyType, property.PropertyType.UnwrapNullableType());
            }
            else
            {
                setterType = typeof(ClrPropertySetter<,>).MakeGenericType(types);
            }

            return (IClrPropertySetter)Activator.CreateInstance(setterType, property.SetMethod.CreateDelegate(actionType));
        }
    }
}
