// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ClrPropertySetterSource : ClrAccessorSource<IClrPropertySetter>
    {
        protected override IClrPropertySetter CreateGeneric<TEntity, TValue>(PropertyInfo property)
        {
            Check.NotNull(property, "property");

            // TODO: Handle case where there is not setter or setter is private on a base type
            var setterDelegate = (Action<TEntity, TValue>)property.SetMethod.CreateDelegate(typeof(Action<TEntity, TValue>));

            return new ClrPropertySetter<TEntity, TValue>(setterDelegate);
        }
    }
}
