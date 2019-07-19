// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
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
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class SqlServerMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        private readonly IMigrationsAnnotationProvider _migrationsAnnotations;

        private IReadOnlyList<MigrationOperation> _operations;
        private int _variableCounter;

        /// <summary>
        ///     Creates a new <see cref="SqlServerMigrationsSqlGenerator"/> instance.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        /// <param name="migrationsAnnotations"> Provider-specific Migrations annotations to use. </param>
        public SqlServerMigrationsSqlGenerator(
            [NotNull] MigrationsSqlGeneratorDependencies dependencies,
            [NotNull] IMigrationsAnnotationProvider migrationsAnnotations)
            : base(dependencies) => _migrationsAnnotations = migrationsAnnotations;

        /// <summary>
        ///     Generates commands from a list of operations.
        /// </summary>
        /// <param name="operations"> The operations. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <returns> The list of commands to be executed or scripted. </returns>
        public override IReadOnlyList<MigrationCommand> Generate(IReadOnlyList<MigrationOperation> operations, IModel model)
        {
            _operations = operations;
            try
            {
                return base.Generate(operations, model);
            }
            finally
            {
                _operations = null;
            }
        }

        /// <summary>
        ///     <para>
        ///         Builds commands for the given <see cref="MigrationOperation" /> by making calls on the given
        ///         <see cref="MigrationCommandListBuilder" />.
        ///     </para>
        ///     <para>
        ///         This method uses a double-dispatch mechanism to call one of the 'Generate' methods that are
        ///         specific to a certain subtype of <see cref="MigrationOperation" />. Typically database providers
        ///         will override these specific methods rather than this method. However, providers can override
        ///         this methods to handle provider-specific operations.
        ///     </para>
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(MigrationOperation operation, IModel model, MigrationCommandListBuilder builder)
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

        /// <summary>
        ///     Builds commands for the given <see cref="AddColumnOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            AddColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate)
        {

            var valueGenerationStrategy = operation[
                SqlServerAnnotationNames.ValueGenerationStrategy] as SqlServerValueGenerationStrategy?;
            var identity = valueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn;
            if (identity)
            {
                // NB: This gets added to all added non-nullable columns by MigrationsModelDiffer. We need to suppress
                //     it, here because SQL Server can have both IDENTITY and a DEFAULT constraint on the same column.
                operation.DefaultValue = null;
            }

            base.Generate(operation, model, builder, terminate: false);

            if (terminate)
            {
                builder
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                    .EndCommand(suppressTransaction: IsMemoryOptimized(operation, model, operation.Schema, operation.Table));
            }
        }

        /// <summary>
        ///     Builds commands for the given <see cref="AddForeignKeyOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            AddForeignKeyOperation operation,
            IModel model,
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            AddPrimaryKeyOperation operation,
            IModel model,
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            AlterColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            IEnumerable<IIndex> indexesToRebuild = null;
            var property = FindProperty(model, operation.Schema, operation.Table, operation.Name);

            if (operation.ComputedColumnSql != null)
            {
                var dropColumnOperation = new DropColumnOperation
                {
                    Schema = operation.Schema,
                    Table = operation.Table,
                    Name = operation.Name
                };
                if (property != null)
                {
                    dropColumnOperation.AddAnnotations(_migrationsAnnotations.ForRemove(property));
                }

                var addColumnOperation = new AddColumnOperation
                {
                    Schema = operation.Schema,
                    Table = operation.Table,
                    Name = operation.Name,
                    ClrType = operation.ClrType,
                    ColumnType = operation.ColumnType,
                    IsUnicode = operation.IsUnicode,
                    MaxLength = operation.MaxLength,
                    IsRowVersion = operation.IsRowVersion,
                    IsNullable = operation.IsNullable,
                    DefaultValue = operation.DefaultValue,
                    DefaultValueSql = operation.DefaultValueSql,
                    ComputedColumnSql = operation.ComputedColumnSql,
                    IsFixedLength = operation.IsFixedLength
                };
                addColumnOperation.AddAnnotations(operation.GetAnnotations());

                // TODO: Use a column rebuild instead
                indexesToRebuild = GetIndexesToRebuild(property, operation).ToList();
                DropIndexes(indexesToRebuild, builder);
                Generate(dropColumnOperation, model, builder, terminate: false);
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                Generate(addColumnOperation, model, builder, terminate: false);
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                CreateIndexes(indexesToRebuild, builder);
                builder.EndCommand(suppressTransaction: IsMemoryOptimized(operation, model, operation.Schema, operation.Table));

                return;
            }

            var narrowed = false;
            if (IsOldColumnSupported(model))
            {
                var valueGenerationStrategy = operation[
                    SqlServerAnnotationNames.ValueGenerationStrategy] as SqlServerValueGenerationStrategy?;
                var identity = valueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn;
                var oldValueGenerationStrategy = operation.OldColumn[
                    SqlServerAnnotationNames.ValueGenerationStrategy] as SqlServerValueGenerationStrategy?;
                var oldIdentity = oldValueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn;
                if (identity != oldIdentity)
                {
                    throw new InvalidOperationException(SqlServerStrings.AlterIdentityColumn);
                }

                var type = operation.ColumnType
                           ?? GetColumnType(
                               operation.Schema,
                               operation.Table,
                               operation.Name,
                               operation,
                               model);
                var oldType = operation.OldColumn.ColumnType
                              ?? GetColumnType(
                                  operation.Schema,
                                  operation.Table,
                                  operation.Name,
                                  operation.OldColumn,
                                  model);
                narrowed = type != oldType || !operation.IsNullable && operation.OldColumn.IsNullable;
            }

            if (narrowed)
            {
                indexesToRebuild = GetIndexesToRebuild(property, operation).ToList();
                DropIndexes(indexesToRebuild, builder);
            }

            DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ALTER COLUMN ");

            // NB: DefaultValue, DefaultValueSql, and identity are handled elsewhere. Don't copy them here.
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
                IsRowVersion = operation.IsRowVersion,
                IsNullable = operation.IsNullable,
                ComputedColumnSql = operation.ComputedColumnSql,
                OldColumn = operation.OldColumn
            };
            definitionOperation.AddAnnotations(
                operation.GetAnnotations().Where(a => a.Name != SqlServerAnnotationNames.ValueGenerationStrategy));

            ColumnDefinition(
                operation.Schema,
                operation.Table,
                operation.Name,
                definitionOperation,
                model,
                builder);

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            if (operation.DefaultValue != null
                || operation.DefaultValueSql != null)
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append(" ADD");
                DefaultValue(operation.DefaultValue, operation.DefaultValueSql, builder);
                builder
                    .Append(" FOR ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }

            if (operation.OldColumn.Comment != operation.Comment)
            {
                if (operation.OldColumn.Comment != null)
                {
                    EndStatement(builder);

                    GenerateDropExtendedProperty(builder, model, "Comment", operation.Schema, operation.Table, operation.Name);
                }

                GenerateComment(operation, model, builder, operation.Comment, operation.Schema, operation.Table, operation.Name);
            }

            if (narrowed)
            {
                CreateIndexes(indexesToRebuild, builder);
            }

            builder.EndCommand(suppressTransaction: IsMemoryOptimized(operation, model, operation.Schema, operation.Table));
        }

        /// <summary>
        ///     Builds commands for the given <see cref="RenameIndexOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            RenameIndexOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (string.IsNullOrEmpty(operation.Table))
            {
                throw new InvalidOperationException(SqlServerStrings.IndexTableRequired);
            }

            Rename(
                Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema) +
                "." +
                Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name),
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(RenameSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            RestartSequenceOperation operation,
            IModel model,
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            CreateTableOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            base.Generate(operation, model, builder, terminate: false);

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

            if (terminate)
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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            RenameTableOperation operation,
            IModel model,
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            DropTableOperation operation,
            IModel model,
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
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            CreateIndexOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var nullableColumns = operation.Columns
                .Where(
                    c =>
                    {
                        var property = FindProperty(model, operation.Schema, operation.Table, c);

                        return property?.IsColumnNullable() != false;
                    })
                .ToList();

            var memoryOptimized = IsMemoryOptimized(operation, model, operation.Schema, operation.Table);
            if (memoryOptimized)
            {
                builder.Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append(" ADD INDEX ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" ");

                if (operation.IsUnique
                    && nullableColumns.Count == 0)
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
                base.Generate(operation, model, builder, terminate: false);

                if (operation.Filter == null
                    && UseLegacyIndexFilters(model))
                {
                    var clustered = operation[SqlServerAnnotationNames.Clustered] as bool?;
                    if (operation.IsUnique
                        && (clustered != true)
                        && nullableColumns.Count != 0)
                    {
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            DropPrimaryKeyOperation operation,
            IModel model,
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(EnsureSchemaOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (string.Equals(operation.Name, "DBO", StringComparison.OrdinalIgnoreCase))
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
                        "CREATE SCHEMA " +
                        Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name) +
                        Dependencies.SqlGenerationHelper.StatementTerminator))
                .Append(")")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand();
        }

        /// <summary>
        ///     Builds commands for the given <see cref="CreateSequenceOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            CreateSequenceOperation operation,
            IModel model,
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected virtual void Generate(
            [NotNull] SqlServerCreateDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected virtual void Generate(
            [NotNull] SqlServerDropDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            AlterDatabaseOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (!IsMemoryOptimized(operation))
            {
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
                        .AppendLine("DECLARE @db_name NVARCHAR(MAX) = DB_NAME();")
                        .AppendLine("DECLARE @fg_name NVARCHAR(MAX);")
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
                        .AppendLine("DECLARE @path NVARCHAR(MAX);")
                        .Append("SELECT TOP(1) @path = [physical_name] FROM [sys].[database_files] ")
                        .AppendLine("WHERE charindex('\\', [physical_name]) > 0 ORDER BY [file_id];")
                        .AppendLine("IF (@path IS NULL)")
                        .IncrementIndent().AppendLine("SET @path = '\\' + @db_name;").DecrementIndent()
                        .AppendLine()
                        .AppendLine("DECLARE @filename NVARCHAR(MAX) = right(@path, charindex('\\', reverse(@path)) - 1);")
                        .AppendLine("SET @filename = REPLACE(left(@filename, len(@filename) - charindex('.', reverse(@filename))), '''', '''''') + N'_MOD';")
                        .AppendLine("DECLARE @new_path NVARCHAR(MAX) = REPLACE(CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS NVARCHAR(MAX)), '''', '''''') + @filename;")
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(AlterTableOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            if (IsMemoryOptimized(operation)
                ^ IsMemoryOptimized(operation.OldTable))
            {
                throw new InvalidOperationException(SqlServerStrings.AlterMemoryOptimizedTable);
            }

            base.Generate(operation, model, builder);
        }

        /// <summary>
        ///     Builds commands for the given <see cref="DropForeignKeyOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            DropForeignKeyOperation operation,
            IModel model,
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            DropIndexOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var memoryOptimized = IsMemoryOptimized(operation, model, operation.Schema, operation.Table);
            if (memoryOptimized)
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            DropColumnOperation operation,
            IModel model,
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
        }

        /// <summary>
        ///     Builds commands for the given <see cref="RenameColumnOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            RenameColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            Rename(
                Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema) +
                "." +
                Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name),
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(SqlOperation operation, IModel model, MigrationCommandListBuilder builder)
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            InsertDataOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            GenerateIdentityInsert(builder, operation, on: true);

            var sqlBuilder = new StringBuilder();
            ((SqlServerUpdateSqlGenerator)Dependencies.UpdateSqlGenerator).AppendBulkInsertOperation(
                sqlBuilder,
                operation.GenerateModificationCommands(model).ToList(),
                0);

            builder.Append(sqlBuilder.ToString());

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

        /// <summary>
        ///     Generates a SQL fragment configuring a sequence with the given options.
        /// </summary>
        /// <param name="schema"> The schema that contains the sequence, or <c>null</c> to use the default schema. </param>
        /// <param name="name"> The sequence name. </param>
        /// <param name="operation"> The sequence options. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void SequenceOptions(
            string schema,
            string name,
            SequenceOperation operation,
            IModel model,
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
        /// <param name="schema"> The schema that contains the table, or <c>null</c> to use the default schema. </param>
        /// <param name="table"> The table that contains the column. </param>
        /// <param name="name"> The column name. </param>
        /// <param name="operation"> The column metadata. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void ColumnDefinition(
            string schema,
            string table,
            string name,
            ColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            base.ColumnDefinition(
                schema,
                table,
                name,
                operation,
                model,
                builder);

            var valueGenerationStrategy = operation[
                SqlServerAnnotationNames.ValueGenerationStrategy] as SqlServerValueGenerationStrategy?;
            var identity = valueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn;
            if (identity)
            {
                var identitySeed = operation[
                    SqlServerAnnotationNames.IdentitySeed] as int?;

                var identityIncrement = operation[
                    SqlServerAnnotationNames.IdentityIncrement] as int?;

                builder.Append(" IDENTITY");

                if ((identitySeed != null && identitySeed != 1) || (identityIncrement != null && identityIncrement != 1))
                {
                    builder.Append($"({identitySeed ?? 1},{identityIncrement ?? 1})");
                }
            }
        }

        /// <summary>
        ///     Generates a SQL fragment for a computed column definition for the given column metadata.
        /// </summary>
        /// <param name="schema"> The schema that contains the table, or <c>null</c> to use the default schema. </param>
        /// <param name="table"> The table that contains the column. </param>
        /// <param name="name"> The column name. </param>
        /// <param name="operation"> The column metadata. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void ComputedColumnDefinition(
            string schema,
            string table,
            string name,
            ColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name))
                .Append(" AS ")
                .Append(operation.ComputedColumnSql);
        }

        /// <summary>
        ///     Generates a rename.
        /// </summary>
        /// <param name="name"> The old name. </param>
        /// <param name="newName"> The new name. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected virtual void Rename(
            [NotNull] string name,
            [NotNull] string newName,
            [NotNull] MigrationCommandListBuilder builder) => Rename(name, newName, /*type:*/ null, builder);

        /// <summary>
        ///     Generates a rename.
        /// </summary>
        /// <param name="name"> The old name. </param>
        /// <param name="newName"> The new name. </param>
        /// <param name="type"> If not <c>null</c>, then appends literal for type of object being renamed (e.g. column or index.) </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected virtual void Rename(
            [NotNull] string name,
            [NotNull] string newName,
            [CanBeNull] string type,
            [NotNull] MigrationCommandListBuilder builder)
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
            [CanBeNull] string newSchema,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] MigrationCommandListBuilder builder)
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void IndexTraits(MigrationOperation operation, IModel model, MigrationCommandListBuilder builder)
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
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void IndexOptions(CreateIndexOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            if (operation[SqlServerAnnotationNames.Include] is IReadOnlyList<string> includeProperties
                && includeProperties.Count > 0)
            {
                builder.Append(" INCLUDE (");
                for (var i = 0; i < includeProperties.Count; i++)
                {
                    builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(includeProperties[i]));

                    if (i != includeProperties.Count - 1)
                    {
                        builder.Append(", ");
                    }
                }

                builder.Append(")");
            }

            base.IndexOptions(operation, model, builder);

            if (operation[SqlServerAnnotationNames.CreatedOnline] is bool isOnline && isOnline)
            {
                builder.Append(" WITH (ONLINE = ON)");
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
            [CanBeNull] string schema,
            [NotNull] string tableName,
            [NotNull] string columnName,
            [NotNull] MigrationCommandListBuilder builder)
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
                .AppendLine("INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]")
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
                        "ALTER TABLE " +
                        Dependencies.SqlGenerationHelper.DelimitIdentifier(tableName, schema) +
                        " DROP CONSTRAINT ["))
                .Append(" + ")
                .Append(variable)
                .Append(" + ']")
                .Append(Dependencies.SqlGenerationHelper.StatementTerminator)
                .Append("')")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }

        /// <summary>
        ///     Gets the list of indexes that need to be rebuilt when the given property is changing.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="currentOperation"> The operation which may require a rebuild. </param>
        /// <returns> The list of indexes affected. </returns>
        protected virtual IEnumerable<IIndex> GetIndexesToRebuild(
            [CanBeNull] IProperty property,
            [NotNull] MigrationOperation currentOperation)
        {
            Check.NotNull(currentOperation, nameof(currentOperation));

            if (property == null)
            {
                yield break;
            }

            var createIndexOperations = _operations.SkipWhile(o => o != currentOperation).Skip(1)
                .OfType<CreateIndexOperation>().ToList();
            foreach (var index in property.DeclaringEntityType.GetIndexes().Concat(property.DeclaringEntityType.GetDerivedTypes().SelectMany(et => et.GetDeclaredIndexes())))
            {
                var indexName = index.GetName();
                if (createIndexOperations.Any(o => o.Name == indexName))
                {
                    continue;
                }

                if (index.Properties.Any(p => p == property))
                {
                    yield return index;
                }
                else if (index.GetSqlServerIncludeProperties() is IReadOnlyList<string> includeProperties)
                {
                    if (includeProperties.Contains(property.Name))
                    {
                        yield return index;
                    }
                }
            }
        }

        /// <summary>
        ///     Generates SQL to drop the given indexes.
        /// </summary>
        /// <param name="indexes"> The indexes to drop. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected virtual void DropIndexes(
            [NotNull] IEnumerable<IIndex> indexes,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(indexes, nameof(indexes));
            Check.NotNull(builder, nameof(builder));

            foreach (var index in indexes)
            {
                var operation = new DropIndexOperation
                {
                    Schema = index.DeclaringEntityType.GetSchema(),
                    Table = index.DeclaringEntityType.GetTableName(),
                    Name = index.GetName()
                };
                operation.AddAnnotations(_migrationsAnnotations.ForRemove(index));

                Generate(operation, index.DeclaringEntityType.Model, builder, terminate: false);
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }
        }

        /// <summary>
        ///     Generates SQL to create the given indexes.
        /// </summary>
        /// <param name="indexes"> The indexes to create. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected virtual void CreateIndexes(
            [NotNull] IEnumerable<IIndex> indexes,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(indexes, nameof(indexes));
            Check.NotNull(builder, nameof(builder));

            foreach (var index in indexes)
            {
                var operation = new CreateIndexOperation
                {
                    IsUnique = index.IsUnique,
                    Name = index.GetName(),
                    Schema = index.DeclaringEntityType.GetSchema(),
                    Table = index.DeclaringEntityType.GetTableName(),
                    Columns = index.Properties.Select(p => p.GetColumnName()).ToArray(),
                    Filter = index.GetFilter()
                };
                operation.AddAnnotations(_migrationsAnnotations.For(index));

                Generate(operation, index.DeclaringEntityType.Model, builder, terminate: false);
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }
        }

        /// <summary>
        ///     Generates SQL to create comment extended properties on table and columns.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="comment"> The comment to be applied. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <param name="table"> The name of the table. </param>
        /// <param name="columnName"> The column name if comment is being applied to a column. </param>
        protected override void GenerateComment(
            MigrationOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            string comment,
            string schema,
            string table,
            string columnName = null)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));
            Check.NotNull(table, nameof(table));

            if (comment != null)
            {
                EndStatement(builder);

                GenerateCreateExtendedProperty(builder, model, "Comment", comment, schema, table, columnName);
            }
        }

        /// <summary>
        ///     Generates SQL to create a extended property on table and columns.
        /// </summary>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="name"> The name of the extended property. </param>
        /// <param name="value"> The value of the extended property. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <param name="table"> The name of the table. </param>
        /// <param name="columnName"> The column name if comment is being applied to a column. </param>
        protected virtual void GenerateCreateExtendedProperty(
            [NotNull] MigrationCommandListBuilder builder,
            [CanBeNull] IModel model,
            [NotNull] string name,
            [NotNull] string value,
            [CanBeNull] string schema,
            [NotNull] string table,
            [CanBeNull] string columnName = null)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NotNull(name, nameof(name));
            Check.NotNull(value, nameof(value));
            Check.NotNull(table, nameof(table));

            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            builder
                .Append("EXEC sp_addextendedproperty @name = ")
                .Append(stringTypeMapping.GenerateSqlLiteral(name))
                .Append(", @value = ")
                .Append(stringTypeMapping.GenerateSqlLiteral(value))
                .Append(", @level0type = N'Schema', @level0name = ")
                .Append(stringTypeMapping.GenerateSqlLiteral(schema ?? model?.GetDefaultSchema()))
                .Append(", @level1type = N'Table', @level1name = ")
                .Append(stringTypeMapping.GenerateSqlLiteral(table));

            if (columnName != null)
            {
                builder
                    .Append(", @level2type = N'Column', @level2name = ")
                    .Append(stringTypeMapping.GenerateSqlLiteral(columnName));
            }
        }

        /// <summary>
        ///     Generates SQL to drop a extended property on table and columns.
        /// </summary>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="name"> The name of the extended property. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <param name="table"> The name of the table. </param>
        /// <param name="columnName"> The column name if comment is being applied to a column. </param>
        protected virtual void GenerateDropExtendedProperty(
            [NotNull] MigrationCommandListBuilder builder,
            [CanBeNull] IModel model,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] string table,
            [CanBeNull] string columnName = null)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NotNull(name, nameof(name));
            Check.NotNull(table, nameof(table));

            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            builder
                .Append("EXEC sp_dropextendedproperty @name = ")
                .Append(stringTypeMapping.GenerateSqlLiteral(name))
                .Append(", @level0type = N'Schema', @level0name = ")
                .Append(stringTypeMapping.GenerateSqlLiteral(schema ?? model?.GetDefaultSchema()))
                .Append(", @level1type = N'Table', @level1name = ")
                .Append(stringTypeMapping.GenerateSqlLiteral(table));

            if (columnName != null)
            {
                builder
                    .Append(", @level2type = N'Column', @level2name = ")
                    .Append(stringTypeMapping.GenerateSqlLiteral(columnName));
            }
        }

        /// <summary>
        ///     Checks whether or not <see cref="CreateIndexOperation" /> should have a filter generated for it by
        ///     Migrations.
        /// </summary>
        /// <param name="model"> The target model. </param>
        /// <returns> True if a filter should be generated. </returns>
        protected virtual bool UseLegacyIndexFilters([CanBeNull] IModel model)
            => !TryGetVersion(model, out var version) || VersionComparer.Compare(version, "2.0.0") < 0;

        private string IntegerConstant(long value)
            => string.Format(CultureInfo.InvariantCulture, "{0}", value);

        private bool IsMemoryOptimized(Annotatable annotatable, IModel model, string schema, string tableName)
            => annotatable[SqlServerAnnotationNames.MemoryOptimized] as bool?
               ?? FindEntityTypes(model, schema, tableName)?.Any(t => t.GetSqlServerIsMemoryOptimized()) == true;

        private static bool IsMemoryOptimized(Annotatable annotatable)
            => annotatable[SqlServerAnnotationNames.MemoryOptimized] as bool? == true;
    }
}
