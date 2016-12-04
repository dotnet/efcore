// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Extensions;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public class RelationalScaffoldingModelFactory : IScaffoldingModelFactory
    {
        internal const string NavigationNameUniquifyingPattern = "{0}Navigation";
        internal const string SelfReferencingPrincipalEndNavigationNamePattern = "Inverse{0}";

        protected virtual ILogger Logger { get; }
        protected virtual IRelationalTypeMapper TypeMapper { get; }
        protected virtual CandidateNamingService CandidateNamingService { get; }

        private Dictionary<TableModel, CSharpUniqueNamer<ColumnModel>> _columnNamers;
        private readonly TableModel _nullTable = new TableModel();
        private CSharpUniqueNamer<TableModel> _tableNamer;
        private readonly IDatabaseModelFactory _databaseModelFactory;
        private readonly HashSet<ColumnModel> _unmappedColumns = new HashSet<ColumnModel>();

        public RelationalScaffoldingModelFactory(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IDatabaseModelFactory databaseModelFactory,
            [NotNull] CandidateNamingService candidateNamingService)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(databaseModelFactory, nameof(databaseModelFactory));
            Check.NotNull(candidateNamingService, nameof(candidateNamingService));

            Logger = loggerFactory.CreateLogger<RelationalScaffoldingModelFactory>();
            TypeMapper = typeMapper;
            CandidateNamingService = candidateNamingService;
            _databaseModelFactory = databaseModelFactory;
        }

        public virtual IModel Create(string connectionString, TableSelectionSet tableSelectionSet)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));

            var databaseModel = _databaseModelFactory.Create(connectionString, tableSelectionSet ?? TableSelectionSet.All);
            if (tableSelectionSet != null)
            {
                CheckSelectionsMatched(tableSelectionSet);
            }

            return CreateFromDatabaseModel(databaseModel);
        }

        public virtual void CheckSelectionsMatched([NotNull] TableSelectionSet tableSelectionSet)
        {
            foreach (var schemaSelection in tableSelectionSet.Schemas.Where(s => !s.IsMatched))
            {
                Logger.LogWarning(
                    RelationalDesignEventId.MissingSchemaWarning,
                    () => RelationalDesignStrings.MissingSchema(schemaSelection.Text));
            }

            foreach (var tableSelection in tableSelectionSet.Tables.Where(t => !t.IsMatched))
            {
                Logger.LogWarning(
                    RelationalDesignEventId.MissingTableWarning,
                    () => RelationalDesignStrings.MissingTable(tableSelection.Text));
            }
        }

        protected virtual IModel CreateFromDatabaseModel([NotNull] DatabaseModel databaseModel)
        {
            Check.NotNull(databaseModel, nameof(databaseModel));

            var modelBuilder = new ModelBuilder(new ConventionSet());

            _tableNamer = new CSharpUniqueNamer<TableModel>(t => CandidateNamingService.GenerateCandidateIdentifier(t.Name));
            _columnNamers = new Dictionary<TableModel, CSharpUniqueNamer<ColumnModel>>();

            VisitDatabaseModel(modelBuilder, databaseModel);

            return modelBuilder.Model;
        }

        protected virtual string GetEntityTypeName([NotNull] TableModel table)
            => _tableNamer.GetName(Check.NotNull(table, nameof(table)));

        protected virtual string GetPropertyName([NotNull] ColumnModel column)
        {
            Check.NotNull(column, nameof(column));

            var table = column.Table ?? _nullTable;
            var usedNames = new List<string>();
            // TODO - need to clean up the way CSharpNamer & CSharpUniqueNamer work (see issue #1671)
            if (column.Table != null)
            {
                usedNames.Add(_tableNamer.GetName(table));
            }

            if (!_columnNamers.ContainsKey(table))
            {
                _columnNamers.Add(table,
                    new CSharpUniqueNamer<ColumnModel>(
                        c => CandidateNamingService.GenerateCandidateIdentifier(c.Name), usedNames));
            }

            return _columnNamers[table].GetName(column);
        }

        protected virtual ModelBuilder VisitDatabaseModel([NotNull] ModelBuilder modelBuilder, [NotNull] DatabaseModel databaseModel)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(databaseModel, nameof(databaseModel));

            if (!string.IsNullOrEmpty(databaseModel.DefaultSchemaName))
            {
                modelBuilder.HasDefaultSchema(databaseModel.DefaultSchemaName);
            }

            if (!string.IsNullOrEmpty(databaseModel.DatabaseName))
            {
                modelBuilder.Model.Relational().DatabaseName = databaseModel.DatabaseName;
            }

            VisitSequences(modelBuilder, databaseModel.Sequences);
            VisitTables(modelBuilder, databaseModel.Tables);
            VisitForeignKeys(modelBuilder, databaseModel.Tables.SelectMany(table => table.ForeignKeys).ToList());

            return modelBuilder;
        }

        protected virtual ModelBuilder VisitSequences([NotNull] ModelBuilder modelBuilder, [NotNull] ICollection<SequenceModel> sequences)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(sequences, nameof(sequences));

            foreach (var sequence in sequences)
            {
                VisitSequence(modelBuilder, sequence);
            }

            return modelBuilder;
        }

        protected virtual RelationalSequenceBuilder VisitSequence([NotNull] ModelBuilder modelBuilder, [NotNull] SequenceModel sequence)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(sequence, nameof(sequence));

            if (string.IsNullOrEmpty(sequence.Name))
            {
                Logger.LogWarning(
                    RelationalDesignEventId.SequenceMustBeNamedWarning,
                    () => RelationalDesignStrings.SequencesRequireName);
                return null;
            }

            Type sequenceType = null;
            if (sequence.DataType != null)
            {
                sequenceType = TypeMapper.FindMapping(sequence.DataType)?.ClrType;
            }

            if (sequenceType != null
                && !Sequence.SupportedTypes.Contains(sequenceType))
            {
                Logger.LogWarning(
                    RelationalDesignEventId.SequenceTypeNotSupportedWarning,
                    () => RelationalDesignStrings.BadSequenceType(sequence.Name, sequence.DataType));
                return null;
            }

            var builder = sequenceType != null
                ? modelBuilder.HasSequence(sequenceType, sequence.Name, sequence.SchemaName)
                : modelBuilder.HasSequence(sequence.Name, sequence.SchemaName);

            if (sequence.IncrementBy.HasValue)
            {
                builder.IncrementsBy(sequence.IncrementBy.Value);
            }

            if (sequence.Max.HasValue)
            {
                builder.HasMax(sequence.Max.Value);
            }

            if (sequence.Min.HasValue)
            {
                builder.HasMin(sequence.Min.Value);
            }

            if (sequence.Start.HasValue)
            {
                builder.StartsAt(sequence.Start.Value);
            }

            if (sequence.IsCyclic.HasValue)
            {
                builder.IsCyclic(sequence.IsCyclic.Value);
            }

            return builder;
        }

        protected virtual ModelBuilder VisitTables([NotNull] ModelBuilder modelBuilder, [NotNull] ICollection<TableModel> tables)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(tables, nameof(tables));

            foreach (var table in tables)
            {
                VisitTable(modelBuilder, table);
            }

            return modelBuilder;
        }

        protected virtual EntityTypeBuilder VisitTable([NotNull] ModelBuilder modelBuilder, [NotNull] TableModel table)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(table, nameof(table));

            var entityTypeName = GetEntityTypeName(table);
            var builder = modelBuilder.Entity(entityTypeName);

            builder.ToTable(table.Name, table.SchemaName);

            VisitColumns(builder, table.Columns);

            var keyBuilder = VisitPrimaryKey(builder, table);

            if (keyBuilder == null)
            {
                var errorMessage = RelationalDesignStrings.UnableToGenerateEntityType(table.DisplayName);
                Logger.LogWarning(
                    RelationalDesignEventId.UnableToGenerateEntityTypeWarning,
                    () => errorMessage);

                var model = modelBuilder.Model;
                model.RemoveEntityType(entityTypeName);
                model.Scaffolding().EntityTypeErrors.Add(entityTypeName, errorMessage);
                return null;
            }

            VisitIndexes(builder, table.Indexes);

            return builder;
        }

        protected virtual EntityTypeBuilder VisitColumns([NotNull] EntityTypeBuilder builder, [NotNull] ICollection<ColumnModel> columns)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NotNull(columns, nameof(columns));

            foreach (var column in columns)
            {
                VisitColumn(builder, column);
            }

            return builder;
        }

        protected virtual PropertyBuilder VisitColumn([NotNull] EntityTypeBuilder builder, [NotNull] ColumnModel column)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NotNull(column, nameof(column));

            var typeMapping = GetTypeMapping(column);

            var clrType = typeMapping?.ClrType;
            if (clrType == null)
            {
                _unmappedColumns.Add(column);
                Logger.LogWarning(
                    RelationalDesignEventId.ColumnTypeNotMappedWarning,
                    () => RelationalDesignStrings.CannotFindTypeMappingForColumn(column.DisplayName, column.DataType));
                return null;
            }

            if (column.IsNullable)
            {
                clrType = clrType.MakeNullable();
            }

            var property = builder.Property(clrType, GetPropertyName(column));

            if (TypeMapper.GetMapping(property.Metadata).StoreType != column.DataType
                && !string.IsNullOrWhiteSpace(column.DataType))
            {
                property.HasColumnType(column.DataType);
            }

            property.HasColumnName(column.Name);

            if (column.MaxLength.HasValue)
            {
                property.HasMaxLength(column.MaxLength.Value);
            }

            if (column.ValueGenerated == ValueGenerated.OnAdd)
            {
                property.ValueGeneratedOnAdd();
            }

            if (column.ValueGenerated == ValueGenerated.OnAddOrUpdate)
            {
                property.ValueGeneratedOnAddOrUpdate();
            }

            if (column.DefaultValue != null)
            {
                property.HasDefaultValueSql(column.DefaultValue);
            }

            if (column.ComputedValue != null)
            {
                property.HasComputedColumnSql(column.ComputedValue);
            }

            if (!column.PrimaryKeyOrdinal.HasValue)
            {
                property.IsRequired(!column.IsNullable);
            }

            property.Metadata.Scaffolding().ColumnOrdinal = column.Ordinal;
            return property;
        }

        protected virtual RelationalTypeMapping GetTypeMapping([NotNull] ColumnModel column)
        {
            Check.NotNull(column, nameof(column));

            return column.DataType == null ? null : TypeMapper.FindMapping(column.DataType);
        }

        protected virtual KeyBuilder VisitPrimaryKey([NotNull] EntityTypeBuilder builder, [NotNull] TableModel table)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NotNull(table, nameof(table));

            var keyColumns = table.Columns
                .Where(c => c.PrimaryKeyOrdinal.HasValue)
                .OrderBy(c => c.PrimaryKeyOrdinal)
                .ToList();

            if (keyColumns.Count == 0)
            {
                Logger.LogWarning(
                    RelationalDesignEventId.MissingPrimaryKeyWarning,
                    () => RelationalDesignStrings.MissingPrimaryKey(table.DisplayName));
                return null;
            }

            var unmappedColumns = keyColumns
                .Where(c => _unmappedColumns.Contains(c))
                .Select(c => c.Name).ToList();
            if (unmappedColumns.Any())
            {
                Logger.LogWarning(
                    RelationalDesignEventId.PrimaryKeyColumnsNotMappedWarning,
                    () => RelationalDesignStrings.PrimaryKeyErrorPropertyNotFound(
                        table.DisplayName,
                        string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumns)));
                return null;
            }

            return builder.HasKey(keyColumns.Select(GetPropertyName).ToArray());
        }

        protected virtual EntityTypeBuilder VisitIndexes([NotNull] EntityTypeBuilder builder, [NotNull] ICollection<IndexModel> indexes)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NotNull(indexes, nameof(indexes));

            foreach (var index in indexes)
            {
                VisitIndex(builder, index);
            }

            return builder;
        }

        protected virtual IndexBuilder VisitIndex([NotNull] EntityTypeBuilder builder, [NotNull] IndexModel index)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NotNull(index, nameof(index));

            var indexColumns = index.IndexColumns
                .OrderBy(ic => ic.Ordinal)
                .Select(ic => ic.Column).ToList();
            var unmappedColumns = indexColumns
                .Where(c => _unmappedColumns.Contains(c))
                .Select(c => c.Name).ToList();
            if (unmappedColumns.Any())
            {
                Logger.LogWarning(
                    RelationalDesignEventId.IndexColumnsNotMappedWarning,
                    () => RelationalDesignStrings.UnableToScaffoldIndexMissingProperty(
                        index.Name,
                        string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumns)));
                return null;
            }

            var columnNames = indexColumns.Select(c => c.Name);
            var propertyNames = indexColumns.Select(GetPropertyName).ToArray();
            if (index.Table != null)
            {
                var primaryKeyColumns = index.Table.Columns
                    .Where(c => c.PrimaryKeyOrdinal.HasValue)
                    .OrderBy(c => c.PrimaryKeyOrdinal);
                if (columnNames.SequenceEqual(primaryKeyColumns.Select(c => c.Name)) && index.Filter == null)
                {
                    // index is supporting the primary key. So there is no need for
                    // an extra index in the model. But if the index name does not
                    // match what would be produced by default then need to call
                    // HasName() on the primary key.
                    if (index.Name !=
                        RelationalKeyAnnotations
                            .GetDefaultKeyName(
                                index.Table.Name,
                                true, /* is primary key */
                                primaryKeyColumns.Select(GetPropertyName)))
                    {
                        builder.HasKey(propertyNames).HasName(index.Name);
                    }
                    return null;
                }
            }

            var indexBuilder = builder.HasIndex(propertyNames)
                .IsUnique(index.IsUnique);

            if (index.Filter != null)
            {
                indexBuilder.HasFilter(index.Filter);
            }

            if (!string.IsNullOrEmpty(index.Name))
            {
                indexBuilder.HasName(index.Name);
            }

            if (index.IsUnique)
            {
                var keyBuilder = builder.HasAlternateKey(propertyNames);
                if (!string.IsNullOrEmpty(index.Name))
                {
                    keyBuilder.HasName(index.Name);
                }
            }

            return indexBuilder;
        }

        protected virtual ModelBuilder VisitForeignKeys([NotNull] ModelBuilder modelBuilder, [NotNull] IList<ForeignKeyModel> foreignKeys)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(foreignKeys, nameof(foreignKeys));

            foreach (var fk in foreignKeys)
            {
                VisitForeignKey(modelBuilder, fk);
            }

            // Note: must completely assign all foreign keys before assigning
            // navigation properties otherwise naming of navigation properties
            // when there are multiple foreign keys does not work.
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(et => et.GetForeignKeys()))
            {
                AddNavigationProperties(foreignKey);
            }

            return modelBuilder;
        }

        protected virtual IMutableForeignKey VisitForeignKey([NotNull] ModelBuilder modelBuilder, [NotNull] ForeignKeyModel foreignKey)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(foreignKey, nameof(foreignKey));

            if (foreignKey.PrincipalTable == null)
            {
                Logger.LogWarning(
                    RelationalDesignEventId.ForeignKeyReferencesMissingTableWarning,
                    () => RelationalDesignStrings.ForeignKeyScaffoldErrorPrincipalTableNotFound(foreignKey.DisplayName));
                return null;
            }

            if (foreignKey.Table == null)
            {
                return null;
            }

            var dependentEntityType = modelBuilder.Model.FindEntityType(GetEntityTypeName(foreignKey.Table));

            if (dependentEntityType == null)
            {
                return null;
            }

            var foreignKeyColumns = foreignKey.Columns.OrderBy(fc => fc.Ordinal);
            var unmappedDependentColumns = foreignKeyColumns
                .Select(fc => fc.Column)
                .Where(c => _unmappedColumns.Contains(c))
                .Select(c => c.Name)
                .ToList();
            if (unmappedDependentColumns.Any())
            {
                Logger.LogWarning(
                    RelationalDesignEventId.ForeignKeyColumnsNotMappedWarning,
                    () => RelationalDesignStrings.ForeignKeyScaffoldErrorPropertyNotFound(
                        foreignKey.DisplayName,
                        string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedDependentColumns)));
                return null;
            }

            var dependentProperties = foreignKeyColumns
                .Select(fc => GetPropertyName(fc.Column))
                .Select(name => dependentEntityType.FindProperty(name))
                .ToList()
                .AsReadOnly();

            var principalEntityType = modelBuilder.Model.FindEntityType(GetEntityTypeName(foreignKey.PrincipalTable));
            if (principalEntityType == null)
            {
                Logger.LogWarning(
                    RelationalDesignEventId.ForeignKeyReferencesMissingTableWarning,
                    () => RelationalDesignStrings.ForeignKeyScaffoldErrorPrincipalTableScaffoldingError(
                        foreignKey.DisplayName, foreignKey.PrincipalTable.DisplayName));
                return null;
            }

            var unmappedPrincipalColumns = foreignKeyColumns
                .Select(fc => fc.PrincipalColumn)
                .Where(pc => principalEntityType.FindProperty(GetPropertyName(pc)) == null)
                .Select(pc => pc.Name)
                .ToList();
            if (unmappedPrincipalColumns.Any())
            {
                Logger.LogWarning(
                    RelationalDesignEventId.ForeignKeyColumnsNotMappedWarning,
                    () => RelationalDesignStrings.ForeignKeyScaffoldErrorPropertyNotFound(
                        foreignKey.DisplayName,
                        string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedPrincipalColumns)));
                return null;
            }

            var principalProperties = foreignKeyColumns
                .Select(fc => GetPropertyName(fc.PrincipalColumn))
                .Select(name => principalEntityType.FindProperty(name))
                .ToList()
                .AsReadOnly();

            var principalKey = principalEntityType.FindKey(principalProperties);
            if (principalKey == null)
            {
                var index = principalEntityType.FindIndex(principalProperties);
                if (index != null
                    && index.IsUnique)
                {
                    principalKey = principalEntityType.AddKey(principalProperties);
                }
                else
                {
                    var principalColumns = foreignKeyColumns
                        .Select(c => c.PrincipalColumn.Name)
                        .Aggregate((a, b) => a + "," + b);

                    Logger.LogWarning(
                        RelationalDesignEventId.ForeignKeyReferencesMissingPrincipalKeyWarning,
                        () => RelationalDesignStrings.ForeignKeyScaffoldErrorPrincipalKeyNotFound(
                            foreignKey.DisplayName, principalColumns, principalEntityType.DisplayName()));
                    return null;
                }
            }

            var key = dependentEntityType.GetOrAddForeignKey(
                dependentProperties, principalKey, principalEntityType);

            key.IsUnique = dependentEntityType.FindKey(dependentProperties) != null;

            key.Relational().Name = foreignKey.Name;

            AssignOnDeleteAction(foreignKey, key);

            return key;
        }

        protected virtual void AddNavigationProperties([NotNull] IMutableForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            var dependentEndExistingIdentifiers = ExistingIdentifiers(foreignKey.DeclaringEntityType);
            var dependentEndNavigationPropertyCandidateName =
                CandidateNamingService.GetDependentEndCandidateNavigationPropertyName(foreignKey);
            var dependentEndNavigationPropertyName =
                CSharpUtilities.Instance.GenerateCSharpIdentifier(
                    dependentEndNavigationPropertyCandidateName,
                    dependentEndExistingIdentifiers,
                    NavigationUniquifier);

            foreignKey.HasDependentToPrincipal(dependentEndNavigationPropertyName);

            var principalEndExistingIdentifiers = ExistingIdentifiers(foreignKey.PrincipalEntityType);
            var principalEndNavigationPropertyCandidateName = foreignKey.IsSelfReferencing()
                ? string.Format(
                    CultureInfo.CurrentCulture,
                    SelfReferencingPrincipalEndNavigationNamePattern,
                    dependentEndNavigationPropertyName)
                : CandidateNamingService.GetPrincipalEndCandidateNavigationPropertyName(
                    foreignKey, dependentEndNavigationPropertyName);
            var principalEndNavigationPropertyName =
                CSharpUtilities.Instance.GenerateCSharpIdentifier(
                    principalEndNavigationPropertyCandidateName,
                    principalEndExistingIdentifiers,
                    NavigationUniquifier);

            foreignKey.HasPrincipalToDependent(principalEndNavigationPropertyName);
        }

        // Stores the names of the EntityType itself and its Properties, but does not include any Navigation Properties
        private readonly Dictionary<IEntityType, List<string>> _entityTypeAndPropertyIdentifiers = new Dictionary<IEntityType, List<string>>();

        protected virtual List<string> ExistingIdentifiers([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            List<string> existingIdentifiers;
            if (!_entityTypeAndPropertyIdentifiers.TryGetValue(entityType, out existingIdentifiers))
            {
                existingIdentifiers = new List<string>();
                existingIdentifiers.Add(entityType.Name);
                existingIdentifiers.AddRange(entityType.GetProperties().Select(p => p.Name));
                _entityTypeAndPropertyIdentifiers[entityType] = existingIdentifiers;
            }

            existingIdentifiers.AddRange(entityType.GetNavigations().Select(p => p.Name));
            return existingIdentifiers;
        }

        private static void AssignOnDeleteAction(
            [NotNull] ForeignKeyModel fkModel, [NotNull] IMutableForeignKey foreignKey)
        {
            Check.NotNull(fkModel, nameof(fkModel));
            Check.NotNull(foreignKey, nameof(foreignKey));

            switch (fkModel.OnDelete)
            {
                case ReferentialAction.Cascade:
                    foreignKey.DeleteBehavior = DeleteBehavior.Cascade;
                    break;

                case ReferentialAction.SetNull:
                    foreignKey.DeleteBehavior = DeleteBehavior.SetNull;
                    break;

                default:
                    foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
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
