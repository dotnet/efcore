// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RequiredNavigationAttributeConvention : NavigationAttributeNavigationConvention<RequiredAttribute>
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Model> _logger;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RequiredNavigationAttributeConvention([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override InternalRelationshipBuilder Apply(
            InternalRelationshipBuilder relationshipBuilder, Navigation navigation, RequiredAttribute attribute)
        {
            Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));
            Check.NotNull(navigation, nameof(navigation));
            Check.NotNull(attribute, nameof(attribute));

            if (!navigation.IsDependentToPrincipal())
            {
                var inverse = navigation.FindInverse();
                if (inverse != null)
                {
                    var attributes = GetAttributes<RequiredAttribute>(inverse.DeclaringEntityType, inverse.Name);
                    if (attributes.Any())
                    {
                        _logger.RequiredAttributeOnBothNavigations(navigation, inverse);
                        return relationshipBuilder;
                    }
                }

                if (!navigation.ForeignKey.IsUnique
                    || relationshipBuilder.Metadata.GetPrincipalEndConfigurationSource() != null)
                {
                    return relationshipBuilder;
                }

                var newRelationshipBuilder = relationshipBuilder.RelatedEntityTypes(
                    relationshipBuilder.Metadata.DeclaringEntityType,
                    relationshipBuilder.Metadata.PrincipalEntityType,
                    ConfigurationSource.Convention);

                if (newRelationshipBuilder == null)
                {
                    return relationshipBuilder;
                }

                _logger.RequiredAttributeOnDependent(newRelationshipBuilder.Metadata.DependentToPrincipal);
                relationshipBuilder = newRelationshipBuilder;
            }

            return relationshipBuilder.IsRequired(true, ConfigurationSource.DataAnnotation) ?? relationshipBuilder;
        }
    }
}
