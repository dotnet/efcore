// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

        /// <inheritdoc />
        public virtual void ProcessNavigationAdded(
            IConventionNavigationBuilder navigationBuilder,
            IConventionContext<IConventionNavigationBuilder> context)
        {
            var navigation = navigationBuilder.Metadata;
            var foreignKey = navigation.ForeignKey;
            var relationshipBuilder = foreignKey.Builder;
            var modelBuilder = navigationBuilder.ModelBuilder;

            if (!IsNonNullable(modelBuilder, navigation)
                || navigation.IsCollection)
            {
                return;
            }

            if (!navigation.IsOnDependent)
            {
                var inverse = navigation.Inverse;
                if (inverse != null)
                {
                    if (IsNonNullable(modelBuilder, inverse))
                    {
                        Dependencies.Logger.NonNullableReferenceOnBothNavigations(navigation, inverse);
                        return;
                    }
                }

                if (!navigation.ForeignKey.IsUnique
                    || foreignKey.GetPrincipalEndConfigurationSource() != null)
                {
                    Dependencies.Logger.NonNullableReferenceOnDependent(navigation.ForeignKey.PrincipalToDependent);
                    return;
                }

                relationshipBuilder = relationshipBuilder.HasEntityTypes(
                    foreignKey.DeclaringEntityType,
                    foreignKey.PrincipalEntityType);

                if (relationshipBuilder == null)
                {
                    return;
                }

                Dependencies.Logger.NonNullableInverted(relationshipBuilder.Metadata.DependentToPrincipal);
            }

            relationshipBuilder = relationshipBuilder.IsRequired(true);

            if (relationshipBuilder != null)
            {
                context.StopProcessingIfChanged(relationshipBuilder.Metadata.DependentToPrincipal?.Builder);
            }
        }

        private bool IsNonNullable(IConventionModelBuilder modelBuilder, IConventionNavigation navigation)
            => navigation.DeclaringEntityType.HasClrType()
                && navigation.DeclaringEntityType.GetRuntimeProperties().Find(navigation.Name) is PropertyInfo propertyInfo
                && IsNonNullableReferenceType(modelBuilder, propertyInfo);
    }
}
