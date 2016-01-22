// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class TimestampAttributeConvention : PropertyAttributeConvention<TimestampAttribute>
    {
        public override InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder, TimestampAttribute attribute, PropertyInfo clrProperty)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(attribute, nameof(attribute));
            Check.NotNull(clrProperty, nameof(clrProperty));

            propertyBuilder.ValueGenerated(ValueGenerated.OnAddOrUpdate, ConfigurationSource.DataAnnotation);
            propertyBuilder.IsConcurrencyToken(true, ConfigurationSource.DataAnnotation);

            return propertyBuilder;
        }
    }
}
