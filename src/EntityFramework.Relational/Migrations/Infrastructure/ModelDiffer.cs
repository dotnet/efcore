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
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Infrastructure
{
    // TODO: Handle transitive renames (See #1907)
    // TODO: Structural matching
    public class ModelDiffer : IModelDiffer
    {
        private static readonly Type[] _dropOperationTypes =
        {
            typeof(DropForeignKeyOperation),
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

        public ModelDiffer(
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IRelationalMetadataExtensionProvider metadataExtensions,
            [NotNull] IMigrationAnnotationProvider annotations)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(metadataExtensions, nameof(metadataExtensions));
            Check.NotNull(annotations, nameof(annotations));

            TypeMapper = typeMapper;
            MetadataExtensions = metadataExtensions;
            Annotations = annotations;
        }

        protected virtual IRelationalTypeMapper TypeMapper { get; }
        protected virtual IRelationalMetadataExtensionProvider MetadataExtensions { get; }
        protected virtual IMigrationAnnotationProvider Annotations { get; }

        public virtual bool HasDifferences(IModel source, [CanBeNull] IModel target) => Diff(source, target).Any();

        public virtual IReadOnlyList<MigrationOperation> GetDifferences(IModel source, IModel target) => Sort(Diff(source, target));

        protected virtual IReadOnlyList<MigrationOperation> Sort([NotNull] IEnumerable<MigrationOperation> operations)
        {
            Check.NotNull(operations, nameof(operations));

            var dropOperations = new List<MigrationOperation>();
            var dropColumnOperations = new List<MigrationOperation>();
            var dropTableOperations = new List<MigrationOperation>();
            var dropSchemaOperations = new List<MigrationOperation>();
            var createSchemaOperations = new List<MigrationOperation>();
            var createSequenceOperations = new List<MigrationOperation>();
            var createTableOperations = new Dictionary<string, CreateTableOperation>();
            var alterOperations = new List<MigrationOperation>();
            var renameOperations = new List<MigrationOperation>();
            var renameIndexOperations = new List<MigrationOperation>();
            var renameTableOperations = new List<MigrationOperation>();
            var leftovers = new List<MigrationOperation>();

            foreach (var operation in operations)
            {
                var type = operation.GetType();
                if (_dropOperationTypes.Contains(type))
                {
                    dropOperations.Add(operation);
                }
                else if (type == typeof(DropColumnOperation))
                {
                    dropColumnOperations.Add(operation);
                }
                else if (type == typeof(DropTableOperation))
                {
                    dropTableOperations.Add(operation);
                }
                else if (type == typeof(DropSchemaOperation))
                {
                    dropSchemaOperations.Add(operation);
                }
                else if (type == typeof(CreateSchemaOperation))
                {
                    createSchemaOperations.Add(operation);
                }
                else if (type == typeof(CreateSequenceOperation))
                {
                    createSequenceOperations.Add(operation);
                }
                else if (type == typeof(CreateTableOperation))
                {
                    var createTableOperation = (CreateTableOperation)operation;

                    createTableOperations.Add(
                        createTableOperation.Schema + ":" + createTableOperation.Name,
                        createTableOperation);
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

            var createTableGraph = new Multigraph<string, AddForeignKeyOperation>();
            createTableGraph.AddVertices(createTableOperations.Keys);
            foreach (var pair in createTableOperations)
            {
                foreach (var addForeignKeyOperation in pair.Value.ForeignKeys)
                {
                    var principalTable = addForeignKeyOperation.ReferencedSchema + ":" + addForeignKeyOperation.ReferencedTable;
                    if (principalTable == pair.Key)
                    {
                        continue;
                    }
                    if (!createTableOperations.ContainsKey(principalTable))
                    {
                        continue;
                    }
                    createTableGraph.AddEdge(principalTable, pair.Key, addForeignKeyOperation);
                }
            }
            var sortedCreateTableOperations = createTableGraph.TopologicalSort(
                (principalTable, key, foreignKeyOperations) =>
                    {
                        foreach (var foreignKeyOperation in foreignKeyOperations)
                        {
                            createTableOperations[key].ForeignKeys.Remove(foreignKeyOperation);
                            alterOperations.Add(foreignKeyOperation);
                        }
                        return true;
                    })
                .Select(k => createTableOperations[k]);

            return dropOperations
                .Concat(dropColumnOperations)
                .Concat(dropTableOperations)
                .Concat(dropSchemaOperations)
                .Concat(createSchemaOperations)
                .Concat(createSequenceOperations)
                .Concat(sortedCreateTableOperations)
                .Concat(alterOperations)
                .Concat(renameOperations)
                .Concat(renameIndexOperations)
                .Concat(renameTableOperations)
                .Concat(leftovers)
                .ToArray();
        }

        #region IModel

        protected virtual IEnumerable<MigrationOperation> Diff([CanBeNull] IModel source, [CanBeNull] IModel target) =>
            source != null && target != null
                ? Diff(source.EntityTypes, target.EntityTypes)
                    .Concat(
                        Diff(MetadataExtensions.Extensions(source).Sequences, MetadataExtensions.Extensions(target).Sequences))
                : target != null
                    ? Add(target)
                    : source != null
                        ? Remove(source)
                        : Enumerable.Empty<MigrationOperation>();

        protected virtual IEnumerable<MigrationOperation> Add(IModel target) =>
            target.EntityTypes.SelectMany(Add)
                .Concat(MetadataExtensions.Extensions(target).Sequences.SelectMany(Add));

        protected virtual IEnumerable<MigrationOperation> Remove(IModel source) =>
            source.EntityTypes.SelectMany(Remove)
                .Concat(MetadataExtensions.Extensions(source).Sequences.SelectMany(Remove));

        #endregion

        #region IEntityType

        protected virtual IEnumerable<MigrationOperation> Diff(IEnumerable<IEntityType> source, IEnumerable<IEntityType> target)
            => DiffCollection(
                source, target,
                Diff, Add, Remove,
                (s, t) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase),
                (s, t) => string.Equals(
                        MetadataExtensions.Extensions(s).Schema,
                        MetadataExtensions.Extensions(t).Schema,
                        StringComparison.OrdinalIgnoreCase)
                    && string.Equals(
                        MetadataExtensions.Extensions(s).Table,
                        MetadataExtensions.Extensions(t).Table,
                        StringComparison.OrdinalIgnoreCase),
                (s, t) => string.Equals(
                     MetadataExtensions.Extensions(s).Table,
                     MetadataExtensions.Extensions(t).Table,
                     StringComparison.OrdinalIgnoreCase));

        protected virtual IEnumerable<MigrationOperation> Diff(IEntityType source, IEntityType target)
        {
            var sourceExtensions = MetadataExtensions.Extensions(source);
            var targetExtensions = MetadataExtensions.Extensions(target);

            var schemaChanged = sourceExtensions.Schema != targetExtensions.Schema;
            var renamed = sourceExtensions.Table != targetExtensions.Table;
            if (schemaChanged || renamed)
            {
                yield return new RenameTableOperation
                {
                    Schema = sourceExtensions.Schema,
                    Name = sourceExtensions.Table,
                    NewSchema = schemaChanged ? targetExtensions.Schema : null,
                    NewName = renamed ? targetExtensions.Table : null
                };
            }

            var sourcePrimaryKey = source.GetPrimaryKey();
            var targetPrimaryKey = target.GetPrimaryKey();

            var operations = Diff(source.GetProperties(), target.GetProperties())
                .Concat(Diff(new[] { sourcePrimaryKey }, new[] { targetPrimaryKey }))
                .Concat(Diff(source.GetKeys().Where(k => k != sourcePrimaryKey), target.GetKeys().Where(k => k != targetPrimaryKey)))
                .Concat(Diff(source.GetForeignKeys(), target.GetForeignKeys()))
                .Concat(Diff(source.GetIndexes(), target.GetIndexes()));
            foreach (var operation in operations)
            {
                yield return operation;
            }
        }

        protected virtual IEnumerable<MigrationOperation> Add(IEntityType target)
        {
            var targetExtensions = MetadataExtensions.Extensions(target);

            var createTableOperation = new CreateTableOperation
            {
                Schema = targetExtensions.Schema,
                Name = targetExtensions.Table
            };
            CopyAnnotations(Annotations.For(target), createTableOperation);

            createTableOperation.Columns.AddRange(target.GetProperties().SelectMany(Add).Cast<AddColumnOperation>());
            var primaryKey = target.GetPrimaryKey();
            createTableOperation.PrimaryKey = Add(primaryKey).Cast<AddPrimaryKeyOperation>().Single();
            createTableOperation.UniqueConstraints.AddRange(
                target.GetKeys().Where(k => k != primaryKey).SelectMany(Add).Cast<AddUniqueConstraintOperation>());
            createTableOperation.ForeignKeys.AddRange(target.GetForeignKeys().SelectMany(Add).Cast<AddForeignKeyOperation>());

            yield return createTableOperation;

            foreach (var operation in target.GetIndexes().SelectMany(Add))
            {
                yield return operation;
            }
        }

        protected virtual IEnumerable<MigrationOperation> Remove(IEntityType source)
        {
            var sourceExtensions = MetadataExtensions.Extensions(source);

            yield return new DropTableOperation
            {
                Schema = sourceExtensions.Schema,
                Name = sourceExtensions.Table
            };
        }

        #endregion

        #region IProperty

        protected virtual IEnumerable<MigrationOperation> Diff(IEnumerable<IProperty> source, IEnumerable<IProperty> target)
            => DiffCollection(
                source, target,
                Diff, Add, Remove,
                (s, t) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase),
                (s, t) => string.Equals(
                    MetadataExtensions.Extensions(s).Column,
                    MetadataExtensions.Extensions(t).Column,
                    StringComparison.OrdinalIgnoreCase));

        protected virtual IEnumerable<MigrationOperation> Diff(IProperty source, IProperty target)
        {
            var sourceExtensions = MetadataExtensions.Extensions(source);
            var sourceEntityTypeExtensions = MetadataExtensions.Extensions(source.EntityType);
            var targetExtensions = MetadataExtensions.Extensions(target);

            if (sourceExtensions.Column != targetExtensions.Column)
            {
                yield return new RenameColumnOperation
                {
                    Schema = sourceEntityTypeExtensions.Schema,
                    Table = sourceEntityTypeExtensions.Table,
                    Name = sourceExtensions.Column,
                    NewName = targetExtensions.Column
                };
            }

            var sourceColumnType = sourceExtensions.ColumnType
                                   ?? TypeMapper.MapPropertyType(source).DefaultTypeName;

            var targetColumnType = targetExtensions.ColumnType
                                   ?? TypeMapper.MapPropertyType(target).DefaultTypeName;

            var targetAnnotations = Annotations.For(target);

            var isNullableChanged = source.IsNullable != target.IsNullable;
            var columnTypeChanged = sourceColumnType != targetColumnType;
            if (isNullableChanged
                || columnTypeChanged
                || sourceExtensions.DefaultValueSql != targetExtensions.DefaultValueSql
                || sourceExtensions.DefaultValue != targetExtensions.DefaultValue
                || HasDifferences(Annotations.For(source), targetAnnotations))
            {
                var isDestructiveChange = (isNullableChanged && source.IsNullable)
                                          // TODO: Detect type narrowing
                                          || columnTypeChanged;

                var alterColumnOperation = new AlterColumnOperation
                {
                    Schema = sourceEntityTypeExtensions.Schema,
                    Table = sourceEntityTypeExtensions.Table,
                    Name = sourceExtensions.Column,
                    Type = targetColumnType,
                    IsNullable = target.IsNullable,
                    DefaultValue = targetExtensions.DefaultValue,
                    DefaultValueSql = targetExtensions.DefaultValueSql,
                    IsDestructiveChange = isDestructiveChange
                };
                CopyAnnotations(targetAnnotations, alterColumnOperation);

                yield return alterColumnOperation;
            }
        }

        protected virtual IEnumerable<MigrationOperation> Add(IProperty target)
        {
            var targetExtensions = MetadataExtensions.Extensions(target);
            var targetEntityTypeExtensions = MetadataExtensions.Extensions(target.EntityType);

            var operation = new AddColumnOperation
            {
                Schema = targetEntityTypeExtensions.Schema,
                Table = targetEntityTypeExtensions.Table,
                Name = targetExtensions.Column,
                Type = targetExtensions.ColumnType ?? TypeMapper.MapPropertyType(target).DefaultTypeName,
                IsNullable = target.IsNullable,
                DefaultValue = targetExtensions.DefaultValue,
                DefaultValueSql = targetExtensions.DefaultValueSql
            };
            CopyAnnotations(Annotations.For(target), operation);

            yield return operation;
        }

        protected virtual IEnumerable<MigrationOperation> Remove(IProperty source)
        {
            var sourceExtensions = MetadataExtensions.Extensions(source);
            var sourceEntityTypeExtensions = MetadataExtensions.Extensions(source.EntityType);

            yield return new DropColumnOperation
            {
                Schema = sourceEntityTypeExtensions.Schema,
                Table = sourceEntityTypeExtensions.Table,
                Name = sourceExtensions.Column
            };
        }

        #endregion

        #region IKey

        protected virtual IEnumerable<MigrationOperation> Diff(IEnumerable<IKey> source, IEnumerable<IKey> target)
            => DiffCollection(
                source, target,
                Diff, Add, Remove,
                (s, t) => MetadataExtensions.Extensions(s).Name == MetadataExtensions.Extensions(t).Name
                    && GetColumnNames(s.Properties).SequenceEqual(GetColumnNames(t.Properties))
                    && s.IsPrimaryKey() == t.IsPrimaryKey());

        protected virtual IEnumerable<MigrationOperation> Diff(IKey source, IKey target)
            => HasDifferences(Annotations.For(source), Annotations.For(target))
                ? Remove(source).Concat(Add(target))
                : Enumerable.Empty<MigrationOperation>();

        protected virtual IEnumerable<MigrationOperation> Add(IKey target)
        {
            var targetExtensions = MetadataExtensions.Extensions(target);
            var targetEntityTypeExtensions = MetadataExtensions.Extensions(target.EntityType);

            MigrationOperation operation;
            if (target.IsPrimaryKey())
            {
                operation = new AddPrimaryKeyOperation
                {
                    Schema = targetEntityTypeExtensions.Schema,
                    Table = targetEntityTypeExtensions.Table,
                    Name = targetExtensions.Name,
                    Columns = GetColumnNames(target.Properties)
                };
            }
            else
            {
                operation = new AddUniqueConstraintOperation
                {
                    Schema = targetEntityTypeExtensions.Schema,
                    Table = targetEntityTypeExtensions.Table,
                    Name = targetExtensions.Name,
                    Columns = GetColumnNames(target.Properties)
                };
            }
            CopyAnnotations(Annotations.For(target), operation);

            yield return operation;
        }

        protected virtual IEnumerable<MigrationOperation> Remove(IKey source)
        {
            var sourceExtensions = MetadataExtensions.Extensions(source);
            var sourceEntityTypeExtensions = MetadataExtensions.Extensions(source.EntityType);

            if (source.IsPrimaryKey())
            {
                yield return new DropPrimaryKeyOperation
                {
                    Schema = sourceEntityTypeExtensions.Schema,
                    Table = sourceEntityTypeExtensions.Table,
                    Name = sourceExtensions.Name
                };
            }
            else
            {
                yield return new DropUniqueConstraintOperation
                {
                    Schema = sourceEntityTypeExtensions.Schema,
                    Table = sourceEntityTypeExtensions.Table,
                    Name = sourceExtensions.Name
                };
            }
        }

        #endregion

        #region IForeignKey

        protected virtual IEnumerable<MigrationOperation> Diff(IEnumerable<IForeignKey> source, IEnumerable<IForeignKey> target)
            => DiffCollection(
                source, target,
                Diff, Add, Remove,
                (s, t) =>
                {
                    var sourcePrincipalEntityTypeExtensions = MetadataExtensions.Extensions(s.PrincipalEntityType);
                    var targetPrincipalEntityTypeExtensions = MetadataExtensions.Extensions(t.PrincipalEntityType);

                    return MetadataExtensions.Extensions(s).Name == MetadataExtensions.Extensions(t).Name
                        && GetColumnNames(s.Properties).SequenceEqual(GetColumnNames(t.Properties))
                        && sourcePrincipalEntityTypeExtensions.Schema == targetPrincipalEntityTypeExtensions.Schema
                        && sourcePrincipalEntityTypeExtensions.Table == targetPrincipalEntityTypeExtensions.Table
                        && GetColumnNames(s.PrincipalKey.Properties).SequenceEqual(GetColumnNames(t.PrincipalKey.Properties));
                });

        protected virtual IEnumerable<MigrationOperation> Diff(IForeignKey source, IForeignKey target)
            => HasDifferences(Annotations.For(source), Annotations.For(target))
                ? Remove(source).Concat(Add(target))
                : Enumerable.Empty<MigrationOperation>();

        protected virtual IEnumerable<MigrationOperation> Add(IForeignKey target)
        {
            var targetExtensions = MetadataExtensions.Extensions(target);
            var targetEntityTypeExtensions = MetadataExtensions.Extensions(target.EntityType);
            var targetPrincipalEntityTypeExtensions = MetadataExtensions.Extensions(target.PrincipalEntityType);

            // TODO: Set OnDelete (See #1084)
            var operation = new AddForeignKeyOperation
            {
                Schema = targetEntityTypeExtensions.Schema,
                Table = targetEntityTypeExtensions.Table,
                Name = targetExtensions.Name,
                Columns = GetColumnNames(target.Properties),
                ReferencedSchema = targetPrincipalEntityTypeExtensions.Schema,
                ReferencedTable = targetPrincipalEntityTypeExtensions.Table,
                ReferencedColumns = GetColumnNames(target.PrincipalKey.Properties)
            };
            CopyAnnotations(Annotations.For(target), operation);

            yield return operation;
        }

        protected virtual IEnumerable<MigrationOperation> Remove(IForeignKey source)
        {
            var sourceExtensions = MetadataExtensions.Extensions(source);
            var sourceEntityTypeExtensions = MetadataExtensions.Extensions(source.EntityType);

            yield return new DropForeignKeyOperation
            {
                Schema = sourceEntityTypeExtensions.Schema,
                Table = sourceEntityTypeExtensions.Table,
                Name = sourceExtensions.Name
            };
        }

        #endregion

        #region IIndex

        protected virtual IEnumerable<MigrationOperation> Diff(IEnumerable<IIndex> source, IEnumerable<IIndex> target)
            => DiffCollection(
                source, target,
                Diff, Add, Remove,
                (s, t) => string.Equals(
                        MetadataExtensions.Extensions(s).Name,
                        MetadataExtensions.Extensions(t).Name,
                        StringComparison.OrdinalIgnoreCase)
                    && GetColumnNames(s.Properties).SequenceEqual(GetColumnNames(t.Properties)),
                (s, t) => GetColumnNames(s.Properties).SequenceEqual(GetColumnNames(t.Properties)));

        protected virtual IEnumerable<MigrationOperation> Diff(IIndex source, IIndex target)
        {
            var sourceEntityTypeExtensions = MetadataExtensions.Extensions(source.EntityType);
            var sourceName = MetadataExtensions.Extensions(source).Name;
            var targetName = MetadataExtensions.Extensions(target).Name;

            if (sourceName != targetName)
            {
                yield return new RenameIndexOperation
                {
                    Schema = sourceEntityTypeExtensions.Schema,
                    Table = sourceEntityTypeExtensions.Table,
                    Name = sourceName,
                    NewName = targetName
                };
            }

            if (source.IsUnique != target.IsUnique
                || HasDifferences(Annotations.For(source), Annotations.For(target)))
            {
                var operations = Remove(source).Concat(Add(target));
                foreach (var operation in operations)
                {
                    yield return operation;
                }
            }
        }

        protected virtual IEnumerable<MigrationOperation> Add(IIndex target)
        {
            var targetExtensions = MetadataExtensions.Extensions(target);
            var targetEntityTypeExtensions = MetadataExtensions.Extensions(target.EntityType);

            var operation = new CreateIndexOperation
            {
                Name = targetExtensions.Name,
                Schema = targetEntityTypeExtensions.Schema,
                Table = targetEntityTypeExtensions.Table,
                Columns = GetColumnNames(target.Properties),
                IsUnique = target.IsUnique
            };
            CopyAnnotations(Annotations.For(target), operation);

            yield return operation;
        }

        protected virtual IEnumerable<MigrationOperation> Remove(IIndex source)
        {
            var sourceExtensions = MetadataExtensions.Extensions(source);
            var sourceEntityTypeExtensions = MetadataExtensions.Extensions(source.EntityType);

            yield return new DropIndexOperation
            {
                Name = sourceExtensions.Name,
                Schema = sourceEntityTypeExtensions.Schema,
                Table = sourceEntityTypeExtensions.Table
            };
        }

        #endregion

        #region ISequence

        protected virtual IEnumerable<MigrationOperation> Diff(IEnumerable<ISequence> source, IEnumerable<ISequence> target)
            => DiffCollection(
                source, target,
                Diff, Add, Remove,
                (s, t) => string.Equals(s.Schema, t.Schema, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase)
                    && s.Type == t.Type,
                (s, t) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase)
                    && s.Type == t.Type);

        protected virtual IEnumerable<MigrationOperation> Diff(ISequence source, ISequence target)
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

            if (source.IncrementBy != target.IncrementBy
                || source.MaxValue != target.MaxValue
                || source.MinValue != target.MinValue
                || source.Cycle != target.Cycle)
            {
                yield return new AlterSequenceOperation
                {
                    Schema = source.Schema,
                    Name = source.Name,
                    IncrementBy = target.IncrementBy,
                    MinValue = target.MinValue,
                    MaxValue = target.MaxValue,
                    Cycle = target.Cycle
                };
            }
        }

        protected virtual IEnumerable<MigrationOperation> Add(ISequence target)
        {
            yield return new CreateSequenceOperation
            {
                Schema = target.Schema,
                Name = target.Name,
                Type = TypeMapper.GetDefaultMapping(target.Type).DefaultTypeName,
                StartWith = target.StartValue,
                IncrementBy = target.IncrementBy,
                MinValue = target.MinValue,
                MaxValue = target.MaxValue,
                Cycle = target.Cycle
            };
        }

        protected virtual IEnumerable<MigrationOperation> Remove(ISequence source)
        {
            yield return new DropSequenceOperation
            {
                Schema = source.Schema,
                Name = source.Name
            };
        }

        #endregion

        protected virtual IEnumerable<MigrationOperation> DiffCollection<T>(
            IEnumerable<T> sources,
            IEnumerable<T> targets,
            Func<T, T, IEnumerable<MigrationOperation>> diff,
            Func<T, IEnumerable<MigrationOperation>> add,
            Func<T, IEnumerable<MigrationOperation>> remove,
            params Func<T, T, bool>[] predicates)
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

        protected virtual string[] GetColumnNames(IEnumerable<IProperty> properties)
            => properties.Select(p => MetadataExtensions.Extensions(p).Column).ToArray();

        protected virtual bool HasDifferences(IEnumerable<IAnnotation> source, IEnumerable<IAnnotation> target)
        {
            var unmatched = new List<IAnnotation>(target);

            foreach (var annotation in source)
            {
                var index = unmatched.FindIndex(a => a.Name == annotation.Name && a.Value == annotation.Value);
                if (index == -1)
                {
                    return true;
                }

                unmatched.RemoveAt(index);
            }

            return unmatched.Count != 0;
        }

        protected virtual void CopyAnnotations(IEnumerable<IAnnotation> annotations, Annotatable annotatable)
        {
            foreach (var annotation in annotations)
            {
                annotatable.AddAnnotation(annotation.Name, annotation.Value);
            }
        }
    }
}
