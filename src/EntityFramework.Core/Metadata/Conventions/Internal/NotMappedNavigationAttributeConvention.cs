// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class NotMappedNavigationAttributeConvention : NavigationAttributeConvention<NotMappedAttribute>
    {
        public override InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder, Navigation navigation, NotMappedAttribute attribute)
        {
            Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));
            Check.NotNull(navigation, nameof(navigation));
            Check.NotNull(attribute, nameof(attribute));

            var entityTypeBuilder = relationshipBuilder.ModelBuilder.Entity(navigation.DeclaringEntityType.Name, ConfigurationSource.DataAnnotation);
            return entityTypeBuilder.Ignore(navigation.Name, ConfigurationSource.DataAnnotation) ? null : relationshipBuilder;
        }
    }
}
