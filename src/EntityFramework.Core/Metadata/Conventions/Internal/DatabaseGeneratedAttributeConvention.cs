// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class DatabaseGeneratedAttributeConvention : PropertyAttributeConvention<DatabaseGeneratedAttribute>
    {
        public override InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder, DatabaseGeneratedAttribute attribute)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(attribute, nameof(attribute));

            StoreGeneratedPattern storeGeneratedPattern;
            Enum.TryParse(attribute.DatabaseGeneratedOption.ToString(), out storeGeneratedPattern);

            propertyBuilder.StoreGeneratedPattern(storeGeneratedPattern, ConfigurationSource.DataAnnotation);

            return propertyBuilder;
        }
    }
}
