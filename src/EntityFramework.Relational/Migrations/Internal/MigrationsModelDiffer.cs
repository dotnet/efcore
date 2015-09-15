// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Internal
{
    // TODO: Structural matching
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
            typeof(AddColumnOperation),
            typeof(AddForeignKeyOperation),
            typeof(AddPrimaryKeyOperation),
            typeof(AddUniqueConstraintOperation),
            typeof(AlterColumnOperation),
            typeof(AlterSequenceOperation),
            typeof(CreateIndexOperation),
            typeof(RestartSequenceOperation)
        };

        private static readonly Type[] _renameOperationTypes =
        {
            typeof(RenameColumnOperation),
            typeof(RenameSequenceOperation)
        };

        public MigrationsModelDiffer(
            [NotNull] IRelationalMetadataExtensionProvider annotations,
            [NotNull] IMigrationsAnnotationProvider migrationsAnnotations)
        {
            Check.NotNull(annotations, nameof(annotations));
            Check.NotNull(migrationsAnnotations, nameof(migrationsAnnotations));

            Annotations = annotations;
            MigrationsAnnotations = migrationsAnnotations;
        }

        protected virtual IRelationalMetadataExtensionProvider Annotations { get; }
        protected virtual IMigrationsAnnotationProvider MigrationsAnnotations { get; }

        public virtual bool HasDifferences(IModel source, [CanBeNull] IModel target)
            => Diff(source, target, new DiffContext()).Any();

        public virtual IReadOnlyList<MigrationOperation> GetDifferences(IModel source, IModel target)
        {
            var diffContext = new DiffContext();

            return Sort(Diff(source, target, diffContext), diffContext);
        }

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
            var alterOperations = new List<MigrationOperation>();
            var renameOperations = new List<MigrationOperation>();
            var renameIndexOperations = new List<MigrationOperation>();
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
                else if (_alterOperationTypes.Contains(type))
                {
                    alterOperations.Add(operation);
                }
                else if (_renameOperationTypes.Contains(type))
                {
                    renameOperations.Add(operation);
                }
                else if (type == typeof(RenameIndexOperation))
                {
                    renameIndexOperations.Add(operation);
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
                (principalCreateTableOperation, createTableOperation, addForeignKeyOperations) =>
                    {
                        foreach (var addForeignKeyOperation in addForeignKeyOperations)
                        {
                            createTableOperation.ForeignKeys.Remove(addForeignKeyOperation);
                            alterOperations.Add(addForeignKeyOperation);
                        }

                        return true;
                    }).ToList();

            var dropTableGraph = new Multigraph<DropTableOperation, IForeignKey>();
            dropTableGraph.AddVertices(dropTableOperations);
            foreach (var dropTableOperation in dropTableOperations)
            {
                var entityType = diffContext.GetMetadata(dropTableOperation);
                foreach (var foreignKey in entityType.GetForeignKeysInHierarchy())
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
                .Concat(dropOperations)
                .Concat(dropColumnOperations)
                .Concat(dropTableOperations)
                .Concat(ensureSchemaOperations)
                .Concat(createSequenceOperations)
                .Concat(createTableOperations)
                .Concat(alterOperations)
                .Concat(renameOperations)
                .Concat(renameIndexOperations)
                .Concat(renameTableOperations)
                .Concat(leftovers)
                .ToArray();
        }

        #region IModel

        protected virtual IEnumerable<MigrationOperation> Diff(
            [CanBeNull] IModel source,
            [CanBeNull] IModel target,
            [NotNull] DiffContext diffContext)
            => source != null && target != null
                ? Diff(GetSchemas(source), GetSchemas(target))
                    .Concat(Diff(source.GetRootEntityTypes(), target.GetRootEntityTypes(), diffContext))
                    .Concat(
                        Diff(Annotations.For(source).Sequences, Annotations.For(target).Sequences))
                    .Concat(
                        Diff(
                            source.EntityTypes.SelectMany(t => t.GetDeclaredForeignKeys()),
                            target.EntityTypes.SelectMany(t => t.GetDeclaredForeignKeys()),
                            diffContext))
                : target != null
                    ? Add(target, diffContext)
                    : source != null
                        ? Remove(source, diffContext)
                        : Enumerable.Empty<MigrationOperation>();

        protected virtual IEnumerable<MigrationOperation> Add([NotNull] IModel target, [NotNull] DiffContext diffContext)
            => GetSchemas(target).SelectMany(Add)
                .Concat(target.GetRootEntityTypes().SelectMany(t => Add(t, diffContext)))
                .Concat(Annotations.For(target).Sequences.SelectMany(Add))
                .Concat(target.EntityTypes.SelectMany(t => t.GetDeclaredForeignKeys()).SelectMany(k => Add(k, diffContext)));

        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] IModel source, [NotNull] DiffContext diffContext) =>
            source.GetRootEntityTypes().SelectMany(t => Remove(t, diffContext))
                .Concat(Annotations.For(source).Sequences.SelectMany(Remove));

        #endregion

        #region Schema

        protected virtual IEnumerable<MigrationOperation> Diff([NotNull] IEnumerable<string> source, [NotNull] IEnumerable<string> target)
            => DiffCollection(
                source, target,
                Diff, Add, Remove,
                (s, t) => s == t);

        protected virtual IEnumerable<MigrationOperation> Diff([NotNull] string source, [NotNull] string target)
            => Enumerable.Empty<MigrationOperation>();

        protected virtual IEnumerable<MigrationOperation> Add([NotNull] string target)
        {
            yield return new EnsureSchemaOperation { Name = target };
        }

        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] string source) => Enumerable.Empty<MigrationOperation>();

        #endregion

        #region IEntityType

        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<IEntityType> source,
            [NotNull] IEnumerable<IEntityType> target,
            [NotNull] DiffContext diffContext)
            => DiffCollection(
                source,
                target,
                (s, t) => Diff(s, t, diffContext),
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

            diffContext.AddMapping(source, target);

            var operations = Diff(source.GetPropertiesInHierarchy(), target.GetPropertiesInHierarchy(), diffContext)
                .Concat(Diff(source.GetKeys(), target.GetKeys(), diffContext))
                .Concat(Diff(source.GetIndexesInHierarchy(), target.GetIndexesInHierarchy(), diffContext));
            foreach (var operation in operations)
            {
                yield return operation;
            }
        }

        protected virtual IEnumerable<MigrationOperation> Add([NotNull] IEntityType target, [NotNull] DiffContext diffContext)
        {
            var targetAnnotations = Annotations.For(target);

            var createTableOperation = new CreateTableOperation
            {
                Schema = targetAnnotations.Schema,
                Name = targetAnnotations.TableName
            };
            CopyAnnotations(MigrationsAnnotations.For(target), createTableOperation);

            createTableOperation.Columns.AddRange(target.GetPropertiesInHierarchy().SelectMany(p => Add(p, inline: true)).Cast<AddColumnOperation>());
            var primaryKey = target.GetPrimaryKey();
            createTableOperation.PrimaryKey = Add(primaryKey).Cast<AddPrimaryKeyOperation>().Single();
            createTableOperation.UniqueConstraints.AddRange(
                target.GetKeys().Where(k => k != primaryKey).SelectMany(Add).Cast<AddUniqueConstraintOperation>());

            diffContext.AddCreate(target, createTableOperation);

            yield return createTableOperation;

            foreach (var operation in target.GetIndexesInHierarchy().SelectMany(Add))
            {
                yield return operation;
            }
        }

        protected virtual IEnumerable<MigrationOperation> Remove(
            [NotNull] IEntityType source, [NotNull] DiffContext diffContext)
        {
            var sourceAnnotations = Annotations.For(source);

            var operation = new DropTableOperation
            {
                Schema = sourceAnnotations.Schema,
                Name = sourceAnnotations.TableName
            };
            diffContext.AddDrop(source, operation);

            yield return operation;
        }

        #endregion

        #region IProperty

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
                t => Add(t),
                Remove,
                (s, t) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase),
                (s, t) => string.Equals(
                    Annotations.For(s).ColumnName,
                    Annotations.For(t).ColumnName,
                    StringComparison.OrdinalIgnoreCase));

        protected virtual IEnumerable<MigrationOperation> Diff([NotNull] IProperty source, [NotNull] IProperty target)
        {
            var sourceAnnotations = Annotations.For(source);
            var sourceEntityTypeAnnotations = Annotations.For(source.DeclaringEntityType.RootType());
            var targetAnnotations = Annotations.For(target);

            if (sourceAnnotations.ColumnName != targetAnnotations.ColumnName)
            {
                yield return new RenameColumnOperation
                {
                    Schema = sourceEntityTypeAnnotations.Schema,
                    Table = sourceEntityTypeAnnotations.TableName,
                    Name = sourceAnnotations.ColumnName,
                    NewName = targetAnnotations.ColumnName
                };
            }

            var targetClrType = target.ClrType.UnwrapNullableType().UnwrapEnumType();

            var targetMigrationsAnnotations = MigrationsAnnotations.For(target);

            var isSourceColumnNullable = source.IsColumnNullable();
            var isTargetColumnNullable = target.IsColumnNullable();
            var isNullableChanged = isSourceColumnNullable != isTargetColumnNullable;
            var columnTypeChanged = source.ClrType.UnwrapNullableType().UnwrapEnumType() != targetClrType
                                    || sourceAnnotations.ColumnType != targetAnnotations.ColumnType;
            if (isNullableChanged
                || columnTypeChanged
                || sourceAnnotations.GeneratedValueSql != targetAnnotations.GeneratedValueSql
                || !Equals(sourceAnnotations.DefaultValue, targetAnnotations.DefaultValue)
                || HasDifferences(MigrationsAnnotations.For(source), targetMigrationsAnnotations))
            {
                var isDestructiveChange = (isNullableChanged && isSourceColumnNullable)
                    // TODO: Detect type narrowing
                                          || columnTypeChanged;

                var alterColumnOperation = new AlterColumnOperation
                {
                    Schema = sourceEntityTypeAnnotations.Schema,
                    Table = sourceEntityTypeAnnotations.TableName,
                    Name = sourceAnnotations.ColumnName,
                    ClrType = targetClrType,
                    ColumnType = targetAnnotations.ColumnType,
                    IsNullable = isTargetColumnNullable,
                    DefaultValue = targetAnnotations.DefaultValue,
                    DefaultValueSql = target.ValueGenerated == ValueGenerated.OnAdd ? targetAnnotations.GeneratedValueSql : null,
                    ComputedColumnSql = target.ValueGenerated == ValueGenerated.OnAddOrUpdate ? targetAnnotations.GeneratedValueSql : null,
                    IsDestructiveChange = isDestructiveChange
                };
                CopyAnnotations(targetMigrationsAnnotations, alterColumnOperation);

                yield return alterColumnOperation;
            }
        }

        protected virtual IEnumerable<MigrationOperation> Add([NotNull] IProperty target, bool inline = false)
        {
            var targetAnnotations = Annotations.For(target);
            var targetEntityTypeAnnotations = Annotations.For(target.DeclaringEntityType.RootType());

            var operation = new AddColumnOperation
            {
                Schema = targetEntityTypeAnnotations.Schema,
                Table = targetEntityTypeAnnotations.TableName,
                Name = targetAnnotations.ColumnName,
                ClrType = target.ClrType.UnwrapNullableType().UnwrapEnumType(),
                ColumnType = targetAnnotations.ColumnType,
                IsNullable = target.IsColumnNullable(),
                DefaultValue = targetAnnotations.DefaultValue
                               ?? (inline || target.IsColumnNullable()
                                   ? null
                                   : GetDefaultValue(target.ClrType)),
                DefaultValueSql = target.ValueGenerated == ValueGenerated.OnAdd ? targetAnnotations.GeneratedValueSql : null,
                ComputedColumnSql = target.ValueGenerated == ValueGenerated.OnAddOrUpdate ? targetAnnotations.GeneratedValueSql : null
            };
            CopyAnnotations(MigrationsAnnotations.For(target), operation);

            yield return operation;
        }

        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] IProperty source)
        {
            var sourceEntityTypeAnnotations = Annotations.For(source.DeclaringEntityType.RootType());

            yield return new DropColumnOperation
            {
                Schema = sourceEntityTypeAnnotations.Schema,
                Table = sourceEntityTypeAnnotations.TableName,
                Name = Annotations.For(source).ColumnName
            };
        }

        #endregion

        #region IKey

        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<IKey> source,
            [NotNull] IEnumerable<IKey> target,
            [NotNull] DiffContext diffContext)
            => DiffCollection(
                source, target,
                Diff, Add, Remove,
                (s, t) => Annotations.For(s).Name == Annotations.For(t).Name
                          && s.Properties.Select(diffContext.FindTarget).SequenceEqual(t.Properties)
                          && s.IsPrimaryKey() == t.IsPrimaryKey());

        protected virtual IEnumerable<MigrationOperation> Diff([NotNull] IKey source, [NotNull] IKey target)
            => HasDifferences(MigrationsAnnotations.For(source), MigrationsAnnotations.For(target))
                ? Remove(source).Concat(Add(target))
                : Enumerable.Empty<MigrationOperation>();

        protected virtual IEnumerable<MigrationOperation> Add([NotNull] IKey target)
        {
            var targetAnnotations = Annotations.For(target);
            var targetEntityTypeAnnotations = Annotations.For(target.EntityType.RootType());

            MigrationOperation operation;
            if (target.IsPrimaryKey())
            {
                operation = new AddPrimaryKeyOperation
                {
                    Schema = targetEntityTypeAnnotations.Schema,
                    Table = targetEntityTypeAnnotations.TableName,
                    Name = targetAnnotations.Name,
                    Columns = GetColumns(target.Properties)
                };
            }
            else
            {
                operation = new AddUniqueConstraintOperation
                {
                    Schema = targetEntityTypeAnnotations.Schema,
                    Table = targetEntityTypeAnnotations.TableName,
                    Name = targetAnnotations.Name,
                    Columns = GetColumns(target.Properties)
                };
            }
            CopyAnnotations(MigrationsAnnotations.For(target), operation);

            yield return operation;
        }

        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] IKey source)
        {
            var sourceAnnotations = Annotations.For(source);
            var sourceEntityTypeAnnotations = Annotations.For(source.EntityType.RootType());

            if (source.IsPrimaryKey())
            {
                yield return new DropPrimaryKeyOperation
                {
                    Schema = sourceEntityTypeAnnotations.Schema,
                    Table = sourceEntityTypeAnnotations.TableName,
                    Name = sourceAnnotations.Name
                };
            }
            else
            {
                yield return new DropUniqueConstraintOperation
                {
                    Schema = sourceEntityTypeAnnotations.Schema,
                    Table = sourceEntityTypeAnnotations.TableName,
                    Name = sourceAnnotations.Name
                };
            }
        }

        #endregion

        #region IForeignKey

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
                (s, t) => Annotations.For(s).Name == Annotations.For(t).Name
                          && s.Properties.Select(diffContext.FindTarget).SequenceEqual(t.Properties)
                          && diffContext.FindTarget(s.PrincipalEntityType.RootType()) == t.PrincipalEntityType.RootType()
                          && s.PrincipalKey.Properties.Select(diffContext.FindTarget).SequenceEqual(t.PrincipalKey.Properties)
                          && s.DeleteBehavior == t.DeleteBehavior);

        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IForeignKey source, [NotNull] IForeignKey target, [NotNull] DiffContext diffContext)
            => HasDifferences(MigrationsAnnotations.For(source), MigrationsAnnotations.For(target))
                ? Remove(source, diffContext).Concat(Add(target, diffContext))
                : Enumerable.Empty<MigrationOperation>();

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
                    : ReferentialAction.NoAction
            };
            CopyAnnotations(MigrationsAnnotations.For(target), operation);

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

        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] IForeignKey source, [NotNull] DiffContext diffContext)
        {
            var declaringRootEntityType = source.DeclaringEntityType.RootType();
            var sourceEntityTypeAnnotations = Annotations.For(declaringRootEntityType);

            var dropTableOperation = diffContext.FindDrop(declaringRootEntityType);
            if (dropTableOperation == null)
            {
                yield return new DropForeignKeyOperation
                {
                    Schema = sourceEntityTypeAnnotations.Schema,
                    Table = sourceEntityTypeAnnotations.TableName,
                    Name = Annotations.For(source).Name
                };
            }
        }

        #endregion

        #region IIndex

        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<IIndex> source,
            [NotNull] IEnumerable<IIndex> target,
            [NotNull] DiffContext diffContext)
            => DiffCollection(
                source, target,
                Diff, Add, Remove,
                (s, t) => string.Equals(
                    Annotations.For(s).Name,
                    Annotations.For(t).Name,
                    StringComparison.OrdinalIgnoreCase)
                          && s.Properties.Select(diffContext.FindTarget).SequenceEqual(t.Properties),
                (s, t) => s.Properties.Select(diffContext.FindTarget).SequenceEqual(t.Properties));

        protected virtual IEnumerable<MigrationOperation> Diff([NotNull] IIndex source, [NotNull] IIndex target)
        {
            var sourceEntityTypeAnnotations = Annotations.For(source.DeclaringEntityType.RootType());
            var sourceName = Annotations.For(source).Name;
            var targetName = Annotations.For(target).Name;

            if (sourceName != targetName)
            {
                yield return new RenameIndexOperation
                {
                    Schema = sourceEntityTypeAnnotations.Schema,
                    Table = sourceEntityTypeAnnotations.TableName,
                    Name = sourceName,
                    NewName = targetName
                };
            }

            if (source.IsUnique != target.IsUnique
                || HasDifferences(MigrationsAnnotations.For(source), MigrationsAnnotations.For(target)))
            {
                var operations = Remove(source).Concat(Add(target));
                foreach (var operation in operations)
                {
                    yield return operation;
                }
            }
        }

        protected virtual IEnumerable<MigrationOperation> Add([NotNull] IIndex target)
        {
            var targetEntityTypeAnnotations = Annotations.For(target.DeclaringEntityType.RootType());

            var operation = new CreateIndexOperation
            {
                Name = Annotations.For(target).Name,
                Schema = targetEntityTypeAnnotations.Schema,
                Table = targetEntityTypeAnnotations.TableName,
                Columns = GetColumns(target.Properties),
                IsUnique = target.IsUnique
            };
            CopyAnnotations(MigrationsAnnotations.For(target), operation);

            yield return operation;
        }

        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] IIndex source)
        {
            var sourceEntityTypeAnnotations = Annotations.For(source.DeclaringEntityType.RootType());

            yield return new DropIndexOperation
            {
                Name = Annotations.For(source).Name,
                Schema = sourceEntityTypeAnnotations.Schema,
                Table = sourceEntityTypeAnnotations.TableName
            };
        }

        #endregion

        #region ISequence

        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<ISequence> source,
            [NotNull] IEnumerable<ISequence> target)
            => DiffCollection(
                source, target,
                Diff, Add, Remove,
                (s, t) => string.Equals(s.Schema, t.Schema, StringComparison.OrdinalIgnoreCase)
                          && string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase)
                          && s.ClrType == t.ClrType,
                (s, t) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase)
                          && s.ClrType == t.ClrType);

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
                    Schema = source.Schema,
                    Name = source.Name,
                    StartValue = target.StartValue
                };
            }

            if (source.IncrementBy != target.IncrementBy
                || source.MaxValue != target.MaxValue
                || source.MinValue != target.MinValue
                || source.IsCyclic != target.IsCyclic)
            {
                yield return new AlterSequenceOperation
                {
                    Schema = source.Schema,
                    Name = source.Name,
                    IncrementBy = target.IncrementBy,
                    MinValue = target.MinValue,
                    MaxValue = target.MaxValue,
                    IsCyclic = target.IsCyclic
                };
            }
        }

        protected virtual IEnumerable<MigrationOperation> Add([NotNull] ISequence target)
        {
            yield return new CreateSequenceOperation
            {
                Schema = target.Schema,
                Name = target.Name,
                ClrType = target.ClrType,
                StartValue = target.StartValue,
                IncrementBy = target.IncrementBy,
                MinValue = target.MinValue,
                MaxValue = target.MaxValue,
                IsCyclic = target.IsCyclic
            };
        }

        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] ISequence source)
        {
            yield return new DropSequenceOperation
            {
                Schema = source.Schema,
                Name = source.Name
            };
        }

        #endregion

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
                    var paired = false;

                    for (var j = targetList.Count - 1; j >= 0; j--)
                    {
                        var target = targetList[j];

                        if (predicate(source, target))
                        {
                            targetList.RemoveAt(j);
                            paired = true;

                            foreach (var operation in diff(source, target))
                            {
                                yield return operation;
                            }

                            break;
                        }
                    }

                    if (paired)
                    {
                        sourceList.RemoveAt(i);
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

        protected virtual string[] GetColumns([NotNull] IEnumerable<IProperty> properties)
            => properties.Select(p => Annotations.For(p).ColumnName).ToArray();

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

        protected virtual void CopyAnnotations([NotNull] IEnumerable<IAnnotation> annotations, [NotNull] Annotatable annotatable)
        {
            foreach (var annotation in annotations)
            {
                annotatable.AddAnnotation(annotation.Name, annotation.Value);
            }
        }

        protected virtual IEnumerable<string> GetSchemas([NotNull] IModel model)
            => model.GetRootEntityTypes().Select(t => Annotations.For(t).Schema).Where(s => !string.IsNullOrEmpty(s))
                .Distinct();

        protected virtual object GetDefaultValue([NotNull] Type type)
            => type == typeof(string)
                ? string.Empty
                : type.IsArray
                    ? Array.CreateInstance(type.GetElementType(), 0)
                    : type.UnwrapNullableType().GetDefaultValue();

        protected class DiffContext
        {
            private readonly IDictionary<IEntityType, IEntityType> _entityTypeMap = new Dictionary<IEntityType, IEntityType>();
            private readonly IDictionary<IProperty, IProperty> _propertyMap = new Dictionary<IProperty, IProperty>();

            private readonly IDictionary<IEntityType, CreateTableOperation> _createTableOperations
                = new Dictionary<IEntityType, CreateTableOperation>();

            private readonly IDictionary<IEntityType, DropTableOperation> _dropTableOperations
                = new Dictionary<IEntityType, DropTableOperation>();

            private readonly IDictionary<DropTableOperation, IEntityType> _removedEntityTypes
                = new Dictionary<DropTableOperation, IEntityType>();

            public virtual void AddMapping([NotNull] IEntityType source, [NotNull] IEntityType target)
                => _entityTypeMap.Add(source, target);

            public virtual void AddMapping([NotNull] IProperty source, [NotNull] IProperty target)
                => _propertyMap.Add(source, target);

            public virtual void AddCreate([NotNull] IEntityType target, [NotNull] CreateTableOperation operation)
                => _createTableOperations.Add(target, operation);

            public virtual void AddDrop([NotNull] IEntityType source, [NotNull] DropTableOperation operation)
            {
                _dropTableOperations.Add(source, operation);
                _removedEntityTypes.Add(operation, source);
            }

            public virtual IEntityType FindTarget([NotNull] IEntityType source)
            {
                IEntityType target;
                _entityTypeMap.TryGetValue(source, out target);

                return target;
            }

            public virtual IProperty FindTarget([NotNull] IProperty source)
            {
                IProperty target;
                _propertyMap.TryGetValue(source, out target);

                return target;
            }

            public virtual CreateTableOperation FindCreate([NotNull] IEntityType target)
            {
                CreateTableOperation operation;
                _createTableOperations.TryGetValue(target, out operation);

                return operation;
            }

            public virtual DropTableOperation FindDrop([NotNull] IEntityType source)
            {
                DropTableOperation operation;
                _dropTableOperations.TryGetValue(source, out operation);

                return operation;
            }

            public virtual IEntityType GetMetadata([NotNull] DropTableOperation operation)
            {
                IEntityType source;
                _removedEntityTypes.TryGetValue(operation, out source);

                return source;
            }
        }
    }
}
