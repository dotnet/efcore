// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public abstract class DbContextCodeGeneratorHelper
    {
        private const string _defaultDbContextName = "ModelContext";
        private static readonly KeyDiscoveryConvention _keyDiscoveryConvention = new KeyDiscoveryConvention();

        public DbContextCodeGeneratorHelper([NotNull]DbContextGeneratorModel generatorModel)
        {
            Check.NotNull(generatorModel, nameof(generatorModel));

            GeneratorModel = generatorModel;
        }

        public virtual DbContextGeneratorModel GeneratorModel { get; [param: NotNull]private set; }

        public virtual IEnumerable<IEntityType> OrderedEntityTypes()
        {
            // default ordering is by Name, which is what we want here
            return GeneratorModel.MetadataModel.EntityTypes;
        }


        public virtual IEnumerable<IProperty> OrderedProperties([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return GeneratorModel.Generator.ModelUtilities.OrderedProperties(entityType);
        }

        public virtual string VerbatimStringLiteral([NotNull]string stringLiteral)
        {
            Check.NotNull(stringLiteral, nameof(stringLiteral));

            return CSharpUtilities.Instance.GenerateVerbatimStringLiteral(stringLiteral);
        }

        public virtual string ClassName([CanBeNull]string connectionString)
        {
            return _defaultDbContextName;
        }

        public virtual IEnumerable<EntityConfiguration> EntityConfigurations
        {
            get
            {
                var entityConfigurations = new List<EntityConfiguration>();

                foreach (var entityType in OrderedEntityTypes())
                {
                    var entityConfiguration = new EntityConfiguration(entityType);

                    AddEntityFacetsConfiguration(entityConfiguration);
                    AddEntityPropertiesConfiguration(entityConfiguration);

                    if (entityConfiguration.FacetConfigurations.Any()
                        || entityConfiguration.PropertyConfigurations.Any())
                    {
                        entityConfigurations.Add(entityConfiguration);
                    }
                }

                return entityConfigurations;
            }
        }


        public virtual IEnumerable<NavigationConfiguration> NavigationConfigurations
        {
            get
            {
                var navigationConfigurations = new List<NavigationConfiguration>();

                foreach (var entityType in OrderedEntityTypes())
                {
                    var navigationConfiguration = new NavigationConfiguration(entityType);

                    AddNavigationFacetsConfiguration(navigationConfiguration);

                    if (navigationConfiguration.FacetConfigurations.Any())
                    {
                        navigationConfigurations.Add(navigationConfiguration);
                    }
                }

                return navigationConfigurations;
            }
        }

        public virtual void AddEntityFacetsConfiguration([NotNull]EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            AddEntityKeyConfiguration(entityConfiguration);
            AddTableNameFacetConfiguration(entityConfiguration);
        }

        public abstract void AddNavigationFacetsConfiguration(
            [NotNull]NavigationConfiguration navigationConfiguration);

        public virtual void AddEntityKeyConfiguration([NotNull]EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            var entityType = (EntityType)entityConfiguration.EntityType;
            var key = entityType.FindPrimaryKey();
            if (key == null || key.Properties.Count == 0)
            {
                return;
            }

            var conventionKeyProperties =
                _keyDiscoveryConvention.DiscoverKeyProperties(entityType);
            if (conventionKeyProperties != null
                && Enumerable.SequenceEqual(
                        key.Properties.OrderBy(p => p.Name),
                        conventionKeyProperties.OrderBy(p => p.Name)))
            {
                return;
            }

            entityConfiguration.AddFacetConfiguration(
                new FacetConfiguration(
                    "Key(e => "
                    + GeneratorModel.Generator.ModelUtilities.GenerateLambdaToKey(key.Properties, "e")
                    + ")"));
        }

        public virtual void AddTableNameFacetConfiguration([NotNull]EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            var entityType = entityConfiguration.EntityType;
            if (entityType.Relational().Schema != null
                && entityType.Relational().Schema != "dbo")
            {
                entityConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        "ForRelational()",
                        string.Format(CultureInfo.InvariantCulture, "Table({0}, {1})",
                            CSharpUtilities.Instance.DelimitString(entityType.Relational().Table),
                            CSharpUtilities.Instance.DelimitString(entityType.Relational().Schema))));
            }
            else if (entityType.Relational().Table != null
                     && entityType.Relational().Table != entityType.DisplayName())
            {
                entityConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        "ForRelational()",
                        string.Format(CultureInfo.InvariantCulture, "Table({0})",
                            CSharpUtilities.Instance.DelimitString(entityType.Relational().Table))));
            }
        }

        public virtual void AddEntityPropertiesConfiguration([NotNull]EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            foreach (var property in OrderedProperties(entityConfiguration.EntityType))
            {
                var propertyConfiguration = new PropertyConfiguration(entityConfiguration, property);

                AddPropertyFacetsConfiguration(propertyConfiguration);

                if (propertyConfiguration.FacetConfigurations.Any())
                {
                    entityConfiguration.AddPropertyConfiguration(propertyConfiguration);
                }
            }
        }

        public virtual void AddPropertyFacetsConfiguration([NotNull]PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            AddMaxLengthFacetConfiguration(propertyConfiguration);
            AddStoreComputedFacetConfiguration(propertyConfiguration);
            AddColumnNameFacetConfiguration(propertyConfiguration);
            AddColumnTypeFacetConfiguration(propertyConfiguration);
            AddDefaultValueFacetConfiguration(propertyConfiguration);
            AddDefaultExpressionFacetConfiguration(propertyConfiguration);
        }

        public virtual void AddMaxLengthFacetConfiguration(
            [NotNull]PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (((Property)propertyConfiguration.Property).GetMaxLength().HasValue)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        string.Format(CultureInfo.InvariantCulture,
                            "MaxLength({0})",
                            CSharpUtilities.Instance.GenerateLiteral(
                                ((Property)propertyConfiguration.Property).GetMaxLength().Value))));
            }
        }

        public virtual void AddStoreComputedFacetConfiguration(
            [NotNull]PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (((Property)propertyConfiguration.Property).IsStoreComputed.HasValue)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        string.Format(CultureInfo.InvariantCulture,
                            "StoreComputed({0})",
                            CSharpUtilities.Instance.GenerateLiteral(
                                ((Property)propertyConfiguration.Property).IsStoreComputed.Value))));
            }
        }

        public virtual void AddColumnNameFacetConfiguration(
            [NotNull]PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (propertyConfiguration.Property.Relational().Column != null
                && propertyConfiguration.Property.Relational().Column != propertyConfiguration.Property.Name)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        "ForRelational()",
                        string.Format(CultureInfo.InvariantCulture,
                            "Column({0})",
                            CSharpUtilities.Instance.DelimitString(
                                propertyConfiguration.Property.Relational().Column))));
            }
        }

        public virtual void AddColumnTypeFacetConfiguration(
            [NotNull]PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (propertyConfiguration.Property.Relational().ColumnType != null)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        "ForRelational()",
                        string.Format(CultureInfo.InvariantCulture,
                            "ColumnType({0})",
                            CSharpUtilities.Instance.DelimitString(
                                propertyConfiguration.Property.Relational().ColumnType))));
            }
        }

        public virtual void AddDefaultValueFacetConfiguration(
            [NotNull]PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (propertyConfiguration.Property.Relational().DefaultValue != null)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        "ForRelational()",
                        string.Format(CultureInfo.InvariantCulture,
                            "DefaultValue({0})",
                            CSharpUtilities.Instance.GenerateLiteral(
                                (dynamic)propertyConfiguration.Property.Relational().DefaultValue))));
            }
        }

        public virtual void AddDefaultExpressionFacetConfiguration(
            [NotNull]PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (propertyConfiguration.Property.Relational().DefaultExpression != null)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        "ForRelational()",
                        string.Format(CultureInfo.InvariantCulture,
                            "DefaultExpression({0})",
                                CSharpUtilities.Instance.DelimitString(
                                    propertyConfiguration.Property.Relational().DefaultExpression))));
            }
        }
    }

    public class EntityConfiguration
    {
        public EntityConfiguration([NotNull]IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            EntityType = entityType;
        }

        public virtual IEntityType EntityType { get;[param: NotNull]private set; }
        public virtual List<FacetConfiguration> FacetConfigurations { get; } = new List<FacetConfiguration>();
        public virtual List<PropertyConfiguration> PropertyConfigurations { get; } = new List<PropertyConfiguration>();

        public virtual void AddFacetConfiguration([NotNull]FacetConfiguration entityTypeFacetConfiguration)
        {
            Check.NotNull(entityTypeFacetConfiguration, nameof(entityTypeFacetConfiguration));

            FacetConfigurations.Add(entityTypeFacetConfiguration);
        }

        public virtual void AddPropertyConfiguration([NotNull]PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            PropertyConfigurations.Add(propertyConfiguration);
        }
    }

    public class FacetConfiguration
    {
        public FacetConfiguration([NotNull]string methodBody)
        {
            Check.NotNull(methodBody, nameof(methodBody));

            MethodBody = methodBody;
        }

        public FacetConfiguration([NotNull]string @for, [NotNull]string methodBody)
        {
            Check.NotNull(@for, nameof(@for));
            Check.NotNull(methodBody, nameof(methodBody));

            For = @for;
            MethodBody = methodBody;
        }

        public virtual string For { get;[param: NotNull]private set; }
        public virtual string MethodBody { get;[param: NotNull]private set; }

        public override string ToString()
        {
            return (For == null ? "." + MethodBody : "." + For + "." + MethodBody);
        }
    }

    public class PropertyConfiguration
    {
        public PropertyConfiguration(
            [NotNull]EntityConfiguration entityConfiguration, [NotNull]IProperty property)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));
            Check.NotNull(property, nameof(property));

            EntityConfiguration = entityConfiguration;
            Property = property;
        }

        public virtual EntityConfiguration EntityConfiguration { get;[param: NotNull]private set; }
        public virtual IProperty Property { get;[param: NotNull]private set; }
        public virtual Dictionary<string, List<string>> FacetConfigurations { get; } = new Dictionary<string, List<string>>();

        public virtual void AddFacetConfiguration([NotNull]FacetConfiguration facetConfiguration)
        {
            Check.NotNull(facetConfiguration, nameof(facetConfiguration));

            var @for = facetConfiguration.For ?? string.Empty;
            List<string> listOfFacetMethodBodies; 
            if (!FacetConfigurations.TryGetValue(@for, out listOfFacetMethodBodies))
            {
                listOfFacetMethodBodies = new List<string>();
                FacetConfigurations.Add(@for, listOfFacetMethodBodies);
            }
            listOfFacetMethodBodies.Add(facetConfiguration.MethodBody);
        }
    }

    public class NavigationConfiguration
    {
        public NavigationConfiguration([NotNull]IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            EntityType = entityType;
        }

        public virtual IEntityType EntityType { get;[param: NotNull]private set; }
        public virtual List<FacetConfiguration> FacetConfigurations { get; } = new List<FacetConfiguration>();

        public virtual void AddFacetConfiguration([NotNull]FacetConfiguration navigationFacetConfiguration)
        {
            Check.NotNull(navigationFacetConfiguration, nameof(navigationFacetConfiguration));

            FacetConfigurations.Add(navigationFacetConfiguration);
        }
    }
}