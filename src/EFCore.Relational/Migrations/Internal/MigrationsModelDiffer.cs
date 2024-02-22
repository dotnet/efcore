// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class MigrationsModelDiffer : IMigrationsModelDiffer
{
    private static readonly Type[] DropOperationTypes =
    [
        typeof(DropIndexOperation),
        typeof(DropPrimaryKeyOperation),
        typeof(DropUniqueConstraintOperation),
        typeof(DropCheckConstraintOperation)
    ];

    private static readonly Type[] AlterOperationTypes =
    [
        typeof(AddPrimaryKeyOperation), typeof(AddUniqueConstraintOperation), typeof(AlterSequenceOperation)
    ];

    private static readonly Type[] RenameOperationTypes =
    [
        typeof(RenameColumnOperation), typeof(RenameIndexOperation), typeof(RenameSequenceOperation)
    ];

    private static readonly Type[] ColumnOperationTypes = [typeof(AddColumnOperation), typeof(AlterColumnOperation)];

    private static readonly Type[] ConstraintOperationTypes =
    [
        typeof(AddForeignKeyOperation), typeof(CreateIndexOperation), typeof(AddCheckConstraintOperation)
    ];

    private Dictionary<ITable, IRowIdentityMap>? _sourceIdentityMaps;
    private Dictionary<ITable, IRowIdentityMap>? _targetIdentityMaps;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public MigrationsModelDiffer(
        IRelationalTypeMappingSource typeMappingSource,
        IMigrationsAnnotationProvider migrationsAnnotationProvider,
        IRelationalAnnotationProvider relationalAnnotationProvider,
        IRowIdentityMapFactory rowIdentityMapFactory,
        CommandBatchPreparerDependencies commandBatchPreparerDependencies)
    {
        TypeMappingSource = typeMappingSource;
        MigrationsAnnotationProvider = migrationsAnnotationProvider;
        RelationalAnnotationProvider = relationalAnnotationProvider;
        RowIdentityMapFactory = rowIdentityMapFactory;
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
    protected virtual IMigrationsAnnotationProvider MigrationsAnnotationProvider { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IRelationalAnnotationProvider RelationalAnnotationProvider { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IRowIdentityMapFactory RowIdentityMapFactory { get; }

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
    public virtual bool HasDifferences(IRelationalModel? source, IRelationalModel? target)
        => Diff(source, target, new DiffContext()).Any();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<MigrationOperation> GetDifferences(IRelationalModel? source, IRelationalModel? target)
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
        IEnumerable<MigrationOperation> operations,
        DiffContext diffContext)
    {
        var dropForeignKeyOperations = new List<MigrationOperation>();
        var dropOperations = new List<MigrationOperation>();
        var dropColumnOperations = new List<MigrationOperation>();
        var dropComputedColumnOperations = new List<MigrationOperation>();
        var dropTableOperations = new List<DropTableOperation>();
        var dropSequenceOperations = new List<MigrationOperation>();
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
            else if (DropOperationTypes.Contains(type))
            {
                dropOperations.Add(operation);
            }
            else if (type == typeof(DropColumnOperation))
            {
                if (string.IsNullOrWhiteSpace(diffContext.FindColumn((DropColumnOperation)operation)!.ComputedColumnSql))
                {
                    dropColumnOperations.Add(operation);
                }
                else
                {
                    dropComputedColumnOperations.Add(operation);
                }
            }
            else if (type == typeof(DropTableOperation))
            {
                dropTableOperations.Add((DropTableOperation)operation);
            }
            else if (type == typeof(DropSequenceOperation))
            {
                dropSequenceOperations.Add((DropSequenceOperation)operation);
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
            else if (ColumnOperationTypes.Contains(type))
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
            else if (AlterOperationTypes.Contains(type))
            {
                alterOperations.Add(operation);
            }
            else if (type == typeof(RestartSequenceOperation))
            {
                restartSequenceOperations.Add(operation);
            }
            else if (ConstraintOperationTypes.Contains(type))
            {
                constraintOperations.Add(operation);
            }
            else if (RenameOperationTypes.Contains(type))
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
                Check.DebugFail("Unexpected operation type: " + operation.GetType());
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

        createTableOperations = (List<CreateTableOperation>)createTableGraph.TopologicalSort(
            (_, createTableOperation, cyclicAddForeignKeyOperations) =>
            {
                foreach (var cyclicAddForeignKeyOperation in cyclicAddForeignKeyOperations)
                {
                    var removed = createTableOperation.ForeignKeys.Remove(cyclicAddForeignKeyOperation);
                    if (removed)
                    {
                        constraintOperations.Add(cyclicAddForeignKeyOperation);
                    }
                    else
                    {
                        Check.DebugFail("Operation removed twice: " + cyclicAddForeignKeyOperation);
                    }
                }

                return true;
            });

        var dropTableGraph = new Multigraph<DropTableOperation, IForeignKeyConstraint>();
        dropTableGraph.AddVertices(dropTableOperations);
        foreach (var dropTableOperation in dropTableOperations)
        {
            var table = diffContext.FindTable(dropTableOperation)!;
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
        dropTableOperations = (List<DropTableOperation>)dropTableGraph.TopologicalSort(
            (_, _, foreignKeys) =>
            {
                dropForeignKeyOperations.AddRange(foreignKeys.SelectMany(c => Remove(c, newDiffContext)));

                return true;
            });

        return dropForeignKeyOperations
            .Concat(dropTableOperations)
            .Concat(dropOperations)
            .Concat(sourceDataOperations)
            .Concat(dropComputedColumnOperations)
            .Concat(dropColumnOperations)
            .Concat(dropSequenceOperations)
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Diff(
        IRelationalModel? source,
        IRelationalModel? target,
        DiffContext diffContext)
    {
        var operations = Enumerable.Empty<MigrationOperation>();
        if (source != null && target != null)
        {
            var sourceMigrationsAnnotations = source.GetAnnotations();
            var targetMigrationsAnnotations = target.GetAnnotations();

            if (source.Collation != target.Collation
                || HasDifferences(sourceMigrationsAnnotations, targetMigrationsAnnotations))
            {
                var alterDatabaseOperation = new AlterDatabaseOperation
                {
                    Collation = target.Collation, OldDatabase = { Collation = source.Collation }
                };

                alterDatabaseOperation.AddAnnotations(targetMigrationsAnnotations);
                alterDatabaseOperation.OldDatabase.AddAnnotations(sourceMigrationsAnnotations);

                operations = new[] { alterDatabaseOperation };
            }

            operations = operations
                .Concat(Diff(GetSchemas(source), GetSchemas(target), diffContext))
                .Concat(Diff(source.Tables, target.Tables, diffContext))
                .Concat(Diff(source.Sequences, target.Sequences, diffContext))
                .Concat(
                    Diff(
                        source.Tables.SelectMany(s => s.ForeignKeyConstraints),
                        target.Tables.SelectMany(t => t.ForeignKeyConstraints),
                        diffContext));
        }
        else
        {
            operations = target != null
                ? Add(target, diffContext)
                : source != null
                    ? Remove(source, diffContext)
                    : Enumerable.Empty<MigrationOperation>();
        }

        return operations.Concat(GetDataOperations(source, target, diffContext));
    }

    private IEnumerable<MigrationOperation> DiffAnnotations(
        IRelationalModel? source,
        IRelationalModel? target)
    {
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
            var sourceMigrationsAnnotationsForRemoved = MigrationsAnnotationProvider.ForRemove(source).ToList();
            if (sourceMigrationsAnnotationsForRemoved.Count > 0)
            {
                var alterDatabaseOperation = new AlterDatabaseOperation();
                alterDatabaseOperation.OldDatabase.AddAnnotations(sourceMigrationsAnnotationsForRemoved);
                yield return alterDatabaseOperation;
            }

            yield break;
        }

        var sourceMigrationsAnnotations = source.GetAnnotations().ToList();
        if (HasDifferences(sourceMigrationsAnnotations, targetMigrationsAnnotations!))
        {
            var alterDatabaseOperation = new AlterDatabaseOperation();
            alterDatabaseOperation.AddAnnotations(targetMigrationsAnnotations!);
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
    protected virtual IEnumerable<MigrationOperation> Add(IRelationalModel target, DiffContext diffContext)
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
    protected virtual IEnumerable<MigrationOperation> Remove(IRelationalModel source, DiffContext diffContext)
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
        IEnumerable<string> source,
        IEnumerable<string> target,
        DiffContext diffContext)
        => DiffCollection(
            source,
            target,
            diffContext,
            Diff,
            Add,
            Remove,
            (s, t, _) => s == t);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Diff(
        string source,
        string target,
        DiffContext diffContext)
        => Enumerable.Empty<MigrationOperation>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Add(string target, DiffContext diffContext)
    {
        yield return new EnsureSchemaOperation { Name = target };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Remove(string source, DiffContext diffContext)
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
        IEnumerable<ITable> source,
        IEnumerable<ITable> target,
        DiffContext diffContext)
        => DiffCollection(
            source,
            target,
            diffContext,
            Diff,
            Add,
            Remove,
            (s, t, _) => string.Equals(
                    s.Schema,
                    t.Schema,
                    StringComparison.OrdinalIgnoreCase)
                && string.Equals(
                    s.Name,
                    t.Name,
                    StringComparison.OrdinalIgnoreCase),
            (s, t, _) => string.Equals(
                s.Name,
                t.Name,
                StringComparison.OrdinalIgnoreCase),
            (s, t, _) => string.Equals(GetMainType(s).Name, GetMainType(t).Name, StringComparison.OrdinalIgnoreCase),
            (s, t, _) => s.EntityTypeMappings.Any(
                se => t.EntityTypeMappings.Any(
                    te => string.Equals(se.TypeBase.Name, te.TypeBase.Name, StringComparison.OrdinalIgnoreCase))));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Diff(
        ITable source,
        ITable target,
        DiffContext diffContext)
    {
        if (source.IsExcludedFromMigrations
            || target.IsExcludedFromMigrations)
        {
            // Populate column mapping
            foreach (var _ in Diff(source.Columns, target.Columns, diffContext))
            {
            }

            yield break;
        }

        if (source.Schema != target.Schema
            || source.Name != target.Name)
        {
            var renameTableOperation = new RenameTableOperation
            {
                Schema = source.Schema,
                Name = source.Name,
                NewSchema = target.Schema,
                NewName = target.Name
            };

            renameTableOperation.AddAnnotations(MigrationsAnnotationProvider.ForRename(source));

            yield return renameTableOperation;
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
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Add(
        ITable target,
        DiffContext diffContext)
    {
        if (target.IsExcludedFromMigrations)
        {
            yield break;
        }

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
            target.UniqueConstraints.Where(c => !c.GetIsPrimaryKey()).SelectMany(c => Add(c, diffContext))
                .Cast<AddUniqueConstraintOperation>());
        createTableOperation.CheckConstraints.AddRange(
            target.CheckConstraints.SelectMany(c => Add(c, diffContext))
                .Cast<AddCheckConstraintOperation>());

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
        ITable source,
        DiffContext diffContext)
    {
        if (source.IsExcludedFromMigrations)
        {
            yield break;
        }

        var operation = new DropTableOperation { Schema = source.Schema, Name = source.Name };
        operation.AddAnnotations(MigrationsAnnotationProvider.ForRemove(source));

        diffContext.AddDrop(source, operation);

        yield return operation;
    }

    private static IEnumerable<IColumn> GetSortedColumns(ITable table)
    {
        var columns = table.Columns.Where(x => x is not JsonColumn).ToHashSet();
        var sortedColumns = new List<IColumn>(columns.Count);
        foreach (var property in GetSortedProperties(GetMainType(table).GetRootType(), table))
        {
            var column = table.FindColumn(property)!;
            if (columns.Remove(column))
            {
                sortedColumns.Add(column);
            }
        }

        Check.DebugAssert(columns.Count == 0, "columns is not empty");

        // issue #28539
        // ideally we should inject JSON column in the place corresponding to the navigation that maps to it in the clr type
        var jsonColumns = table.Columns.Where(x => x is JsonColumn).OrderBy(x => x.Name);

        return sortedColumns.Where(c => c.Order.HasValue).OrderBy(c => c.Order)
            .Concat(sortedColumns.Where(c => !c.Order.HasValue))
            .Concat(columns)
            .Concat(jsonColumns);
    }

    private static IEnumerable<IProperty> GetSortedProperties(IEntityType entityType, ITable table)
    {
        var leastPriorityProperties = new List<IProperty>();
        var leastPriorityPrimaryKeyProperties = new List<IProperty>();
        var primaryKeyPropertyGroups = new Dictionary<PropertyInfo, IProperty>();
        var groups = new Dictionary<PropertyInfo, List<IProperty>>();
        var unorderedGroups = new Dictionary<PropertyInfo, SortedDictionary<(int, string), IProperty>>();
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

                clrProperty = foreignKey.DependentToPrincipal!.PropertyInfo!;
                var groupIndex = foreignKey.Properties.IndexOf(property);

                unorderedGroups.GetOrAddNew(clrProperty).Add((groupIndex, property.Name), property);
            }
            else
            {
                if (property.IsPrimaryKey())
                {
                    primaryKeyPropertyGroups.Add(clrProperty, property);
                }

                groups.Add(clrProperty, [property]);
            }

            var clrType = clrProperty.DeclaringType!;
            var index = clrType.GetTypeInfo().DeclaredProperties
                .IndexOf(clrProperty, PropertyInfoEqualityComparer.Instance);

            Check.DebugAssert(clrType != null, "clrType is null");
            types.GetOrAddNew(clrType)[index] = clrProperty;
        }

        AddNestedComplexProperties(entityType, leastPriorityProperties);

        foreach (var (propertyInfo, properties) in unorderedGroups)
        {
            groups.Add(propertyInfo, properties.Values.ToList());
        }

        if (table.EntityTypeMappings.Any(m => m.TypeBase == entityType))
        {
            foreach (var linkingForeignKey in table.GetReferencingRowInternalForeignKeys(entityType))
            {
                // skip JSON entities, their properties are not mapped to anything
                if (linkingForeignKey.DeclaringEntityType.IsMappedToJson())
                {
                    continue;
                }

                var linkingNavigationProperty = linkingForeignKey.PrincipalToDependent?.PropertyInfo;
                var properties = GetSortedProperties(linkingForeignKey.DeclaringEntityType, table).ToList();
                if (linkingNavigationProperty == null
                    || (linkingForeignKey.PrincipalToDependent!.IsIndexerProperty()))
                {
                    leastPriorityProperties.AddRange(properties);

                    continue;
                }

                groups.Add(linkingNavigationProperty, properties);

                var clrType = linkingNavigationProperty.DeclaringType!;
                var index = clrType.GetTypeInfo().DeclaredProperties
                    .IndexOf(linkingNavigationProperty, PropertyInfoEqualityComparer.Instance);

                Check.DebugAssert(clrType != null, "clrType is null");
                types.GetOrAddNew(clrType)[index] = linkingNavigationProperty;
            }
        }

        var graph = new Multigraph<Type, object?>();
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
            // ReSharper disable once RedundantEnumerableCastCall
            .Where(e => e != null).Cast<IProperty>()
            .Concat(leastPriorityPrimaryKeyProperties)
            .Concat(
                sortedPropertyInfos
                    .Where(pi => !primaryKeyPropertyGroups.ContainsKey(pi) && entityType.ClrType.IsAssignableFrom(pi.DeclaringType))
                    .SelectMany(p => groups[p]))
            .Concat(leastPriorityProperties)
            .Concat(entityType.GetDirectlyDerivedTypes().SelectMany(et => GetSortedProperties(et, table)))
            .Concat(
                sortedPropertyInfos
                    .Where(pi => !primaryKeyPropertyGroups.ContainsKey(pi) && !entityType.ClrType.IsAssignableFrom(pi.DeclaringType))
                    .SelectMany(p => groups[p]));
    }

    private static void AddNestedComplexProperties(ITypeBase typeBase, List<IProperty> leastPriorityProperties)
    {
        foreach (var complexProperty in typeBase.GetDeclaredComplexProperties())
        {
            foreach (var complexTypeProperty in complexProperty.ComplexType.GetDeclaredProperties())
            {
                leastPriorityProperties.Add(complexTypeProperty);
            }

            AddNestedComplexProperties(complexProperty.ComplexType, leastPriorityProperties);
        }
    }

    private sealed class PropertyInfoEqualityComparer : IEqualityComparer<PropertyInfo>
    {
        private PropertyInfoEqualityComparer()
        {
        }

        public static readonly PropertyInfoEqualityComparer Instance = new();

        public bool Equals(PropertyInfo? x, PropertyInfo? y)
            => x.IsSameAs(y);

        public int GetHashCode(PropertyInfo obj)
            => throw new NotSupportedException();
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
        IEnumerable<IColumn> source,
        IEnumerable<IColumn> target,
        DiffContext diffContext)
        => DiffCollection(
            source,
            target,
            diffContext,
            Diff,
            (t, c) => Add(t, c),
            Remove,
            (s, t, _) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase),
            (s, t, c) => s.PropertyMappings.Any(
                sm =>
                    t.PropertyMappings.Any(
                        tm =>
                            string.Equals(sm.Property.Name, tm.Property.Name, StringComparison.OrdinalIgnoreCase)
                            && EntityTypePathEquals(sm.Property.DeclaringType, tm.Property.DeclaringType, c))),
            (s, t, _) => s.PropertyMappings.Any(
                sm =>
                    t.PropertyMappings.Any(
                        tm =>
                            string.Equals(sm.Property.Name, tm.Property.Name, StringComparison.OrdinalIgnoreCase))),
            (s, t, c) => ColumnStructureEquals(s, t)
                && s.PropertyMappings.Any(
                    sm =>
                        t.PropertyMappings.Any(
                            tm =>
                                string.Equals(sm.Property.Name, tm.Property.Name, StringComparison.OrdinalIgnoreCase)
                                && EntityTypePathEquals(sm.Property.DeclaringType, tm.Property.DeclaringType, c))),
            (s, t, _) => ColumnStructureEquals(s, t) && ColumnAnnotationsEqual(s, t, matchValues: true),
            (s, t, _) => ColumnStructureEquals(s, t) && ColumnAnnotationsEqual(s, t, matchValues: false),
            (s, t, _) => ColumnStructureEquals(s, t));

    private static bool ColumnAnnotationsEqual(IColumn source, IColumn target, bool matchValues)
    {
        var sourceAnnotations = source.GetAnnotations().ToList();
        var targetAnnotations = target.GetAnnotations().ToList();

        if (sourceAnnotations.Count != targetAnnotations.Count)
        {
            return false;
        }

        foreach (var sourceAnnotation in sourceAnnotations)
        {
            var matchFound = false;
            for (var i = 0; i < targetAnnotations.Count; i++)
            {
                var targetAnnotation = targetAnnotations[i];

                if (sourceAnnotation.Name != targetAnnotation.Name)
                {
                    continue;
                }

                if (matchValues && sourceAnnotation.Value != targetAnnotation.Value)
                {
                    continue;
                }

                targetAnnotations.RemoveAt(i);
                matchFound = true;

                break;
            }

            if (!matchFound)
            {
                return false;
            }
        }

        return true;
    }

    private static bool ColumnStructureEquals(IColumn source, IColumn target)
    {
        if (!source.TryGetDefaultValue(out var sourceDefault))
        {
            sourceDefault = null;
        }

        if (!target.TryGetDefaultValue(out var targetDefault))
        {
            targetDefault = null;
        }

        return source.StoreType == target.StoreType
            && source.IsRowVersion == target.IsRowVersion
            && source.IsNullable == target.IsNullable
            && source.Precision == target.Precision
            && source.Scale == target.Scale
            && source.IsUnicode == target.IsUnicode
            && source.MaxLength == target.MaxLength
            && source.IsFixedLength == target.IsFixedLength
            && source.Collation == target.Collation
            && source.Comment == target.Comment
            && source.IsStored == target.IsStored
            && source.ComputedColumnSql == target.ComputedColumnSql
            && Equals(sourceDefault, targetDefault)
            && source.DefaultValueSql == target.DefaultValueSql;
    }

    private static bool EntityTypePathEquals(ITypeBase source, ITypeBase target, DiffContext diffContext)
    {
        var sourceTable = diffContext.FindTable(source);
        var targetTable = diffContext.FindTable(target);

        if ((sourceTable == null
                && targetTable == null)
            || (sourceTable?.EntityTypeMappings.Count() == 1
                && targetTable?.EntityTypeMappings.Count() == 1))
        {
            return true;
        }

        if (source.Name != target.Name)
        {
            return false;
        }

        if (source is IEntityType sourceEntityType
            && target is IEntityType targetEntityType)
        {
            var nextSource = sourceTable?.GetRowInternalForeignKeys(sourceEntityType).FirstOrDefault()?.PrincipalEntityType;
            var nextTarget = targetTable?.GetRowInternalForeignKeys(targetEntityType).FirstOrDefault()?.PrincipalEntityType;
            return (nextSource == null && nextTarget == null)
                || (nextSource != null
                    && nextTarget != null
                    && EntityTypePathEquals(nextSource, nextTarget, diffContext));
        }

        if (source is IComplexType sourceComplexType
            && target is IComplexType targetComplexType)
        {
            var nextSource = sourceComplexType.ComplexProperty.DeclaringType;
            var nextTarget = targetComplexType.ComplexProperty.DeclaringType;
            return (nextSource == null && nextTarget == null)
                || (nextSource != null
                    && nextTarget != null
                    && EntityTypePathEquals(nextSource, nextTarget, diffContext));
        }

        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Diff(
        IColumn source,
        IColumn target,
        DiffContext diffContext)
    {
        var table = target.Table;

        if (source.Name != target.Name)
        {
            var renameColumnOperation = new RenameColumnOperation
            {
                Schema = table.Schema,
                Table = table.Name,
                Name = source.Name,
                NewName = target.Name
            };

            renameColumnOperation.AddAnnotations(MigrationsAnnotationProvider.ForRename(source));

            yield return renameColumnOperation;
        }

        var sourceMigrationsAnnotations = source.GetAnnotations();
        var targetMigrationsAnnotations = target.GetAnnotations();

        var isNullableChanged = source.IsNullable != target.IsNullable;
        var columnTypeChanged = source.StoreType != target.StoreType;

        if (!source.TryGetDefaultValue(out var sourceDefault))
        {
            sourceDefault = null;
        }

        if (!target.TryGetDefaultValue(out var targetDefault))
        {
            targetDefault = null;
        }

        if (isNullableChanged
            || columnTypeChanged
            || source.DefaultValueSql != target.DefaultValueSql
            || source.ComputedColumnSql != target.ComputedColumnSql
            || source.IsStored != target.IsStored
            || sourceDefault?.GetType() != targetDefault?.GetType()
            || (sourceDefault != DBNull.Value && !target.ProviderValueComparer.Equals(sourceDefault, targetDefault))
            || source.Comment != target.Comment
            || source.Collation != target.Collation
            || source.Order != target.Order
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

            InitializeColumnHelper(alterColumnOperation, target, inline: !source.IsNullable);
            InitializeColumnHelper(alterColumnOperation.OldColumn, source, inline: true);

            if (source.Order != target.Order)
            {
                if (source is not JsonColumn && source.Order.HasValue)
                {
                    alterColumnOperation.OldColumn.AddAnnotation(RelationalAnnotationNames.ColumnOrder, source.Order.Value);
                }

                if (target is not JsonColumn && target.Order.HasValue)
                {
                    alterColumnOperation.AddAnnotation(RelationalAnnotationNames.ColumnOrder, target.Order.Value);
                }
            }

            yield return alterColumnOperation;
        }
    }

    private void InitializeColumnHelper(ColumnOperation columnOperation, IColumn column, bool inline)
    {
        if (column is JsonColumn jsonColumn)
        {
            InitializeJsonColumn(columnOperation, jsonColumn, jsonColumn.IsNullable, column.GetAnnotations(), inline);
        }
        else
        {
            var columnTypeMapping = column.StoreTypeMapping;

            Initialize(
                columnOperation, column, columnTypeMapping, column.IsNullable,
                column.GetAnnotations(), inline);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Add(
        IColumn target,
        DiffContext diffContext,
        bool inline = false)
    {
        var table = target.Table;

        var operation = new AddColumnOperation
        {
            Schema = table.Schema,
            Table = table.Name,
            Name = target.Name
        };

        InitializeColumnHelper(operation, target, inline);
        if (target is not JsonColumn)
        {
            if (!inline && target.Order.HasValue)
            {
                operation.AddAnnotation(RelationalAnnotationNames.ColumnOrder, target.Order.Value);
            }
        }

        yield return operation;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Remove(IColumn source, DiffContext diffContext)
    {
        var table = source.Table;

        var operation = new DropColumnOperation
        {
            Schema = table.Schema,
            Table = table.Name,
            Name = source.Name
        };
        operation.AddAnnotations(MigrationsAnnotationProvider.ForRemove(source));

        diffContext.AddDrop(source, operation);

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
        if (column.DefaultValue == DBNull.Value)
        {
            throw new InvalidOperationException(
                RelationalStrings.DefaultValueUnspecified(
                    column.Table.SchemaQualifiedName,
                    column.Name));
        }

        if (column.DefaultValueSql?.Length == 0)
        {
            throw new InvalidOperationException(
                RelationalStrings.DefaultValueSqlUnspecified(
                    column.Table.SchemaQualifiedName,
                    column.Name));
        }

        if (column.ComputedColumnSql?.Length == 0)
        {
            throw new InvalidOperationException(
                RelationalStrings.ComputedColumnSqlUnspecified(
                    column.Name,
                    column.Table.SchemaQualifiedName));
        }

        var property = column.PropertyMappings.First().Property;
        var valueConverter = GetValueConverter(property, typeMapping);
        columnOperation.ClrType
            = (valueConverter?.ProviderClrType
                ?? typeMapping.ClrType).UnwrapNullableType();

        if (!column.TryGetDefaultValue(out var defaultValue))
        {
            // for non-nullable collections of primitives that are mapped to JSON we set a default value corresponding to empty JSON collection
            defaultValue = !inline
                && column is { IsNullable: false, StoreTypeMapping: { ElementTypeMapping: not null, Converter: ValueConverter columnValueConverter } }
                && columnValueConverter.GetType() is Type { IsGenericType: true } columnValueConverterType
                && columnValueConverterType.GetGenericTypeDefinition() == typeof(CollectionToJsonStringConverter<>)
                ? "[]"
                : null;
        }

        columnOperation.DefaultValue = defaultValue
            ?? (inline || isNullable
                ? null
                : GetDefaultValue(columnOperation.ClrType));
        columnOperation.DefaultValueSql = column.DefaultValueSql;
        columnOperation.ColumnType = column.StoreType;
        columnOperation.MaxLength = column.MaxLength;
        columnOperation.Precision = column.Precision;
        columnOperation.Scale = column.Scale;
        columnOperation.IsUnicode = column.IsUnicode;
        columnOperation.IsFixedLength = column.IsFixedLength;
        columnOperation.IsRowVersion = column.IsRowVersion;
        columnOperation.IsNullable = isNullable;
        columnOperation.ComputedColumnSql = column.ComputedColumnSql;
        columnOperation.IsStored = column.IsStored;
        columnOperation.Comment = column.Comment;
        columnOperation.Collation = column.Collation;
        columnOperation.AddAnnotations(migrationsAnnotations);
    }

    private void InitializeJsonColumn(
        ColumnOperation columnOperation,
        JsonColumn jsonColumn,
        bool isNullable,
        IEnumerable<IAnnotation> migrationsAnnotations,
        bool inline = false)
    {
        columnOperation.ColumnType = jsonColumn.StoreType;
        columnOperation.IsNullable = isNullable;

        // TODO: flow this from type mapping
        // issue #28596
        columnOperation.ClrType = typeof(string);
        columnOperation.DefaultValue = inline || isNullable
            ? null
            : "{}";

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
        IEnumerable<IUniqueConstraint> source,
        IEnumerable<IUniqueConstraint> target,
        DiffContext diffContext)
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
                && s.GetIsPrimaryKey() == t.GetIsPrimaryKey()
                && !HasDifferences(s.GetAnnotations(), t.GetAnnotations()));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Diff(
        IUniqueConstraint source,
        IUniqueConstraint target,
        DiffContext diffContext)
        => Enumerable.Empty<MigrationOperation>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Add(IUniqueConstraint target, DiffContext diffContext)
    {
        if (target.GetIsPrimaryKey())
        {
            yield return AddPrimaryKeyOperation.CreateFrom((IPrimaryKeyConstraint)target);
        }
        else
        {
            yield return AddUniqueConstraintOperation.CreateFrom(target);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Remove(
        IUniqueConstraint source,
        DiffContext diffContext)
    {
        var table = source.Table;

        MigrationOperation operation;
        if (source.GetIsPrimaryKey())
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

        operation.AddAnnotations(MigrationsAnnotationProvider.ForRemove(source));

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
        IEnumerable<IForeignKeyConstraint> source,
        IEnumerable<IForeignKeyConstraint> target,
        DiffContext diffContext)
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
        IForeignKeyConstraint source,
        IForeignKeyConstraint target,
        DiffContext diffContext)
        => Enumerable.Empty<MigrationOperation>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Add(IForeignKeyConstraint target, DiffContext diffContext)
    {
        var targetTable = target.Table;
        if (targetTable.IsExcludedFromMigrations)
        {
            yield break;
        }

        var operation = AddForeignKeyOperation.CreateFrom(target);

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
    protected virtual IEnumerable<MigrationOperation> Remove(IForeignKeyConstraint source, DiffContext diffContext)
    {
        var sourceTable = source.Table;
        if (sourceTable.IsExcludedFromMigrations)
        {
            yield break;
        }

        var dropTableOperation = diffContext.FindDrop(sourceTable);
        if (dropTableOperation == null)
        {
            var operation = new DropForeignKeyOperation
            {
                Schema = sourceTable.Schema,
                Table = sourceTable.Name,
                Name = source.Name
            };
            operation.AddAnnotations(MigrationsAnnotationProvider.ForRemove(source));

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
        IEnumerable<ITableIndex> source,
        IEnumerable<ITableIndex> target,
        DiffContext diffContext)
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
            && ((source.IsDescending is null && target.IsDescending is null)
                || (source.IsDescending is not null
                    && target.IsDescending is not null
                    && source.IsDescending.SequenceEqual(target.IsDescending)))
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
        ITableIndex source,
        ITableIndex target,
        DiffContext diffContext)
    {
        var targetTable = target.Table;
        var sourceName = source.Name;
        var targetName = target.Name;

        if (sourceName != targetName)
        {
            var renameIndexOperation = new RenameIndexOperation
            {
                Schema = targetTable.Schema,
                Table = targetTable.Name,
                Name = sourceName,
                NewName = targetName
            };

            renameIndexOperation.AddAnnotations(MigrationsAnnotationProvider.ForRename(source));

            yield return renameIndexOperation;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Add(
        ITableIndex target,
        DiffContext diffContext)
    {
        yield return CreateIndexOperation.CreateFrom(target);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Remove(ITableIndex source, DiffContext diffContext)
    {
        var sourceTable = source.Table;

        var operation = new DropIndexOperation
        {
            Name = source.Name,
            Schema = sourceTable.Schema,
            Table = sourceTable.Name
        };
        operation.AddAnnotations(MigrationsAnnotationProvider.ForRemove(source));

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
        IEnumerable<ICheckConstraint> source,
        IEnumerable<ICheckConstraint> target,
        DiffContext diffContext)
        => DiffCollection(
            source,
            target,
            diffContext,
            Diff,
            Add,
            Remove,
            (s, t, c) => c.FindTable(s.EntityType) == c.FindSource(c.FindTable(t.EntityType))
                && string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase)
                && string.Equals(s.Sql, t.Sql, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Diff(
        ICheckConstraint source,
        ICheckConstraint target,
        DiffContext diffContext)
        => Enumerable.Empty<MigrationOperation>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Add(ICheckConstraint target, DiffContext diffContext)
    {
        var operation = AddCheckConstraintOperation.CreateFrom(target);
        operation.AddAnnotations(RelationalAnnotationProvider.For(target, designTime: true));
        yield return operation;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Remove(ICheckConstraint source, DiffContext diffContext)
    {
        var sourceEntityType = source.EntityType;

        var operation = new DropCheckConstraintOperation
        {
            Name = source.Name!,
            Schema = sourceEntityType.GetSchema(),
            Table = sourceEntityType.GetTableName()!
        };
        operation.AddAnnotations(MigrationsAnnotationProvider.ForRemove(source));

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
        IEnumerable<ISequence> source,
        IEnumerable<ISequence> target,
        DiffContext diffContext)
        => DiffCollection(
            source,
            target,
            diffContext,
            Diff,
            Add,
            Remove,
            (s, t, _) => string.Equals(s.Schema, t.Schema, StringComparison.OrdinalIgnoreCase)
                && string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase)
                && s.Type == t.Type,
            (s, t, _) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase)
                && s.Type == t.Type);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Diff(
        ISequence source,
        ISequence target,
        DiffContext diffContext)
    {
        if (source.Schema != target.Schema
            || source.Name != target.Name)
        {
            var renameSequenceOperation = new RenameSequenceOperation
            {
                Schema = source.Schema,
                Name = source.Name,
                NewSchema = target.Schema,
                NewName = target.Name
            };

            renameSequenceOperation.AddAnnotations(MigrationsAnnotationProvider.ForRename(source));

            yield return renameSequenceOperation;
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

        var sourceMigrationsAnnotations = RelationalAnnotationProvider.For(source, designTime: true);
        var targetMigrationsAnnotations = RelationalAnnotationProvider.For(target, designTime: true);

        if (source.IncrementBy != target.IncrementBy
            || source.MaxValue != target.MaxValue
            || source.MinValue != target.MinValue
            || source.IsCyclic != target.IsCyclic
            || source.IsCached != target.IsCached
            || source.CacheSize != target.CacheSize
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
    protected virtual IEnumerable<MigrationOperation> Add(ISequence target, DiffContext diffContext)
    {
        var operation = new CreateSequenceOperation
        {
            Schema = target.Schema,
            Name = target.Name,
            ClrType = target.Type,
            StartValue = target.StartValue
        };

        yield return Initialize(operation, target, RelationalAnnotationProvider.For(target, designTime: true));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> Remove(ISequence source, DiffContext diffContext)
    {
        var operation = new DropSequenceOperation { Schema = source.Schema, Name = source.Name };
        operation.AddAnnotations(MigrationsAnnotationProvider.ForRemove(source));

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
        sequenceOperation.IsCached = sequence.IsCached;
        sequenceOperation.CacheSize = sequence.CacheSize;
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
        IRelationalModel? source,
        IRelationalModel? target,
        DiffContext diffContext)
    {
        if (target == null)
        {
            _targetIdentityMaps = null;
            return;
        }

        if (_targetIdentityMaps == null)
        {
            _targetIdentityMaps = new Dictionary<ITable, IRowIdentityMap>(TableBaseIdentityComparer.Instance);
        }
        else
        {
            _targetIdentityMaps.Clear();
        }

        foreach (var targetEntityType in target.Model.GetEntityTypes())
        {
            AddSeedData(targetEntityType, _targetIdentityMaps, EntityState.Added);
        }

        if (source == null)
        {
            _sourceIdentityMaps = null;
            return;
        }

        if (_sourceIdentityMaps == null)
        {
            _sourceIdentityMaps = new Dictionary<ITable, IRowIdentityMap>(TableBaseIdentityComparer.Instance);
        }
        else
        {
            _sourceIdentityMaps.Clear();
        }

        foreach (var sourceEntityType in source.Model.GetEntityTypes())
        {
            AddSeedData(sourceEntityType, _sourceIdentityMaps, EntityState.Deleted);
        }
    }

    private void AddSeedData(IEntityType entityType, Dictionary<ITable, IRowIdentityMap> identityMaps, EntityState initialState)
    {
        var sensitiveLoggingEnabled = CommandBatchPreparerDependencies.LoggingOptions.IsSensitiveDataLoggingEnabled;

#pragma warning disable EF1001 // Internal EF Core API usage.
        foreach (var rawSeed in ((EntityType)entityType).GetRawSeedData())
        {
#pragma warning restore EF1001 // Internal EF Core API usage.
            Func<IProperty, object, (object?, bool)> getValue;
            var type = rawSeed.GetType();
            if (entityType.ClrType.IsAssignableFrom(type))
            {
                getValue = GetClrValue;
            }
            else
            {
                // anonymous type
                var anonymousProperties = type.GetMembersInHierarchy()
                    .OfType<PropertyInfo>()
                    .ToDictionary(p => p.GetSimpleMemberName());

                getValue = (property, seed) =>
                    anonymousProperties.TryGetValue(property.Name, out var propertyInfo)
                        ? (propertyInfo.GetValue(seed), true)
                        : (null, false);
            }

            foreach (var mapping in entityType.GetTableMappings())
            {
                INonTrackedModificationCommand command;
                var table = mapping.Table;
                var keyConstraint = table.PrimaryKey!;
                if (!identityMaps.TryGetValue(table, out var identityMap))
                {
                    identityMap = RowIdentityMapFactory.Create(keyConstraint);
                    identityMaps.Add(table, identityMap);
                }

                var key = new object?[keyConstraint.Columns.Count];
                var keyFound = true;
                for (var i = 0; i < key.Length; i++)
                {
                    var columnMapping = keyConstraint.Columns[i].FindColumnMapping(entityType)!;
                    var property = columnMapping.Property;
                    var (value, hasValue) = getValue(property, rawSeed);
                    if (!hasValue)
                    {
                        keyFound = false;
                        break;
                    }

                    var valueConverter = columnMapping.TypeMapping.Converter;
                    key[i] = valueConverter == null
                        ? value
                        : valueConverter.ConvertToProvider(value);
                }

                if (!keyFound)
                {
                    continue;
                }

                if (identityMap.FindCommand(key) is { } existingCommand)
                {
                    if (!table.IsShared)
                    {
                        if (sensitiveLoggingEnabled)
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.DuplicateSeedDataSensitive(
                                    entityType.DisplayName(),
                                    BuildValuesString(key),
                                    table.SchemaQualifiedName));
                        }

                        throw new InvalidOperationException(
                            RelationalStrings.DuplicateSeedData(
                                entityType.DisplayName(),
                                table.SchemaQualifiedName));
                    }

                    command = existingCommand;
                }
                else
                {
                    command = CommandBatchPreparerDependencies.ModificationCommandFactory.CreateNonTrackedModificationCommand(
                        new NonTrackedModificationCommandParameters(table, sensitiveLoggingEnabled));
                    command.EntityState = initialState;

                    identityMap.Add(key, command);
                }

                foreach (var columnMapping in mapping.ColumnMappings)
                {
                    var property = columnMapping.Property;
                    var column = columnMapping.Column;

                    if ((column.ComputedColumnSql != null)
                        || property.ValueGenerated.HasFlag(ValueGenerated.OnUpdate))
                    {
                        continue;
                    }

                    var writeValue = true;
                    var (value, hasValue) = getValue(property, rawSeed);
                    if (!hasValue)
                    {
                        value = property.ClrType.GetDefaultValue();
                    }

                    if (!hasValue
                        || Equals(value, property.ClrType.GetDefaultValue()))
                    {
                        if (property.GetValueGeneratorFactory() != null
                            && property == (property.DeclaringType as IEntityType)?.FindDiscriminatorProperty())
                        {
                            value = entityType.GetDiscriminatorValue()!;
                        }
                        else if (property.ValueGenerated.HasFlag(ValueGenerated.OnAdd))
                        {
                            writeValue = false;
                        }
                    }

                    var valueConverter = columnMapping.TypeMapping.Converter;
                    value = valueConverter == null
                        ? value
                        : valueConverter.ConvertToProvider(value);

                    if (!writeValue)
                    {
                        if (column.DefaultValue != null)
                        {
                            value = column.DefaultValue;
                        }
                        else if (value == null
                                 && !column.IsNullable)
                        {
                            value = column.ProviderClrType.GetDefaultValue();
                        }
                    }

                    var existingColumnModification = command.ColumnModifications.FirstOrDefault(c => c.ColumnName == column.Name);
                    if (existingColumnModification != null)
                    {
                        if (!Equals(existingColumnModification.Value, value))
                        {
                            if (sensitiveLoggingEnabled)
                            {
                                throw new InvalidOperationException(
                                    RelationalStrings.ConflictingSeedValuesSensitive(
                                        entityType.DisplayName(),
                                        BuildValuesString(key),
                                        table.SchemaQualifiedName,
                                        existingColumnModification.ColumnName,
                                        Convert.ToString(existingColumnModification.Value, CultureInfo.InvariantCulture),
                                        Convert.ToString(value, CultureInfo.InvariantCulture)));
                            }

                            throw new InvalidOperationException(
                                RelationalStrings.ConflictingSeedValues(
                                    entityType.DisplayName(),
                                    table.SchemaQualifiedName,
                                    existingColumnModification.ColumnName));
                        }

                        continue;
                    }

                    writeValue = writeValue
                        && initialState != EntityState.Deleted
                        && property.GetBeforeSaveBehavior() == PropertySaveBehavior.Save;

                    command.AddColumnModification(
                        new ColumnModificationParameters(
                            column, originalValue: value, value, property, columnMapping.TypeMapping,
                            read: false, write: writeValue,
                            key: property.IsPrimaryKey(), condition: false,
                            sensitiveLoggingEnabled, column.IsNullable));
                }
            }
        }

        static (object?, bool) GetClrValue(IProperty property, object seed)
        {
#pragma warning disable EF1001 // Internal EF Core API usage.
            if (!property.TryGetMemberInfo(forMaterialization: false, forSet: false, out var memberInfo, out var _))
            {
                return (null, false);
            }
#pragma warning restore EF1001 // Internal EF Core API usage.

            object? value = null;
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    if (property.IsIndexerProperty())
                    {
                        try
                        {
                            value = propertyInfo.GetValue(seed, [property.Name]);
                        }
                        catch (Exception)
                        {
                            return (null, false);
                        }
                    }
                    else
                    {
                        value = propertyInfo.GetValue(seed);
                    }

                    break;
                case FieldInfo fieldInfo:
                    value = fieldInfo.GetValue(seed);
                    break;
            }

            return (value, true);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void DiffData(
        IRelationalModel? source,
        IRelationalModel? target,
        DiffContext diffContext)
    {
        if (source == null
            || target == null
            || _sourceIdentityMaps == null
            || _targetIdentityMaps == null)
        {
            return;
        }

        var tableMapping = new Dictionary<ITable, (ITable, IRowIdentityMap)?>();
        var unchangedColumns = new List<IColumnModification>();
        var overriddenColumns = new List<IColumnModification>();
        foreach (var targetPair in _targetIdentityMaps)
        {
            var (targetTable, targetIdentityMap) = targetPair;
            if (!tableMapping.TryGetValue(targetTable, out var sourcePair))
            {
                var targetKey = targetTable.PrimaryKey!;
                var foundSourceTable = diffContext.FindSource(targetTable);
                var sourceKey = foundSourceTable?.PrimaryKey;
                if (sourceKey == null
                    || !_sourceIdentityMaps.TryGetValue(foundSourceTable!, out var foundSourceIdentityMap))
                {
                    tableMapping.Add(targetTable, null);
                    continue;
                }

                var mappingFound = true;
                for (var i = 0; i < targetKey.Columns.Count; i++)
                {
                    var keyColumn = targetKey.Columns[i];
                    var sourceColumn = diffContext.FindSource(keyColumn);
                    if (sourceColumn == null
                        || sourceKey.Columns[i] != sourceColumn
                        || keyColumn.ProviderClrType != sourceColumn.ProviderClrType)
                    {
                        mappingFound = false;
                        break;
                    }
                }

                if (!mappingFound
                    || targetKey.Columns.Count != sourceKey.Columns.Count)
                {
                    tableMapping.Add(targetTable, null);
                    continue;
                }

                sourcePair = (foundSourceTable!, foundSourceIdentityMap);
                tableMapping.Add(targetTable, sourcePair);
            }
            else if (sourcePair == null)
            {
                continue;
            }

            var (sourceTable, sourceIdentityMap) = sourcePair.Value;
            var key = targetTable.PrimaryKey!;
            var keyValues = new object?[key.Columns.Count];
            foreach (var targetRow in targetIdentityMap.Rows)
            {
                for (var i = 0; i < keyValues.Length; i++)
                {
                    var modification = targetRow.ColumnModifications.First(m => m.ColumnName == key.Columns[i].Name);
                    keyValues[i] = modification.Value;
                }

                var sourceRow = sourceIdentityMap.FindCommand(keyValues);
                if (sourceRow == null)
                {
                    if (sourceTable.IsExcludedFromMigrations
                        || targetTable.IsExcludedFromMigrations)
                    {
                        targetRow.EntityState = EntityState.Unchanged;
                    }

                    continue;
                }

                if (sourceTable.IsExcludedFromMigrations
                    || targetTable.IsExcludedFromMigrations)
                {
                    targetRow.EntityState = EntityState.Unchanged;
                    sourceRow.EntityState = EntityState.Unchanged;
                    continue;
                }

                if (diffContext.FindDrop(sourceTable) != null)
                {
                    sourceRow.EntityState = EntityState.Unchanged;
                    continue;
                }

                var recreateRow = false;
                unchangedColumns.Clear();
                overriddenColumns.Clear();
                var anyColumnsModified = false;
                foreach (var targetColumnModification in targetRow.ColumnModifications)
                {
                    var targetColumnBase = targetColumnModification.Column!;
                    Check.DebugAssert(targetColumnBase is IColumn, "Non-IColumn columns not allowed");
                    var targetColumn = (IColumn)targetColumnBase;
                    var targetMapping = targetColumn.PropertyMappings.First();
                    var targetProperty = targetMapping.Property;

                    var sourceColumn = diffContext.FindSource(targetColumn);
                    if (sourceColumn == null)
                    {
                        if (targetProperty.GetAfterSaveBehavior() != PropertySaveBehavior.Save
                            && !targetProperty.ValueGenerated.HasFlag(ValueGenerated.OnUpdate))
                        {
                            recreateRow = true;
                            break;
                        }

                        anyColumnsModified = true;
                        continue;
                    }

                    var sourceColumnModification = sourceRow.ColumnModifications.FirstOrDefault(m => m.ColumnName == sourceColumn.Name);
                    if (sourceColumnModification == null)
                    {
                        if (targetColumnModification.IsWrite)
                        {
                            anyColumnsModified = true;
                        }

                        continue;
                    }

                    var sourceValue = sourceColumnModification.OriginalValue;
                    var targetValue = targetColumnModification.Value;
                    var comparer = targetColumn.ProviderValueComparer;
                    if (sourceColumn.ProviderClrType == targetColumn.ProviderClrType
                        && comparer.Equals(sourceValue, targetValue))
                    {
                        unchangedColumns.Add(targetColumnModification);
                        continue;
                    }

                    if (!targetColumnModification.IsWrite)
                    {
                        overriddenColumns.Add(targetColumnModification);
                    }
                    else if (targetProperty.GetAfterSaveBehavior() != PropertySaveBehavior.Save)
                    {
                        recreateRow = true;
                        break;
                    }

                    anyColumnsModified = true;
                }

                if (!recreateRow)
                {
                    sourceRow.EntityState = EntityState.Unchanged;
                    if (anyColumnsModified)
                    {
                        targetRow.EntityState = EntityState.Modified;
                        foreach (var unchangedColumn in unchangedColumns)
                        {
                            unchangedColumn.IsWrite = false;
                        }

                        foreach (var overriddenColumn in overriddenColumns)
                        {
                            overriddenColumn.IsWrite = true;
                        }
                    }
                    else
                    {
                        targetRow.EntityState = EntityState.Unchanged;
                    }
                }
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> GetDataOperations(
        IRelationalModel? source,
        IRelationalModel? target,
        DiffContext diffContext)
    {
        TrackData(source, target, diffContext);

        DiffData(source, target, diffContext);

        var dataOperations = GetDataOperations(forSource: true, diffContext)
            .Concat(GetDataOperations(forSource: false, diffContext));

        // This needs to be evaluated lazily
        foreach (var operation in dataOperations)
        {
            yield return operation;
        }
    }

    private IEnumerable<MigrationOperation> GetDataOperations(
        bool forSource,
        DiffContext diffContext)
    {
        var identityMaps = forSource ? _sourceIdentityMaps : _targetIdentityMaps;
        if (identityMaps == null)
        {
            yield break;
        }

        if (identityMaps.Count == 0)
        {
            yield break;
        }

        var commands = identityMaps.Values
            .SelectMany(m => m.Rows)
            .Where(
                r => r.EntityState is EntityState.Added or EntityState.Modified
                    || (r.EntityState is EntityState.Deleted && diffContext.FindDrop(r.Table!) == null));

        var commandSets = new CommandBatchPreparer(CommandBatchPreparerDependencies)
            .TopologicalSort(commands);

        foreach (var commandSet in commandSets)
        {
            InsertDataOperation? batchInsertOperation = null;
            foreach (var command in commandSet)
            {
                switch (command.EntityState)
                {
                    case EntityState.Added:
                        if (batchInsertOperation != null)
                        {
                            if (batchInsertOperation.Table == command.TableName
                                && batchInsertOperation.Schema == command.Schema
                                && batchInsertOperation.Columns.SequenceEqual(
                                    command.ColumnModifications.Where(col => col.IsKey || col.IsWrite).Select(col => col.ColumnName)))
                            {
                                batchInsertOperation.Values =
                                    AddToMultidimensionalArray(
                                        command.ColumnModifications.Where(col => col.IsKey || col.IsWrite).Select(col => col.Value)
                                            .ToList(),
                                        batchInsertOperation.Values);
                                continue;
                            }

                            yield return batchInsertOperation;
                        }

                        if (forSource)
                        {
                            Check.DebugFail("Insert using the source model");
                            break;
                        }

                        batchInsertOperation = new InsertDataOperation
                        {
                            Schema = command.Schema,
                            Table = command.TableName,
                            Columns = command.ColumnModifications.Where(col => col.IsKey || col.IsWrite).Select(col => col.ColumnName)
                                .ToArray(),
                            Values = ToMultidimensionalArray(
                                command.ColumnModifications.Where(col => col.IsKey || col.IsWrite).Select(col => col.Value).ToList())
                        };
                        break;
                    case EntityState.Modified:
                        if (batchInsertOperation != null)
                        {
                            yield return batchInsertOperation;
                            batchInsertOperation = null;
                        }

                        if (forSource)
                        {
                            Check.DebugFail("Update using the source model");
                            break;
                        }

                        yield return new UpdateDataOperation
                        {
                            Schema = command.Schema,
                            Table = command.TableName,
                            KeyColumns = command.ColumnModifications.Where(col => col.IsKey).Select(col => col.ColumnName).ToArray(),
                            KeyValues = ToMultidimensionalArray(
                                command.ColumnModifications.Where(col => col.IsKey).Select(col => col.Value).ToList()),
                            Columns = command.ColumnModifications.Where(col => col.IsWrite).Select(col => col.ColumnName).ToArray(),
                            Values = ToMultidimensionalArray(
                                command.ColumnModifications.Where(col => col.IsWrite).Select(col => col.Value).ToList()),
                            IsDestructiveChange = true
                        };
                        break;
                    case EntityState.Deleted:
                        if (batchInsertOperation != null)
                        {
                            yield return batchInsertOperation;
                            batchInsertOperation = null;
                        }

                        // There shouldn't be any deletes using the target model
                        Check.DebugAssert(forSource, "Delete using the target model");

                        var keyColumns = command.ColumnModifications.Where(col => col.IsKey)
                            .Select(c => (IColumn)c.Column!);
                        var anyKeyColumnDropped = keyColumns.Any(c => diffContext.FindDrop(c) != null);

                        yield return new DeleteDataOperation
                        {
                            Schema = command.Schema,
                            Table = command.TableName,
                            KeyColumns = command.ColumnModifications.Where(col => col.IsKey).Select(col => col.ColumnName).ToArray(),
                            KeyColumnTypes = anyKeyColumnDropped
                                ? keyColumns.Select(col => col.StoreType).ToArray()
                                : null,
                            KeyValues = ToMultidimensionalArray(
                                command.ColumnModifications.Where(col => col.IsKey).Select(col => col.Value).ToArray()),
                            IsDestructiveChange = true
                        };

                        break;
                    default:
                        throw new InvalidOperationException(command.EntityState.ToString());
                }
            }

            if (batchInsertOperation != null)
            {
                yield return batchInsertOperation;
            }
        }
    }

    #endregion

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<MigrationOperation> DiffCollection<T>(
        IEnumerable<T> sources,
        IEnumerable<T> targets,
        DiffContext diffContext,
        Func<T, T, DiffContext, IEnumerable<MigrationOperation>> diff,
        Func<T, DiffContext, IEnumerable<MigrationOperation>> add,
        Func<T, DiffContext, IEnumerable<MigrationOperation>> remove,
        params Func<T, T, DiffContext, bool>[] predicates)
        where T : notnull
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
    protected virtual bool HasDifferences(IEnumerable<IAnnotation> source, IEnumerable<IAnnotation> target)
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
    protected virtual IEnumerable<string> GetSchemas(IRelationalModel model)
        => model.Tables.Where(t => !t.IsExcludedFromMigrations).Select(t => t.Schema)
            .Concat(model.Views.Where(t => t.ViewDefinitionSql != null).Select(s => s.Schema))
            .Concat(model.Sequences.Select(s => s.Schema))
            .Where(s => !string.IsNullOrEmpty(s))
            // ReSharper disable once RedundantEnumerableCastCall
            .Cast<string>()
            .Distinct();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual object? GetDefaultValue(Type type)
        => type == typeof(string)
            ? string.Empty
            : type.IsArray
                ? Array.CreateInstance(type.GetElementType()!, 0)
                : type.UnwrapNullableType().GetDefaultValue();

    private static ValueConverter? GetValueConverter(IProperty property, RelationalTypeMapping? typeMapping = null)
        => (property.FindRelationalTypeMapping() ?? typeMapping)?.Converter;

    private static IEntityType GetMainType(ITable table)
        => (IEntityType)table.EntityTypeMappings.First(t => t.IsSharedTablePrincipal ?? true).TypeBase;

    private static object?[,] ToMultidimensionalArray(IReadOnlyList<object?> values)
    {
        var result = new object?[1, values.Count];
        for (var i = 0; i < values.Count; i++)
        {
            result[0, i] = values[i];
        }

        return result;
    }

    private static object?[,] AddToMultidimensionalArray(IReadOnlyList<object?> values, object?[,] array)
    {
        var width = array.GetLength(0);
        var height = array.GetLength(1);

        Check.DebugAssert(height == values.Count, $"height of {height} != values.Count of {values.Count}");

        var result = new object?[width + 1, height];
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

    private static string BuildValuesString(object?[] values)
        => "{"
            + string.Join(
                ", ", values.Select(
                    p => p == null
                        ? "<null>"
                        : Convert.ToString(p, CultureInfo.InvariantCulture)))
            + "}";

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

        private readonly IDictionary<IColumn, DropColumnOperation> _dropColumnOperations
            = new Dictionary<IColumn, DropColumnOperation>();

        private readonly IDictionary<DropTableOperation, ITable> _removedTables
            = new Dictionary<DropTableOperation, ITable>();

        private readonly IDictionary<DropColumnOperation, IColumn> _removedColumns
            = new Dictionary<DropColumnOperation, IColumn>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddMapping<T>(T source, T target)
            where T : notnull
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
        public virtual void AddCreate(ITable target, CreateTableOperation operation)
            => _createTableOperations.Add(target, operation);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddDrop(ITable source, DropTableOperation operation)
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
        public virtual void AddDrop(IColumn source, DropColumnOperation operation)
        {
            _dropColumnOperations.Add(source, operation);
            _removedColumns.Add(operation, source);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ITable? FindTable(ITypeBase typeBase)
            => typeBase.GetTableMappings().FirstOrDefault()?.Table;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual T? FindSource<T>(T? target)
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
        public virtual T? FindTarget<T>(T? source)
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
        public virtual CreateTableOperation? FindCreate(ITable target)
            => _createTableOperations.TryGetValue(target, out var operation)
                ? operation
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DropTableOperation? FindDrop(ITable source)
            => _dropTableOperations.TryGetValue(source, out var operation)
                ? operation
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DropColumnOperation? FindDrop(IColumn source)
            => _dropColumnOperations.TryGetValue(source, out var operation)
                ? operation
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ITable? FindTable(DropTableOperation operation)
            => _removedTables.TryGetValue(operation, out var source)
                ? source
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IColumn? FindColumn(DropColumnOperation operation)
            => _removedColumns.TryGetValue(operation, out var source)
                ? source
                : null;
    }
}
