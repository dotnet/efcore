// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Microsoft.Data.Entity.Relational.Migrations.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations
{
    public class ModelDiffer
    {
        private readonly IRelationalMetadataExtensionProvider _extensionProvider;
        private readonly RelationalTypeMapper _typeMapper;
        private readonly MigrationOperationFactory _operationFactory;
        private readonly MigrationOperationProcessor _operationProcessor;
        private MigrationOperationCollection _operations;

        public ModelDiffer(
            [NotNull] IRelationalMetadataExtensionProvider extensionProvider,
            [NotNull] RelationalTypeMapper typeMapper,
            [NotNull] MigrationOperationFactory operationFactory,
            [NotNull] MigrationOperationProcessor operationProcessor)
        {
            Check.NotNull(extensionProvider, "extensionProvider");
            Check.NotNull(typeMapper, "typeMapper");
            Check.NotNull(operationFactory, "operationFactory");
            Check.NotNull(operationProcessor, "operationProcessor");

            _extensionProvider = extensionProvider;
            _typeMapper = typeMapper;
            _operationFactory = operationFactory;
            _operationProcessor = operationProcessor;
        }

        public virtual IRelationalMetadataExtensionProvider ExtensionProvider
        {
            get { return _extensionProvider; }
        }

        public virtual RelationalNameBuilder NameBuilder
        {
            get { return ExtensionProvider.NameBuilder; }
        }

        public virtual RelationalTypeMapper TypeMapper
        {
            get { return _typeMapper; }
        }

        public virtual MigrationOperationFactory OperationFactory
        {
            get { return _operationFactory; }
        }

        public virtual MigrationOperationProcessor OperationProcessor
        {
            get { return _operationProcessor; }
        }

        public virtual IReadOnlyList<MigrationOperation> CreateSchema([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            _operations = new MigrationOperationCollection();

            _operations.AddRange(GetSequences(model)
                .Select(s => OperationFactory.CreateSequenceOperation(s)));

            _operations.AddRange(model.EntityTypes
                .Select(t => OperationFactory.CreateTableOperation(t)));

            // TODO: GitHub#1107
            _operations.AddRange(_operations.Get<CreateTableOperation>()
                .SelectMany(o => o.ForeignKeys));

            _operations.AddRange(_operations.Get<CreateTableOperation>()
                .SelectMany(o => o.Indexes));

            return OperationProcessor.Process(_operations, new Model(), model);
        }

        public virtual IReadOnlyList<MigrationOperation> DropSchema([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            _operations = new MigrationOperationCollection();

            _operations.AddRange(GetSequences(model).Select(
                s => OperationFactory.DropSequenceOperation(s)));

            _operations.AddRange(model.EntityTypes.Select(
                t => OperationFactory.DropTableOperation(t)));

            return OperationProcessor.Process(_operations, model, new Model());
        }

        public virtual IReadOnlyList<MigrationOperation> Diff([NotNull] IModel sourceModel, [NotNull] IModel targetModel)
        {
            Check.NotNull(sourceModel, "sourceModel");
            Check.NotNull(targetModel, "targetModel");

            _operations = new MigrationOperationCollection();

            var modelPair = Tuple.Create(sourceModel, targetModel);
            Dictionary<IProperty, IProperty> columnMap;
            DiffTables(modelPair, out columnMap);
            DiffSequences(modelPair, columnMap);            

            // TODO: Add more unit tests for the operation order.

            HandleTransitiveRenames();

            return OperationProcessor.Process(_operations, sourceModel, targetModel);
        }

        private void DiffTables(Tuple<IModel, IModel> modelPair, out Dictionary<IProperty, IProperty> columnMap)
        {
            var tablePairs = FindEntityTypePairs(modelPair);
            var columnPairs = new IReadOnlyList<Tuple<IProperty, IProperty>>[tablePairs.Count];
            columnMap = new Dictionary<IProperty, IProperty>();

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
            FindCreatedTables(modelPair, tablePairs);
            FindDroppedTables(modelPair, tablePairs);

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

        private void DiffSequences(Tuple<IModel, IModel> modelPair, Dictionary<IProperty, IProperty> columnMap)
        {
            var sourceSequences = GetSequences(modelPair.Item1);
            var targetSequences = GetSequences(modelPair.Item2);

            var sequencePairs = FindSequencePairs(columnMap);

            FindMovedSequences(sequencePairs);
            FindRenamedSequences(sequencePairs);
            FindCreatedSequences(sequencePairs, targetSequences);
            FindDroppedSequences(sequencePairs, sourceSequences);
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

        private IReadOnlyList<Tuple<IEntityType, IEntityType>> FindEntityTypePairs(
            Tuple<IModel, IModel> modelPair)
        {
            var simpleMatchPairs =
                (from et1 in modelPair.Item1.EntityTypes
                 from et2 in modelPair.Item2.EntityTypes
                    where SimpleMatchEntityTypes(et1, et2)
                    select Tuple.Create(et1, et2))
                    .ToList();

            var fuzzyMatchPairs =
                from et1 in modelPair.Item1.EntityTypes.Except(simpleMatchPairs.Select(p => p.Item1))
                from et2 in modelPair.Item2.EntityTypes.Except(simpleMatchPairs.Select(p => p.Item2))
                where FuzzyMatchEntityTypes(et1, et2)
                select Tuple.Create(et1, et2);

            return simpleMatchPairs.Concat(fuzzyMatchPairs).ToList();
        }

        private void FindMovedTables(
            IEnumerable<Tuple<IEntityType, IEntityType>> tablePairs)
        {
            _operations.AddRange(
                tablePairs
                    .Where(pair => !MatchTableSchemas(pair.Item1, pair.Item2))
                    .Select(pair => OperationFactory.MoveTableOperation(pair.Item1, pair.Item2)));
        }

        private void FindRenamedTables(
            IEnumerable<Tuple<IEntityType, IEntityType>> tablePairs)
        {
            _operations.AddRange(
                tablePairs
                    .Where(pair => !MatchTableNames(pair.Item1, pair.Item2))
                    .Select(pair => OperationFactory.RenameTableOperation(pair.Item1, pair.Item2)));
        }

        private void FindCreatedTables(
            Tuple<IModel, IModel> modelPair,
            IEnumerable<Tuple<IEntityType, IEntityType>> tablePairs)
        {
            var tables =
                modelPair.Item2.EntityTypes
                    .Except(tablePairs.Select(p => p.Item2))
                    .ToList();

            _operations.AddRange(
                tables.Select(t => OperationFactory.CreateTableOperation(t)));

            // TODO: GitHub#1107
            _operations.AddRange(_operations.Get<CreateTableOperation>()
                .SelectMany(o => o.ForeignKeys));

            _operations.AddRange(_operations.Get<CreateTableOperation>()
                .SelectMany(o => o.Indexes));
        }

        private void FindDroppedTables(
            Tuple<IModel, IModel> modelPair,
            IEnumerable<Tuple<IEntityType, IEntityType>> tablePairs)
        {
            _operations.AddRange(
                modelPair.Item1.EntityTypes
                    .Except(tablePairs.Select(p => p.Item1))
                    .Select(t => OperationFactory.DropTableOperation(t)));
        }

        private IReadOnlyList<Tuple<IProperty, IProperty>> FindColumnPairs(
            Tuple<IEntityType, IEntityType> tablePair)
        {
            var simplePropertyMatchPairs =
                (from p1 in tablePair.Item1.Properties
                    from p2 in tablePair.Item2.Properties
                    where MatchPropertyNames(p1, p2)
                    select Tuple.Create(p1, p2))
                    .ToList();

            var simpleColumnMatchPairs =
                from p1 in tablePair.Item1.Properties.Except(simplePropertyMatchPairs.Select(p => p.Item1))
                from p2 in tablePair.Item2.Properties.Except(simplePropertyMatchPairs.Select(p => p.Item2))
                where MatchColumnNames(p1, p2)
                select Tuple.Create(p1, p2);

            return simplePropertyMatchPairs.Concat(simpleColumnMatchPairs).ToList();
        }

        private void FindRenamedColumns(
            IEnumerable<Tuple<IProperty, IProperty>> columnPairs)
        {
            _operations.AddRange(
                columnPairs
                    .Where(pair => !MatchColumnNames(pair.Item1, pair.Item2))
                    .Select(pair => OperationFactory.RenameColumnOperation(pair.Item1, pair.Item2)));
        }

        private void FindAddedColumns(
            Tuple<IEntityType, IEntityType> tablePair,
            IEnumerable<Tuple<IProperty, IProperty>> columnPairs)
        {
            _operations.AddRange(
                tablePair.Item2.Properties
                    .Except(columnPairs.Select(pair => pair.Item2))
                    .Select(c => OperationFactory.AddColumnOperation(c)));
        }

        private void FindDroppedColumns(
            Tuple<IEntityType, IEntityType> tablePair,
            IEnumerable<Tuple<IProperty, IProperty>> columnPairs)
        {
            _operations.AddRange(
                tablePair.Item1.Properties
                    .Except(columnPairs.Select(pair => pair.Item1))
                    .Select(c => OperationFactory.DropColumnOperation(c)));
        }

        private void FindAlteredColumns(
            IEnumerable<Tuple<IProperty, IProperty>> columnPairs)
        {
            _operations.AddRange(
                columnPairs.Where(pair => !EquivalentColumns(pair.Item1, pair.Item2))
                    .Select(pair => OperationFactory.AlterColumnOperation(pair.Item1, pair.Item2, isDestructiveChange: true)));

            // TODO: Add functionality to determine the value of isDestructiveChange.
        }

        private void FindPrimaryKeyChanges(
            Tuple<IEntityType, IEntityType> tablePair,
            IDictionary<IProperty, IProperty> columnMap)
        {
            var sourcePrimaryKey = tablePair.Item1.TryGetPrimaryKey();
            var targetPrimaryKey = tablePair.Item2.TryGetPrimaryKey();

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
            else if (!EquivalentPrimaryKeys(sourcePrimaryKey, targetPrimaryKey, columnMap))
            {
                DropPrimaryKey(sourcePrimaryKey);
                AddPrimaryKey(targetPrimaryKey);
            }
        }

        private void AddPrimaryKey(IKey key)
        {
            _operations.Add(OperationFactory.AddPrimaryKeyOperation(key));
        }

        private void DropPrimaryKey(IKey key)
        {
            _operations.Add(OperationFactory.DropPrimaryKeyOperation(key));
        }

        private IReadOnlyList<Tuple<IKey, IKey>> FindUniqueConstraintPairs(
            Tuple<IEntityType, IEntityType> table,
            IDictionary<IProperty, IProperty> columnMap)
        {
            var pk1 = table.Item1.TryGetPrimaryKey();
            var pk2 = table.Item2.TryGetPrimaryKey();

            return
                (from uc1 in table.Item1.Keys.Where(k => k != pk1)
                    from uc2 in table.Item2.Keys.Where(k => k != pk2)
                    where EquivalentUniqueConstraints(uc1, uc2, columnMap)
                    select Tuple.Create(uc1, uc2))
                    .ToList();
        }

        private void FindAddedUniqueConstraints(
            Tuple<IEntityType, IEntityType> tablePair,
            IEnumerable<Tuple<IKey, IKey>> uniqueConstraintPairs)
        {
            var pk2 = tablePair.Item2.TryGetPrimaryKey();

            _operations.AddRange(
                tablePair.Item2.Keys.Where(k => k != pk2)
                    .Except(uniqueConstraintPairs.Select(pair => pair.Item2))
                    .Select(uc => OperationFactory.AddUniqueConstraintOperation(uc)));
        }

        private void FindDroppedUniqueConstraints(
            Tuple<IEntityType, IEntityType> tablePair,
            IEnumerable<Tuple<IKey, IKey>> uniqueConstraintPairs)
        {
            var pk1 = tablePair.Item1.TryGetPrimaryKey();

            _operations.AddRange(
                tablePair.Item1.Keys.Where(k => k != pk1)
                    .Except(uniqueConstraintPairs.Select(pair => pair.Item1))
                    .Select(uc => OperationFactory.DropUniqueConstraintOperation(uc)));
        }

        private IReadOnlyList<Tuple<IForeignKey, IForeignKey>> FindForeignKeyPairs(
            Tuple<IEntityType, IEntityType> table,
            IDictionary<IProperty, IProperty> columnMap)
        {
            return
                (from fk1 in table.Item1.ForeignKeys
                    from fk2 in table.Item2.ForeignKeys
                    where EquivalentForeignKeys(fk1, fk2, columnMap)
                    select Tuple.Create(fk1, fk2))
                    .ToList();
        }

        private void FindAddedForeignKeys(
            Tuple<IEntityType, IEntityType> tablePair,
            IEnumerable<Tuple<IForeignKey, IForeignKey>> foreignKeyPairs)
        {
            _operations.AddRange(
                tablePair.Item2.ForeignKeys
                    .Except(foreignKeyPairs.Select(pair => pair.Item2))
                    .Select(fk => OperationFactory.AddForeignKeyOperation(fk)));
        }

        private void FindDroppedForeignKeys(
            Tuple<IEntityType, IEntityType> tablePair,
            IEnumerable<Tuple<IForeignKey, IForeignKey>> foreignKeyPairs)
        {
            _operations.AddRange(
                tablePair.Item1.ForeignKeys
                    .Except(foreignKeyPairs.Select(pair => pair.Item1))
                    .Select(fk => OperationFactory.DropForeignKeyOperation(fk)));
        }

        private IReadOnlyList<Tuple<IIndex, IIndex>> FindIndexPairs(
            Tuple<IEntityType, IEntityType> tablePair,
            IDictionary<IProperty, IProperty> columnMap)
        {
            return
                (from ix1 in tablePair.Item1.Indexes
                    from ix2 in tablePair.Item2.Indexes
                    where EquivalentIndexes(ix1, ix2, columnMap)
                    select Tuple.Create(ix1, ix2))
                    .ToList();
        }

        private void FindRenamedIndexes(
            IEnumerable<Tuple<IIndex, IIndex>> indexPairs)
        {
            _operations.AddRange(
                indexPairs
                    .Where(pair => !MatchIndexNames(pair.Item1, pair.Item2))
                    .Select(pair => OperationFactory.RenameIndexOperation(pair.Item1, pair.Item2)));
        }

        private void FindCreatedIndexes(
            Tuple<IEntityType, IEntityType> tablePair,
            IEnumerable<Tuple<IIndex, IIndex>> indexPairs)
        {
            _operations.AddRange(
                tablePair.Item2.Indexes
                    .Except(indexPairs.Select(pair => pair.Item2))
                    .Select(ix => OperationFactory.CreateIndexOperation(ix)));
        }

        private void FindDroppedIndexes(
            Tuple<IEntityType, IEntityType> tablePair,
            IEnumerable<Tuple<IIndex, IIndex>> indexPairs)
        {
            _operations.AddRange(
                tablePair.Item1.Indexes
                    .Except(indexPairs.Select(pair => pair.Item1))
                    .Select(ix => OperationFactory.DropIndexOperation(ix)));
        }

        private IReadOnlyList<Tuple<ISequence, ISequence>> FindSequencePairs(
            Dictionary<IProperty, IProperty> columnMap)
        {
            return
                (from pair in columnMap
                    let sourceSequence = TryGetSequence(pair.Key)
                    where sourceSequence != null
                    let targetSequence = TryGetSequence(pair.Value)
                    where targetSequence != null
                    where MatchSequences(sourceSequence, targetSequence)
                    select Tuple.Create(sourceSequence, targetSequence))
                    .Distinct((x, y) => MatchSequenceNames(x.Item1, y.Item1) && MatchSequenceSchemas(x.Item1, y.Item1))
                    .ToList();
        }

        private void FindMovedSequences(IEnumerable<Tuple<ISequence, ISequence>> sequencePairs)
        {
            _operations.AddRange(
                sequencePairs
                    .Where(pair => !MatchSequenceSchemas(pair.Item1, pair.Item2))
                    .Select(pair => OperationFactory.MoveSequenceOperation(pair.Item1, pair.Item2)));
        }

        private void FindRenamedSequences(IEnumerable<Tuple<ISequence, ISequence>> sequencePairs)
        {
            _operations.AddRange(
                sequencePairs
                    .Where(pair => !MatchSequenceNames(pair.Item1, pair.Item2))
                    .Select(pair => OperationFactory.RenameSequenceOperation(pair.Item1, pair.Item2)));
        }

        private void FindCreatedSequences(IEnumerable<Tuple<ISequence, ISequence>> sequencePairs, IEnumerable<ISequence> targetSequences)
        {
            _operations.AddRange(
                targetSequences
                    .Except(sequencePairs.Select(p => p.Item2), (x, y) => MatchSequenceNames(x, y) && MatchSequenceSchemas(x, y))
                    .Select(s => OperationFactory.CreateSequenceOperation(s)));
        }

        private void FindDroppedSequences(IEnumerable<Tuple<ISequence, ISequence>> sequencePairs, IEnumerable<ISequence> sourceSequences)
        {
            _operations.AddRange(
                sourceSequences
                    .Except(sequencePairs.Select(p => p.Item1), (x, y) => MatchSequenceNames(x, y) && MatchSequenceSchemas(x, y))
                    .Select(s => OperationFactory.DropSequenceOperation(s)));
        }

        private void FindAlteredSequences(IEnumerable<Tuple<ISequence, ISequence>> sequencePairs)
        {
            _operations.AddRange(
                sequencePairs
                    .Where(pair => !EquivalentSequences(pair.Item1, pair.Item2))
                    .Select(pair => OperationFactory.AlterSequenceOperation(pair.Item1, pair.Item2)));
        }

        protected virtual bool SimpleMatchEntityTypes([NotNull] IEntityType source, [NotNull] IEntityType target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return source.Name == target.Name;
        }

        protected virtual bool FuzzyMatchEntityTypes([NotNull] IEntityType source, [NotNull] IEntityType target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            var matchingPropertyCount =
                (from p1 in source.Properties
                 from p2 in target.Properties
                    where MatchProperties(p1, p2)
                    select 1)
                    .Count();

            // At least 80% of properties, across both entities, must be equivalent.
            return (matchingPropertyCount * 2.0f / (source.Properties.Count + target.Properties.Count)) >= 0.80;
        }

        protected virtual bool MatchProperties([NotNull] IProperty source, [NotNull] IProperty target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return
                source.Name == target.Name
                && source.PropertyType == target.PropertyType;
        }

        protected virtual bool MatchPropertyNames([NotNull] IProperty source, [NotNull] IProperty target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return source.Name == target.Name;
        }

        protected virtual bool MatchSequences([NotNull] ISequence source, [NotNull] ISequence target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return source.Type == target.Type;
        }

        protected virtual bool MatchSequenceNames([NotNull] ISequence source, [NotNull] ISequence target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return source.Name == target.Name;
        }

        protected virtual bool MatchSequenceSchemas([NotNull] ISequence source, [NotNull] ISequence target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return source.Schema == target.Schema;
        }

        protected virtual bool MatchTableNames([NotNull] IEntityType source, [NotNull] IEntityType target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return ExtensionProvider.Extensions(source).Table == ExtensionProvider.Extensions(target).Table;
        }

        protected virtual bool MatchTableSchemas([NotNull] IEntityType source, [NotNull] IEntityType target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return ExtensionProvider.Extensions(source).Schema == ExtensionProvider.Extensions(target).Schema;
        }

        protected virtual bool MatchColumnNames([NotNull] IProperty source, [NotNull] IProperty target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return ExtensionProvider.Extensions(source).Column == ExtensionProvider.Extensions(target).Column;
        }

        protected virtual bool MatchIndexNames([NotNull] IIndex source, [NotNull] IIndex target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return ExtensionProvider.Extensions(source).Name == ExtensionProvider.Extensions(target).Name;
        }

        protected virtual bool EquivalentColumns([NotNull] IProperty source, [NotNull] IProperty target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            var sourceExtensions = ExtensionProvider.Extensions(source);
            var targetExtensions = ExtensionProvider.Extensions(target);

            return
                source.PropertyType == target.PropertyType
                && ColumnType(source) == ColumnType(target)
                && sourceExtensions.DefaultValue == targetExtensions.DefaultValue
                && sourceExtensions.DefaultExpression == targetExtensions.DefaultExpression
                && source.IsNullable == target.IsNullable
                && source.GenerateValueOnAdd == target.GenerateValueOnAdd
                && source.IsStoreComputed == target.IsStoreComputed
                && source.IsConcurrencyToken == target.IsConcurrencyToken
                && source.MaxLength == target.MaxLength;
        }

        protected virtual bool EquivalentSequences([NotNull] ISequence source, [NotNull] ISequence target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return
                source.IncrementBy == target.IncrementBy;
        }

        protected virtual bool EquivalentPrimaryKeys(
            [NotNull] IKey sourceKey,
            [NotNull] IKey targetKey,
            [NotNull] IDictionary<IProperty, IProperty> columnMap)
        {
            Check.NotNull(sourceKey, "sourceKey");
            Check.NotNull(targetKey, "targetKey");
            Check.NotNull(columnMap, "columnMap");

            return
                NameBuilder.KeyName(sourceKey) == NameBuilder.KeyName(targetKey)
                && EquivalentColumnReferences(sourceKey.Properties, targetKey.Properties, columnMap);
        }

        protected virtual bool EquivalentUniqueConstraints(
            [NotNull] IKey sourceKey,
            [NotNull] IKey targetKey,
            [NotNull] IDictionary<IProperty, IProperty> columnMap)
        {
            Check.NotNull(sourceKey, "sourceKey");
            Check.NotNull(targetKey, "targetKey");
            Check.NotNull(columnMap, "columnMap");

            return
                NameBuilder.KeyName(sourceKey) == NameBuilder.KeyName(targetKey)
                && EquivalentColumnReferences(sourceKey.Properties, targetKey.Properties, columnMap);
        }

        protected virtual bool EquivalentForeignKeys(
            [NotNull] IForeignKey sourceForeignKey,
            [NotNull] IForeignKey targetForeignKey,
            [NotNull] IDictionary<IProperty, IProperty> columnMap)
        {
            Check.NotNull(sourceForeignKey, "sourceForeignKey");
            Check.NotNull(targetForeignKey, "targetForeignKey");
            Check.NotNull(columnMap, "columnMap");

            return
                NameBuilder.ForeignKeyName(sourceForeignKey) == NameBuilder.ForeignKeyName(targetForeignKey)
                && EquivalentColumnReferences(sourceForeignKey.Properties, targetForeignKey.Properties, columnMap)
                && EquivalentColumnReferences(sourceForeignKey.ReferencedProperties, targetForeignKey.ReferencedProperties, columnMap);
        }

        protected virtual bool EquivalentIndexes(
            [NotNull] IIndex sourceIndex,
            [NotNull] IIndex targetIndex,
            [NotNull] IDictionary<IProperty, IProperty> columnMap)
        {
            Check.NotNull(sourceIndex, "sourceIndex");
            Check.NotNull(targetIndex, "targetIndex");
            Check.NotNull(columnMap, "columnMap");

            return
                sourceIndex.IsUnique == targetIndex.IsUnique
                && EquivalentColumnReferences(sourceIndex.Properties, targetIndex.Properties, columnMap);
        }

        protected virtual bool EquivalentColumnReferences(
            [NotNull] IProperty sourceColumn,
            [NotNull] IProperty targetColumn,
            [NotNull] IDictionary<IProperty, IProperty> columnMap)
        {
            Check.NotNull(sourceColumn, "sourceColumn");
            Check.NotNull(targetColumn, "targetColumn");
            Check.NotNull(columnMap, "columnMap");

            IProperty column;
            return columnMap.TryGetValue(sourceColumn, out column) && ReferenceEquals(column, targetColumn);
        }

        protected virtual bool EquivalentColumnReferences(
            [NotNull] IReadOnlyList<IProperty> sourceColumns,
            [NotNull] IReadOnlyList<IProperty> targetColumns,
            [NotNull] IDictionary<IProperty, IProperty> columnMap)
        {
            Check.NotNull(sourceColumns, "sourceColumns");
            Check.NotNull(targetColumns, "targetColumns");
            Check.NotNull(columnMap, "columnMap");

            return
                sourceColumns.Count == targetColumns.Count
                && !sourceColumns.Where((t, i) => !EquivalentColumnReferences(t, targetColumns[i], columnMap)).Any();
        }

        protected virtual string ColumnType(IProperty property)
        {
            var extensions = ExtensionProvider.Extensions(property);

            return
                TypeMapper.GetTypeMapping(
                    extensions.ColumnType,
                    extensions.Column,
                    property.PropertyType,
                    property.IsKey() || property.IsForeignKey(),
                    property.IsConcurrencyToken)
                    .StoreTypeName;
        }

        protected virtual ISequence TryGetSequence([NotNull] IProperty property)
        {
            return null;
        }

        protected virtual IReadOnlyList<ISequence> GetSequences([NotNull] IModel model)
        {
            return new ISequence[0];
        }
    }
}
