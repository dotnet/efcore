// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the principal side of the relationship as required if the
    ///     <see cref="RequiredAttribute" /> is applied on the navigation property to the principal entity type.
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

        /// <inheritdoc/>
        public override void ProcessNavigationAdded(
            IConventionNavigationBuilder navigationBuilder,
            RequiredAttribute attribute,
            IConventionContext<IConventionNavigationBuilder> context)
        {
            var navigation = navigationBuilder.Metadata;
            var foreignKey = navigation.ForeignKey;
            if (navigation.IsCollection)
            {
                Dependencies.Logger.RequiredAttributeOnCollection(foreignKey.DependentToPrincipal);
                return;
            }

            var relationshipBuilder = foreignKey.Builder;
            if (!navigation.IsOnDependent)
            {
                var inverse = navigation.Inverse;
                if (inverse != null)
                {
                    var attributes = GetAttributes<RequiredAttribute>(inverse.DeclaringEntityType, inverse);
                    if (attributes.Any())
                    {
                        Dependencies.Logger.RequiredAttributeOnBothNavigations(navigation, inverse);
                        return;
                    }
                }

                if (foreignKey.GetPrincipalEndConfigurationSource() != null)
                {
                    Dependencies.Logger.RequiredAttributeOnDependent(foreignKey.PrincipalToDependent);
                    return;
                }

                relationshipBuilder = relationshipBuilder.HasEntityTypes(
                    foreignKey.DeclaringEntityType,
                    foreignKey.PrincipalEntityType);

                if (relationshipBuilder == null)
                {
                    return;
                }

                Dependencies.Logger.RequiredAttributeInverted(relationshipBuilder.Metadata.DependentToPrincipal);
            }

            relationshipBuilder = relationshipBuilder.IsRequired(true, fromDataAnnotation: true);
            if (relationshipBuilder == null)
            {
                return;
            }

            context.StopProcessingIfChanged(relationshipBuilder.Metadata.DependentToPrincipal?.Builder);
        }
    }
}
