// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Configuration.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityConfiguration
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityConfiguration(
            [NotNull] ModelConfiguration modelConfiguration,
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(modelConfiguration, nameof(modelConfiguration));
            Check.NotNull(entityType, nameof(entityType));

            ModelConfiguration = modelConfiguration;
            EntityType = entityType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ModelConfiguration ModelConfiguration { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEntityType EntityType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual List<IAttributeConfiguration> AttributeConfigurations { get; } = new List<IAttributeConfiguration>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual List<IFluentApiConfiguration> FluentApiConfigurations { get; } = new List<IFluentApiConfiguration>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual List<PropertyConfiguration> PropertyConfigurations { get; } = new List<PropertyConfiguration>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual List<NavigationPropertyConfiguration> NavigationPropertyConfigurations { get; } =
            new List<NavigationPropertyConfiguration>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual List<NavigationPropertyInitializerConfiguration> NavigationPropertyInitializerConfigurations { get; } =
            new List<NavigationPropertyInitializerConfiguration>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual List<RelationshipConfiguration> RelationshipConfigurations { get; } = new List<RelationshipConfiguration>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual PropertyConfiguration FindPropertyConfiguration([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return PropertyConfigurations.FirstOrDefault(pc => pc.Property == property);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual PropertyConfiguration GetOrAddPropertyConfiguration(
            [NotNull] EntityConfiguration entityConfiguration, [NotNull] Property property)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));
            Check.NotNull(property, nameof(property));

            var propertyConfiguration = FindPropertyConfiguration(property);
            if (propertyConfiguration == null)
            {
                propertyConfiguration = new PropertyConfiguration(entityConfiguration, property);
                PropertyConfigurations.Add(propertyConfiguration);
            }

            return propertyConfiguration;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual List<IFluentApiConfiguration> GetFluentApiConfigurations(bool useFluentApiOnly)
        {
            return (useFluentApiOnly
                    ? FluentApiConfigurations
                    : FluentApiConfigurations.Where(flc => !flc.HasAttributeEquivalent))
                .ToList();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual List<PropertyConfiguration> GetPropertyConfigurations(bool useFluentApiOnly)
        {
            return PropertyConfigurations
                .Where(pc => pc.GetFluentApiConfigurations(useFluentApiOnly).Any()).ToList();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual List<RelationshipConfiguration> GetRelationshipConfigurations(bool useFluentApiOnly)
            => RelationshipConfigurations
                .Where(rc => useFluentApiOnly || !rc.HasAttributeEquivalent)
                .ToList();
    }
}
