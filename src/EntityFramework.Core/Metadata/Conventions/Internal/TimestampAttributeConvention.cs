// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class TimestampAttributeConvention : PropertyAttributeConvention<TimestampAttribute>
    {
        public override InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder, TimestampAttribute attribute, PropertyInfo clrProperty)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(attribute, nameof(attribute));
            Check.NotNull(clrProperty, nameof(clrProperty));

            propertyBuilder.ValueGenerated(ValueGenerated.OnAddOrUpdate, ConfigurationSource.DataAnnotation);
            propertyBuilder.ConcurrencyToken(true, ConfigurationSource.DataAnnotation);

            return propertyBuilder;
        }
    }
}
