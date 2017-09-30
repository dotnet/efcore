// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MigrationsModelDiffer : IMigrationsModelDiffer
    {
        private static readonly Type[] _dropOperationTypes =
        {
            typeof(DropIndexOperation),
            typeof(DropPrimaryKeyOperation),
            typeof(DropSequenceOperation),
            typeof(DropUniqueConstraintOperation)
        };

        private static readonly Type[] _alterOperationTypes =
        {
            typeof(AddPrimaryKeyOperation),
            typeof(AddUniqueConstraintOperation),
            typeof(AlterSequenceOperation)
        };

        private static readonly Type[] _renameOperationTypes =
        {
            typeof(RenameColumnOperation),
            typeof(RenameIndexOperation),
            typeof(RenameSequenceOperation)
        };

        private static readonly Type[] _columnOperationTypes =
        {
            typeof(AddColumnOperation),
            typeof(AlterColumnOperation)
        };

        private static readonly Type[] _constraintOperationTypes =
        {
            typeof(AddForeignKeyOperation),
            typeof(CreateIndexOperation)
        };

        private IStateManager _targetStateManager;
        private IStateManager _sourceStateManager;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MigrationsModelDiffer(
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IMigrationsAnnotationProvider migrationsAnnotations,
            [NotNull] IChangeDetector changeDetector,
            [NotNull] StateManagerDependencies stateManagerDependencies,
            [NotNull] CommandBatchPreparerDependencies commandBatchPreparerDependencies)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(migrationsAnnotations, nameof(migrationsAnnotations));
            Check.NotNull(stateManagerDependencies, nameof(stateManagerDependencies));
            Check.NotNull(commandBatchPreparerDependencies, nameof(commandBatchPreparerDependencies));

            TypeMapper = typeMapper;
            MigrationsAnnotations = migrationsAnnotations;
            ChangeDetector = changeDetector;
            StateManagerStateManagerDependencies = stateManagerDependencies;
            CommandBatchPreparerDependencies = commandBatchPreparerDependencies;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IRelationalTypeMapper TypeMapper { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IMigrationsAnnotationProvider MigrationsAnnotations { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual StateManagerDependencies StateManagerStateManagerDependencies { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual CommandBatchPreparerDependencies CommandBatchPreparerDependencies { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IChangeDetector ChangeDetector { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasDifferences(IModel source, IModel target)
            => Diff(source, target, new DiffContext(source, target)).Any();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<MigrationOperation> GetDifferences(IModel source, IModel target)
        {
            var diffContext = new DiffContext(source, target);
            return Sort(Diff(source, target, diffContext), diffContext);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IReadOnlyList<MigrationOperation> Sort(
            [NotNull] IEnumerable<MigrationOperation> operations,
            [NotNull] DiffContext diffContext)
        {
            Check.NotNull(operations, nameof(operations));

            var dropForeignKeyOperations = new List<MigrationOperation>();
            var dropOperations = new List<MigrationOperation>();
            var dropColumnOperations = new List<MigrationOperation>();
            var dropTableOperations = new List<DropTableOperation>();
            var ensureSchemaOperations = new List<MigrationOperation>();
            var createSequenceOperations = new List<MigrationOperation>();
            var createTableOperations = new List<CreateTableOperation>();
            var alterDatabaseOperations = new List<MigrationOperation>();
            var alterTableOperations = new List<MigrationOperation>();
            var columnOperations = new List<MigrationOperation>();
            var computedColumnOperations = new List<MigrationOperation>();
            var alterOperations = new List<MigrationOperation>();
            var restartSequenceOperations = new List<MigrationOperation>();
            var constraintOperations = new List<MigrationOperation>();
            var renameOperations = new List<MigrationOperation>();
            var renameTableOperations = new List<MigrationOperation>();
            var sourceDataOperations = new List<MigrationOperation>();
            var targetDataOperations = new List<MigrationOperation>();
            var leftovers = new List<MigrationOperation>();

            foreach (var operation in operations)
            {
                var type = operation.GetType();
                if (type == typeof(DropForeignKeyOperation))
                {
                    dropForeignKeyOperations.Add(operation);
                }
                else if (_dropOperationTypes.Contains(type))
                {
                    dropOperations.Add(operation);
                }
                else if (type == typeof(DropColumnOperation))
                {
                    dropColumnOperations.Add(operation);
                }
                else if (type == typeof(DropTableOperation))
                {
                    dropTableOperations.Add((DropTableOperation)operation);
                }
                else if (type == typeof(EnsureSchemaOperation))
                {
                    ensureSchemaOperations.Add(operation);
                }
                else if (type == typeof(CreateSequenceOperation))
                {
                    createSequenceOperations.Add(operation);
                }
                else if (type == typeof(CreateTableOperation))
                {
                    createTableOperations.Add((CreateTableOperation)operation);
                }
                else if (type == typeof(AlterDatabaseOperation))
                {
                    alterDatabaseOperations.Add(operation);
                }
                else if (type == typeof(AlterTableOperation))
                {
                    alterTableOperations.Add(operation);
                }
                else if (_columnOperationTypes.Contains(type))
                {
                    if (string.IsNullOrWhiteSpace(((ColumnOperation)operation).ComputedColumnSql))
                    {
                        columnOperations.Add(operation);
                    }
                    else
                    {
                        computedColumnOperations.Add(operation);
                    }
                }
                else if (_alterOperationTypes.Contains(type))
                {
                    alterOperations.Add(operation);
                }
                else if (type == typeof(RestartSequenceOperation))
                {
                    restartSequenceOperations.Add(operation);
                }
                else if (_constraintOperationTypes.Contains(type))
                {
                    constraintOperations.Add(operation);
                }
                else if (_renameOperationTypes.Contains(type))
                {
                    renameOperations.Add(operation);
                }
                else if (type == typeof(RenameTableOperation))
                {
                    renameTableOperations.Add(operation);
                }
                else if (type == typeof(DeleteDataOperation))
                {
                    sourceDataOperations.Add(operation);
                }
                else if (type == typeof(InsertDataOperation)
                    || type == typeof(UpdateDataOperation))
                {
                    targetDataOperations.Add(operation);
                }
                else
                {
                    Debug.Assert(false, "Unexpected operation type: " + operation.GetType());
                    leftovers.Add(operation);
                }
            }

            var createTableGraph = new Multigraph<CreateTableOperation, AddForeignKeyOperation>();
            createTableGraph.AddVertices(createTableOperations);
            foreach (var createTableOperation in createTableOperations)
            {
                foreach (var addForeignKeyOperation in createTableOperation.ForeignKeys)
                {
                    if (addForeignKeyOperation.Table == addForeignKeyOperation.PrincipalTable
                        && addForeignKeyOperation.Schema == addForeignKeyOperation.PrincipalSchema)
                    {
                        continue;
                    }

                    var principalCreateTableOperation = createTableOperations.FirstOrDefault(
                        o => o.Name == addForeignKeyOperation.PrincipalTable
                             && o.Schema == addForeignKeyOperation.PrincipalSchema);
                    if (principalCreateTableOperation != null)
                    {
                        createTableGraph.AddEdge(principalCreateTableOperation, createTableOperation, addForeignKeyOperation);
                    }
                }
            }
            createTableOperations = createTableGraph.TopologicalSort(
                (principalCreateTableOperation, createTableOperation, cyclicAddForeignKeyOperations) =>
                    {
                        foreach (var cyclicAddForeignKeyOperation in cyclicAddForeignKeyOperations)
                        {
                            createTableOperation.ForeignKeys.Remove(cyclicAddForeignKeyOperation);
                            constraintOperations.Add(cyclicAddForeignKeyOperation);
                        }

                        return true;
                    }).ToList();

            var dropTableGraph = new Multigraph<DropTableOperation, IForeignKey>();
            dropTableGraph.AddVertices(dropTableOperations);
            foreach (var dropTableOperation in dropTableOperations)
            {
                var table = diffContext.FindTable(dropTableOperation);
                foreach (var foreignKey in table.GetForeignKeys())
                {
                    var principalRootEntityType = foreignKey.PrincipalEntityType;
                    var principalDropTableOperation = diffContext.FindDrop(principalRootEntityType);
                    if (principalDropTableOperation != null
                        && principalDropTableOperation != dropTableOperation)
                    {
                        dropTableGraph.AddEdge(dropTableOperation, principalDropTableOperation, foreignKey);
                    }
                }
            }
            var newDiffContext = new DiffContext(null, null);
            dropTableOperations = dropTableGraph.TopologicalSort(
                (dropTableOperation, principalDropTableOperation, foreignKeys) =>
                    {
                        dropForeignKeyOperations.AddRange(foreignKeys.SelectMany(c => Remove(c, newDiffContext)));

                        return true;
                    }).ToList();

            return dropForeignKeyOperations
                .Concat(dropTableOperations)
                .Concat(dropOperations)
                .Concat(sourceDataOperations)
                .Concat(dropColumnOperations)
                .Concat(ensureSchemaOperations)
                .Concat(renameTableOperations)
                .Concat(renameOperations)
                .Concat(alterDatabaseOperations)
                .Concat(createSequenceOperations)
                .Concat(alterTableOperations)
                .Concat(columnOperations)
                .Concat(computedColumnOperations)
                .Concat(alterOperations)
                .Concat(restartSequenceOperations)
                .Concat(createTableOperations)
                .Concat(targetDataOperations)
                .Concat(constraintOperations)
                .Concat(leftovers)
                .ToList();
        }

        #region IModel

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [CanBeNull] IModel source,
            [CanBeNull] IModel target,
            [NotNull] DiffContext diffContext)
        {
            _sourceStateManager = source == null ? null : new StateManager(StateManagerStateManagerDependencies.With(source));
            _targetStateManager = target == null ? null : new StateManager(StateManagerStateManagerDependencies.With(target));

            var schemaOperations = source != null && target != null
                ? DiffAnnotations(source, target)
                    .Concat(Diff(GetSchemas(source), GetSchemas(target)))
                    .Concat(Diff(diffContext.GetSourceTables(), diffContext.GetTargetTables(), diffContext))
                    .Concat(Diff(source.Relational().Sequences, target.Relational().Sequences))
                    .Concat(
                        Diff(
                            diffContext.GetSourceTables().SelectMany(s => s.GetForeignKeys()),
                            diffContext.GetTargetTables().SelectMany(t => t.GetForeignKeys()),
                            diffContext))
                : target != null
                    ? Add(target, diffContext)
                    : source != null
                        ? Remove(source, diffContext)
                        : Enumerable.Empty<MigrationOperation>();

            return schemaOperations.Concat(DiffSeedData());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> DiffSeedData()
        {
            foreach (var stateManager in new[] { _sourceStateManager, _targetStateManager })
            {
                if (stateManager == null)
                {
                    continue;
                }

                ChangeDetector.DetectChanges(stateManager);
                var entries = stateManager.GetEntriesToSave();
                if (entries == null
                    || entries.Count == 0)
                {
                    continue;
                }

                var batchCommands = new CommandBatchPreparer(CommandBatchPreparerDependencies.With(() => stateManager))
                    .BatchCommands(entries)
                    .SelectMany(o => o.ModificationCommands);

                foreach (var c in batchCommands)
                {
                    if (c.EntityState == EntityState.Added)
                    {
                        yield return new InsertDataOperation
                        {
                            Schema = c.Schema,
                            Table = c.TableName,
                            Columns = c.ColumnModifications.Select(col => col.ColumnName).ToArray(),
                            Values = ToMultidimensionalArray(c.ColumnModifications.Select(col => col.Value).ToList())
                        };
                    }
                    else if (c.EntityState == EntityState.Modified)
                    {
                        yield return new UpdateDataOperation
                        {
                            Schema = c.Schema,
                            Table = c.TableName,
                            KeyColumns = c.ColumnModifications.Where(col => col.IsKey).Select(col => col.ColumnName).ToArray(),
                            KeyValues = ToMultidimensionalArray(c.ColumnModifications.Where(col => col.IsKey).Select(col => col.Value).ToList()),
                            Columns = c.ColumnModifications.Where(col => !col.IsKey).Select(col => col.ColumnName).ToArray(),
                            Values = ToMultidimensionalArray(c.ColumnModifications.Where(col => !col.IsKey).Select(col => col.Value).ToList())
                        };
                    }
                    else
                    {
                        yield return new DeleteDataOperation
                        {
                            Schema = c.Schema,
                            Table = c.TableName,
                            KeyColumns = c.ColumnModifications.Select(col => col.ColumnName).ToArray(),
                            KeyValues = ToMultidimensionalArray(c.ColumnModifications.Select(col => col.Value).ToArray())
                        };
                    }
                }
            }
        }

        private IEnumerable<MigrationOperation> DiffAnnotations(
            IModel source,
            IModel target)
        {
            var sourceMigrationsAnnotations = source == null ? null : MigrationsAnnotations.For(source).ToList();
            var targetMigrationsAnnotations = target == null ? null : MigrationsAnnotations.For(target).ToList();

            if (source == null)
            {
                if (targetMigrationsAnnotations != null
                    && targetMigrationsAnnotations.Count > 0)
                {
                    var alterDatabaseOperation = new AlterDatabaseOperation();
                    alterDatabaseOperation.AddAnnotations(targetMigrationsAnnotations);
                    yield return alterDatabaseOperation;
                }
                yield break;
            }

            if (target == null)
            {
                sourceMigrationsAnnotations = MigrationsAnnotations.ForRemove(source).ToList();
                if (sourceMigrationsAnnotations.Count > 0)
                {
                    var alterDatabaseOperation = new AlterDatabaseOperation();
                    alterDatabaseOperation.OldDatabase.AddAnnotations(MigrationsAnnotations.ForRemove(source));
                    yield return alterDatabaseOperation;
                }
                yield break;
            }

            if (HasDifferences(sourceMigrationsAnnotations, targetMigrationsAnnotations))
            {
                var alterDatabaseOperation = new AlterDatabaseOperation();
                alterDatabaseOperation.AddAnnotations(targetMigrationsAnnotations);
                alterDatabaseOperation.OldDatabase.AddAnnotations(sourceMigrationsAnnotations);
                yield return alterDatabaseOperation;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add([NotNull] IModel target, [NotNull] DiffContext diffContext)
            => DiffAnnotations(null, target)
                .Concat(GetSchemas(target).SelectMany(Add))
                .Concat(diffContext.GetTargetTables().SelectMany(t => Add(t, diffContext)))
                .Concat(target.Relational().Sequences.SelectMany(Add))
                .Concat(diffContext.GetTargetTables().SelectMany(t => t.GetForeignKeys()).SelectMany(k => Add(k, diffContext)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] IModel source, [NotNull] DiffContext diffContext)
            => DiffAnnotations(source, null)
                .Concat(diffContext.GetSourceTables().SelectMany(t => Remove(t, diffContext)))
                .Concat(source.Relational().Sequences.SelectMany(Remove));

        #endregion

        #region Schema

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff([NotNull] IEnumerable<string> source, [NotNull] IEnumerable<string> target)
            => DiffCollection(
                source,
                target,
                Diff,
                Add,
                Remove,
                (s, t) => s == t);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff([NotNull] string source, [NotNull] string target)
            => Enumerable.Empty<MigrationOperation>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add([NotNull] string target)
        {
            yield return new EnsureSchemaOperation { Name = target };
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] string source)
            => Enumerable.Empty<MigrationOperation>();

        #endregion

        #region IEntityType

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<TableMapping> source,
            [NotNull] IEnumerable<TableMapping> target,
            [NotNull] DiffContext diffContext)
            => DiffCollection(
                source,
                target,
                (s, t) =>
                    {
                        diffContext.AddMapping(s, t);

                        return Diff(s, t, diffContext);
                    },
                t => Add(t, diffContext),
                s => Remove(s, diffContext),
                (s, t) => s.EntityTypes.Any(se => t.EntityTypes.Any(te => string.Equals(se.Name, te.Name, StringComparison.OrdinalIgnoreCase))),
                (s, t) => string.Equals(
                              s.Schema,
                              t.Schema,
                              StringComparison.OrdinalIgnoreCase)
                          && string.Equals(
                              s.Name,
                              t.Name,
                              StringComparison.OrdinalIgnoreCase),
                (s, t) => string.Equals(
                    s.Name,
                    t.Name,
                    StringComparison.OrdinalIgnoreCase));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] TableMapping source,
            [NotNull] TableMapping target,
            [NotNull] DiffContext diffContext)
        {
            var schemaChanged = source.Schema != target.Schema;
            var renamed = source.Name != target.Name;
            if (schemaChanged || renamed)
            {
                yield return new RenameTableOperation
                {
                    Schema = source.Schema,
                    Name = source.Name,
                    NewSchema = schemaChanged ? target.Schema : null,
                    NewName = renamed ? target.Name : null
                };
            }

            var operations = DiffAnnotations(source, target)
                .Concat(Diff(source.GetProperties(), target.GetProperties(), diffContext))
                .Concat(Diff(source.GetKeys(), target.GetKeys(), diffContext))
                .Concat(Diff(source.GetIndexes(), target.GetIndexes(), diffContext));
            foreach (var operation in operations)
            {
                yield return operation;
            }

            TrackSeeds(source, target, diffContext);
        }

        private IEnumerable<MigrationOperation> DiffAnnotations(
            [NotNull] TableMapping source,
            [NotNull] TableMapping target)
        {
            // Validation should ensure that all the relevant annotations for the colocated entity types are the same
            var sourceMigrationsAnnotations = MigrationsAnnotations.For(source.EntityTypes[0]).ToList();
            var targetMigrationsAnnotations = MigrationsAnnotations.For(target.EntityTypes[0]).ToList();
            if (HasDifferences(sourceMigrationsAnnotations, targetMigrationsAnnotations))
            {
                var alterTableOperation = new AlterTableOperation
                {
                    Name = target.Name,
                    Schema = target.Schema
                };
                alterTableOperation.AddAnnotations(targetMigrationsAnnotations);

                alterTableOperation.OldTable.AddAnnotations(sourceMigrationsAnnotations);
                yield return alterTableOperation;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add(
            [NotNull] TableMapping target, [NotNull] DiffContext diffContext)
        {
            var createTableOperation = new CreateTableOperation
            {
                Schema = target.Schema,
                Name = target.Name
            };
            createTableOperation.AddAnnotations(MigrationsAnnotations.For(target.EntityTypes[0]));

            createTableOperation.Columns.AddRange(
                target.GetProperties().SelectMany(p => Add(p, diffContext, inline: true)).Cast<AddColumnOperation>());
            var primaryKey = target.EntityTypes[0].FindPrimaryKey();
            createTableOperation.PrimaryKey = Add(primaryKey, diffContext).Cast<AddPrimaryKeyOperation>().Single();
            createTableOperation.UniqueConstraints.AddRange(
                target.GetKeys().Where(k => !k.IsPrimaryKey()).SelectMany(k => Add(k, diffContext))
                    .Cast<AddUniqueConstraintOperation>());

            foreach (var targetEntityType in target.EntityTypes)
            {
                diffContext.AddCreate(targetEntityType, createTableOperation);
            }

            yield return createTableOperation;

            foreach (var operation in target.GetIndexes().SelectMany(i => Add(i, diffContext)))
            {
                yield return operation;
            }

            TrackSeeds(null, target, diffContext);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove(
            [NotNull] TableMapping source, [NotNull] DiffContext diffContext)
        {
            var operation = new DropTableOperation
            {
                Schema = source.Schema,
                Name = source.Name
            };
            operation.AddAnnotations(MigrationsAnnotations.ForRemove(source.EntityTypes[0]));

            diffContext.AddDrop(source, operation);

            yield return operation;
        }

        #endregion

        #region IProperty

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<IProperty> source,
            [NotNull] IEnumerable<IProperty> target,
            [NotNull] DiffContext diffContext)
            => DiffCollection(
                source,
                target,
                (s, t) =>
                    {
                        diffContext.AddMapping(s, t);

                        return Diff(s, t);
                    },
                t => Add(t, diffContext),
                Remove,
                (s, t) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase),
                (s, t) => string.Equals(
                    s.Relational().ColumnName,
                    t.Relational().ColumnName,
                    StringComparison.OrdinalIgnoreCase),
                (s, t) =>
                    {
                        var sAnnotations = s.Relational();
                        var tAnnotations = t.Relational();

                        return s.ClrType == t.ClrType
                               && s.IsConcurrencyToken == t.IsConcurrencyToken
                               && s.ValueGenerated == t.ValueGenerated
                               && s.GetMaxLength() == t.GetMaxLength()
                               && s.IsColumnNullable() == t.IsColumnNullable()
                               && s.IsUnicode() == t.IsUnicode()
                               && s.GetConfiguredColumnType() == t.GetConfiguredColumnType()
                               && sAnnotations.ComputedColumnSql == tAnnotations.ComputedColumnSql
                               && Equals(sAnnotations.DefaultValue, tAnnotations.DefaultValue)
                               && sAnnotations.DefaultValueSql == tAnnotations.DefaultValueSql;
                    });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff([NotNull] IProperty source, [NotNull] IProperty target)
        {
            var sourceAnnotations = source.Relational();
            var targetEntityTypeAnnotations = target.DeclaringEntityType.RootType().Relational();
            var targetAnnotations = target.Relational();

            if (sourceAnnotations.ColumnName != targetAnnotations.ColumnName)
            {
                yield return new RenameColumnOperation
                {
                    Schema = targetEntityTypeAnnotations.Schema,
                    Table = targetEntityTypeAnnotations.TableName,
                    Name = sourceAnnotations.ColumnName,
                    NewName = targetAnnotations.ColumnName
                };
            }

            var sourceColumnType = sourceAnnotations.ColumnType
                                   ?? TypeMapper.GetMapping(source).StoreType;
            var targetColumnType = targetAnnotations.ColumnType
                                   ?? TypeMapper.GetMapping(target).StoreType;

            var sourceMigrationsAnnotations = MigrationsAnnotations.For(source).ToList();
            var targetMigrationsAnnotations = MigrationsAnnotations.For(target).ToList();

            var isSourceColumnNullable = source.IsColumnNullable();
            var isTargetColumnNullable = target.IsColumnNullable();
            var isNullableChanged = isSourceColumnNullable != isTargetColumnNullable;
            var columnTypeChanged = sourceColumnType != targetColumnType;

            if (isNullableChanged
                || columnTypeChanged
                || sourceAnnotations.DefaultValueSql != targetAnnotations.DefaultValueSql
                || sourceAnnotations.ComputedColumnSql != targetAnnotations.ComputedColumnSql
                || !Equals(sourceAnnotations.DefaultValue, targetAnnotations.DefaultValue)
                || HasDifferences(sourceMigrationsAnnotations, targetMigrationsAnnotations))
            {
                var isDestructiveChange = isNullableChanged && isSourceColumnNullable
                                          // TODO: Detect type narrowing
                                          || columnTypeChanged;

                var alterColumnOperation = new AlterColumnOperation
                {
                    Schema = targetEntityTypeAnnotations.Schema,
                    Table = targetEntityTypeAnnotations.TableName,
                    Name = targetAnnotations.ColumnName,
                    IsDestructiveChange = isDestructiveChange
                };

                Initialize(
                    alterColumnOperation, target, isTargetColumnNullable, targetAnnotations, targetMigrationsAnnotations, true);

                Initialize(
                    alterColumnOperation.OldColumn, source, isSourceColumnNullable, sourceAnnotations, sourceMigrationsAnnotations, true);

                yield return alterColumnOperation;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add(
            [NotNull] IProperty target,
            [NotNull] DiffContext diffContext,
            bool inline = false)
        {
            var targetAnnotations = target.Relational();
            var targetEntityTypeAnnotations = target.DeclaringEntityType.RootType().Relational();

            var operation = new AddColumnOperation
            {
                Schema = targetEntityTypeAnnotations.Schema,
                Table = targetEntityTypeAnnotations.TableName,
                Name = targetAnnotations.ColumnName
            };
            Initialize(operation, target, target.IsColumnNullable(), targetAnnotations, MigrationsAnnotations.For(target), inline);

            yield return operation;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] IProperty source)
        {
            var sourceEntityTypeAnnotations = source.DeclaringEntityType.RootType().Relational();

            var operation = new DropColumnOperation
            {
                Schema = sourceEntityTypeAnnotations.Schema,
                Table = sourceEntityTypeAnnotations.TableName,
                Name = source.Relational().ColumnName
            };
            operation.AddAnnotations(MigrationsAnnotations.ForRemove(source));

            yield return operation;
        }

        private void Initialize(
            ColumnOperation columnOperation,
            IProperty property,
            bool isNullable,
            IRelationalPropertyAnnotations annotations,
            IEnumerable<IAnnotation> migrationsAnnotations,
            bool inline = false)
        {
            columnOperation.ClrType = property.ClrType.UnwrapNullableType().UnwrapEnumType();
            columnOperation.ColumnType = property.GetConfiguredColumnType();
            columnOperation.MaxLength = property.GetMaxLength();
            columnOperation.IsUnicode = property.IsUnicode();
            columnOperation.IsRowVersion = property.ClrType == typeof(byte[])
                                           && property.IsConcurrencyToken
                                           && property.ValueGenerated == ValueGenerated.OnAddOrUpdate;
            columnOperation.IsNullable = isNullable;

            var defaultValue = annotations.DefaultValue;
            columnOperation.DefaultValue = (defaultValue == DBNull.Value ? null : defaultValue)
                                           ?? (inline || isNullable
                                               ? null
                                               : GetDefaultValue(columnOperation.ClrType));

            columnOperation.DefaultValueSql = annotations.DefaultValueSql;
            columnOperation.ComputedColumnSql = annotations.ComputedColumnSql;
            columnOperation.AddAnnotations(migrationsAnnotations);
        }

        #endregion

        #region IKey

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<IKey> source,
            [NotNull] IEnumerable<IKey> target,
            [NotNull] DiffContext diffContext)
            => DiffCollection(
                source,
                target,
                (s, t) => Diff(s, t, diffContext),
                t => Add(t, diffContext),
                s => Remove(s, diffContext),
                (s, t) => s.Relational().Name == t.Relational().Name
                          && s.Properties.Select(p => p.Relational().ColumnName).SequenceEqual(
                              t.Properties.Select(diffContext.FindSourceColumn))
                          && s.IsPrimaryKey() == t.IsPrimaryKey()
                          && !HasDifferences(MigrationsAnnotations.For(s), MigrationsAnnotations.For(t)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IKey source,
            [NotNull] IKey target,
            [NotNull] DiffContext diffContext)
            => Enumerable.Empty<MigrationOperation>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add([NotNull] IKey target, [NotNull] DiffContext diffContext)
        {
            var targetAnnotations = target.Relational();
            var targetEntityTypeAnnotations = target.DeclaringEntityType.RootType().Relational();
            var columns = GetColumns(target.Properties);

            MigrationOperation operation;
            if (target.IsPrimaryKey())
            {
                operation = new AddPrimaryKeyOperation
                {
                    Schema = targetEntityTypeAnnotations.Schema,
                    Table = targetEntityTypeAnnotations.TableName,
                    Name = targetAnnotations.Name,
                    Columns = columns
                };
            }
            else
            {
                operation = new AddUniqueConstraintOperation
                {
                    Schema = targetEntityTypeAnnotations.Schema,
                    Table = targetEntityTypeAnnotations.TableName,
                    Name = targetAnnotations.Name,
                    Columns = columns
                };
            }
            operation.AddAnnotations(MigrationsAnnotations.For(target));

            yield return operation;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove(
            [NotNull] IKey source,
            [NotNull] DiffContext diffContext)
        {
            var sourceAnnotations = source.Relational();
            var sourceEntityTypeAnnotations = source.DeclaringEntityType.RootType().Relational();

            MigrationOperation operation;
            if (source.IsPrimaryKey())
            {
                operation = new DropPrimaryKeyOperation
                {
                    Schema = sourceEntityTypeAnnotations.Schema,
                    Table = sourceEntityTypeAnnotations.TableName,
                    Name = sourceAnnotations.Name
                };
            }
            else
            {
                operation = new DropUniqueConstraintOperation
                {
                    Schema = sourceEntityTypeAnnotations.Schema,
                    Table = sourceEntityTypeAnnotations.TableName,
                    Name = sourceAnnotations.Name
                };
            }
            operation.AddAnnotations(MigrationsAnnotations.ForRemove(source));

            yield return operation;
        }

        #endregion

        #region IForeignKey

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<IForeignKey> source,
            [NotNull] IEnumerable<IForeignKey> target,
            [NotNull] DiffContext diffContext)
            => DiffCollection(
                source,
                target,
                (s, t) => Diff(s, t, diffContext),
                t => Add(t, diffContext),
                s => Remove(s, diffContext),
                (s, t) => s.Relational().Name == t.Relational().Name
                          && s.Properties.Select(p => p.Relational().ColumnName).SequenceEqual(
                              t.Properties.Select(diffContext.FindSourceColumn))
                          && diffContext.FindTarget(diffContext.FindSourceTable(s.PrincipalEntityType))
                          == diffContext.FindTargetTable(t.PrincipalEntityType)
                          && s.PrincipalKey.Properties.Select(p => p.Relational().ColumnName).SequenceEqual(
                              t.PrincipalKey.Properties.Select(diffContext.FindSourceColumn))
                          && s.DeleteBehavior == t.DeleteBehavior
                          && !HasDifferences(MigrationsAnnotations.For(s), MigrationsAnnotations.For(t)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IForeignKey source, [NotNull] IForeignKey target, [NotNull] DiffContext diffContext)
            => Enumerable.Empty<MigrationOperation>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add([NotNull] IForeignKey target, [NotNull] DiffContext diffContext)
        {
            var declaringRootEntityType = target.DeclaringEntityType.RootType();
            var targetEntityTypeAnnotations = declaringRootEntityType.Relational();
            var targetPrincipalEntityTypeAnnotations = target.PrincipalEntityType.RootType().Relational();

            var operation = new AddForeignKeyOperation
            {
                Schema = targetEntityTypeAnnotations.Schema,
                Table = targetEntityTypeAnnotations.TableName,
                Name = target.Relational().Name,
                Columns = GetColumns(target.Properties),
                PrincipalSchema = targetPrincipalEntityTypeAnnotations.Schema,
                PrincipalTable = targetPrincipalEntityTypeAnnotations.TableName,
                PrincipalColumns = GetColumns(target.PrincipalKey.Properties),
                OnDelete = target.DeleteBehavior == DeleteBehavior.Cascade
                    ? ReferentialAction.Cascade
                    : target.DeleteBehavior == DeleteBehavior.SetNull
                        ? ReferentialAction.SetNull
                        : ReferentialAction.Restrict
            };
            operation.AddAnnotations(MigrationsAnnotations.For(target));

            var createTableOperation = diffContext.FindCreate(declaringRootEntityType);
            if (createTableOperation != null)
            {
                createTableOperation.ForeignKeys.Add(operation);
            }
            else
            {
                yield return operation;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] IForeignKey source, [NotNull] DiffContext diffContext)
        {
            var declaringRootEntityType = source.DeclaringEntityType.RootType();
            var sourceEntityTypeAnnotations = declaringRootEntityType.Relational();

            var dropTableOperation = diffContext.FindDrop(declaringRootEntityType);
            if (dropTableOperation == null)
            {
                var operation = new DropForeignKeyOperation
                {
                    Schema = sourceEntityTypeAnnotations.Schema,
                    Table = sourceEntityTypeAnnotations.TableName,
                    Name = source.Relational().Name
                };
                operation.AddAnnotations(MigrationsAnnotations.ForRemove(source));

                yield return operation;
            }
        }

        #endregion

        #region IIndex

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<IIndex> source,
            [NotNull] IEnumerable<IIndex> target,
            [NotNull] DiffContext diffContext)
            => DiffCollection(
                source,
                target,
                (s, t) => Diff(s, t, diffContext),
                t => Add(t, diffContext),
                Remove,
                (s, t) => string.Equals(
                    s.Relational().Name,
                    t.Relational().Name,
                    StringComparison.OrdinalIgnoreCase)
                          && s.IsUnique == t.IsUnique
                          && s.Relational().Filter == t.Relational().Filter
                          && !HasDifferences(MigrationsAnnotations.For(s), MigrationsAnnotations.For(t))
                          && s.Properties.Select(p => p.Relational().ColumnName).SequenceEqual(
                              t.Properties.Select(diffContext.FindSourceColumn)),
                // ReSharper disable once ImplicitlyCapturedClosure
                (s, t) => s.IsUnique == t.IsUnique
                          && s.Relational().Filter == t.Relational().Filter
                          && !HasDifferences(MigrationsAnnotations.For(s), MigrationsAnnotations.For(t))
                          && s.Properties.Select(p => p.Relational().ColumnName).SequenceEqual(
                              t.Properties.Select(diffContext.FindSourceColumn)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IIndex source,
            [NotNull] IIndex target,
            [NotNull] DiffContext diffContext)
        {
            var targetEntityTypeAnnotations = target.DeclaringEntityType.RootType().Relational();
            var sourceName = source.Relational().Name;
            var targetName = target.Relational().Name;

            if (sourceName != targetName)
            {
                yield return new RenameIndexOperation
                {
                    Schema = targetEntityTypeAnnotations.Schema,
                    Table = targetEntityTypeAnnotations.TableName,
                    Name = sourceName,
                    NewName = targetName
                };
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add(
            [NotNull] IIndex target,
            [NotNull] DiffContext diffContext)
        {
            var targetEntityTypeAnnotations = target.DeclaringEntityType.RootType().Relational();

            var operation = new CreateIndexOperation
            {
                Name = target.Relational().Name,
                Schema = targetEntityTypeAnnotations.Schema,
                Table = targetEntityTypeAnnotations.TableName,
                Columns = GetColumns(target.Properties),
                IsUnique = target.IsUnique,
                Filter = target.Relational().Filter
            };
            operation.AddAnnotations(MigrationsAnnotations.For(target));

            yield return operation;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] IIndex source)
        {
            var sourceEntityTypeAnnotations = source.DeclaringEntityType.RootType().Relational();

            var operation = new DropIndexOperation
            {
                Name = source.Relational().Name,
                Schema = sourceEntityTypeAnnotations.Schema,
                Table = sourceEntityTypeAnnotations.TableName
            };
            operation.AddAnnotations(MigrationsAnnotations.ForRemove(source));

            yield return operation;
        }

        #endregion

        #region ISequence

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<ISequence> source,
            [NotNull] IEnumerable<ISequence> target)
            => DiffCollection(
                source,
                target,
                Diff,
                Add,
                Remove,
                (s, t) => string.Equals(s.Schema, t.Schema, StringComparison.OrdinalIgnoreCase)
                          && string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase)
                          && s.ClrType == t.ClrType,
                (s, t) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase)
                          && s.ClrType == t.ClrType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff([NotNull] ISequence source, [NotNull] ISequence target)
        {
            var schemaChanged = source.Schema != target.Schema;
            var renamed = source.Name != target.Name;
            if (schemaChanged || renamed)
            {
                yield return new RenameSequenceOperation
                {
                    Schema = source.Schema,
                    Name = source.Name,
                    NewSchema = schemaChanged ? target.Schema : null,
                    NewName = renamed ? target.Name : null
                };
            }

            if (source.StartValue != target.StartValue)
            {
                yield return new RestartSequenceOperation
                {
                    Schema = target.Schema,
                    Name = target.Name,
                    StartValue = target.StartValue
                };
            }

            var sourceMigrationsAnnotations = MigrationsAnnotations.For(source).ToList();
            var targetMigrationsAnnotations = MigrationsAnnotations.For(target).ToList();

            if (source.IncrementBy != target.IncrementBy
                || source.MaxValue != target.MaxValue
                || source.MinValue != target.MinValue
                || source.IsCyclic != target.IsCyclic
                || HasDifferences(sourceMigrationsAnnotations, targetMigrationsAnnotations))
            {
                var alterSequenceOperation = new AlterSequenceOperation
                {
                    Schema = target.Schema,
                    Name = target.Name
                };
                Initialize(alterSequenceOperation, target, targetMigrationsAnnotations);

                Initialize(alterSequenceOperation.OldSequence, source, sourceMigrationsAnnotations);

                yield return alterSequenceOperation;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add([NotNull] ISequence target)
        {
            var operation = new CreateSequenceOperation
            {
                Schema = target.Schema,
                Name = target.Name,
                ClrType = target.ClrType,
                StartValue = target.StartValue
            };

            yield return Initialize(operation, target, MigrationsAnnotations.For(target));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] ISequence source)
        {
            var operation = new DropSequenceOperation
            {
                Schema = source.Schema,
                Name = source.Name
            };
            operation.AddAnnotations(MigrationsAnnotations.ForRemove(source));

            yield return operation;
        }

        private SequenceOperation Initialize(
            SequenceOperation sequenceOperation,
            ISequence sequence,
            IEnumerable<IAnnotation> migrationsAnnotations)
        {
            sequenceOperation.IncrementBy = sequence.IncrementBy;
            sequenceOperation.MinValue = sequence.MinValue;
            sequenceOperation.MaxValue = sequence.MaxValue;
            sequenceOperation.IsCyclic = sequence.IsCyclic;
            sequenceOperation.AddAnnotations(migrationsAnnotations);

            return sequenceOperation;
        }

        #endregion

        #region SeedData

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void TrackSeeds(
            [CanBeNull] TableMapping source,
            [NotNull] TableMapping target,
            [NotNull] DiffContext diffContext)
        {
            Check.NotNull(target, nameof(target));
            Check.NotNull(diffContext, nameof(diffContext));

            foreach (var targetEntityType in target.EntityTypes)
            {
                foreach (var targetSeed in targetEntityType.GetSeedData())
                {
                    _targetStateManager.GetOrCreateEntry(targetSeed, targetEntityType).SetEntityState(EntityState.Added);
                }
            }

            if (source == null)
            {
                return;
            }

            var targetKeys = target.EntityTypes.SelectMany(Metadata.Internal.EntityTypeExtensions.GetDeclaredKeys).ToList();
            var keyMapping = new Dictionary<IEntityType, Dictionary<IKey, List<string>>>();
            foreach (var sourceEntityType in source.EntityTypes)
            {
                foreach (var targetKey in targetKeys)
                {
                    if (!targetKey.IsPrimaryKey())
                    {
                        continue;
                    }

                    var keyPropertiesMap = new List<string>();
                    foreach (var keyProperty in targetKey.Properties)
                    {
                        var sourceProperty = diffContext.FindCompatibleSource(keyProperty, sourceEntityType);
                        if (sourceProperty == null)
                        {
                            break;
                        }

                        keyPropertiesMap.Add(sourceProperty.Name);
                    }

                    if (keyPropertiesMap.Count == targetKey.Properties.Count)
                    {
                        keyMapping.GetOrAddNew(sourceEntityType)[targetKey] = keyPropertiesMap;
                    }
                }
            }

            foreach (var sourceEntityType in source.EntityTypes)
            {
                foreach (var sourceSeed in sourceEntityType.GetSeedData())
                {
                    var targetEntryFound = false;
                    if (keyMapping.TryGetValue(sourceEntityType, out var targetKeyMap))
                    {
                        foreach (var targetKey in targetKeys)
                        {
                            if (!targetKeyMap.TryGetValue(targetKey, out var keyPropertiesMap))
                            {
                                continue;
                            }

                            var targetKeyValues = new object[keyPropertiesMap.Count];
                            for (var i = 0; i < keyPropertiesMap.Count; i++)
                            {
                                var sourceName = keyPropertiesMap[i];
                                if (!sourceSeed.TryGetValue(sourceName, out var value))
                                {
                                    targetKeyValues = null;
                                    break;
                                }

                                targetKeyValues[i] = value;
                            }

                            if (targetKeyValues == null)
                            {
                                continue;
                            }

                            var entry = _targetStateManager.TryGetEntry(targetKey, targetKeyValues)?.ToEntityEntry();
                            if (entry == null)
                            {
                                continue;
                            }

                            if (entry.State == EntityState.Added)
                            {
                                foreach (var targetProperty in entry.Metadata.GetProperties())
                                {
                                    if (targetProperty.AfterSaveBehavior == PropertySaveBehavior.Save)
                                    {
                                        entry.OriginalValues[targetProperty] = targetProperty.ClrType.GetDefaultValue();
                                    }
                                }

                                entry.State = EntityState.Unchanged;
                            }

                            foreach (var targetProperty in entry.Metadata.GetProperties())
                            {
                                var sourceProperty = diffContext.FindCompatibleSource(targetProperty, sourceEntityType);
                                if (sourceProperty != null
                                    && sourceProperty.AfterSaveBehavior == PropertySaveBehavior.Save
                                    && sourceSeed.TryGetValue(sourceProperty.Name, out var sourceValue))
                                {
                                    entry.OriginalValues[targetProperty] = sourceValue;
                                }
                            }

                            targetEntryFound = true;
                        }
                    }

                    if (!targetEntryFound)
                    {
                        _sourceStateManager.GetOrCreateEntry(sourceSeed, sourceEntityType).SetEntityState(EntityState.Deleted);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> DiffCollection<T>(
            [NotNull] IEnumerable<T> sources,
            [NotNull] IEnumerable<T> targets,
            [NotNull] Func<T, T, IEnumerable<MigrationOperation>> diff,
            [NotNull] Func<T, IEnumerable<MigrationOperation>> add,
            [NotNull] Func<T, IEnumerable<MigrationOperation>> remove,
            [NotNull] params Func<T, T, bool>[] predicates)
        {
            var sourceList = sources.ToList();
            var targetList = targets.ToList();

            foreach (var predicate in predicates)
            {
                for (var i = sourceList.Count - 1; i >= 0; i--)
                {
                    var source = sourceList[i];

                    for (var j = targetList.Count - 1; j >= 0; j--)
                    {
                        var target = targetList[j];

                        if (predicate(source, target))
                        {
                            sourceList.RemoveAt(i);
                            targetList.RemoveAt(j);

                            foreach (var operation in diff(source, target))
                            {
                                yield return operation;
                            }

                            break;
                        }
                    }
                }
            }

            foreach (var source in sourceList)
            {
                foreach (var operation in remove(source))
                {
                    yield return operation;
                }
            }

            foreach (var target in targetList)
            {
                foreach (var operation in add(target))
                {
                    yield return operation;
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual string[] GetColumns([NotNull] IEnumerable<IProperty> properties)
            => properties.Select(p => p.Relational().ColumnName).ToArray();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual bool HasDifferences([NotNull] IEnumerable<IAnnotation> source, [NotNull] IEnumerable<IAnnotation> target)
        {
            var unmatched = new List<IAnnotation>(target);

            foreach (var annotation in source)
            {
                var index = unmatched.FindIndex(a => a.Name == annotation.Name && Equals(a.Value, annotation.Value));
                if (index == -1)
                {
                    return true;
                }

                unmatched.RemoveAt(index);
            }

            return unmatched.Count != 0;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<string> GetSchemas([NotNull] IModel model)
            => model.GetRootEntityTypes().Select(t => t.Relational().Schema)
                .Concat(model.Relational().Sequences.Select(s => s.Schema))
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual object GetDefaultValue([NotNull] Type type)
            => type == typeof(string)
                ? string.Empty
                : type.IsArray
                    ? Array.CreateInstance(type.GetElementType(), 0)
                    : type.UnwrapNullableType().GetDefaultValue();

        private static object[,] ToMultidimensionalArray(IReadOnlyList<object> values)
        {
            var result = new object[1, values.Count];
            for (var i = 0; i < values.Count; i++)
            {
                result[0, i] = values[i];
            }
            return result;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected class DiffContext
        {
            private readonly IReadOnlyList<TableMapping> _sourceTables;
            private readonly IReadOnlyList<TableMapping> _targetTables;

            private readonly IDictionary<IEntityType, TableMapping> _sourceEntitiesMap
                = new Dictionary<IEntityType, TableMapping>();

            private readonly IDictionary<IEntityType, TableMapping> _targetEntitiesMap
                = new Dictionary<IEntityType, TableMapping>();

            private readonly IDictionary<TableMapping, TableMapping> _tableMappingMap = new Dictionary<TableMapping, TableMapping>();
            private readonly IDictionary<string, Dictionary<string, string>> _inverseColumnMap =
                new Dictionary<string, Dictionary<string, string>>();

            private readonly IDictionary<IEntityType, CreateTableOperation> _createTableOperations
                = new Dictionary<IEntityType, CreateTableOperation>();

            private readonly IDictionary<IEntityType, DropTableOperation> _dropTableOperations
                = new Dictionary<IEntityType, DropTableOperation>();

            private readonly IDictionary<DropTableOperation, TableMapping> _removedTables
                = new Dictionary<DropTableOperation, TableMapping>();

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public DiffContext([CanBeNull] IModel source, [CanBeNull] IModel target)
            {
                if (source != null)
                {
                    _sourceTables = TableMapping.GetTableMappings(source);
                    foreach (var table in _sourceTables)
                    {
                        foreach (var entityType in table.EntityTypes)
                        {
                            _sourceEntitiesMap.Add(entityType, table);
                        }
                    }
                }

                if (target != null)
                {
                    _targetTables = TableMapping.GetTableMappings(target);
                    foreach (var table in _targetTables)
                    {
                        foreach (var entityType in table.EntityTypes)
                        {
                            _targetEntitiesMap.Add(entityType, table);
                        }
                    }
                }
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual IEnumerable<TableMapping> GetSourceTables() => _sourceTables;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual IEnumerable<TableMapping> GetTargetTables() => _targetTables;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual void AddMapping([NotNull] TableMapping source, [NotNull] TableMapping target)
                => _tableMappingMap.Add(source, target);

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual void AddMapping([NotNull] IProperty source, [NotNull] IProperty target)
                => _inverseColumnMap.GetOrAddNew(target.DeclaringEntityType.Relational().TableName)
                    .Add(target.Relational().ColumnName, source.Relational().ColumnName);

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual void AddCreate([NotNull] IEntityType target, [NotNull] CreateTableOperation operation)
                => _createTableOperations.Add(target, operation);

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual void AddDrop([NotNull] TableMapping source, [NotNull] DropTableOperation operation)
            {
                foreach (var sourceEntityType in source.EntityTypes)
                {
                    _dropTableOperations.Add(sourceEntityType, operation);
                }
                _removedTables.Add(operation, source);
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual TableMapping FindSourceTable(IEntityType entityType)
                => _sourceEntitiesMap.TryGetValue(entityType, out var table)
                    ? table
                    : null;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual TableMapping FindTargetTable(IEntityType entityType)
                => _targetEntitiesMap.TryGetValue(entityType, out var table)
                    ? table
                    : null;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual TableMapping FindTarget([NotNull] TableMapping source)
                => _tableMappingMap.TryGetValue(source, out var target)
                    ? target
                    : null;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual string FindSourceColumn([NotNull] IProperty target)
                => _inverseColumnMap.TryGetValue(target.DeclaringEntityType.Relational().TableName, out var map)
                    ? map.TryGetValue(target.Relational().ColumnName, out var source)
                        ? source
                        : null
                    : null;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual IProperty FindCompatibleSource([NotNull] IProperty target, IEntityType sourceEntityType)
            {
                var sourceColumn = FindSourceColumn(target);
                return sourceColumn == null
                    ? null
                    : sourceEntityType.GetProperties().FirstOrDefault(p => p.ClrType == target.ClrType
                                                                           && p.Relational().ColumnName == sourceColumn);
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual CreateTableOperation FindCreate([NotNull] IEntityType target)
                => _createTableOperations.TryGetValue(target, out var operation)
                    ? operation
                    : null;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual DropTableOperation FindDrop([NotNull] IEntityType source)
                => _dropTableOperations.TryGetValue(source, out var operation)
                    ? operation
                    : null;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual TableMapping FindTable([NotNull] DropTableOperation operation)
                => _removedTables.TryGetValue(operation, out var source)
                    ? source
                    : null;
        }
    }
}
