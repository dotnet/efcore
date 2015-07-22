// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class NotMappedPropertyAttributeConvention : PropertyAttributeConvention<NotMappedAttribute>
    {
        public override InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder, NotMappedAttribute attribute)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(attribute, nameof(attribute));

            var entityTypeBuilder = propertyBuilder.ModelBuilder.Entity(propertyBuilder.Metadata.DeclaringEntityType.Name, ConfigurationSource.DataAnnotation);
            var ignored = entityTypeBuilder.Ignore(propertyBuilder.Metadata.Name, ConfigurationSource.DataAnnotation);

            return ignored ? null : propertyBuilder;
        }
    }
}
