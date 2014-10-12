// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using ForeignKey = Microsoft.Data.Entity.Relational.Model.ForeignKey;
using Index = Microsoft.Data.Entity.Relational.Model.Index;

namespace Microsoft.Data.Entity.Migrations
{
    public class ModelDiffer
    {
        private ModelDatabaseMapping _sourceMapping;
        private ModelDatabaseMapping _targetMapping;
        private MigrationOperationCollection _operations;

        private readonly DatabaseBuilder _databaseBuilder;

        public ModelDiffer([NotNull] DatabaseBuilder databaseBuilder)
        {
            Check.NotNull(databaseBuilder, "databaseBuilder");

            _databaseBuilder = databaseBuilder;
        }

        public virtual DatabaseBuilder DatabaseBuilder
        {
            get { return _databaseBuilder; }
        }

        public virtual IReadOnlyList<MigrationOperation> CreateSchema([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            var database = _databaseBuilder.GetDatabase(model);

            return CreateSchema(database);
        }

        public virtual IReadOnlyList<MigrationOperation> CreateSchema([NotNull] DatabaseModel database)
        {
            Check.NotNull(database, "database");

            var createSequenceOperations = database.Sequences.Select(
                s => new CreateSequenceOperation(s));

            var createTableOperations = database.Tables.Select(
                t => new CreateTableOperation(t));

            var addForeignKeyOperations = database.Tables.SelectMany(
                t => t.ForeignKeys,
                (t, fk) => new AddForeignKeyOperation(
                    fk.Table.Name,
                    fk.Name,
                    fk.Columns.Select(c => c.Name).ToArray(),
                    fk.ReferencedTable.Name,
                    fk.ReferencedColumns.Select(c => c.Name).ToArray(),
                    fk.CascadeDelete));

            var createIndexOperations = database.Tables.SelectMany(
                t => t.Indexes,
                (t, idx) => new CreateIndexOperation(
                    idx.Table.Name,
                    idx.Name,
                    idx.Columns.Select(c => c.Name).ToArray(),
                    idx.IsUnique, idx.IsClustered));

            return
                ((IEnumerable<MigrationOperation>)createSequenceOperations)
                    .Concat(createTableOperations)
                    .Concat(addForeignKeyOperations)
                    .Concat(createIndexOperations)
                    .ToArray();
        }

        public virtual IReadOnlyList<MigrationOperation> DropSchema([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            var database = _databaseBuilder.GetDatabase(model);

            return DropSchema(database);
        }

        public virtual IReadOnlyList<MigrationOperation> DropSchema([NotNull] DatabaseModel database)
        {
            Check.NotNull(database, "database");

            var dropSequenceOperations = database.Sequences.Select(
                s => new DropSequenceOperation(s.Name));

            var dropForeignKeyOperations = database.Tables.SelectMany(
                t => t.ForeignKeys,
                (t, fk) => new DropForeignKeyOperation(fk.Table.Name, fk.Name));

            var dropTableOperations = database.Tables.Select(
                t => new DropTableOperation(t.Name));

            return
                ((IEnumerable<MigrationOperation>)dropSequenceOperations)
                    .Concat(dropForeignKeyOperations)
                    .Concat(dropTableOperations)
                    .ToArray();
        }

        public virtual IReadOnlyList<MigrationOperation> Diff([NotNull] IModel sourceModel, [NotNull] IModel targetModel)
        {
            Check.NotNull(sourceModel, "sourceModel");
            Check.NotNull(targetModel, "targetModel");

            _sourceMapping = _databaseBuilder.GetMapping(sourceModel);
            _targetMapping = _databaseBuilder.GetMapping(targetModel);
            _operations = new MigrationOperationCollection();

            Dictionary<Column, Column> columnMap;
            DiffTables(out columnMap);
            DiffSequences(columnMap);

            // TODO: Add more unit tests for the operation order.

            HandleTransitiveRenames();

            return
                ((IEnumerable<MigrationOperation>)_operations.Get<DropSequenceOperation>())
                    .Concat(_operations.Get<MoveSequenceOperation>())
                    .Concat(_operations.Get<RenameSequenceOperation>())
                    .Concat(_operations.Get<AlterSequenceOperation>())
                    .Concat(_operations.Get<CreateSequenceOperation>())
                    .Concat(_operations.Get<DropIndexOperation>())
                    .Concat(_operations.Get<DropForeignKeyOperation>())
                    .Concat(_operations.Get<DropUniqueConstraintOperation>())
                    .Concat(_operations.Get<DropPrimaryKeyOperation>())
                    .Concat(_operations.Get<DropColumnOperation>())
                    .Concat(_operations.Get<DropTableOperation>())
                    .Concat(_operations.Get<MoveTableOperation>())
                    .Concat(_operations.Get<RenameTableOperation>())
                    .Concat(_operations.Get<RenameColumnOperation>())
                    .Concat(_operations.Get<RenameIndexOperation>())
                    .Concat(_operations.Get<AlterColumnOperation>())
                    .Concat(_operations.Get<CreateTableOperation>())
                    .Concat(_operations.Get<AddColumnOperation>())
                    .Concat(_operations.Get<AddPrimaryKeyOperation>())
                    .Concat(_operations.Get<AddUniqueConstraintOperation>())
                    .Concat(_operations.Get<AddForeignKeyOperation>())
                    .Concat(_operations.Get<CreateIndexOperation>())
                    .ToArray();
        }

        private void DiffTables(out Dictionary<Column, Column> columnMap)
        {
            var tablePairs = FindTablePairs(FindEntityTypePairs());
            var columnPairs = new IReadOnlyList<Tuple<Column, Column>>[tablePairs.Count];
            columnMap = new Dictionary<Column, Column>();

            for (var i = 0; i < tablePairs.Count; i++)
            {
                var tableColumnPairs = FindColumnPairs(tablePairs[i]);

                columnPairs[i] = tableColumnPairs;

                foreach (var pair in tableColumnPairs)
                {
                    columnMap.Add(pair.Item1, pair.Item2);
                }
            }

            FindMovedTables(tablePairs);
            FindRenamedTables(tablePairs);
            FindCreatedTables(tablePairs);
            FindDroppedTables(tablePairs);

            for (var i = 0; i < tablePairs.Count; i++)
            {
                var tablePair = tablePairs[i];
                var tableColumnPairs = columnPairs[i];

                FindRenamedColumns(tableColumnPairs);
                FindAddedColumns(tablePair, tableColumnPairs);
                FindDroppedColumns(tablePair, tableColumnPairs);
                FindAlteredColumns(tableColumnPairs);

                FindPrimaryKeyChanges(tablePair, columnMap);

                var uniqueConstraintPairs = FindUniqueConstraintPairs(tablePair, columnMap);

                FindAddedUniqueConstraints(tablePair, uniqueConstraintPairs);
                FindDroppedUniqueConstraints(tablePair, uniqueConstraintPairs);

                var foreignKeyPairs = FindForeignKeyPairs(tablePair, columnMap);

                FindAddedForeignKeys(tablePair, foreignKeyPairs);
                FindDroppedForeignKeys(tablePair, foreignKeyPairs);

                var indexPairs = FindIndexPairs(tablePair, columnMap);

                FindRenamedIndexes(indexPairs);
                FindCreatedIndexes(tablePair, indexPairs);
                FindDroppedIndexes(tablePair, indexPairs);
            }
        }

        private void DiffSequences(Dictionary<Column, Column> columnMap)
        {
            Check.NotNull(columnMap, "columnMap");

            var sequencePairs = FindSequencePairs(columnMap);

            FindMovedSequences(sequencePairs);
            FindRenamedSequences(sequencePairs);
            FindCreatedSequences(sequencePairs);
            FindDroppedSequences(sequencePairs);
            FindAlteredSequences(sequencePairs);
        }

        private void HandleTransitiveRenames()
        {
            const string temporaryNamePrefix = "__mig_tmp__";
            var temporaryNameIndex = 0;

            _operations.Set(HandleTransitiveRenames(
                _operations.Get<RenameSequenceOperation>(),
                op => null,
                op => op.SequenceName,
                op => new SchemaQualifiedName(op.NewSequenceName, op.SequenceName.Schema),
                op => new SchemaQualifiedName(temporaryNamePrefix + temporaryNameIndex++, op.SequenceName.Schema),
                (parentName, name, newName) => new RenameSequenceOperation(name, SchemaQualifiedName.Parse(newName).Name)));

            _operations.Set(HandleTransitiveRenames(
                _operations.Get<RenameTableOperation>(),
                op => null, 
                op => op.TableName, 
                op => new SchemaQualifiedName(op.NewTableName, op.TableName.Schema), 
                op => new SchemaQualifiedName(temporaryNamePrefix + temporaryNameIndex++, op.TableName.Schema), 
                (parentName, name, newName) => new RenameTableOperation(name, SchemaQualifiedName.Parse(newName).Name)));

            _operations.Set(HandleTransitiveRenames(
                _operations.Get<RenameColumnOperation>(), 
                op => op.TableName, 
                op => op.ColumnName, 
                op => op.NewColumnName, 
                op => temporaryNamePrefix + temporaryNameIndex++, 
                (parentName, name, newName) => new RenameColumnOperation(parentName, name, newName)));

            _operations.Set(HandleTransitiveRenames(
                _operations.Get<RenameIndexOperation>(), 
                op => op.TableName, 
                op => op.IndexName, 
                op => op.NewIndexName, 
                op => temporaryNamePrefix + temporaryNameIndex++, 
                (parentName, name, newName) => new RenameIndexOperation(parentName, name, newName)));
        }

        private static IEnumerable<T> HandleTransitiveRenames<T>(
            IReadOnlyList<T> renameOperations,
            Func<T, string> getParentName,
            Func<T, string> getName,
            Func<T, string> getNewName,
            Func<T, string> generateTempName,
            Func<string, string, string, T> createRenameOperation)
            where T : MigrationOperation
        {
            var tempRenameOperations = new List<T>();

            for (var i = 0; i < renameOperations.Count; i++)
            {
                var renameOperation = renameOperations[i];

                var dependentRenameOperation
                    = renameOperations
                        .Skip(i + 1)
                        .SingleOrDefault(r => getName(r) == getNewName(renameOperation));

                if (dependentRenameOperation != null)
                {
                    var tempName = generateTempName(renameOperation);

                    tempRenameOperations.Add(
                        createRenameOperation(
                            getParentName(renameOperation),
                            tempName,
                            getNewName(renameOperation)));

                    renameOperation
                        = createRenameOperation(
                            getParentName(renameOperation),
                            getName(renameOperation),
                            tempName);
                }

                yield return renameOperation;
            }

            foreach (var renameOperation in tempRenameOperations)
            {
                yield return renameOperation;
            }
        }

        private IReadOnlyList<Tuple<IEntityType, IEntityType>> FindEntityTypePairs()
        {
            var simpleMatchPairs =
                (from et1 in _sourceMapping.Model.EntityTypes
                    from et2 in _targetMapping.Model.EntityTypes
                    where SimpleMatchEntityTypes(et1, et2)
                    select Tuple.Create(et1, et2))
                    .ToArray();

            var fuzzyMatchPairs =
                from et1 in _sourceMapping.Model.EntityTypes.Except(simpleMatchPairs.Select(p => p.Item1))
                from et2 in _targetMapping.Model.EntityTypes.Except(simpleMatchPairs.Select(p => p.Item2))
                where FuzzyMatchEntityTypes(et1, et2)
                select Tuple.Create(et1, et2);

            return simpleMatchPairs.Concat(fuzzyMatchPairs).ToArray();
        }

        private IReadOnlyList<Tuple<Table, Table>> FindTablePairs(
            IEnumerable<Tuple<IEntityType, IEntityType>> entityTypePairs)
        {
            return entityTypePairs
                .Select(pair =>
                    Tuple.Create(
                        _sourceMapping.GetDatabaseObject<Table>(pair.Item1),
                        _targetMapping.GetDatabaseObject<Table>(pair.Item2)))
                .ToArray();
        }

        private void FindMovedTables(
            IEnumerable<Tuple<Table, Table>> tablePairs)
        {
            _operations.AddRange(
                tablePairs
                    .Where(pair =>
                        pair.Item1.Name.Schema != pair.Item2.Name.Schema)
                    .Select(pair =>
                        new MoveTableOperation(
                            pair.Item1.Name,
                            pair.Item2.Name.Schema)));
        }

        private void FindRenamedTables(
            IEnumerable<Tuple<Table, Table>> tablePairs)
        {
            _operations.AddRange(
                tablePairs
                    .Where(pair =>
                        pair.Item1.Name.Name != pair.Item2.Name.Name)
                    .Select(pair =>
                        new RenameTableOperation(
                            new SchemaQualifiedName(
                                pair.Item1.Name.Name,
                                pair.Item2.Name.Schema),
                            pair.Item2.Name.Name)));
        }

        private void FindCreatedTables(
            IEnumerable<Tuple<Table, Table>> tablePairs)
        {
            var tables =
                _targetMapping.Database.Tables
                    .Except(tablePairs.Select(p => p.Item2))
                    .ToArray();

            _operations.AddRange(
                tables
                    .Select(t => new CreateTableOperation(t)));

            _operations.AddRange(
                tables
                    .SelectMany(t => t.ForeignKeys)
                    .Select(fk =>
                        new AddForeignKeyOperation(
                            fk.Table.Name,
                            fk.Name,
                            fk.Columns.Select(c => c.Name).ToArray(),
                            fk.ReferencedTable.Name,
                            fk.ReferencedColumns.Select(c => c.Name).ToArray(),
                            fk.CascadeDelete)));

            _operations.AddRange(
                tables
                    .SelectMany(t => t.Indexes)
                    .Select(idx =>
                        new CreateIndexOperation(
                            idx.Table.Name,
                            idx.Name,
                            idx.Columns.Select(c => c.Name).ToArray(),
                            idx.IsUnique,
                            idx.IsClustered)));
        }

        private void FindDroppedTables(
            IEnumerable<Tuple<Table, Table>> tablePairs)
        {
            _operations.AddRange(
                _sourceMapping.Database.Tables
                    .Except(tablePairs.Select(p => p.Item1))
                    .Select(t => new DropTableOperation(t.Name)));
        }

        private IReadOnlyList<Tuple<Column, Column>> FindColumnPairs(
            Tuple<Table, Table> tablePair)
        {
            var simplePropertyMatchPairs =
                (from c1 in tablePair.Item1.Columns
                    from c2 in tablePair.Item2.Columns
                    where
                        SimpleMatchProperties(
                            _sourceMapping.GetModelObject<IProperty>(c1),
                            _targetMapping.GetModelObject<IProperty>(c2))
                    select Tuple.Create(c1, c2))
                    .ToArray();

            var simpleColumnMatchPairs =
                from c1 in tablePair.Item1.Columns.Except(simplePropertyMatchPairs.Select(p => p.Item1))
                from c2 in tablePair.Item2.Columns.Except(simplePropertyMatchPairs.Select(p => p.Item2))
                where SimpleMatchColumns(c1, c2)
                select Tuple.Create(c1, c2);

            return simplePropertyMatchPairs.Concat(simpleColumnMatchPairs).ToArray();
        }

        private void FindRenamedColumns(
            IEnumerable<Tuple<Column, Column>> columnPairs)
        {
            _operations.AddRange(
                columnPairs
                    .Where(pair =>
                        pair.Item1.Name != pair.Item2.Name)
                    .Select(pair =>
                        new RenameColumnOperation(
                            pair.Item2.Table.Name,
                            pair.Item1.Name,
                            pair.Item2.Name)));
        }

        private void FindAddedColumns(
            Tuple<Table, Table> tablePair,
            IEnumerable<Tuple<Column, Column>> columnPairs)
        {
            _operations.AddRange(
                tablePair.Item2.Columns
                    .Except(columnPairs.Select(pair => pair.Item2))
                    .Select(c => new AddColumnOperation(c.Table.Name, c)));
        }

        private void FindDroppedColumns(
            Tuple<Table, Table> tablePair,
            IEnumerable<Tuple<Column, Column>> columnPairs)
        {
            _operations.AddRange(
                tablePair.Item1.Columns
                    .Except(columnPairs.Select(pair => pair.Item1))
                    .Select(c => new DropColumnOperation(tablePair.Item2.Name, c.Name)));
        }

        private void FindAlteredColumns(
            IEnumerable<Tuple<Column, Column>> columnPairs)
        {
            _operations.AddRange(
                columnPairs
                    .Where(pair => !EquivalentColumns(pair.Item1, pair.Item2))
                    .Select(pair =>
                        new AlterColumnOperation(
                            pair.Item2.Table.Name,
                            pair.Item2,
                            isDestructiveChange: true)));

            // TODO: Add functionality to determine the value of isDestructiveChange.
        }

        private void FindPrimaryKeyChanges(
            Tuple<Table, Table> tablePair,
            IDictionary<Column, Column> columnMap)
        {
            var sourcePrimaryKey = tablePair.Item1.PrimaryKey;
            var targetPrimaryKey = tablePair.Item2.PrimaryKey;

            if (targetPrimaryKey == null)
            {
                if (sourcePrimaryKey == null)
                {
                    return;
                }

                DropPrimaryKey(sourcePrimaryKey);
            }
            else if (sourcePrimaryKey == null)
            {
                AddPrimaryKey(targetPrimaryKey);
            }
            else if (!MatchPrimaryKeys(sourcePrimaryKey, targetPrimaryKey, columnMap))
            {
                DropPrimaryKey(sourcePrimaryKey);
                AddPrimaryKey(targetPrimaryKey);
            }
        }

        private void AddPrimaryKey(PrimaryKey primaryKey)
        {
            _operations.Add(
                new AddPrimaryKeyOperation(
                    primaryKey.Table.Name,
                    primaryKey.Name,
                    primaryKey.Columns.Select(c => c.Name).ToArray(),
                    primaryKey.IsClustered));
        }

        private void DropPrimaryKey(PrimaryKey primaryKey)
        {
            _operations.Add(
                new DropPrimaryKeyOperation(
                    primaryKey.Table.Name,
                    primaryKey.Name));
        }

        private IReadOnlyList<Tuple<UniqueConstraint, UniqueConstraint>> FindUniqueConstraintPairs(
            Tuple<Table, Table> table,
            IDictionary<Column, Column> columnMap)
        {
            return
                (from uc1 in table.Item1.UniqueConstraints
                 from uc2 in table.Item2.UniqueConstraints
                 where MatchUniqueConstraints(uc1, uc2, columnMap)
                 select Tuple.Create(uc1, uc2))
                    .ToArray();
        }

        private void FindAddedUniqueConstraints(
            Tuple<Table, Table> tablePair,
            IEnumerable<Tuple<UniqueConstraint, UniqueConstraint>> uniqueConstraintPairs)
        {
            _operations.AddRange(
                tablePair.Item2.UniqueConstraints
                    .Except(uniqueConstraintPairs.Select(pair => pair.Item2))
                    .Select(uc => new AddUniqueConstraintOperation(uc)));
        }

        private void FindDroppedUniqueConstraints(
            Tuple<Table, Table> tablePair,
            IEnumerable<Tuple<UniqueConstraint, UniqueConstraint>> uniqueConstraintPairs)
        {
            _operations.AddRange(
                tablePair.Item1.UniqueConstraints
                    .Except(uniqueConstraintPairs.Select(pair => pair.Item1))
                    .Select(uc =>
                        new DropUniqueConstraintOperation(
                            uc.Table.Name,
                            uc.Name)));
        }

        private IReadOnlyList<Tuple<ForeignKey, ForeignKey>> FindForeignKeyPairs(
            Tuple<Table, Table> table,
            IDictionary<Column, Column> columnMap)
        {
            return
                (from fk1 in table.Item1.ForeignKeys
                    from fk2 in table.Item2.ForeignKeys
                    where MatchForeignKeys(fk1, fk2, columnMap)
                    select Tuple.Create(fk1, fk2))
                    .ToArray();
        }

        private void FindAddedForeignKeys(
            Tuple<Table, Table> tablePair,
            IEnumerable<Tuple<ForeignKey, ForeignKey>> foreignKeyPairs)
        {
            _operations.AddRange(
                tablePair.Item2.ForeignKeys
                    .Except(foreignKeyPairs.Select(pair => pair.Item2))
                    .Select(fk =>
                        new AddForeignKeyOperation(
                            fk.Table.Name,
                            fk.Name,
                            fk.Columns.Select(c => c.Name).ToArray(),
                            fk.ReferencedTable.Name,
                            fk.ReferencedColumns.Select(c => c.Name).ToArray(),
                            fk.CascadeDelete)));
        }

        private void FindDroppedForeignKeys(
            Tuple<Table, Table> tablePair,
            IEnumerable<Tuple<ForeignKey, ForeignKey>> foreignKeyPairs)
        {
            _operations.AddRange(
                tablePair.Item1.ForeignKeys
                    .Except(foreignKeyPairs.Select(pair => pair.Item1))
                    .Select(fk =>
                        new DropForeignKeyOperation(
                            fk.Table.Name,
                            fk.Name)));
        }

        private IReadOnlyList<Tuple<Index, Index>> FindIndexPairs(
            Tuple<Table, Table> tablePair,
            IDictionary<Column, Column> columnMap)
        {
            return
                (from ix1 in tablePair.Item1.Indexes
                    from ix2 in tablePair.Item2.Indexes
                    where MatchIndexes(ix1, ix2, columnMap)
                    select Tuple.Create(ix1, ix2))
                    .ToArray();
        }

        private void FindRenamedIndexes(
            IEnumerable<Tuple<Index, Index>> indexPairs)
        {
            _operations.AddRange(
                indexPairs
                    .Where(pair =>
                        !string.Equals(pair.Item1.Name, pair.Item2.Name))
                    .Select(pair =>
                        new RenameIndexOperation(
                            pair.Item2.Table.Name,
                            pair.Item1.Name,
                            pair.Item2.Name)));
        }

        private void FindCreatedIndexes(
            Tuple<Table, Table> tablePair,
            IEnumerable<Tuple<Index, Index>> indexPairs)
        {
            _operations.AddRange(
                tablePair.Item2.Indexes
                    .Except(indexPairs.Select(pair => pair.Item2))
                    .Select(idx =>
                        new CreateIndexOperation(
                            idx.Table.Name,
                            idx.Name,
                            idx.Columns.Select(c => c.Name).ToArray(),
                            idx.IsUnique,
                            idx.IsClustered)));
        }

        private void FindDroppedIndexes(
            Tuple<Table, Table> tablePair,
            IEnumerable<Tuple<Index, Index>> indexPairs)
        {
            _operations.AddRange(
                tablePair.Item1.Indexes
                    .Except(indexPairs.Select(pair => pair.Item1))
                    .Select(idx =>
                        new DropIndexOperation(
                            idx.Table.Name,
                            idx.Name)));
        }

        private IReadOnlyList<Tuple<Sequence, Sequence>> FindSequencePairs(Dictionary<Column, Column> columnMap)
        {
            return
                (from columnPair in columnMap
                    let sourceSequenceName = GetSequenceName(columnPair.Key)
                    where sourceSequenceName != null
                    let targetSequenceName = GetSequenceName(columnPair.Value)
                    where targetSequenceName != null
                    let sourceSequence = _sourceMapping.Database.GetSequence(sourceSequenceName)
                    let targetSequence = _targetMapping.Database.GetSequence(targetSequenceName)
                    where SimpleMatchSequences(sourceSequence, targetSequence)
                    select Tuple.Create(sourceSequence, targetSequence))
                    .ToList();
        }

        private void FindMovedSequences(IEnumerable<Tuple<Sequence, Sequence>> sequencePairs)
        {
            _operations.AddRange(
                sequencePairs
                    .Where(pair =>
                        pair.Item1.Name.Schema != pair.Item2.Name.Schema)
                    .Select(pair =>
                        new MoveSequenceOperation(
                            pair.Item1.Name,
                            pair.Item2.Name.Schema)));
        }

        private void FindRenamedSequences(IEnumerable<Tuple<Sequence, Sequence>> sequencePairs)
        {
            _operations.AddRange(
                sequencePairs
                    .Where(pair =>
                        pair.Item1.Name.Name != pair.Item2.Name.Name)
                    .Select(pair =>
                        new RenameSequenceOperation(
                            new SchemaQualifiedName(
                                pair.Item1.Name.Name,
                                pair.Item2.Name.Schema),
                            pair.Item2.Name.Name)));
        }

        private void FindCreatedSequences(IEnumerable<Tuple<Sequence, Sequence>> sequencePairs)
        {
            _operations.AddRange(
                _targetMapping.Database.Sequences
                    .Except(sequencePairs.Select(p => p.Item2))
                    .Select(s => new CreateSequenceOperation(s)));
        }

        private void FindDroppedSequences(IEnumerable<Tuple<Sequence, Sequence>> sequencePairs)
        {
            _operations.AddRange(
                _sourceMapping.Database.Sequences
                    .Except(sequencePairs.Select(p => p.Item1))
                    .Select(s => new DropSequenceOperation(s.Name)));
        }

        private void FindAlteredSequences(IEnumerable<Tuple<Sequence, Sequence>> sequencePairs)
        {
            _operations.AddRange(
                sequencePairs
                    .Where(pair => !EquivalentSequences(pair.Item1, pair.Item2))
                    .Select(pair =>
                        new AlterSequenceOperation(
                            pair.Item2.Name,
                            pair.Item2.IncrementBy)));
        }

        protected virtual bool SimpleMatchEntityTypes([NotNull] IEntityType sourceEntityType, [NotNull] IEntityType targetEntityType)
        {
            Check.NotNull(sourceEntityType, "sourceEntityType");
            Check.NotNull(targetEntityType, "targetEntityType");

            return sourceEntityType.Name == targetEntityType.Name;
        }

        protected virtual bool FuzzyMatchEntityTypes([NotNull] IEntityType sourceEntityType, [NotNull] IEntityType targetEntityType)
        {
            Check.NotNull(sourceEntityType, "sourceEntityType");
            Check.NotNull(targetEntityType, "targetEntityType");

            var matchingPropertyCount =
                (from p1 in sourceEntityType.Properties
                    from p2 in targetEntityType.Properties
                    where EquivalentProperties(p1, p2)
                    select 1)
                    .Count();

            // At least 80% of properties, across both entities, must be equivalent.
            return (matchingPropertyCount * 2.0f / (sourceEntityType.Properties.Count + targetEntityType.Properties.Count)) >= 0.80;
        }

        protected virtual bool EquivalentProperties([NotNull] IProperty sourceProperty, [NotNull] IProperty targetProperty)
        {
            Check.NotNull(sourceProperty, "sourceProperty");
            Check.NotNull(targetProperty, "targetProperty");

            return
                sourceProperty.Name == targetProperty.Name
                && sourceProperty.PropertyType == targetProperty.PropertyType;
        }

        protected virtual bool SimpleMatchProperties([NotNull] IProperty sourceProperty, [NotNull] IProperty targetProperty)
        {
            Check.NotNull(sourceProperty, "sourceProperty");
            Check.NotNull(targetProperty, "targetProperty");

            return sourceProperty.Name == targetProperty.Name;
        }

        protected virtual bool EquivalentColumns([NotNull] Column sourceColumn, [NotNull] Column targetColumn)
        {
            Check.NotNull(sourceColumn, "sourceColumn");
            Check.NotNull(targetColumn, "targetColumn");

            return
                sourceColumn.ClrType == targetColumn.ClrType
                && sourceColumn.DataType == targetColumn.DataType
                && sourceColumn.DefaultValue == targetColumn.DefaultValue
                && sourceColumn.DefaultSql == targetColumn.DefaultSql
                && sourceColumn.IsNullable == targetColumn.IsNullable
                && sourceColumn.ValueGenerationStrategy == targetColumn.ValueGenerationStrategy
                && sourceColumn.IsTimestamp == targetColumn.IsTimestamp
                && sourceColumn.MaxLength == targetColumn.MaxLength
                && sourceColumn.Precision == targetColumn.Precision
                && sourceColumn.Scale == targetColumn.Scale
                && sourceColumn.IsFixedLength == targetColumn.IsFixedLength
                && sourceColumn.IsUnicode == targetColumn.IsUnicode;
        }

        protected virtual bool SimpleMatchSequences([NotNull] Sequence sourceSequence, [NotNull] Sequence targetSequence)
        {
            Check.NotNull(sourceSequence, "sourceSequence");
            Check.NotNull(targetSequence, "targetSequence");

            return
                sourceSequence.DataType == targetSequence.DataType;
        }

        protected virtual bool EquivalentSequences([NotNull] Sequence sourceSequence, [NotNull] Sequence targetSequence)
        {
            Check.NotNull(sourceSequence, "sourceSequence");
            Check.NotNull(targetSequence, "targetSequence");

            return 
                sourceSequence.IncrementBy == targetSequence.IncrementBy;
        }

        protected virtual bool SimpleMatchColumns([NotNull] Column sourceColumn, [NotNull] Column targetColumn)
        {
            Check.NotNull(sourceColumn, "sourceColumn");
            Check.NotNull(targetColumn, "targetColumn");

            return sourceColumn.Name == targetColumn.Name;
        }

        protected virtual bool MatchPrimaryKeys(
            [NotNull] PrimaryKey sourcePrimaryKey,
            [NotNull] PrimaryKey targetPrimaryKey,
            [NotNull] IDictionary<Column, Column> columnMap)
        {
            Check.NotNull(sourcePrimaryKey, "sourcePrimaryKey");
            Check.NotNull(targetPrimaryKey, "targetPrimaryKey");
            Check.NotNull(columnMap, "columnMap");

            return
                sourcePrimaryKey.Name == targetPrimaryKey.Name
                && sourcePrimaryKey.IsClustered == targetPrimaryKey.IsClustered
                && MatchColumnReferences(sourcePrimaryKey.Columns, targetPrimaryKey.Columns, columnMap);
        }

        protected virtual bool MatchUniqueConstraints(
            [NotNull] UniqueConstraint sourceUniqueConstraint,
            [NotNull] UniqueConstraint targetUniqueConstraint,
            [NotNull] IDictionary<Column, Column> columnMap)
        {
            Check.NotNull(sourceUniqueConstraint, "sourceUniqueConstraint");
            Check.NotNull(targetUniqueConstraint, "targetUniqueConstraint");
            Check.NotNull(columnMap, "columnMap");

            return
                sourceUniqueConstraint.Name == targetUniqueConstraint.Name
                && MatchColumnReferences(sourceUniqueConstraint.Columns, targetUniqueConstraint.Columns, columnMap);
        }

        protected virtual bool MatchForeignKeys(
            [NotNull] ForeignKey sourceForeignKey,
            [NotNull] ForeignKey targetForeignKey,
            [NotNull] IDictionary<Column, Column> columnMap)
        {
            Check.NotNull(sourceForeignKey, "sourceForeignKey");
            Check.NotNull(targetForeignKey, "targetForeignKey");
            Check.NotNull(columnMap, "columnMap");

            return
                sourceForeignKey.Name == targetForeignKey.Name
                && sourceForeignKey.CascadeDelete == targetForeignKey.CascadeDelete
                && MatchColumnReferences(sourceForeignKey.Columns, targetForeignKey.Columns, columnMap)
                && MatchColumnReferences(sourceForeignKey.ReferencedColumns, targetForeignKey.ReferencedColumns, columnMap);
        }

        protected virtual bool MatchIndexes(
            [NotNull] Index sourceIndex,
            [NotNull] Index targetIndex,
            [NotNull] IDictionary<Column, Column> columnMap)
        {
            Check.NotNull(sourceIndex, "sourceIndex");
            Check.NotNull(targetIndex, "targetIndex");
            Check.NotNull(columnMap, "columnMap");

            return
                sourceIndex.IsUnique == targetIndex.IsUnique
                && sourceIndex.IsClustered == targetIndex.IsClustered
                && MatchColumnReferences(sourceIndex.Columns, targetIndex.Columns, columnMap);
        }

        protected virtual bool MatchColumnReferences(
            [NotNull] Column sourceColumn,
            [NotNull] Column targetColumn,
            [NotNull] IDictionary<Column, Column> columnMap)
        {
            Check.NotNull(sourceColumn, "sourceColumn");
            Check.NotNull(targetColumn, "targetColumn");
            Check.NotNull(columnMap, "columnMap");

            Column column;
            return columnMap.TryGetValue(sourceColumn, out column) && ReferenceEquals(column, targetColumn);
        }

        protected virtual bool MatchColumnReferences(
            [NotNull] IReadOnlyList<Column> sourceColumns,
            [NotNull] IReadOnlyList<Column> targetColumns,
            [NotNull] IDictionary<Column, Column> columnMap)
        {
            Check.NotNull(sourceColumns, "sourceColumns");
            Check.NotNull(targetColumns, "targetColumns");
            Check.NotNull(columnMap, "columnMap");

            return
                sourceColumns.Count == targetColumns.Count
                && !sourceColumns.Where((t, i) => !MatchColumnReferences(t, targetColumns[i], columnMap)).Any();
        }

        protected virtual string GetSequenceName([NotNull] Column column)
        {
            return null;
        }

        protected class MigrationOperationCollection
        {
            private readonly Dictionary<Type, List<MigrationOperation>> _allOperations
                = new Dictionary<Type, List<MigrationOperation>>();

            public void Add(MigrationOperation newOperation)
            {
                List<MigrationOperation> operations;

                if (_allOperations.TryGetValue(newOperation.GetType(), out operations))
                {
                    operations.Add(newOperation);
                }
                else
                {
                    _allOperations.Add(newOperation.GetType(), new List<MigrationOperation> { newOperation });
                }
            }

            public void AddRange<T>(IEnumerable<T> newOperations)
                where T : MigrationOperation
            {
                List<MigrationOperation> operations;

                if (_allOperations.TryGetValue(typeof(T), out operations))
                {
                    operations.AddRange(newOperations);
                }
                else
                {
                    _allOperations.Add(typeof(T), new List<MigrationOperation>(newOperations));
                }
            }

            public void Set<T>(IEnumerable<T> operations)
                where T : MigrationOperation
            {
                _allOperations[typeof(T)] = new List<MigrationOperation>(operations);
            }

            public IReadOnlyList<T> Get<T>()
                where T : MigrationOperation
            {
                List<MigrationOperation> operations;

                return
                    _allOperations.TryGetValue(typeof(T), out operations)
                        ? operations.Cast<T>().ToArray()
                        : Enumerable.Empty<T>().ToArray();
            }
        }
    }
}
