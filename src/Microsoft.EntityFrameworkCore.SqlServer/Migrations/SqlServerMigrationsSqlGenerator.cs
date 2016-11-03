// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class SqlServerMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        private readonly IMigrationsAnnotationProvider _migrationsAnnotations;

        private int _variableCounter;

        public SqlServerMigrationsSqlGenerator(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IRelationalAnnotationProvider annotations,
            [NotNull] IMigrationsAnnotationProvider migrationsAnnotations)
            : base(commandBuilderFactory, sqlGenerationHelper, typeMapper, annotations)
        {
            _migrationsAnnotations = migrationsAnnotations;
        }

        protected override void Generate(MigrationOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var createDatabaseOperation = operation as SqlServerCreateDatabaseOperation;
            var dropDatabaseOperation = operation as SqlServerDropDatabaseOperation;
            if (createDatabaseOperation != null)
            {
                Generate(createDatabaseOperation, model, builder);
            }
            else if (dropDatabaseOperation != null)
            {
                Generate(dropDatabaseOperation, model, builder);
            }
            else
            {
                base.Generate(operation, model, builder);
            }
        }

        protected override void Generate(
            AddColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
            => Generate(operation, model, builder, terminate: true);

        protected override void Generate(
            AddColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate)
        {
            base.Generate(operation, model, builder, terminate: false);

            if (terminate)
            {
                builder
                    .AppendLine(SqlGenerationHelper.StatementTerminator)
                    .EndCommand(suppressTransaction: IsMemoryOptimized(operation));
            }
        }

        protected override void Generate(
            AddForeignKeyOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            base.Generate(operation, model, builder, terminate: false);

            builder
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: IsMemoryOptimized(operation));
        }

        protected override void Generate(
            AddPrimaryKeyOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            base.Generate(operation, model, builder, terminate: false);

            builder
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: IsMemoryOptimized(operation));
        }

        protected override void Generate(
            AlterColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

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
                    ComputedColumnSql = operation.ComputedColumnSql
                };
                addColumnOperation.AddAnnotations(operation.GetAnnotations());

                // TODO: Use a column rebuild instead
                DropIndexes(property, builder);
                Generate(dropColumnOperation, model, builder, terminate: false);
                builder.AppendLine(SqlGenerationHelper.StatementTerminator);
                Generate(addColumnOperation, model, builder, terminate: false);
                builder.AppendLine(SqlGenerationHelper.StatementTerminator);
                CreateIndexes(property, builder);
                builder.EndCommand(suppressTransaction: IsMemoryOptimized(operation));

                return;
            }

            var narrowed = false;
            if (IsOldColumnSupported(model))
            {
                var valueGenerationStrategy = operation[
                    SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy] as SqlServerValueGenerationStrategy?;
                var identity = valueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn;
                var oldValueGenerationStrategy = operation.OldColumn[
                    SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy] as SqlServerValueGenerationStrategy?;
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
                               operation.ClrType,
                               operation.IsUnicode,
                               operation.MaxLength,
                               operation.IsRowVersion,
                               model);
                var oldType = operation.OldColumn.ColumnType
                              ?? GetColumnType(
                                  operation.Schema,
                                  operation.Table,
                                  operation.Name,
                                  operation.OldColumn.ClrType,
                                  operation.OldColumn.IsUnicode,
                                  operation.OldColumn.MaxLength,
                                  operation.OldColumn.IsRowVersion,
                                  model);
                narrowed = type != oldType || (!operation.IsNullable && operation.OldColumn.IsNullable);
            }

            if (narrowed)
            {
                DropIndexes(property, builder);
            }

            DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);

            builder
                .Append("ALTER TABLE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ALTER COLUMN ");

            ColumnDefinition(
                operation.Schema,
                operation.Table,
                operation.Name,
                operation.ClrType,
                operation.ColumnType,
                operation.IsUnicode,
                operation.MaxLength,
                operation.IsRowVersion,
                operation.IsNullable,
                /*defaultValue:*/ null,
                /*defaultValueSql:*/ null,
                operation.ComputedColumnSql,
                /*identity:*/ false,
                operation,
                model,
                builder);

            builder.AppendLine(SqlGenerationHelper.StatementTerminator);

            if ((operation.DefaultValue != null)
                || (operation.DefaultValueSql != null))
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append(" ADD");
                DefaultValue(operation.DefaultValue, operation.DefaultValueSql, builder);
                builder
                    .Append(" FOR ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .AppendLine(SqlGenerationHelper.StatementTerminator);
            }

            if (narrowed)
            {
                CreateIndexes(property, builder);
            }

            builder.EndCommand(suppressTransaction: IsMemoryOptimized(operation));
        }

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

            var qualifiedName = new StringBuilder();
            if (operation.Schema != null)
            {
                qualifiedName
                    .Append(operation.Schema)
                    .Append(".");
            }
            qualifiedName
                .Append(operation.Table)
                .Append(".")
                .Append(operation.Name);

            Rename(qualifiedName.ToString(), operation.NewName, "INDEX", builder);
            builder.EndCommand(suppressTransaction: IsMemoryOptimized(operation));
        }

        protected override void Generate(RenameSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var name = operation.Name;
            if (operation.NewName != null)
            {
                var qualifiedName = new StringBuilder();
                if (operation.Schema != null)
                {
                    qualifiedName
                        .Append(operation.Schema)
                        .Append(".");
                }
                qualifiedName.Append(operation.Name);

                Rename(qualifiedName.ToString(), operation.NewName, builder);

                name = operation.NewName;
            }

            if (operation.NewSchema != null)
            {
                Transfer(operation.NewSchema, operation.Schema, name, builder);
            }

            builder.EndCommand();
        }

        protected override void Generate(
            CreateTableOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
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

            builder
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: memoryOptimized);
        }

        protected override void Generate(
            RenameTableOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var name = operation.Name;
            if (operation.NewName != null)
            {
                var qualifiedName = new StringBuilder();
                if (operation.Schema != null)
                {
                    qualifiedName
                        .Append(operation.Schema)
                        .Append(".");
                }
                qualifiedName.Append(operation.Name);

                Rename(qualifiedName.ToString(), operation.NewName, builder);

                name = operation.NewName;
            }

            if (operation.NewSchema != null)
            {
                Transfer(operation.NewSchema, operation.Schema, name, builder);
            }

            builder.EndCommand();
        }

        protected override void Generate(DropTableOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            base.Generate(operation, model, builder, terminate: false);

            builder
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: IsMemoryOptimized(operation));
        }

        protected override void Generate(
            CreateIndexOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
            => Generate(operation, model, builder, terminate: true);

        protected override void Generate(
            CreateIndexOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var nullableColumns = operation.Columns
                .Where(
                    c =>
                        {
                            var property = FindProperty(model, operation.Schema, operation.Table, c);

                            return property == null // Couldn't bind column to property
                                   || property.IsColumnNullable();
                        })
                .ToList();

            var memoryOptimized = IsMemoryOptimized(operation);
            if (memoryOptimized)
            {
                builder.Append("ALTER TABLE ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table))
                    .Append(" ADD INDEX ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
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
            }

            if (terminate)
            {
                builder
                    .AppendLine(SqlGenerationHelper.StatementTerminator)
                    .EndCommand(suppressTransaction: memoryOptimized);
            }
        }

        protected override void Generate(
            DropPrimaryKeyOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            base.Generate(operation, model, builder, terminate: false);

            builder
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: IsMemoryOptimized(operation));
        }

        protected override void Generate(EnsureSchemaOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (string.Equals(operation.Name, "DBO", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder
                .Append("IF SCHEMA_ID(")
                .Append(SqlGenerationHelper.GenerateLiteral(operation.Name))
                .Append(") IS NULL EXEC(N'CREATE SCHEMA ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(SqlGenerationHelper.StatementTerminator)
                .Append("')")
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .EndCommand();
        }

        protected virtual void Generate(
            [NotNull] SqlServerCreateDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("CREATE DATABASE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (!string.IsNullOrEmpty(operation.FileName))
            {
                var fileName = ExpandFileName(operation.FileName);
                var name = Path.GetFileNameWithoutExtension(fileName);

                var logFileName = Path.ChangeExtension(fileName, ".ldf");
                var logName = name + "_log";

                // Match default naming behavior of SQL Server
                logFileName = logFileName.Insert(logFileName.Length - ".ldf".Length, "_log");

                builder
                    .AppendLine()
                    .Append("ON (NAME = '")
                    .Append(SqlGenerationHelper.EscapeLiteral(name))
                    .Append("', FILENAME = '")
                    .Append(SqlGenerationHelper.EscapeLiteral(fileName))
                    .Append("')")
                    .AppendLine()
                    .Append("LOG ON (NAME = '")
                    .Append(SqlGenerationHelper.EscapeLiteral(logName))
                    .Append("', FILENAME = '")
                    .Append(SqlGenerationHelper.EscapeLiteral(logFileName))
                    .Append("')");
            }

            builder
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: true)
                .Append("IF SERVERPROPERTY('EngineEdition') <> 5 EXEC(N'ALTER DATABASE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" SET READ_COMMITTED_SNAPSHOT ON")
                .Append(SqlGenerationHelper.StatementTerminator)
                .Append("')")
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: true);
        }

        private static string ExpandFileName(string fileName)
        {
            Check.NotNull(fileName, nameof(fileName));

#if NET451

            if (fileName.StartsWith("|DataDirectory|", StringComparison.OrdinalIgnoreCase))
            {
                var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                if (string.IsNullOrEmpty(dataDirectory))
                    dataDirectory = AppDomain.CurrentDomain.BaseDirectory;
                fileName = Path.Combine(dataDirectory, fileName.Substring("|DataDirectory|".Length));
            }

#endif

            return Path.GetFullPath(fileName);
        }

        protected virtual void Generate(
            [NotNull] SqlServerDropDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("IF SERVERPROPERTY('EngineEdition') <> 5 EXEC(N'ALTER DATABASE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" SET SINGLE_USER WITH ROLLBACK IMMEDIATE")
                .Append(SqlGenerationHelper.StatementTerminator)
                .Append("')")
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: true)
                .Append("DROP DATABASE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: true);
        }

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

        protected override void Generate(AlterTableOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            if (IsMemoryOptimized(operation)
                ^ IsMemoryOptimized(operation.OldTable))
            {
                throw new InvalidOperationException(SqlServerStrings.AlterMemoryOptimizedTable);
            }

            base.Generate(operation, model, builder);
        }

        protected override void Generate(DropForeignKeyOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            base.Generate(operation, model, builder, terminate: false);

            builder
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: IsMemoryOptimized(operation));
        }

        protected override void Generate(
            DropIndexOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
            => Generate(operation, model, builder, terminate: true);

        protected virtual void Generate(
            [NotNull] DropIndexOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var memoryOptimized = IsMemoryOptimized(operation);
            if (memoryOptimized)
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append(" DROP INDEX ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name));
            }
            else
            {
                builder
                    .Append("DROP INDEX ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" ON ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));
            }

            if (terminate)
            {
                builder
                    .AppendLine(SqlGenerationHelper.StatementTerminator)
                    .EndCommand(suppressTransaction: memoryOptimized);
            }
        }

        protected override void Generate(
            DropColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
            => Generate(operation, model, builder, terminate: true);

        protected override void Generate(
            DropColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);
            base.Generate(operation, model, builder, terminate: false);

            if (terminate)
            {
                builder
                    .AppendLine(SqlGenerationHelper.StatementTerminator)
                    .EndCommand(suppressTransaction: IsMemoryOptimized(operation));
            }
        }

        protected override void Generate(
            RenameColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var qualifiedName = new StringBuilder();
            if (operation.Schema != null)
            {
                qualifiedName
                    .Append(operation.Schema)
                    .Append(".");
            }
            qualifiedName
                .Append(operation.Table)
                .Append(".")
                .Append(operation.Name);

            Rename(qualifiedName.ToString(), operation.NewName, "COLUMN", builder);
            builder.EndCommand();
        }

        protected override void ColumnDefinition(
            string schema,
            string table,
            string name,
            Type clrType,
            string type,
            bool? unicode,
            int? maxLength,
            bool rowVersion,
            bool nullable,
            object defaultValue,
            string defaultValueSql,
            string computedColumnSql,
            IAnnotatable annotatable,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            var valueGenerationStrategy = annotatable[
                SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy] as SqlServerValueGenerationStrategy?;

            ColumnDefinition(
                schema,
                table,
                name,
                clrType,
                type,
                unicode,
                maxLength,
                rowVersion,
                nullable,
                defaultValue,
                defaultValueSql,
                computedColumnSql,
                valueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn,
                annotatable,
                model,
                builder);
        }

        protected virtual void ColumnDefinition(
            [CanBeNull] string schema,
            [NotNull] string table,
            [NotNull] string name,
            [NotNull] Type clrType,
            [CanBeNull] string type,
            [CanBeNull] bool? unicode,
            [CanBeNull] int? maxLength,
            bool rowVersion,
            bool nullable,
            [CanBeNull] object defaultValue,
            [CanBeNull] string defaultValueSql,
            [CanBeNull] string computedColumnSql,
            bool identity,
            [NotNull] IAnnotatable annotatable,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NotNull(builder, nameof(builder));

            if (computedColumnSql != null)
            {
                builder
                    .Append(SqlGenerationHelper.DelimitIdentifier(name))
                    .Append(" AS ")
                    .Append(computedColumnSql);

                return;
            }

            base.ColumnDefinition(
                schema,
                table,
                name,
                clrType,
                type,
                unicode,
                maxLength,
                rowVersion,
                nullable,
                identity
                    ? null
                    : defaultValue,
                defaultValueSql,
                computedColumnSql,
                annotatable,
                model,
                builder);

            if (identity)
            {
                builder.Append(" IDENTITY");
            }
        }

        protected virtual void Rename(
            [NotNull] string name,
            [NotNull] string newName,
            [NotNull] MigrationCommandListBuilder builder) => Rename(name, newName, /*type:*/ null, builder);

        protected virtual void Rename(
            [NotNull] string name,
            [NotNull] string newName,
            [CanBeNull] string type,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(newName, nameof(newName));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("EXEC sp_rename ")
                .Append(SqlGenerationHelper.GenerateLiteral(name))
                .Append(", ")
                .Append(SqlGenerationHelper.GenerateLiteral(newName));

            if (type != null)
            {
                builder
                    .Append(", ")
                    .Append(SqlGenerationHelper.GenerateLiteral(type));
            }

            builder.AppendLine(SqlGenerationHelper.StatementTerminator);
        }

        protected virtual void Transfer(
            [NotNull] string newSchema,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(newSchema, nameof(newSchema));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER SCHEMA ")
                .Append(SqlGenerationHelper.DelimitIdentifier(newSchema))
                .Append(" TRANSFER ")
                .Append(SqlGenerationHelper.DelimitIdentifier(name, schema))
                .AppendLine(SqlGenerationHelper.StatementTerminator);
        }

        protected override void IndexTraits(MigrationOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var clustered = operation[SqlServerFullAnnotationNames.Instance.Clustered] as bool?;
            if (clustered.HasValue)
            {
                builder.Append(clustered.Value ? "CLUSTERED " : "NONCLUSTERED ");
            }
        }

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

        protected virtual void DropDefaultConstraint(
            [CanBeNull] string schema,
            [NotNull] string tableName,
            [NotNull] string columnName,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(tableName, nameof(tableName));
            Check.NotEmpty(columnName, nameof(columnName));
            Check.NotNull(builder, nameof(builder));

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
                .Append("WHERE ([d].[parent_object_id] = OBJECT_ID(N'");

            if (schema != null)
            {
                builder
                    .Append(SqlGenerationHelper.EscapeLiteral(schema))
                    .Append(".");
            }

            builder
                .Append(SqlGenerationHelper.EscapeLiteral(tableName))
                .Append("') AND [c].[name] = N'")
                .Append(SqlGenerationHelper.EscapeLiteral(columnName))
                .AppendLine("');")
                .Append("IF ")
                .Append(variable)
                .Append(" IS NOT NULL EXEC(N'ALTER TABLE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(tableName, schema))
                .Append(" DROP CONSTRAINT [' + ")
                .Append(variable)
                .Append(" + ']")
                .Append(SqlGenerationHelper.StatementTerminator)
                .Append("')")
                .AppendLine(SqlGenerationHelper.StatementTerminator);
        }

        protected virtual void DropIndexes(
            [CanBeNull] IProperty property,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            if (property == null)
            {
                return;
            }

            foreach (var index in property.GetContainingIndexes())
            {
                var operation = new DropIndexOperation
                {
                    Schema = Annotations.For(index.DeclaringEntityType).Schema,
                    Table = Annotations.For(index.DeclaringEntityType).TableName,
                    Name = Annotations.For(index).Name
                };
                operation.AddAnnotations(_migrationsAnnotations.ForRemove(index));

                Generate(operation, index.DeclaringEntityType.Model, builder, terminate: false);
                builder.AppendLine(SqlGenerationHelper.StatementTerminator);
            }
        }

        protected virtual void CreateIndexes(
            [CanBeNull] IProperty property,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            if (property == null)
            {
                return;
            }

            foreach (var index in property.GetContainingIndexes())
            {
                var operation = new CreateIndexOperation
                {
                    IsUnique = index.IsUnique,
                    Name = Annotations.For(index).Name,
                    Schema = Annotations.For(index.DeclaringEntityType).Schema,
                    Table = Annotations.For(index.DeclaringEntityType).TableName,
                    Columns = index.Properties.Select(p => Annotations.For(p).ColumnName).ToArray(),
                    Filter = Annotations.For(index).Filter
                };
                operation.AddAnnotations(_migrationsAnnotations.For(index));

                Generate(operation, index.DeclaringEntityType.Model, builder, terminate: false);
                builder.AppendLine(SqlGenerationHelper.StatementTerminator);
            }
        }

        private static bool IsMemoryOptimized(Annotatable annotatable)
            => annotatable[SqlServerFullAnnotationNames.Instance.MemoryOptimized] as bool? == true;
    }
}
