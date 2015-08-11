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
        private const string DefaultDbContextName = "ModelContext";
        protected static readonly KeyDiscoveryConvention _keyDiscoveryConvention = new KeyDiscoveryConvention();
        protected static readonly KeyConvention _keyConvention = new KeyConvention();

        protected DbContextCodeGeneratorHelper([NotNull] DbContextGeneratorModel generatorModel,
            IRelationalMetadataExtensionProvider extensionsProvider)
        {
            Check.NotNull(generatorModel, nameof(generatorModel));
            Check.NotNull(extensionsProvider, nameof(extensionsProvider));

            GeneratorModel = generatorModel;
            ExtensionsProvider = extensionsProvider;
        }

        protected IRelationalMetadataExtensionProvider ExtensionsProvider { get; private set; }

        public virtual DbContextGeneratorModel GeneratorModel { get; }

        // default ordering is by Name, which is what we want here but
        // do not configure EntityTypes for which we had an error when generating
        public virtual IEnumerable<IEntityType> OrderedEntityTypes() =>
            GeneratorModel.MetadataModel.EntityTypes
                .Where(e => ((EntityType)e).FindAnnotation(
                    ReverseEngineeringMetadataModelProvider.AnnotationNameEntityTypeError) == null);

        public virtual IEnumerable<IProperty> OrderedProperties([NotNull] IEntityType entityType)
            => GeneratorModel.Generator.ModelUtilities.OrderedProperties(Check.NotNull(entityType, nameof(entityType)));

        public virtual string VerbatimStringLiteral([NotNull] string stringLiteral)
            => CSharpUtilities.Instance.GenerateVerbatimStringLiteral(Check.NotNull(stringLiteral, nameof(stringLiteral)));

        public abstract string UseMethodName { get; } // "UseSqlServer" for SqlServer, "UseSqlite" for Sqlite etc

        public virtual string ClassName([CanBeNull] string connectionString) => DefaultDbContextName;

        public virtual IEnumerable<OptionsBuilderConfiguration> OnConfiguringConfigurations
        {
            get
            {
                var onConfiguringConfigurations = new List<OptionsBuilderConfiguration>();
                AddConnectionStringConfiguration(onConfiguringConfigurations);
                return onConfiguringConfigurations;
            }
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
                    AddRelationshipConfiguration(entityConfiguration);

                    if (entityConfiguration.FacetConfigurations.Any()
                        || entityConfiguration.PropertyConfigurations.Any()
                        || entityConfiguration.RelationshipConfigurations.Any())
                    {
                        entityConfigurations.Add(entityConfiguration);
                    }
                }

                return entityConfigurations;
            }
        }

        public virtual void AddConnectionStringConfiguration(
            [NotNull] List<OptionsBuilderConfiguration> optionsBuilderConfigurations)
        {
            Check.NotNull(optionsBuilderConfigurations, nameof(optionsBuilderConfigurations));

            optionsBuilderConfigurations.Add(
                new OptionsBuilderConfiguration(
                    UseMethodName + "(" + VerbatimStringLiteral(GeneratorModel.ConnectionString)+ ")"));
        }

        public virtual void AddEntityFacetsConfiguration([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            AddEntityKeyConfiguration(entityConfiguration);
            AddTableNameFacetConfiguration(entityConfiguration);
        }

        public virtual void AddRelationshipConfiguration([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            foreach (var foreignKey in entityConfiguration.EntityType.GetForeignKeys())
            {
                var dependentEndNavigationPropertyName =
                    (string)foreignKey[ReverseEngineeringMetadataModelProvider.AnnotationNameDependentEndNavPropName];
                var principalEndNavigationPropertyName =
                    (string)foreignKey[ReverseEngineeringMetadataModelProvider.AnnotationNamePrincipalEndNavPropName];

                entityConfiguration.RelationshipConfigurations.Add(
                    new RelationshipConfiguration(entityConfiguration, foreignKey,
                        dependentEndNavigationPropertyName, principalEndNavigationPropertyName));
            }
        }

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
            if (ExtensionsProvider.For(entityType).Schema != null
                && ExtensionsProvider.For(entityType).Schema != "dbo")
            {
                entityConfiguration.FacetConfigurations.Add(
                    new FacetConfiguration(
                        string.Format(CultureInfo.InvariantCulture, "ToTable({0}, {1})",
                            CSharpUtilities.Instance.DelimitString(ExtensionsProvider.For(entityType).TableName),
                            CSharpUtilities.Instance.DelimitString(ExtensionsProvider.For(entityType).Schema))));
            }
            else if (ExtensionsProvider.For(entityType).TableName != null
                     && ExtensionsProvider.For(entityType).TableName != entityType.DisplayName())
            {
                entityConfiguration.FacetConfigurations.Add(
                    new FacetConfiguration(
                        string.Format(CultureInfo.InvariantCulture, "ToTable({0})",
                            CSharpUtilities.Instance.DelimitString(ExtensionsProvider.For(entityType).TableName))));
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
            AddColumnNameFacetConfiguration(propertyConfiguration);
            AddColumnTypeFacetConfiguration(propertyConfiguration);
            AddDefaultValueFacetConfiguration(propertyConfiguration);
            AddDefaultExpressionFacetConfiguration(propertyConfiguration);
            AddValueGeneratedFacetConfiguration(propertyConfiguration);
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

        public virtual void AddValueGeneratedFacetConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            var valueGenerated = propertyConfiguration.Property.ValueGenerated;
            switch (valueGenerated)
            {
                case ValueGenerated.OnAdd:
                    // If this property is the single integer primary key on the EntityType then
                    // KeyConvention assumes ValueGeneratedOnAdd() so there is no need to add it.
                    if (_keyConvention.ValueGeneratedOnAddProperty(
                        new List<Property> { (Property)propertyConfiguration.Property },
                        (EntityType)propertyConfiguration.EntityConfiguration.EntityType) == null)
                    {
                        propertyConfiguration.AddFacetConfiguration(
                            new FacetConfiguration("ValueGeneratedOnAdd()"));
                    }

                    break;

                case ValueGenerated.OnAddOrUpdate:
                    propertyConfiguration.AddFacetConfiguration(new FacetConfiguration("ValueGeneratedOnAddOrUpdate()"));
                    break;
            }
        }

        public virtual void AddColumnNameFacetConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (ExtensionsProvider.For(propertyConfiguration.Property).ColumnName != null
                && ExtensionsProvider.For(propertyConfiguration.Property).ColumnName != propertyConfiguration.Property.Name)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        string.Format(CultureInfo.InvariantCulture,
                            "HasColumnName({0})",
                            CSharpUtilities.Instance.DelimitString(
                                ExtensionsProvider.For(propertyConfiguration.Property).ColumnName))));
            }
        }

        public virtual void AddColumnTypeFacetConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (ExtensionsProvider.For(propertyConfiguration.Property).ColumnType != null)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        string.Format(CultureInfo.InvariantCulture,
                            "HasColumnType({0})",
                            CSharpUtilities.Instance.DelimitString(
                                ExtensionsProvider.For(propertyConfiguration.Property).ColumnType))));
            }
        }

        public virtual void AddDefaultValueFacetConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (ExtensionsProvider.For(propertyConfiguration.Property).DefaultValue != null)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        string.Format(CultureInfo.InvariantCulture,
                            "HasDefaultValue({0})",
                            CSharpUtilities.Instance.GenerateLiteral(
                                (dynamic)ExtensionsProvider.For(propertyConfiguration.Property).DefaultValue))));
            }
        }

        public virtual void AddDefaultExpressionFacetConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (ExtensionsProvider.For(propertyConfiguration.Property).GeneratedValueSql != null)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        string.Format(CultureInfo.InvariantCulture,
                            "HasDefaultValueSql({0})",
                            CSharpUtilities.Instance.DelimitString(
                                ExtensionsProvider.For(propertyConfiguration.Property).GeneratedValueSql))));
            }
        }
    }
}
