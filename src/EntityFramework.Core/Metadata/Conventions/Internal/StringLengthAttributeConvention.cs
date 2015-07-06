// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class StringLengthAttributeConvention : PropertyAttributeConvention<StringLengthAttribute>
    {
        public override InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder, StringLengthAttribute attribute)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(attribute, nameof(attribute));

            if (attribute.MaximumLength > 0)
            {
                propertyBuilder.MaxLength(attribute.MaximumLength, ConfigurationSource.DataAnnotation);
            }

            return propertyBuilder;
        }
    }
}
