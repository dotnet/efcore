// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public abstract class DbContextCodeGeneratorHelper
    {
        private const string _defaultDbContextName = "ModelContext";
        private static readonly KeyDiscoveryConvention _keyDiscoveryConvention = new KeyDiscoveryConvention();

        public DbContextCodeGeneratorHelper([NotNull] DbContextGeneratorModel generatorModel)
        {
            Check.NotNull(generatorModel, nameof(generatorModel));

            GeneratorModel = generatorModel;
        }

        public virtual DbContextGeneratorModel GeneratorModel { get; }

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

        public virtual string VerbatimStringLiteral([NotNull] string stringLiteral)
        {
            Check.NotNull(stringLiteral, nameof(stringLiteral));

            return CSharpUtilities.Instance.GenerateVerbatimStringLiteral(stringLiteral);
        }

        public virtual string ClassName([CanBeNull] string connectionString)
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
                    AddNavigationsConfiguration(entityConfiguration);

                    if (entityConfiguration.FacetConfigurations.Any()
                        || entityConfiguration.PropertyConfigurations.Any()
                        || entityConfiguration.NavigationConfigurations.Any())
                    {
                        entityConfigurations.Add(entityConfiguration);
                    }
                }

                return entityConfigurations;
            }
        }

        public virtual void AddEntityFacetsConfiguration([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            AddEntityKeyConfiguration(entityConfiguration);
            AddTableNameFacetConfiguration(entityConfiguration);
        }

        public abstract void AddNavigationsConfiguration([NotNull] EntityConfiguration entityConfiguration);

        public virtual void AddEntityKeyConfiguration([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            var entityType = (EntityType)entityConfiguration.EntityType;
            var key = entityType.FindPrimaryKey();
            if (key == null
                || key.Properties.Count == 0)
            {
                return;
            }

            var conventionKeyProperties =
                _keyDiscoveryConvention.DiscoverKeyProperties(entityType);
            if (conventionKeyProperties != null
                && key.Properties.OrderBy(p => p.Name).SequenceEqual(conventionKeyProperties.OrderBy(p => p.Name)))
            {
                return;
            }

            entityConfiguration.FacetConfigurations.Add(
                new FacetConfiguration(
                    "Key(e => "
                    + GeneratorModel.Generator.ModelUtilities.GenerateLambdaToKey(key.Properties, "e")
                    + ")"));
        }

        public virtual void AddTableNameFacetConfiguration([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            var entityType = entityConfiguration.EntityType;
            if (entityType.Relational().Schema != null
                && entityType.Relational().Schema != "dbo")
            {
                entityConfiguration.FacetConfigurations.Add(
                    new FacetConfiguration(
                        string.Format(CultureInfo.InvariantCulture, "Table({0}, {1})",
                            CSharpUtilities.Instance.DelimitString(entityType.Relational().Table),
                            CSharpUtilities.Instance.DelimitString(entityType.Relational().Schema))));
            }
            else if (entityType.Relational().Table != null
                     && entityType.Relational().Table != entityType.DisplayName())
            {
                entityConfiguration.FacetConfigurations.Add(
                    new FacetConfiguration(
                        string.Format(CultureInfo.InvariantCulture, "Table({0})",
                            CSharpUtilities.Instance.DelimitString(entityType.Relational().Table))));
            }
        }

        public virtual void AddEntityPropertiesConfiguration([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            foreach (var property in OrderedProperties(entityConfiguration.EntityType))
            {
                var propertyConfiguration = new PropertyConfiguration(entityConfiguration, property);

                AddPropertyFacetsConfiguration(propertyConfiguration);

                if (propertyConfiguration.FacetConfigurations.Any())
                {
                    entityConfiguration.PropertyConfigurations.Add(propertyConfiguration);
                }
            }
        }

        public virtual void AddPropertyFacetsConfiguration([NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            AddRequiredFacetConfiguration(propertyConfiguration);
            AddMaxLengthFacetConfiguration(propertyConfiguration);
            AddStoreGeneratedPatternFacetConfiguration(propertyConfiguration);
            AddColumnNameFacetConfiguration(propertyConfiguration);
            AddColumnTypeFacetConfiguration(propertyConfiguration);
            AddDefaultValueFacetConfiguration(propertyConfiguration);
            AddDefaultExpressionFacetConfiguration(propertyConfiguration);
        }

        public virtual void AddRequiredFacetConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (!propertyConfiguration.Property.IsNullable)
            {
                var entityKeyProperties =
                    ((EntityType)propertyConfiguration.EntityConfiguration.EntityType)
                        .FindPrimaryKey()?.Properties
                    ?? Enumerable.Empty<Property>();
                if (!entityKeyProperties.Contains(propertyConfiguration.Property))
                {
                    propertyConfiguration.AddFacetConfiguration(
                        new FacetConfiguration("Required()"));
                }
            }
        }

        public virtual void AddMaxLengthFacetConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
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

        public virtual void AddStoreGeneratedPatternFacetConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (propertyConfiguration.Property.StoreGeneratedPattern != StoreGeneratedPattern.None)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        string.Format(CultureInfo.InvariantCulture,
                            "StoreGeneratedPattern({0})",
                            CSharpUtilities.Instance.GenerateLiteral(
                                propertyConfiguration.Property.StoreGeneratedPattern))));
            }
        }

        public virtual void AddColumnNameFacetConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (propertyConfiguration.Property.Relational().Column != null
                && propertyConfiguration.Property.Relational().Column != propertyConfiguration.Property.Name)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        string.Format(CultureInfo.InvariantCulture,
                            "HasColumnName({0})",
                            CSharpUtilities.Instance.DelimitString(
                                propertyConfiguration.Property.Relational().Column))));
            }
        }

        public virtual void AddColumnTypeFacetConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (propertyConfiguration.Property.Relational().ColumnType != null)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        string.Format(CultureInfo.InvariantCulture,
                            "HasColumnType({0})",
                            CSharpUtilities.Instance.DelimitString(
                                propertyConfiguration.Property.Relational().ColumnType))));
            }
        }

        public virtual void AddDefaultValueFacetConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (propertyConfiguration.Property.Relational().DefaultValue != null)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        string.Format(CultureInfo.InvariantCulture,
                            "DefaultValue({0})",
                            CSharpUtilities.Instance.GenerateLiteral(
                                (dynamic)propertyConfiguration.Property.Relational().DefaultValue))));
            }
        }

        public virtual void AddDefaultExpressionFacetConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (propertyConfiguration.Property.Relational().DefaultValueSql != null)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        string.Format(CultureInfo.InvariantCulture,
                            "DefaultValueSql({0})",
                            CSharpUtilities.Instance.DelimitString(
                                propertyConfiguration.Property.Relational().DefaultValueSql))));
            }
        }
    }
}
