// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     SQLite-specific implementation of <see cref="MigrationsSqlGenerator" />.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see>, and
///         <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
///     </para>
/// </remarks>
public class SqliteMigrationsSqlGenerator : MigrationsSqlGenerator
{
    /// <summary>
    ///     Creates a new <see cref="SqliteMigrationsSqlGenerator" /> instance.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    /// <param name="migrationsAnnotations">Provider-specific Migrations annotations to use.</param>
    public SqliteMigrationsSqlGenerator(
        MigrationsSqlGeneratorDependencies dependencies,
        IRelationalAnnotationProvider migrationsAnnotations)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     Generates commands from a list of operations.
    /// </summary>
    /// <param name="operations">The operations.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="options">The options to use when generating commands.</param>
    /// <returns>The list of commands to be executed or scripted.</returns>
    public override IReadOnlyList<MigrationCommand> Generate(
        IReadOnlyList<MigrationOperation> operations,
        IModel? model = null,
        MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
        => base.Generate(RewriteOperations(operations, model), model, options);

    private bool IsSpatialiteColumn(AddColumnOperation operation, IModel? model)
        => SqliteTypeMappingSource.IsSpatialiteType(
            operation.ColumnType
            ?? GetColumnType(
                operation.Schema,
                operation.Table,
                operation.Name,
                operation,
                model)!);

    private IReadOnlyList<MigrationOperation> RewriteOperations(
        IReadOnlyList<MigrationOperation> migrationOperations,
        IModel? model)
    {
        var operations = new List<MigrationOperation>();
        var rebuilds = new Dictionary<(string Table, string? Schema), RebuildContext>();
        foreach (var operation in migrationOperations)
        {
            switch (operation)
            {
                case AddPrimaryKeyOperation:
                case AddUniqueConstraintOperation:
                case AddCheckConstraintOperation:
                case AlterTableOperation:
                case DropCheckConstraintOperation:
                case DropForeignKeyOperation:
                case DropPrimaryKeyOperation:
                case DropUniqueConstraintOperation:
                {
                    var tableOperation = (ITableMigrationOperation)operation;
                    var rebuild = rebuilds.GetOrAddNew((tableOperation.Table, tableOperation.Schema));
                    rebuild.OperationsToReplace.Add(operation);

                    operations.Add(operation);

                    break;
                }

                case DropColumnOperation dropColumnOperation:
                {
                    var rebuild = rebuilds.GetOrAddNew((dropColumnOperation.Table, dropColumnOperation.Schema));
                    rebuild.OperationsToReplace.Add(dropColumnOperation);
                    rebuild.DropColumnsDeferred.Add(dropColumnOperation.Name);

                    operations.Add(dropColumnOperation);

                    break;
                }

                case AddForeignKeyOperation foreignKeyOperation:
                {
                    var table = operations
                        .OfType<CreateTableOperation>()
                        .FirstOrDefault(o => o.Name == foreignKeyOperation.Table);

                    if (table != null)
                    {
                        table.ForeignKeys.Add(foreignKeyOperation);
                    }
                    else
                    {
                        var rebuild = rebuilds.GetOrAddNew((foreignKeyOperation.Table, foreignKeyOperation.Schema));
                        rebuild.OperationsToReplace.Add(foreignKeyOperation);

                        operations.Add(foreignKeyOperation);
                    }

                    break;
                }

                case AlterColumnOperation alterColumnOperation:
                {
                    var rebuild = rebuilds.GetOrAddNew((alterColumnOperation.Table, alterColumnOperation.Schema));
                    rebuild.OperationsToReplace.Add(alterColumnOperation);
                    rebuild.AlterColumnsDeferred.Add(alterColumnOperation.Name, alterColumnOperation);

                    operations.Add(alterColumnOperation);

                    break;
                }

                case CreateIndexOperation createIndexOperation:
                {
                    if (rebuilds.TryGetValue((createIndexOperation.Table, createIndexOperation.Schema), out var rebuild)
                        && (rebuild.AddColumnsDeferred.Keys.Intersect(createIndexOperation.Columns).Any()
                            || rebuild.RenameColumnsDeferred.Keys.Intersect(createIndexOperation.Columns).Any()))
                    {
                        rebuild.OperationsToReplace.Add(createIndexOperation);
                        rebuild.CreateIndexesDeferred.Add(createIndexOperation.Name);
                    }

                    operations.Add(createIndexOperation);

                    break;
                }

                case RenameIndexOperation renameIndexOperation:
                {
                    var index = renameIndexOperation.Table != null
                        ? model?.GetRelationalModel().FindTable(renameIndexOperation.Table, renameIndexOperation.Schema)
                            ?.Indexes.FirstOrDefault(i => i.Name == renameIndexOperation.NewName)
                        : null;
                    if (index != null)
                    {
                        operations.Add(
                            new DropIndexOperation
                            {
                                Table = renameIndexOperation.Table,
                                Schema = renameIndexOperation.Schema,
                                Name = renameIndexOperation.Name
                            });

                        operations.Add(CreateIndexOperation.CreateFrom(index));
                    }
                    else
                    {
                        operations.Add(renameIndexOperation);
                    }

                    break;
                }

                case AddColumnOperation addColumnOperation:
                {
                    if (rebuilds.TryGetValue((addColumnOperation.Table, addColumnOperation.Schema), out var rebuild)
                        && rebuild.DropColumnsDeferred.Contains(addColumnOperation.Name))
                    {
                        rebuild.OperationsToReplace.Add(addColumnOperation);
                        rebuild.AddColumnsDeferred.Add(addColumnOperation.Name, addColumnOperation);
                    }
                    else if (addColumnOperation.Comment != null)
                    {
                        rebuilds.GetOrAddNew((addColumnOperation.Table, addColumnOperation.Schema));
                    }

                    operations.Add(addColumnOperation);

                    break;
                }

                case RenameColumnOperation renameColumnOperation:
                {
                    if (rebuilds.TryGetValue((renameColumnOperation.Table, renameColumnOperation.Schema), out var rebuild))
                    {
                        if (rebuild.DropColumnsDeferred.Contains(renameColumnOperation.NewName))
                        {
                            rebuild.OperationsToReplace.Add(renameColumnOperation);
                            rebuild.DropColumnsDeferred.Add(renameColumnOperation.Name);
                            rebuild.RenameColumnsDeferred.Add(renameColumnOperation.NewName, renameColumnOperation);
                        }
                    }

                    operations.Add(renameColumnOperation);

                    break;
                }

                case RenameTableOperation renameTableOperation:
                {
                    if (rebuilds.Remove((renameTableOperation.Name, renameTableOperation.Schema), out var rebuild))
                    {
                        rebuilds.Add(
                            (renameTableOperation.NewName ?? renameTableOperation.Name, renameTableOperation.NewSchema), rebuild);
                    }

                    operations.Add(renameTableOperation);

                    break;
                }

                case AlterSequenceOperation:
                case CreateSequenceOperation:
                case CreateTableOperation:
                case DropIndexOperation:
                case DropSchemaOperation:
                case DropSequenceOperation:
                case DropTableOperation:
                case EnsureSchemaOperation:
                case RenameSequenceOperation:
                case RestartSequenceOperation:
                {
                    operations.Add(operation);

                    break;
                }

                case DeleteDataOperation:
                case InsertDataOperation:
                case UpdateDataOperation:
                {
                    var tableOperation = (ITableMigrationOperation)operation;
                    if (rebuilds.TryGetValue((tableOperation.Table, tableOperation.Schema), out var rebuild))
                    {
                        rebuild.OperationsToWarnFor.Add(operation);
                    }

                    operations.Add(operation);

                    break;
                }

                default:
                {
                    foreach (var rebuild in rebuilds.Values)
                    {
                        rebuild.OperationsToWarnFor.Add(operation);
                    }

                    operations.Add(operation);

                    break;
                }
            }
        }

        var skippedRebuilds = new List<(string Table, string? Schema)>();
        var indexesToRebuild = new List<ITableIndex>();
        foreach (var (key, rebuildContext) in rebuilds)
        {
            var table = model?.GetRelationalModel().FindTable(key.Table, key.Schema);
            if (table == null)
            {
                skippedRebuilds.Add(key);

                continue;
            }

            foreach (var operationToWarnFor in rebuildContext.OperationsToWarnFor)
            {
                // TODO: Consider warning once per table--list all operation types we're warning for
                // TODO: Consider listing which operations required a rebuild
                Dependencies.MigrationsLogger.TableRebuildPendingWarning(operationToWarnFor.GetType(), table.Name);
            }

            foreach (var operationToReplace in rebuildContext.OperationsToReplace)
            {
                operations.Remove(operationToReplace);
            }

            var createTableOperation = new CreateTableOperation
            {
                Name = "ef_temp_" + table.Name,
                Schema = table.Schema,
                Comment = table.Comment
            };

            var primaryKey = table.PrimaryKey;
            if (primaryKey != null)
            {
                createTableOperation.PrimaryKey = AddPrimaryKeyOperation.CreateFrom(primaryKey);
            }

            foreach (var column in table.Columns.Where(c => c.Order.HasValue).OrderBy(c => c.Order!.Value)
                         .Concat(table.Columns.Where(c => !c.Order.HasValue)))
            {
                if (!column.TryGetDefaultValue(out var defaultValue))
                {
                    defaultValue = null;
                }

                var addColumnOperation = new AddColumnOperation
                {
                    Name = column.Name,
                    ColumnType = column.StoreType,
                    IsNullable = column.IsNullable,
                    DefaultValue = rebuildContext.AddColumnsDeferred.TryGetValue(column.Name, out var originalOperation)
                        && !originalOperation.IsNullable
                            ? originalOperation.DefaultValue
                            : defaultValue,
                    DefaultValueSql = column.DefaultValueSql,
                    ComputedColumnSql = column.ComputedColumnSql,
                    IsStored = column.IsStored,
                    Comment = column.Comment,
                    Collation = column.Collation,
                    Table = createTableOperation.Name
                };
                addColumnOperation.AddAnnotations(column.GetAnnotations());
                createTableOperation.Columns.Add(addColumnOperation);
            }

            foreach (var foreignKey in table.ForeignKeyConstraints)
            {
                createTableOperation.ForeignKeys.Add(AddForeignKeyOperation.CreateFrom(foreignKey));
            }

            foreach (var uniqueConstraint in table.UniqueConstraints.Where(c => !c.GetIsPrimaryKey()))
            {
                createTableOperation.UniqueConstraints.Add(AddUniqueConstraintOperation.CreateFrom(uniqueConstraint));
            }

            foreach (var checkConstraint in table.CheckConstraints)
            {
                createTableOperation.CheckConstraints.Add(AddCheckConstraintOperation.CreateFrom(checkConstraint));
            }

            createTableOperation.AddAnnotations(table.GetAnnotations());
            operations.Add(createTableOperation);

            foreach (var index in table.Indexes)
            {
                if (index.IsUnique && rebuildContext.CreateIndexesDeferred.Contains(index.Name))
                {
                    var createIndexOperation = CreateIndexOperation.CreateFrom(index);
                    createIndexOperation.Table = createTableOperation.Name;
                    operations.Add(createIndexOperation);
                }
                else
                {
                    indexesToRebuild.Add(index);
                }
            }

            var intoBuilder = new StringBuilder();
            var selectBuilder = new StringBuilder();
            var first = true;
            foreach (var column in table.Columns)
            {
                if (column.ComputedColumnSql != null
                    || rebuildContext.AddColumnsDeferred.ContainsKey(column.Name))
                {
                    continue;
                }

                if (first)
                {
                    first = false;
                }
                else
                {
                    intoBuilder.Append(", ");
                    selectBuilder.Append(", ");
                }

                intoBuilder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(column.Name));

                var defaultValue = rebuildContext.AlterColumnsDeferred.TryGetValue(column.Name, out var alterColumnOperation)
                    && alterColumnOperation is { IsNullable: false, OldColumn.IsNullable: true }
                        ? alterColumnOperation.DefaultValue
                        : null;
                if (defaultValue != null)
                {
                    selectBuilder.Append("IFNULL(");
                }

                selectBuilder.Append(
                    Dependencies.SqlGenerationHelper.DelimitIdentifier(
                        rebuildContext.RenameColumnsDeferred.TryGetValue(column.Name, out var renameColumnOperation)
                            ? renameColumnOperation.Name
                            : column.Name));

                if (defaultValue != null)
                {
                    var defaultValueTypeMapping = (column.StoreType == null
                            ? null
                            : Dependencies.TypeMappingSource.FindMapping(defaultValue.GetType(), column.StoreType))
                        ?? Dependencies.TypeMappingSource.GetMappingForValue(defaultValue);

                    selectBuilder
                        .Append(", ")
                        .Append(defaultValueTypeMapping.GenerateSqlLiteral(defaultValue))
                        .Append(')');
                }
            }

            operations.Add(
                new SqlOperation
                {
                    Sql = new StringBuilder()
                        .Append("INSERT INTO ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(createTableOperation.Name))
                        .Append(" (")
                        .Append(intoBuilder)
                        .AppendLine(")")
                        .Append("SELECT ")
                        .Append(selectBuilder)
                        .AppendLine()
                        .Append("FROM ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(table.Name))
                        .Append(Dependencies.SqlGenerationHelper.StatementTerminator)
                        .ToString()
                });
        }

        foreach (var skippedRebuild in skippedRebuilds)
        {
            rebuilds.Remove(skippedRebuild);
        }

        if (rebuilds.Any())
        {
            operations.Add(
                new SqlOperation { Sql = "PRAGMA foreign_keys = 0;", SuppressTransaction = true });
        }

        foreach (var ((table, schema), _) in rebuilds)
        {
            operations.Add(
                new DropTableOperation { Name = table, Schema = schema });
            operations.Add(
                new RenameTableOperation
                {
                    Name = "ef_temp_" + table,
                    Schema = schema,
                    NewName = table,
                    NewSchema = schema
                });
        }

        if (rebuilds.Any())
        {
            operations.Add(
                new SqlOperation { Sql = "PRAGMA foreign_keys = 1;", SuppressTransaction = true });
        }

        foreach (var index in indexesToRebuild)
        {
            operations.Add(CreateIndexOperation.CreateFrom(index));
        }

        return operations;
    }

    /// <summary>
    ///     Builds commands for the given <see cref="AlterDatabaseOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(AlterDatabaseOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        if (operation[SqliteAnnotationNames.InitSpatialMetaData] as bool? != true
            || operation.OldDatabase[SqliteAnnotationNames.InitSpatialMetaData] as bool? == true)
        {
            return;
        }

        builder
            .Append("SELECT InitSpatialMetaData()")
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        EndStatement(builder);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="AddColumnOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(AddColumnOperation operation, IModel? model, MigrationCommandListBuilder builder, bool terminate)
    {
        if (!IsSpatialiteColumn(operation, model))
        {
            base.Generate(operation, model, builder, terminate);

            return;
        }

        var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));
        var longTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(long));

        var srid = operation[SqliteAnnotationNames.Srid] as int? ?? 0;

        var geometryType = operation.ColumnType
            ?? GetColumnType(
                operation.Schema,
                operation.Table,
                operation.Name,
                operation,
                model);

        builder
            .Append("SELECT AddGeometryColumn(")
            .Append(stringTypeMapping.GenerateSqlLiteral(operation.Table))
            .Append(", ")
            .Append(stringTypeMapping.GenerateSqlLiteral(operation.Name))
            .Append(", ")
            .Append(longTypeMapping.GenerateSqlLiteral(srid))
            .Append(", ")
            .Append(stringTypeMapping.GenerateSqlLiteral(geometryType))
            .Append(", -1, ")
            .Append(operation.IsNullable ? "0" : "1")
            .Append(")");

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }
        else
        {
            Debug.Fail("I have a bad feeling about this. Geometry columns don't compose well.");
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DropIndexOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        DropIndexOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate)
    {
        builder
            .Append("DROP INDEX ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

        if (terminate)
        {
            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand();
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="RenameIndexOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(RenameIndexOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => throw new NotSupportedException(
            SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

    /// <summary>
    ///     Builds commands for the given <see cref="RenameTableOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(RenameTableOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        if (operation.NewName != null
            && operation.NewName != operation.Name)
        {
            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" RENAME TO ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand();
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="RenameTableOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(RenameColumnOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table))
            .Append(" RENAME COLUMN ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
            .Append(" TO ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
            .EndCommand();

    /// <summary>
    ///     Builds commands for the given <see cref="CreateTableOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        CreateTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        var spatialiteColumns = new Stack<AddColumnOperation>();
        for (var i = operation.Columns.Count - 1; i >= 0; i--)
        {
            var addColumnOperation = operation.Columns[i];

            if (IsSpatialiteColumn(addColumnOperation, model))
            {
                spatialiteColumns.Push(addColumnOperation);
                operation.Columns.RemoveAt(i);
            }
        }

        // Lifts a primary key definition into the typename.
        // This handles the quirks of creating integer primary keys using autoincrement, not default rowid behavior.
        if (operation.PrimaryKey?.Columns.Length == 1)
        {
            var columnOp = operation.Columns.FirstOrDefault(o => o.Name == operation.PrimaryKey.Columns[0]);
            if (columnOp != null)
            {
                columnOp.AddAnnotation(SqliteAnnotationNames.InlinePrimaryKey, true);
                if (!string.IsNullOrEmpty(operation.PrimaryKey.Name))
                {
                    columnOp.AddAnnotation(SqliteAnnotationNames.InlinePrimaryKeyName, operation.PrimaryKey.Name);
                }

                operation.PrimaryKey = null;
            }
        }

        builder
            .Append("CREATE TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
            .AppendLine(" (");

        using (builder.Indent())
        {
            if (!string.IsNullOrEmpty(operation.Comment))
            {
                builder
                    .AppendLines(Dependencies.SqlGenerationHelper.GenerateComment(operation.Comment))
                    .AppendLine();
            }

            CreateTableColumns(operation, model, builder);
            CreateTableConstraints(operation, model, builder);
            builder.AppendLine();
        }

        builder.Append(")");

        if (spatialiteColumns.Any())
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            while (spatialiteColumns.TryPop(out var spatialiteColumn))
            {
                Generate(spatialiteColumn, model, builder, spatialiteColumns.Any() || terminate);
            }
        }
        else if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for the column definitions in a <see cref="CreateTableOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected override void CreateTableColumns(
        CreateTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (operation.Columns.All(c => string.IsNullOrEmpty(c.Comment)))
        {
            base.CreateTableColumns(operation, model, builder);
        }
        else
        {
            CreateTableColumnsWithComments(operation, model, builder);
        }
    }

    private void CreateTableColumnsWithComments(
        CreateTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        for (var i = 0; i < operation.Columns.Count; i++)
        {
            var column = operation.Columns[i];

            if (i > 0)
            {
                builder.AppendLine();
            }

            if (!string.IsNullOrEmpty(column.Comment))
            {
                builder.AppendLines(Dependencies.SqlGenerationHelper.GenerateComment(column.Comment));
            }

            ColumnDefinition(column, model, builder);

            if (i != operation.Columns.Count - 1)
            {
                builder.AppendLine(",");
            }
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for a column definition for the given column metadata.
    /// </summary>
    /// <param name="schema">The schema that contains the table, or <see langword="null" /> to use the default schema.</param>
    /// <param name="table">The table that contains the column.</param>
    /// <param name="name">The column name.</param>
    /// <param name="operation">The column metadata.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected override void ColumnDefinition(
        string? schema,
        string table,
        string name,
        ColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        base.ColumnDefinition(schema, table, name, operation, model, builder);

        var inlinePk = operation[SqliteAnnotationNames.InlinePrimaryKey] as bool?;
        if (inlinePk == true)
        {
            var inlinePkName = operation[
                SqliteAnnotationNames.InlinePrimaryKeyName] as string;
            if (!string.IsNullOrEmpty(inlinePkName))
            {
                builder
                    .Append(" CONSTRAINT ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(inlinePkName));
            }

            builder.Append(" PRIMARY KEY");
            var autoincrement = operation[SqliteAnnotationNames.Autoincrement] as bool?
                // NB: Migrations scaffolded with version 1.0.0 don't have the prefix. See #6461
                ?? operation[SqliteAnnotationNames.LegacyAutoincrement] as bool?;
            if (autoincrement == true)
            {
                builder.Append(" AUTOINCREMENT");
            }
        }
    }

    #region Invalid migration operations

    /// <summary>
    ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
    ///     are not yet supported.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        AddForeignKeyOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
        => throw new NotSupportedException(
            SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

    /// <summary>
    ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
    ///     are not yet supported.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        AddPrimaryKeyOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
        => throw new NotSupportedException(
            SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

    /// <summary>
    ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
    ///     are not yet supported.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(AddUniqueConstraintOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => throw new NotSupportedException(
            SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

    /// <summary>
    ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
    ///     are not yet supported.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(AddCheckConstraintOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => throw new NotSupportedException(
            SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

    /// <summary>
    ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
    ///     are not yet supported.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        DropColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
        => throw new NotSupportedException(
            SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

    /// <summary>
    ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
    ///     are not yet supported.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        DropForeignKeyOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
        => throw new NotSupportedException(
            SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

    /// <summary>
    ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
    ///     are not yet supported.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        DropPrimaryKeyOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
        => throw new NotSupportedException(
            SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

    /// <summary>
    ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
    ///     are not yet supported.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(DropUniqueConstraintOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => throw new NotSupportedException(
            SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

    /// <summary>
    ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
    ///     are not yet supported.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(DropCheckConstraintOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => throw new NotSupportedException(
            SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

    /// <summary>
    ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
    ///     are not yet supported.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(AlterColumnOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => throw new NotSupportedException(
            SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

    /// <summary>
    ///     Generates a SQL fragment for a computed column definition for the given column metadata.
    /// </summary>
    /// <param name="schema">The schema that contains the table, or <see langword="null" /> to use the default schema.</param>
    /// <param name="table">The table that contains the column.</param>
    /// <param name="name">The column name.</param>
    /// <param name="operation">The column metadata.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected override void ComputedColumnDefinition(
        string? schema,
        string table,
        string name,
        ColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name));

        builder
            .Append(" AS (")
            .Append(operation.ComputedColumnSql!)
            .Append(")");

        if (operation.IsStored == true)
        {
            builder.Append(" STORED");
        }

        if (operation.Collation != null)
        {
            builder
                .Append(" COLLATE ")
                .Append(operation.Collation);
        }
    }

    #endregion

    #region Ignored schema operations

    /// <summary>
    ///     Ignored, since schemas are not supported by SQLite and are silently ignored to improve testing compatibility.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(EnsureSchemaOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
    }

    /// <summary>
    ///     Ignored, since schemas are not supported by SQLite and are silently ignored to improve testing compatibility.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(DropSchemaOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
    }

    #endregion

    #region Sequences not supported

    /// <summary>
    ///     Throws <see cref="NotSupportedException" /> since SQLite does not support sequences.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(RestartSequenceOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => throw new NotSupportedException(SqliteStrings.SequencesNotSupported);

    /// <summary>
    ///     Throws <see cref="NotSupportedException" /> since SQLite does not support sequences.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(CreateSequenceOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => throw new NotSupportedException(SqliteStrings.SequencesNotSupported);

    /// <summary>
    ///     Throws <see cref="NotSupportedException" /> since SQLite does not support sequences.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(RenameSequenceOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => throw new NotSupportedException(SqliteStrings.SequencesNotSupported);

    /// <summary>
    ///     Throws <see cref="NotSupportedException" /> since SQLite does not support sequences.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(AlterSequenceOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => throw new NotSupportedException(SqliteStrings.SequencesNotSupported);

    /// <summary>
    ///     Throws <see cref="NotSupportedException" /> since SQLite does not support sequences.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(DropSequenceOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => throw new NotSupportedException(SqliteStrings.SequencesNotSupported);

    #endregion

    private sealed class RebuildContext
    {
        public ICollection<MigrationOperation> OperationsToReplace { get; } = new List<MigrationOperation>();
        public IDictionary<string, AddColumnOperation> AddColumnsDeferred { get; } = new Dictionary<string, AddColumnOperation>();
        public ICollection<string> DropColumnsDeferred { get; } = new HashSet<string>();
        public readonly IDictionary<string, AlterColumnOperation> AlterColumnsDeferred = new Dictionary<string, AlterColumnOperation>();

        public readonly IDictionary<string, RenameColumnOperation> RenameColumnsDeferred =
            new Dictionary<string, RenameColumnOperation>();

        public ICollection<string> CreateIndexesDeferred { get; } = new HashSet<string>();
        public ICollection<MigrationOperation> OperationsToWarnFor { get; } = new List<MigrationOperation>();
    }
}
