// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Configuration.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ModelConfiguration
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected const string DbContextSuffix = "Context";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected const string DefaultDbContextName = "Model" + DbContextSuffix;

        private static readonly KeyDiscoveryConvention _keyDiscoveryConvention = new KeyDiscoveryConvention();
        private static readonly KeyConvention _keyConvention = new KeyConvention();

        private readonly ConfigurationFactory _configurationFactory;
        private List<OptionsBuilderConfiguration> _onConfiguringConfigurations;
        private SortedDictionary<EntityType, EntityConfiguration> _entityConfigurationMap;
        private List<SequenceConfiguration> _sequenceConfigurations;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ModelConfiguration(
            [NotNull] ConfigurationFactory configurationFactory,
            [NotNull] IModel model,
            [NotNull] CustomConfiguration customConfiguration,
            [NotNull] IRelationalAnnotationProvider annotationProvider,
            [NotNull] CSharpUtilities cSharpUtilities,
            [NotNull] ScaffoldingUtilities scaffoldingUtilities)
        {
            Check.NotNull(configurationFactory, nameof(configurationFactory));
            Check.NotNull(model, nameof(model));
            Check.NotNull(customConfiguration, nameof(customConfiguration));
            Check.NotNull(annotationProvider, nameof(annotationProvider));
            Check.NotNull(cSharpUtilities, nameof(cSharpUtilities));
            Check.NotNull(scaffoldingUtilities, nameof(scaffoldingUtilities));

            _configurationFactory = configurationFactory;
            Model = model;
            CustomConfiguration = customConfiguration;
            AnnotationProvider = annotationProvider;
            CSharpUtilities = cSharpUtilities;
            ScaffoldingUtilities = scaffoldingUtilities;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IModel Model { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IRelationalAnnotationProvider AnnotationProvider { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual CSharpUtilities CSharpUtilities { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ScaffoldingUtilities ScaffoldingUtilities { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual CustomConfiguration CustomConfiguration { get; [param: NotNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string ClassName()
        {
            var annotatedName = AnnotationProvider.For(Model).DatabaseName;
            if (!string.IsNullOrEmpty(annotatedName))
            {
                return CSharpUtilities.GenerateCSharpIdentifier(annotatedName + DbContextSuffix, null);
            }

            return DefaultDbContextName;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Namespace() => CustomConfiguration.Namespace;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual List<EntityConfiguration> EntityConfigurations
        {
            get
            {
                if (_entityConfigurationMap == null)
                {
                    _entityConfigurationMap = new
                        SortedDictionary<EntityType, EntityConfiguration>(EntityTypeNameComparer.Instance);
                    AddEntityConfigurations();
                }

                return _entityConfigurationMap.Values.ToList();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual List<SequenceConfiguration> SequenceConfigurations
        {
            get
            {
                if (_sequenceConfigurations == null)
                {
                    AddSequenceConfigurations();
                }
                return _sequenceConfigurations;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddSequenceConfigurations()
        {
            _sequenceConfigurations = new List<SequenceConfiguration>();
            foreach (var sequence in AnnotationProvider.For(Model).Sequences)
            {
                var config = _configurationFactory.CreateSequenceConfiguration();

                config.NameIdentifier = CSharpUtilities.DelimitString(sequence.Name);

                if (sequence.ClrType != Sequence.DefaultClrType)
                {
                    config.TypeIdentifier = CSharpUtilities.GetTypeName(sequence.ClrType);
                }

                if (!string.IsNullOrEmpty(sequence.Schema)
                    && AnnotationProvider.For(Model).DefaultSchema != sequence.Schema)
                {
                    config.SchemaNameIdentifier = CSharpUtilities.DelimitString(sequence.Schema);
                }

                if (sequence.StartValue != Sequence.DefaultStartValue)
                {
                    config.FluentApiConfigurations.Add(
                        _configurationFactory.CreateFluentApiConfiguration(
                            false,
                            nameof(RelationalSequenceBuilder.StartsAt),
                            sequence.StartValue.ToString(CultureInfo.InvariantCulture)));
                }
                if (sequence.IncrementBy != Sequence.DefaultIncrementBy)
                {
                    config.FluentApiConfigurations.Add(
                        _configurationFactory.CreateFluentApiConfiguration(
                            false,
                            nameof(RelationalSequenceBuilder.IncrementsBy),
                            sequence.IncrementBy.ToString(CultureInfo.InvariantCulture)));
                }

                if (sequence.MinValue != Sequence.DefaultMinValue)
                {
                    config.FluentApiConfigurations.Add(
                        _configurationFactory.CreateFluentApiConfiguration(
                            false,
                            nameof(RelationalSequenceBuilder.HasMin),
                            sequence.MinValue?.ToString(CultureInfo.InvariantCulture) ?? ""));
                }

                if (sequence.MaxValue != Sequence.DefaultMaxValue)
                {
                    config.FluentApiConfigurations.Add(
                        _configurationFactory.CreateFluentApiConfiguration(
                            false,
                            nameof(RelationalSequenceBuilder.HasMax),
                            sequence.MaxValue?.ToString(CultureInfo.InvariantCulture) ?? ""));
                }

                if (sequence.IsCyclic != Sequence.DefaultIsCyclic)
                {
                    config.FluentApiConfigurations.Add(
                        _configurationFactory.CreateFluentApiConfiguration(
                            false,
                            nameof(RelationalSequenceBuilder.IsCyclic)));
                }

                _sequenceConfigurations.Add(config);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddEntityConfigurations()
        {
            foreach (var entityType in Model.GetEntityTypes())
            {
                var entityConfiguration =
                    _configurationFactory.CreateEntityConfiguration(this, entityType);

                AddEntityPropertiesConfiguration(entityConfiguration);
                AddEntityConfiguration(entityConfiguration);
                AddNavigationProperties(entityConfiguration);
                AddNavigationPropertyInitializers(entityConfiguration);
                AddRelationshipConfiguration(entityConfiguration);

                _entityConfigurationMap.Add((EntityType)entityType, entityConfiguration);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddConnectionStringConfiguration()
        {
            var methodName = Model.Scaffolding().UseProviderMethodName;

            if (string.IsNullOrEmpty(methodName))
            {
                throw new InvalidOperationException(RelationalDesignStrings.MissingUseProviderMethodNameAnnotation);
            }

            _onConfiguringConfigurations.Add(
                _configurationFactory.CreateOptionsBuilderConfiguration(
                    new List<string>
                    {
                        methodName
                        + "("
                        + CSharpUtilities.GenerateVerbatimStringLiteral(CustomConfiguration.ConnectionString)
                        + ")"
                    }));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddEntityPropertiesConfiguration([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            foreach (var property in ScaffoldingUtilities.OrderedProperties(entityConfiguration.EntityType))
            {
                var propertyConfiguration =
                    _configurationFactory.CreatePropertyConfiguration(entityConfiguration, property);
                AddPropertyConfiguration(propertyConfiguration);
                entityConfiguration.PropertyConfigurations.Add(propertyConfiguration);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddEntityConfiguration([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            AddKeyConfiguration(entityConfiguration);
            AddTableNameConfiguration(entityConfiguration);
            AddIndexConfigurations(entityConfiguration);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddKeyConfiguration([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            var entityType = (EntityType)entityConfiguration.EntityType;
            foreach (var key in entityType.GetKeys())
            {
                if (key == null
                    || key.Properties.Count == 0)
                {
                    continue;
                }

                var conventionKeyProperties =
                    _keyDiscoveryConvention.DiscoverKeyProperties(entityType, entityType.GetProperties().ToList());
                if (conventionKeyProperties != null
                    && key.Properties.OrderBy(p => p.Name).SequenceEqual(conventionKeyProperties.OrderBy(p => p.Name)))
                {
                    continue;
                }

                if (key.IsPrimaryKey())
                {
                    var keyFluentApi = _configurationFactory
                        .CreateKeyFluentApiConfiguration("e", key);

                    if (key.Properties.Count == 1
                        && key.Relational().Name ==
                        RelationalKeyAnnotations
                            .GetDefaultKeyName(
                                entityType.Relational().TableName,
                                true, /* is primary key */
                                key.Properties.Select(p => p.Relational().ColumnName)))
                    {
                        keyFluentApi.HasAttributeEquivalent = true;

                        var propertyConfiguration =
                            entityConfiguration.GetOrAddPropertyConfiguration(
                                entityConfiguration, key.Properties.First());
                        propertyConfiguration.AttributeConfigurations.Add(
                            _configurationFactory.CreateAttributeConfiguration(nameof(KeyAttribute)));
                    }

                    entityConfiguration.FluentApiConfigurations.Add(keyFluentApi);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddTableNameConfiguration([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            var entityType = entityConfiguration.EntityType;
            if (AnnotationProvider.For(entityType).Schema != null
                && AnnotationProvider.For(entityType).Schema != AnnotationProvider.For(Model).DefaultSchema)
            {
                var delimitedTableName =
                    CSharpUtilities.DelimitString(AnnotationProvider.For(entityType).TableName);
                var delimitedSchemaName =
                    CSharpUtilities.DelimitString(AnnotationProvider.For(entityType).Schema);
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
            else if (AnnotationProvider.For(entityType).TableName != null
                     && AnnotationProvider.For(entityType).TableName != entityType.DisplayName())
            {
                var delimitedTableName =
                    CSharpUtilities.DelimitString(AnnotationProvider.For(entityType).TableName);
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddIndexConfigurations([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            foreach (var index in entityConfiguration.EntityType.GetIndexes().Cast<Index>())
            {
                AddIndexConfiguration(entityConfiguration, index);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddIndexConfiguration(
            [NotNull] EntityConfiguration entityConfiguration,
            [NotNull] Index index)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));
            Check.NotNull(index, nameof(index));

            entityConfiguration.FluentApiConfigurations.Add(
                _configurationFactory.CreateIndexConfiguration("e", index));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddPropertyConfiguration([NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            AddRequiredConfiguration(propertyConfiguration);
            AddColumnNameAndTypeConfiguration(propertyConfiguration);
            AddMaxLengthConfiguration(propertyConfiguration);
            AddDefaultValueConfiguration(propertyConfiguration);
            AddDefaultExpressionConfiguration(propertyConfiguration);
            AddComputedExpressionConfiguration(propertyConfiguration);
            AddValueGeneratedConfiguration(propertyConfiguration);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddMaxLengthConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            var maxLength = propertyConfiguration.Property.GetMaxLength();
            if (maxLength.HasValue)
            {
                var maxLengthLiteral =
                    CSharpUtilities.GenerateLiteral(maxLength.Value);
                propertyConfiguration.FluentApiConfigurations.Add(
                    _configurationFactory.CreateFluentApiConfiguration(
                        /* hasAttributeEquivalent */ true,
                        nameof(PropertyBuilder.HasMaxLength), maxLengthLiteral));
                propertyConfiguration.AttributeConfigurations.Add(
                    _configurationFactory.CreateAttributeConfiguration(nameof(MaxLengthAttribute), maxLengthLiteral));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddValueGeneratedConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (!((Property)propertyConfiguration.Property).GetValueGeneratedConfigurationSource().HasValue)
            {
                return;
            }

            var valueGenerated = propertyConfiguration.Property.ValueGenerated;

            switch (valueGenerated)
            {
                case ValueGenerated.OnAdd:
                    // If this property is the single integer primary key on the EntityType then
                    // KeyConvention assumes ValueGeneratedOnAdd() so there is no need to add it.
                    if (_keyConvention.FindValueGeneratedOnAddProperty(
                            new List<Property> { (Property)propertyConfiguration.Property },
                            (EntityType)propertyConfiguration.EntityConfiguration.EntityType) == null
                        && AnnotationProvider.For(propertyConfiguration.Property).DefaultValueSql == null)
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
                    propertyConfiguration.FluentApiConfigurations.Add(
                        _configurationFactory.CreateFluentApiConfiguration(
                            /* hasAttributeEquivalent */ false,
                            nameof(PropertyBuilder.ValueGeneratedNever)));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddColumnNameAndTypeConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            var delimitedColumnName =
                AnnotationProvider.For(propertyConfiguration.Property).ColumnName != null
                && AnnotationProvider.For(propertyConfiguration.Property).ColumnName != propertyConfiguration.Property.Name
                    ? CSharpUtilities.DelimitString(
                        AnnotationProvider.For(propertyConfiguration.Property).ColumnName)
                    : null;

            var delimitedColumnTypeName =
                AnnotationProvider.For(propertyConfiguration.Property).ColumnType != null
                    ? CSharpUtilities.DelimitString(
                        AnnotationProvider.For(propertyConfiguration.Property).ColumnType)
                    : null;

            if (delimitedColumnName != null
                && delimitedColumnTypeName != null)
            {
                propertyConfiguration.FluentApiConfigurations.Add(
                    _configurationFactory.CreateFluentApiConfiguration(
                        /* hasAttributeEquivalent */ true,
                        nameof(RelationalPropertyBuilderExtensions.HasColumnName),
                        delimitedColumnName));
                propertyConfiguration.FluentApiConfigurations.Add(
                    _configurationFactory.CreateFluentApiConfiguration(
                        /* hasAttributeEquivalent */ true,
                        nameof(RelationalPropertyBuilderExtensions.HasColumnType),
                        delimitedColumnTypeName));
                propertyConfiguration.AttributeConfigurations.Add(
                    _configurationFactory.CreateAttributeConfiguration(
                        nameof(ColumnAttribute),
                        delimitedColumnName,
                        nameof(ColumnAttribute.TypeName) + " = " + delimitedColumnTypeName));
            }
            else if (delimitedColumnName != null)
            {
                propertyConfiguration.FluentApiConfigurations.Add(
                    _configurationFactory.CreateFluentApiConfiguration(
                        /* hasAttributeEquivalent */ true,
                        nameof(RelationalPropertyBuilderExtensions.HasColumnName),
                        delimitedColumnName));
                propertyConfiguration.AttributeConfigurations.Add(
                    _configurationFactory.CreateAttributeConfiguration(nameof(ColumnAttribute), delimitedColumnName));
            }
            else if (delimitedColumnTypeName != null)
            {
                propertyConfiguration.FluentApiConfigurations.Add(
                    _configurationFactory.CreateFluentApiConfiguration(
                        /* hasAttributeEquivalent */ true,
                        nameof(RelationalPropertyBuilderExtensions.HasColumnType),
                        delimitedColumnTypeName));
                propertyConfiguration.AttributeConfigurations.Add(
                    _configurationFactory.CreateAttributeConfiguration(
                        nameof(ColumnAttribute), nameof(ColumnAttribute.TypeName) + " = " + delimitedColumnTypeName));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddDefaultValueConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (AnnotationProvider.For(propertyConfiguration.Property).DefaultValue != null)
            {
                propertyConfiguration.FluentApiConfigurations.Add(
                    _configurationFactory.CreateFluentApiConfiguration(
                        /* hasAttributeEquivalent */ false,
                        nameof(RelationalPropertyBuilderExtensions.HasDefaultValue),
                        CSharpUtilities.GenerateLiteral(
                            (dynamic)AnnotationProvider.For(propertyConfiguration.Property).DefaultValue)));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddDefaultExpressionConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (AnnotationProvider.For(propertyConfiguration.Property).DefaultValueSql != null)
            {
                propertyConfiguration.FluentApiConfigurations.Add(
                    _configurationFactory.CreateFluentApiConfiguration(
                        /* hasAttributeEquivalent */ false,
                        nameof(RelationalPropertyBuilderExtensions.HasDefaultValueSql),
                        CSharpUtilities.DelimitString(
                            AnnotationProvider.For(propertyConfiguration.Property).DefaultValueSql)));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddComputedExpressionConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (AnnotationProvider.For(propertyConfiguration.Property).ComputedColumnSql != null)
            {
                propertyConfiguration.FluentApiConfigurations.Add(
                    _configurationFactory.CreateFluentApiConfiguration(
                        /* hasAttributeEquivalent */ false,
                        nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql),
                        CSharpUtilities.DelimitString(
                            AnnotationProvider.For(propertyConfiguration.Property).ComputedColumnSql)));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddNavigationProperties([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            foreach (var otherEntityType in entityConfiguration.EntityType
                .Model.GetEntityTypes().Where(et => et != entityConfiguration.EntityType))
            {
                // set up the navigation properties for foreign keys from another EntityType
                // which reference this EntityType (i.e. this EntityType is the principal)
                foreach (var foreignKey in otherEntityType
                    .GetForeignKeys().Where(fk => fk.PrincipalEntityType == entityConfiguration.EntityType))
                {
                    var principalNavProp = foreignKey.PrincipalToDependent;
                    if (principalNavProp != null)
                    {
                        var referencedType = foreignKey.IsUnique
                            ? otherEntityType.Name
                            : "ICollection<" + otherEntityType.Name + ">";
                        var navPropConfiguration =
                            _configurationFactory.CreateNavigationPropertyConfiguration(
                                referencedType, principalNavProp.Name);

                        var dependentNavProp = foreignKey.DependentToPrincipal;
                        if (foreignKey.PrincipalKey.IsPrimaryKey()
                            && dependentNavProp != null)
                        {
                            navPropConfiguration.AttributeConfigurations.Add(
                                _configurationFactory.CreateAttributeConfiguration(
                                    nameof(InversePropertyAttribute),
                                    CSharpUtilities.DelimitString(dependentNavProp.Name)));
                        }

                        entityConfiguration.NavigationPropertyConfigurations.Add(navPropConfiguration);
                    }
                }
            }

            foreach (var foreignKey in entityConfiguration.EntityType.GetForeignKeys())
            {
                // set up the navigation property on this end of foreign keys owned by this EntityType
                // (i.e. this EntityType is the dependent)
                var dependentNavProp = foreignKey.DependentToPrincipal;
                if (dependentNavProp != null)
                {
                    var dependentEndNavPropConfiguration =
                        _configurationFactory.CreateNavigationPropertyConfiguration(
                            foreignKey.PrincipalEntityType.Name, dependentNavProp.Name);

                    var principalNavProp = foreignKey.PrincipalToDependent;
                    if (foreignKey.PrincipalKey.IsPrimaryKey()
                        && principalNavProp != null)
                    {
                        dependentEndNavPropConfiguration.AttributeConfigurations.Add(
                            _configurationFactory.CreateAttributeConfiguration(
                                nameof(ForeignKeyAttribute),
                                CSharpUtilities.DelimitString(
                                    string.Join(",", foreignKey.Properties.Select(p => p.Name)))));
                        dependentEndNavPropConfiguration.AttributeConfigurations.Add(
                            _configurationFactory.CreateAttributeConfiguration(
                                nameof(InversePropertyAttribute),
                                CSharpUtilities.DelimitString(principalNavProp.Name)));
                    }

                    entityConfiguration.NavigationPropertyConfigurations.Add(
                        dependentEndNavPropConfiguration);

                    // set up the other navigation property for self-referencing foreign keys owned by this EntityType
                    if (((ForeignKey)foreignKey).IsSelfReferencing()
                        && principalNavProp != null)
                    {
                        var referencedType = foreignKey.IsUnique
                            ? foreignKey.DeclaringEntityType.Name
                            : "ICollection<" + foreignKey.DeclaringEntityType.Name + ">";
                        var principalEndNavPropConfiguration =
                            _configurationFactory.CreateNavigationPropertyConfiguration(
                                referencedType, principalNavProp.Name);
                        principalEndNavPropConfiguration.AttributeConfigurations.Add(
                            _configurationFactory.CreateAttributeConfiguration(
                                nameof(InversePropertyAttribute),
                                CSharpUtilities.DelimitString(dependentNavProp.Name)));
                        entityConfiguration.NavigationPropertyConfigurations.Add(
                            principalEndNavPropConfiguration);
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddNavigationPropertyInitializers([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            foreach (var otherEntityType in entityConfiguration.EntityType.Model.GetEntityTypes().Where(et => et != entityConfiguration.EntityType))
            {
                // find navigation properties for foreign keys from another EntityType which reference this EntityType
                foreach (var foreignKey in otherEntityType
                    .GetForeignKeys().Where(fk => fk.PrincipalEntityType == entityConfiguration.EntityType))
                {
                    var navigationProperty = foreignKey.PrincipalToDependent;
                    if (!foreignKey.IsUnique
                        && navigationProperty != null)
                    {
                        entityConfiguration.NavigationPropertyInitializerConfigurations.Add(
                            _configurationFactory.CreateNavigationPropertyInitializerConfiguration(
                                navigationProperty.Name, otherEntityType.Name));
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddRelationshipConfiguration([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            foreach (var foreignKey in entityConfiguration.EntityType.GetForeignKeys())
            {
                var dependentEndNavigationProperty = foreignKey.DependentToPrincipal;
                var principalEndNavigationProperty = foreignKey.PrincipalToDependent;
                if (dependentEndNavigationProperty != null
                    && principalEndNavigationProperty != null)
                {
                    var relationshipConfiguration = _configurationFactory
                        .CreateRelationshipConfiguration(
                            entityConfiguration,
                            foreignKey,
                            dependentEndNavigationProperty.Name,
                            principalEndNavigationProperty.Name,
                            foreignKey.DeleteBehavior);
                    relationshipConfiguration.HasAttributeEquivalent = foreignKey.PrincipalKey.IsPrimaryKey();
                    entityConfiguration.RelationshipConfigurations.Add(relationshipConfiguration);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityConfiguration GetEntityConfiguration([NotNull] EntityType entityType)
        {
            if (_entityConfigurationMap == null)
            {
                // ReSharper disable once UnusedVariable
                var _ = EntityConfigurations;
                Debug.Assert(_entityConfigurationMap != null);
            }

            return _entityConfigurationMap[entityType];
        }
    }
}
