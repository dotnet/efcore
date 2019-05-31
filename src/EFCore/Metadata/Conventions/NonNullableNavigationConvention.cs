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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class NonNullableNavigationConvention : NonNullableConvention, INavigationAddedConvention
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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

            if (!IsNonNullable(navigation)
                || navigation.IsCollection())
            {
                return;
            }

            if (!navigation.IsDependentToPrincipal())
            {
                var inverse = navigation.FindInverse();
                if (inverse != null)
                {
                    if (IsNonNullable(inverse))
                    {
                        Dependencies.Logger.NonNullableReferenceOnBothNavigations(navigation, inverse);
                        return;
                    }
                }

                if (!navigation.ForeignKey.IsUnique
                    || relationshipBuilder.Metadata.GetPrincipalEndConfigurationSource() != null)
                {
                    return;
                }

                var newRelationshipBuilder = relationshipBuilder.HasEntityTypes(
                    relationshipBuilder.Metadata.DeclaringEntityType,
                    relationshipBuilder.Metadata.PrincipalEntityType);

                if (newRelationshipBuilder == null)
                {
                    return;
                }

                Dependencies.Logger.NonNullableOnDependent(newRelationshipBuilder.Metadata.DependentToPrincipal);
                relationshipBuilder = newRelationshipBuilder;
            }

            relationshipBuilder.IsRequired(true);

            context.StopProcessingIfChanged(relationshipBuilder.Metadata.DependentToPrincipal);
        }

        private bool IsNonNullable(IConventionNavigation navigation)
            => navigation.DeclaringEntityType.HasClrType()
               && navigation.DeclaringEntityType.GetRuntimeProperties().Find(navigation.Name) is PropertyInfo propertyInfo
               && IsNonNullable(propertyInfo);
    }
}
