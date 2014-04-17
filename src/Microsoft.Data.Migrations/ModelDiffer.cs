// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;
using Microsoft.Data.Relational.Model;
using ForeignKey = Microsoft.Data.Relational.Model.ForeignKey;

namespace Microsoft.Data.Migrations
{
    public class ModelDiffer
    {
        private ModelDatabaseMapping _sourceMapping;
        private ModelDatabaseMapping _targetMapping;
        private MigrationOperationCollection _operations;

        // TODO: Rename this method because it is not suggestive of what it does.
        public virtual IReadOnlyList<MigrationOperation> DiffSource([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            var database = new DatabaseBuilder().Build(model);

            var createSequenceOperations = database.Sequences.Select(
                s => new CreateSequenceOperation(s));

            var createTableOperations = database.Tables.Select(
                t => new CreateTableOperation(t));

            var addForeignKeyOperations = database.Tables.SelectMany(
                t => t.ForeignKeys,
                (t, fk) => new AddForeignKeyOperation(
                    fk.Name, fk.Table.Name, fk.ReferencedTable.Name,
                    fk.Columns.Select(c => c.Name).ToArray(),
                    fk.ReferencedColumns.Select(c => c.Name).ToArray(),
                    fk.CascadeDelete));

            var createIndexOperations = database.Tables.SelectMany(
                t => t.Indexes,
                (t, idx) => new CreateIndexOperation(
                    idx.Table.Name, idx.Name,
                    idx.Columns.Select(c => c.Name).ToArray(),
                    idx.IsUnique, idx.IsClustered));

            return
                ((IEnumerable<MigrationOperation>)createSequenceOperations)
                    .Concat(createTableOperations)
                    .Concat(addForeignKeyOperations)
                    .Concat(createIndexOperations)
                    .ToArray();
        }

        // TODO: Rename this method because it is not suggestive of what it does.
        public virtual IReadOnlyList<MigrationOperation> DiffTarget([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            var database = new DatabaseBuilder().Build(model);

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

            _sourceMapping = new DatabaseBuilder().BuildMapping(sourceModel);
            _targetMapping = new DatabaseBuilder().BuildMapping(targetModel);
            _operations = new MigrationOperationCollection();

            DiffSequences();
            DiffTables();

            // TODO: Needs to handle name reuse between renames and circular renames.
            // TODO: Add unit tests for rename column conflict and operation order.

            HandleRenameConflicts();

            return
                ((IEnumerable<MigrationOperation>)_operations.Get<DropIndexOperation>())
                    .Concat(_operations.Get<DropForeignKeyOperation>())
                    .Concat(_operations.Get<DropPrimaryKeyOperation>())
                    .Concat(_operations.Get<DropDefaultConstraintOperation>())
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
                    .Concat(_operations.Get<DropColumnOperation>())
                    .Concat(_operations.Get<DropTableOperation>())
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

                // TODO: Determine how to specify an index in the model to be able to build index pairs.

                //FindRenamedIndexes(indexPairs);
                //FindCreatedIndexes(tablePair, indexPairs);
                //FindDroppedIndexes(tablePair, indexPairs);
            }
        }

        private void HandleRenameConflicts()
        {
            const string newNamePrefix = "__mig_tmp__";
            string newName;
            var newNameIndex = 0;

            foreach (var pair in 
                (from renameOp in _operations.Get<RenameTableOperation>()
                 from dropOp in _operations.Get<DropTableOperation>()
                 where new SchemaQualifiedName(renameOp.NewTableName, renameOp.TableName.Schema).Equals(dropOp.TableName)
                 select new { RenameOp = renameOp, DropOp = dropOp })
                    .ToArray())
            {
                newName = newNamePrefix + newNameIndex++;

                _operations.InsertBefore(pair.RenameOp, new RenameTableOperation(
                    pair.DropOp.TableName, newName));
                _operations.Replace(pair.DropOp, new DropTableOperation(
                    new SchemaQualifiedName(newName, pair.DropOp.TableName.Schema)));
            }

            foreach (var pair in 
                (from renameOp in _operations.Get<RenameColumnOperation>()
                 from dropOp in _operations.Get<DropColumnOperation>()
                 where string.Equals(renameOp.NewColumnName, dropOp.ColumnName, StringComparison.Ordinal)
                 select new { RenameOp = renameOp, DropOp = dropOp })
                    .ToArray())
            {
                newName = newNamePrefix + newNameIndex++;

                _operations.InsertBefore(pair.RenameOp, new RenameColumnOperation(
                    pair.DropOp.TableName, pair.DropOp.ColumnName, newName));
                _operations.Replace(pair.DropOp, new DropColumnOperation(
                    pair.DropOp.TableName, newName));
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
                            fk.Name,
                            fk.Table.Name,
                            fk.ReferencedTable.Name,
                            fk.Columns.Select(c => c.Name).ToArray(),
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
                            fk.Name,
                            fk.Table.Name,
                            fk.ReferencedTable.Name,
                            fk.Columns.Select(c => c.Name).ToArray(),
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

            public void InsertBefore<T>(T refOperation, T newOperation)
                where T : MigrationOperation
            {
                var operations = _allOperations[typeof(T)];
                operations.Insert(operations.IndexOf(refOperation), newOperation);
            }

            public void Replace<T>(T oldOperation, T newOperation)
                where T : MigrationOperation
            {
                var operations = _allOperations[typeof(T)];
                operations[operations.IndexOf(oldOperation)] = newOperation;
            }

            public IEnumerable<T> Get<T>()
                where T : MigrationOperation
            {
                List<MigrationOperation> operations;

                return
                    _allOperations.TryGetValue(typeof(T), out operations)
                        ? operations.Cast<T>()
                        : Enumerable.Empty<T>();
            }
        }
    }
}
