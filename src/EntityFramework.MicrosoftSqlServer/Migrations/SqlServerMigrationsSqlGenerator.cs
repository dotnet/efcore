// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations
{
    public class SqlServerMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        private int _variableCounter;

        public SqlServerMigrationsSqlGenerator(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerator sqlGenerator,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IRelationalAnnotationProvider annotations)
            : base(commandBuilderFactory, sqlGenerator, typeMapper, annotations)
        {
        }

        protected override void Generate(MigrationOperation operation, IModel model, RelationalCommandListBuilder builder)
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
            AlterColumnOperation operation,
            IModel model,
            RelationalCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);

            builder
                .Append("ALTER TABLE ")
                .Append(SqlGenerator.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ALTER COLUMN ");

            ColumnDefinition(
                operation.Schema,
                operation.Table,
                operation.Name,
                operation.ClrType,
                operation.ColumnType,
                operation.IsNullable,
                /*defaultValue:*/ null,
                /*defaultValueSql:*/ null,
                operation.ComputedColumnSql,
                /*identity:*/ false,
                operation,
                model,
                builder);

            if (operation.DefaultValue != null
                || operation.DefaultValueSql != null)
            {
                builder
                    .AppendLine(";")
                    .Append("ALTER TABLE ")
                    .Append(SqlGenerator.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append(" ADD");
                DefaultValue(operation.DefaultValue, operation.DefaultValueSql, builder);
                builder
                    .Append(" FOR ")
                    .Append(SqlGenerator.DelimitIdentifier(operation.Name));
            }
        }

        protected override void Generate(
            RenameIndexOperation operation,
            IModel model,
            RelationalCommandListBuilder builder)
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

            Rename(qualifiedName.ToString(), operation.NewName, "INDEX", builder);
        }

        protected override void Generate(RenameSequenceOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var separate = false;
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

                separate = true;
                name = operation.NewName;
            }

            if (operation.NewSchema != null)
            {
                if (separate)
                {
                    builder.AppendLine(SqlGenerator.BatchCommandSeparator);
                }

                Transfer(operation.NewSchema, operation.Schema, name, builder);
            }
        }

        protected override void Generate(
            RenameTableOperation operation,
            IModel model,
            RelationalCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var separate = false;
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

                separate = true;
                name = operation.NewName;
            }

            if (operation.NewSchema != null)
            {
                if (separate)
                {
                    builder.AppendLine(SqlGenerator.BatchCommandSeparator);
                }

                Transfer(operation.NewSchema, operation.Schema, name, builder);
            }
        }

        protected override void Generate(CreateIndexOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            base.Generate(operation, model, builder);

            var clustered = operation[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered] as bool?;
            if (operation.IsUnique
                && clustered != true)
            {
                builder.Append(" WHERE ");
                for (var i = 0; i < operation.Columns.Length; i++)
                {
                    if (i != 0)
                    {
                        builder.Append(" AND ");
                    }

                    builder
                        .Append(SqlGenerator.DelimitIdentifier(operation.Columns[i]))
                        .Append(" IS NOT NULL");
                }
            }
        }

        protected override void Generate(EnsureSchemaOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (string.Equals(operation.Name, "DBO", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder
                .Append("IF SCHEMA_ID(N")
                .Append(SqlGenerator.GenerateLiteral(operation.Name))
                .Append(") IS NULL EXEC(N'CREATE SCHEMA ")
                .Append(SqlGenerator.DelimitIdentifier(operation.Name))
                .Append("')");
        }

        protected virtual void Generate(
            [NotNull] SqlServerCreateDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] RelationalCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("CREATE DATABASE ")
                .Append(SqlGenerator.DelimitIdentifier(operation.Name))
                .AppendLine(SqlGenerator.BatchCommandSeparator)
                .EndCommand()
                .Append("IF SERVERPROPERTY('EngineEdition') <> 5 EXEC(N'ALTER DATABASE ")
                .Append(SqlGenerator.DelimitIdentifier(operation.Name))
                .Append(" SET READ_COMMITTED_SNAPSHOT ON')");
        }

        protected virtual void Generate(
            [NotNull] SqlServerDropDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] RelationalCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("IF SERVERPROPERTY('EngineEdition') <> 5 EXEC(N'ALTER DATABASE ")
                .Append(SqlGenerator.DelimitIdentifier(operation.Name))
                .Append(" SET SINGLE_USER WITH ROLLBACK IMMEDIATE')")
                .AppendLine(SqlGenerator.BatchCommandSeparator)
                .EndCommand()
                .Append("DROP DATABASE ")
                .Append(SqlGenerator.DelimitIdentifier(operation.Name));
        }

        protected override void Generate(
            DropIndexOperation operation,
            IModel model,
            RelationalCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP INDEX ")
                .Append(SqlGenerator.DelimitIdentifier(operation.Name))
                .Append(" ON ")
                .Append(SqlGenerator.DelimitIdentifier(operation.Table, operation.Schema));
        }

        protected override void Generate(
            DropColumnOperation operation,
            IModel model,
            RelationalCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);
            base.Generate(operation, model, builder);
        }

        protected override void Generate(
            RenameColumnOperation operation,
            IModel model,
            RelationalCommandListBuilder builder)
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
        }

        protected override void ColumnDefinition(
            string schema,
            string table,
            string name,
            Type clrType,
            string type,
            bool nullable,
            object defaultValue,
            string defaultValueSql,
            string computedColumnSql,
            IAnnotatable annotatable,
            IModel model,
            RelationalCommandListBuilder builder)
        {
            var valueGenerationStrategy = annotatable[
                SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGenerationStrategy] as SqlServerValueGenerationStrategy?;

            ColumnDefinition(
                schema,
                table,
                name,
                clrType,
                type,
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
            [CanBeNull] string table,
            [NotNull] string name,
            [NotNull] Type clrType,
            [CanBeNull] string type,
            bool nullable,
            [CanBeNull] object defaultValue,
            [CanBeNull] string defaultValueSql,
            [CanBeNull] string computedColumnSql,
            bool identity,
            [NotNull] IAnnotatable annotatable,
            [CanBeNull] IModel model,
            [NotNull] RelationalCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NotNull(builder, nameof(builder));

            if (computedColumnSql != null)
            {
                builder
                    .Append(SqlGenerator.DelimitIdentifier(name))
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
                nullable,
                defaultValue,
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
            [NotNull] RelationalCommandListBuilder builder) => Rename(name, newName, /*type:*/ null, builder);

        protected virtual void Rename(
            [NotNull] string name,
            [NotNull] string newName,
            [CanBeNull] string type,
            [NotNull] RelationalCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(newName, nameof(newName));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("EXEC sp_rename N")
                .Append(SqlGenerator.GenerateLiteral(name))
                .Append(", N")
                .Append(SqlGenerator.GenerateLiteral(newName));

            if (type != null)
            {
                builder
                    .Append(", ")
                    .Append(SqlGenerator.GenerateLiteral(type));
            }
        }

        protected virtual void Transfer(
            [NotNull] string newSchema,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] RelationalCommandListBuilder builder)
        {
            Check.NotEmpty(newSchema, nameof(newSchema));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER SCHEMA ")
                .Append(SqlGenerator.DelimitIdentifier(newSchema))
                .Append(" TRANSFER ")
                .Append(SqlGenerator.DelimitIdentifier(name, schema));
        }

        protected override void IndexTraits(MigrationOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var clustered = operation[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered] as bool?;
            if (clustered.HasValue)
            {
                builder.Append(clustered.Value ? "CLUSTERED " : "NONCLUSTERED ");
            }
        }

        protected override void ForeignKeyAction(ReferentialAction referentialAction, RelationalCommandListBuilder builder)
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
            [NotNull] RelationalCommandListBuilder builder)
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
                .AppendLine("INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id]")
                .Append("WHERE ([d].[parent_object_id] = OBJECT_ID(N'");

            if (schema != null)
            {
                builder
                    .Append(SqlGenerator.EscapeLiteral(schema))
                    .Append(".");
            }

            builder
                .Append(SqlGenerator.EscapeLiteral(tableName))
                .Append("') AND [c].[name] = N'")
                .Append(SqlGenerator.EscapeLiteral(columnName))
                .AppendLine("');")
                .Append("IF ")
                .Append(variable)
                .Append(" IS NOT NULL EXEC(N'ALTER TABLE ")
                .Append(SqlGenerator.DelimitIdentifier(tableName, schema))
                .Append(" DROP CONSTRAINT [' + ")
                .Append(variable)
                .AppendLine(" + ']');");
        }
    }
}
