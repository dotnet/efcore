// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class RequiredNavigationAttributeConvention : NavigationAttributeNavigationConvention<RequiredAttribute>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public RequiredNavigationAttributeConvention([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
            : base(logger)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override InternalRelationshipBuilder Apply(
            InternalRelationshipBuilder relationshipBuilder, Navigation navigation, RequiredAttribute attribute)
        {
            Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));
            Check.NotNull(navigation, nameof(navigation));
            Check.NotNull(attribute, nameof(attribute));

            if (navigation.IsCollection())
            {
                return relationshipBuilder;
            }

            if (!navigation.IsDependentToPrincipal())
            {
                var inverse = navigation.FindInverse();
                if (inverse != null)
                {
                    var attributes = GetAttributes<RequiredAttribute>(inverse.DeclaringEntityType, inverse.Name);
                    if (attributes.Any())
                    {
                        Logger.RequiredAttributeOnBothNavigations(navigation, inverse);
                        return relationshipBuilder;
                    }
                }

                if (!navigation.ForeignKey.IsUnique
                    || relationshipBuilder.Metadata.GetPrincipalEndConfigurationSource() != null)
                {
                    return relationshipBuilder;
                }

                var newRelationshipBuilder = relationshipBuilder.HasEntityTypes(
                    relationshipBuilder.Metadata.DeclaringEntityType,
                    relationshipBuilder.Metadata.PrincipalEntityType,
                    ConfigurationSource.Convention);

                if (newRelationshipBuilder == null)
                {
                    return relationshipBuilder;
                }

                Logger.RequiredAttributeOnDependent(newRelationshipBuilder.Metadata.DependentToPrincipal);
                relationshipBuilder = newRelationshipBuilder;
            }

            return relationshipBuilder.IsRequired(true, ConfigurationSource.DataAnnotation) ?? relationshipBuilder;
        }
    }
}
