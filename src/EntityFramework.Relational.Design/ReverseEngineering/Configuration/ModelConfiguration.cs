// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Metadata;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration
{
    public class ModelConfiguration
    {
        protected const string DbContextSuffix = "Context";
        protected const string DefaultDbContextName = "Model" + DbContextSuffix;
        protected static readonly KeyDiscoveryConvention _keyDiscoveryConvention = new KeyDiscoveryConvention();
        protected static readonly KeyConvention _keyConvention = new KeyConvention();

        protected readonly ConfigurationFactory _configurationFactory;
        protected List<OptionsBuilderConfiguration> _onConfiguringConfigurations;
        protected SortedDictionary<EntityType, EntityConfiguration> _entityConfigurationMap;
        private readonly IMethodNameProvider _methodNameProvider;

        public ModelConfiguration(
            [NotNull] ConfigurationFactory configurationFactory, 
            [NotNull] IModel model, 
            [NotNull] CustomConfiguration customConfiguration, 
            [NotNull] IMethodNameProvider methodNameProvider, 
            [NotNull] IRelationalAnnotationProvider extensionsProvider, 
            [NotNull] CSharpUtilities cSharpUtilities, 
            [NotNull] ModelUtilities modelUtilities)
        {
            Check.NotNull(configurationFactory, nameof(configurationFactory));
            Check.NotNull(model, nameof(model));
            Check.NotNull(methodNameProvider, nameof(methodNameProvider));
            Check.NotNull(customConfiguration, nameof(customConfiguration));
            Check.NotNull(extensionsProvider, nameof(extensionsProvider));
            Check.NotNull(cSharpUtilities, nameof(modelUtilities));
            Check.NotNull(modelUtilities, nameof(cSharpUtilities));

            _configurationFactory = configurationFactory;
            Model = model;
            _methodNameProvider = methodNameProvider;
            CustomConfiguration = customConfiguration;
            ExtensionsProvider = extensionsProvider;
            CSharpUtilities = cSharpUtilities;
            ModelUtilities = modelUtilities;
        }

        public virtual IModel Model { get;[param: NotNull] private set; }
        public virtual IRelationalAnnotationProvider ExtensionsProvider { get; private set; }
        public virtual CSharpUtilities CSharpUtilities { get;[param: NotNull] private set; }
        public virtual ModelUtilities ModelUtilities { get;[param: NotNull] private set; }
        public virtual CustomConfiguration CustomConfiguration { get;[param: NotNull] set; }
        public virtual string ClassName()
        {
            var annotatedName = ExtensionsProvider.For(Model).DatabaseName;
            if (!string.IsNullOrEmpty(annotatedName))
            {
                return CSharpUtilities.GenerateCSharpIdentifier(annotatedName + DbContextSuffix, null);
            }

            return DefaultDbContextName;
        }

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
                var entityConfiguration =
                    _configurationFactory.CreateEntityConfiguration(this, entityType);
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
                _configurationFactory.CreateOptionsBuilderConfiguration(
                    _methodNameProvider.DbOptionBuilderExtension
                    + "("
                    + CSharpUtilities.GenerateVerbatimStringLiteral(CustomConfiguration.ConnectionString)
                    + ")"));
        }

        public virtual void AddEntityPropertiesConfiguration([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            foreach (var property in ModelUtilities.OrderedProperties(entityConfiguration.EntityType))
            {
                var propertyConfiguration =
                    _configurationFactory.CreatePropertyConfiguration(entityConfiguration, property);
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
            foreach(var key in entityType.GetKeys())
            {
                if (key == null
                    || key.Properties.Count == 0)
                {
                    continue;
                }

                var conventionKeyProperties =
                    _keyDiscoveryConvention.DiscoverKeyProperties(entityType, entityType.Properties.ToList());
                if (conventionKeyProperties != null
                    && key.Properties.OrderBy(p => p.Name).SequenceEqual(conventionKeyProperties.OrderBy(p => p.Name)))
                {
                    continue;
                }

                var keyFluentApi = _configurationFactory
                    .CreateKeyFluentApiConfiguration("e", key.Properties);

                if(key.IsPrimaryKey())
                {
                    keyFluentApi.IsPrimaryKey = true;

                    if (key.Properties.Count == 1)
                    {
                        keyFluentApi.HasAttributeEquivalent = true;

                        var propertyConfiguration =
                            entityConfiguration.GetOrAddPropertyConfiguration(
                                entityConfiguration, key.Properties.First());
                        propertyConfiguration.AttributeConfigurations.Add(
                            _configurationFactory.CreateAttributeConfiguration(nameof(KeyAttribute)));
                    }
                }

                entityConfiguration.FluentApiConfigurations.Add(keyFluentApi);
            }
        }

        public virtual void AddTableNameConfiguration([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            var entityType = entityConfiguration.EntityType;
            if (ExtensionsProvider.For(entityType).Schema != null
                && ExtensionsProvider.For(entityType).Schema != ExtensionsProvider.For(Model).DefaultSchema)
            {
                var delimitedTableName =
                    CSharpUtilities.DelimitString(ExtensionsProvider.For(entityType).TableName);
                var delimitedSchemaName =
                    CSharpUtilities.DelimitString(ExtensionsProvider.For(entityType).Schema);
                entityConfiguration.FluentApiConfigurations.Add(
                    _configurationFactory.CreateFluentApiConfiguration(
                         /* hasAttributeEquivalent */ true,
                        nameof(RelationalEntityTypeBuilderExtensions.ToTable),
                        delimitedTableName,
                        delimitedSchemaName));
                entityConfiguration.AttributeConfigurations.Add(
                    _configurationFactory.CreateAttributeConfiguration(
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
                    _configurationFactory.CreateFluentApiConfiguration(
                         /* hasAttributeEquivalent */ true,
                        nameof(RelationalEntityTypeBuilderExtensions.ToTable),
                        delimitedTableName));
                entityConfiguration.AttributeConfigurations.Add(
                    _configurationFactory.CreateAttributeConfiguration(
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
                        _configurationFactory.CreateFluentApiConfiguration(
                            /* hasAttributeEquivalent */ true,
                            nameof(PropertyBuilder.IsRequired)));
                    propertyConfiguration.AttributeConfigurations.Add(
                        _configurationFactory.CreateAttributeConfiguration(nameof(RequiredAttribute)));
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
                    _configurationFactory.CreateFluentApiConfiguration(
                         /* hasAttributeEquivalent */ true,
                        nameof(PropertyBuilder.HasMaxLength), maxLengthLiteral));
                propertyConfiguration.AttributeConfigurations.Add(
                    _configurationFactory.CreateAttributeConfiguration(nameof(MaxLengthAttribute), maxLengthLiteral));
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
                        (EntityType)propertyConfiguration.EntityConfiguration.EntityType) == null
                        && ExtensionsProvider.For(propertyConfiguration.Property).GeneratedValueSql == null)
                    {
                        propertyConfiguration.FluentApiConfigurations.Add(
                            _configurationFactory.CreateFluentApiConfiguration(
                                /* hasAttributeEquivalent */ false,
                                nameof(PropertyBuilder.ValueGeneratedOnAdd)));
                    }

                    break;

                case ValueGenerated.OnAddOrUpdate:
                    propertyConfiguration.FluentApiConfigurations.Add(
                        _configurationFactory.CreateFluentApiConfiguration(
                            /* hasAttributeEquivalent */ false,
                            nameof(PropertyBuilder.ValueGeneratedOnAddOrUpdate)));
                    break;

                case ValueGenerated.Never:
                    // used to prevent conventions from altering the value generated value
                    if (propertyConfiguration.Property.RelationalDesign().ExplicitValueGeneratedNever == true)
                    {
                        propertyConfiguration.FluentApiConfigurations.Add(
                            _configurationFactory.CreateFluentApiConfiguration(
                                /* hasAttributeEquivalent */ false,
                                nameof(PropertyBuilder.ValueGeneratedNever)));
                    }
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
                    _configurationFactory.CreateFluentApiConfiguration(
                        /* hasAttributeEquivalent */ true,
                        nameof(RelationalPropertyBuilderExtensions.HasColumnName),
                        delimitedColumnName));

                if (delimitedColumnTypeName == null)
                {
                    propertyConfiguration.AttributeConfigurations.Add(
                        _configurationFactory.CreateAttributeConfiguration(nameof(ColumnAttribute), delimitedColumnName));
                }
                else
                {
                    propertyConfiguration.FluentApiConfigurations.Add(
                        _configurationFactory.CreateFluentApiConfiguration(
                            /* hasAttributeEquivalent */ true,
                            nameof(RelationalPropertyBuilderExtensions.HasColumnType),
                            delimitedColumnTypeName));
                    propertyConfiguration.AttributeConfigurations.Add(
                        _configurationFactory.CreateAttributeConfiguration(
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
                    _configurationFactory.CreateFluentApiConfiguration(
                        /* hasAttributeEquivalent */ false,
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
                    _configurationFactory.CreateFluentApiConfiguration(
                        /* hasAttributeEquivalent */ false,
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
                    _configurationFactory.CreateFluentApiConfiguration(
                        /* hasAttributeEquivalent */ false,
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
                    var referencedType = foreignKey.IsUnique
                        ? otherEntityType.Name
                        : "ICollection<" + otherEntityType.Name + ">";
                    var navPropConfiguration =
                        _configurationFactory.CreateNavigationPropertyConfiguration(
                            referencedType,
                            foreignKey.RelationalDesign().PrincipalEndNavPropName);
                    if (otherEntityType.RelationalDesign().EntityTypeError != null)
                    {
                        navPropConfiguration.ErrorAnnotation =
                            RelationalDesignStrings.UnableToAddNavigationProperty(otherEntityType.Name);
                    }
                    else
                    {
                        if (foreignKey.PrincipalKey.IsPrimaryKey())
                        {
                            navPropConfiguration.AttributeConfigurations.Add(
                            _configurationFactory.CreateAttributeConfiguration(
                                nameof(InversePropertyAttribute),
                                CSharpUtilities.DelimitString(
                                    foreignKey.RelationalDesign().DependentEndNavPropName)));
                        }
                    }

                    entityConfiguration.NavigationPropertyConfigurations.Add(navPropConfiguration);
                }
            }

            foreach (var foreignKey in entityConfiguration.EntityType.GetForeignKeys())
            {
                // set up the navigation property on this end of foreign keys owned by this EntityType
                // (i.e. this EntityType is the dependent)
                var dependentEndNavPropConfiguration =
                    _configurationFactory.CreateNavigationPropertyConfiguration(
                        foreignKey.PrincipalEntityType.Name,
                        foreignKey.RelationalDesign().DependentEndNavPropName);

                if(foreignKey.PrincipalKey.IsPrimaryKey())
                {
                    dependentEndNavPropConfiguration.AttributeConfigurations.Add(
                        _configurationFactory.CreateAttributeConfiguration(
                            nameof(ForeignKeyAttribute),
                            CSharpUtilities.DelimitString(
                                string.Join(",", foreignKey.Properties.Select(p => p.Name)))));
                    dependentEndNavPropConfiguration.AttributeConfigurations.Add(
                        _configurationFactory.CreateAttributeConfiguration(
                            nameof(InversePropertyAttribute),
                            CSharpUtilities.DelimitString(
                                foreignKey.RelationalDesign().PrincipalEndNavPropName)));
                }

                entityConfiguration.NavigationPropertyConfigurations.Add(
                    dependentEndNavPropConfiguration);

                // set up the other navigation property for self-referencing foreign keys owned by this EntityType
                if (((ForeignKey)foreignKey).IsSelfReferencing())
                {
                    var referencedType = foreignKey.IsUnique
                        ? foreignKey.DeclaringEntityType.Name
                        : "ICollection<" + foreignKey.DeclaringEntityType.Name + ">";
                    var principalEndNavPropConfiguration =
                        _configurationFactory.CreateNavigationPropertyConfiguration(
                            referencedType,
                            foreignKey.RelationalDesign().PrincipalEndNavPropName);
                    principalEndNavPropConfiguration.AttributeConfigurations.Add(
                        _configurationFactory.CreateAttributeConfiguration(
                            nameof(InversePropertyAttribute),
                            CSharpUtilities.DelimitString(
                                foreignKey.RelationalDesign().DependentEndNavPropName)));
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
                        foreignKey.RelationalDesign().PrincipalEndNavPropName;
                    if (otherEntityType.RelationalDesign().EntityTypeError == null)
                    {
                        if (!foreignKey.IsUnique)
                        {
                            entityConfiguration.NavigationPropertyInitializerConfigurations.Add(
                                _configurationFactory.CreateNavigationPropertyInitializerConfiguration(
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
                    foreignKey.RelationalDesign().DependentEndNavPropName;
                var principalEndNavigationPropertyName =
                    foreignKey.RelationalDesign().PrincipalEndNavPropName;

                var relationshipConfiguration = _configurationFactory
                    .CreateRelationshipConfiguration(
                        entityConfiguration,
                        foreignKey,
                        dependentEndNavigationPropertyName,
                        principalEndNavigationPropertyName,
                        foreignKey.DeleteBehavior);
                relationshipConfiguration.HasAttributeEquivalent = foreignKey.PrincipalKey.IsPrimaryKey();
                entityConfiguration.RelationshipConfigurations.Add(relationshipConfiguration);
            }
        }

        // default ordering is by Name, which is what we want here but
        // do not configure EntityTypes for which we had an error when generating
        public virtual IEnumerable<EntityConfiguration> OrderedEntityConfigurations() =>
            EntityConfigurations
                .Where(ec => ec.EntityType.RelationalDesign().EntityTypeError == null);

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
