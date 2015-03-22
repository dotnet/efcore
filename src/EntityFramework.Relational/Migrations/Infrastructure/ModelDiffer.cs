// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Infrastructure
{
    // TODO: Move constraint naming
    // TODO: Handle transitive renames
    // TODO: Match similar items
    public class ModelDiffer : IModelDiffer
    {
        private static readonly Type[] _dropOperationTypes =
            {
                typeof(DropColumnOperation),
                typeof(DropForeignKeyOperation),
                typeof(DropIndexOperation),
                typeof(DropPrimaryKeyOperation),
                typeof(DropSequenceOperation),
                typeof(DropTableOperation),
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
                typeof(CreateIndexOperation)
            };

        private static readonly Type[] _renameOperationTypes =
            {
                typeof(RenameColumnOperation),
                typeof(RenameIndexOperation),
                typeof(RenameSequenceOperation)
            };

        private static readonly Type[] _moveOperationTypes =
            {
                typeof(MoveSequenceOperation),
                typeof(MoveTableOperation)
            };

        public ModelDiffer([NotNull] RelationalTypeMapper typeMapper)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));

            TypeMapper = typeMapper;
        }

        protected virtual RelationalTypeMapper TypeMapper { get; }

        public virtual bool HasDifferences(IModel source, [CanBeNull] IModel target) => Diff(source, target).Any();

        public virtual IReadOnlyList<MigrationOperation> GetDifferences(IModel source, IModel target) => Sort(Diff(source, target));

        protected virtual IReadOnlyList<MigrationOperation> Sort([NotNull] IEnumerable<MigrationOperation> operations)
        {
            Check.NotNull(operations, nameof(operations));

            var dropOperations = new List<MigrationOperation>();
            var createSequenceOperations = new List<MigrationOperation>();
            var createTableOperations = new Dictionary<string, CreateTableOperation>();
            var alterOperations = new List<MigrationOperation>();
            var renameOperations = new List<MigrationOperation>();
            var renameTableOperations = new List<MigrationOperation>();
            var moveOperations = new List<MigrationOperation>();
            var leftovers = new List<MigrationOperation>();

            foreach (var operation in operations)
            {
                var type = operation.GetType();
                if (_dropOperationTypes.Contains(type))
                {
                    dropOperations.Add(operation);
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
                else if (type == typeof(RenameTableOperation))
                {
                    renameTableOperations.Add(operation);
                }
                else if (_moveOperationTypes.Contains(type))
                {
                    moveOperations.Add(operation);
                }
                else
                {
                    leftovers.Add(operation);
                }
            }

            var createTableGraph = new Multigraph<string, AddForeignKeyOperation>();
            createTableGraph.AddVertices(createTableOperations.Keys);
            foreach (var pair in createTableOperations)
            {
                foreach (var addForeignKeyOperation in pair.Value.ForeignKeys)
                {
                    var principalTable = addForeignKeyOperation.PrincipalSchema + ":" + addForeignKeyOperation.PrincipalTable;
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
            var sortedCreatetableOperations = createTableGraph.TopologicalSort(
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
                .Concat(createSequenceOperations)
                .Concat(sortedCreatetableOperations)
                .Concat(alterOperations)
                .Concat(renameOperations)
                .Concat(renameTableOperations)
                .Concat(moveOperations)
                .Concat(leftovers)
                .ToArray();
        }

        #region IModel

        protected virtual IEnumerable<MigrationOperation> Diff([CanBeNull] IModel source, [CanBeNull] IModel target) =>
            source != null && target != null
                ? Diff(source.EntityTypes, target.EntityTypes)
                    .Concat(Diff(source.Relational().Sequences, target.Relational().Sequences))
                : target != null
                    ? Add(target)
                    : source != null
                        ? Remove(source)
                        : Enumerable.Empty<MigrationOperation>();

        protected virtual IEnumerable<MigrationOperation> Add(IModel target) =>
            target.EntityTypes.SelectMany(Add)
                .Concat(target.Relational().Sequences.SelectMany(Add));

        protected virtual IEnumerable<MigrationOperation> Remove(IModel source) =>
            source.EntityTypes.SelectMany(Remove)
                .Concat(source.Relational().Sequences.SelectMany(Remove));

        #endregion

        #region IEntityType

        protected virtual IEnumerable<MigrationOperation> Diff(IEnumerable<IEntityType> source, IEnumerable<IEntityType> target) =>
            DiffCollection(source, target, Diff, TryMatch, Add, Remove);

        protected virtual IEnumerable<MigrationOperation> Diff(IEntityType source, IEntityType target)
        {
            if (source.Relational().Schema != target.Relational().Schema)
            {
                yield return new MoveTableOperation(
                    source.Relational().Table,
                    source.Relational().Schema,
                    target.Relational().Schema);
            }

            if (source.Relational().Table != target.Relational().Table)
            {
                yield return new RenameTableOperation(
                    source.Relational().Table,
                    source.Relational().Schema,
                    target.Relational().Table);
            }

            var sourcePrimaryKey = source.GetPrimaryKey();
            var targetPrimaryKey = target.GetPrimaryKey();

            var operations = Diff(source.GetProperties(), target.GetProperties())
                .Concat(Diff(sourcePrimaryKey, targetPrimaryKey))
                .Concat(Diff(source.GetKeys().Where(k => k != sourcePrimaryKey), target.GetKeys().Where(k => k != targetPrimaryKey)))
                .Concat(Diff(source.GetForeignKeys(), target.GetForeignKeys()))
                .Concat(Diff(source.GetIndexes(), target.GetIndexes()));
            foreach (var operation in operations)
            {
                yield return operation;
            }
        }

        protected virtual IEntityType TryMatch(IEntityType source, IEnumerable<IEntityType> targets)
        {
            var candidates = new Dictionary<IEntityType, int>();
            foreach (var target in targets)
            {
                if (string.Equals(source.Name, target.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return target;
                }
                if (string.Equals(
                    source.Relational().Table,
                    target.Relational().Table,
                    StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(
                        source.Relational().Schema,
                        target.Relational().Schema,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        candidates.Add(target, 0);
                    }
                    else
                    {
                        candidates.Add(target, 1);
                    }
                }
            }

            return candidates.OrderBy(c => c.Value).Select(c => c.Key).FirstOrDefault();
        }

        protected virtual IEnumerable<MigrationOperation> Add(IEntityType target)
        {
            var createTableOperation = new CreateTableOperation(target.Relational().Table, target.Relational().Schema);

            // TODO: Build ColumnModel objects directly
            createTableOperation.Columns.AddRange(target.GetProperties().SelectMany(Add).Cast<AddColumnOperation>().Select(o => o.Column));

            var primaryKey = target.GetPrimaryKey();
            if (primaryKey != null)
            {
                createTableOperation.PrimaryKey = Add(primaryKey).Cast<AddPrimaryKeyOperation>().Single();
            }

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
            yield return new DropTableOperation(source.Relational().Table, source.Relational().Schema);
        }

        #endregion

        #region IProperty

        protected virtual IEnumerable<MigrationOperation> Diff(IEnumerable<IProperty> source, IEnumerable<IProperty> target) =>
            DiffCollection(source, target, Diff, TryMatch, Add, Remove);

        protected virtual IEnumerable<MigrationOperation> Diff(IProperty source, IProperty target)
        {
            if (source.Relational().Column != target.Relational().Column)
            {
                yield return new RenameColumnOperation(
                    source.EntityType.Relational().Table,
                    source.EntityType.Relational().Schema,
                    source.Relational().Column,
                    target.Relational().Column);
            }

            var sourceColumnType = TypeMapper.GetTypeMapping(source).StoreTypeName;
            var targetColumnType = TypeMapper.GetTypeMapping(target).StoreTypeName;

            // TODO: How do DefaultExpression and DefaultValue relate to IsStoreComputed?
            if (source.IsNullable != target.IsNullable
                || source.IsStoreComputed != target.IsStoreComputed
                || sourceColumnType != targetColumnType
                || source.Relational().DefaultExpression != target.Relational().DefaultExpression
                || source.Relational().DefaultValue != target.Relational().DefaultValue)
            {
                // TODO: Determine if data loss
                yield return new AlterColumnOperation(
                    source.EntityType.Relational().Table,
                    source.EntityType.Relational().Schema,
                    new ColumnModel(
                        source.Relational().Column,
                        targetColumnType,
                        target.IsNullable,
                        target.Relational().DefaultValue,
                        target.Relational().DefaultExpression));
            }
        }

        protected virtual IProperty TryMatch(IProperty source, IEnumerable<IProperty> targets)
        {
            var candidates = new List<IProperty>();
            foreach (var target in targets)
            {
                if (string.Equals(source.Name, target.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return target;
                }
                if (string.Equals(
                    source.Relational().Column,
                    target.Relational().Column,
                    StringComparison.OrdinalIgnoreCase))
                {
                    candidates.Add(target);
                }
            }

            return candidates.FirstOrDefault();
        }

        protected virtual IEnumerable<MigrationOperation> Add(IProperty target)
        {
            yield return new AddColumnOperation(
                target.EntityType.Relational().Table,
                target.EntityType.Relational().Schema,
                new ColumnModel(
                    target.Relational().Column,
                    TypeMapper.GetTypeMapping(target).StoreTypeName,
                    target.IsNullable,
                    target.Relational().DefaultValue,
                    target.Relational().DefaultExpression));
        }

        protected virtual IEnumerable<MigrationOperation> Remove(IProperty source)
        {
            yield return new DropColumnOperation(
                source.EntityType.Relational().Table,
                source.EntityType.Relational().Schema,
                source.Relational().Column);
        }

        #endregion

        #region IKey

        protected virtual IEnumerable<MigrationOperation> Diff(IEnumerable<IKey> source, IEnumerable<IKey> target) =>
            DiffCollection(source, target, Diff, TryMatch, Add, Remove);

        protected virtual IEnumerable<MigrationOperation> Diff(IKey source, IKey target)
        {
            if (source != null
                && target != null)
            {
                if (Name(source) != Name(target)
                    || !source.Properties.Select(p => p.Relational().Column).SequenceEqual(
                        target.Properties.Select(p => p.Relational().Column)))
                {
                    return Remove(source)
                        .Concat(Add(target));
                }
            }
            else if (target != null)
            {
                return Add(target);
            }
            else if (source != null)
            {
                return Remove(source);
            }

            return Enumerable.Empty<MigrationOperation>();
        }

        protected virtual IKey TryMatch(IKey source, IEnumerable<IKey> targets)
        {
            foreach (var target in targets)
            {
                if (string.Equals(Name(source), Name(target), StringComparison.OrdinalIgnoreCase))
                {
                    return target;
                }
            }

            return null;
        }

        protected virtual IEnumerable<MigrationOperation> Add(IKey target)
        {
            if (target.IsPrimaryKey())
            {
                yield return new AddPrimaryKeyOperation(
                    target.EntityType.Relational().Table,
                    target.EntityType.Relational().Schema,
                    Name(target),
                    target.Properties.Select(p => p.Relational().Column).ToArray());
            }
            else
            {
                yield return new AddUniqueConstraintOperation(
                    target.EntityType.Relational().Table,
                    target.EntityType.Relational().Schema,
                    Name(target),
                    target.Properties.Select(p => p.Relational().Column).ToArray());
            }
        }

        protected virtual IEnumerable<MigrationOperation> Remove(IKey source)
        {
            if (source.IsPrimaryKey())
            {
                yield return new DropPrimaryKeyOperation(
                    source.EntityType.Relational().Table,
                    source.EntityType.Relational().Schema,
                    Name(source));
            }
            else
            {
                yield return new DropUniqueConstraintOperation(
                    source.EntityType.Relational().Table,
                    source.EntityType.Relational().Schema,
                    Name(source));
            }
        }

        #endregion

        #region IForeignKey

        protected virtual IEnumerable<MigrationOperation> Diff(IEnumerable<IForeignKey> source, IEnumerable<IForeignKey> target) =>
            DiffCollection(source, target, Diff, TryMatch, Add, Remove);

        protected virtual IEnumerable<MigrationOperation> Diff(IForeignKey source, IForeignKey target)
        {
            if (Name(source) != Name(target)
                || !source.Properties.Select(p => p.Relational().Column).SequenceEqual(
                    target.Properties.Select(p => p.Relational().Column))
                || source.PrincipalEntityType.Relational().Table != target.PrincipalEntityType.Relational().Table
                || !source.PrincipalKey.Properties.Select(p => p.Relational().Column).SequenceEqual(
                    target.PrincipalKey.Properties.Select(p => p.Relational().Column)))
            {
                return Remove(source)
                    .Concat(Add(target));
            }

            return Enumerable.Empty<MigrationOperation>();
        }

        protected virtual IForeignKey TryMatch(IForeignKey source, IEnumerable<IForeignKey> targets)
        {
            foreach (var target in targets)
            {
                if (string.Equals(Name(source), Name(target), StringComparison.OrdinalIgnoreCase))
                {
                    return target;
                }
            }

            return null;
        }

        protected virtual IEnumerable<MigrationOperation> Add(IForeignKey target)
        {
            // TODO: Set cascadeDelete (See #1084)
            yield return new AddForeignKeyOperation(
                target.EntityType.Relational().Table,
                target.EntityType.Relational().Schema,
                Name(target),
                target.Properties.Select(p => p.Relational().Column).ToArray(),
                target.PrincipalEntityType.Relational().Table,
                target.PrincipalEntityType.Relational().Schema,
                target.PrincipalKey.Properties.Select(p => p.Relational().Column).ToArray(),
                cascadeDelete: false);
        }

        protected virtual IEnumerable<MigrationOperation> Remove(IForeignKey source)
        {
            yield return new DropForeignKeyOperation(
                source.EntityType.Relational().Table,
                source.EntityType.Relational().Schema,
                Name(source));
        }

        #endregion

        #region IIndex

        protected virtual IEnumerable<MigrationOperation> Diff(IEnumerable<IIndex> source, IEnumerable<IIndex> target) =>
            DiffCollection(source, target, Diff, TryMatch, Add, Remove);

        protected virtual IEnumerable<MigrationOperation> Diff(IIndex source, IIndex target)
        {
            var sourceName = Name(source);
            var targetName = Name(target);

            if (sourceName != targetName)
            {
                yield return new RenameIndexOperation(
                    source.EntityType.Relational().Table,
                    source.EntityType.Relational().Schema,
                    sourceName,
                    targetName);
            }

            if (!source.Properties.Select(p => p.Relational().Column).SequenceEqual(
                target.Properties.Select(p => p.Relational().Column))
                || source.IsUnique != target.IsUnique)
            {
                var operations = Remove(source)
                    .Concat(Add(target));
                foreach (var operation in operations)
                {
                    yield return operation;
                }
            }
        }

        protected virtual IIndex TryMatch(IIndex source, IEnumerable<IIndex> targets)
        {
            foreach (var target in targets)
            {
                if (string.Equals(Name(source), Name(target), StringComparison.OrdinalIgnoreCase))
                {
                    return target;
                }
            }

            return null;
        }

        protected virtual IEnumerable<MigrationOperation> Add(IIndex target)
        {
            yield return new CreateIndexOperation(
                Name(target),
                target.EntityType.Relational().Table,
                target.EntityType.Relational().Schema,
                target.Properties.Select(p => p.Relational().Column).ToArray(),
                target.IsUnique);
        }

        protected virtual IEnumerable<MigrationOperation> Remove(IIndex source)
        {
            yield return new DropIndexOperation(
                Name(source),
                source.EntityType.Relational().Table,
                source.EntityType.Relational().Schema);
        }

        #endregion

        #region ISequence

        protected virtual IEnumerable<MigrationOperation> Diff(IEnumerable<ISequence> source, IEnumerable<ISequence> target) =>
            DiffCollection(source, target, Diff, TryMatch, Add, Remove);

        protected virtual IEnumerable<MigrationOperation> Diff(ISequence source, ISequence target)
        {
            if (source.Name != target.Name)
            {
                yield return new RenameSequenceOperation(source.Name, source.Schema, target.Name);
            }

            if (source.Schema != target.Schema)
            {
                yield return new MoveSequenceOperation(source.Name, source.Schema, target.Schema);
            }

            var sourceStoreType = TypeMapper.GetTypeMapping(source).StoreTypeName;
            var targetStoreType = TypeMapper.GetTypeMapping(target).StoreTypeName;

            if (source.IncrementBy != target.IncrementBy
                || source.MaxValue != target.MaxValue
                || source.MinValue != target.MinValue
                || source.StartValue != target.StartValue
                || sourceStoreType != targetStoreType)
            {
                // TODO: What about StartValue and StoreType?
                yield return new AlterSequenceOperation(
                    source.Name,
                    source.Schema,
                    target.IncrementBy,
                    target.MinValue,
                    target.MaxValue);
            }
        }

        protected virtual ISequence TryMatch(ISequence source, IEnumerable<ISequence> targets)
        {
            var candidates = new List<ISequence>();
            foreach (var target in targets)
            {
                if (string.Equals(source.Name, target.Name, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(source.Schema, target.Schema, StringComparison.OrdinalIgnoreCase))
                    {
                        return target;
                    }
                    candidates.Add(target);
                }
            }

            return candidates.FirstOrDefault();
        }

        protected virtual IEnumerable<MigrationOperation> Add(ISequence target)
        {
            yield return new CreateSequenceOperation(
                target.Name,
                target.Schema,
                target.StartValue,
                target.IncrementBy,
                target.MinValue,
                target.MaxValue,
                TypeMapper.GetTypeMapping(target).StoreTypeName);
        }

        protected virtual IEnumerable<MigrationOperation> Remove(ISequence source)
        {
            yield return new DropSequenceOperation(source.Name, source.Schema);
        }

        #endregion

        // TODO: Better factoring?
        protected virtual IEnumerable<MigrationOperation> DiffCollection<T>(
            IEnumerable<T> sources,
            IEnumerable<T> targets,
            Func<T, T, IEnumerable<MigrationOperation>> diff,
            Func<T, IEnumerable<T>, T> tryMatch,
            Func<T, IEnumerable<MigrationOperation>> add,
            Func<T, IEnumerable<MigrationOperation>> remove)
        {
            var added = new List<T>(targets);
            foreach (var source in sources)
            {
                var target = tryMatch(source, added);
                if (target == null)
                {
                    foreach (var operation in remove(source))
                    {
                        yield return operation;
                    }
                }
                else
                {
                    added.Remove(target);
                    foreach (var operation in diff(source, target))
                    {
                        yield return operation;
                    }
                }
            }

            foreach (var operation in added.SelectMany(add))
            {
                yield return operation;
            }
        }

        private string Name([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var builder = new StringBuilder();

            var schema = entityType.Relational().Schema;
            if (schema != null)
            {
                builder
                    .Append(schema)
                    .Append(".");
            }

            builder.Append(entityType.Relational().Table);

            return builder.ToString();
        }

        private string Name([NotNull] IReadOnlyCollection<IProperty> properties)
        {
            Check.NotNull(properties, nameof(properties));

            return string.Join("_", properties.Select(Name));
        }

        private string Name([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.Relational().Column;
        }

        private string Name([NotNull] IKey key)
        {
            Check.NotNull(key, nameof(key));

            var name = key.Relational().Name;
            if (name != null)
            {
                return name;
            }

            var builder = new StringBuilder();

            builder
                .Append(key.IsPrimaryKey() ? "PK_" : "AK_")
                .Append(Name(key.EntityType));

            if (!key.IsPrimaryKey())
            {
                builder
                    .Append("_")
                    .Append(Name(key.Properties));
            }

            return builder.ToString();
        }

        private string Name([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            return foreignKey.Relational().Name
                   ?? "FK_" + Name(foreignKey.EntityType) + "_" + Name(foreignKey.PrincipalEntityType) + "_" +
                   Name(foreignKey.Properties);
        }

        private string Name([NotNull] IIndex index)
        {
            Check.NotNull(index, nameof(index));

            return index.Relational().Name ?? "IX_" + Name(index.EntityType) + "_" + Name(index.Properties);
        }
    }
}
