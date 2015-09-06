// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration
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

        public virtual ModelConfiguration ModelConfiguration { get;[param: NotNull] private set; }
        public virtual IEntityType EntityType { get; [param: NotNull] private set; }
        public virtual List<IAttributeConfiguration> AttributeConfigurations { get; } = new List<IAttributeConfiguration>();
        public virtual List<IFluentApiConfiguration> FluentApiConfigurations { get; } = new List<IFluentApiConfiguration>();
        public virtual List<PropertyConfiguration> PropertyConfigurations { get; } = new List<PropertyConfiguration>();
        public virtual List<NavigationPropertyConfiguration> NavigationPropertyConfigurations { get; }
            = new List<NavigationPropertyConfiguration>();
        public virtual List<NavigationPropertyInitializerConfiguration>
            NavigationPropertyInitializerConfigurations { get; } = new List<NavigationPropertyInitializerConfiguration>();
        public virtual List<RelationshipConfiguration> RelationshipConfigurations { get; } = new List<RelationshipConfiguration>();

        public virtual string ErrorMessageAnnotation
            => (string)EntityType[RelationalMetadataModelProvider.AnnotationNameEntityTypeError];

        public virtual List<IFluentApiConfiguration> NonAttributeFluentApiConfigurations
        {
            get
            {
                return FluentApiConfigurations.Where(fc => !fc.HasAttributeEquivalent).ToList();
            }
        }

        public virtual PropertyConfiguration FindPropertyConfiguration([NotNull] Property property)
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

        public virtual List<IFluentApiConfiguration> GetFluentApiConfigurations(bool useAttributesOverFluentApi)
        {
            return (useAttributesOverFluentApi
                ? FluentApiConfigurations.Where(flc => !flc.HasAttributeEquivalent)
                : FluentApiConfigurations)
                .ToList();
        }

        public virtual List<PropertyConfiguration> GetPropertyConfigurations(bool useAttributesOverFluentApi)
        {
            return PropertyConfigurations
                .Where(pc => pc.GetFluentApiConfigurations(useAttributesOverFluentApi).Any()).ToList();
        }

        public virtual List<RelationshipConfiguration> GetRelationshipConfigurations(bool useAttributesOverFluentApi)
        {
            return useAttributesOverFluentApi
                ? Enumerable.Empty<RelationshipConfiguration>().ToList()
                : RelationshipConfigurations;
        }
    }
}
