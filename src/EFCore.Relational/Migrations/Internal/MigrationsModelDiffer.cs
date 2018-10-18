// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Update;
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

        private IStateManager _sourceStateManager;
        private IStateManager _targetStateManager;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MigrationsModelDiffer(
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] IMigrationsAnnotationProvider migrationsAnnotations,
            [NotNull] IChangeDetector changeDetector,
            [NotNull] StateManagerDependencies stateManagerDependencies,
            [NotNull] CommandBatchPreparerDependencies commandBatchPreparerDependencies)
        {
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            Check.NotNull(migrationsAnnotations, nameof(migrationsAnnotations));
            Check.NotNull(stateManagerDependencies, nameof(stateManagerDependencies));
            Check.NotNull(commandBatchPreparerDependencies, nameof(commandBatchPreparerDependencies));

            TypeMappingSource = typeMappingSource;
            MigrationsAnnotations = migrationsAnnotations;
            ChangeDetector = changeDetector;
            StateManagerDependencies = stateManagerDependencies;
            CommandBatchPreparerDependencies = commandBatchPreparerDependencies;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IRelationalTypeMappingSource TypeMappingSource { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IMigrationsAnnotationProvider MigrationsAnnotations { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual StateManagerDependencies StateManagerDependencies { get; }

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
            TrackData(source, target);

            var schemaOperations = source != null && target != null
                ? DiffAnnotations(source, target)
                    .Concat(Diff(GetSchemas(source), GetSchemas(target), diffContext))
                    .Concat(Diff(diffContext.GetSourceTables(), diffContext.GetTargetTables(), diffContext))
                    .Concat(Diff(source.Relational().Sequences, target.Relational().Sequences, diffContext))
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

            return schemaOperations.Concat(GetDataOperations());
        }

        private IEnumerable<MigrationOperation> DiffAnnotations(
            IModel source,
            IModel target)
        {
            var sourceMigrationsAnnotations = source == null ? null : MigrationsAnnotations.For(source).ToList();
            var targetMigrationsAnnotations = target == null ? null : MigrationsAnnotations.For(target).ToList();

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
                .Concat(GetSchemas(target).SelectMany(t => Add(t, diffContext)))
                .Concat(diffContext.GetTargetTables().SelectMany(t => Add(t, diffContext)))
                .Concat(target.Relational().Sequences.SelectMany(t => Add(t, diffContext)))
                .Concat(diffContext.GetTargetTables().SelectMany(t => t.GetForeignKeys()).SelectMany(k => Add(k, diffContext)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] IModel source, [NotNull] DiffContext diffContext)
            => DiffAnnotations(source, null)
                .Concat(diffContext.GetSourceTables().SelectMany(t => Remove(t, diffContext)))
                .Concat(source.Relational().Sequences.SelectMany(t => Remove(t, diffContext)));

        #endregion

        #region Schema

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff([NotNull] IEnumerable<string> source, [NotNull] IEnumerable<string> target, [NotNull] DiffContext diffContext)
            => DiffCollection(
                source,
                target,
                diffContext,
                Diff,
                Add,
                Remove,
                (s, t, c) => s == t);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff([NotNull] string source, [NotNull] string target, [NotNull] DiffContext diffContext)
            => Enumerable.Empty<MigrationOperation>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Add([NotNull] string target, [NotNull] DiffContext diffContext)
        {
            yield return new EnsureSchemaOperation
            {
                Name = target
            };
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] string source, [NotNull] DiffContext diffContext)
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
                (s, t, c) => string.Equals(s.GetRootType().Name, t.GetRootType().Name, StringComparison.OrdinalIgnoreCase),
                (s, t, c) => s.EntityTypes.Any(
                    se => t.EntityTypes.Any(
                        te =>
                            string.Equals(se.Name, te.Name, StringComparison.OrdinalIgnoreCase))));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff(
            [NotNull] TableMapping source,
            [NotNull] TableMapping target,
            [NotNull] DiffContext diffContext)
        {
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

            var operations = DiffAnnotations(source, target)
                .Concat(Diff(source.GetProperties(), target.GetProperties(), diffContext))
                .Concat(Diff(source.GetKeys(), target.GetKeys(), diffContext))
                .Concat(Diff(source.GetIndexes(), target.GetIndexes(), diffContext));
            foreach (var operation in operations)
            {
                yield return operation;
            }

            DiffData(source, target, diffContext);
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
                GetSortedProperties(target).SelectMany(p => Add(p, diffContext, inline: true)).Cast<AddColumnOperation>());
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

        private static IEnumerable<IProperty> GetSortedProperties(TableMapping target)
            => GetSortedProperties(target.GetRootType())
                .Distinct((x, y) => x.Relational().ColumnName == y.Relational().ColumnName);

        private static IEnumerable<IProperty> GetSortedProperties(IEntityType entityType)
        {
            var shadowProperties = new List<IProperty>();
            var shadowPrimaryKeyProperties = new List<IProperty>();
            var primaryKeyPropertyGroups = new Dictionary<PropertyInfo, IProperty>();
            var groups = new Dictionary<PropertyInfo, List<IProperty>>();
            var unorderedGroups = new Dictionary<PropertyInfo, SortedDictionary<int, IProperty>>();
            var types = new Dictionary<Type, SortedDictionary<int, PropertyInfo>>();

            foreach (var property in entityType.GetDeclaredProperties())
            {
                var clrProperty = property.PropertyInfo;
                if (clrProperty == null)
                {
                    if (property.IsPrimaryKey())
                    {
                        shadowPrimaryKeyProperties.Add(property);

                        continue;
                    }

                    var foreignKey = property.GetContainingForeignKeys()
                        .FirstOrDefault(fk => fk.DependentToPrincipal?.PropertyInfo != null);
                    if (foreignKey == null)
                    {
                        shadowProperties.Add(property);

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
                        clrProperty, new List<IProperty>
                        {
                            property
                        });
                }

                var clrType = clrProperty.DeclaringType;
                var index = clrType.GetTypeInfo().DeclaredProperties
                    .IndexOf(clrProperty, PropertyInfoEqualityComparer.Instance);

                types.GetOrAddNew(clrType)[index] = clrProperty;
            }

            foreach (var group in unorderedGroups)
            {
                groups.Add(group.Key, group.Value.Values.ToList());
            }

            foreach (var definingForeignKey in entityType.GetDeclaredReferencingForeignKeys()
                .Where(
                    fk => fk.DeclaringEntityType.RootType() != entityType.RootType()
                          && fk.DeclaringEntityType.Relational().TableName == entityType.Relational().TableName
                          && fk == fk.DeclaringEntityType
                              .FindForeignKey(
                                  fk.DeclaringEntityType.FindPrimaryKey().Properties,
                                  entityType.FindPrimaryKey(),
                                  entityType)))
            {
                var clrProperty = definingForeignKey.PrincipalToDependent?.PropertyInfo;
                var properties = GetSortedProperties(definingForeignKey.DeclaringEntityType).ToList();
                if (clrProperty == null)
                {
                    shadowProperties.AddRange(properties);

                    continue;
                }

                groups.Add(clrProperty, properties);

                var clrType = clrProperty.DeclaringType;
                var index = clrType.GetTypeInfo().DeclaredProperties
                    .IndexOf(clrProperty, PropertyInfoEqualityComparer.Instance);

                types.GetOrAddNew(clrType)[index] = clrProperty;
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
                .Concat(shadowPrimaryKeyProperties)
                .Concat(sortedPropertyInfos.Where(pi => !primaryKeyPropertyGroups.ContainsKey(pi)).SelectMany(p => groups[p]))
                .Concat(shadowProperties)
                .Concat(entityType.GetDirectlyDerivedTypes().SelectMany(GetSortedProperties));
        }

        private class PropertyInfoEqualityComparer : IEqualityComparer<PropertyInfo>
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
                diffContext,
                Diff,
                (t, c) => Add(t, c),
                Remove,
                (s, t, c) => string.Equals(
                    s.Relational().ColumnName,
                    t.Relational().ColumnName,
                    StringComparison.OrdinalIgnoreCase),
                (s, t, c) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase)
                             && EntityTypePathEquals(s.DeclaringEntityType, t.DeclaringEntityType, c),
                (s, t, c) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase),
                (s, t, c) => EntityTypePathEquals(s.DeclaringEntityType, t.DeclaringEntityType, c)
                             && PropertyStructureEquals(s, t),
                (s, t, c) => PropertyStructureEquals(s, t));

        private bool PropertyStructureEquals(IProperty source, IProperty target)
        {
            var sourceAnnotations = source.Relational();
            var targetAnnotations = target.Relational();
            return source.ClrType == target.ClrType
                   && source.IsConcurrencyToken == target.IsConcurrencyToken
                   && source.ValueGenerated == target.ValueGenerated
                   && source.GetMaxLength() == target.GetMaxLength()
                   && source.IsColumnNullable() == target.IsColumnNullable()
                   && source.IsUnicode() == target.IsUnicode()
                   && sourceAnnotations.IsFixedLength == targetAnnotations.IsFixedLength
                   && source.GetConfiguredColumnType() == target.GetConfiguredColumnType()
                   && sourceAnnotations.ComputedColumnSql == targetAnnotations.ComputedColumnSql
                   && Equals(GetDefaultValue(source), GetDefaultValue(target))
                   && sourceAnnotations.DefaultValueSql == targetAnnotations.DefaultValueSql;
        }

        private static bool EntityTypePathEquals(IEntityType source, IEntityType target, DiffContext diffContext)
        {
            var sourceTable = diffContext.FindSourceTable(source);
            var targetTable = diffContext.FindTargetTable(target);

            if (sourceTable.EntityTypes.Count == 1
                && targetTable.EntityTypes.Count == 1)
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
                || !sourceTable.EntityTypes.Contains(nextSource)
                || nextTarget == null
                || !targetTable.EntityTypes.Contains(nextTarget)
                ? true
                : EntityTypePathEquals(nextSource, nextTarget, diffContext);
        }

        private static string GetDefiningNavigationName(IEntityType entityType)
        {
            if (entityType.DefiningNavigationName != null)
            {
                return entityType.DefiningNavigationName;
            }

            var primaryKey = entityType.FindDeclaredPrimaryKey();
            if (primaryKey != null)
            {
                var definingForeignKey = entityType
                    .FindForeignKeys(primaryKey.Properties)
                    .FirstOrDefault(fk => fk.PrincipalEntityType.Relational().TableName == entityType.Relational().TableName);
                if (definingForeignKey?.DependentToPrincipal != null)
                {
                    return definingForeignKey.DependentToPrincipal.Name;
                }
            }

            return entityType.Name;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff([NotNull] IProperty source, [NotNull] IProperty target, [NotNull] DiffContext diffContext)
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

            var sourceTypeMapping = TypeMappingSource.GetMapping(source);
            var targetTypeMapping = TypeMappingSource.GetMapping(target);

            var sourceColumnType = sourceAnnotations.ColumnType
                                   ?? sourceTypeMapping.StoreType;
            var targetColumnType = targetAnnotations.ColumnType
                                   ?? targetTypeMapping.StoreType;

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
                || !Equals(GetDefaultValue(source), GetDefaultValue(target))
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
                    alterColumnOperation, target, targetTypeMapping,
                    isTargetColumnNullable, targetAnnotations, targetMigrationsAnnotations, inline: true);

                Initialize(
                    alterColumnOperation.OldColumn, source, sourceTypeMapping,
                    isSourceColumnNullable, sourceAnnotations, sourceMigrationsAnnotations, inline: true);

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

            Initialize(
                operation, target, TypeMappingSource.GetMapping(target), target.IsColumnNullable(),
                targetAnnotations, MigrationsAnnotations.For(target), inline);

            yield return operation;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] IProperty source, [NotNull] DiffContext diffContext)
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
            CoreTypeMapping typeMapping,
            bool isNullable,
            IRelationalPropertyAnnotations annotations,
            IEnumerable<IAnnotation> migrationsAnnotations,
            bool inline = false)
        {
            columnOperation.ClrType
                = (typeMapping.Converter?.ProviderClrType
                   ?? typeMapping.ClrType).UnwrapNullableType();

            columnOperation.ColumnType = property.GetConfiguredColumnType();
            columnOperation.MaxLength = property.GetMaxLength();
            columnOperation.IsUnicode = property.IsUnicode();
            columnOperation.IsFixedLength = annotations.IsFixedLength;
            columnOperation.IsRowVersion = property.ClrType == typeof(byte[])
                                           && property.IsConcurrencyToken
                                           && property.ValueGenerated == ValueGenerated.OnAddOrUpdate;
            columnOperation.IsNullable = isNullable;

            var defaultValue = GetDefaultValue(property);
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
                diffContext,
                Diff,
                Add,
                Remove,
                (s, t, c) => s.Relational().Name == t.Relational().Name
                             && s.Properties.Select(p => p.Relational().ColumnName).SequenceEqual(
                                 t.Properties.Select(p => c.FindSource(p)?.Relational().ColumnName))
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
                diffContext,
                Diff,
                Add,
                Remove,
                (s, t, c) => s.Relational().Name == t.Relational().Name
                             && s.Properties.Select(p => p.Relational().ColumnName).SequenceEqual(
                                 t.Properties.Select(p => c.FindSource(p)?.Relational().ColumnName))
                             && c.FindSourceTable(s.PrincipalEntityType)
                             == c.FindSource(c.FindTargetTable(t.PrincipalEntityType))
                             && s.PrincipalKey.Properties.Select(p => p.Relational().ColumnName).SequenceEqual(
                                 t.PrincipalKey.Properties.Select(p => c.FindSource(p)?.Relational().ColumnName))
                             && ToReferentialAction(s.DeleteBehavior) == ToReferentialAction(t.DeleteBehavior)
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
                OnDelete = ToReferentialAction(target.DeleteBehavior)
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
                diffContext,
                Diff,
                Add,
                Remove,
                (s, t, c) => string.Equals(
                                 s.Relational().Name,
                                 t.Relational().Name,
                                 StringComparison.OrdinalIgnoreCase)
                             && IndexStructureEquals(s, t, c),
                (s, t, c) => IndexStructureEquals(s, t, c));

        private bool IndexStructureEquals(IIndex source, IIndex target, DiffContext diffContext)
            => source.IsUnique == target.IsUnique
               && source.Relational().Filter == target.Relational().Filter
               && !HasDifferences(MigrationsAnnotations.For(source), MigrationsAnnotations.For(target))
               && source.Properties.Select(p => p.Relational().ColumnName).SequenceEqual(
                   target.Properties.Select(p => diffContext.FindSource(p)?.Relational().ColumnName));

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
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] IIndex source, [NotNull] DiffContext diffContext)
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> Diff([NotNull] ISequence source, [NotNull] ISequence target, [NotNull] DiffContext diffContext)
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
        protected virtual IEnumerable<MigrationOperation> Add([NotNull] ISequence target, [NotNull] DiffContext diffContext)
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
        protected virtual IEnumerable<MigrationOperation> Remove([NotNull] ISequence source, [NotNull] DiffContext diffContext)
        {
            var operation = new DropSequenceOperation
            {
                Schema = source.Schema,
                Name = source.Name
            };
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void TrackData(
            [CanBeNull] IModel source,
            [CanBeNull] IModel target)
        {
            if (target == null)
            {
                _targetStateManager = null;
                return;
            }

            _targetStateManager = new StateManager(StateManagerDependencies.With(target));

            foreach (var targetEntityType in target.GetEntityTypes())
            {
                foreach (var targetSeed in targetEntityType.GetData())
                {
                    _targetStateManager.CreateEntry(targetSeed, targetEntityType).SetEntityState(EntityState.Added);
                }
            }

            if (source == null)
            {
                _sourceStateManager = null;
                return;
            }

            _sourceStateManager = new StateManager(StateManagerDependencies.With(source));

            foreach (var sourceEntityType in source.GetEntityTypes())
            {
                foreach (var sourceSeed in sourceEntityType.GetData())
                {
                    _sourceStateManager.CreateEntry(sourceSeed, sourceEntityType).SetEntityState(EntityState.Added);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void DiffData(
            [NotNull] TableMapping source,
            [NotNull] TableMapping target,
            [NotNull] DiffContext diffContext)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(target, nameof(target));
            Check.NotNull(diffContext, nameof(diffContext));

            var targetTableEntryMappingMap = SharedTableEntryMap<List<InternalEntityEntry>>.CreateSharedTableEntryMapFactory(
                    target.EntityTypes,
                    _targetStateManager,
                    target.Name,
                    target.Schema)
                ((t, s, c) => new List<InternalEntityEntry>());

            foreach (var targetEntityType in target.EntityTypes)
            {
                foreach (var targetSeed in targetEntityType.GetData())
                {
                    var targetEntry = GetEntry(targetSeed, targetEntityType, _targetStateManager);
                    var targetEntries = targetTableEntryMappingMap.GetOrAddValue(targetEntry);
                    targetEntries.Add(targetEntry);
                }
            }

            var targetKeys = target.EntityTypes.SelectMany(Metadata.Internal.EntityTypeExtensions.GetDeclaredKeys)
                .Where(k => k.IsPrimaryKey()).ToList();
            var keyMapping = new Dictionary<IEntityType,
                Dictionary<IKey, List<(IProperty Property, ValueConverter SourceConverter, ValueConverter TargetConverter)>>>();
            foreach (var sourceEntityType in source.EntityTypes)
            {
                foreach (var targetKey in targetKeys)
                {
                    var keyPropertiesMap = new List<(IProperty, ValueConverter, ValueConverter)>();
                    foreach (var keyProperty in targetKey.Properties)
                    {
                        var sourceProperty = diffContext.FindSource(keyProperty);
                        if (sourceProperty == null)
                        {
                            break;
                        }

                        foreach (var matchingSourceProperty in sourceEntityType.GetProperties())
                        {
                            if (matchingSourceProperty.Relational().ColumnName == sourceProperty.Relational().ColumnName)
                            {
                                var sourceConverter = GetValueConverter(sourceProperty);
                                var targetConverter = GetValueConverter(keyProperty);
                                if (matchingSourceProperty.ClrType != keyProperty.ClrType
                                    && (sourceConverter == null || sourceConverter.ProviderClrType != keyProperty.ClrType)
                                    && (targetConverter == null || targetConverter.ProviderClrType != matchingSourceProperty.ClrType))
                                {
                                    continue;
                                }

                                keyPropertiesMap.Add((matchingSourceProperty, sourceConverter, targetConverter));
                                break;
                            }
                        }
                    }

                    if (keyPropertiesMap.Count == targetKey.Properties.Count)
                    {
                        keyMapping.GetOrAddNew(sourceEntityType)[targetKey] = keyPropertiesMap;
                    }
                }
            }

            var sourceTableEntryMappingMap = SharedTableEntryMap<EntryMapping>.CreateSharedTableEntryMapFactory(
                    source.EntityTypes,
                    _sourceStateManager,
                    source.Name,
                    source.Schema)
                ((t, s, c) => new EntryMapping());

            foreach (var sourceEntityType in source.EntityTypes)
            {
                foreach (var sourceSeed in sourceEntityType.GetData())
                {
                    var sourceEntry = GetEntry(sourceSeed, sourceEntityType, _sourceStateManager);
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
                            if (targetKey.Properties[i].ClrType != sourceProperty.ClrType)
                            {
                                if (sourceConverter != null)
                                {
                                    targetKeyValues[i] = sourceConverter.ConvertToProvider(sourceValue);
                                }
                                else
                                {
                                    targetKeyValues[i] = targetConverter.ConvertFromProvider(sourceValue);
                                }
                            }
                            else
                            {
                                targetKeyValues[i] = sourceValue;
                            }
                        }

                        var entry = _targetStateManager.TryGetEntry(targetKey, targetKeyValues);
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
                                if (targetProperty.AfterSaveBehavior == PropertySaveBehavior.Save)
                                {
                                    targetEntry.SetOriginalValue(targetProperty, targetProperty.ClrType.GetDefaultValue());
                                }
                            }

                            targetEntry.SetEntityState(EntityState.Unchanged);
                        }

                        if (entryMapping.RecreateRow)
                        {
                            continue;
                        }

                        foreach (var targetProperty in entry.EntityType.GetProperties())
                        {
                            var sourceProperty = diffContext.FindSource(targetProperty);
                            if (sourceProperty == null
                                || !sourceEntityType.GetProperties().Contains(sourceProperty))
                            {
                                continue;
                            }

                            var sourceValue = sourceEntry.GetCurrentValue(sourceProperty);
                            var targetValue = entry.GetCurrentValue(targetProperty);
                            var comparer = targetProperty.GetValueComparer() ??
                                           sourceProperty.GetValueComparer() ??
                                           targetProperty.FindMapping()?.Comparer ??
                                           sourceProperty.FindMapping()?.Comparer;

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

                                var storeValuesChanged = convertedSourceValue?.GetType().UnwrapNullableType() != convertedTargetValue?.GetType().UnwrapNullableType();

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

                            if (targetProperty.AfterSaveBehavior != PropertySaveBehavior.Save)
                            {
                                entryMapping.RecreateRow = true;
                                break;
                            }

                            entry.SetPropertyModified(targetProperty);
                        }
                    }
                }
            }

            foreach (var entryMapping in sourceTableEntryMappingMap.Values)
            {
                if (entryMapping.RecreateRow
                    || entryMapping.TargetEntries.Count == 0)
                {
                    foreach (var targetEntry in entryMapping.TargetEntries)
                    {
                        targetEntry.SetEntityState(EntityState.Added);
                    }

                    foreach (var sourceEntry in entryMapping.SourceEntries)
                    {
                        sourceEntry.SetEntityState(EntityState.Deleted);
                    }
                }
                else
                {
                    foreach (var sourceEntry in entryMapping.SourceEntries)
                    {
                        sourceEntry.SetEntityState(EntityState.Detached);
                    }
                }
            }
        }

        private static InternalEntityEntry GetEntry(
            IDictionary<string, object> sourceSeed, IEntityType sourceEntityType, IStateManager stateManager)
        {
            var key = sourceEntityType.FindPrimaryKey();
            var keyValues = new object[key.Properties.Count];
            for (var i = 0; i < keyValues.Length; i++)
            {
                keyValues[i] = sourceSeed[key.Properties[i].Name];
            }

            return stateManager.TryGetEntry(key, keyValues);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<MigrationOperation> GetDataOperations()
        {
            if (_sourceStateManager != null)
            {
                foreach (var sourceEntry in _sourceStateManager.Entries.ToList())
                {
                    if (sourceEntry.EntityState == EntityState.Added)
                    {
                        sourceEntry.SetEntityState(EntityState.Detached);
                    }
                }
            }

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

                var commandBatches = new CommandBatchPreparer(CommandBatchPreparerDependencies.With(() => stateManager))
                    .BatchCommands(entries);

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
                                    && batchInsertOperation.Columns.SequenceEqual(c.ColumnModifications.Select(col => col.ColumnName)))
                                {
                                    batchInsertOperation.Values =
                                        AddToMultidimensionalArray(c.ColumnModifications.Select(GetValue).ToList(), batchInsertOperation.Values);
                                    continue;
                                }

                                yield return batchInsertOperation;
                            }

                            batchInsertOperation = new InsertDataOperation
                            {
                                Schema = c.Schema,
                                Table = c.TableName,
                                Columns = c.ColumnModifications.Where(col => col.IsKey || col.IsWrite).Select(col => col.ColumnName).ToArray(),
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
            return converter != null
                ? converter.ConvertToProvider(columnModification.Value)
                : columnModification.Value;
        }

        #endregion

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
                var index = unmatched.FindIndex(a => a.Name == annotation.Name && StructuralComparisons.StructuralEqualityComparer.Equals(a.Value, annotation.Value));
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

        private object GetDefaultValue(IProperty property)
        {
            var value = property.Relational().DefaultValue;
            var converter = GetValueConverter(property);
            return converter != null
                ? converter.ConvertToProvider(value)
                : value;
        }

        private ValueConverter GetValueConverter(IProperty property)
            => TypeMappingSource.GetMapping(property).Converter;

        private static ReferentialAction ToReferentialAction(DeleteBehavior deleteBehavior)
            => deleteBehavior == DeleteBehavior.Cascade
                ? ReferentialAction.Cascade
                : deleteBehavior == DeleteBehavior.SetNull
                    ? ReferentialAction.SetNull
                    : ReferentialAction.Restrict;

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

            Debug.Assert(height == values.Count);

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

        private class EntryMapping
        {
            public HashSet<InternalEntityEntry> SourceEntries { get; } = new HashSet<InternalEntityEntry>();
            public HashSet<InternalEntityEntry> TargetEntries { get; } = new HashSet<InternalEntityEntry>();
            public bool RecreateRow { get; set; }
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

            private readonly IDictionary<object, object> _targetToSource = new Dictionary<object, object>();

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
            public virtual void AddMapping<T>([NotNull] T source, [NotNull] T target)
                => _targetToSource.Add(target, source);

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
            public virtual T FindSource<T>([NotNull] T target)
                => _targetToSource.TryGetValue(target, out var source)
                    ? (T)source
                    : default;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual IProperty FindSource([NotNull] IProperty target)
            {
                var source = FindSource<IProperty>(target);
                if (source != null)
                {
                    return source;
                }

                var synonymousTargets = FindTargetTable(target.DeclaringEntityType).GetProperties()
                    .Where(p => p != target && p.Relational().ColumnName == target.Relational().ColumnName);
                foreach (var synonymousTarget in synonymousTargets)
                {
                    source = FindSource<IProperty>(synonymousTarget);
                    if (source != null)
                    {
                        _targetToSource.Add(target, source);

                        return source;
                    }
                }

                return null;
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
