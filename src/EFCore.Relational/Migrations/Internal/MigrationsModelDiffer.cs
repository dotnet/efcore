// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class MigrationsModelDiffer : IMigrationsModelDiffer
    {
        private static readonly Type[] _dropOperationTypes =
        {
            typeof(DropIndexOperation),
            typeof(DropPrimaryKeyOperation),
            typeof(DropSequenceOperation),
            typeof(DropUniqueConstraintOperation),
            typeof(DropCheckConstraintOperation)
        };

        private static readonly Type[] _alterOperationTypes =
        {
            typeof(AddPrimaryKeyOperation), typeof(AddUniqueConstraintOperation), typeof(AlterSequenceOperation)
        };

        private static readonly Type[] _renameOperationTypes =
        {
            typeof(RenameColumnOperation), typeof(RenameIndexOperation), typeof(RenameSequenceOperation)
        };

        private static readonly Type[] _columnOperationTypes = { typeof(AddColumnOperation), typeof(AlterColumnOperation) };

        private static readonly Type[] _constraintOperationTypes = { typeof(AddForeignKeyOperation), typeof(CreateIndexOperation) };

        private IUpdateAdapter _sourceUpdateAdapter;
        private IUpdateAdapter _targetUpdateAdapter;
        private readonly List<SharedTableEntryMap<EntryMapping>> _sharedTableEntryMaps = new List<SharedTableEntryMap<EntryMapping>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public MigrationsModelDiffer(
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] IMigrationsAnnotationProvider migrationsAnnotations,
            [NotNull] IChangeDetector changeDetector,
            [NotNull] IUpdateAdapterFactory updateAdapterFactory,
            [NotNull] CommandBatchPreparerDependencies commandBatchPreparerDependencies)
        {
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            Check.NotNull(migrationsAnnotations, nameof(migrationsAnnotations));
            Check.NotNull(changeDetector, nameof(changeDetector));
            Check.NotNull(updateAdapterFactory, nameof(updateAdapterFactory));
            Check.NotNull(commandBatchPreparerDependencies, nameof(commandBatchPreparerDependencies));

            TypeMappingSource = typeMappingSource;
            MigrationsAnnotations = migrationsAnnotations;
            ChangeDetector = changeDetector;
            UpdateAdapterFactory = updateAdapterFactory;
            CommandBatchPreparerDependencies = commandBatchPreparerDependencies;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IRelationalTypeMappingSource TypeMappingSource { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IMigrationsAnnotationProvider MigrationsAnnotations { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IUpdateAdapterFactory UpdateAdapterFactory { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual CommandBatchPreparerDependencies CommandBatchPreparerDependencies { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IChangeDetector ChangeDetector { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool HasDifferences(IRelationalModel source, IRelationalModel target)
            => Diff(source, target, new DiffContext()).Any();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<MigrationOperation> GetDifferences(IRelationalModel source, IRelationalModel target)
        {
            var diffContext = new DiffContext();
            return Sort(Diff(source, target, diffContext), diffContext);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
            var createCheckConstraintOperations = new List<MigrationOperation>();
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
                else if (type == typeof(CreateCheckConstraintOperation))
                {
                    createCheckConstraintOperations.Add(operation);
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
                    leftovers.Add(operation);
                    Check.DebugAssert(false, "Unexpected operation type: " + operation.GetType());
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

            var dropTableGraph = new Multigraph<DropTableOperation, IForeignKeyConstraint>();
            dropTableGraph.AddVertices(dropTableOperations);
            foreach (var dropTableOperation in dropTableOperations)
            {
                var table = diffContext.FindTable(dropTableOperation);
                foreach (var foreignKey in table.ForeignKeyConstraints)
                {
                    var principalDropTableOperation = diffContext.FindDrop(foreignKey.PrincipalTable);
                    if (principalDropTableOperation != null
                        && principalDropTableOperation != dropTableOperation)
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
                .Concat(sourceDataOperations)
                .Concat(dropColumnOperations)
                .Concat(ensureSchemaOperations)
                .Concat(renameTableOperations)
                .Concat(renameOperations)
                .Concat(alterDatabaseOperations)
                .Concat(createSequenceOperations)
                .Concat(createCheckConstraintOperations)
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [CanBeNull] IRelationalModel source,
            [CanBeNull] IRelationalModel target,
            [NotNull] DiffContext diffContext)
        {
            TrackData(source, target);

            var schemaOperations = source != null && target != null
                ? DiffAnnotations(source, target)
                    .Concat(Diff(GetSchemas(source), GetSchemas(target), diffContext))
                    .Concat(Diff(source.Tables, target.Tables, diffContext))
                    .Concat(Diff(source.Sequences, target.Sequences, diffContext))
                    .Concat(
                        Diff(
                            source.Tables.SelectMany(s => s.ForeignKeyConstraints),
                            target.Tables.SelectMany(t => t.ForeignKeyConstraints),
                            diffContext))
                : target != null
                    ? Add(target, diffContext)
                    : source != null
                        ? Remove(source, diffContext)
                        : Enumerable.Empty<MigrationOperation>();

            return schemaOperations.Concat(GetDataOperations(diffContext));
        }

        private IEnumerable<MigrationOperation> DiffAnnotations(
            IRelationalModel source,
            IRelationalModel target)
        {
            var sourceMigrationsAnnotations = source?.GetAnnotations().ToList();
            var targetMigrationsAnnotations = target?.GetAnnotations().ToList();

            if (source == null)
            {
                if (targetMigrationsAnnotations?.Count > 0)
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
                    alterDatabaseOperation.OldDatabase.AddAnnotations(sourceMigrationsAnnotations);
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add([NotNull] IRelationalModel target, [NotNull] DiffContext diffContext)
            => DiffAnnotations(null, target)
                .Concat(GetSchemas(target).SelectMany(t => Add(t, diffContext)))
                .Concat(target.Tables.SelectMany(t => Add(t, diffContext)))
                .Concat(target.Sequences.SelectMany(t => Add(t, diffContext)))
                .Concat(target.Tables.SelectMany(t => t.ForeignKeyConstraints).SelectMany(k => Add(k, diffContext)));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] IRelationalModel source, [NotNull] DiffContext diffContext)
            => DiffAnnotations(source, null)
                .Concat(source.Tables.SelectMany(t => Remove(t, diffContext)))
                .Concat(source.Sequences.SelectMany(t => Remove(t, diffContext)));

        #endregion

        #region Schema

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<string> source, [NotNull] IEnumerable<string> target, [NotNull] DiffContext diffContext)
            => DiffCollection(
                source,
                target,
                diffContext,
                Diff,
                Add,
                Remove,
                (s, t, c) => s == t);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] string source, [NotNull] string target, [NotNull] DiffContext diffContext)
            => Enumerable.Empty<MigrationOperation>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add([NotNull] string target, [NotNull] DiffContext diffContext)
        {
            yield return new EnsureSchemaOperation { Name = target };
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] string source, [NotNull] DiffContext diffContext)
            => Enumerable.Empty<MigrationOperation>();

        #endregion

        #region IEntityType

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<ITable> source,
            [NotNull] IEnumerable<ITable> target,
            [NotNull] DiffContext diffContext)
            => DiffCollection(
                source,
                target,
                diffContext,
                Diff,
                Add,
                Remove,
                (s, t, c) => string.Equals(
                        s.Schema,
                        t.Schema,
                        StringComparison.OrdinalIgnoreCase)
                    && string.Equals(
                        s.Name,
                        t.Name,
                        StringComparison.OrdinalIgnoreCase),
                (s, t, c) => string.Equals(
                    s.Name,
                    t.Name,
                    StringComparison.OrdinalIgnoreCase),
                (s, t, c) => string.Equals(GetRootType(s).Name, GetRootType(t).Name, StringComparison.OrdinalIgnoreCase),
                (s, t, c) => s.EntityTypeMappings.Any(
                    se => t.EntityTypeMappings.Any(
                        te => string.Equals(se.EntityType.Name, te.EntityType.Name, StringComparison.OrdinalIgnoreCase))));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] ITable source,
            [NotNull] ITable target,
            [NotNull] DiffContext diffContext)
        {
            if (!source.IsMigratable
                || !target.IsMigratable)
            {
                yield break;
            }

            if (source.Schema != target.Schema
                || source.Name != target.Name)
            {
                yield return new RenameTableOperation
                {
                    Schema = source.Schema,
                    Name = source.Name,
                    NewSchema = target.Schema,
                    NewName = target.Name
                };
            }

            var sourceMigrationsAnnotations = source.GetAnnotations();
            var targetMigrationsAnnotations = target.GetAnnotations();

            if (source.Comment != target.Comment
                || HasDifferences(sourceMigrationsAnnotations, targetMigrationsAnnotations))
            {
                var alterTableOperation = new AlterTableOperation
                {
                    Name = target.Name,
                    Schema = target.Schema,
                    Comment = target.Comment,
                    OldTable = { Comment = source.Comment }
                };

                alterTableOperation.AddAnnotations(targetMigrationsAnnotations);
                alterTableOperation.OldTable.AddAnnotations(sourceMigrationsAnnotations);

                yield return alterTableOperation;
            }

            var operations = Diff(source.Columns, target.Columns, diffContext)
                .Concat(Diff(source.UniqueConstraints, target.UniqueConstraints, diffContext))
                .Concat(Diff(source.Indexes, target.Indexes, diffContext))
                .Concat(Diff(source.CheckConstraints, target.CheckConstraints, diffContext));

            foreach (var operation in operations)
            {
                yield return operation;
            }

            DiffData(source, target, diffContext);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add(
            [NotNull] ITable target, [NotNull] DiffContext diffContext)
        {
            if (!target.IsMigratable)
            {
                yield break;
            }

            var entityType = GetRootType(target);
            var createTableOperation = new CreateTableOperation
            {
                Schema = target.Schema,
                Name = target.Name,
                Comment = target.Comment
            };
            createTableOperation.AddAnnotations(target.GetAnnotations());

            createTableOperation.Columns.AddRange(
                GetSortedColumns(target).SelectMany(p => Add(p, diffContext, inline: true)).Cast<AddColumnOperation>());
            var primaryKey = target.PrimaryKey;
            if (primaryKey != null)
            {
                createTableOperation.PrimaryKey = Add(primaryKey, diffContext).Cast<AddPrimaryKeyOperation>().Single();
            }

            createTableOperation.UniqueConstraints.AddRange(
                target.UniqueConstraints.Where(c => !c.IsPrimaryKey).SelectMany(c => Add(c, diffContext))
                    .Cast<AddUniqueConstraintOperation>());
            createTableOperation.CheckConstraints.AddRange(
                target.CheckConstraints.SelectMany(c => Add(c, diffContext))
                    .Cast<CreateCheckConstraintOperation>());

            diffContext.AddCreate(target, createTableOperation);

            yield return createTableOperation;

            foreach (var operation in target.Indexes.SelectMany(i => Add(i, diffContext)))
            {
                yield return operation;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove(
            [NotNull] ITable source, [NotNull] DiffContext diffContext)
        {
            if (!source.IsMigratable)
            {
                yield break;
            }

            var operation = new DropTableOperation { Schema = source.Schema, Name = source.Name };
            operation.AddAnnotations(MigrationsAnnotations.ForRemove(source));

            diffContext.AddDrop(source, operation);

            yield return operation;
        }

        private static IEnumerable<IColumn> GetSortedColumns(ITable table)
        {
            var columns = table.Columns.ToHashSet();
            var sortedColumns = new List<IColumn>(columns.Count);
            foreach (var property in GetSortedProperties(GetRootType(table), table))
            {
                var column = property.GetTableColumnMappings().FirstOrDefault(m => m.TableMapping.Table == table)?.Column;
                if (columns.Remove(column))
                {
                    sortedColumns.Add(column);
                }
            }

            return sortedColumns;
        }

        private static IEnumerable<IProperty> GetSortedProperties(IEntityType entityType, ITable table)
        {
            var leastPriorityProperties = new List<IProperty>();
            var leastPriorityPrimaryKeyProperties = new List<IProperty>();
            var primaryKeyPropertyGroups = new Dictionary<PropertyInfo, IProperty>();
            var groups = new Dictionary<PropertyInfo, List<IProperty>>();
            var unorderedGroups = new Dictionary<PropertyInfo, SortedDictionary<int, IProperty>>();
            var types = new Dictionary<Type, SortedDictionary<int, PropertyInfo>>();

            foreach (var property in entityType.GetDeclaredProperties())
            {
                var clrProperty = property.PropertyInfo;
                if (clrProperty == null
                    || clrProperty.IsIndexerProperty())
                {
                    if (property.IsPrimaryKey())
                    {
                        leastPriorityPrimaryKeyProperties.Add(property);

                        continue;
                    }

                    var foreignKey = property.GetContainingForeignKeys()
                        .FirstOrDefault(fk => fk.DependentToPrincipal?.PropertyInfo != null);
                    if (foreignKey == null)
                    {
                        leastPriorityProperties.Add(property);

                        continue;
                    }

                    clrProperty = foreignKey.DependentToPrincipal.PropertyInfo;
                    var groupIndex = foreignKey.Properties.IndexOf(property);

                    unorderedGroups.GetOrAddNew(clrProperty).Add(groupIndex, property);
                }
                else
                {
                    if (property.IsPrimaryKey())
                    {
                        primaryKeyPropertyGroups.Add(clrProperty, property);
                    }

                    groups.Add(
                        clrProperty, new List<IProperty> { property });
                }

                var clrType = clrProperty.DeclaringType;
                var index = clrType.GetTypeInfo().DeclaredProperties
                    .IndexOf(clrProperty, PropertyInfoEqualityComparer.Instance);

                Check.DebugAssert(clrType != null, "clrType is null");
                types.GetOrAddNew(clrType)[index] = clrProperty;
            }

            foreach (var group in unorderedGroups)
            {
                groups.Add(group.Key, group.Value.Values.ToList());
            }

            foreach (var linkingForeignKey in table.GetReferencingInternalForeignKeys(entityType))
            {
                var linkingNavigationProperty = linkingForeignKey.PrincipalToDependent?.PropertyInfo;
                var properties = GetSortedProperties(linkingForeignKey.DeclaringEntityType, table).ToList();
                if (linkingNavigationProperty == null)
                {
                    leastPriorityProperties.AddRange(properties);

                    continue;
                }

                groups.Add(linkingNavigationProperty, properties);

                var clrType = linkingNavigationProperty.DeclaringType;
                var index = clrType.GetTypeInfo().DeclaredProperties
                    .IndexOf(linkingNavigationProperty, PropertyInfoEqualityComparer.Instance);

                Check.DebugAssert(clrType != null, "clrType is null");
                types.GetOrAddNew(clrType)[index] = linkingNavigationProperty;
            }

            var graph = new Multigraph<Type, object>();
            graph.AddVertices(types.Keys);

            foreach (var left in types.Keys)
            {
                var found = false;
                foreach (var baseType in left.GetBaseTypes())
                {
                    foreach (var right in types.Keys)
                    {
                        if (right == baseType)
                        {
                            graph.AddEdge(right, left, null);
                            found = true;

                            break;
                        }
                    }

                    if (found)
                    {
                        break;
                    }
                }
            }

            var sortedPropertyInfos = graph.TopologicalSort().SelectMany(e => types[e].Values).ToList();

            return sortedPropertyInfos
                .Select(pi => primaryKeyPropertyGroups.ContainsKey(pi) ? primaryKeyPropertyGroups[pi] : null)
                .Where(e => e != null)
                .Concat(leastPriorityPrimaryKeyProperties)
                .Concat(sortedPropertyInfos.Where(pi => !primaryKeyPropertyGroups.ContainsKey(pi)).SelectMany(p => groups[p]))
                .Concat(leastPriorityProperties)
                .Concat(entityType.GetDirectlyDerivedTypes().SelectMany(et => GetSortedProperties(et, table)));
        }

        private sealed class PropertyInfoEqualityComparer : IEqualityComparer<PropertyInfo>
        {
            private PropertyInfoEqualityComparer()
            {
            }

            public static readonly PropertyInfoEqualityComparer Instance = new PropertyInfoEqualityComparer();

            public bool Equals(PropertyInfo x, PropertyInfo y)
                => x.IsSameAs(y);

            public int GetHashCode(PropertyInfo obj)
                => throw new NotImplementedException();
        }

        #endregion

        #region IProperty

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<IColumn> source,
            [NotNull] IEnumerable<IColumn> target,
            [NotNull] DiffContext diffContext)
            => DiffCollection(
                source,
                target,
                diffContext,
                Diff,
                (t, c) => Add(t, c),
                Remove,
                (s, t, c) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase),
                (s, t, c) => s.PropertyMappings.Any(sm =>
                    t.PropertyMappings.Any(tm =>
                        string.Equals(sm.Property.Name, tm.Property.Name, StringComparison.OrdinalIgnoreCase)
                            && EntityTypePathEquals(sm.Property.DeclaringEntityType, tm.Property.DeclaringEntityType, c))),
                (s, t, c) => s.PropertyMappings.Any(sm =>
                    t.PropertyMappings.Any(tm =>
                        string.Equals(sm.Property.Name, tm.Property.Name, StringComparison.OrdinalIgnoreCase))),
                (s, t, c) => s.PropertyMappings.Any(sm =>
                    t.PropertyMappings.Any(tm =>
                        EntityTypePathEquals(sm.Property.DeclaringEntityType, tm.Property.DeclaringEntityType, c)
                            && PropertyStructureEquals(sm.Property, tm.Property))),
                (s, t, c) => s.PropertyMappings.Any(sm =>
                    t.PropertyMappings.Any(tm =>
                        PropertyStructureEquals(sm.Property, tm.Property))));

        private bool PropertyStructureEquals(IProperty source, IProperty target)
            => source.ClrType == target.ClrType
                && source.IsConcurrencyToken == target.IsConcurrencyToken
                && source.ValueGenerated == target.ValueGenerated
                && source.GetMaxLength() == target.GetMaxLength()
                && source.GetPrecision() == target.GetPrecision()
                && source.GetScale() == target.GetScale()
                && source.IsColumnNullable() == target.IsColumnNullable()
                && source.IsUnicode() == target.IsUnicode()
                && source.IsFixedLength() == target.IsFixedLength()
                && source.GetConfiguredColumnType() == target.GetConfiguredColumnType()
                && source.GetComputedColumnSql() == target.GetComputedColumnSql()
                && Equals(GetDefaultValue(source), GetDefaultValue(target))
                && source.GetDefaultValueSql() == target.GetDefaultValueSql();

        private static bool EntityTypePathEquals(IEntityType source, IEntityType target, DiffContext diffContext)
        {
            var sourceTable = diffContext.GetTable(source);
            var targetTable = diffContext.GetTable(target);

            if (sourceTable.EntityTypeMappings.Count() == 1
                && targetTable.EntityTypeMappings.Count() == 1)
            {
                return true;
            }

            if (!string.Equals(
                GetDefiningNavigationName(source),
                GetDefiningNavigationName(target),
                StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var nextSource = source.DefiningEntityType ?? source.BaseType;
            var nextTarget = target.DefiningEntityType ?? target.BaseType;
            return nextSource == null
                || !sourceTable.EntityTypeMappings.Any(m => m.EntityType == nextSource)
                || nextTarget == null
                || !targetTable.EntityTypeMappings.Any(m => m.EntityType == nextTarget)
                || EntityTypePathEquals(nextSource, nextTarget, diffContext);
        }

        private static string GetDefiningNavigationName(IEntityType entityType)
        {
            if (entityType.DefiningNavigationName != null)
            {
                return entityType.DefiningNavigationName;
            }

            var primaryKey = entityType.BaseType == null ? entityType.FindPrimaryKey() : null;
            if (primaryKey != null)
            {
                var definingForeignKey = entityType
                    .FindForeignKeys(primaryKey.Properties)
                    .FirstOrDefault(fk => fk.PrincipalEntityType.GetTableName() == entityType.GetTableName());
                if (definingForeignKey?.DependentToPrincipal != null)
                {
                    return definingForeignKey.DependentToPrincipal.Name;
                }
            }

            return entityType.Name;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IColumn source, [NotNull] IColumn target, [NotNull] DiffContext diffContext)
        {
            var sourceMapping = source.PropertyMappings.First();
            var targetMapping = target.PropertyMappings.First();
            var table = target.Table;

            if (source.Name != target.Name)
            {
                yield return new RenameColumnOperation
                {
                    Schema = table.Schema,
                    Table = table.Name,
                    Name = source.Name,
                    NewName = target.Name
                };
            }

            var sourceProperty = sourceMapping.Property;
            var targetProperty = targetMapping.Property;

            var sourceTypeMapping = sourceMapping.TypeMapping;
            var targetTypeMapping = targetMapping.TypeMapping;

            var sourceColumnType = source.Type ?? sourceTypeMapping.StoreType;
            var targetColumnType = target.Type ?? targetTypeMapping.StoreType;

            var sourceMigrationsAnnotations = source.GetAnnotations();
            var targetMigrationsAnnotations = target.GetAnnotations();

            var isNullableChanged = source.IsNullable != target.IsNullable;
            var columnTypeChanged = sourceColumnType != targetColumnType;

            if (isNullableChanged
                || columnTypeChanged
                || source.DefaultValueSql != target.DefaultValueSql
                || source.ComputedColumnSql != target.ComputedColumnSql
                || !Equals(GetDefaultValue(sourceProperty, GetValueConverter(sourceProperty, sourceTypeMapping)),
                    GetDefaultValue(targetProperty, GetValueConverter(sourceProperty, targetTypeMapping)))
                || source.Comment != target.Comment
                || HasDifferences(sourceMigrationsAnnotations, targetMigrationsAnnotations))
            {
                var isDestructiveChange = isNullableChanged && source.IsNullable
                    // TODO: Detect type narrowing
                    || columnTypeChanged;

                var alterColumnOperation = new AlterColumnOperation
                {
                    Schema = table.Schema,
                    Table = table.Name,
                    Name = target.Name,
                    IsDestructiveChange = isDestructiveChange
                };

                Initialize(
                    alterColumnOperation, target, targetTypeMapping,
                    target.IsNullable, targetMigrationsAnnotations, inline: true);

                Initialize(
                    alterColumnOperation.OldColumn, source, sourceTypeMapping,
                    source.IsNullable, sourceMigrationsAnnotations, inline: true);

                yield return alterColumnOperation;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add(
            [NotNull] IColumn target,
            [NotNull] DiffContext diffContext,
            bool inline = false)
        {
            var table = target.Table;

            var operation = new AddColumnOperation
            {
                Schema = table.Schema,
                Table = table.Name,
                Name = target.Name
            };

            var targetMapping = target.PropertyMappings.First();
            var targetTypeMapping = targetMapping.TypeMapping;

            Initialize(
                operation, target, targetTypeMapping, target.IsNullable,
                target.GetAnnotations(), inline);

            yield return operation;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] IColumn source, [NotNull] DiffContext diffContext)
        {
            var table = source.Table;

            var operation = new DropColumnOperation
            {
                Schema = table.Schema,
                Table = table.Name,
                Name = source.Name
            };
            operation.AddAnnotations(MigrationsAnnotations.ForRemove(source));

            yield return operation;
        }

        private void Initialize(
            ColumnOperation columnOperation,
            IColumn column,
            RelationalTypeMapping typeMapping,
            bool isNullable,
            IEnumerable<IAnnotation> migrationsAnnotations,
            bool inline = false)
        {
            var property = column.PropertyMappings.First().Property;
            var valueConverter = GetValueConverter(property, typeMapping);
            columnOperation.ClrType
                = (valueConverter?.ProviderClrType
                    ?? typeMapping.ClrType).UnwrapNullableType();

            columnOperation.ColumnType = column.Type;
            columnOperation.MaxLength = column.MaxLength;
            columnOperation.Precision = column.Precision;
            columnOperation.Scale = column.Scale;
            columnOperation.IsUnicode = column.IsUnicode;
            columnOperation.IsFixedLength = column.IsFixedLength;
            columnOperation.IsRowVersion = column.IsRowVersion;
            columnOperation.IsNullable = isNullable;

            var defaultValue = GetDefaultValue(property, valueConverter);
            columnOperation.DefaultValue = (defaultValue == DBNull.Value ? null : defaultValue)
                ?? (inline || isNullable
                    ? null
                    : GetDefaultValue(columnOperation.ClrType));

            columnOperation.DefaultValueSql = column.DefaultValueSql;
            columnOperation.ComputedColumnSql = column.ComputedColumnSql;
            columnOperation.Comment = column.Comment;
            columnOperation.AddAnnotations(migrationsAnnotations);
        }

        #endregion

        #region IKey

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<IUniqueConstraint> source,
            [NotNull] IEnumerable<IUniqueConstraint> target,
            [NotNull] DiffContext diffContext)
            => DiffCollection(
                source,
                target,
                diffContext,
                Diff,
                Add,
                Remove,
                (s, t, c) => s.Name == t.Name
                    && s.Columns.Select(p => p.Name).SequenceEqual(
                        t.Columns.Select(p => c.FindSource(p)?.Name))
                    && s.IsPrimaryKey == t.IsPrimaryKey
                    && !HasDifferences(s.GetAnnotations(), t.GetAnnotations()));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IUniqueConstraint source,
            [NotNull] IUniqueConstraint target,
            [NotNull] DiffContext diffContext)
            => Enumerable.Empty<MigrationOperation>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add([NotNull] IUniqueConstraint target, [NotNull] DiffContext diffContext)
        {
            var targetTable = target.Table;
            var columns = target.Columns;

            MigrationOperation operation;
            if (target.IsPrimaryKey)
            {
                operation = new AddPrimaryKeyOperation
                {
                    Schema = targetTable.Schema,
                    Table = targetTable.Name,
                    Name = target.Name,
                    Columns = columns.Select(c => c.Name).ToArray()
                };
            }
            else
            {
                operation = new AddUniqueConstraintOperation
                {
                    Schema = targetTable.Schema,
                    Table = targetTable.Name,
                    Name = target.Name,
                    Columns = columns.Select(c => c.Name).ToArray()
                };
            }

            operation.AddAnnotations(target.GetAnnotations());

            yield return operation;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove(
            [NotNull] IUniqueConstraint source,
            [NotNull] DiffContext diffContext)
        {
            var table = source.Table;

            MigrationOperation operation;
            if (source.IsPrimaryKey)
            {
                operation = new DropPrimaryKeyOperation
                {
                    Schema = table.Schema,
                    Table = table.Name,
                    Name = source.Name
                };
            }
            else
            {
                operation = new DropUniqueConstraintOperation
                {
                    Schema = table.Schema,
                    Table = table.Name,
                    Name = source.Name
                };
            }

            operation.AddAnnotations(MigrationsAnnotations.ForRemove(source));

            yield return operation;
        }

        #endregion

        #region IForeignKey

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<IForeignKeyConstraint> source,
            [NotNull] IEnumerable<IForeignKeyConstraint> target,
            [NotNull] DiffContext diffContext)
            => DiffCollection(
                source,
                target,
                diffContext,
                Diff,
                Add,
                Remove,
                (s, t, context) => s.Name == t.Name
                    && s.Columns.Select(c => c.Name).SequenceEqual(
                        t.Columns.Select(c => context.FindSource(c)?.Name))
                    && s.PrincipalTable == context.FindSource(t.PrincipalTable)
                    && s.PrincipalColumns.Select(c => c.Name).SequenceEqual(
                        t.PrincipalColumns.Select(c => context.FindSource(c)?.Name))
                    && s.OnDeleteAction == t.OnDeleteAction
                    && !HasDifferences(s.GetAnnotations(), t.GetAnnotations()));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IForeignKeyConstraint source, [NotNull] IForeignKeyConstraint target, [NotNull] DiffContext diffContext)
            => Enumerable.Empty<MigrationOperation>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add([NotNull] IForeignKeyConstraint target, [NotNull] DiffContext diffContext)
        {
            var targetTable = target.Table;
            if (!targetTable.IsMigratable)
            {
                yield break;
            }

            var targetPrincipleTable = target.PrincipalTable;

            var operation = new AddForeignKeyOperation
            {
                Schema = targetTable.Schema,
                Table = targetTable.Name,
                Name = target.Name,
                Columns = target.Columns.Select(c => c.Name).ToArray(),
                PrincipalSchema = targetPrincipleTable.Schema,
                PrincipalTable = targetPrincipleTable.Name,
                PrincipalColumns = target.PrincipalColumns.Select(c => c.Name).ToArray(),
                OnDelete = target.OnDeleteAction
            };
            operation.AddAnnotations(target.GetAnnotations());

            var createTableOperation = diffContext.FindCreate(targetTable);
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] IForeignKeyConstraint source, [NotNull] DiffContext diffContext)
        {
            var sourceTable = source.Table;

            var dropTableOperation = diffContext.FindDrop(sourceTable);
            if (dropTableOperation == null)
            {
                var operation = new DropForeignKeyOperation
                {
                    Schema = sourceTable.Schema,
                    Table = sourceTable.Name,
                    Name = source.Name
                };
                operation.AddAnnotations(MigrationsAnnotations.ForRemove(source));

                yield return operation;
            }
        }

        #endregion

        #region IIndex

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<ITableIndex> source,
            [NotNull] IEnumerable<ITableIndex> target,
            [NotNull] DiffContext diffContext)
            => DiffCollection(
                source,
                target,
                diffContext,
                Diff,
                Add,
                Remove,
                (s, t, c) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase)
                    && IndexStructureEquals(s, t, c),
                (s, t, c) => IndexStructureEquals(s, t, c));

        private bool IndexStructureEquals(ITableIndex source, ITableIndex target, DiffContext diffContext)
            => source.IsUnique == target.IsUnique
                && source.Filter == target.Filter
                && !HasDifferences(source.GetAnnotations(), target.GetAnnotations())
                && source.Columns.Select(p => p.Name).SequenceEqual(
                    target.Columns.Select(p => diffContext.FindSource(p)?.Name));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] ITableIndex source,
            [NotNull] ITableIndex target,
            [NotNull] DiffContext diffContext)
        {
            var targetTable = target.Table;
            var sourceName = source.Name;
            var targetName = target.Name;

            if (sourceName != targetName)
            {
                yield return new RenameIndexOperation
                {
                    Schema = targetTable.Schema,
                    Table = targetTable.Name,
                    Name = sourceName,
                    NewName = targetName
                };
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add(
            [NotNull] ITableIndex target,
            [NotNull] DiffContext diffContext)
        {
            var targetTable = target.Table;

            var operation = new CreateIndexOperation
            {
                Name = target.Name,
                Schema = targetTable.Schema,
                Table = targetTable.Name,
                Columns = target.Columns.Select(c => c.Name).ToArray(),
                IsUnique = target.IsUnique,
                Filter = target.Filter
            };
            operation.AddAnnotations(target.GetAnnotations());

            yield return operation;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] ITableIndex source, [NotNull] DiffContext diffContext)
        {
            var sourceTable = source.Table;

            var operation = new DropIndexOperation
            {
                Name = source.Name,
                Schema = sourceTable.Schema,
                Table = sourceTable.Name
            };
            operation.AddAnnotations(MigrationsAnnotations.ForRemove(source));

            yield return operation;
        }

        #endregion

        #region ICheckConstraint

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<ICheckConstraint> source,
            [NotNull] IEnumerable<ICheckConstraint> target,
            [NotNull] DiffContext diffContext)
            => DiffCollection(
                source,
                target,
                diffContext,
                Diff,
                Add,
                Remove,
                (s, t, c) => c.GetTable(s.EntityType) == c.FindSource(c.GetTable(t.EntityType))
                    && string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(s.Sql, t.Sql, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] ICheckConstraint source, [NotNull] ICheckConstraint target, [NotNull] DiffContext diffContext)
            => Enumerable.Empty<MigrationOperation>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add([NotNull] ICheckConstraint target, [NotNull] DiffContext diffContext)
        {
            var targetEntityType = target.EntityType.GetRootType();

            var operation = new CreateCheckConstraintOperation
            {
                Name = target.Name,
                Sql = target.Sql,
                Schema = targetEntityType.GetSchema(),
                Table = targetEntityType.GetTableName()
            };

            operation.Sql = target.Sql;
            operation.AddAnnotations(target.GetAnnotations());

            yield return operation;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] ICheckConstraint source, [NotNull] DiffContext diffContext)
        {
            var sourceEntityType = source.EntityType.GetRootType();

            var operation = new DropCheckConstraintOperation
            {
                Name = source.Name,
                Schema = sourceEntityType.GetSchema(),
                Table = sourceEntityType.GetTableName()
            };
            operation.AddAnnotations(MigrationsAnnotations.ForRemove(source));

            yield return operation;
        }

        #endregion

        #region ISequence

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] IEnumerable<ISequence> source,
            [NotNull] IEnumerable<ISequence> target,
            [NotNull] DiffContext diffContext)
            => DiffCollection(
                source,
                target,
                diffContext,
                Diff,
                Add,
                Remove,
                (s, t, c) => string.Equals(s.Schema, t.Schema, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase)
                    && s.ClrType == t.ClrType,
                (s, t, c) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase)
                    && s.ClrType == t.ClrType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] ISequence source, [NotNull] ISequence target, [NotNull] DiffContext diffContext)
        {
            if (source.Schema != target.Schema
                || source.Name != target.Name)
            {
                yield return new RenameSequenceOperation
                {
                    Schema = source.Schema,
                    Name = source.Name,
                    NewSchema = target.Schema,
                    NewName = target.Name
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

            var sourceMigrationsAnnotations = source.GetAnnotations();
            var targetMigrationsAnnotations = target.GetAnnotations();

            if (source.IncrementBy != target.IncrementBy
                || source.MaxValue != target.MaxValue
                || source.MinValue != target.MinValue
                || source.IsCyclic != target.IsCyclic
                || HasDifferences(sourceMigrationsAnnotations, targetMigrationsAnnotations))
            {
                var alterSequenceOperation = new AlterSequenceOperation { Schema = target.Schema, Name = target.Name };
                Initialize(alterSequenceOperation, target, targetMigrationsAnnotations);

                Initialize(alterSequenceOperation.OldSequence, source, sourceMigrationsAnnotations);

                yield return alterSequenceOperation;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add([NotNull] ISequence target, [NotNull] DiffContext diffContext)
        {
            var operation = new CreateSequenceOperation
            {
                Schema = target.Schema,
                Name = target.Name,
                ClrType = target.ClrType,
                StartValue = target.StartValue
            };

            yield return Initialize(operation, target, target.GetAnnotations());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] ISequence source, [NotNull] DiffContext diffContext)
        {
            var operation = new DropSequenceOperation { Schema = source.Schema, Name = source.Name };
            operation.AddAnnotations(MigrationsAnnotations.ForRemove(source));

            yield return operation;
        }

        private static SequenceOperation Initialize(
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

        #region Data

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void TrackData(
            [CanBeNull] IRelationalModel source,
            [CanBeNull] IRelationalModel target)
        {
            if (target == null)
            {
                _targetUpdateAdapter = null;
                return;
            }

            _targetUpdateAdapter = UpdateAdapterFactory.CreateStandalone(target.Model);
            _targetUpdateAdapter.CascadeDeleteTiming = CascadeTiming.Never;

            foreach (var targetEntityType in target.Model.GetEntityTypes())
            {
                foreach (var targetSeed in targetEntityType.GetSeedData())
                {
                    _targetUpdateAdapter
                        .CreateEntry(targetSeed, targetEntityType)
                        .EntityState = EntityState.Added;
                }
            }

            if (source == null)
            {
                _sourceUpdateAdapter = null;
                return;
            }

            _sourceUpdateAdapter = UpdateAdapterFactory.CreateStandalone(source.Model);
            _sourceUpdateAdapter.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;

            foreach (var sourceEntityType in source.Model.GetEntityTypes())
            {
                foreach (var sourceSeed in sourceEntityType.GetSeedData())
                {
                    var entry = _sourceUpdateAdapter
                        .CreateEntry(sourceSeed, sourceEntityType);

                    // Mark as added first to generate missing values
                    // Issue #15289
                    entry.EntityState = EntityState.Added;
                    entry.EntityState = EntityState.Unchanged;
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void DiffData(
            [NotNull] ITable source,
            [NotNull] ITable target,
            [NotNull] DiffContext diffContext)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(target, nameof(target));
            Check.NotNull(diffContext, nameof(diffContext));

            var targetTableEntryMappingMap = SharedTableEntryMap<List<IUpdateEntry>>.CreateSharedTableEntryMapFactory(
                    target,
                    _targetUpdateAdapter)
                ((_, __, ___) => new List<IUpdateEntry>());

            foreach (var targetMapping in target.EntityTypeMappings)
            {
                var targetEntityType = targetMapping.EntityType;
                foreach (var targetSeed in targetEntityType.GetSeedData())
                {
                    var targetEntry = GetEntry(targetSeed, targetEntityType, _targetUpdateAdapter);
                    var targetEntries = targetTableEntryMappingMap.GetOrAddValue(targetEntry);
                    targetEntries.Add(targetEntry);
                }
            }

            var targetKeys = target.EntityTypeMappings
                .SelectMany(m => m.EntityType.GetDeclaredKeys()).Where(k => k.IsPrimaryKey()).ToList();
            var keyMapping = new Dictionary<IEntityType,
                Dictionary<IKey, List<(IProperty Property, ValueConverter SourceConverter, ValueConverter TargetConverter)>>>();
            foreach (var sourceMapping in source.EntityTypeMappings)
            {
                var sourceEntityType = sourceMapping.EntityType;
                foreach (var targetKey in targetKeys)
                {
                    var keyPropertiesMap = new List<(IProperty, ValueConverter, ValueConverter)>();
                    foreach (var keyProperty in targetKey.Properties)
                    {
                        var targetColumn = keyProperty.GetTableColumnMappings().Single(m => m.TableMapping.Table == target).Column;
                        var sourceColumn = diffContext.FindSource(targetColumn);
                        if (sourceColumn == null)
                        {
                            break;
                        }

                        foreach (var sourceProperty in sourceColumn.PropertyMappings.Select(m => m.Property))
                        {
                            if (!sourceProperty.DeclaringEntityType.IsAssignableFrom(sourceEntityType))
                            {
                                continue;
                            }

                            var sourceConverter = GetValueConverter(sourceProperty);
                            var targetConverter = GetValueConverter(keyProperty);
                            if (sourceProperty.ClrType != keyProperty.ClrType
                                && (sourceConverter == null || sourceConverter.ProviderClrType != keyProperty.ClrType)
                                && (targetConverter == null || targetConverter.ProviderClrType != sourceProperty.ClrType))
                            {
                                continue;
                            }

                            keyPropertiesMap.Add((sourceProperty, sourceConverter, targetConverter));
                            break;
                        }
                    }

                    if (keyPropertiesMap.Count == targetKey.Properties.Count)
                    {
                        keyMapping.GetOrAddNew(sourceEntityType)[targetKey] = keyPropertiesMap;
                    }
                }
            }

            var sourceTableEntryMappingMap = SharedTableEntryMap<EntryMapping>.CreateSharedTableEntryMapFactory(
                    source,
                    _sourceUpdateAdapter)
                ((_, __, ___) => new EntryMapping());
            _sharedTableEntryMaps.Add(sourceTableEntryMappingMap);

            foreach (var sourceMapping in source.EntityTypeMappings)
            {
                var sourceEntityType = sourceMapping.EntityType;
                foreach (var sourceSeed in sourceEntityType.GetSeedData())
                {
                    var sourceEntry = GetEntry(sourceSeed, sourceEntityType, _sourceUpdateAdapter);
                    var entryMapping = sourceTableEntryMappingMap.GetOrAddValue(sourceEntry);
                    entryMapping.SourceEntries.Add(sourceEntry);

                    if (!keyMapping.TryGetValue(sourceEntityType, out var targetKeyMap))
                    {
                        entryMapping.RecreateRow = true;
                        continue;
                    }

                    foreach (var targetKey in targetKeys)
                    {
                        if (!targetKeyMap.TryGetValue(targetKey, out var keyPropertiesMap))
                        {
                            continue;
                        }

                        var targetKeyValues = new object[keyPropertiesMap.Count];
                        for (var i = 0; i < keyPropertiesMap.Count; i++)
                        {
                            var (sourceProperty, sourceConverter, targetConverter) = keyPropertiesMap[i];
                            var sourceValue = sourceEntry.GetCurrentValue(sourceProperty);
                            targetKeyValues[i] = targetKey.Properties[i].ClrType != sourceProperty.ClrType
                                ? sourceConverter != null
                                    ? sourceConverter.ConvertToProvider(sourceValue)
                                    : targetConverter.ConvertFromProvider(sourceValue)
                                : sourceValue;
                        }

                        var entry = _targetUpdateAdapter.TryGetEntry(targetKey, targetKeyValues);
                        if (entry == null)
                        {
                            continue;
                        }

                        foreach (var targetEntry in targetTableEntryMappingMap.GetOrAddValue(entry))
                        {
                            if (!entryMapping.TargetEntries.Add(targetEntry))
                            {
                                continue;
                            }

                            foreach (var targetProperty in targetEntry.EntityType.GetProperties())
                            {
                                if (targetProperty.GetAfterSaveBehavior() == PropertySaveBehavior.Save)
                                {
                                    targetEntry.SetOriginalValue(targetProperty, targetProperty.ClrType.GetDefaultValue());
                                }
                            }

                            targetEntry.EntityState = EntityState.Unchanged;
                        }

                        if (sourceEntry.EntityState == EntityState.Deleted
                            || entryMapping.RecreateRow)
                        {
                            entryMapping.RecreateRow = true;
                            continue;
                        }

                        foreach (var targetProperty in entry.EntityType.GetProperties())
                        {
                            if (targetProperty.ValueGenerated != ValueGenerated.Never)
                            {
                                continue;
                            }

                            var targetColumn = targetProperty.GetTableColumnMappings().Single(m => m.TableMapping.Table == target).Column;
                            var sourceColumn = diffContext.FindSource(targetColumn);
                            if (sourceColumn == null)
                            {
                                continue;
                            }

                            var sourceProperty = sourceColumn.PropertyMappings.Select(m => m.Property)
                                .SingleOrDefault(p => p.DeclaringEntityType.IsAssignableFrom(sourceEntityType));
                            if (sourceProperty == null)
                            {
                                continue;
                            }

                            var sourceValue = sourceEntry.GetCurrentValue(sourceProperty);
                            var targetValue = entry.GetCurrentValue(targetProperty);
                            var comparer = targetProperty.GetValueComparer()
                                ?? sourceProperty.GetValueComparer();

                            var modelValuesChanged
                                = sourceProperty.ClrType.UnwrapNullableType() == targetProperty.ClrType.UnwrapNullableType()
                                && comparer?.Equals(sourceValue, targetValue) == false;

                            if (!modelValuesChanged)
                            {
                                var sourceConverter = GetValueConverter(sourceProperty);
                                var targetConverter = GetValueConverter(targetProperty);

                                var convertedSourceValue = sourceConverter == null
                                    ? sourceValue
                                    : sourceConverter.ConvertToProvider(sourceValue);

                                var convertedTargetValue = targetConverter == null
                                    ? targetValue
                                    : targetConverter.ConvertToProvider(targetValue);

                                var convertedType = sourceConverter?.ProviderClrType
                                    ?? targetConverter?.ProviderClrType;

                                if (convertedType != null
                                    && !convertedType.IsNullableType())
                                {
                                    var defaultValue = convertedType.GetDefaultValue();
                                    convertedSourceValue ??= defaultValue;
                                    convertedTargetValue ??= defaultValue;
                                }

                                var storeValuesChanged = convertedSourceValue?.GetType().UnwrapNullableType()
                                    != convertedTargetValue?.GetType().UnwrapNullableType();

                                if (!storeValuesChanged
                                    && convertedType != null)
                                {
                                    comparer = TypeMappingSource.FindMapping(convertedType)?.Comparer;

                                    storeValuesChanged = !comparer?.Equals(convertedSourceValue, convertedTargetValue)
                                        ?? !Equals(convertedSourceValue, convertedTargetValue);
                                }

                                if (!storeValuesChanged)
                                {
                                    entry.SetOriginalValue(targetProperty, entry.GetCurrentValue(targetProperty));

                                    continue;
                                }
                            }

                            if (targetProperty.GetAfterSaveBehavior() != PropertySaveBehavior.Save)
                            {
                                entryMapping.RecreateRow = true;
                                break;
                            }

                            entry.SetPropertyModified(targetProperty);
                        }
                    }
                }
            }
        }

        private static IUpdateEntry GetEntry(
            IDictionary<string, object> sourceSeed, IEntityType sourceEntityType, IUpdateAdapter updateAdapter)
        {
            var key = sourceEntityType.FindPrimaryKey();
            var keyValues = new object[key.Properties.Count];
            for (var i = 0; i < keyValues.Length; i++)
            {
                keyValues[i] = sourceSeed[key.Properties[i].Name];
            }

            return updateAdapter.TryGetEntry(key, keyValues);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> GetDataOperations([NotNull] DiffContext diffContext)
        {
            foreach (var sourceTableEntryMappingMap in _sharedTableEntryMaps)
            {
                foreach (var entryMapping in sourceTableEntryMappingMap.Values)
                {
                    if (entryMapping.RecreateRow
                        || entryMapping.TargetEntries.Count == 0)
                    {
                        foreach (var sourceEntry in entryMapping.SourceEntries)
                        {
                            sourceEntry.EntityState = EntityState.Deleted;
                            _sourceUpdateAdapter.CascadeDelete(
                                sourceEntry,
                                sourceEntry.EntityType.GetReferencingForeignKeys()
                                    .Where(
                                        fk =>
                                        {
                                            var behavior = diffContext.FindTarget(fk)?.DeleteBehavior;
                                            return behavior != null && behavior != DeleteBehavior.ClientNoAction;
                                        }));
                        }
                    }
                }
            }

            foreach (var sourceTableEntryMappingMap in _sharedTableEntryMaps)
            {
                foreach (var entryMapping in sourceTableEntryMappingMap.Values)
                {
                    if (entryMapping.SourceEntries.Any(e => e.EntityState == EntityState.Deleted))
                    {
                        foreach (var targetEntry in entryMapping.TargetEntries)
                        {
                            targetEntry.EntityState = EntityState.Added;
                        }

                        foreach (var sourceEntry in entryMapping.SourceEntries)
                        {
                            sourceEntry.EntityState = EntityState.Deleted;
                        }
                    }
                }
            }

            _sharedTableEntryMaps.Clear();

            foreach (var updateAdapter in new[] { _sourceUpdateAdapter, _targetUpdateAdapter })
            {
                if (updateAdapter == null)
                {
                    continue;
                }

                updateAdapter.DetectChanges();
                var entries = updateAdapter.GetEntriesToSave();
                if (entries == null
                    || entries.Count == 0)
                {
                    continue;
                }

                var model = updateAdapter.Model.GetRelationalModel();
                var commandBatches = new CommandBatchPreparer(CommandBatchPreparerDependencies)
                    .BatchCommands(entries, updateAdapter);

                foreach (var commandBatch in commandBatches)
                {
                    InsertDataOperation batchInsertOperation = null;
                    foreach (var c in commandBatch.ModificationCommands)
                    {
                        if (c.EntityState == EntityState.Added)
                        {
                            if (batchInsertOperation != null)
                            {
                                if (batchInsertOperation.Table == c.TableName
                                    && batchInsertOperation.Schema == c.Schema
                                    && batchInsertOperation.Columns.SequenceEqual(
                                        c.ColumnModifications.Where(col => col.IsKey || col.IsWrite).Select(col => col.ColumnName)))
                                {
                                    batchInsertOperation.Values =
                                        AddToMultidimensionalArray(
                                            c.ColumnModifications.Where(col => col.IsKey || col.IsWrite).Select(GetValue).ToList(),
                                            batchInsertOperation.Values);
                                    continue;
                                }

                                yield return batchInsertOperation;
                            }

                            batchInsertOperation = new InsertDataOperation
                            {
                                Schema = c.Schema,
                                Table = c.TableName,
                                Columns = c.ColumnModifications.Where(col => col.IsKey || col.IsWrite).Select(col => col.ColumnName)
                                    .ToArray(),
                                Values = ToMultidimensionalArray(
                                    c.ColumnModifications.Where(col => col.IsKey || col.IsWrite).Select(GetValue).ToList())
                            };
                        }
                        else if (c.EntityState == EntityState.Modified)
                        {
                            if (batchInsertOperation != null)
                            {
                                yield return batchInsertOperation;
                                batchInsertOperation = null;
                            }

                            yield return new UpdateDataOperation
                            {
                                Schema = c.Schema,
                                Table = c.TableName,
                                KeyColumns = c.ColumnModifications.Where(col => col.IsKey).Select(col => col.ColumnName).ToArray(),
                                KeyValues = ToMultidimensionalArray(
                                    c.ColumnModifications.Where(col => col.IsKey).Select(GetValue).ToList()),
                                Columns = c.ColumnModifications.Where(col => col.IsWrite).Select(col => col.ColumnName).ToArray(),
                                Values = ToMultidimensionalArray(
                                    c.ColumnModifications.Where(col => col.IsWrite).Select(GetValue).ToList())
                            };
                        }
                        else
                        {
                            if (batchInsertOperation != null)
                            {
                                yield return batchInsertOperation;
                                batchInsertOperation = null;
                            }

                            if (diffContext.FindDrop(model.FindTable(c.TableName, c.Schema)) == null)
                            {
                                yield return new DeleteDataOperation
                                {
                                    Schema = c.Schema,
                                    Table = c.TableName,
                                    KeyColumns = c.ColumnModifications.Where(col => col.IsKey).Select(col => col.ColumnName).ToArray(),
                                    KeyValues = ToMultidimensionalArray(
                                        c.ColumnModifications.Where(col => col.IsKey).Select(GetValue).ToArray())
                                };
                            }
                        }
                    }

                    if (batchInsertOperation != null)
                    {
                        yield return batchInsertOperation;
                    }
                }
            }
        }

        private object GetValue(ColumnModification columnModification)
        {
            var converter = GetValueConverter(columnModification.Property);
            var value = columnModification.UseCurrentValueParameter
                ? columnModification.Value
                : columnModification.OriginalValue;
            return converter != null
                ? converter.ConvertToProvider(value)
                : value;
        }

        #endregion

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> DiffCollection<T>(
            [NotNull] IEnumerable<T> sources,
            [NotNull] IEnumerable<T> targets,
            [NotNull] DiffContext diffContext,
            [NotNull] Func<T, T, DiffContext, IEnumerable<MigrationOperation>> diff,
            [NotNull] Func<T, DiffContext, IEnumerable<MigrationOperation>> add,
            [NotNull] Func<T, DiffContext, IEnumerable<MigrationOperation>> remove,
            [NotNull] params Func<T, T, DiffContext, bool>[] predicates)
        {
            var sourceList = sources.ToList();
            var targetList = targets.ToList();
            var pairedList = new List<(T source, T target)>();

            foreach (var predicate in predicates)
            {
                for (var i = sourceList.Count - 1; i >= 0; i--)
                {
                    var source = sourceList[i];

                    for (var j = targetList.Count - 1; j >= 0; j--)
                    {
                        var target = targetList[j];

                        if (predicate(source, target, diffContext))
                        {
                            sourceList.RemoveAt(i);
                            targetList.RemoveAt(j);
                            pairedList.Add((source, target));
                            diffContext.AddMapping(source, target);

                            break;
                        }
                    }
                }
            }

            foreach (var (source, target) in pairedList)
            {
                foreach (var operation in diff(source, target, diffContext))
                {
                    yield return operation;
                }
            }

            foreach (var source in sourceList)
            {
                foreach (var operation in remove(source, diffContext))
                {
                    yield return operation;
                }
            }

            foreach (var target in targetList)
            {
                foreach (var operation in add(target, diffContext))
                {
                    yield return operation;
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual string[] GetColumns([NotNull] IEnumerable<IProperty> properties)
            => properties.Select(p => p.GetColumnName()).ToArray();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual bool HasDifferences([NotNull] IEnumerable<IAnnotation> source, [NotNull] IEnumerable<IAnnotation> target)
        {
            var unmatched = new List<IAnnotation>(target);

            foreach (var annotation in source)
            {
                var index = unmatched.FindIndex(
                    a => a.Name == annotation.Name && StructuralComparisons.StructuralEqualityComparer.Equals(a.Value, annotation.Value));
                if (index == -1)
                {
                    return true;
                }

                unmatched.RemoveAt(index);
            }

            return unmatched.Count != 0;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<string> GetSchemas([NotNull] IRelationalModel model)
            => model.Tables.Where(t => t.IsMigratable).Select(t => t.Schema)
                .Concat(model.Views.Where(t => t.ViewDefinition != null).Select(s => s.Schema))
                .Concat(model.Sequences.Select(s => s.Schema))
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual object GetDefaultValue([NotNull] Type type)
            => type == typeof(string)
                ? string.Empty
                : type.IsArray
                    ? Array.CreateInstance(type.GetElementType(), 0)
                    : type.UnwrapNullableType().GetDefaultValue();

        private object GetDefaultValue(IProperty property, ValueConverter valueConverter = null)
        {
            var value = property.GetDefaultValue();
            var converter = valueConverter ?? GetValueConverter(property);
            return converter != null
                ? converter.ConvertToProvider(value)
                : value;
        }

        private ValueConverter GetValueConverter(IProperty property, RelationalTypeMapping typeMapping = null)
            => property.GetValueConverter() ?? (property.FindRelationalTypeMapping() ?? typeMapping)?.Converter;

        private static IEntityType GetRootType(ITable table)
            => table.EntityTypeMappings.Select(m => m.EntityType).FirstOrDefault(
                t => t.BaseType == null && !table.GetInternalForeignKeys(t).Any());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IProperty[] GetMappedProperties([NotNull] ITable table, [NotNull] string[] names)
        {
            var properties = new IProperty[names.Length];
            for (var i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var column = table.FindColumn(name);
                if (column == null)
                {
                    continue;
                }

                properties[i] = column.PropertyMappings.First().Property;
            }

            return properties;
        }

        private static object[,] ToMultidimensionalArray(IReadOnlyList<object> values)
        {
            var result = new object[1, values.Count];
            for (var i = 0; i < values.Count; i++)
            {
                result[0, i] = values[i];
            }

            return result;
        }

        private static object[,] AddToMultidimensionalArray(IReadOnlyList<object> values, object[,] array)
        {
            var width = array.GetLength(0);
            var height = array.GetLength(1);

            Check.DebugAssert(height == values.Count, $"height of {height} != values.Count of {values.Count}");

            var result = new object[width + 1, height];
            for (var i = 0; i < width; i++)
            {
                Array.Copy(array, i * height, result, i * height, height);
            }

            for (var i = 0; i < values.Count; i++)
            {
                result[width, i] = values[i];
            }

            return result;
        }

        private sealed class EntryMapping
        {
            public HashSet<IUpdateEntry> SourceEntries { get; } = new HashSet<IUpdateEntry>();
            public HashSet<IUpdateEntry> TargetEntries { get; } = new HashSet<IUpdateEntry>();
            public bool RecreateRow { get; set; }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected class DiffContext
        {
            private readonly IDictionary<object, object> _targetToSource = new Dictionary<object, object>();
            private readonly IDictionary<object, object> _sourceToTarget = new Dictionary<object, object>();

            private readonly IDictionary<ITable, CreateTableOperation> _createTableOperations
                = new Dictionary<ITable, CreateTableOperation>();

            private readonly IDictionary<ITable, DropTableOperation> _dropTableOperations
                = new Dictionary<ITable, DropTableOperation>();

            private readonly IDictionary<DropTableOperation, ITable> _removedTables
                = new Dictionary<DropTableOperation, ITable>();

            /// <summary>
            ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
            ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
            ///     any release. You should only use it directly in your code with extreme caution and knowing that
            ///     doing so can result in application failures when updating to a new Entity Framework Core release.
            /// </summary>
            public virtual void AddMapping<T>([NotNull] T source, [NotNull] T target)
            {
                _targetToSource.Add(target, source);
                _sourceToTarget.Add(source, target);
            }

            /// <summary>
            ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
            ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
            ///     any release. You should only use it directly in your code with extreme caution and knowing that
            ///     doing so can result in application failures when updating to a new Entity Framework Core release.
            /// </summary>
            public virtual void AddCreate([NotNull] ITable target, [NotNull] CreateTableOperation operation)
                => _createTableOperations.Add(target, operation);

            /// <summary>
            ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
            ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
            ///     any release. You should only use it directly in your code with extreme caution and knowing that
            ///     doing so can result in application failures when updating to a new Entity Framework Core release.
            /// </summary>
            public virtual void AddDrop([NotNull] ITable source, [NotNull] DropTableOperation operation)
            {
                _dropTableOperations.Add(source, operation);
                _removedTables.Add(operation, source);
            }

            /// <summary>
            ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
            ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
            ///     any release. You should only use it directly in your code with extreme caution and knowing that
            ///     doing so can result in application failures when updating to a new Entity Framework Core release.
            /// </summary>
            public virtual ITable GetTable(IEntityType entityType)
                => entityType.GetTableMappings().First().Table;

            /// <summary>
            ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
            ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
            ///     any release. You should only use it directly in your code with extreme caution and knowing that
            ///     doing so can result in application failures when updating to a new Entity Framework Core release.
            /// </summary>
            public virtual T FindSource<T>([CanBeNull] T target)
                where T : class
                => target == null
                    ? null
                    : _targetToSource.TryGetValue(target, out var source)
                        ? (T)source
                        : null;

            /// <summary>
            ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
            ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
            ///     any release. You should only use it directly in your code with extreme caution and knowing that
            ///     doing so can result in application failures when updating to a new Entity Framework Core release.
            /// </summary>
            public virtual T FindTarget<T>([CanBeNull] T source)
                where T : class
                => source == null
                    ? null
                    : _sourceToTarget.TryGetValue(source, out var target)
                        ? (T)target
                        : null;

            /// <summary>
            ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
            ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
            ///     any release. You should only use it directly in your code with extreme caution and knowing that
            ///     doing so can result in application failures when updating to a new Entity Framework Core release.
            /// </summary>
            public virtual CreateTableOperation FindCreate([NotNull] ITable target)
                => _createTableOperations.TryGetValue(target, out var operation)
                    ? operation
                    : null;

            /// <summary>
            ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
            ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
            ///     any release. You should only use it directly in your code with extreme caution and knowing that
            ///     doing so can result in application failures when updating to a new Entity Framework Core release.
            /// </summary>
            public virtual DropTableOperation FindDrop([NotNull] ITable source)
                => _dropTableOperations.TryGetValue(source, out var operation)
                    ? operation
                    : null;

            /// <summary>
            ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
            ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
            ///     any release. You should only use it directly in your code with extreme caution and knowing that
            ///     doing so can result in application failures when updating to a new Entity Framework Core release.
            /// </summary>
            public virtual ITable FindTable([NotNull] DropTableOperation operation)
                => _removedTables.TryGetValue(operation, out var source)
                    ? source
                    : null;
        }
    }
}
