// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Relational.Design;
using Microsoft.Data.Entity.Relational.Design.Model;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering
{
    public class SqliteMetadataModelProvider : RelationalMetadataModelProvider
    {
        private readonly IMetadataReader _metadataReader;
        private readonly IRelationalTypeMapper _typeMapper;
        private CSharpNamer<Column> _columnNamer;
        private CSharpUniqueNamer<Table> _tableNamer;

        public SqliteMetadataModelProvider(
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] ModelUtilities modelUtilities,
            [NotNull] CSharpUtilities cSharpUtilities,
            [NotNull] IMetadataReader metadataReader)
            : base(loggerFactory, modelUtilities, cSharpUtilities)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(metadataReader, nameof(metadataReader));

            _typeMapper = typeMapper;
            _metadataReader = metadataReader;
        }

        protected override IRelationalAnnotationProvider ExtensionsProvider => new SqliteAnnotationProvider();

        public override IModel GenerateMetadataModel([NotNull] string connectionString, [CanBeNull] TableSelectionSet tableSelectionSet)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));

            var databaseInfo = _metadataReader.GetSchema(connectionString, _tableSelectionSet);

            return GetModel(databaseInfo);
        }

        public override IModel ConstructRelationalModel([NotNull] string connectionString)
        {
            // TODO change base implementation
            throw new NotSupportedException();
        }

        protected virtual IModel GetModel([NotNull] SchemaInfo schemaInfo)
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            _tableNamer = new CSharpUniqueNamer<Table>(t => t.Name);
            _columnNamer = new CSharpNamer<Column>(c => c.Name);

            AddEntityTypes(modelBuilder, schemaInfo);

            return modelBuilder.Model;
        }

        protected virtual string GetEntityTypeName([NotNull] Table table)
           => _tableNamer.GetName(Check.NotNull(table, nameof(table)));

        protected virtual string GetPropertyName([NotNull] Column column)
           => _columnNamer.GetName(Check.NotNull(column, nameof(column)));

        protected virtual void AddEntityTypes([NotNull] ModelBuilder modelBuilder, [NotNull] SchemaInfo schemaInfo)
        {
            foreach (var table in schemaInfo.Tables)
            {
                modelBuilder.Entity(GetEntityTypeName(table), builder =>
                    {
                        builder.ToTable(table.Name, table.SchemaName);

                        AddColumns(builder, table);
                        AddPrimaryKey(builder, table);
                        AddIndexes(builder, table);
                    });
            }

            // TODO can we add navigation properties inline with adding foreign keys?
            foreach (var table in schemaInfo.Tables)
            {
                AddForeignKeys(modelBuilder, table);
            }

            AddNavigationProperties(modelBuilder);
        }

        protected virtual void AddColumns([NotNull] EntityTypeBuilder builder, [NotNull] Table table)
        {
            foreach (var column in table.Columns)
            {
                AddColumn(builder, column);
            }
        }

        protected virtual void AddColumn([NotNull] EntityTypeBuilder builder, [NotNull] Column column)
        {
            // TODO log bad datatypes/catch exception
            RelationalTypeMapping mapping;
            if (!_typeMapper.TryGetMapping(column.DataType, out mapping) || mapping?.ClrType == null)
            {
                LogUnmappableColumn(column);
                return;
            }

            var clrType = (column.IsNullable) ? mapping.ClrType.MakeNullable() : mapping.ClrType;

            var property = builder.Property(clrType, GetPropertyName(column))
                .HasColumnName(column.Name);

            if (column.MaxLength.HasValue)
            {
                property.HasMaxLength(column.MaxLength.Value);
            }

            if (column.IsStoreGenerated == true)
            {
                property.ValueGeneratedOnAdd();
            }

            if (column.IsComputed == true)
            {
                property.ValueGeneratedOnAddOrUpdate();
            }

            if (_typeMapper.GetMapping(property.Metadata).DefaultTypeName != column.DataType
                && !string.IsNullOrWhiteSpace(column.DataType))
            {
                property.HasColumnType(column.DataType);
            }

            if (column.DefaultValue != null)
            {
                property.HasDefaultValueSql(column.DefaultValue);
            }

            if (!column.PrimaryKeyOrdinal.HasValue)
            {
                property.IsRequired(!column.IsNullable);
            }
        }

        protected virtual void AddPrimaryKey([NotNull] EntityTypeBuilder builder, [NotNull] Table table)
        {
            var keyProps = table.Columns
                .Where(c => c.PrimaryKeyOrdinal.HasValue)
                .OrderBy(c => c.PrimaryKeyOrdinal)
                .Select(GetPropertyName)
                .ToArray();

            if (keyProps.Length > 0)
            {
                builder.HasKey(keyProps);
            }
            else
            {
                var errorMessage = SqliteDesignStrings.MissingPrimaryKey(table.Name);
                Logger.LogWarning(errorMessage);

                builder.Metadata.AddAnnotation(AnnotationNameEntityTypeError, RelationalDesignStrings.UnableToGenerateEntityType(builder.Metadata.DisplayName(), errorMessage));

            }
        }

        protected virtual void AddIndexes([NotNull] EntityTypeBuilder entity, [NotNull] Table table)
        {
            foreach (var index in table.Indexes)
            {
                var properties = index.Columns.Select(GetPropertyName).ToArray();

                var indexBuilder = entity.HasIndex(properties).IsUnique(index.IsUnique);

                if (!string.IsNullOrEmpty(index.Name))
                {
                    indexBuilder.HasName(index.Name);
                }

                if (index.IsUnique)
                {
                    var keyBuilder = entity.HasAlternateKey(properties);
                    if (!string.IsNullOrEmpty(index.Name))
                    {
                        keyBuilder.HasName(index.Name);
                    }
                }
            }
        }

        protected virtual void AddForeignKeys([NotNull] ModelBuilder modelBuilder, [NotNull] Table table)
        {
            foreach (var fkInfo in table.ForeignKeys)
            {
                if (fkInfo.PrincipalTable == null)
                {
                    LogFailedForeignKey(fkInfo);
                    continue;
                }

                try
                {
                    var dependentEntityType = modelBuilder.Model.GetEntityType(GetEntityTypeName(fkInfo.Table));
                    var principalEntityType = modelBuilder.Model.GetEntityType(GetEntityTypeName(fkInfo.PrincipalTable));

                    var principalProps = fkInfo.To
                        .Select(GetPropertyName)
                        .Select(to => principalEntityType.GetProperty(to))
                        .ToList()
                        .AsReadOnly();

                    var principalKey = principalEntityType.FindKey(principalProps);
                    if (principalKey == null)
                    {
                        var index = principalEntityType.FindIndex(principalProps);
                        if (index != null
                            && index.IsUnique == true)
                        {
                            principalKey = principalEntityType.AddKey(principalProps);
                        }
                        else
                        {
                            LogFailedForeignKey(fkInfo);
                            continue;
                        }
                    }

                    var depProps = fkInfo.From
                        .Select(GetPropertyName)
                        .Select(@from => dependentEntityType.GetProperty(@from))
                        .ToList()
                        .AsReadOnly();

                    var foreignKey = dependentEntityType.GetOrAddForeignKey(depProps, principalKey, principalEntityType);

                    foreignKey.IsUnique = dependentEntityType.FindKey(depProps) != null;
                }
                catch (Exception ex)
                {
                    if (ex is ModelItemNotFoundException
                        || ex is InvalidOperationException)
                    {
                        LogFailedForeignKey(fkInfo);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        protected virtual void AddNavigationProperties([NotNull] ModelBuilder modelBuilder)
        {
            // TODO perf cleanup can we do this in 1 loop instead of 2?

            Check.NotNull(modelBuilder, nameof(modelBuilder));

            var entityTypeToExistingIdentifiers = new Dictionary<IEntityType, List<string>>();
            foreach (var entityType in modelBuilder.Model.EntityTypes)
            {
                var existingIdentifiers = new List<string>();
                entityTypeToExistingIdentifiers.Add(entityType, existingIdentifiers);
                existingIdentifiers.Add(entityType.Name);
                existingIdentifiers.AddRange(
                    ModelUtilities.OrderedProperties(entityType).Select(p => p.Name));
            }

            foreach (var entityType in modelBuilder.Model.EntityTypes)
            {
                var dependentEndExistingIdentifiers = entityTypeToExistingIdentifiers[entityType];
                foreach (var foreignKey in entityType.GetForeignKeys().Cast<Metadata.ForeignKey>())
                {
                    // set up the name of the navigation property on the dependent end of the foreign key
                    var dependentEndNavigationPropertyCandidateName =
                        ModelUtilities.GetDependentEndCandidateNavigationPropertyName(foreignKey);
                    var dependentEndNavigationPropertyName =
                        CSharpUtilities.GenerateCSharpIdentifier(
                            dependentEndNavigationPropertyCandidateName,
                            dependentEndExistingIdentifiers,
                            NavigationUniquifier);
                    foreignKey.AddAnnotation(
                        AnnotationNameDependentEndNavPropName,
                        dependentEndNavigationPropertyName);
                    dependentEndExistingIdentifiers.Add(dependentEndNavigationPropertyName);

                    // set up the name of the navigation property on the principal end of the foreign key
                    var principalEndExistingIdentifiers =
                        entityTypeToExistingIdentifiers[foreignKey.PrincipalEntityType];
                    var principalEndNavigationPropertyCandidateName =
                        foreignKey.IsSelfReferencing()
                            ? string.Format(
                                CultureInfo.CurrentCulture,
                                SelfReferencingPrincipalEndNavigationNamePattern,
                                dependentEndNavigationPropertyName)
                            : ModelUtilities.GetPrincipalEndCandidateNavigationPropertyName(foreignKey);
                    var principalEndNavigationPropertyName =
                        CSharpUtilities.GenerateCSharpIdentifier(
                            principalEndNavigationPropertyCandidateName,
                            principalEndExistingIdentifiers,
                            NavigationUniquifier);
                    foreignKey.AddAnnotation(
                        AnnotationNamePrincipalEndNavPropName,
                        principalEndNavigationPropertyName);
                    principalEndExistingIdentifiers.Add(principalEndNavigationPropertyName);
                }
            }
        }

        protected virtual void LogFailedForeignKey([NotNull] Relational.Design.Model.ForeignKey foreignKey)
            => Logger.LogWarning(
                SqliteDesignStrings.ForeignKeyScaffoldError(
                    foreignKey.Table?.Name, string.Join(",", foreignKey.From.Select(f => f.Name))));

        protected virtual void LogUnmappableColumn([NotNull] Column column)
            => Logger.LogWarning(
                RelationalDesignStrings.CannotFindTypeMappingForColumn(
                    $"{column.Table?.Name}.{column.Name}", column.DataType));
    }
}
