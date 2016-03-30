// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class RequiredNavigationAttributeConvention : NavigationAttributeNavigationConvention<RequiredAttribute>
    {
        public override InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder, Navigation navigation, RequiredAttribute attribute)
        {
            Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));
            Check.NotNull(navigation, nameof(navigation));
            Check.NotNull(attribute, nameof(attribute));

            if (!navigation.IsDependentToPrincipal())
            {
                if (!navigation.ForeignKey.IsUnique
                    || (relationshipBuilder.Metadata.GetPrincipalEndConfigurationSource() != null))
                {
                    return relationshipBuilder;
                }

                var inverse = navigation.FindInverse();
                if (inverse != null)
                {
                    var attributes = GetAttributes<RequiredAttribute>(inverse.DeclaringEntityType, inverse.Name);
                    if (attributes.Any())
                    {
                        return relationshipBuilder;
                    }
                }

                var newRelationshipBuilder = relationshipBuilder.RelatedEntityTypes(
                    relationshipBuilder.Metadata.DeclaringEntityType,
                    relationshipBuilder.Metadata.PrincipalEntityType,
                    ConfigurationSource.Convention);

                if (newRelationshipBuilder == null)
                {
                    return relationshipBuilder;
                }
                relationshipBuilder = newRelationshipBuilder;
            }

            return relationshipBuilder.IsRequired(true, ConfigurationSource.DataAnnotation) ?? relationshipBuilder;
        }
    }
}
