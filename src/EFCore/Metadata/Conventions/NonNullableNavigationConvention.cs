// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the non-nullable navigations to principal entity type as required.
    /// </summary>
    public class NonNullableNavigationConvention : NonNullableConventionBase, INavigationAddedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="NonNullableNavigationConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public NonNullableNavigationConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     Called after a navigation is added to the entity type.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="navigation"> The navigation. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessNavigationAdded(
            IConventionRelationshipBuilder relationshipBuilder,
            IConventionNavigation navigation,
            IConventionContext<IConventionNavigation> context)
        {
            Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));
            Check.NotNull(navigation, nameof(navigation));

            var modelBuilder = relationshipBuilder.ModelBuilder;

            if (!IsNonNullable(modelBuilder, navigation) || navigation.IsCollection())
            {
                return;
            }

            if (!navigation.IsDependentToPrincipal())
            {
                var inverse = navigation.FindInverse();
                if (inverse != null)
                {
                    if (IsNonNullable(modelBuilder, inverse))
                    {
                        Dependencies.Logger.NonNullableReferenceOnBothNavigations(navigation, inverse);
                        return;
                    }
                }

                if (!navigation.ForeignKey.IsUnique
                    || relationshipBuilder.Metadata.GetPrincipalEndConfigurationSource() != null)
                {
                    Dependencies.Logger.NonNullableReferenceOnDependent(navigation.ForeignKey.PrincipalToDependent);
                    return;
                }

                var newRelationshipBuilder = relationshipBuilder.HasEntityTypes(
                    relationshipBuilder.Metadata.DeclaringEntityType,
                    relationshipBuilder.Metadata.PrincipalEntityType);

                if (newRelationshipBuilder == null)
                {
                    return;
                }

                Dependencies.Logger.NonNullableInverted(newRelationshipBuilder.Metadata.DependentToPrincipal);
                relationshipBuilder = newRelationshipBuilder;
            }

            relationshipBuilder.IsRequired(true);

            context.StopProcessingIfChanged(relationshipBuilder.Metadata.DependentToPrincipal);
        }

        private bool IsNonNullable(IConventionModelBuilder modelBuilder, IConventionNavigation navigation)
            => navigation.DeclaringEntityType.HasClrType()
               && navigation.DeclaringEntityType.GetRuntimeProperties().Find(navigation.Name) is PropertyInfo propertyInfo
               && IsNonNullableReferenceType(modelBuilder, propertyInfo);
    }
}
