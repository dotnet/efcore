// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
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

        /// <inheritdoc />
        public override void ProcessNavigationAdded(
            IConventionNavigationBuilder navigationBuilder,
            RequiredAttribute attribute,
            IConventionContext<IConventionNavigationBuilder> context)
        {
            ProcessNavigation(navigationBuilder);
            context.StopProcessingIfChanged(navigationBuilder.Metadata.Builder);
        }

        /// <inheritdoc />
        public override void ProcessForeignKeyPrincipalEndChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IEnumerable<RequiredAttribute> dependentToPrincipalAttributes,
            IEnumerable<RequiredAttribute> principalToDependentAttributes,
            IConventionContext<IConventionForeignKeyBuilder> context)
        {
            var fk = relationshipBuilder.Metadata;
            if (dependentToPrincipalAttributes != null
                && dependentToPrincipalAttributes.Any())
            {
                ProcessNavigation(fk.DependentToPrincipal.Builder);
            }

            if (principalToDependentAttributes != null
                && principalToDependentAttributes.Any())
            {
                ProcessNavigation(fk.PrincipalToDependent.Builder);
            }

            context.StopProcessingIfChanged(relationshipBuilder.Metadata.Builder);
        }

        private void ProcessNavigation(IConventionNavigationBuilder navigationBuilder)
        {
            var navigation = navigationBuilder.Metadata;
            var foreignKey = navigation.ForeignKey;
            if (navigation.IsCollection)
            {
                Dependencies.Logger.RequiredAttributeOnCollection(navigation);
                return;
            }

            if (foreignKey.GetPrincipalEndConfigurationSource() != null)
            {
                if (navigation.IsOnDependent)
                {
                    foreignKey.Builder.IsRequired(true, fromDataAnnotation: true);
                }
                else
                {
                    foreignKey.Builder.IsRequiredDependent(true, fromDataAnnotation: true);
                }
            }
        }

        /// <inheritdoc />
        public override void ProcessSkipNavigationAdded(
            IConventionSkipNavigationBuilder skipNavigationBuilder,
            RequiredAttribute attribute,
            IConventionContext<IConventionSkipNavigationBuilder> context)
        {
            Dependencies.Logger.RequiredAttributeOnSkipNavigation(skipNavigationBuilder.Metadata);
        }
    }
}
