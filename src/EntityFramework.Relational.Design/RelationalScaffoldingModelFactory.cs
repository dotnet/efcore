// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Scaffolding.Internal;
using Microsoft.Data.Entity.Scaffolding.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Scaffolding
{
    public class RelationalScaffoldingModelFactory : IScaffoldingModelFactory
    {
        internal const string NavigationNameUniquifyingPattern = "{0}Navigation";
        internal const string SelfReferencingPrincipalEndNavigationNamePattern = "Inverse{0}";

        internal static IReadOnlyList<string> IgnoredAnnotations { get; } = new List<string>
        {
            CoreAnnotationNames.OriginalValueIndexAnnotation,
            CoreAnnotationNames.ShadowIndexAnnotation
        };

        protected virtual ILogger Logger { get; }

        private Dictionary<TableModel, CSharpUniqueNamer<ColumnModel>> _columnNamers;
        private readonly TableModel _nullTable = new TableModel();
        private CSharpUniqueNamer<TableModel> _tableNamer;
        private readonly IRelationalTypeMapper _typeMapper;
        private readonly IDatabaseModelFactory _databaseModelFactory;

        public RelationalScaffoldingModelFactory(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IDatabaseModelFactory databaseModelFactory)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(databaseModelFactory, nameof(databaseModelFactory));

            Logger = loggerFactory.CreateCommandsLogger();
            _typeMapper = typeMapper;
            _databaseModelFactory = databaseModelFactory;
        }

        public virtual IModel Create([NotNull] string connectionString, [CanBeNull] TableSelectionSet tableSelectionSet)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));

            var schemaInfo = _databaseModelFactory.Create(connectionString, tableSelectionSet ?? TableSelectionSet.All);

            return CreateFromDatabaseModel(schemaInfo);
        }

        protected virtual IModel CreateFromDatabaseModel([NotNull] DatabaseModel databaseModel)
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            _tableNamer = new CSharpUniqueNamer<TableModel>(t => t.Name);
            _columnNamers = new Dictionary<TableModel, CSharpUniqueNamer<ColumnModel>>();

            VisitDatabaseModel(modelBuilder, databaseModel);

            if (!string.IsNullOrEmpty(databaseModel.DefaultSchemaName))
            {
                modelBuilder.HasDefaultSchema(databaseModel.DefaultSchemaName);
            }

            if (!string.IsNullOrEmpty(databaseModel.DatabaseName))
            {
                modelBuilder.Model.Relational().DatabaseName = databaseModel.DatabaseName;
            }

            return modelBuilder.Model;
        }

        protected virtual string GetEntityTypeName([NotNull] TableModel table)
            => _tableNamer.GetName(Check.NotNull(table, nameof(table)));

        protected virtual string GetPropertyName([NotNull] ColumnModel column)
        {
            var table = column.Table ?? _nullTable;

            if (!_columnNamers.ContainsKey(table))
            {
                _columnNamers.Add(table, new CSharpUniqueNamer<ColumnModel>(c => c.Name));
            }

            return _columnNamers[table].GetName(column);
        }

        protected virtual ModelBuilder VisitDatabaseModel([NotNull] ModelBuilder modelBuilder, [NotNull] DatabaseModel databaseModel)
        {
            var tables = databaseModel.Tables;
            VisitTables(modelBuilder, tables);

            // TODO can we add navigation properties inline with adding foreign keys?
            foreach (var table in databaseModel.Tables)
            {
                VisitForeignKeys(modelBuilder, table);
            }

            VisitNavigationProperties(modelBuilder.Model);

            return modelBuilder;
        }

        protected virtual ModelBuilder VisitTables([NotNull] ModelBuilder modelBuilder, [NotNull] IList<TableModel> tables)
        {
            foreach (var table in tables)
            {
                modelBuilder.Entity(GetEntityTypeName(table), builder =>
                    {
                        builder.ToTable(table.Name, table.SchemaName);

                        VisitTable(builder, table);
                    });
            }

            return modelBuilder;
        }

        protected virtual EntityTypeBuilder VisitTable([NotNull] EntityTypeBuilder builder, [NotNull] TableModel table)
        {
            VisitColumns(builder, table.Columns);
            VisitPrimaryKey(builder, table);
            VisitIndexes(builder, table.Indexes);

            return builder;
        }

        protected virtual EntityTypeBuilder VisitColumns([NotNull] EntityTypeBuilder builder, [NotNull] IList<ColumnModel> columns)
        {
            foreach (var column in columns)
            {
                try
                {
                    VisitColumn(builder, column);
                }
                catch (NotSupportedException)
                {
                    VisitUnmappableColumn(column);
                }
            }

            return builder;
        }

        protected virtual PropertyBuilder VisitColumn([NotNull] EntityTypeBuilder builder, [NotNull] ColumnModel column)
        {
            RelationalTypeMapping mapping;

            if (!_typeMapper.TryGetMapping(column.DataType, out mapping)
                || mapping.ClrType == null)
            {
                throw new NotSupportedException();
            }

            var clrType = (column.IsNullable) ? mapping.ClrType.MakeNullable() : mapping.ClrType;

            var property = builder.Property(clrType, GetPropertyName(column));

            if (_typeMapper.GetMapping(property.Metadata).DefaultTypeName != column.DataType
                && !string.IsNullOrWhiteSpace(column.DataType))
            {
                property.HasColumnType(column.DataType);
            }

            property.HasColumnName(column.Name);

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

            if (column.DefaultValue != null)
            {
                property.HasDefaultValueSql(column.DefaultValue);
            }

            if (!column.PrimaryKeyOrdinal.HasValue)
            {
                property.IsRequired(!column.IsNullable);
            }

            return property;
        }

        protected virtual void VisitUnmappableColumn([NotNull] ColumnModel column)
            => Logger.LogWarning(
                RelationalDesignStrings.CannotFindTypeMappingForColumn(
                    column.DisplayName, column.DataType));

        protected virtual EntityTypeBuilder VisitPrimaryKey([NotNull] EntityTypeBuilder builder, [NotNull] TableModel table)
        {
            var keyProps = table.Columns
                .Where(c => c.PrimaryKeyOrdinal.HasValue)
                .OrderBy(c => c.PrimaryKeyOrdinal)
                .Select(GetPropertyName)
                .ToArray();

            if (keyProps.Length > 0)
            {
                try
                {
                    builder.HasKey(keyProps);
                    return builder;
                }
                catch (InvalidOperationException)
                {
                    // swallow. Handled by logging
                }
            }

            var errorMessage = RelationalDesignStrings.MissingPrimaryKey(table.DisplayName);
            Logger.LogWarning(errorMessage);

            builder.Metadata.Scaffolding().EntityTypeError = RelationalDesignStrings.UnableToGenerateEntityType(builder.Metadata.DisplayName(), errorMessage);

            return builder;
        }

        protected virtual EntityTypeBuilder VisitIndexes([NotNull] EntityTypeBuilder builder, [NotNull] IList<IndexModel> indexes)
        {
            foreach (var index in indexes)
            {
                var indexBuilder = VisitIndex(builder, index);
                if (indexBuilder == null)
                {
                    Logger.LogWarning(RelationalDesignStrings.UnableToScaffoldIndex(index.Name));
                }
            }

            return builder;
        }

        protected virtual IndexBuilder VisitIndex([NotNull] EntityTypeBuilder builder, [NotNull] IndexModel index)
        {
            var properties = index.Columns.Select(GetPropertyName).ToArray();
            if (properties.Any(i => i == null))
            {
                return null;
            }

            var indexBuilder = builder.HasIndex(properties)
                .IsUnique(index.IsUnique);

            if (!string.IsNullOrEmpty(index.Name))
            {
                indexBuilder.HasName(index.Name);
            }

            if (index.IsUnique)
            {
                var keyBuilder = builder.HasAlternateKey(properties);
                if (!string.IsNullOrEmpty(index.Name))
                {
                    keyBuilder.HasName(index.Name);
                }
            }

            return indexBuilder;
        }

        protected virtual ModelBuilder VisitForeignKeys([NotNull] ModelBuilder modelBuilder, [NotNull] TableModel table)
        {
            foreach (var fkInfo in table.ForeignKeys)
            {
                VisitForeignKey(modelBuilder, fkInfo);
            }

            return modelBuilder;
        }

        protected virtual IMutableForeignKey VisitForeignKey([NotNull] ModelBuilder modelBuilder, [NotNull] ForeignKeyModel foreignKey)
        {
            var key = TryVisitForeignKey(modelBuilder, foreignKey);

            if (key == null)
            {
                VisitFailedForeignKey(foreignKey);
            }
            return key;
        }

        private IMutableForeignKey TryVisitForeignKey(ModelBuilder modelBuilder, ForeignKeyModel foreignKey)
        {
            if (foreignKey.PrincipalTable == null)
            {
                return null;
            }

            var dependentEntityType = modelBuilder.Model.FindEntityType(GetEntityTypeName(foreignKey.Table));
            var principalEntityType = modelBuilder.Model.FindEntityType(GetEntityTypeName(foreignKey.PrincipalTable));

            if (dependentEntityType == null
                || principalEntityType == null)
            {
                return null;
            }

            var principalProps = foreignKey.PrincipalColumns
                .Select(GetPropertyName)
                .Select(to => principalEntityType.FindProperty(to))
                .ToList()
                .AsReadOnly();

            if (principalProps.Any(p => p == null))
            {
                return null;
            }

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
                    return null;
                }
            }

            var depProps = foreignKey.Columns
                .Select(GetPropertyName)
                .Select(@from => dependentEntityType.FindProperty(@from))
                .ToList()
                .AsReadOnly();

            var key = dependentEntityType.GetOrAddForeignKey(depProps, principalKey, principalEntityType);

            key.IsUnique = dependentEntityType.FindKey(depProps) != null;

            AssignOnDeleteAction(foreignKey, key);

            return key;
        }

        protected virtual void VisitFailedForeignKey([NotNull] ForeignKeyModel foreignKey)
            => Logger.LogWarning(
                RelationalDesignStrings.ForeignKeyScaffoldError(foreignKey.DisplayName));

        protected virtual void VisitNavigationProperties([NotNull] IModel model)
        {
            // TODO perf cleanup can we do this in 1 loop instead of 2?
            var modelUtilities = new ModelUtilities();
            Check.NotNull(model, nameof(model));

            var entityTypeToExistingIdentifiers = new Dictionary<IEntityType, List<string>>();
            foreach (var entityType in model.GetEntityTypes())
            {
                var existingIdentifiers = new List<string>();
                entityTypeToExistingIdentifiers.Add(entityType, existingIdentifiers);
                existingIdentifiers.Add(entityType.Name);
                existingIdentifiers.AddRange(
                    modelUtilities.OrderedProperties(entityType).Select(p => p.Name));
            }

            foreach (var entityType in model.GetEntityTypes())
            {
                var dependentEndExistingIdentifiers = entityTypeToExistingIdentifiers[entityType];
                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    // set up the name of the navigation property on the dependent end of the foreign key
                    var dependentEndNavigationPropertyCandidateName =
                        modelUtilities.GetDependentEndCandidateNavigationPropertyName(foreignKey);
                    var dependentEndNavigationPropertyName =
                        CSharpUtilities.Instance.GenerateCSharpIdentifier(
                            dependentEndNavigationPropertyCandidateName,
                            dependentEndExistingIdentifiers,
                            NavigationUniquifier);
                    foreignKey.Scaffolding().DependentEndNavigation = dependentEndNavigationPropertyName;
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
                            : modelUtilities.GetPrincipalEndCandidateNavigationPropertyName(foreignKey);
                    var principalEndNavigationPropertyName =
                        CSharpUtilities.Instance.GenerateCSharpIdentifier(
                            principalEndNavigationPropertyCandidateName,
                            principalEndExistingIdentifiers,
                            NavigationUniquifier);
                    foreignKey.Scaffolding().PrincipalEndNavigation = principalEndNavigationPropertyName;
                    principalEndExistingIdentifiers.Add(principalEndNavigationPropertyName);
                }
            }
        }

        private static void AssignOnDeleteAction(
            [NotNull] ForeignKeyModel from, [NotNull] IMutableForeignKey to)
        {
            Check.NotNull(from, nameof(from));
            Check.NotNull(to, nameof(to));

            switch (from.OnDelete)
            {
                case ReferentialAction.Cascade:
                    to.DeleteBehavior = DeleteBehavior.Cascade;
                    break;

                case ReferentialAction.SetNull:
                    to.DeleteBehavior = DeleteBehavior.SetNull;
                    break;

                default:
                    to.DeleteBehavior = DeleteBehavior.Restrict;
                    break;
            }
        }

        // TODO use CSharpUniqueNamer
        private string NavigationUniquifier([NotNull] string proposedIdentifier, [CanBeNull] ICollection<string> existingIdentifiers)
        {
            if (existingIdentifiers == null
                || !existingIdentifiers.Contains(proposedIdentifier))
            {
                return proposedIdentifier;
            }

            var finalIdentifier =
                string.Format(CultureInfo.CurrentCulture, NavigationNameUniquifyingPattern, proposedIdentifier);
            var suffix = 1;
            while (existingIdentifiers.Contains(finalIdentifier))
            {
                finalIdentifier = proposedIdentifier + suffix;
                suffix++;
            }

            return finalIdentifier;
        }
    }
}
