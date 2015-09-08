// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class ClrPropertySetterSource : ClrAccessorSource<IClrPropertySetter>
    {
        protected override IClrPropertySetter CreateGeneric<TEntity, TValue, TNonNullableEnumValue>(PropertyInfo property)
        {
            // TODO: Handle case where there is not setter or setter is private on a base type
            // Issue #753
            var setterDelegate = (Action<TEntity, TValue>)property.SetMethod.CreateDelegate(typeof(Action<TEntity, TValue>));

            return (property.PropertyType.IsNullableType()
                    && property.PropertyType.UnwrapNullableType().GetTypeInfo().IsEnum) ?
                new NullableEnumClrPropertySetter<TEntity, TValue, TNonNullableEnumValue>(setterDelegate) :
                (IClrPropertySetter)new ClrPropertySetter<TEntity, TValue>(setterDelegate);
        }
    }
}
