// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration
{
    public abstract class ModelConfiguration
    {
        protected const string DbContextSuffix = "Context";
        protected const string DefaultDbContextName = "Model" + DbContextSuffix;
        protected static readonly KeyDiscoveryConvention _keyDiscoveryConvention = new KeyDiscoveryConvention();
        protected static readonly KeyConvention _keyConvention = new KeyConvention();

        protected List<OptionsBuilderConfiguration> _onConfiguringConfigurations;
        protected SortedDictionary<EntityType, EntityConfiguration> _entityConfigurationMap;

        public ModelConfiguration(
            [NotNull] IModel model,
            [NotNull] CustomConfiguration customConfiguration,
            [NotNull] IRelationalMetadataExtensionProvider extensionsProvider,
            [NotNull] CSharpUtilities cSharpUtilities,
            [NotNull] ModelUtilities modelUtilities)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(customConfiguration, nameof(customConfiguration));
            Check.NotNull(extensionsProvider, nameof(extensionsProvider));
            Check.NotNull(cSharpUtilities, nameof(modelUtilities));
            Check.NotNull(modelUtilities, nameof(cSharpUtilities));

            Model = model;
            CustomConfiguration = customConfiguration;
            ExtensionsProvider = extensionsProvider;
            CSharpUtilities = cSharpUtilities;
            ModelUtilities = modelUtilities;
        }

        public virtual IModel Model { get;[param: NotNull] private set; }
        public virtual IRelationalMetadataExtensionProvider ExtensionsProvider { get; private set; }
        public virtual CSharpUtilities CSharpUtilities { get;[param: NotNull] private set; }
        public virtual ModelUtilities ModelUtilities { get;[param: NotNull] private set; }
        public virtual CustomConfiguration CustomConfiguration { get;[param: NotNull] set; }

        public abstract string UseMethodName { get; } // "UseSqlServer" for SqlServer, "UseSqlite" for Sqlite etc
        public virtual string DefaultSchemaName { get; } // e.g. "dbo for SqlServer. Leave null if there is no concept of a default schema.
        public virtual string ClassName() => DefaultDbContextName;
        public virtual string Namespace() => CustomConfiguration.Namespace;

        public virtual List<OptionsBuilderConfiguration> OnConfiguringConfigurations
        {
            get
            {
                if (_onConfiguringConfigurations == null)
                {
                    _onConfiguringConfigurations = new List<OptionsBuilderConfiguration>();
                    AddConnectionStringConfiguration();
                }

                return _onConfiguringConfigurations;
            }
        }

        public virtual List<EntityConfiguration> EntityConfigurations
        {
            get
            {
                if (_entityConfigurationMap == null)
                {
                    _entityConfigurationMap = new 
                        SortedDictionary<EntityType, EntityConfiguration>(new EntityTypeNameComparer());
                    AddEntityConfigurations();
                }

                return _entityConfigurationMap.Values.ToList();
            }
        }

        public virtual void AddEntityConfigurations()
        {
            var entityConfigurations = new List<EntityConfiguration>();

            foreach (var entityType in Model.EntityTypes)
            {
                var entityConfiguration = new EntityConfiguration(this, entityType);
                if (entityConfiguration.ErrorMessageAnnotation == null)
                {
                    AddEntityPropertiesConfiguration(entityConfiguration);
                    AddEntityConfiguration(entityConfiguration);
                    AddNavigationProperties(entityConfiguration);
                    AddNavigationPropertyInitializers(entityConfiguration);
                    AddRelationshipConfiguration(entityConfiguration);
                }

                _entityConfigurationMap.Add((EntityType)entityType, entityConfiguration);
            }
        }

        public virtual void AddConnectionStringConfiguration()
        {
            _onConfiguringConfigurations.Add(
                new OptionsBuilderConfiguration(
                    UseMethodName
                    + "("
                    + CSharpUtilities.GenerateVerbatimStringLiteral(CustomConfiguration.ConnectionString)
                    + ")"));
        }

        public virtual void AddEntityPropertiesConfiguration([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            foreach (var property in ModelUtilities.OrderedProperties(entityConfiguration.EntityType))
            {
                var propertyConfiguration = new PropertyConfiguration(entityConfiguration, property);
                AddPropertyConfiguration(propertyConfiguration);
                entityConfiguration.PropertyConfigurations.Add(propertyConfiguration);
            }
        }

        public virtual void AddEntityConfiguration([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            AddKeyConfiguration(entityConfiguration);
            AddTableNameConfiguration(entityConfiguration);
        }

        public virtual void AddKeyConfiguration([NotNull] EntityConfiguration entityConfiguration)
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

            var keyFluentApi = new KeyFluentApiConfiguration("e", key.Properties);
            if (key.Properties.Count == 1)
            {
                keyFluentApi.HasAttributeEquivalent = true;

                var propertyConfiguration =
                    entityConfiguration.GetOrAddPropertyConfiguration(
                        entityConfiguration, key.Properties.First());
                propertyConfiguration.AttributeConfigurations.Add(
                    new AttributeConfiguration(nameof(KeyAttribute)));
            }
            entityConfiguration.FluentApiConfigurations.Add(keyFluentApi);
        }

        public virtual void AddTableNameConfiguration([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            var entityType = entityConfiguration.EntityType;
            if (ExtensionsProvider.For(entityType).Schema != null
                && ExtensionsProvider.For(entityType).Schema != DefaultSchemaName)
            {
                var delimitedTableName =
                    CSharpUtilities.DelimitString(ExtensionsProvider.For(entityType).TableName);
                var delimitedSchemaName =
                    CSharpUtilities.DelimitString(ExtensionsProvider.For(entityType).Schema);
                entityConfiguration.FluentApiConfigurations.Add(
                    new FluentApiConfiguration(
                        nameof(RelationalEntityTypeBuilderExtensions.ToTable),
                        delimitedTableName,
                        delimitedSchemaName)
                    {
                        HasAttributeEquivalent = true
                    });
                entityConfiguration.AttributeConfigurations.Add(
                    new AttributeConfiguration(
                        nameof(TableAttribute),
                        delimitedTableName,
                        nameof(TableAttribute.Schema) + " = " + delimitedSchemaName));
            }
            else if (ExtensionsProvider.For(entityType).TableName != null
                     && ExtensionsProvider.For(entityType).TableName != entityType.DisplayName())
            {
                var delimitedTableName =
                    CSharpUtilities.DelimitString(ExtensionsProvider.For(entityType).TableName);
                entityConfiguration.FluentApiConfigurations.Add(
                    new FluentApiConfiguration(
                        nameof(RelationalEntityTypeBuilderExtensions.ToTable),
                        delimitedTableName)
                    {
                        HasAttributeEquivalent = true
                    });
                entityConfiguration.AttributeConfigurations.Add(
                    new AttributeConfiguration(
                        nameof(TableAttribute), delimitedTableName));
            }
        }

        public virtual void AddPropertyConfiguration([NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            AddRequiredConfiguration(propertyConfiguration);
            AddMaxLengthConfiguration(propertyConfiguration);
            AddColumnNameAndTypeConfiguration(propertyConfiguration);
            AddDefaultValueConfiguration(propertyConfiguration);
            AddDefaultExpressionConfiguration(propertyConfiguration);
            AddValueGeneratedConfiguration(propertyConfiguration);
        }

        public virtual void AddRequiredConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (!propertyConfiguration.Property.IsNullable
                && propertyConfiguration.Property.ClrType.IsNullableType())
            {
                var entityKeyProperties =
                    ((EntityType)propertyConfiguration.EntityConfiguration.EntityType)
                        .FindPrimaryKey()?.Properties
                    ?? Enumerable.Empty<Property>();
                if (!entityKeyProperties.Contains(propertyConfiguration.Property))
                {
                    propertyConfiguration.FluentApiConfigurations.Add(
                        new FluentApiConfiguration(nameof(PropertyBuilder.Required))
                        {
                            HasAttributeEquivalent = true
                        });
                    propertyConfiguration.AttributeConfigurations.Add(
                        new AttributeConfiguration(nameof(RequiredAttribute)));
                }
            }
        }

        public virtual void AddMaxLengthConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (((Property)propertyConfiguration.Property).GetMaxLength().HasValue)
            {
                var maxLengthLiteral =
                    CSharpUtilities.GenerateLiteral(
                        ((Property)propertyConfiguration.Property).GetMaxLength().Value);
                propertyConfiguration.FluentApiConfigurations.Add(
                    new FluentApiConfiguration(nameof(PropertyBuilder.MaxLength), maxLengthLiteral)
                    {
                        HasAttributeEquivalent = true
                    });
                propertyConfiguration.AttributeConfigurations.Add(
                    new AttributeConfiguration(nameof(MaxLengthAttribute), maxLengthLiteral));
            }
        }

        public virtual void AddValueGeneratedConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            var valueGenerated = propertyConfiguration.Property.ValueGenerated;
            switch (valueGenerated)
            {
                case ValueGenerated.OnAdd:
                    // If this property is the single integer primary key on the EntityType then
                    // KeyConvention assumes ValueGeneratedOnAdd() so there is no need to add it.
                    if (_keyConvention.FindValueGeneratedOnAddProperty(
                        new List<Property> { (Property)propertyConfiguration.Property },
                        (EntityType)propertyConfiguration.EntityConfiguration.EntityType) == null)
                    {
                        propertyConfiguration.FluentApiConfigurations.Add(
                            new FluentApiConfiguration(nameof(PropertyBuilder.ValueGeneratedOnAdd)));
                    }

                    break;

                case ValueGenerated.OnAddOrUpdate:
                    propertyConfiguration.FluentApiConfigurations.Add(
                        new FluentApiConfiguration(nameof(PropertyBuilder.ValueGeneratedOnAddOrUpdate)));
                    break;
            }
        }

        public virtual void AddColumnNameAndTypeConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));


            var delimitedColumnName = 
                ExtensionsProvider.For(propertyConfiguration.Property).ColumnName != null
                && ExtensionsProvider.For(propertyConfiguration.Property).ColumnName != propertyConfiguration.Property.Name
                ? CSharpUtilities.DelimitString(
                    ExtensionsProvider.For(propertyConfiguration.Property).ColumnName)
                : null;

            var delimitedColumnTypeName = 
                ExtensionsProvider.For(propertyConfiguration.Property).ColumnType != null
                ? CSharpUtilities.DelimitString(
                        ExtensionsProvider.For(propertyConfiguration.Property).ColumnType)
                : null;

            if (delimitedColumnName != null)
            {
                propertyConfiguration.FluentApiConfigurations.Add(
                    new FluentApiConfiguration(
                        nameof(RelationalPropertyBuilderExtensions.HasColumnName),
                        delimitedColumnName)
                    {
                        HasAttributeEquivalent = true
                    });

                if (delimitedColumnTypeName == null)
                {
                    propertyConfiguration.AttributeConfigurations.Add(
                        new AttributeConfiguration(nameof(ColumnAttribute), delimitedColumnName));
                }
                else
                {
                    propertyConfiguration.FluentApiConfigurations.Add(
                        new FluentApiConfiguration(
                            nameof(RelationalPropertyBuilderExtensions.HasColumnType),
                            delimitedColumnTypeName)
                        {
                            HasAttributeEquivalent = true
                        });
                    propertyConfiguration.AttributeConfigurations.Add(
                        new AttributeConfiguration(
                            nameof(ColumnAttribute),
                            new[] {
                                delimitedColumnName,
                                nameof(ColumnAttribute.TypeName) + " = " + delimitedColumnTypeName
                            }));
                }
            }
            else if (delimitedColumnTypeName != null)
            {
                propertyConfiguration.FluentApiConfigurations.Add(
                    new FluentApiConfiguration(
                        nameof(RelationalPropertyBuilderExtensions.HasColumnType),
                        delimitedColumnTypeName));
            }
        }

        public virtual void AddDefaultValueConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (ExtensionsProvider.For(propertyConfiguration.Property).DefaultValue != null)
            {
                propertyConfiguration.FluentApiConfigurations.Add(
                    new FluentApiConfiguration(
                        nameof(RelationalPropertyBuilderExtensions.HasDefaultValue),
                        CSharpUtilities.GenerateLiteral(
                            (dynamic)ExtensionsProvider.For(propertyConfiguration.Property).DefaultValue)));
            }
        }

        public virtual void AddDefaultExpressionConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (ExtensionsProvider.For(propertyConfiguration.Property).GeneratedValueSql != null)
            {
                propertyConfiguration.FluentApiConfigurations.Add(
                    new FluentApiConfiguration(
                        nameof(RelationalPropertyBuilderExtensions.HasDefaultValueSql),
                        CSharpUtilities.DelimitString(
                            ExtensionsProvider.For(propertyConfiguration.Property).GeneratedValueSql)));
            }
        }

        public virtual void AddNavigationProperties([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            foreach (var otherEntityType in entityConfiguration.EntityType
                .Model.EntityTypes.Where(et => et != entityConfiguration.EntityType))
            {
                // set up the navigation properties for foreign keys from another EntityType
                // which reference this EntityType (i.e. this EntityType is the principal)
                foreach (var foreignKey in otherEntityType
                    .GetForeignKeys().Where(fk => fk.PrincipalEntityType == entityConfiguration.EntityType))
                {
                    if (((EntityType)otherEntityType)
                        .FindAnnotation(RelationalMetadataModelProvider.AnnotationNameEntityTypeError) != null)
                    {
                        entityConfiguration.NavigationPropertyConfigurations.Add(
                            new NavigationPropertyConfiguration(
                                Strings.UnableToAddNavigationProperty(otherEntityType.Name)));
                    }
                    else
                    {
                        var referencedType = foreignKey.IsUnique
                            ? otherEntityType.Name
                            : "ICollection<" + otherEntityType.Name + ">";
                        var navPropConfiguration =
                            new NavigationPropertyConfiguration(
                                referencedType,
                                (string)foreignKey[RelationalMetadataModelProvider.AnnotationNamePrincipalEndNavPropName]);
                        navPropConfiguration.AttributeConfigurations.Add(
                            new AttributeConfiguration(
                                nameof(InversePropertyAttribute),
                                CSharpUtilities.DelimitString(
                                    (string)foreignKey[RelationalMetadataModelProvider.AnnotationNameDependentEndNavPropName])));
                        entityConfiguration.NavigationPropertyConfigurations.Add(
                            navPropConfiguration);
                    }
                }
            }

            foreach (var foreignKey in entityConfiguration.EntityType.GetForeignKeys())
            {
                // set up the navigation property on this end of foreign keys owned by this EntityType
                // (i.e. this EntityType is the dependent)
                var dependentEndNavPropConfiguration =
                    new NavigationPropertyConfiguration(
                        foreignKey.PrincipalEntityType.Name,
                        (string)foreignKey[RelationalMetadataModelProvider.AnnotationNameDependentEndNavPropName]);
                dependentEndNavPropConfiguration.AttributeConfigurations.Add(
                    new AttributeConfiguration(
                        nameof(ForeignKeyAttribute),
                        CSharpUtilities.DelimitString(
                            string.Join(",", foreignKey.Properties.Select(p => p.Name)))));
                dependentEndNavPropConfiguration.AttributeConfigurations.Add(
                    new AttributeConfiguration(
                        nameof(InversePropertyAttribute),
                        CSharpUtilities.DelimitString(
                            (string)foreignKey[RelationalMetadataModelProvider.AnnotationNamePrincipalEndNavPropName])));
                entityConfiguration.NavigationPropertyConfigurations.Add(
                    dependentEndNavPropConfiguration);

                // set up the other navigation property for self-referencing foreign keys owned by this EntityType
                if (((ForeignKey)foreignKey).IsSelfReferencing())
                {
                    var referencedType = foreignKey.IsUnique
                        ? foreignKey.DeclaringEntityType.Name
                        : "ICollection<" + foreignKey.DeclaringEntityType.Name + ">";
                    var principalEndNavPropConfiguration = new NavigationPropertyConfiguration(
                            referencedType,
                            (string)foreignKey[RelationalMetadataModelProvider.AnnotationNamePrincipalEndNavPropName]);
                    principalEndNavPropConfiguration.AttributeConfigurations.Add(
                        new AttributeConfiguration(
                            nameof(InversePropertyAttribute),
                            CSharpUtilities.DelimitString(
                                (string)foreignKey[RelationalMetadataModelProvider.AnnotationNameDependentEndNavPropName])));
                    entityConfiguration.NavigationPropertyConfigurations.Add(
                        principalEndNavPropConfiguration);
                }
            }
        }

        public virtual void AddNavigationPropertyConfiguration(
            [NotNull] NavigationPropertyConfiguration navigationPropertyConfiguration)
        {
        }

        public virtual void AddNavigationPropertyInitializers([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            foreach (var otherEntityType in entityConfiguration.EntityType.Model
                .EntityTypes.Where(et => et != entityConfiguration.EntityType))
            {
                // find navigation properties for foreign keys from another EntityType which reference this EntityType
                foreach (var foreignKey in otherEntityType
                    .GetForeignKeys().Where(fk => fk.PrincipalEntityType == entityConfiguration.EntityType))
                {
                    var navigationPropertyName =
                        (string)foreignKey[RelationalMetadataModelProvider.AnnotationNamePrincipalEndNavPropName];
                    if (((EntityType)otherEntityType)
                        .FindAnnotation(RelationalMetadataModelProvider.AnnotationNameEntityTypeError) == null)
                    {
                        if (!foreignKey.IsUnique)
                        {
                            entityConfiguration.NavigationPropertyInitializerConfigurations.Add(
                                new NavigationPropertyInitializerConfiguration(
                                    navigationPropertyName, otherEntityType.Name));
                        }
                    }
                }
            }
        }

        public virtual void AddRelationshipConfiguration([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            foreach (var foreignKey in entityConfiguration.EntityType.GetForeignKeys())
            {
                var dependentEndNavigationPropertyName =
                    (string)foreignKey[RelationalMetadataModelProvider.AnnotationNameDependentEndNavPropName];
                var principalEndNavigationPropertyName =
                    (string)foreignKey[RelationalMetadataModelProvider.AnnotationNamePrincipalEndNavPropName];

                entityConfiguration.RelationshipConfigurations.Add(
                    new RelationshipConfiguration(entityConfiguration, foreignKey,
                        dependentEndNavigationPropertyName, principalEndNavigationPropertyName));
            }
        }

        // default ordering is by Name, which is what we want here but
        // do not configure EntityTypes for which we had an error when generating
        public virtual IEnumerable<EntityConfiguration> OrderedEntityConfigurations() =>
            EntityConfigurations
                .Where(ec => ((EntityType)ec.EntityType).FindAnnotation(
                    RelationalMetadataModelProvider.AnnotationNameEntityTypeError) == null);

        public virtual EntityConfiguration GetEntityConfiguration([NotNull] EntityType entityType)
        {
            if (_entityConfigurationMap == null)
            {
                var _ = EntityConfigurations;
            }

            return _entityConfigurationMap[entityType];
        }
    }
}
