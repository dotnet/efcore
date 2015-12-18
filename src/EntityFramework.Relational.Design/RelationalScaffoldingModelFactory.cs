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
using Microsoft.Data.Entity.Metadata.Internal;
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
            Check.NotNull(databaseModel, nameof(databaseModel));

            var modelBuilder = new ModelBuilder(new ConventionSet());

            _tableNamer = new CSharpUniqueNamer<TableModel>(t => t.Name);
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
            // TODO - need to clean up the way CSharpNamer & CSharpUniqueNamer work (see issue #3711)
            if (column.Table != null)
            {
                usedNames.Add(_tableNamer.GetName(table));
            }

            if (!_columnNamers.ContainsKey(table))
            {
                _columnNamers.Add(table, new CSharpUniqueNamer<ColumnModel>(c => c.Name, usedNames));
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
            // TODO can we add navigation properties inline with adding foreign keys?
            VisitNavigationProperties(modelBuilder);

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
                Logger.LogWarning(RelationalDesignStrings.SequencesRequireName);
                return null;
            }

            Type sequenceType = null;
            if (sequence.DataType != null)
            {
                sequenceType = _typeMapper.FindMapping(sequence.DataType)?.ClrType;
            }

            if (sequenceType != null
                && !Sequence.SupportedTypes.Contains(sequenceType))
            {
                Logger.LogWarning(RelationalDesignStrings.BadSequenceType(sequence.Name, sequence.DataType));
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
                Logger.LogWarning(errorMessage);

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

            var mapping = column.DataType != null
                ? _typeMapper.FindMapping(column.DataType)
                : null;

            if (mapping?.ClrType == null)
            {
                Logger.LogWarning(RelationalDesignStrings.CannotFindTypeMappingForColumn(column.DisplayName, column.DataType));
                return null;
            }

            var clrType = column.IsNullable ? mapping.ClrType.MakeNullable() : mapping.ClrType;

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

            if (!column.PrimaryKeyOrdinal.HasValue)
            {
                property.IsRequired(!column.IsNullable);
            }

            return property;
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
                Logger.LogWarning(RelationalDesignStrings.MissingPrimaryKey(table.DisplayName));
                return null;
            }

            var keyProps = keyColumns.Select(GetPropertyName)
                .Where(name => builder.Metadata.FindProperty(name) != null)
                .ToArray();

            if (keyProps.Length != keyColumns.Count)
            {
                Logger.LogWarning(RelationalDesignStrings.PrimaryKeyErrorPropertyNotFound(table.DisplayName));
                return null;
            }

            return builder.HasKey(keyProps);
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

            var propertyNames = index.IndexColumns
                .OrderBy(ic => ic.Ordinal)
                .Select(ic => GetPropertyName(ic.Column))
                .ToArray();

            if (propertyNames.Count(p => builder.Metadata.FindProperty(p) != null) != propertyNames.Length)
            {
                Logger.LogWarning(RelationalDesignStrings.UnableToScaffoldIndexMissingProperty(index.Name));
                return null;
            }

            var columnNames = index.IndexColumns
                .OrderBy(ic => ic.Ordinal)
                .Select(ic => ic.Column.Name);

            if (index.Table != null)
            {
                var primaryKeyColumns = index.Table.Columns
                    .Where(c => c.PrimaryKeyOrdinal.HasValue)
                    .OrderBy(c => c.PrimaryKeyOrdinal);
                if (columnNames.SequenceEqual(primaryKeyColumns.Select(c => c.Name)))
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
                                primaryKeyColumns.Select(c => GetPropertyName(c))))
                    {
                        builder.HasKey(propertyNames.ToArray()).HasName(index.Name);
                    }
                    return null;
                }
            }

            var indexBuilder = builder.HasIndex(propertyNames)
                .IsUnique(index.IsUnique);

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

            return modelBuilder;
        }

        protected virtual IMutableForeignKey VisitForeignKey([NotNull] ModelBuilder modelBuilder, [NotNull] ForeignKeyModel foreignKey)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(foreignKey, nameof(foreignKey));

            if (foreignKey.PrincipalTable == null)
            {
                Logger.LogWarning(RelationalDesignStrings.ForeignKeyScaffoldErrorPrincipalTableNotFound(foreignKey.DisplayName));
                return null;
            }

            var dependentEntityType = modelBuilder.Model.FindEntityType(GetEntityTypeName(foreignKey.Table));

            if (dependentEntityType == null)
            {
                return null;
            }

            var depProps = foreignKey.Columns
                .OrderBy(fc => fc.Ordinal)
                .Select(fc => GetPropertyName(fc.Column))
                .Select(@from => dependentEntityType.FindProperty(@from))
                .ToList()
                .AsReadOnly();

            if (depProps.Any(p => p == null))
            {
                // TODO log which column was not found
                Logger.LogWarning(RelationalDesignStrings.ForeignKeyScaffoldErrorPropertyNotFound(foreignKey.DisplayName));
                return null;
            }

            var principalEntityType = modelBuilder.Model.FindEntityType(GetEntityTypeName(foreignKey.PrincipalTable));

            if (principalEntityType == null)
            {
                Logger.LogWarning(RelationalDesignStrings.ForeignKeyScaffoldErrorPrincipalTableScaffoldingError(foreignKey.DisplayName, foreignKey.PrincipalTable.DisplayName));
                return null;
            }

            var principalProps = foreignKey.Columns
                .OrderBy(fc => fc.Ordinal)
                .Select(fc => GetPropertyName(fc.PrincipalColumn))
                .Select(to => principalEntityType.FindProperty(to))
                .ToList()
                .AsReadOnly();

            if (principalProps.Any(p => p == null))
            {
                Logger.LogWarning(RelationalDesignStrings.ForeignKeyScaffoldErrorPropertyNotFound(foreignKey.DisplayName));
                return null;
            }

            var principalKey = principalEntityType.FindKey(principalProps);
            if (principalKey == null)
            {
                var index = principalEntityType.FindIndex(principalProps);
                if (index != null
                    && index.IsUnique)
                {
                    principalKey = principalEntityType.AddKey(principalProps);
                }
                else
                {
                    var principalColumns = foreignKey.Columns
                        .OrderBy(fc => fc.Ordinal)
                        .Select(c => c.PrincipalColumn.Name)
                        .Aggregate((a, b) => a + "," + b);

                    Logger.LogWarning(
                        RelationalDesignStrings.ForeignKeyScaffoldErrorPrincipalKeyNotFound(
                            foreignKey.DisplayName, principalColumns, principalEntityType.DisplayName()));
                    return null;
                }
            }

            var key = dependentEntityType.GetOrAddForeignKey(depProps, principalKey, principalEntityType);

            key.IsUnique = dependentEntityType.FindKey(depProps) != null;

            key.Relational().Name = foreignKey.Name;

            AssignOnDeleteAction(foreignKey, key);

            return key;
        }

        protected virtual void VisitNavigationProperties([NotNull] ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            // TODO perf cleanup can we do this in 1 loop instead of 2?
            var model = modelBuilder.Model;
            var modelUtilities = new ModelUtilities();

            var entityTypeToExistingIdentifiers = new Dictionary<IEntityType, List<string>>();
            foreach (var entityType in model.GetEntityTypes())
            {
                var existingIdentifiers = new List<string>();
                entityTypeToExistingIdentifiers.Add(entityType, existingIdentifiers);
                existingIdentifiers.Add(entityType.Name);
                existingIdentifiers.AddRange(entityType.GetProperties().Select(p => p.Name));
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
