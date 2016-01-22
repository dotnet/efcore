// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public static class PropertyBaseExtensions
    {
        public static PropertyAccessors GetPropertyAccessors([NotNull] this IPropertyBase propertyBase)
        {
            var accessors = propertyBase as IPropertyBaseAccessors;

            return accessors != null
                ? accessors.Accessors
                : new PropertyAccessorsFactory().Create(propertyBase);
        }

        public static IClrPropertyGetter GetGetter([NotNull] this IPropertyBase propertyBase)
        {
            var accessors = propertyBase as IPropertyBaseAccessors;

            return accessors != null
                ? accessors.Getter
                : new ClrPropertyGetterFactory().Create(propertyBase);
        }

        public static IClrPropertySetter GetSetter([NotNull] this IPropertyBase propertyBase)
        {
            var accessors = propertyBase as IPropertyBaseAccessors;

            return accessors != null
                ? accessors.Setter
                : new ClrPropertySetterFactory().Create(propertyBase);
        }
    }
}
