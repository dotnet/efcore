// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    public class RequiredAttributeConvention : PropertyAttributeConvention<RequiredAttribute>
    {
        public override void Apply(InternalPropertyBuilder propertyBuilder, RequiredAttribute attribute)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(attribute, nameof(attribute));

            propertyBuilder.Required(true, ConfigurationSource.DataAnnotation);
        }
    }
}
