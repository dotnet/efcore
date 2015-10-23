// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class ClrPropertyGetterFactory : ClrAccessorFactory<IClrPropertyGetter>
    {
        public override IClrPropertyGetter Create(PropertyInfo property)
        {
            var types = new[] { property.DeclaringType, property.PropertyType };
            var getterType = typeof(ClrPropertyGetter<,>).MakeGenericType(types);
            var funcType = typeof(Func<,>).MakeGenericType(types);
            return (IClrPropertyGetter)Activator.CreateInstance(getterType, property.GetMethod.CreateDelegate(funcType));
        }
    }
}
