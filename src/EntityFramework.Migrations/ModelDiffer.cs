// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using System;
using System.Collections.Generic;
using System.Linq;
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

            DiffSequences();
            DiffTables();

            // TODO: Add more unit tests for the operation order.

            HandleTransitiveRenames();

            return
                ((IEnumerable<MigrationOperation>)_operations.Get<DropIndexOperation>())
                    .Concat(_operations.Get<DropForeignKeyOperation>())
                    .Concat(_operations.Get<DropPrimaryKeyOperation>())
                    .Concat(_operations.Get<DropDefaultConstraintOperation>())
                    .Concat(_operations.Get<DropColumnOperation>())
                    .Concat(_operations.Get<DropTableOperation>())
                    .Concat(_operations.Get<MoveTableOperation>())
                    .Concat(_operations.Get<RenameTableOperation>())
                    .Concat(_operations.Get<RenameColumnOperation>())
                    .Concat(_operations.Get<RenameIndexOperation>())
                    .Concat(_operations.Get<CreateTableOperation>())
                    .Concat(_operations.Get<AddColumnOperation>())
                    .Concat(_operations.Get<AlterColumnOperation>())
                    .Concat(_operations.Get<AddDefaultConstraintOperation>())
                    .Concat(_operations.Get<AddPrimaryKeyOperation>())
                    .Concat(_operations.Get<AddForeignKeyOperation>())
                    .Concat(_operations.Get<CreateIndexOperation>())
                    .ToArray();
        }

        private void DiffSequences()
        {
            // TODO: Not implemented.
        }

        private void DiffTables()
        {
            var entityTypePairs = FindEntityTypePairs();
            var tablePairs = FindTablePairs(entityTypePairs);

            FindMovedTables(tablePairs);
            FindRenamedTables(tablePairs);
            FindCreatedTables(tablePairs);
            FindDroppedTables(tablePairs);

            var primaryKeyPairs = FindPrimaryKeyPairs(FindPrimaryKeyPairs(entityTypePairs));

            FindAddedPrimaryKeys(tablePairs, primaryKeyPairs);
            FindDroppedPrimaryKeys(tablePairs, primaryKeyPairs);

            for (var i = 0; i < tablePairs.Count; i++)
            {
                var entityTypePair = entityTypePairs[i];
                var tablePair = tablePairs[i];

                var columnPairs = FindColumnPairs(FindPropertyPairs(entityTypePair));

                FindRenamedColumns(columnPairs);
                FindAddedColumns(tablePair, columnPairs);
                FindDroppedColumns(tablePair, columnPairs);
                FindAlteredColumns(columnPairs);
                FindAddedDefaultConstraints(columnPairs);
                FindDroppedDefaultConstraints(columnPairs);

                var foreignKeyPairs = FindForeignKeyPairs(FindForeignKeyPairs(entityTypePair));

                FindAddedForeignKeys(tablePair, foreignKeyPairs);
                FindDroppedForeignKeys(tablePair, foreignKeyPairs);

                var indexPairs = FindIndexPairs(FindIndexPairs(entityTypePair));

                FindRenamedIndexes(indexPairs);
                FindCreatedIndexes(tablePair, indexPairs);
                FindDroppedIndexes(tablePair, indexPairs);
            }
        }

        private void HandleTransitiveRenames()
        {
            const string temporaryNamePrefix = "__mig_tmp__";
            var temporaryNameIndex = 0;

            _operations.Set(
                HandleTransitiveRenames(
                    _operations.Get<RenameTableOperation>(),
                    getParentName: op => null,
                    getName: op => op.TableName,
                    getNewName: op => new SchemaQualifiedName(op.NewTableName, op.TableName.Schema),
                    generateTempName: (op) => new SchemaQualifiedName(temporaryNamePrefix + temporaryNameIndex++, op.TableName.Schema),
                    createRenameOperation: (parentName, name, newName)
                        => new RenameTableOperation(name, SchemaQualifiedName.Parse(newName).Name)));

            _operations.Set(
                HandleTransitiveRenames(
                    _operations.Get<RenameColumnOperation>(),
                    getParentName: op => op.TableName,
                    getName: op => op.ColumnName,
                    getNewName: op => op.NewColumnName,
                    generateTempName: (op) => temporaryNamePrefix + temporaryNameIndex++,
                    createRenameOperation: (parentName, name, newName)
                        => new RenameColumnOperation(parentName, name, newName)));

            _operations.Set(
                HandleTransitiveRenames(
                    _operations.Get<RenameIndexOperation>(),
                    getParentName: op => op.TableName,
                    getName: op => op.IndexName,
                    getNewName: op => op.NewIndexName,
                    generateTempName: (op) => temporaryNamePrefix + temporaryNameIndex++,
                    createRenameOperation: (parentName, name, newName)
                        => new RenameIndexOperation(parentName, name, newName)));
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
            var nameMatchPairs =
                (from et1 in _sourceMapping.Model.EntityTypes
                    from et2 in _targetMapping.Model.EntityTypes
                    where et1.Name.Equals(et2.Name, StringComparison.Ordinal)
                    select Tuple.Create(et1, et2))
                    .ToArray();

            var fuzzyMatchPairs =
                from et1 in _sourceMapping.Model.EntityTypes.Except(nameMatchPairs.Select(p => p.Item1))
                from et2 in _targetMapping.Model.EntityTypes.Except(nameMatchPairs.Select(p => p.Item2))
                where FuzzyMatchEntityTypes(et1, et2)
                select Tuple.Create(et1, et2);

            return nameMatchPairs.Concat(fuzzyMatchPairs).ToArray();
        }

        private static bool FuzzyMatchEntityTypes(IEntityType et1, IEntityType et2)
        {
            // TODO: Not implemented. Needs code to compare keys, properties, etc.

            return false;
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
                        !string.Equals(
                            pair.Item1.Name.Schema,
                            pair.Item2.Name.Schema,
                            StringComparison.Ordinal))
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
                        !string.Equals(
                            pair.Item1.Name.Name,
                            pair.Item2.Name.Name,
                            StringComparison.Ordinal))
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

        private IReadOnlyList<Tuple<IProperty, IProperty>> FindPropertyPairs(
            Tuple<IEntityType, IEntityType> entitTypePair)
        {
            // TODO: This should include the case of property being renamed but column being the same.

            return
                (from p1 in entitTypePair.Item1.Properties
                    from p2 in entitTypePair.Item2.Properties
                    where string.Equals(p1.Name, p2.Name, StringComparison.Ordinal)
                    select Tuple.Create(p1, p2))
                    .ToArray();
        }

        private IReadOnlyList<Tuple<Column, Column>> FindColumnPairs(
            IEnumerable<Tuple<IProperty, IProperty>> propertyPairs)
        {
            return propertyPairs
                .Select(pair =>
                    Tuple.Create(
                        _sourceMapping.GetDatabaseObject<Column>(pair.Item1),
                        _targetMapping.GetDatabaseObject<Column>(pair.Item2)))
                .ToArray();
        }

        private void FindRenamedColumns(
            IEnumerable<Tuple<Column, Column>> columnPairs)
        {
            _operations.AddRange(
                columnPairs
                    .Where(pair =>
                        !string.Equals(
                            pair.Item1.Name,
                            pair.Item2.Name,
                            StringComparison.Ordinal))
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
                    .Where(pair =>
                        SameDefault(pair.Item1, pair.Item2)
                        && (pair.Item1.IsNullable != pair.Item2.IsNullable
                            || !SameType(pair.Item1, pair.Item2)))
                    .Select(pair =>
                        new AlterColumnOperation(
                            pair.Item2.Table.Name,
                            pair.Item2,
                            isDestructiveChange: true)));

            // TODO: Add functionality to determine the value of isDestructiveChange.
        }

        private void FindAddedDefaultConstraints(
            IEnumerable<Tuple<Column, Column>> columnPairs)
        {
            _operations.AddRange(
                columnPairs
                    .Where(pair =>
                        pair.Item2.HasDefault
                        && !SameDefault(pair.Item1, pair.Item2))
                    .Select(pair =>
                        new AddDefaultConstraintOperation(
                            pair.Item2.Table.Name,
                            pair.Item2.Name,
                            pair.Item2.DefaultValue,
                            pair.Item2.DefaultSql)));
        }

        private void FindDroppedDefaultConstraints(
            IEnumerable<Tuple<Column, Column>> columnPairs)
        {
            _operations.AddRange(
                columnPairs
                    .Where(pair =>
                        pair.Item1.HasDefault
                        && !SameDefault(pair.Item1, pair.Item2))
                    .Select(pair =>
                        new DropDefaultConstraintOperation(
                            pair.Item1.Table.Name,
                            pair.Item1.Name)));
        }

        private IReadOnlyList<Tuple<IKey, IKey>> FindPrimaryKeyPairs(
            IEnumerable<Tuple<IEntityType, IEntityType>> entityTypePairs)
        {
            return entityTypePairs
                .Where(pair =>
                    pair.Item1.GetKey() != null
                    && pair.Item2.GetKey() != null
                    && pair.Item1.GetKey().IsClustered()
                    == pair.Item2.GetKey().IsClustered()
                    && SameNames(
                        pair.Item1.GetKey().Properties,
                        pair.Item2.GetKey().Properties))
                .Select(pair =>
                    Tuple.Create(
                        pair.Item1.GetKey(),
                        pair.Item2.GetKey()))
                .ToArray();
        }

        private IReadOnlyList<Tuple<PrimaryKey, PrimaryKey>> FindPrimaryKeyPairs(
            IEnumerable<Tuple<IKey, IKey>> keyPairs)
        {
            return keyPairs
                .Select(pair =>
                    Tuple.Create(
                        _sourceMapping.GetDatabaseObject<PrimaryKey>(pair.Item1),
                        _targetMapping.GetDatabaseObject<PrimaryKey>(pair.Item2)))
                .ToArray();
        }

        private void FindAddedPrimaryKeys(
            IEnumerable<Tuple<Table, Table>> tablePairs,
            IEnumerable<Tuple<PrimaryKey, PrimaryKey>> primaryKeyPairs)
        {
            _operations.AddRange(
                tablePairs
                    .Select(pair => pair.Item2)
                    .Where(t => t.PrimaryKey != null)
                    .Select(t => t.PrimaryKey)
                    .Except(primaryKeyPairs.Select(pair => pair.Item2))
                    .Select(pk =>
                        new AddPrimaryKeyOperation(
                            pk.Table.Name,
                            pk.Name,
                            pk.Columns.Select(c => c.Name).ToArray(),
                            pk.IsClustered)));
        }

        private void FindDroppedPrimaryKeys(
            IEnumerable<Tuple<Table, Table>> tablePairs,
            IEnumerable<Tuple<PrimaryKey, PrimaryKey>> primaryKeyPairs)
        {
            _operations.AddRange(
                tablePairs
                    .Select(pair => pair.Item1)
                    .Where(t => t.PrimaryKey != null)
                    .Select(t => t.PrimaryKey)
                    .Except(primaryKeyPairs.Select(pair => pair.Item1))
                    .Select(pk =>
                        new DropPrimaryKeyOperation(
                            pk.Table.Name,
                            pk.Name)));
        }

        private IEnumerable<Tuple<IForeignKey, IForeignKey>> FindForeignKeyPairs(
            Tuple<IEntityType, IEntityType> entityTypePair)
        {
            return
                (from fk1 in entityTypePair.Item1.ForeignKeys
                    from fk2 in entityTypePair.Item2.ForeignKeys
                    where SameNames(fk1.Properties, fk2.Properties)
                          && SameNames(fk1.ReferencedProperties, fk2.ReferencedProperties)
                          && fk1.CascadeDelete() == fk2.CascadeDelete()
                    select Tuple.Create(fk1, fk2))
                    .ToArray();
        }

        private IReadOnlyList<Tuple<ForeignKey, ForeignKey>> FindForeignKeyPairs(
            IEnumerable<Tuple<IForeignKey, IForeignKey>> foreignKeyPairs)
        {
            return foreignKeyPairs
                .Select(pair =>
                    Tuple.Create(
                        _sourceMapping.GetDatabaseObject<ForeignKey>(pair.Item1),
                        _targetMapping.GetDatabaseObject<ForeignKey>(pair.Item2)))
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

        private IEnumerable<Tuple<IIndex, IIndex>> FindIndexPairs(
            Tuple<IEntityType, IEntityType> entityTypePair)
        {
            return
                (from ix1 in entityTypePair.Item1.Indexes
                    from ix2 in entityTypePair.Item2.Indexes
                    where SameNames(ix1.Properties, ix2.Properties)
                          && ix1.IsUnique == ix2.IsUnique
                          && ix1.IsClustered() == ix2.IsClustered()
                    select Tuple.Create(ix1, ix2))
                    .ToArray();
        }

        private IReadOnlyList<Tuple<Index, Index>> FindIndexPairs(
            IEnumerable<Tuple<IIndex, IIndex>> indexPairs)
        {
            return indexPairs
                .Select(pair =>
                    Tuple.Create(
                        _sourceMapping.GetDatabaseObject<Index>(pair.Item1),
                        _targetMapping.GetDatabaseObject<Index>(pair.Item2)))
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

        private static bool SameNames(
            IReadOnlyList<IProperty> sourceProperties,
            IReadOnlyList<IProperty> targetProperties)
        {
            return
                sourceProperties.Count == targetProperties.Count
                && !sourceProperties
                    .Where((t, i) =>
                        !string.Equals(
                            t.Name,
                            targetProperties[i].Name,
                            StringComparison.Ordinal))
                    .Any();
        }

        private static bool SameDefault(Column sourceColumn, Column targetColumn)
        {
            return
                sourceColumn.DefaultValue == targetColumn.DefaultValue
                && string.Equals(
                    sourceColumn.DefaultSql,
                    targetColumn.DefaultSql,
                    StringComparison.Ordinal);
        }

        private static bool SameType(Column sourceColumn, Column targetColumn)
        {
            return
                sourceColumn.ClrType == targetColumn.ClrType
                && string.Equals(
                    sourceColumn.DataType,
                    targetColumn.DataType,
                    StringComparison.Ordinal);
        }

        private class MigrationOperationCollection
        {
            private readonly Dictionary<Type, List<MigrationOperation>> _allOperations
                = new Dictionary<Type, List<MigrationOperation>>();

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
