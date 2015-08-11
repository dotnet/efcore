// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public abstract class PropertyAttributeConvention<TAttribute> : IPropertyConvention
        where TAttribute : Attribute
    {
        public virtual InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var clrType = propertyBuilder.Metadata.DeclaringEntityType.ClrType;
            var propertyName = propertyBuilder.Metadata.Name;

            var propertyInfo = clrType?.GetRuntimeProperties().FirstOrDefault(pi => pi.Name == propertyName);
            var attributes = propertyInfo?.GetCustomAttributes<TAttribute>(true);
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    propertyBuilder = Apply(propertyBuilder, attribute, propertyInfo);
                    if (propertyBuilder == null)
                    {
                        break;
                    }
                }
            }
            return propertyBuilder;
        }

        public abstract InternalPropertyBuilder Apply([NotNull] InternalPropertyBuilder propertyBuilder, [NotNull] TAttribute attribute, [NotNull] PropertyInfo clrProperty);
    }
}
