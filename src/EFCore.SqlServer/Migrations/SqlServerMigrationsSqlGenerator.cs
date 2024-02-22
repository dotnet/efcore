// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     SQL Server-specific implementation of <see cref="MigrationsSqlGenerator" />.
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
///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class SqlServerMigrationsSqlGenerator : MigrationsSqlGenerator
{
    private IReadOnlyList<MigrationOperation> _operations = null!;
    private int _variableCounter;

    private readonly ICommandBatchPreparer _commandBatchPreparer;

    /// <summary>
    ///     Creates a new <see cref="SqlServerMigrationsSqlGenerator" /> instance.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    /// <param name="commandBatchPreparer">The command batch preparer.</param>
    public SqlServerMigrationsSqlGenerator(
        MigrationsSqlGeneratorDependencies dependencies,
        ICommandBatchPreparer commandBatchPreparer)
        : base(dependencies)
    {
        _commandBatchPreparer = commandBatchPreparer;
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
    {
        _operations = operations;
        try
        {
            return base.Generate(RewriteOperations(operations, model, options), model, options);
        }
        finally
        {
            _operations = null!;
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="MigrationOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <remarks>
    ///     This method uses a double-dispatch mechanism to call the <see cref="O:MigrationsSqlGenerator.Generate" /> method
    ///     that is specific to a certain subtype of <see cref="MigrationOperation" />. Typically database providers
    ///     will override these specific methods rather than this method. However, providers can override
    ///     this methods to handle provider-specific operations.
    /// </remarks>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        switch (operation)
        {
            case SqlServerCreateDatabaseOperation createDatabaseOperation:
                Generate(createDatabaseOperation, model, builder);
                break;
            case SqlServerDropDatabaseOperation dropDatabaseOperation:
                Generate(dropDatabaseOperation, model, builder);
                break;
            default:
                base.Generate(operation, model, builder);
                break;
        }
    }

    /// <inheritdoc />
    protected override void Generate(AddCheckConstraintOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => GenerateExecWhenIdempotent(builder, b => base.Generate(operation, model, b));

    /// <summary>
    ///     Builds commands for the given <see cref="AddColumnOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        AddColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate)
    {
        if (!terminate
            && operation.Comment != null)
        {
            throw new ArgumentException(SqlServerStrings.CannotProduceUnterminatedSQLWithComments(nameof(AddColumnOperation)));
        }

        if (IsIdentity(operation))
        {
            // NB: This gets added to all added non-nullable columns by MigrationsModelDiffer. We need to suppress
            //     it, here because SQL Server can't have both IDENTITY and a DEFAULT constraint on the same column.
            operation.DefaultValue = null;
        }

        var needsExec = Options.HasFlag(MigrationsSqlGenerationOptions.Idempotent)
            && operation.ComputedColumnSql != null;
        if (needsExec)
        {
            var subBuilder = new MigrationCommandListBuilder(Dependencies);
            base.Generate(operation, model, subBuilder, terminate: false);
            subBuilder.EndCommand();

            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));
            var command = subBuilder.GetCommandList().Single();

            builder
                .Append("EXEC(")
                .Append(stringTypeMapping.GenerateSqlLiteral(command.CommandText))
                .Append(")");
        }
        else
        {
            base.Generate(operation, model, builder, terminate: false);
        }

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            if (operation.Comment != null)
            {
                AddDescription(
                    builder, operation.Comment,
                    operation.Schema,
                    operation.Table,
                    operation.Name);
            }

            builder.EndCommand(suppressTransaction: IsMemoryOptimized(operation, model, operation.Schema, operation.Table));
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="AddForeignKeyOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
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
    {
        base.Generate(operation, model, builder, terminate: false);

        if (terminate)
        {
            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: IsMemoryOptimized(operation, model, operation.Schema, operation.Table));
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="AddPrimaryKeyOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
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
    {
        base.Generate(operation, model, builder, terminate: false);

        if (terminate)
        {
            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: IsMemoryOptimized(operation, model, operation.Schema, operation.Table));
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="AlterColumnOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(
        AlterColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (operation[RelationalAnnotationNames.ColumnOrder] != operation.OldColumn[RelationalAnnotationNames.ColumnOrder])
        {
            Dependencies.MigrationsLogger.ColumnOrderIgnoredWarning(operation);
        }

        IEnumerable<ITableIndex>? indexesToRebuild = null;
        var column = model?.GetRelationalModel().FindTable(operation.Table, operation.Schema)
            ?.Columns.FirstOrDefault(c => c.Name == operation.Name);

        if (operation.ComputedColumnSql != operation.OldColumn.ComputedColumnSql
            || operation.IsStored != operation.OldColumn.IsStored)
        {
            var dropColumnOperation = new DropColumnOperation
            {
                Schema = operation.Schema,
                Table = operation.Table,
                Name = operation.Name
            };
            if (column != null)
            {
                dropColumnOperation.AddAnnotations(column.GetAnnotations());
            }

            var addColumnOperation = new AddColumnOperation
            {
                Schema = operation.Schema,
                Table = operation.Table,
                Name = operation.Name,
                ClrType = operation.ClrType,
                ColumnType = operation.ColumnType,
                IsUnicode = operation.IsUnicode,
                IsFixedLength = operation.IsFixedLength,
                MaxLength = operation.MaxLength,
                Precision = operation.Precision,
                Scale = operation.Scale,
                IsRowVersion = operation.IsRowVersion,
                IsNullable = operation.IsNullable,
                DefaultValue = operation.DefaultValue,
                DefaultValueSql = operation.DefaultValueSql,
                ComputedColumnSql = operation.ComputedColumnSql,
                IsStored = operation.IsStored,
                Comment = operation.Comment,
                Collation = operation.Collation
            };
            addColumnOperation.AddAnnotations(operation.GetAnnotations());

            // TODO: Use a column rebuild instead
            indexesToRebuild = GetIndexesToRebuild(column, operation).ToList();
            DropIndexes(indexesToRebuild, builder);
            Generate(dropColumnOperation, model, builder, terminate: false);
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            Generate(addColumnOperation, model, builder);
            CreateIndexes(indexesToRebuild, builder);
            builder.EndCommand(suppressTransaction: IsMemoryOptimized(operation, model, operation.Schema, operation.Table));

            return;
        }

        var columnType = operation.ColumnType
            ?? GetColumnType(
                operation.Schema,
                operation.Table,
                operation.Name,
                operation,
                model);

        var narrowed = false;
        var oldColumnSupported = IsOldColumnSupported(model);
        if (oldColumnSupported)
        {
            if (IsIdentity(operation) != IsIdentity(operation.OldColumn))
            {
                throw new InvalidOperationException(SqlServerStrings.AlterIdentityColumn);
            }

            var oldType = operation.OldColumn.ColumnType
                ?? GetColumnType(
                    operation.Schema,
                    operation.Table,
                    operation.Name,
                    operation.OldColumn,
                    model);
            narrowed = columnType != oldType
                || operation.Collation != operation.OldColumn.Collation
                || operation is { IsNullable: false, OldColumn.IsNullable: true };
        }

        if (narrowed)
        {
            indexesToRebuild = GetIndexesToRebuild(column, operation).ToList();
            DropIndexes(indexesToRebuild, builder);
        }

        // Handle change of identity seed value
        if (IsIdentity(operation) && oldColumnSupported)
        {
            Check.DebugAssert(IsIdentity(operation.OldColumn), "Unsupported column change to identity");

            var oldSeed = 1;
            if (TryParseIdentitySeedIncrement(operation, out var newSeed, out _)
                && (operation.OldColumn[SqlServerAnnotationNames.Identity] is null
                    || TryParseIdentitySeedIncrement(operation.OldColumn, out oldSeed, out _))
                && newSeed != oldSeed)
            {
                var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));
                var table = stringTypeMapping.GenerateSqlLiteral(
                    Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));

                builder
                    .Append($"DBCC CHECKIDENT({table}, RESEED, {newSeed})")
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }
        }

        var newAnnotations = operation.GetAnnotations().Where(a => a.Name != SqlServerAnnotationNames.Identity);
        var oldAnnotations = operation.OldColumn.GetAnnotations().Where(a => a.Name != SqlServerAnnotationNames.Identity);

        var alterStatementNeeded = narrowed
            || !oldColumnSupported
            || operation.ClrType != operation.OldColumn.ClrType
            || columnType != operation.OldColumn.ColumnType
            || operation.IsUnicode != operation.OldColumn.IsUnicode
            || operation.IsFixedLength != operation.OldColumn.IsFixedLength
            || operation.MaxLength != operation.OldColumn.MaxLength
            || operation.Precision != operation.OldColumn.Precision
            || operation.Scale != operation.OldColumn.Scale
            || operation.IsRowVersion != operation.OldColumn.IsRowVersion
            || operation.IsNullable != operation.OldColumn.IsNullable
            || operation.Collation != operation.OldColumn.Collation
            || HasDifferences(newAnnotations, oldAnnotations);

        var (oldDefaultValue, oldDefaultValueSql) = (operation.OldColumn.DefaultValue, operation.OldColumn.DefaultValueSql);

        if (alterStatementNeeded
            || !Equals(operation.DefaultValue, oldDefaultValue)
            || operation.DefaultValueSql != oldDefaultValueSql)
        {
            DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);
            (oldDefaultValue, oldDefaultValueSql) = (null, null);
        }

        // The column is being made non-nullable. Generate an update statement before doing that, to convert any existing null values to
        // the default value (otherwise SQL Server fails).
        if (operation is { IsNullable: false, OldColumn.IsNullable: true }
            && (operation.DefaultValueSql is not null || operation.DefaultValue is not null))
        {
            string defaultValueSql;
            if (operation.DefaultValueSql is not null)
            {
                defaultValueSql = operation.DefaultValueSql;
            }
            else
            {
                Check.DebugAssert(operation.DefaultValue is not null, "operation.DefaultValue is not null");

                var typeMapping = (columnType != null
                        ? Dependencies.TypeMappingSource.FindMapping(operation.DefaultValue.GetType(), columnType)
                        : null)
                    ?? Dependencies.TypeMappingSource.GetMappingForValue(operation.DefaultValue);

                defaultValueSql = typeMapping.GenerateSqlLiteral(operation.DefaultValue);
            }

            var updateBuilder = new StringBuilder()
                .Append("UPDATE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" SET ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" = ")
                .Append(defaultValueSql)
                .Append(" WHERE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" IS NULL");

            if (Options.HasFlag(MigrationsSqlGenerationOptions.Idempotent))
            {
                builder
                    .Append("EXEC(N'")
                    .Append(updateBuilder.ToString().TrimEnd('\n', '\r', ';').Replace("'", "''"))
                    .Append("')");
            }
            else
            {
                builder.Append(updateBuilder.ToString());
            }

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }

        if (alterStatementNeeded)
        {
            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ALTER COLUMN ");

            // NB: ComputedColumnSql, IsStored, DefaultValue, DefaultValueSql, Comment, ValueGenerationStrategy, and Identity are
            //     handled elsewhere. Don't copy them here.
            var definitionOperation = new AlterColumnOperation
            {
                Schema = operation.Schema,
                Table = operation.Table,
                Name = operation.Name,
                ClrType = operation.ClrType,
                ColumnType = operation.ColumnType,
                IsUnicode = operation.IsUnicode,
                IsFixedLength = operation.IsFixedLength,
                MaxLength = operation.MaxLength,
                Precision = operation.Precision,
                Scale = operation.Scale,
                IsRowVersion = operation.IsRowVersion,
                IsNullable = operation.IsNullable,
                Collation = operation.Collation,
                OldColumn = operation.OldColumn
            };
            definitionOperation.AddAnnotations(
                operation.GetAnnotations().Where(
                    a => a.Name != SqlServerAnnotationNames.ValueGenerationStrategy
                        && a.Name != SqlServerAnnotationNames.Identity));

            ColumnDefinition(
                operation.Schema,
                operation.Table,
                operation.Name,
                definitionOperation,
                model,
                builder);

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }

        if (!Equals(operation.DefaultValue, oldDefaultValue) || operation.DefaultValueSql != oldDefaultValueSql)
        {
            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ADD");
            DefaultValue(operation.DefaultValue, operation.DefaultValueSql, operation.ColumnType, builder);
            builder
                .Append(" FOR ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }

        if (operation.OldColumn.Comment != operation.Comment)
        {
            var dropDescription = operation.OldColumn.Comment != null;
            if (dropDescription)
            {
                DropDescription(
                    builder,
                    operation.Schema,
                    operation.Table,
                    operation.Name);
            }

            if (operation.Comment != null)
            {
                AddDescription(
                    builder, operation.Comment,
                    operation.Schema,
                    operation.Table,
                    operation.Name,
                    omitVariableDeclarations: dropDescription);
            }
        }

        if (narrowed)
        {
            CreateIndexes(indexesToRebuild!, builder);
        }

        builder.EndCommand(suppressTransaction: IsMemoryOptimized(operation, model, operation.Schema, operation.Table));
    }

    /// <summary>
    ///     Builds commands for the given <see cref="RenameIndexOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(
        RenameIndexOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (string.IsNullOrEmpty(operation.Table))
        {
            throw new InvalidOperationException(SqlServerStrings.IndexTableRequired);
        }

        Rename(
            Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema)
            + "."
            + Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name),
            operation.NewName,
            "INDEX",
            builder);
        builder.EndCommand(suppressTransaction: IsMemoryOptimized(operation, model, operation.Schema, operation.Table));
    }

    /// <summary>
    ///     Builds commands for the given <see cref="RenameSequenceOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(RenameSequenceOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        var name = operation.Name;
        if (operation.NewName != null
            && operation.NewName != name)
        {
            Rename(
                Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema),
                operation.NewName,
                "OBJECT",
                builder);

            name = operation.NewName;
        }

        if (operation.NewSchema != operation.Schema
            && (operation.NewSchema != null
                || !HasLegacyRenameOperations(model)))
        {
            Transfer(operation.NewSchema, operation.Schema, name, builder);
        }

        builder.EndCommand();
    }

    /// <summary>
    ///     Builds commands for the given <see cref="RestartSequenceOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(
        RestartSequenceOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append("ALTER SEQUENCE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
            .Append(" RESTART");

        if (operation.StartValue.HasValue)
        {
            builder
                .Append(" WITH ")
                .Append(IntegerConstant(operation.StartValue.Value));
        }

        builder
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        EndStatement(builder);
    }

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
        var hasComments = operation.Comment != null || operation.Columns.Any(c => c.Comment != null);

        if (!terminate && hasComments)
        {
            throw new ArgumentException(SqlServerStrings.CannotProduceUnterminatedSQLWithComments(nameof(CreateTableOperation)));
        }

        var needsExec = false;

        var tableCreationOptions = new List<string>();

        if (operation[SqlServerAnnotationNames.IsTemporal] as bool? == true)
        {
            var historyTableSchema = operation[SqlServerAnnotationNames.TemporalHistoryTableSchema] as string
                ?? model?.GetDefaultSchema();

            needsExec = historyTableSchema == null;
            var subBuilder = needsExec
                ? new MigrationCommandListBuilder(Dependencies)
                : builder;

            subBuilder
                .Append("CREATE TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .AppendLine(" (");

            using (subBuilder.Indent())
            {
                CreateTableColumns(operation, model, subBuilder);
                CreateTableConstraints(operation, model, subBuilder);
                subBuilder.AppendLine(",");
                var startColumnName = operation[SqlServerAnnotationNames.TemporalPeriodStartColumnName] as string;
                var endColumnName = operation[SqlServerAnnotationNames.TemporalPeriodEndColumnName] as string;
                var start = Dependencies.SqlGenerationHelper.DelimitIdentifier(startColumnName!);
                var end = Dependencies.SqlGenerationHelper.DelimitIdentifier(endColumnName!);
                subBuilder.AppendLine($"PERIOD FOR SYSTEM_TIME({start}, {end})");
            }

            subBuilder.Append(")");

            if (needsExec)
            {
                subBuilder
                    .EndCommand();

                var execBody = subBuilder.GetCommandList().Single().CommandText.Replace("'", "''");

                builder
                    .AppendLine("DECLARE @historyTableSchema sysname = SCHEMA_NAME()")
                    .Append("EXEC(N'")
                    .Append(execBody);
            }

            var historyTableName = operation[SqlServerAnnotationNames.TemporalHistoryTableName] as string;
            string historyTable;
            if (needsExec)
            {
                historyTable = Dependencies.SqlGenerationHelper.DelimitIdentifier(historyTableName!);
                tableCreationOptions.Add($"SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].{historyTable})");
            }
            else
            {
                historyTable = Dependencies.SqlGenerationHelper.DelimitIdentifier(historyTableName!, historyTableSchema);
                tableCreationOptions.Add($"SYSTEM_VERSIONING = ON (HISTORY_TABLE = {historyTable})");
            }
        }
        else
        {
            base.Generate(operation, model, builder, terminate: false);
        }

        var memoryOptimized = IsMemoryOptimized(operation);
        if (memoryOptimized)
        {
            tableCreationOptions.Add("MEMORY_OPTIMIZED = ON");
        }

        if (tableCreationOptions.Count > 0)
        {
            builder.Append(" WITH (");
            if (tableCreationOptions.Count == 1)
            {
                builder
                    .Append(tableCreationOptions[0])
                    .Append(")");
            }
            else
            {
                builder.AppendLine();

                using (builder.Indent())
                {
                    for (var i = 0; i < tableCreationOptions.Count; i++)
                    {
                        builder.Append(tableCreationOptions[i]);

                        if (i < tableCreationOptions.Count - 1)
                        {
                            builder.Append(",");
                        }

                        builder.AppendLine();
                    }
                }

                builder.Append(")");
            }
        }

        if (needsExec)
        {
            builder.Append("')");
        }

        if (hasComments)
        {
            Check.DebugAssert(terminate, "terminate is false but there are comments");

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            var firstDescription = true;
            if (operation.Comment != null)
            {
                AddDescription(builder, operation.Comment, operation.Schema, operation.Name);

                firstDescription = false;
            }

            foreach (var column in operation.Columns)
            {
                if (column.Comment == null)
                {
                    continue;
                }

                AddDescription(
                    builder, column.Comment,
                    operation.Schema,
                    operation.Name,
                    column.Name,
                    omitVariableDeclarations: !firstDescription);

                firstDescription = false;
            }

            builder.EndCommand(suppressTransaction: memoryOptimized);
        }
        else if (terminate)
        {
            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: memoryOptimized);
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="RenameTableOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(
        RenameTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        var name = operation.Name;
        if (operation.NewName != null
            && operation.NewName != name)
        {
            Rename(
                Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema),
                operation.NewName,
                "OBJECT",
                builder);

            name = operation.NewName;
        }

        if (operation.NewSchema != operation.Schema
            && (operation.NewSchema != null
                || !HasLegacyRenameOperations(model)))
        {
            Transfer(operation.NewSchema, operation.Schema, name, builder);
        }

        builder.EndCommand();
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DropTableOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        DropTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        base.Generate(operation, model, builder, terminate: false);

        if (terminate)
        {
            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: IsMemoryOptimized(operation, model, operation.Schema, operation.Name));
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="CreateIndexOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        CreateIndexOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        var table = model?.GetRelationalModel().FindTable(operation.Table, operation.Schema);
        var hasNullableColumns = operation.Columns.Any(c => table?.FindColumn(c)?.IsNullable != false);

        var memoryOptimized = IsMemoryOptimized(operation, model, operation.Schema, operation.Table);
        if (memoryOptimized)
        {
            builder.Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ADD INDEX ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" ");

            if (operation.IsUnique && !hasNullableColumns)
            {
                builder.Append("UNIQUE ");
            }

            IndexTraits(operation, model, builder);

            builder.Append("(");
            GenerateIndexColumnList(operation, model, builder);
            builder.Append(")");
        }
        else
        {
            var needsLegacyFilter = UseLegacyIndexFilters(operation, model);
            var needsExec = Options.HasFlag(MigrationsSqlGenerationOptions.Idempotent)
                && (operation.Filter != null
                    || needsLegacyFilter);
            var subBuilder = needsExec
                ? new MigrationCommandListBuilder(Dependencies)
                : builder;

            base.Generate(operation, model, subBuilder, terminate: false);

            if (needsExec)
            {
                subBuilder
                    .EndCommand();

                var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));
                var command = subBuilder.GetCommandList().Single();

                builder
                    .Append("EXEC(")
                    .Append(stringTypeMapping.GenerateSqlLiteral(command.CommandText))
                    .Append(")");
            }
        }

        if (terminate)
        {
            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: memoryOptimized);
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DropPrimaryKeyOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
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
    {
        base.Generate(operation, model, builder, terminate: false);
        if (terminate)
        {
            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: IsMemoryOptimized(operation, model, operation.Schema, operation.Table));
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="EnsureSchemaOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(EnsureSchemaOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        if (string.Equals(operation.Name, "dbo", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

        builder
            .Append("IF SCHEMA_ID(")
            .Append(stringTypeMapping.GenerateSqlLiteral(operation.Name))
            .Append(") IS NULL EXEC(")
            .Append(
                stringTypeMapping.GenerateSqlLiteral(
                    "CREATE SCHEMA "
                    + Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name)
                    + Dependencies.SqlGenerationHelper.StatementTerminator))
            .Append(")")
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
            .EndCommand();
    }

    /// <summary>
    ///     Builds commands for the given <see cref="CreateSequenceOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(
        CreateSequenceOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append("CREATE SEQUENCE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

        if (operation.ClrType != typeof(long))
        {
            var typeMapping = Dependencies.TypeMappingSource.GetMapping(operation.ClrType);

            builder
                .Append(" AS ")
                .Append(typeMapping.StoreTypeNameBase);
        }

        builder
            .Append(" START WITH ")
            .Append(IntegerConstant(operation.StartValue));

        SequenceOptions(operation, model, builder);

        builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        EndStatement(builder);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="SqlServerCreateDatabaseOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        SqlServerCreateDatabaseOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append("CREATE DATABASE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

        if (!string.IsNullOrEmpty(operation.FileName))
        {
            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            var fileName = ExpandFileName(operation.FileName);
            var name = Path.GetFileNameWithoutExtension(fileName);

            var logFileName = Path.ChangeExtension(fileName, ".ldf");
            var logName = name + "_log";

            // Match default naming behavior of SQL Server
            logFileName = logFileName.Insert(logFileName.Length - ".ldf".Length, "_log");

            builder
                .AppendLine()
                .Append("ON (NAME = ")
                .Append(stringTypeMapping.GenerateSqlLiteral(name))
                .Append(", FILENAME = ")
                .Append(stringTypeMapping.GenerateSqlLiteral(fileName))
                .Append(")")
                .AppendLine()
                .Append("LOG ON (NAME = ")
                .Append(stringTypeMapping.GenerateSqlLiteral(logName))
                .Append(", FILENAME = ")
                .Append(stringTypeMapping.GenerateSqlLiteral(logFileName))
                .Append(")");
        }

        if (!string.IsNullOrEmpty(operation.Collation))
        {
            builder
                .AppendLine()
                .Append("COLLATE ")
                .Append(operation.Collation);
        }

        builder
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
            .EndCommand(suppressTransaction: true)
            .AppendLine("IF SERVERPROPERTY('EngineEdition') <> 5")
            .AppendLine("BEGIN");

        using (builder.Indent())
        {
            builder
                .Append("ALTER DATABASE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" SET READ_COMMITTED_SNAPSHOT ON")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }

        builder
            .Append("END")
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
            .EndCommand(suppressTransaction: true);
    }

    private static string ExpandFileName(string fileName)
    {
        if (fileName.StartsWith("|DataDirectory|", StringComparison.OrdinalIgnoreCase))
        {
            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (string.IsNullOrEmpty(dataDirectory))
            {
                dataDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }

            fileName = Path.Combine(dataDirectory, fileName["|DataDirectory|".Length..]);
        }

        return Path.GetFullPath(fileName);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="SqlServerDropDatabaseOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        SqlServerDropDatabaseOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .AppendLine("IF SERVERPROPERTY('EngineEdition') <> 5")
            .AppendLine("BEGIN");

        using (builder.Indent())
        {
            builder
                .Append("ALTER DATABASE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" SET SINGLE_USER WITH ROLLBACK IMMEDIATE")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }

        builder
            .Append("END")
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
            .EndCommand(suppressTransaction: true)
            .Append("DROP DATABASE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
            .EndCommand(suppressTransaction: true);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="AlterDatabaseOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(
        AlterDatabaseOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (operation[SqlServerAnnotationNames.EditionOptions] is string editionOptions)
        {
            builder
                .AppendLine("BEGIN")
                .AppendLine("DECLARE @db_name nvarchar(max) = DB_NAME();")
                .AppendLine("EXEC(N'ALTER DATABASE [' + @db_name + '] MODIFY ( ")
                .Append(editionOptions.Replace("'", "''"))
                .AppendLine(" );');")
                .AppendLine("END")
                .AppendLine();
        }

        if (operation.Collation != operation.OldDatabase.Collation)
        {
            builder
                .AppendLine("BEGIN")
                .AppendLine("DECLARE @db_name nvarchar(max) = DB_NAME();");

            if (operation.Collation == null)
            {
                builder.AppendLine("DECLARE @defaultCollation nvarchar(max) = CAST(SERVERPROPERTY('Collation') AS nvarchar(max));");
            }

            builder
                .Append("EXEC(N'ALTER DATABASE [' + @db_name + '] COLLATE ")
                .Append(operation.Collation ?? "' + @defaultCollation + N'")
                .AppendLine(";');")
                .AppendLine("END")
                .AppendLine();
        }

        if (!IsMemoryOptimized(operation))
        {
            builder.EndCommand(suppressTransaction: true);
            return;
        }

        builder.AppendLine("IF SERVERPROPERTY('IsXTPSupported') = 1 AND SERVERPROPERTY('EngineEdition') <> 5");
        using (builder.Indent())
        {
            builder
                .AppendLine("BEGIN")
                .AppendLine("IF NOT EXISTS (");
            using (builder.Indent())
            {
                builder
                    .Append("SELECT 1 FROM [sys].[filegroups] [FG] ")
                    .Append("JOIN [sys].[database_files] [F] ON [FG].[data_space_id] = [F].[data_space_id] ")
                    .AppendLine("WHERE [FG].[type] = N'FX' AND [F].[type] = 2)");
            }

            using (builder.Indent())
            {
                builder
                    .AppendLine("BEGIN")
                    .AppendLine("ALTER DATABASE CURRENT SET AUTO_CLOSE OFF;")
                    .AppendLine("DECLARE @db_name nvarchar(max) = DB_NAME();")
                    .AppendLine("DECLARE @fg_name nvarchar(max);")
                    .AppendLine("SELECT TOP(1) @fg_name = [name] FROM [sys].[filegroups] WHERE [type] = N'FX';")
                    .AppendLine()
                    .AppendLine("IF @fg_name IS NULL");

                using (builder.Indent())
                {
                    builder
                        .AppendLine("BEGIN")
                        .AppendLine("SET @fg_name = @db_name + N'_MODFG';")
                        .AppendLine("EXEC(N'ALTER DATABASE CURRENT ADD FILEGROUP [' + @fg_name + '] CONTAINS MEMORY_OPTIMIZED_DATA;');")
                        .AppendLine("END");
                }

                builder
                    .AppendLine()
                    .AppendLine("DECLARE @path nvarchar(max);")
                    .Append("SELECT TOP(1) @path = [physical_name] FROM [sys].[database_files] ")
                    .AppendLine("WHERE charindex('\\', [physical_name]) > 0 ORDER BY [file_id];")
                    .AppendLine("IF (@path IS NULL)")
                    .IncrementIndent().AppendLine("SET @path = '\\' + @db_name;").DecrementIndent()
                    .AppendLine()
                    .AppendLine("DECLARE @filename nvarchar(max) = right(@path, charindex('\\', reverse(@path)) - 1);")
                    .AppendLine(
                        "SET @filename = REPLACE(left(@filename, len(@filename) - charindex('.', reverse(@filename))), '''', '''''') + N'_MOD';")
                    .AppendLine(
                        "DECLARE @new_path nvarchar(max) = REPLACE(CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(max)), '''', '''''') + @filename;")
                    .AppendLine()
                    .AppendLine("EXEC(N'");

                using (builder.Indent())
                {
                    builder
                        .AppendLine("ALTER DATABASE CURRENT")
                        .AppendLine("ADD FILE (NAME=''' + @filename + ''', filename=''' + @new_path + ''')")
                        .AppendLine("TO FILEGROUP [' + @fg_name + '];')");
                }

                builder.AppendLine("END");
            }

            builder.AppendLine("END");
        }

        builder.AppendLine()
            .AppendLine("IF SERVERPROPERTY('IsXTPSupported') = 1")
            .AppendLine("EXEC(N'");
        using (builder.Indent())
        {
            builder
                .AppendLine("ALTER DATABASE CURRENT")
                .AppendLine("SET MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT ON;')");
        }

        builder.EndCommand(suppressTransaction: true);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="AlterTableOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(AlterTableOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        if (IsMemoryOptimized(operation)
            ^ IsMemoryOptimized(operation.OldTable))
        {
            throw new InvalidOperationException(SqlServerStrings.AlterMemoryOptimizedTable);
        }

        if (operation.OldTable.Comment != operation.Comment)
        {
            var dropDescription = operation.OldTable.Comment != null;
            if (dropDescription)
            {
                DropDescription(builder, operation.Schema, operation.Name);
            }

            if (operation.Comment != null)
            {
                AddDescription(
                    builder,
                    operation.Comment,
                    operation.Schema,
                    operation.Name,
                    omitVariableDeclarations: dropDescription);
            }
        }

        builder.EndCommand(suppressTransaction: IsMemoryOptimized(operation, model, operation.Schema, operation.Name));
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DropForeignKeyOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
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
    {
        base.Generate(operation, model, builder, terminate: false);

        if (terminate)
        {
            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: IsMemoryOptimized(operation, model, operation.Schema, operation.Table));
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
        if (string.IsNullOrEmpty(operation.Table))
        {
            throw new InvalidOperationException(SqlServerStrings.IndexTableRequired);
        }

        var memoryOptimized = IsMemoryOptimized(operation, model, operation.Schema, operation.Table);
        if (memoryOptimized)
        {
            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table!, operation.Schema))
                .Append(" DROP INDEX ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));
        }
        else
        {
            builder
                .Append("DROP INDEX ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" ON ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));
        }

        if (terminate)
        {
            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: memoryOptimized);
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DropColumnOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
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
    {
        DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);
        base.Generate(operation, model, builder, terminate: false);

        if (terminate)
        {
            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: IsMemoryOptimized(operation, model, operation.Schema, operation.Table));
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="RenameColumnOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(
        RenameColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        Rename(
            Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema)
            + "."
            + Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name),
            operation.NewName,
            "COLUMN",
            builder);
        builder.EndCommand();
    }

    /// <summary>
    ///     Builds commands for the given <see cref="SqlOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(SqlOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        var preBatched = operation.Sql
            .Replace("\\\n", "")
            .Replace("\\\r\n", "")
            .Split(["\r\n", "\n"], StringSplitOptions.None);

        var batchBuilder = new StringBuilder();
        foreach (var line in preBatched)
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("GO", StringComparison.OrdinalIgnoreCase)
                && (trimmed.Length == 2
                    || char.IsWhiteSpace(trimmed[2])))
            {
                var batch = batchBuilder.ToString();
                batchBuilder.Clear();

                var count = trimmed.Length >= 4
                    && int.TryParse(trimmed.Substring(3), out var specifiedCount)
                        ? specifiedCount
                        : 1;

                for (var j = 0; j < count; j++)
                {
                    AppendBatch(batch);
                }
            }
            else
            {
                batchBuilder.AppendLine(line);
            }
        }

        AppendBatch(batchBuilder.ToString());

        void AppendBatch(string batch)
        {
            builder.Append(batch);
            EndStatement(builder, operation.SuppressTransaction);
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="InsertDataOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        InsertDataOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        GenerateIdentityInsert(builder, operation, on: true, model);

        var sqlBuilder = new StringBuilder();

        var modificationCommands = GenerateModificationCommands(operation, model).ToList();
        var updateSqlGenerator = (ISqlServerUpdateSqlGenerator)Dependencies.UpdateSqlGenerator;

        foreach (var batch in _commandBatchPreparer.CreateCommandBatches(modificationCommands, moreCommandSets: true))
        {
            updateSqlGenerator.AppendBulkInsertOperation(sqlBuilder, batch.ModificationCommands, commandPosition: 0);
        }

        if (Options.HasFlag(MigrationsSqlGenerationOptions.Idempotent))
        {
            builder
                .Append("EXEC(N'")
                .Append(sqlBuilder.ToString().TrimEnd('\n', '\r', ';').Replace("'", "''"))
                .Append("')")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }
        else
        {
            builder.Append(sqlBuilder.ToString());
        }

        GenerateIdentityInsert(builder, operation, on: false, model);

        if (terminate)
        {
            builder.EndCommand();
        }
    }

    private void GenerateIdentityInsert(MigrationCommandListBuilder builder, InsertDataOperation operation, bool on, IModel? model)
    {
        var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

        builder
            .Append("IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE")
            .Append(" [name] IN (")
            .Append(string.Join(", ", operation.Columns.Select(stringTypeMapping.GenerateSqlLiteral)))
            .Append(") AND [object_id] = OBJECT_ID(")
            .Append(
                stringTypeMapping.GenerateSqlLiteral(
                    Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema ?? model?.GetDefaultSchema())))
            .AppendLine("))");

        using (builder.Indent())
        {
            builder
                .Append("SET IDENTITY_INSERT ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema ?? model?.GetDefaultSchema()))
                .Append(on ? " ON" : " OFF")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }
    }

    /// <inheritdoc />
    protected override void Generate(DeleteDataOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => GenerateExecWhenIdempotent(builder, b => base.Generate(operation, model, b));

    /// <inheritdoc />
    protected override void Generate(UpdateDataOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => GenerateExecWhenIdempotent(builder, b => base.Generate(operation, model, b));

    /// <inheritdoc />
    protected override void SequenceOptions(
        string? schema,
        string name,
        SequenceOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool forAlter)
    {
        builder
            .Append(" INCREMENT BY ")
            .Append(IntegerConstant(operation.IncrementBy));

        if (operation.MinValue.HasValue)
        {
            builder
                .Append(" MINVALUE ")
                .Append(IntegerConstant(operation.MinValue.Value));
        }
        else if (forAlter)
        {
            builder.Append(" NO MINVALUE");
        }

        if (operation.MaxValue.HasValue)
        {
            builder
                .Append(" MAXVALUE ")
                .Append(IntegerConstant(operation.MaxValue.Value));
        }
        else if (forAlter)
        {
            builder.Append(" NO MAXVALUE");
        }

        builder.Append(operation.IsCyclic ? " CYCLE" : " NO CYCLE");

        if (!operation.IsCached)
        {
            builder.Append(" NO CACHE");
        }
        else if (operation.CacheSize.HasValue)
        {
            builder
            .Append(" CACHE ")
                .Append(IntegerConstant(operation.CacheSize.Value));
        }
        else if (forAlter)
        {
            builder
                .Append(" CACHE");
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
        if (operation.ComputedColumnSql != null)
        {
            ComputedColumnDefinition(schema, table, name, operation, model, builder);

            return;
        }

        var columnType = operation.ColumnType ?? GetColumnType(schema, table, name, operation, model)!;
        builder
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name))
            .Append(" ")
            .Append(columnType);

        if (operation.Collation != null)
        {
            builder
                .Append(" COLLATE ")
                .Append(operation.Collation);
        }

        if (operation[SqlServerAnnotationNames.Sparse] is bool isSparse && isSparse)
        {
            builder.Append(" SPARSE");
        }

        var isPeriodStartColumn = operation[SqlServerAnnotationNames.TemporalIsPeriodStartColumn] as bool? == true;
        var isPeriodEndColumn = operation[SqlServerAnnotationNames.TemporalIsPeriodEndColumn] as bool? == true;

        // falling back to legacy annotations, in case the migration was generated using pre-9.0 bits
        if (!isPeriodStartColumn && !isPeriodEndColumn)
        {
            if (operation[SqlServerAnnotationNames.TemporalPeriodStartColumnName] is string periodStartColumnName
                && operation[SqlServerAnnotationNames.TemporalPeriodEndColumnName] is string periodEndColumnName)
            {
                isPeriodStartColumn = operation.Name == periodStartColumnName;
                isPeriodEndColumn = operation.Name == periodEndColumnName;
            }
        }

        if (isPeriodStartColumn || isPeriodEndColumn)
        {
            builder.Append(" GENERATED ALWAYS AS ROW ");
            builder.Append(isPeriodStartColumn ? "START" : "END");
            builder.Append(" HIDDEN");
        }

        builder.Append(operation.IsNullable ? " NULL" : " NOT NULL");

        if (!string.Equals(columnType, "rowversion", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(columnType, "timestamp", StringComparison.OrdinalIgnoreCase))
        {
            // rowversion/timestamp columns cannot have default values, but also don't need them when adding a new column.
            DefaultValue(operation.DefaultValue, operation.DefaultValueSql, columnType, builder);
        }

        var identity = operation[SqlServerAnnotationNames.Identity] as string;
        if (identity != null
            || operation[SqlServerAnnotationNames.ValueGenerationStrategy] as SqlServerValueGenerationStrategy?
            == SqlServerValueGenerationStrategy.IdentityColumn)
        {
            builder.Append(" IDENTITY");

            if (!string.IsNullOrEmpty(identity)
                && identity != "1, 1")
            {
                builder
                    .Append("(")
                    .Append(identity)
                    .Append(")");
            }
        }
    }

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
            .Append(" AS ")
            .Append(operation.ComputedColumnSql!);

        if (operation.Collation != null)
        {
            builder
                .Append(" COLLATE ")
                .Append(operation.Collation);
        }

        if (operation.IsStored == true)
        {
            builder.Append(" PERSISTED");
        }
    }

    /// <summary>
    ///     Generates a rename.
    /// </summary>
    /// <param name="name">The old name.</param>
    /// <param name="newName">The new name.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Rename(
        string name,
        string newName,
        MigrationCommandListBuilder builder)
        => Rename(name, newName, /*type:*/ null, builder);

    /// <summary>
    ///     Generates a rename.
    /// </summary>
    /// <param name="name">The old name.</param>
    /// <param name="newName">The new name.</param>
    /// <param name="type">If not <see langword="null" />, then appends literal for type of object being renamed (e.g. column or index.)</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Rename(
        string name,
        string newName,
        string? type,
        MigrationCommandListBuilder builder)
    {
        // Types come from https://learn.microsoft.com/sql/relational-databases/system-stored-procedures/sp-rename-transact-sql
        var typeMappingSource = Dependencies.TypeMappingSource;
        var nameTypeMapping = typeMappingSource.FindMapping(typeof(string), "nvarchar(776)")!;

        builder
            .Append("EXEC sp_rename ")
            .Append(nameTypeMapping.GenerateSqlLiteral(name))
            .Append(", ")
            .Append(nameTypeMapping.GenerateSqlLiteral(newName));

        if (type != null)
        {
            builder
                .Append(", ")
                .Append(typeMappingSource.FindMapping(typeof(string), "varchar(13)")!.GenerateSqlLiteral(type));
        }

        builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
    }

    /// <summary>
    ///     Generates a transfer from one schema to another..
    /// </summary>
    /// <param name="newSchema">The schema to transfer to.</param>
    /// <param name="schema">The schema to transfer from.</param>
    /// <param name="name">The name of the item to transfer.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Transfer(
        string? newSchema,
        string? schema,
        string name,
        MigrationCommandListBuilder builder)
    {
        if (newSchema == null)
        {
            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            builder
                .AppendLine("DECLARE @defaultSchema sysname = SCHEMA_NAME();")
                .Append("EXEC(")
                .Append("N'ALTER SCHEMA [' + @defaultSchema + ")
                .Append(
                    stringTypeMapping.GenerateSqlLiteral(
                        "] TRANSFER " + Dependencies.SqlGenerationHelper.DelimitIdentifier(name, schema) + ";"))
                .AppendLine(");");
        }
        else
        {
            builder
                .Append("ALTER SCHEMA ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(newSchema))
                .Append(" TRANSFER ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name, schema))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for extra with options of a key from a
    ///     <see cref="AddPrimaryKeyOperation" />, or <see cref="AddUniqueConstraintOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected override void KeyWithOptions(MigrationOperation operation, MigrationCommandListBuilder builder)
    {
        var options = new List<string>();

        if (operation[SqlServerAnnotationNames.FillFactor] is int fillFactor)
        {
            options.Add("FILLFACTOR = " + fillFactor);
        }

        if (options.Count > 0)
        {
            builder
                .Append(" WITH (")
                .Append(string.Join(", ", options))
                .Append(")");
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for traits of an index from a <see cref="CreateIndexOperation" />,
    ///     <see cref="AddPrimaryKeyOperation" />, or <see cref="AddUniqueConstraintOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected override void IndexTraits(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        if (operation[SqlServerAnnotationNames.Clustered] is bool clustered)
        {
            builder.Append(clustered ? "CLUSTERED " : "NONCLUSTERED ");
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for extras (filter, included columns, options) of an index from a <see cref="CreateIndexOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected override void IndexOptions(CreateIndexOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        if (operation[SqlServerAnnotationNames.Include] is IReadOnlyList<string> includeColumns
            && includeColumns.Count > 0)
        {
            builder.Append(" INCLUDE (");
            for (var i = 0; i < includeColumns.Count; i++)
            {
                builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(includeColumns[i]));

                if (i != includeColumns.Count - 1)
                {
                    builder.Append(", ");
                }
            }

            builder.Append(")");
        }

        if (!string.IsNullOrEmpty(operation.Filter))
        {
            builder
                .Append(" WHERE ")
                .Append(operation.Filter);
        }
        else if (UseLegacyIndexFilters(operation, model))
        {
            var table = model?.GetRelationalModel().FindTable(operation.Table, operation.Schema);
            var nullableColumns = operation.Columns
                .Where(c => table?.FindColumn(c)?.IsNullable != false)
                .ToList();

            builder.Append(" WHERE ");
            for (var i = 0; i < nullableColumns.Count; i++)
            {
                if (i != 0)
                {
                    builder.Append(" AND ");
                }

                builder
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(nullableColumns[i]))
                    .Append(" IS NOT NULL");
            }
        }

        IndexWithOptions(operation, builder);
    }

    private static void IndexWithOptions(CreateIndexOperation operation, MigrationCommandListBuilder builder)
    {
        var options = new List<string>();

        if (operation[SqlServerAnnotationNames.FillFactor] is int fillFactor)
        {
            options.Add("FILLFACTOR = " + fillFactor);
        }

        if (operation[SqlServerAnnotationNames.CreatedOnline] is bool isOnline && isOnline)
        {
            options.Add("ONLINE = ON");
        }

        if (operation[SqlServerAnnotationNames.SortInTempDb] is bool sortInTempDb && sortInTempDb)
        {
            options.Add("SORT_IN_TEMPDB = ON");
        }

        if (operation[SqlServerAnnotationNames.DataCompression] is DataCompressionType dataCompressionType)
        {
            switch (dataCompressionType)
            {
                case DataCompressionType.None:
                    options.Add("DATA_COMPRESSION = NONE");
                    break;
                case DataCompressionType.Row:
                    options.Add("DATA_COMPRESSION = ROW");
                    break;
                case DataCompressionType.Page:
                    options.Add("DATA_COMPRESSION = PAGE");
                    break;
            }
        }

        if (options.Count > 0)
        {
            builder
                .Append(" WITH (")
                .Append(string.Join(", ", options))
                .Append(")");
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for the given referential action.
    /// </summary>
    /// <param name="referentialAction">The referential action.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected override void ForeignKeyAction(ReferentialAction referentialAction, MigrationCommandListBuilder builder)
    {
        if (referentialAction == ReferentialAction.Restrict)
        {
            builder.Append("NO ACTION");
        }
        else
        {
            base.ForeignKeyAction(referentialAction, builder);
        }
    }

    /// <summary>
    ///     Generates a SQL fragment to drop default constraints for a column.
    /// </summary>
    /// <param name="schema">The schema that contains the table.</param>
    /// <param name="tableName">The table that contains the column.</param>
    /// <param name="columnName">The column.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected virtual void DropDefaultConstraint(
        string? schema,
        string tableName,
        string columnName,
        MigrationCommandListBuilder builder)
    {
        var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

        var variable = "@var" + _variableCounter++;

        builder
            .Append("DECLARE ")
            .Append(variable)
            .AppendLine(" sysname;")
            .Append("SELECT ")
            .Append(variable)
            .AppendLine(" = [d].[name]")
            .AppendLine("FROM [sys].[default_constraints] [d]")
            .AppendLine(
                "INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]")
            .Append("WHERE ([d].[parent_object_id] = OBJECT_ID(")
            .Append(
                stringTypeMapping.GenerateSqlLiteral(
                    Dependencies.SqlGenerationHelper.DelimitIdentifier(tableName, schema)))
            .Append(") AND [c].[name] = ")
            .Append(stringTypeMapping.GenerateSqlLiteral(columnName))
            .AppendLine(");")
            .Append("IF ")
            .Append(variable)
            .Append(" IS NOT NULL EXEC(")
            .Append(
                stringTypeMapping.GenerateSqlLiteral(
                    "ALTER TABLE " + Dependencies.SqlGenerationHelper.DelimitIdentifier(tableName, schema) + " DROP CONSTRAINT ["))
            .Append(" + ")
            .Append(variable)
            .Append(" + ']")
            .Append(Dependencies.SqlGenerationHelper.StatementTerminator)
            .Append("')")
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
    }

    /// <summary>
    ///     Gets the list of indexes that need to be rebuilt when the given column is changing.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <param name="currentOperation">The operation which may require a rebuild.</param>
    /// <returns>The list of indexes affected.</returns>
    protected virtual IEnumerable<ITableIndex> GetIndexesToRebuild(
        IColumn? column,
        MigrationOperation currentOperation)
    {
        if (column == null)
        {
            yield break;
        }

        var table = column.Table;
        var createIndexOperations = _operations.SkipWhile(o => o != currentOperation).Skip(1)
            .OfType<CreateIndexOperation>().Where(o => o.Table == table.Name && o.Schema == table.Schema).ToList();
        foreach (var index in table.Indexes)
        {
            var indexName = index.Name;
            if (createIndexOperations.Any(o => o.Name == indexName))
            {
                continue;
            }

            if (index.Columns.Any(c => c == column))
            {
                yield return index;
            }
            else if (index[SqlServerAnnotationNames.Include] is IReadOnlyList<string> includeColumns
                     && includeColumns.Contains(column.Name))
            {
                yield return index;
            }
        }
    }

    /// <summary>
    ///     Generates SQL to drop the given indexes.
    /// </summary>
    /// <param name="indexes">The indexes to drop.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void DropIndexes(
        IEnumerable<ITableIndex> indexes,
        MigrationCommandListBuilder builder)
    {
        foreach (var index in indexes)
        {
            var table = index.Table;
            var operation = new DropIndexOperation
            {
                Schema = table.Schema,
                Table = table.Name,
                Name = index.Name
            };
            operation.AddAnnotations(index.GetAnnotations());

            Generate(operation, table.Model.Model, builder, terminate: false);
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }
    }

    /// <summary>
    ///     Generates SQL to create the given indexes.
    /// </summary>
    /// <param name="indexes">The indexes to create.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void CreateIndexes(
        IEnumerable<ITableIndex> indexes,
        MigrationCommandListBuilder builder)
    {
        foreach (var index in indexes)
        {
            Generate(CreateIndexOperation.CreateFrom(index), index.Table.Model.Model, builder, terminate: false);
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }
    }

    /// <summary>
    ///     Generates add commands for descriptions on tables and columns.
    /// </summary>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="description">The new description to be applied.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <param name="table">The name of the table.</param>
    /// <param name="column">The name of the column.</param>
    /// <param name="omitVariableDeclarations">
    ///     Indicates whether the variable declarations should be omitted.
    /// </param>
    protected virtual void AddDescription(
        MigrationCommandListBuilder builder,
        string description,
        string? schema,
        string table,
        string? column = null,
        bool omitVariableDeclarations = false)
    {
        string schemaLiteral;
        if (schema == null)
        {
            if (!omitVariableDeclarations)
            {
                builder.Append("DECLARE @defaultSchema AS sysname")
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                builder.Append("SET @defaultSchema = SCHEMA_NAME()")
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }

            schemaLiteral = "@defaultSchema";
        }
        else
        {
            schemaLiteral = Literal(schema);
        }

        if (!omitVariableDeclarations)
        {
            builder.Append("DECLARE @description AS sql_variant")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }

        builder.Append("SET @description = ")
            .Append(Literal(description))
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        builder
            .Append("EXEC sp_addextendedproperty 'MS_Description', ")
            .Append("@description")
            .Append(", 'SCHEMA', ")
            .Append(schemaLiteral)
            .Append(", 'TABLE', ")
            .Append(Literal(table));

        if (column != null)
        {
            builder
                .Append(", 'COLUMN', ")
                .Append(Literal(column));
        }

        builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        string Literal(string s)
            => SqlLiteral(s);

        static string SqlLiteral(string value)
        {
            var builder = new StringBuilder();

            var start = 0;
            int i;
            int length;
            var openApostrophe = false;
            var lastConcatStartPoint = 0;
            var concatCount = 1;
            var concatStartList = new List<int>();
            for (i = 0; i < value.Length; i++)
            {
                var lineFeed = value[i] == '\n';
                var carriageReturn = value[i] == '\r';
                var apostrophe = value[i] == '\'';
                if (lineFeed || carriageReturn || apostrophe)
                {
                    length = i - start;
                    if (length != 0)
                    {
                        if (!openApostrophe)
                        {
                            AddConcatOperatorIfNeeded();
                            builder.Append("N\'");
                            openApostrophe = true;
                        }

                        builder.Append(value.AsSpan().Slice(start, length));
                    }

                    if (lineFeed || carriageReturn)
                    {
                        if (openApostrophe)
                        {
                            builder.Append('\'');
                            openApostrophe = false;
                        }

                        AddConcatOperatorIfNeeded();
                        builder
                            .Append("NCHAR(")
                            .Append(lineFeed ? "10" : "13")
                            .Append(')');
                    }
                    else if (apostrophe)
                    {
                        if (!openApostrophe)
                        {
                            AddConcatOperatorIfNeeded();
                            builder.Append("N'");
                            openApostrophe = true;
                        }

                        builder.Append("''");
                    }

                    start = i + 1;
                }
            }

            length = i - start;
            if (length != 0)
            {
                if (!openApostrophe)
                {
                    AddConcatOperatorIfNeeded();
                    builder.Append("N\'");
                    openApostrophe = true;
                }

                builder.Append(value.AsSpan().Slice(start, length));
            }

            if (openApostrophe)
            {
                builder.Append('\'');
            }

            for (var j = concatStartList.Count - 1; j >= 0; j--)
            {
                builder.Insert(concatStartList[j], "CONCAT(");
                builder.Append(')');
            }

            if (builder.Length == 0)
            {
                builder.Append("N''");
            }

            var result = builder.ToString();

            return result;

            void AddConcatOperatorIfNeeded()
            {
                if (builder.Length != 0)
                {
                    builder.Append(", ");
                    concatCount++;

                    if (concatCount == 2)
                    {
                        concatStartList.Add(lastConcatStartPoint);
                    }

                    if (concatCount == 254)
                    {
                        lastConcatStartPoint = builder.Length;
                        concatCount = 1;
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Generates drop commands for descriptions on tables and columns.
    /// </summary>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="schema">The schema of the table.</param>
    /// <param name="table">The name of the table.</param>
    /// <param name="column">The name of the column.</param>
    /// <param name="omitVariableDeclarations">
    ///     Indicates whether the variable declarations should be omitted.
    /// </param>
    protected virtual void DropDescription(
        MigrationCommandListBuilder builder,
        string? schema,
        string table,
        string? column = null,
        bool omitVariableDeclarations = false)
    {
        var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

        string schemaLiteral;
        if (schema == null)
        {
            if (!omitVariableDeclarations)
            {
                builder.Append("DECLARE @defaultSchema AS sysname")
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                builder.Append("SET @defaultSchema = SCHEMA_NAME()")
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }

            schemaLiteral = "@defaultSchema";
        }
        else
        {
            schemaLiteral = Literal(schema);
        }

        if (!omitVariableDeclarations)
        {
            builder.Append("DECLARE @description AS sql_variant")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }

        builder
            .Append("EXEC sp_dropextendedproperty 'MS_Description', 'SCHEMA', ")
            .Append(schemaLiteral)
            .Append(", 'TABLE', ")
            .Append(Literal(table));

        if (column != null)
        {
            builder
                .Append(", 'COLUMN', ")
                .Append(Literal(column));
        }

        builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        string Literal(string s)
            => stringTypeMapping.GenerateSqlLiteral(s);
    }

    /// <summary>
    ///     Checks whether or not <see cref="CreateIndexOperation" /> should have a filter generated for it by
    ///     Migrations.
    /// </summary>
    /// <param name="operation">The index creation operation.</param>
    /// <param name="model">The target model.</param>
    /// <returns><see langword="true" /> if a filter should be generated.</returns>
    protected virtual bool UseLegacyIndexFilters(CreateIndexOperation operation, IModel? model)
        => (!TryGetVersion(model, out var version) || VersionComparer.Compare(version, "2.0.0") < 0)
            && operation.Filter is null
            && operation.IsUnique
            && operation[SqlServerAnnotationNames.Clustered] is null or false
            && model?.GetRelationalModel().FindTable(operation.Table, operation.Schema) is var table
            && operation.Columns.Any(c => table?.FindColumn(c)?.IsNullable != false);

    private static string IntegerConstant(long value)
        => string.Format(CultureInfo.InvariantCulture, "{0}", value);

    private static string IntegerConstant(int value)
        => string.Format(CultureInfo.InvariantCulture, "{0}", value);

    private static bool IsMemoryOptimized(Annotatable annotatable, IModel? model, string? schema, string tableName)
        => annotatable[SqlServerAnnotationNames.MemoryOptimized] as bool?
            ?? model?.GetRelationalModel().FindTable(tableName, schema)?[SqlServerAnnotationNames.MemoryOptimized] as bool? == true;

    private static bool IsMemoryOptimized(Annotatable annotatable)
        => annotatable[SqlServerAnnotationNames.MemoryOptimized] as bool? == true;

    private static bool IsIdentity(ColumnOperation operation)
        => operation[SqlServerAnnotationNames.Identity] != null
            || operation[SqlServerAnnotationNames.ValueGenerationStrategy] as SqlServerValueGenerationStrategy?
            == SqlServerValueGenerationStrategy.IdentityColumn;

    private static bool TryParseIdentitySeedIncrement(ColumnOperation operation, out int seed, out int increment)
    {
        if (operation[SqlServerAnnotationNames.Identity] is string seedIncrement
            && seedIncrement.Split(",") is [var seedString, var incrementString]
            && int.TryParse(seedString, out var seedParsed)
            && int.TryParse(incrementString, out var incrementParsed))
        {
            (seed, increment) = (seedParsed, incrementParsed);
            return true;
        }

        (seed, increment) = (0, 0);
        return false;
    }

    private void GenerateExecWhenIdempotent(
        MigrationCommandListBuilder builder,
        Action<MigrationCommandListBuilder> generate)
    {
        if (Options.HasFlag(MigrationsSqlGenerationOptions.Idempotent))
        {
            var subBuilder = new MigrationCommandListBuilder(Dependencies);
            generate(subBuilder);

            var command = subBuilder.GetCommandList().Single();
            builder
                .Append("EXEC(N'")
                .Append(command.CommandText.TrimEnd('\n', '\r', ';').Replace("'", "''"))
                .Append("')")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand(command.TransactionSuppressed);

            return;
        }

        generate(builder);
    }

    private static bool HasDifferences(IEnumerable<IAnnotation> source, IEnumerable<IAnnotation> target)
    {
        var targetAnnotations = target.ToDictionary(a => a.Name);

        var count = 0;
        foreach (var sourceAnnotation in source)
        {
            if (!targetAnnotations.TryGetValue(sourceAnnotation.Name, out var targetAnnotation)
                || !Equals(sourceAnnotation.Value, targetAnnotation.Value))
            {
                return true;
            }

            count++;
        }

        return count != targetAnnotations.Count;
    }

    private IReadOnlyList<MigrationOperation> RewriteOperations(
        IReadOnlyList<MigrationOperation> migrationOperations,
        IModel? model,
        MigrationsSqlGenerationOptions options)
    {
        var operations = new List<MigrationOperation>();
        var availableSchemas = new List<string>();

        // we need to know temporal information for all the tables involved in the migration
        // problem is, the temporal information is stored only on table operations and not column operations
        // if migration operation doesn't contain the table operation, or the table operation comes later
        // we don't know what we should do
        // to fix that, we loop through all the operations and extract initial temporal state for relevant tables
        // if we don't encounter any table operations, then we can take information from the model
        // since migration hasn't changed it at all - be we can only know that after looping though all ops
        // once we have the initial state of the table, we can update it each time we encounter a table operation
        // and we can use what we stored when dealing with all other operations (that don't contain temporal annotations themselves)
        var temporalTableInformationMap = new Dictionary<(string TableName, string? Schema), TemporalOperationInformation>();
        var missingTemporalTableInformation = new List<(string TableName, string? Schema)>();

        foreach (var operation in migrationOperations)
        {
            switch (operation)
            {
                case CreateTableOperation createTableOperation:
                {
                    var tableName = createTableOperation.Name;
                    var rawSchema = createTableOperation.Schema;
                    var schema = rawSchema ?? model?.GetDefaultSchema();
                    if (!temporalTableInformationMap.ContainsKey((tableName, rawSchema)))
                    {
                        var temporalTableInformation = BuildTemporalInformationFromMigrationOperation(schema, createTableOperation);
                        temporalTableInformationMap[(tableName, rawSchema)] = temporalTableInformation;
                    }
                    // no need to remove from missingTemporalTableInformation - CreateTable should be first operation for this table
                    // so there can't be entry for it in missingTemporalTableInformation (they are added by other/earlier operations on that table)
                    // the only possibility is that we had a table before, dropped it and now creating a new table with the same name
                    // but in this case we would have generated the necessary information from the DropTableOperation
                    // and also removed the missingTemporalTableInformation entry if there was one before
                    break;
                }

                case DropTableOperation dropTableOperation:
                {
                    var tableName = dropTableOperation.Name;
                    var rawSchema = dropTableOperation.Schema;
                    var schema = rawSchema ?? model?.GetDefaultSchema();
                    if (!temporalTableInformationMap.ContainsKey((tableName, rawSchema)))
                    {
                        var temporalTableInformation = BuildTemporalInformationFromMigrationOperation(schema, dropTableOperation);
                        temporalTableInformationMap[(tableName, rawSchema)] = temporalTableInformation;
                    }

                    missingTemporalTableInformation.Remove((tableName, rawSchema));
                    break;
                }

                case RenameTableOperation renameTableOperation:
                {
                    var tableName = renameTableOperation.Name;
                    var rawSchema = renameTableOperation.Schema;
                    var schema = rawSchema ?? model?.GetDefaultSchema();
                    var newTableName = renameTableOperation.NewName!;
                    var newRawSchema = renameTableOperation.NewSchema;
                    var newSchema = newRawSchema ?? model?.GetDefaultSchema();

                    if (!temporalTableInformationMap.ContainsKey((tableName, rawSchema)))
                    {
                        var temporalTableInformation = BuildTemporalInformationFromMigrationOperation(schema, renameTableOperation);
                        temporalTableInformationMap[(tableName, rawSchema)] = temporalTableInformation;
                        temporalTableInformationMap[(newTableName, newRawSchema)] = temporalTableInformation;
                    }

                    missingTemporalTableInformation.Remove((tableName, rawSchema));
                    missingTemporalTableInformation.Remove((newTableName, newRawSchema));

                    break;
                }

                case AlterTableOperation alterTableOperation:
                {
                    var tableName = alterTableOperation.Name;
                    var rawSchema = alterTableOperation.Schema;
                    var schema = rawSchema ?? model?.GetDefaultSchema();
                    if (!temporalTableInformationMap.ContainsKey((tableName, rawSchema)))
                    {
                        // we create the temporal info based on the OLD table here - we want the initial state
                        var temporalTableInformation = BuildTemporalInformationFromMigrationOperation(schema, alterTableOperation.OldTable);
                        temporalTableInformationMap[(tableName, rawSchema)] = temporalTableInformation;
                    }

                    missingTemporalTableInformation.Remove((tableName, schema));
                    break;
                }

                default:
                {
                    if (operation is ITableMigrationOperation tableMigrationOperation)
                    {
                        var tableName = tableMigrationOperation.Table;
                        var rawSchema = tableMigrationOperation.Schema;
                        if (!temporalTableInformationMap.ContainsKey((tableName, rawSchema))
                            && !missingTemporalTableInformation.Contains((tableName, rawSchema)))
                        {
                            missingTemporalTableInformation.Add((tableName, rawSchema));
                        }
                    }
                    break;
                }
            }
        }

        // fill the missing temporal information from Relational Model - it's the second best source we have
        // if we can't figure out proper temporal info from table annotations,
        // and we don't have it in relational model (for whatever reason) we assume table is not temporal
        // this last step is purely defensive and shouldn't happen in real situations
        foreach (var missingInfo in missingTemporalTableInformation)
        {
            var table = model?.GetRelationalModel().FindTable(missingInfo.TableName, missingInfo.Schema)!;
            if (table != null)
            {
                var schema = missingInfo.Schema ?? model?.GetDefaultSchema();

                var temporalTableInformation = BuildTemporalInformationFromMigrationOperation(schema, table);
                temporalTableInformationMap[(missingInfo.TableName, missingInfo.Schema)] = temporalTableInformation;
            }
            else
            {
                temporalTableInformationMap[(missingInfo.TableName, missingInfo.Schema)] = new TemporalOperationInformation
                {
                    IsTemporalTable = false,
                    HistoryTableName = null,
                    HistoryTableSchema = null,
                    PeriodStartColumnName = null,
                    PeriodEndColumnName = null
                };
            }
        }

        // now we do proper processing - for table operations we look at the annotations on them
        // and continuously update the stored temporal info as the table is being modified
        // for column (and other) operations we don't have annotations on them, so we look into the
        // information we stored in the initial pass and updated in when processing table ops that happened earlier
        foreach (var operation in migrationOperations)
        {
            if (operation is EnsureSchemaOperation ensureSchemaOperation)
            {
                availableSchemas.Add(ensureSchemaOperation.Name);
            }

            if (operation is not ITableMigrationOperation tableMigrationOperation)
            {
                operations.Add(operation);
                continue;
            }

            var tableName = tableMigrationOperation.Table;
            var rawSchema = tableMigrationOperation.Schema;

            var suppressTransaction = IsMemoryOptimized(operation, model, rawSchema, tableName);

            var schema = rawSchema ?? model?.GetDefaultSchema();

            // we are guaranteed to find entry here - we looped through all the operations earlier,
            // info missing from operations we got from the model
            // and in case of no/incomplete model we created dummy (non-temporal) entries
            var temporalInformation = temporalTableInformationMap[(tableName, rawSchema)];

            switch (operation)
            {
                case CreateTableOperation createTableOperation:
                {
                    // for create table we always generate new temporal information from the operation itself
                    // just in case there was a table with that name before that got deleted/renamed
                    // this shouldn't happen as we re-use existin tables rather than drop/recreate
                    // but we are being extra defensive here
                    // and also, temporal state (disabled versioning etc.) should always reset when creating a table
                    temporalInformation = BuildTemporalInformationFromMigrationOperation(schema, createTableOperation);

                    if (temporalInformation.IsTemporalTable
                        && temporalInformation.HistoryTableSchema != schema
                        && temporalInformation.HistoryTableSchema != null
                        && !availableSchemas.Contains(temporalInformation.HistoryTableSchema))
                    {
                        operations.Add(new EnsureSchemaOperation { Name = temporalInformation.HistoryTableSchema });
                        availableSchemas.Add(temporalInformation.HistoryTableSchema);
                    }

                    operations.Add(operation);

                    break;
                }

                case DropTableOperation dropTableOperation:
                {
                    var isTemporalTable = dropTableOperation[SqlServerAnnotationNames.IsTemporal] as bool? == true;
                    if (isTemporalTable)
                    {
                        // if we don't have temporal information, but we know table is temporal
                        // (based on the annotation found on the operation itself)
                        // we assume that versioning must be disabled, if we have temporal info we can check properly
                        if (temporalInformation is null || !temporalInformation.DisabledVersioning)
                        {
                            AddDisableVersioningOperation(tableName, schema, suppressTransaction);
                        }

                        if (temporalInformation is not null)
                        {
                            temporalInformation.ShouldEnableVersioning = false;
                            temporalInformation.ShouldEnablePeriod = false;
                        }

                        operations.Add(operation);

                        var historyTableName = dropTableOperation[SqlServerAnnotationNames.TemporalHistoryTableName] as string;
                        var historyTableSchema = dropTableOperation[SqlServerAnnotationNames.TemporalHistoryTableSchema] as string ?? schema;
                        var dropHistoryTableOperation = new DropTableOperation { Name = historyTableName!, Schema = historyTableSchema };
                        operations.Add(dropHistoryTableOperation);
                    }
                    else
                    {
                        operations.Add(operation);
                    }

                    // we removed the table, so we no longer need it's temporal information
                    // there will be no more operations involving this table
                    temporalTableInformationMap.Remove((tableName, schema));

                    break;
                }

                case RenameTableOperation renameTableOperation:
                {
                    if (temporalInformation is null)
                    {
                        temporalInformation = BuildTemporalInformationFromMigrationOperation(schema, renameTableOperation);
                    }

                    var isTemporalTable = renameTableOperation[SqlServerAnnotationNames.IsTemporal] as bool? == true;
                    if (isTemporalTable)
                    {
                        DisableVersioning(
                            tableName,
                            schema,
                            temporalInformation,
                            suppressTransaction,
                            shouldEnableVersioning: true);
                    }

                    operations.Add(operation);

                    // since table was renamed, update entry in the temporal info map
                    temporalTableInformationMap[(renameTableOperation.NewName!, renameTableOperation.NewSchema)] = temporalInformation;
                    temporalTableInformationMap.Remove((tableName, schema));

                    break;
                }

                case AlterTableOperation alterTableOperation:
                {
                    var isTemporalTable = alterTableOperation[SqlServerAnnotationNames.IsTemporal] as bool? == true;
                    var historyTableName = alterTableOperation[SqlServerAnnotationNames.TemporalHistoryTableName] as string;
                    var historyTableSchema = alterTableOperation[SqlServerAnnotationNames.TemporalHistoryTableSchema] as string ?? schema;
                    var periodStartColumnName = alterTableOperation[SqlServerAnnotationNames.TemporalPeriodStartColumnName] as string;
                    var periodEndColumnName = alterTableOperation[SqlServerAnnotationNames.TemporalPeriodEndColumnName] as string;

                    var oldIsTemporalTable = alterTableOperation.OldTable[SqlServerAnnotationNames.IsTemporal] as bool? == true;
                    var oldHistoryTableName =
                        alterTableOperation.OldTable[SqlServerAnnotationNames.TemporalHistoryTableName] as string;
                    var oldHistoryTableSchema =
                        alterTableOperation.OldTable[SqlServerAnnotationNames.TemporalHistoryTableSchema] as string
                        ?? alterTableOperation.OldTable.Schema
                        ?? model?[RelationalAnnotationNames.DefaultSchema] as string;

                    if (isTemporalTable)
                    {
                        if (!oldIsTemporalTable)
                        {
                            // converting from regular table to temporal table - enable period and versioning at the end
                            // other temporal information (history table, period columns etc) is added below
                            temporalInformation.ShouldEnablePeriod = true;
                            temporalInformation.ShouldEnableVersioning = true;
                        }
                        else
                        {
                            // changing something within temporal table
                            if (oldHistoryTableName != historyTableName
                                || oldHistoryTableSchema != historyTableSchema)
                            {
                                if (historyTableSchema != null
                                    && !availableSchemas.Contains(historyTableSchema))
                                {
                                    operations.Add(new EnsureSchemaOperation { Name = historyTableSchema });
                                    availableSchemas.Add(historyTableSchema);
                                }

                                operations.Add(
                                    new RenameTableOperation
                                    {
                                        Name = oldHistoryTableName!,
                                        Schema = oldHistoryTableSchema,
                                        NewName = historyTableName,
                                        NewSchema = historyTableSchema
                                    });

                                temporalInformation.HistoryTableName = historyTableName;
                                temporalInformation.HistoryTableSchema = historyTableSchema;
                            }
                        }
                    }
                    else
                    {
                        if (oldIsTemporalTable)
                        {
                            // converting from temporal table to regular table
                            var oldPeriodStartColumnName =
                                alterTableOperation.OldTable[SqlServerAnnotationNames.TemporalPeriodStartColumnName] as string;
                            var oldPeriodEndColumnName =
                                alterTableOperation.OldTable[SqlServerAnnotationNames.TemporalPeriodEndColumnName] as string;

                            DisableVersioning(
                                tableName,
                                schema,
                                temporalInformation,
                                suppressTransaction,
                                shouldEnableVersioning: null);

                            if (!temporalInformation.DisabledPeriod)
                            {
                                DisablePeriod(tableName, schema, temporalInformation, suppressTransaction);
                            }

                            if (oldHistoryTableName != null)
                            {
                                operations.Add(new DropTableOperation { Name = oldHistoryTableName, Schema = oldHistoryTableSchema });
                            }

                            // also clear any pending versioning/period, that would be switched on at the end
                            // we don't need it now that the table is no longer temporal
                            temporalInformation.ShouldEnableVersioning = false;
                            temporalInformation.ShouldEnablePeriod = false;
                        }
                    }

                    temporalInformation.IsTemporalTable = isTemporalTable;
                    temporalInformation.HistoryTableName = historyTableName;
                    temporalInformation.HistoryTableSchema = historyTableSchema;
                    temporalInformation.PeriodStartColumnName = periodStartColumnName;
                    temporalInformation.PeriodEndColumnName = periodEndColumnName;

                    operations.Add(operation);
                    break;
                }

                case AddColumnOperation addColumnOperation:
                {
                    // when adding a period column, we need to add it as a normal column first, and only later enable period
                    // removing the period information now, so that when we generate SQL that adds the column we won't be making them
                    // auto generated as period it won't work, unless period is enabled but we can't enable period without adding the
                    // columns first - chicken and egg
                    if (temporalInformation.IsTemporalTable)
                    {
                        addColumnOperation.RemoveAnnotation(SqlServerAnnotationNames.TemporalIsPeriodStartColumn);
                        addColumnOperation.RemoveAnnotation(SqlServerAnnotationNames.TemporalIsPeriodEndColumn);

                        // model differ adds default value, but for period end we need to replace it with the correct one -
                        // DateTime.MaxValue
                        if (addColumnOperation.Name == temporalInformation.PeriodEndColumnName)
                        {
                            addColumnOperation.DefaultValue = DateTime.MaxValue;
                        }

                        var isSparse = addColumnOperation[SqlServerAnnotationNames.Sparse] as bool? == true;
                        var isComputed = addColumnOperation.ComputedColumnSql != null;

                        if (isSparse || isComputed)
                        {
                            DisableVersioning(
                                tableName,
                                schema,
                                temporalInformation,
                                suppressTransaction,
                                shouldEnableVersioning: true);
                        }

                        // when adding sparse column to temporal table, we need to disable versioning.
                        // This is because it may be the case that HistoryTable is using compression (by default)
                        // and the add column operation fails in that situation
                        // in order to make it work we need to disable versioning (if we haven't done it already)
                        // and de-compress the HistoryTable
                        if (isSparse)
                        {
                            DecompressTable(temporalInformation.HistoryTableName!, temporalInformation.HistoryTableSchema, suppressTransaction);
                        }

                        if (addColumnOperation.ComputedColumnSql != null)
                        {
                            DisableVersioning(
                                tableName,
                                schema,
                                temporalInformation,
                                suppressTransaction,
                                shouldEnableVersioning: true);
                        }

                        operations.Add(addColumnOperation);

                        // when adding (non-period) column to an existing temporal table we need to check if we have disabled versioning
                        // due to some other operations in the same migration (e.g. delete column)
                        // if so, we need to also add the same column to history table
                        if (addColumnOperation.Name != temporalInformation.PeriodStartColumnName
                            && addColumnOperation.Name != temporalInformation.PeriodEndColumnName
                            && temporalInformation.DisabledVersioning)
                        {
                            var addHistoryTableColumnOperation = CopyColumnOperation<AddColumnOperation>(addColumnOperation);
                            addHistoryTableColumnOperation.Table = temporalInformation.HistoryTableName!;
                            addHistoryTableColumnOperation.Schema = temporalInformation.HistoryTableSchema;

                            if (addHistoryTableColumnOperation.ComputedColumnSql != null)
                            {
                                // computed columns are not allowed inside HistoryTables
                                // but the historical computed value will be copied over to the non-computed counterpart,
                                // as long as their names and types (including nullability) match
                                // so we remove ComputedColumnSql info, so that the column in history table "appears normal"
                                addHistoryTableColumnOperation.ComputedColumnSql = null;
                            }

                            operations.Add(addHistoryTableColumnOperation);
                        }
                    }
                    else
                    {
                        operations.Add(addColumnOperation);
                    }

                    break;
                }

                case DropColumnOperation dropColumnOperation:
                {
                    if (temporalInformation.IsTemporalTable)
                    {
                        var droppingPeriodColumn = dropColumnOperation.Name == temporalInformation.PeriodStartColumnName
                            || dropColumnOperation.Name == temporalInformation.PeriodEndColumnName;

                        // if we are dropping non-period column, we should enable versioning at the end.
                        // When dropping period column there is no need - we are removing the versioning for this table altogether
                        DisableVersioning(
                            tableName,
                            schema,
                            temporalInformation,
                            suppressTransaction,
                            shouldEnableVersioning: droppingPeriodColumn ? null : true);

                        if (droppingPeriodColumn && !temporalInformation.DisabledPeriod)
                        {
                            DisablePeriod(tableName, schema, temporalInformation, suppressTransaction);

                            // if we remove the period columns, it means we will be dropping the table
                            // also or at least convert it back to regular - no need to enable period later
                            temporalInformation.ShouldEnablePeriod = false;
                        }

                        operations.Add(operation);

                        if (!droppingPeriodColumn)
                        {
                            operations.Add(new DropColumnOperation
                            {
                                Name = dropColumnOperation.Name,
                                Table = temporalInformation.HistoryTableName!,
                                Schema = temporalInformation.HistoryTableSchema
                            });
                        }
                    }
                    else
                    {
                        operations.Add(operation);
                    }
                    break;
                }

                case RenameColumnOperation renameColumnOperation:
                {
                    operations.Add(renameColumnOperation);

                    // if we disabled period for the temporal table and now we are renaming the column,
                    // we need to also rename this same column in history table
                    if (temporalInformation.IsTemporalTable
                        && temporalInformation.DisabledVersioning
                        && temporalInformation.ShouldEnableVersioning)
                    {
                        var renameHistoryTableColumnOperation = new RenameColumnOperation
                        {
                            IsDestructiveChange = renameColumnOperation.IsDestructiveChange,
                            Name = renameColumnOperation.Name,
                            NewName = renameColumnOperation.NewName,
                            Table = temporalInformation.HistoryTableName!,
                            Schema = temporalInformation.HistoryTableSchema
                        };

                        operations.Add(renameHistoryTableColumnOperation);
                    }

                    break;
                }

                case AlterColumnOperation alterColumnOperation:
                {
                    // we can remove temporal annotations, they don't make a difference when it comes to
                    // generating ALTER COLUMN operations and could just muddy the waters
                    alterColumnOperation.RemoveAnnotation(SqlServerAnnotationNames.TemporalIsPeriodStartColumn);
                    alterColumnOperation.RemoveAnnotation(SqlServerAnnotationNames.TemporalIsPeriodEndColumn);
                    alterColumnOperation.OldColumn.RemoveAnnotation(SqlServerAnnotationNames.TemporalIsPeriodStartColumn);
                    alterColumnOperation.OldColumn.RemoveAnnotation(SqlServerAnnotationNames.TemporalIsPeriodEndColumn);

                    if (temporalInformation.IsTemporalTable)
                    {
                        if (alterColumnOperation.OldColumn.ComputedColumnSql != alterColumnOperation.ComputedColumnSql)
                        {
                            throw new NotSupportedException(
                                SqlServerStrings.TemporalMigrationModifyingComputedColumnNotSupported(
                                    alterColumnOperation.Name,
                                    alterColumnOperation.Table));
                        }

                        // for alter column operation converting column from nullable to non-nullable in the temporal table
                        // we must disable versioning in order to properly handle it
                        // specifically, switching values in history table from null to the default value
                        var changeToNonNullable = alterColumnOperation.OldColumn.IsNullable
                            && !alterColumnOperation.IsNullable;

                        // for alter column converting to sparse we also need to disable versioning
                        // in case HistoryTable is compressed (so that we can de-compress it)
                        var changeToSparse = alterColumnOperation.OldColumn[SqlServerAnnotationNames.Sparse] as bool? != true
                            && alterColumnOperation[SqlServerAnnotationNames.Sparse] as bool? == true;

                        if (changeToNonNullable || changeToSparse)
                        {
                            DisableVersioning(
                                tableName!,
                                schema,
                                temporalInformation,
                                suppressTransaction,
                                shouldEnableVersioning: true);
                        }

                        if (changeToSparse)
                        {
                            DecompressTable(temporalInformation.HistoryTableName!, temporalInformation.HistoryTableSchema, suppressTransaction);
                        }

                        operations.Add(alterColumnOperation);

                        // when modifying a period column, we need to perform the operations as a normal column first, and only later enable period
                        // removing the period information now, so that when we generate SQL that modifies the column we won't be making them auto generated as period
                        // (making column auto generated is not allowed in ALTER COLUMN statement)
                        // in later operation we enable the period and the period columns get set to auto generated automatically
                        //
                        // if the column is not period we just remove temporal information - it's no longer needed and could affect the generated sql
                        // we will generate all the necessary operations involved with temporal tables here
                        if (temporalInformation.DisabledVersioning && temporalInformation.ShouldEnableVersioning)
                        {
                            var alterHistoryTableColumn = CopyColumnOperation<AlterColumnOperation>(alterColumnOperation);
                            alterHistoryTableColumn.Table = temporalInformation.HistoryTableName!;
                            alterHistoryTableColumn.Schema = temporalInformation.HistoryTableSchema;
                            alterHistoryTableColumn.OldColumn = CopyColumnOperation<AddColumnOperation>(alterColumnOperation.OldColumn);
                            alterHistoryTableColumn.OldColumn.Table = temporalInformation.HistoryTableName!;
                            alterHistoryTableColumn.OldColumn.Schema = temporalInformation.HistoryTableSchema;

                            operations.Add(alterHistoryTableColumn);
                        }
                    }
                    else
                    {
                        operations.Add(alterColumnOperation);
                    }
                    break;
                }

                case DropPrimaryKeyOperation:
                case AddPrimaryKeyOperation:
                    if (temporalInformation.IsTemporalTable)
                    {
                        DisableVersioning(
                            tableName!,
                            schema,
                            temporalInformation,
                            suppressTransaction,
                            shouldEnableVersioning: true);
                    }

                    operations.Add(operation);
                    break;

                default:
                    operations.Add(operation);
                    break;
            }
        }

        foreach (var temporalInformation in temporalTableInformationMap.Where(x => x.Value.ShouldEnablePeriod))
        {
            EnablePeriod(
                temporalInformation.Key.TableName,
                temporalInformation.Key.Schema,
                temporalInformation.Value.PeriodStartColumnName!,
                temporalInformation.Value.PeriodEndColumnName!,
                temporalInformation.Value.SuppressTransaction);
        }

        foreach (var temporalInformation in temporalTableInformationMap.Where(x => x.Value.ShouldEnableVersioning))
        {
            EnableVersioning(
                temporalInformation.Key.TableName,
                temporalInformation.Key.Schema,
                temporalInformation.Value.HistoryTableName!,
                temporalInformation.Value.HistoryTableSchema,
                temporalInformation.Value.SuppressTransaction);
        }

        return operations;

        static TemporalOperationInformation BuildTemporalInformationFromMigrationOperation(
            string? schema,
            IAnnotatable operation)
        {
            var isTemporalTable = operation[SqlServerAnnotationNames.IsTemporal] as bool? == true;
            var historyTableName = operation[SqlServerAnnotationNames.TemporalHistoryTableName] as string;
            var historyTableSchema = operation[SqlServerAnnotationNames.TemporalHistoryTableSchema] as string ?? schema;
            var periodStartColumnName = operation[SqlServerAnnotationNames.TemporalPeriodStartColumnName] as string;
            var periodEndColumnName = operation[SqlServerAnnotationNames.TemporalPeriodEndColumnName] as string;

            return new TemporalOperationInformation
            {
                IsTemporalTable = isTemporalTable,
                HistoryTableName = historyTableName,
                HistoryTableSchema = historyTableSchema,
                PeriodStartColumnName = periodStartColumnName,
                PeriodEndColumnName = periodEndColumnName
            };
        }

        void DisableVersioning(
            string tableName,
            string? schema,
            TemporalOperationInformation temporalInformation,
            bool suppressTransaction,
            bool? shouldEnableVersioning)
        {
            if (!temporalInformation.DisabledVersioning
                && !temporalInformation.ShouldEnableVersioning)
            {
                temporalInformation.DisabledVersioning = true;

                AddDisableVersioningOperation(tableName, schema, suppressTransaction);

                if (shouldEnableVersioning != null)
                {
                    temporalInformation.ShouldEnableVersioning = shouldEnableVersioning.Value;
                    if (shouldEnableVersioning.Value)
                    {
                        temporalInformation.SuppressTransaction = suppressTransaction;
                    }
                }
            }
        }

        void AddDisableVersioningOperation(string tableName, string? schema, bool suppressTransaction)
        {
            operations.Add(
                new SqlOperation
                {
                    Sql = new StringBuilder()
                        .Append("ALTER TABLE ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(tableName, schema))
                        .AppendLine(" SET (SYSTEM_VERSIONING = OFF)")
                        .ToString(),
                    SuppressTransaction = suppressTransaction
                });
        }

        void EnableVersioning(string table, string? schema, string historyTableName, string? historyTableSchema, bool suppressTransaction)
        {
            var stringBuilder = new StringBuilder();

            if (historyTableSchema == null)
            {
                // need to run command using EXEC to inject default schema
                stringBuilder.AppendLine("DECLARE @historyTableSchema sysname = SCHEMA_NAME()");
                stringBuilder.Append("EXEC(N'");
            }

            var historyTable = historyTableSchema != null
                ? Dependencies.SqlGenerationHelper.DelimitIdentifier(historyTableName, historyTableSchema)
                : Dependencies.SqlGenerationHelper.DelimitIdentifier(historyTableName);

            stringBuilder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(table, schema));

            if (historyTableSchema != null)
            {
                stringBuilder.AppendLine($" SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = {historyTable}))");
            }
            else
            {
                stringBuilder.AppendLine(
                    $" SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].{historyTable}))')");
            }

            operations.Add(
                new SqlOperation { Sql = stringBuilder.ToString(), SuppressTransaction = suppressTransaction });
        }

        void DisablePeriod(
            string table,
            string? schema,
            TemporalOperationInformation temporalInformation,
            bool suppressTransaction)
        {
            temporalInformation.DisabledPeriod = true;

            operations.Add(
                new SqlOperation
                {
                    Sql = new StringBuilder()
                        .Append("ALTER TABLE ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(table, schema))
                        .AppendLine(" DROP PERIOD FOR SYSTEM_TIME")
                        .ToString(),
                    SuppressTransaction = suppressTransaction
                });
        }

        void EnablePeriod(string table, string? schema, string periodStartColumnName, string periodEndColumnName, bool suppressTransaction)
        {
            var addPeriodSql = new StringBuilder()
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(table, schema))
                .Append(" ADD PERIOD FOR SYSTEM_TIME (")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(periodStartColumnName))
                .Append(", ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(periodEndColumnName))
                .Append(')')
                .ToString();

            if (options.HasFlag(MigrationsSqlGenerationOptions.Idempotent))
            {
                addPeriodSql = new StringBuilder()
                    .Append("EXEC(N'")
                    .Append(addPeriodSql.Replace("'", "''"))
                    .Append("')")
                    .ToString();
            }

            operations.Add(
                new SqlOperation { Sql = addPeriodSql, SuppressTransaction = suppressTransaction });

            operations.Add(
                new SqlOperation
                {
                    Sql = new StringBuilder()
                        .Append("ALTER TABLE ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(table, schema))
                        .Append(" ALTER COLUMN ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(periodStartColumnName))
                        .Append(" ADD HIDDEN")
                        .ToString(),
                    SuppressTransaction = suppressTransaction
                });

            operations.Add(
                new SqlOperation
                {
                    Sql = new StringBuilder()
                        .Append("ALTER TABLE ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(table, schema))
                        .Append(" ALTER COLUMN ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(periodEndColumnName))
                        .Append(" ADD HIDDEN")
                        .ToString(),
                    SuppressTransaction = suppressTransaction
                });
        }

        void DecompressTable(string tableName, string? schema, bool suppressTransaction)
        {
            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            var decompressTableCommand = new StringBuilder()
                .Append("IF EXISTS (")
                .Append("SELECT 1 FROM [sys].[tables] [t] ")
                .Append("INNER JOIN [sys].[partitions] [p] ON [t].[object_id] = [p].[object_id] ")
                .Append($"WHERE [t].[name] = '{tableName}' ");

            if (schema != null)
            {
                decompressTableCommand.Append($"AND [t].[schema_id] = schema_id('{schema}') ");
            }

            decompressTableCommand.AppendLine("AND data_compression <> 0)")
                .Append("EXEC(")
                .Append(stringTypeMapping.GenerateSqlLiteral("ALTER TABLE " +
                    Dependencies.SqlGenerationHelper.DelimitIdentifier(tableName, schema) +
                    " REBUILD PARTITION = ALL WITH (DATA_COMPRESSION = NONE)" +
                    Dependencies.SqlGenerationHelper.StatementTerminator))
                .Append(")")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            operations.Add(
                new SqlOperation
                {
                    Sql = decompressTableCommand.ToString(),
                    SuppressTransaction = suppressTransaction
                });
        }

        static TOperation CopyColumnOperation<TOperation>(ColumnOperation source)
            where TOperation : ColumnOperation, new()
        {
            var result = new TOperation
            {
                ClrType = source.ClrType,
                Collation = source.Collation,
                ColumnType = source.ColumnType,
                Comment = source.Comment,
                ComputedColumnSql = source.ComputedColumnSql,
                DefaultValue = source.DefaultValue,
                DefaultValueSql = source.DefaultValueSql,
                IsDestructiveChange = source.IsDestructiveChange,
                IsFixedLength = source.IsFixedLength,
                IsNullable = source.IsNullable,
                IsRowVersion = source.IsRowVersion,
                IsStored = source.IsStored,
                IsUnicode = source.IsUnicode,
                MaxLength = source.MaxLength,
                Name = source.Name,
                Precision = source.Precision,
                Scale = source.Scale,
                Table = source.Table,
                Schema = source.Schema
            };

            foreach (var annotation in source.GetAnnotations())
            {
                result.AddAnnotation(annotation.Name, annotation.Value);
            }

            return result;
        }
    }

    private sealed class TemporalOperationInformation
    {
        public bool IsTemporalTable { get; set; }
        public string? HistoryTableName { get; set; }
        public string? HistoryTableSchema { get; set; }
        public string? PeriodStartColumnName { get; set; }
        public string? PeriodEndColumnName { get; set; }

        public bool DisabledVersioning { get; set; } = false;
        public bool DisabledPeriod { get; set; } = false;

        public bool ShouldEnableVersioning { get; set; } = false;
        public bool ShouldEnablePeriod { get; set; } = false;
        public bool SuppressTransaction { get; set; } = false;
    }
}
