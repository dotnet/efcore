// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MigrationsModelDiffer(
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IRelationalAnnotationProvider annotations,
            [NotNull] IMigrationsAnnotationProvider migrationsAnnotations)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(annotations, nameof(annotations));
            Check.NotNull(migrationsAnnotations, nameof(migrationsAnnotations));

            TypeMapper = typeMapper;
            Annotations = annotations;
            MigrationsAnnotations = migrationsAnnotations;
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
        protected virtual IRelationalAnnotationProvider Annotations { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IMigrationsAnnotationProvider MigrationsAnnotations { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasDifferences(IModel source, IModel target)
            => Diff(source, target, new DiffContext()).Any();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<MigrationOperation> GetDifferences(IModel source, IModel target)
        {
            var diffContext = new DiffContext();

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
                    if ((addForeignKeyOperation.Table == addForeignKeyOperation.PrincipalTable)
                        && (addForeignKeyOperation.Schema == addForeignKeyOperation.PrincipalSchema))
                    {
                        continue;
                    }

                    var principalCreateTableOperation = createTableOperations.FirstOrDefault(
                        o => (o.Name == addForeignKeyOperation.PrincipalTable)
                             && (o.Schema == addForeignKeyOperation.PrincipalSchema));
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
                var entityType = diffContext.GetMetadata(dropTableOperation);
                foreach (var foreignKey in GetForeignKeysInHierarchy(entityType))
                {
                    var principalRootEntityType = foreignKey.PrincipalEntityType.RootType();
                    if (entityType == principalRootEntityType)
                    {
                        continue;
                    }

                    var principalDropTableOperation = diffContext.FindDrop(principalRootEntityType);
                    if (principalDropTableOperation != null)
                    {
                        dropTableGraph.AddEdge(dropTableOperation, principalDropTableOperation, foreignKey);
                    }
                }
            }
            var newDiffContext = new DiffContext();
            dropTableOperations = dropTableGraph.TopologicalSort(
                (dropTableOperation, principalDropTableOperation, foreignKeys) =>
                {
                    dropForeignKeyOperations.AddRange(foreignKeys.SelectMany(c => Remove(c, newDiffContext)));

                    return true;
                }).ToList();

            return dropForeignKeyOperations
                .Concat(dropTableOperations)
                .Concat(dropOperations)
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
            => (source != null) && (target != null)
                ? DiffAnnotations(source, target)
                    .Concat(Diff(GetSchemas(source), GetSchemas(target)))
                    .Concat(Diff(source.GetRootEntityTypes(), target.GetRootEntityTypes(), diffContext))
                    .Concat(
                        Diff(Annotations.For(source).Sequences, Annotations.For(target).Sequences))
                    .Concat(
                        Diff(
                            source.GetRootEntityTypes().SelectMany(GetForeignKeysInHierarchy),
                            target.GetRootEntityTypes().SelectMany(GetForeignKeysInHierarchy),
                            diffContext))
                : target != null
                    ? Add(target, diffContext)
                    : source != null
                        ? Remove(source, diffContext)
                        : Enumerable.Empty<MigrationOperation>();

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
                .Concat(target.GetRootEntityTypes().SelectMany(t => Add(t, diffContext)))
                .Concat(Annotations.For(target).Sequences.SelectMany(Add))
                .Concat(target.GetRootEntityTypes().SelectMany(GetForeignKeysInHierarchy).SelectMany(k => Add(k, diffContext)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] IModel source, [NotNull] DiffContext diffContext)
            => DiffAnnotations(source, null)
                .Concat(source.GetRootEntityTypes().SelectMany(t => Remove(t, diffContext)))
                .Concat(Annotations.For(source).Sequences.SelectMany(Remove));

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
            [NotNull] IEnumerable<IEntityType> source,
            [NotNull] IEnumerable<IEntityType> target,
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
                (s, t) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase),
                (s, t) => string.Equals(
                    Annotations.For(s).Schema,
                    Annotations.For(t).Schema,
                    StringComparison.OrdinalIgnoreCase)
                          && string.Equals(
                              Annotations.For(s).TableName,
                              Annotations.For(t).TableName,
                              StringComparison.OrdinalIgnoreCase),
                (s, t) => string.Equals(
                    Annotations.For(s).TableName,
                    Annotations.For(t).TableName,
                    StringComparison.OrdinalIgnoreCase));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEntityType source,
            [NotNull] IEntityType target,
            [NotNull] DiffContext diffContext)
        {
            var sourceAnnotations = Annotations.For(source);
            var targetAnnotations = Annotations.For(target);

            var schemaChanged = sourceAnnotations.Schema != targetAnnotations.Schema;
            var renamed = sourceAnnotations.TableName != targetAnnotations.TableName;
            if (schemaChanged || renamed)
            {
                yield return new RenameTableOperation
                {
                    Schema = sourceAnnotations.Schema,
                    Name = sourceAnnotations.TableName,
                    NewSchema = schemaChanged ? targetAnnotations.Schema : null,
                    NewName = renamed ? targetAnnotations.TableName : null
                };
            }

            var operations = DiffAnnotations(source, target)
                .Concat(Diff(GetPropertiesInHierarchy(source), GetPropertiesInHierarchy(target), diffContext))
                .Concat(Diff(source.GetKeys(), target.GetKeys(), diffContext))
                .Concat(Diff(GetIndexesInHierarchy(source), GetIndexesInHierarchy(target), diffContext));
            foreach (var operation in operations)
            {
                yield return operation;
            }
        }

        private IEnumerable<MigrationOperation> DiffAnnotations(
            [NotNull] IEntityType source,
            [NotNull] IEntityType target)
        {
            var sourceMigrationsAnnotations = MigrationsAnnotations.For(source).ToList();
            var targetMigrationsAnnotations = MigrationsAnnotations.For(target).ToList();
            if (HasDifferences(sourceMigrationsAnnotations, targetMigrationsAnnotations))
            {
                var targetAnnotations = Annotations.For(target);
                var alterTableOperation = new AlterTableOperation
                {
                    Name = targetAnnotations.TableName,
                    Schema = targetAnnotations.Schema
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
        protected virtual IEnumerable<MigrationOperation> Add([NotNull] IEntityType target, [NotNull] DiffContext diffContext)
        {
            var targetAnnotations = Annotations.For(target);

            var createTableOperation = new CreateTableOperation
            {
                Schema = targetAnnotations.Schema,
                Name = targetAnnotations.TableName
            };
            createTableOperation.AddAnnotations(MigrationsAnnotations.For(target));

            createTableOperation.Columns.AddRange(
                GetPropertiesInHierarchy(target).SelectMany(p => Add(p, diffContext, inline: true)).Cast<AddColumnOperation>());
            var primaryKey = target.FindPrimaryKey();
            createTableOperation.PrimaryKey = Add(primaryKey, diffContext).Cast<AddPrimaryKeyOperation>().Single();
            createTableOperation.UniqueConstraints.AddRange(
                target.GetKeys().Where(k => k != primaryKey).SelectMany(k => Add(k, diffContext))
                    .Cast<AddUniqueConstraintOperation>());

            diffContext.AddCreate(target, createTableOperation);

            yield return createTableOperation;

            foreach (var operation in GetIndexesInHierarchy(target).SelectMany(i => Add(i, diffContext)))
            {
                yield return operation;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove(
            [NotNull] IEntityType source, [NotNull] DiffContext diffContext)
        {
            var sourceAnnotations = Annotations.For(source);

            var operation = new DropTableOperation
            {
                Schema = sourceAnnotations.Schema,
                Name = sourceAnnotations.TableName
            };
            operation.AddAnnotations(MigrationsAnnotations.ForRemove(source));
            diffContext.AddDrop(source, operation);

            yield return operation;
        }

        private IEnumerable<IForeignKey> GetForeignKeysInHierarchy(IEntityType entityType)
            => entityType.GetDerivedTypesInclusive().SelectMany(t => t.GetDeclaredForeignKeys())
                .Distinct((x, y) => Annotations.For(x).Name == Annotations.For(y).Name);

        private IEnumerable<IIndex> GetIndexesInHierarchy(IEntityType entityType)
            => entityType.GetDerivedTypesInclusive().SelectMany(t => t.GetDeclaredIndexes())
                .Distinct((x, y) => Annotations.For(x).Name == Annotations.For(y).Name);

        private IEnumerable<IProperty> GetPropertiesInHierarchy(IEntityType entityType)
            => entityType.GetDerivedTypesInclusive().SelectMany(t => t.GetDeclaredProperties())
                .Distinct((x, y) => Annotations.For(x).ColumnName == Annotations.For(y).ColumnName);

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
                    Annotations.For(s).ColumnName,
                    Annotations.For(t).ColumnName,
                    StringComparison.OrdinalIgnoreCase),
                (s, t) =>
                {
                    var sAnnotations = Annotations.For(s);
                    var tAnnotations = Annotations.For(t);

                    return s.ClrType == t.ClrType
                           && s.IsConcurrencyToken == t.IsConcurrencyToken
                           && s.ValueGenerated == t.ValueGenerated
                           && s.GetMaxLength() == t.GetMaxLength()
                           && s.IsColumnNullable() == t.IsColumnNullable()
                           && s.IsUnicode() == t.IsUnicode()
                           && sAnnotations.ColumnType == tAnnotations.ColumnType
                           && sAnnotations.ComputedColumnSql == tAnnotations.ComputedColumnSql
                           && sAnnotations.DefaultValue == tAnnotations.DefaultValue
                           && sAnnotations.DefaultValueSql == tAnnotations.DefaultValueSql;
                });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff([NotNull] IProperty source, [NotNull] IProperty target)
        {
            var sourceAnnotations = Annotations.For(source);
            var targetEntityTypeAnnotations = Annotations.For(target.DeclaringEntityType.RootType());
            var targetAnnotations = Annotations.For(target);

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
                var isDestructiveChange = (isNullableChanged && isSourceColumnNullable)
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
            var targetAnnotations = Annotations.For(target);
            var targetEntityTypeAnnotations = Annotations.For(target.DeclaringEntityType.RootType());

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
            var sourceEntityTypeAnnotations = Annotations.For(source.DeclaringEntityType.RootType());

            var operation = new DropColumnOperation
            {
                Schema = sourceEntityTypeAnnotations.Schema,
                Table = sourceEntityTypeAnnotations.TableName,
                Name = Annotations.For(source).ColumnName
            };
            operation.AddAnnotations(MigrationsAnnotations.ForRemove(source));

            yield return operation;
        }

        private ColumnOperation Initialize(
            ColumnOperation columnOperation,
            IProperty property,
            bool isNullable,
            IRelationalPropertyAnnotations annotations,
            IEnumerable<IAnnotation> migrationsAnnotations,
            bool inline = false)
        {
            columnOperation.ClrType = property.ClrType.UnwrapNullableType().UnwrapEnumType();
            columnOperation.ColumnType = annotations.ColumnType;
            columnOperation.MaxLength = property.GetMaxLength();
            columnOperation.IsUnicode = property.IsUnicode();
            columnOperation.IsRowVersion = property.ClrType == typeof(byte[])
                                           && property.IsConcurrencyToken
                                           && property.ValueGenerated == ValueGenerated.OnAddOrUpdate;
            columnOperation.IsNullable = isNullable;
            columnOperation.DefaultValue = annotations.DefaultValue
                                           ?? (inline || isNullable
                                               ? null
                                               : GetDefaultValue(columnOperation.ClrType));
            columnOperation.DefaultValueSql = annotations.DefaultValueSql;
            columnOperation.ComputedColumnSql = annotations.ComputedColumnSql;
            columnOperation.AddAnnotations(migrationsAnnotations);

            return columnOperation;
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
                (s, t) => (Annotations.For(s).Name == Annotations.For(t).Name)
                          && s.Properties.Select(diffContext.FindTarget).SequenceEqual(t.Properties)
                          && (s.IsPrimaryKey() == t.IsPrimaryKey())
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
            var targetAnnotations = Annotations.For(target);
            var targetEntityTypeAnnotations = Annotations.For(
                target.DeclaringEntityType.RootType());
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
            var sourceAnnotations = Annotations.For(source);
            var sourceEntityTypeAnnotations = Annotations.For(source.DeclaringEntityType.RootType());

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
                (s, t) => (Annotations.For(s).Name == Annotations.For(t).Name)
                          && s.Properties.Select(diffContext.FindTarget).SequenceEqual(t.Properties)
                          && (diffContext.FindTarget(s.PrincipalEntityType.RootType()) == t.PrincipalEntityType.RootType())
                          && s.PrincipalKey.Properties.Select(diffContext.FindTarget).SequenceEqual(t.PrincipalKey.Properties)
                          && (s.DeleteBehavior == t.DeleteBehavior)
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
            var targetEntityTypeAnnotations = Annotations.For(declaringRootEntityType);
            var targetPrincipalEntityTypeAnnotations = Annotations.For(target.PrincipalEntityType.RootType());

            var operation = new AddForeignKeyOperation
            {
                Schema = targetEntityTypeAnnotations.Schema,
                Table = targetEntityTypeAnnotations.TableName,
                Name = Annotations.For(target).Name,
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
            var sourceEntityTypeAnnotations = Annotations.For(declaringRootEntityType);

            var dropTableOperation = diffContext.FindDrop(declaringRootEntityType);
            if (dropTableOperation == null)
            {
                var operation = new DropForeignKeyOperation
                {
                    Schema = sourceEntityTypeAnnotations.Schema,
                    Table = sourceEntityTypeAnnotations.TableName,
                    Name = Annotations.For(source).Name
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
                    Annotations.For(s).Name,
                    Annotations.For(t).Name,
                    StringComparison.OrdinalIgnoreCase)
                          && s.IsUnique == t.IsUnique
                          && Annotations.For(s).Filter == Annotations.For(t).Filter
                          && !HasDifferences(MigrationsAnnotations.For(s), MigrationsAnnotations.For(t))
                          && s.Properties.Select(diffContext.FindTarget).SequenceEqual(t.Properties),
                // ReSharper disable once ImplicitlyCapturedClosure
                (s, t) => s.IsUnique == t.IsUnique
                          && Annotations.For(s).Filter == Annotations.For(t).Filter
                          && !HasDifferences(MigrationsAnnotations.For(s), MigrationsAnnotations.For(t))
                          && s.Properties.Select(diffContext.FindTarget).SequenceEqual(t.Properties));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IIndex source,
            [NotNull] IIndex target,
            [NotNull] DiffContext diffContext)
        {
            var targetEntityTypeAnnotations = Annotations.For(target.DeclaringEntityType.RootType());
            var sourceName = Annotations.For(source).Name;
            var targetName = Annotations.For(target).Name;

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
            var targetEntityTypeAnnotations = Annotations.For(
                target.DeclaringEntityType.RootType());

            var operation = new CreateIndexOperation
            {
                Name = Annotations.For(target).Name,
                Schema = targetEntityTypeAnnotations.Schema,
                Table = targetEntityTypeAnnotations.TableName,
                Columns = GetColumns(target.Properties),
                IsUnique = target.IsUnique,
                Filter = Annotations.For(target).Filter
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
            var sourceEntityTypeAnnotations = Annotations.For(source.DeclaringEntityType.RootType());

            var operation = new DropIndexOperation
            {
                Name = Annotations.For(source).Name,
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
                          && (s.ClrType == t.ClrType),
                (s, t) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase)
                          && (s.ClrType == t.ClrType));

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

            if ((source.IncrementBy != target.IncrementBy)
                || (source.MaxValue != target.MaxValue)
                || (source.MinValue != target.MinValue)
                || (source.IsCyclic != target.IsCyclic)
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
            => properties.Select(p => Annotations.For(p).ColumnName).ToArray();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual bool HasDifferences([NotNull] IEnumerable<IAnnotation> source, [NotNull] IEnumerable<IAnnotation> target)
        {
            var unmatched = new List<IAnnotation>(target);

            foreach (var annotation in source)
            {
                var index = unmatched.FindIndex(a => (a.Name == annotation.Name) && Equals(a.Value, annotation.Value));
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
            => model.GetRootEntityTypes().Select(t => Annotations.For(t).Schema)
                .Concat(Annotations.For(model).Sequences.Select(s => s.Schema))
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected class DiffContext
        {
            private readonly IDictionary<IEntityType, IEntityType> _entityTypeMap = new Dictionary<IEntityType, IEntityType>();
            private readonly IDictionary<IEntityType, IEntityType> _reverseEntityTypeMap = new Dictionary<IEntityType, IEntityType>();
            private readonly IDictionary<IProperty, IProperty> _propertyMap = new Dictionary<IProperty, IProperty>();
            private readonly IDictionary<IProperty, IProperty> _reversePropertyMap = new Dictionary<IProperty, IProperty>();

            private readonly IDictionary<IEntityType, CreateTableOperation> _createTableOperations
                = new Dictionary<IEntityType, CreateTableOperation>();

            private readonly IDictionary<IEntityType, DropTableOperation> _dropTableOperations
                = new Dictionary<IEntityType, DropTableOperation>();

            private readonly IDictionary<DropTableOperation, IEntityType> _removedEntityTypes
                = new Dictionary<DropTableOperation, IEntityType>();

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual void AddMapping([NotNull] IEntityType source, [NotNull] IEntityType target)
            {
                _entityTypeMap.Add(source, target);
                _reverseEntityTypeMap.Add(target, source);
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual void AddMapping([NotNull] IProperty source, [NotNull] IProperty target)
            {
                _propertyMap.Add(source, target);
                _reversePropertyMap.Add(target, source);
            }

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
            public virtual void AddDrop([NotNull] IEntityType source, [NotNull] DropTableOperation operation)
            {
                _dropTableOperations.Add(source, operation);
                _removedEntityTypes.Add(operation, source);
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual IEntityType FindTarget([NotNull] IEntityType source)
            {
                IEntityType target;
                _entityTypeMap.TryGetValue(source, out target);

                return target;
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual IProperty FindTarget([NotNull] IProperty source)
            {
                IProperty target;
                _propertyMap.TryGetValue(source, out target);

                return target;
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual IEntityType FindSource([NotNull] IEntityType target)
            {
                IEntityType source;
                _reverseEntityTypeMap.TryGetValue(target, out source);

                return source ?? target;
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual IProperty FindSource([NotNull] IProperty target)
            {
                IProperty source;
                _reversePropertyMap.TryGetValue(target, out source);

                return source ?? target;
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual CreateTableOperation FindCreate([NotNull] IEntityType target)
            {
                CreateTableOperation operation;
                _createTableOperations.TryGetValue(target, out operation);

                return operation;
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual DropTableOperation FindDrop([NotNull] IEntityType source)
            {
                DropTableOperation operation;
                _dropTableOperations.TryGetValue(source, out operation);

                return operation;
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual IEntityType GetMetadata([NotNull] DropTableOperation operation)
            {
                IEntityType source;
                _removedEntityTypes.TryGetValue(operation, out source);

                return source;
            }
        }
    }
}
