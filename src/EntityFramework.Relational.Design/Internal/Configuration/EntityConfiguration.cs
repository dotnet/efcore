// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Scaffolding.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Internal.Configuration
{
    public class EntityConfiguration
    {
        public EntityConfiguration(
            [NotNull] ModelConfiguration modelConfiguration,
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(modelConfiguration, nameof(modelConfiguration));
            Check.NotNull(entityType, nameof(entityType));

            ModelConfiguration = modelConfiguration;
            EntityType = entityType;
        }

        public virtual ModelConfiguration ModelConfiguration { get; }
        public virtual IEntityType EntityType { get; }
        public virtual List<IAttributeConfiguration> AttributeConfigurations { get; } = new List<IAttributeConfiguration>();
        public virtual List<IFluentApiConfiguration> FluentApiConfigurations { get; } = new List<IFluentApiConfiguration>();
        public virtual List<PropertyConfiguration> PropertyConfigurations { get; } = new List<PropertyConfiguration>();
        public virtual List<NavigationPropertyConfiguration> NavigationPropertyConfigurations { get; }
            = new List<NavigationPropertyConfiguration>();
        public virtual List<NavigationPropertyInitializerConfiguration>
            NavigationPropertyInitializerConfigurations { get; } = new List<NavigationPropertyInitializerConfiguration>();
        public virtual List<RelationshipConfiguration> RelationshipConfigurations { get; } = new List<RelationshipConfiguration>();

        public virtual PropertyConfiguration FindPropertyConfiguration([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return PropertyConfigurations.FirstOrDefault(pc => pc.Property == property);
        }

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

        public virtual List<IFluentApiConfiguration> GetFluentApiConfigurations(bool useFluentApiOnly)
        {
            return (useFluentApiOnly
                ? FluentApiConfigurations
                : FluentApiConfigurations.Where(flc => !flc.HasAttributeEquivalent))
                .ToList();
        }

        public virtual List<PropertyConfiguration> GetPropertyConfigurations(bool useFluentApiOnly)
        {
            return PropertyConfigurations
                .Where(pc => pc.GetFluentApiConfigurations(useFluentApiOnly).Any()).ToList();
        }

        public virtual List<RelationshipConfiguration> GetRelationshipConfigurations(bool useFluentApiOnly)
            => RelationshipConfigurations
                .Where(rc => useFluentApiOnly || !rc.HasAttributeEquivalent)
                .ToList();
    }
}
