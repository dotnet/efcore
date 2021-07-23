// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     <para>
    ///         SQL Server-specific implementation of <see cref="MigrationsSqlGenerator" />.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class SqlServerMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        private IReadOnlyList<MigrationOperation> _operations = null!;
        private int _variableCounter;

        /// <summary>
        ///     Creates a new <see cref="SqlServerMigrationsSqlGenerator" /> instance.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        /// <param name="migrationsAnnotations"> Provider-specific Migrations annotations to use. </param>
        public SqlServerMigrationsSqlGenerator(
            MigrationsSqlGeneratorDependencies dependencies,
            IRelationalAnnotationProvider migrationsAnnotations)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     Generates commands from a list of operations.
        /// </summary>
        /// <param name="operations"> The operations. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="options"> The options to use when generating commands. </param>
        /// <returns> The list of commands to be executed or scripted. </returns>
        public override IReadOnlyList<MigrationCommand> Generate(
            IReadOnlyList<MigrationOperation> operations,
            IModel? model = null,
            MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
        {
            _operations = operations;
            try
            {
                return base.Generate(RewriteOperations(operations, model), model, options);
            }
            finally
            {
                _operations = null!;
            }
        }

        /// <summary>
        ///     <para>
        ///         Builds commands for the given <see cref="MigrationOperation" /> by making calls on the given
        ///         <see cref="MigrationCommandListBuilder" />.
        ///     </para>
        ///     <para>
        ///         This method uses a double-dispatch mechanism to call the <see cref="M:MigrationsSqlGenerator.Generate" /> method
        ///         that is specific to a certain subtype of <see cref="MigrationOperation" />. Typically database providers
        ///         will override these specific methods rather than this method. However, providers can override
        ///         this methods to handle provider-specific operations.
        ///     </para>
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            AlterColumnOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            IEnumerable<ITableIndex>? indexesToRebuild = null;
            var column = model?.GetRelationalModel().FindTable(operation.Table, operation.Schema)
                ?.Columns.FirstOrDefault(c => c.Name == operation.Name);

            if (operation.ComputedColumnSql != null)
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
                    || !operation.IsNullable && operation.OldColumn.IsNullable;
            }

            if (narrowed)
            {
                indexesToRebuild = GetIndexesToRebuild(column, operation).ToList();
                DropIndexes(indexesToRebuild, builder);
            }

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
                || HasDifferences(operation.GetAnnotations(), operation.OldColumn.GetAnnotations());

            var (oldDefaultValue, oldDefaultValueSql) = (operation.OldColumn.DefaultValue, operation.OldColumn.DefaultValueSql);

            if (alterStatementNeeded
                || !Equals(operation.DefaultValue, oldDefaultValue)
                || operation.DefaultValueSql != oldDefaultValueSql)
            {
                DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);
                (oldDefaultValue, oldDefaultValueSql) = (null, null);
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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            RenameIndexOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(RenameSequenceOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var name = operation.Name;
            if (operation.NewName != null
                && operation.NewName != name)
            {
                Rename(
                    Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema),
                    operation.NewName,
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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            RestartSequenceOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER SEQUENCE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" RESTART WITH ")
                .Append(IntegerConstant(operation.StartValue))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        /// <summary>
        ///     Builds commands for the given <see cref="CreateTableOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            CreateTableOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            if (!terminate
                && operation.Comment != null)
            {
                throw new ArgumentException(SqlServerStrings.CannotProduceUnterminatedSQLWithComments(nameof(CreateTableOperation)));
            }

            if (operation[SqlServerAnnotationNames.IsTemporal] as bool? == true)
            {
                var historyTableSchema = operation[SqlServerAnnotationNames.TemporalHistoryTableSchema] as string ?? model?.GetDefaultSchema();

                if (historyTableSchema != operation.Schema && historyTableSchema != null)
                {
                    Generate(new EnsureSchemaOperation { Name = historyTableSchema }, model, builder);
                }

                var needsExec = historyTableSchema == null;
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
                var historyTable = default(string);
                if (needsExec)
                {
                    historyTable = Dependencies.SqlGenerationHelper.DelimitIdentifier(historyTableName!);
                    builder.Append($") WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].{ historyTable}))')");
                }
                else
                {
                    historyTable = Dependencies.SqlGenerationHelper.DelimitIdentifier(historyTableName!, historyTableSchema);
                    builder.Append($") WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = {historyTable}))");
                }
            }
            else
            {
                base.Generate(operation, model, builder, terminate: false);
            }

            var memoryOptimized = IsMemoryOptimized(operation);
            if (memoryOptimized)
            {
                builder.AppendLine();
                using (builder.Indent())
                {
                    builder.AppendLine("WITH");
                    using (builder.Indent())
                    {
                        builder.Append("(MEMORY_OPTIMIZED = ON)");
                    }
                }
            }

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

        /// <summary>
        ///     Builds commands for the given <see cref="RenameTableOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            RenameTableOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var name = operation.Name;
            if (operation.NewName != null
                && operation.NewName != name)
            {
                Rename(
                    Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema),
                    operation.NewName,
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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
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

            if (operation[SqlServerAnnotationNames.IsTemporal] as bool? == true)
            {
                var schema = operation.Schema ?? model?[RelationalAnnotationNames.DefaultSchema] as string;
                var historyTableSchema = operation[SqlServerAnnotationNames.TemporalHistoryTableSchema] as string ?? schema;
                if (operation[SqlServerAnnotationNames.TemporalHistoryTableName] is string historyTableName)
                {
                    var dropHistoryTableOperation = new DropTableOperation
                    {
                        Name = historyTableName,
                        Schema = historyTableSchema,
                    };

                    Generate(dropHistoryTableOperation, model, builder, terminate);
                }
            }
        }

        /// <summary>
        ///     Builds commands for the given <see cref="CreateIndexOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            CreateIndexOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

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

                builder
                    .Append("(")
                    .Append(ColumnList(operation.Columns))
                    .Append(")");
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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(EnsureSchemaOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            CreateSequenceOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("CREATE SEQUENCE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

            if (operation.ClrType != typeof(long))
            {
                var typeMapping = Dependencies.TypeMappingSource.GetMapping(operation.ClrType);

                builder
                    .Append(" AS ")
                    .Append(typeMapping.StoreType);
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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected virtual void Generate(
            SqlServerCreateDatabaseOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

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
            Check.NotNull(fileName, nameof(fileName));

            if (fileName.StartsWith("|DataDirectory|", StringComparison.OrdinalIgnoreCase))
            {
                var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                if (string.IsNullOrEmpty(dataDirectory))
                {
                    dataDirectory = AppDomain.CurrentDomain.BaseDirectory;
                }

                fileName = Path.Combine(dataDirectory, fileName.Substring("|DataDirectory|".Length));
            }

            return Path.GetFullPath(fileName);
        }

        /// <summary>
        ///     Builds commands for the given <see cref="SqlServerDropDatabaseOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected virtual void Generate(
            SqlServerDropDatabaseOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            AlterDatabaseOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            DropIndexOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            DropColumnOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);
            base.Generate(operation, model, builder, terminate: false);

            if (terminate)
            {
                builder
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                    .EndCommand(suppressTransaction: IsMemoryOptimized(operation, model, operation.Schema, operation.Table));
            }

            if (operation[SqlServerAnnotationNames.IsTemporal] as bool? == true)
            {
                var historyTableName = operation[SqlServerAnnotationNames.TemporalHistoryTableName] as string;
                var historyTableSchema = operation[SqlServerAnnotationNames.TemporalHistoryTableSchema] as string ?? model?.GetDefaultSchema();
                var periodStartColumnName = operation[SqlServerAnnotationNames.TemporalPeriodStartColumnName] as string;
                var periodEndColumnName = operation[SqlServerAnnotationNames.TemporalPeriodEndColumnName] as string;

                // when dropping column, we only need to drop the column from history table as well if that column is not part of the period
                // for columns that are part of the period - if we are removing them from the temporal table, it means
                // that we are converting back to a regular table, and the history table will be removed anyway
                // so we don't need to keep it in sync
                if (operation.Name != periodStartColumnName
                    && operation.Name != periodEndColumnName)
                {
                    Generate(new DropColumnOperation
                    {
                        Name = operation.Name,
                        Table = historyTableName!,
                        Schema = historyTableSchema
                    }, model, builder, terminate);
                }
            }
        }

        /// <summary>
        ///     Builds commands for the given <see cref="RenameColumnOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            RenameColumnOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(SqlOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var batches = Regex.Split(
                Regex.Replace(
                    operation.Sql,
                    @"\\\r?\n",
                    string.Empty,
                    default,
                    TimeSpan.FromMilliseconds(1000.0)),
                @"^\s*(GO[ \t]+[0-9]+|GO)(?:\s+|$)",
                RegexOptions.IgnoreCase | RegexOptions.Multiline,
                TimeSpan.FromMilliseconds(1000.0));
            for (var i = 0; i < batches.Length; i++)
            {
                if (batches[i].StartsWith("GO", StringComparison.OrdinalIgnoreCase)
                    || string.IsNullOrWhiteSpace(batches[i]))
                {
                    continue;
                }

                var count = 1;
                if (i != batches.Length - 1
                    && batches[i + 1].StartsWith("GO", StringComparison.OrdinalIgnoreCase))
                {
                    var match = Regex.Match(
                        batches[i + 1], "([0-9]+)",
                        default,
                        TimeSpan.FromMilliseconds(1000.0));
                    if (match.Success)
                    {
                        count = int.Parse(match.Value);
                    }
                }

                for (var j = 0; j < count; j++)
                {
                    builder.Append(batches[i]);

                    if (i == batches.Length - 1)
                    {
                        builder.AppendLine();
                    }

                    EndStatement(builder, operation.SuppressTransaction);
                }
            }
        }

        /// <summary>
        ///     Builds commands for the given <see cref="InsertDataOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            InsertDataOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            GenerateIdentityInsert(builder, operation, on: true);

            var sqlBuilder = new StringBuilder();
            ((SqlServerUpdateSqlGenerator)Dependencies.UpdateSqlGenerator).AppendBulkInsertOperation(
                sqlBuilder,
                GenerateModificationCommands(operation, model).ToList(),
                0);

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

            GenerateIdentityInsert(builder, operation, on: false);

            if (terminate)
            {
                builder.EndCommand();
            }
        }

        private void GenerateIdentityInsert(MigrationCommandListBuilder builder, InsertDataOperation operation, bool on)
        {
            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            builder
                .Append("IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE")
                .Append(" [name] IN (")
                .Append(string.Join(", ", operation.Columns.Select(stringTypeMapping.GenerateSqlLiteral)))
                .Append(") AND [object_id] = OBJECT_ID(")
                .Append(
                    stringTypeMapping.GenerateSqlLiteral(
                        Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema)))
                .AppendLine("))");

            using (builder.Indent())
            {
                builder
                    .Append("SET IDENTITY_INSERT ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
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

        /// <summary>
        ///     Generates a SQL fragment configuring a sequence with the given options.
        /// </summary>
        /// <param name="schema"> The schema that contains the sequence, or <see langword="null" /> to use the default schema. </param>
        /// <param name="name"> The sequence name. </param>
        /// <param name="operation"> The sequence options. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void SequenceOptions(
            string? schema,
            string name,
            SequenceOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(" INCREMENT BY ")
                .Append(IntegerConstant(operation.IncrementBy));

            if (operation.MinValue.HasValue)
            {
                builder
                    .Append(" MINVALUE ")
                    .Append(IntegerConstant(operation.MinValue.Value));
            }
            else
            {
                builder.Append(" NO MINVALUE");
            }

            if (operation.MaxValue.HasValue)
            {
                builder
                    .Append(" MAXVALUE ")
                    .Append(IntegerConstant(operation.MaxValue.Value));
            }
            else
            {
                builder.Append(" NO MAXVALUE");
            }

            builder.Append(operation.IsCyclic ? " CYCLE" : " NO CYCLE");
        }

        /// <summary>
        ///     Generates a SQL fragment for a column definition for the given column metadata.
        /// </summary>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <param name="table"> The table that contains the column. </param>
        /// <param name="name"> The column name. </param>
        /// <param name="operation"> The column metadata. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void ColumnDefinition(
            string? schema,
            string table,
            string name,
            ColumnOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

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

            var periodStartColumnName = operation[SqlServerAnnotationNames.TemporalPeriodStartColumnName] as string;
            var periodEndColumnName = operation[SqlServerAnnotationNames.TemporalPeriodEndColumnName] as string;

            if (name == periodStartColumnName
                || name == periodEndColumnName)
            {
                builder.Append(" GENERATED ALWAYS AS ROW ");

                if (name == periodStartColumnName)
                {
                    builder.Append("START");
                }
                else
                {
                    builder.Append("END");
                }
            }

            builder.Append(operation.IsNullable ? " NULL" : " NOT NULL");

            DefaultValue(operation.DefaultValue, operation.DefaultValueSql, columnType, builder);

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
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <param name="table"> The table that contains the column. </param>
        /// <param name="name"> The column name. </param>
        /// <param name="operation"> The column metadata. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void ComputedColumnDefinition(
            string? schema,
            string table,
            string name,
            ColumnOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name));

            builder
                .Append(" AS ")
                .Append(operation.ComputedColumnSql!);

            if (operation.IsStored == true)
            {
                builder.Append(" PERSISTED");
            }

            if (operation.Collation != null)
            {
                builder
                    .Append(" COLLATE ")
                    .Append(operation.Collation);
            }
        }

        /// <summary>
        ///     Generates a rename.
        /// </summary>
        /// <param name="name"> The old name. </param>
        /// <param name="newName"> The new name. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected virtual void Rename(
            string name,
            string newName,
            MigrationCommandListBuilder builder)
            => Rename(name, newName, /*type:*/ null, builder);

        /// <summary>
        ///     Generates a rename.
        /// </summary>
        /// <param name="name"> The old name. </param>
        /// <param name="newName"> The new name. </param>
        /// <param name="type"> If not <see langword="null" />, then appends literal for type of object being renamed (e.g. column or index.) </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected virtual void Rename(
            string name,
            string newName,
            string? type,
            MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(newName, nameof(newName));
            Check.NotNull(builder, nameof(builder));

            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            builder
                .Append("EXEC sp_rename ")
                .Append(stringTypeMapping.GenerateSqlLiteral(name))
                .Append(", ")
                .Append(stringTypeMapping.GenerateSqlLiteral(newName));

            if (type != null)
            {
                builder
                    .Append(", ")
                    .Append(stringTypeMapping.GenerateSqlLiteral(type));
            }

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }

        /// <summary>
        ///     Generates a transfer from one schema to another..
        /// </summary>
        /// <param name="newSchema"> The schema to transfer to. </param>
        /// <param name="schema"> The schema to transfer from. </param>
        /// <param name="name"> The name of the item to transfer. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected virtual void Transfer(
            string? newSchema,
            string? schema,
            string name,
            MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(builder, nameof(builder));

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
        ///     Generates a SQL fragment for traits of an index from a <see cref="CreateIndexOperation" />,
        ///     <see cref="AddPrimaryKeyOperation" />, or <see cref="AddUniqueConstraintOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void IndexTraits(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var clustered = operation[SqlServerAnnotationNames.Clustered] as bool?;
            if (clustered.HasValue)
            {
                builder.Append(clustered.Value ? "CLUSTERED " : "NONCLUSTERED ");
            }
        }

        /// <summary>
        ///     Generates a SQL fragment for extras (filter, included columns, options) of an index from a <see cref="CreateIndexOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null" /> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
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

        private void IndexWithOptions(CreateIndexOperation operation, MigrationCommandListBuilder builder)
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
        /// <param name="referentialAction"> The referential action. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void ForeignKeyAction(ReferentialAction referentialAction, MigrationCommandListBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

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
        /// <param name="schema"> The schema that contains the table. </param>
        /// <param name="tableName"> The table that contains the column.</param>
        /// <param name="columnName"> The column. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected virtual void DropDefaultConstraint(
            string? schema,
            string tableName,
            string columnName,
            MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(tableName, nameof(tableName));
            Check.NotEmpty(columnName, nameof(columnName));
            Check.NotNull(builder, nameof(builder));

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
        /// <param name="column"> The column. </param>
        /// <param name="currentOperation"> The operation which may require a rebuild. </param>
        /// <returns> The list of indexes affected. </returns>
        protected virtual IEnumerable<ITableIndex> GetIndexesToRebuild(
            IColumn? column,
            MigrationOperation currentOperation)
        {
            Check.NotNull(currentOperation, nameof(currentOperation));

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
        /// <param name="indexes"> The indexes to drop. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected virtual void DropIndexes(
            IEnumerable<ITableIndex> indexes,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(indexes, nameof(indexes));
            Check.NotNull(builder, nameof(builder));

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
        /// <param name="indexes"> The indexes to create. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected virtual void CreateIndexes(
            IEnumerable<ITableIndex> indexes,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(indexes, nameof(indexes));
            Check.NotNull(builder, nameof(builder));

            foreach (var index in indexes)
            {
                Generate(CreateIndexOperation.CreateFrom(index), index.Table.Model.Model, builder, terminate: false);
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }
        }

        /// <summary>
        ///     <para>
        ///         Generates add commands for descriptions on tables and columns.
        ///     </para>
        /// </summary>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="description"> The new description to be applied. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <param name="table"> The name of the table. </param>
        /// <param name="column"> The name of the column. </param>
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
        ///     <para>
        ///         Generates drop commands for descriptions on tables and columns.
        ///     </para>
        /// </summary>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <param name="table"> The name of the table. </param>
        /// <param name="column"> The name of the column. </param>
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
        /// <param name="operation"> The index creation operation. </param>
        /// <param name="model"> The target model. </param>
        /// <returns> <see langword="true" /> if a filter should be generated. </returns>
        protected virtual bool UseLegacyIndexFilters(CreateIndexOperation operation, IModel? model)
            => (!TryGetVersion(model, out var version) || VersionComparer.Compare(version, "2.0.0") < 0)
                && operation.Filter is null
                && operation.IsUnique
                && operation[SqlServerAnnotationNames.Clustered] is null or false
                && model?.GetRelationalModel().FindTable(operation.Table, operation.Schema) is var table
                && operation.Columns.Any(c => table?.FindColumn(c)?.IsNullable != false);

        /// <summary>
        ///     Checks whether or not <see cref="CreateIndexOperation" /> should have a filter generated for it by
        ///     Migrations.
        /// </summary>
        /// <param name="model"> The target model. </param>
        /// <returns> <see langword="true" /> if a filter should be generated. </returns>
        [Obsolete("Use UseLegacyIndexFilters which accepts a CreateIndexOperation")]
        protected virtual bool UseLegacyIndexFilters(IModel? model)
            => !TryGetVersion(model, out var version) || VersionComparer.Compare(version, "2.0.0") < 0;

        private string IntegerConstant(long value)
            => string.Format(CultureInfo.InvariantCulture, "{0}", value);

        private bool IsMemoryOptimized(Annotatable annotatable, IModel? model, string? schema, string tableName)
            => annotatable[SqlServerAnnotationNames.MemoryOptimized] as bool?
                ?? model?.GetRelationalModel().FindTable(tableName, schema)?[SqlServerAnnotationNames.MemoryOptimized] as bool? == true;

        private static bool IsMemoryOptimized(Annotatable annotatable)
            => annotatable[SqlServerAnnotationNames.MemoryOptimized] as bool? == true;

        private static bool IsIdentity(ColumnOperation operation)
            => operation[SqlServerAnnotationNames.Identity] != null
                || operation[SqlServerAnnotationNames.ValueGenerationStrategy] as SqlServerValueGenerationStrategy?
                == SqlServerValueGenerationStrategy.IdentityColumn;

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
            IModel? model)
        {
            var operations = new List<MigrationOperation>();

            var versioningMap = new Dictionary<(string?, string?), (string, string?)>();
            var periodMap = new Dictionary<(string?, string?), (string, string)>();

            foreach (var operation in migrationOperations)
            {
                var isTemporal = operation[SqlServerAnnotationNames.IsTemporal] as bool? == true;
                if (isTemporal)
                {
                    string? table = null;
                    string? schema = null;

                    if (operation is ITableMigrationOperation tableMigrationOperation)
                    {
                        table = tableMigrationOperation.Table;
                        schema = tableMigrationOperation.Schema;
                    }

                    schema ??= model?.GetDefaultSchema();
                    var historyTableName = operation[SqlServerAnnotationNames.TemporalHistoryTableName] as string;
                    var historyTableSchema = operation[SqlServerAnnotationNames.TemporalHistoryTableSchema] as string ?? model?.GetDefaultSchema();
                    var periodStartColumnName = operation[SqlServerAnnotationNames.TemporalPeriodStartColumnName] as string;
                    var periodEndColumnName = operation[SqlServerAnnotationNames.TemporalPeriodEndColumnName] as string;

                    switch (operation)
                    {
                        case DropTableOperation dropTableOperation:
                            DisableVersioning(table!, schema, historyTableName!, historyTableSchema);
                            operations.Add(operation);

                            versioningMap.Remove((table, schema));
                            periodMap.Remove((table, schema));
                            break;

                        case RenameTableOperation renameTableOperation:
                            DisableVersioning(table!, schema, historyTableName!, historyTableSchema);
                            operations.Add(operation);

                            // since table was renamed, remove old entry and add new entry
                            // marked as versioning disabled, so we enable it in the end for the new table
                            versioningMap.Remove((table, schema));
                            versioningMap[(renameTableOperation.NewName, renameTableOperation.NewSchema)] = (historyTableName!, historyTableSchema);

                            // same thing for disabled system period - remove one associated with old table and add one for the new table
                            if (periodMap.TryGetValue((table, schema), out var result))
                            {
                                periodMap.Remove((table, schema));
                                periodMap[(renameTableOperation.NewName, renameTableOperation.NewSchema)] = result;
                            }

                            break;

                        case AlterTableOperation alterTableOperation:
                            var oldIsTemporal = alterTableOperation.OldTable[SqlServerAnnotationNames.IsTemporal] as bool? == true;
                            if (!oldIsTemporal)
                            {
                                periodMap[(alterTableOperation.Name, alterTableOperation.Schema)] = (periodStartColumnName!, periodEndColumnName!);
                                versioningMap[(alterTableOperation.Name, alterTableOperation.Schema)] = (historyTableName!, historyTableSchema);
                            }
                            else
                            {
                                var oldHistoryTableName = alterTableOperation.OldTable[SqlServerAnnotationNames.TemporalHistoryTableName] as string;
                                var oldHistoryTableSchema = alterTableOperation.OldTable[SqlServerAnnotationNames.TemporalHistoryTableSchema] as string
                                    ?? alterTableOperation.OldTable.Schema
                                    ?? model?[RelationalAnnotationNames.DefaultSchema] as string;

                                if (oldHistoryTableName != historyTableName
                                    || oldHistoryTableSchema != historyTableSchema)
                                {
                                    if (historyTableSchema != null)
                                    {
                                        operations.Add(new EnsureSchemaOperation { Name = historyTableSchema });
                                    }

                                    operations.Add(new RenameTableOperation
                                    {
                                        Name = oldHistoryTableName!,
                                        Schema = oldHistoryTableSchema,
                                        NewName = historyTableName,
                                        NewSchema = historyTableSchema
                                    });

                                    if (versioningMap.ContainsKey((alterTableOperation.Name, alterTableOperation.Schema)))
                                    {
                                        versioningMap[(alterTableOperation.Name, alterTableOperation.Schema)] = (historyTableName!, historyTableSchema);
                                    }
                                }
                            }

                            operations.Add(operation);
                            break;

                        case AlterColumnOperation alterColumnOperation:
                            // if only difference is in temporal annotations being removed or history table changed etc - we can ignore this operation
                            if (!CanSkipAlterColumnOperation(alterColumnOperation.OldColumn, alterColumnOperation))
                            {
                                // when modifying a period column, we need to perform the operations as a normal column first, and only later enable period
                                // removing the period information now, so that when we generate SQL that modifies the column we won't be making them auto generated as period
                                // (making column auto generated is not allowed in ALTER COLUMN statement)
                                // in later operation we enable the period and the period columns get set to auto generated automatically
                                if (alterColumnOperation[SqlServerAnnotationNames.IsTemporal] as bool? == true
                                    && alterColumnOperation.OldColumn[SqlServerAnnotationNames.IsTemporal] is null)
                                {
                                    alterColumnOperation.RemoveAnnotation(SqlServerAnnotationNames.IsTemporal);
                                    alterColumnOperation.RemoveAnnotation(SqlServerAnnotationNames.TemporalPeriodStartColumnName);
                                    alterColumnOperation.RemoveAnnotation(SqlServerAnnotationNames.TemporalPeriodEndColumnName);

                                    // TODO: test what happens if default value just changes (from temporal to temporal)
                                }

                                operations.Add(operation);
                            }
                            break;

                        case DropPrimaryKeyOperation:
                        case AddPrimaryKeyOperation:
                            DisableVersioning(table!, schema, historyTableName!, historyTableSchema);
                            operations.Add(operation);
                            break;

                        case DropColumnOperation dropColumnOperation:
                            DisableVersioning(table!, schema, historyTableName!, historyTableSchema);
                            if (dropColumnOperation.Name == periodStartColumnName
                                || dropColumnOperation.Name == periodEndColumnName)
                            {
                                // period columns can be null here - it doesn't really matter since we are never enabling the period back
                                // if we remove the period columns, it means we will be dropping the table also or at least convert it back to regular
                                // which will clear the entry in the periodMap for this table
                                DisablePeriod(table!, schema, periodStartColumnName!, periodEndColumnName!);
                            }

                            operations.Add(operation);

                            break;

                        case AddColumnOperation addColumnOperation:
                            // when adding a period column, we need to add it as a normal column first, and only later enable period
                            // removing the period information now, so that when we generate SQL that adds the column we won't be making them auto generated as period
                            // it won't work, unless period is enabled
                            // but we can't enable period without adding the columns first - chicken and egg
                            if (addColumnOperation[SqlServerAnnotationNames.IsTemporal] as bool? == true)
                            {
                                addColumnOperation.RemoveAnnotation(SqlServerAnnotationNames.IsTemporal);
                                addColumnOperation.RemoveAnnotation(SqlServerAnnotationNames.TemporalPeriodStartColumnName);
                                addColumnOperation.RemoveAnnotation(SqlServerAnnotationNames.TemporalPeriodEndColumnName);

                                // model differ adds default value, but for period end we need to replace it with the correct one - DateTime.MaxValue
                                if (addColumnOperation.Name == periodEndColumnName)
                                {
                                    addColumnOperation.DefaultValue = DateTime.MaxValue;
                                }
                            }

                            operations.Add(addColumnOperation);
                            break;

                        default:
                            // CreateTableOperation
                            // RenameColumnOperation
                            operations.Add(operation);
                            break;
                    }
                }
                else
                {
                    if (operation is AlterTableOperation alterTableOperation
                        && alterTableOperation.OldTable[SqlServerAnnotationNames.IsTemporal] as bool? == true)
                    {
                        var historyTableName = alterTableOperation.OldTable[SqlServerAnnotationNames.TemporalHistoryTableName] as string;
                        var historyTableSchema = alterTableOperation.OldTable[SqlServerAnnotationNames.TemporalHistoryTableSchema] as string
                            ?? alterTableOperation.OldTable.Schema
                            ?? model?[RelationalAnnotationNames.DefaultSchema] as string;

                        var periodStartColumnName = alterTableOperation.OldTable[SqlServerAnnotationNames.TemporalPeriodStartColumnName] as string;
                        var periodEndColumnName = alterTableOperation.OldTable[SqlServerAnnotationNames.TemporalPeriodEndColumnName] as string;

                        DisableVersioning(alterTableOperation.Name, alterTableOperation.Schema, historyTableName!, historyTableSchema);
                        DisablePeriod(alterTableOperation.Name, alterTableOperation.Schema, periodStartColumnName!, periodEndColumnName!);

                        if (historyTableName != null)
                        {
                            operations.Add(
                                new DropTableOperation
                                {
                                    Name = historyTableName,
                                    Schema = alterTableOperation.OldTable.Schema
                                });
                        }

                        operations.Add(operation);

                        // when we disable versioning and period earlier, we marked it to be re-enabled
                        // since table is no longer temporal we don't need to do that anymore
                        versioningMap.Remove((alterTableOperation.Name, alterTableOperation.Schema));
                        periodMap.Remove((alterTableOperation.Name, alterTableOperation.Schema));
                    }
                    else if (operation is AlterColumnOperation alterColumnOperation)
                    {
                        // if only difference is in temporal annotations being removed or history table changed etc - we can ignore this operation
                        if (alterColumnOperation.OldColumn?[SqlServerAnnotationNames.IsTemporal] as bool? != true
                            || !CanSkipAlterColumnOperation(alterColumnOperation.OldColumn, alterColumnOperation))
                        {
                            operations.Add(operation);
                        }
                    }
                    else
                    {
                        operations.Add(operation);
                    }
                }
            }

            foreach (var periodMapEntry in periodMap)
            {
                EnablePeriod(periodMapEntry.Key.Item1!, periodMapEntry.Key.Item2, periodMapEntry.Value.Item1, periodMapEntry.Value.Item2);
            }

            foreach (var versioningMapEntry in versioningMap)
            {
                EnableVersioning(versioningMapEntry.Key.Item1!, versioningMapEntry.Key.Item2, versioningMapEntry.Value.Item1, versioningMapEntry.Value.Item2);
            }

            return operations;

            void DisableVersioning(string table, string? schema, string historyTableName, string? historyTableSchema)
            {
                if (!versioningMap.TryGetValue((table, schema), out var result))
                {
                    versioningMap[(table, schema)] = (historyTableName, historyTableSchema);

                    operations.Add(
                        new SqlOperation
                        {
                            Sql = new StringBuilder()
                                .Append("ALTER TABLE ")
                                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(table, schema))
                                .AppendLine(" SET (SYSTEM_VERSIONING = OFF)")
                                .ToString()
                        });
                }
            }

            void EnableVersioning(string table, string? schema, string historyTableName, string? historyTableSchema)
            {
                var stringBuilder = new StringBuilder();

                if (historyTableSchema == null)
                {
                    // need to run command using EXEC to inject default schema
                    stringBuilder.AppendLine("DECLARE @historyTableSchema sysname = SCHEMA_NAME()");
                    stringBuilder.Append("EXEC(N'");
                }

                var historyTable = default(string);
                if (historyTableSchema != null)
                {
                    historyTable = Dependencies.SqlGenerationHelper.DelimitIdentifier(historyTableName, historyTableSchema);
                }
                else
                {
                    historyTable = Dependencies.SqlGenerationHelper.DelimitIdentifier(historyTableName);
                }

                stringBuilder
                    .Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(table, schema));

                if (historyTableSchema != null)
                {
                    stringBuilder.AppendLine($" SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = {historyTable}))");
                }
                else
                {
                    stringBuilder.AppendLine($" SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].{historyTable}))')");
                }

                operations.Add(
                    new SqlOperation
                    {
                        Sql = stringBuilder.ToString()
                    });
            }

            void DisablePeriod(string table, string? schema, string periodStartColumnName, string periodEndColumnName)
            {
                if (!periodMap.TryGetValue((table, schema), out var result))
                {
                    periodMap[(table, schema)] = (periodStartColumnName, periodEndColumnName);

                    operations.Add(
                        new SqlOperation
                        {
                            Sql = new StringBuilder()
                                .Append("ALTER TABLE ")
                                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(table, schema))
                                .AppendLine(" DROP PERIOD FOR SYSTEM_TIME")
                                .ToString()
                        });
                }
            }

            void EnablePeriod(string table, string? schema, string periodStartColumnName, string periodEndColumnName)
            {
                operations.Add(
                    new SqlOperation
                    {
                        Sql = new StringBuilder()
                            .Append("ALTER TABLE ")
                            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(table, schema))
                            .Append(" ADD PERIOD FOR SYSTEM_TIME (")
                            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(periodStartColumnName))
                            .Append(", ")
                            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(periodEndColumnName))
                            .Append(")")
                            .ToString()
                    });
            }

            static bool CanSkipAlterColumnOperation(ColumnOperation first, ColumnOperation second)
                => ColumnPropertiesAreTheSame(first, second)
                    && ColumnOperationsOnlyDifferByTemporalTableAnnotation(first, second)
                    && ColumnOperationsOnlyDifferByTemporalTableAnnotation(second, first);

            static bool ColumnPropertiesAreTheSame(ColumnOperation first, ColumnOperation second)
                => first.ClrType == second.ClrType
                    && first.Collation == second.Collation
                    && first.ColumnType == second.ColumnType
                    && first.Comment == second.Comment
                    && first.ComputedColumnSql == second.ComputedColumnSql
                    && Equals(first.DefaultValue, second.DefaultValue)
                    && first.DefaultValueSql == second.DefaultValueSql
                    && first.IsDestructiveChange == second.IsDestructiveChange
                    && first.IsFixedLength == second.IsFixedLength
                    && first.IsNullable == second.IsNullable
                    && first.IsReadOnly == second.IsReadOnly
                    && first.IsRowVersion == second.IsRowVersion
                    && first.IsStored == second.IsStored
                    && first.IsUnicode == second.IsUnicode
                    && first.MaxLength == second.MaxLength
                    && first.Precision == second.Precision
                    && first.Scale == second.Scale;

            static bool ColumnOperationsOnlyDifferByTemporalTableAnnotation(ColumnOperation first, ColumnOperation second)
            {
                var unmatched = first.GetAnnotations().ToList();
                foreach (var annotation in second.GetAnnotations())
                {
                    var index = unmatched.FindIndex(
                        a => a.Name == annotation.Name && StructuralComparisons.StructuralEqualityComparer.Equals(a.Value, annotation.Value));
                    if (index == -1)
                    {
                        continue;
                    }

                    unmatched.RemoveAt(index);
                }

                return unmatched.All(a => a.Name == SqlServerAnnotationNames.IsTemporal
                    || a.Name == SqlServerAnnotationNames.TemporalHistoryTableName
                    || a.Name == SqlServerAnnotationNames.TemporalHistoryTableSchema
                    || a.Name == SqlServerAnnotationNames.TemporalPeriodStartPropertyName
                    || a.Name == SqlServerAnnotationNames.TemporalPeriodEndPropertyName
                    || a.Name == SqlServerAnnotationNames.TemporalPeriodStartColumnName
                    || a.Name == SqlServerAnnotationNames.TemporalPeriodEndColumnName);
            }
        }
    }
}
