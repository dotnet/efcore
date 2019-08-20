// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the principal side of the relationship as required if the
    ///     <see cref="RequiredAttribute"/> is applied on the navigation property to the principal entity type 
    /// </summary>
    public class RequiredNavigationAttributeConvention : NavigationAttributeConventionBase<RequiredAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RequiredNavigationAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public RequiredNavigationAttributeConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     Called after a navigation property that has an attribute is added to an entity type.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the relationship. </param>
        /// <param name="navigation"> The navigation. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public override void ProcessNavigationAdded(
            IConventionRelationshipBuilder relationshipBuilder,
            IConventionNavigation navigation,
            RequiredAttribute attribute,
            IConventionContext<IConventionNavigation> context)
        {
            Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));
            Check.NotNull(navigation, nameof(navigation));
            Check.NotNull(attribute, nameof(attribute));

            if (navigation.IsCollection())
            {
                Dependencies.Logger.RequiredAttributeOnCollection(navigation.ForeignKey.DependentToPrincipal);
                return;
            }

            if (!navigation.IsDependentToPrincipal())
            {
                var inverse = navigation.FindInverse();
                if (inverse != null)
                {
                    var attributes = GetAttributes<RequiredAttribute>(inverse.DeclaringEntityType, inverse);
                    if (attributes.Any())
                    {
                        Dependencies.Logger.RequiredAttributeOnBothNavigations(navigation, inverse);
                        return;
                    }
                }

                if (relationshipBuilder.Metadata.GetPrincipalEndConfigurationSource() != null)
                {
                    Dependencies.Logger.RequiredAttributeOnDependent(navigation.ForeignKey.PrincipalToDependent);
                    return;
                }

                var newRelationshipBuilder = relationshipBuilder.HasEntityTypes(
                    relationshipBuilder.Metadata.DeclaringEntityType,
                    relationshipBuilder.Metadata.PrincipalEntityType);

                if (newRelationshipBuilder == null)
                {
                    return;
                }

                Dependencies.Logger.RequiredAttributeInverted(newRelationshipBuilder.Metadata.DependentToPrincipal);
                relationshipBuilder = newRelationshipBuilder;
            }

            relationshipBuilder.IsRequired(true, fromDataAnnotation: true);

            context.StopProcessingIfChanged(relationshipBuilder.Metadata.DependentToPrincipal);
        }
    }
}
