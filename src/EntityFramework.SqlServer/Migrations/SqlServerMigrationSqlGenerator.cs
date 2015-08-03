// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Migrations.Sql;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Migrations;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerMigrationSqlGenerator : MigrationSqlGenerator
    {
        private readonly ISqlServerUpdateSqlGenerator _sql;
        private int _variableCounter;

        public SqlServerMigrationSqlGenerator(
            [NotNull] ISqlServerUpdateSqlGenerator sqlGenerator,
            [NotNull] SqlServerTypeMapper typeMapper,
            [NotNull] SqlServerMetadataExtensionProvider annotations)
            : base(sqlGenerator, typeMapper, annotations)
        {
            _sql = sqlGenerator;
        }

        public override void Generate(
            [NotNull] AlterColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);

            builder
                .Append("ALTER TABLE ")
                .Append(_sql.DelimitIdentifier(operation.Table, operation.Schema))
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
                    operation,
                    model,
                    builder);

            if (operation.DefaultValue != null || operation.DefaultValueSql != null)
            {
                builder
                    .AppendLine(";")
                    .Append("ALTER TABLE ")
                    .Append(_sql.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append(" ADD");
                DefaultValue(operation.DefaultValue, operation.DefaultValueSql, builder);
                builder
                    .Append(" FOR ")
                    .Append(_sql.DelimitIdentifier(operation.Name));
            }
        }

        public override void Generate(
            [NotNull] RenameIndexOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.NewName != null)
            {
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
        }

        public override void Generate(RenameSequenceOperation operation, IModel model, SqlBatchBuilder builder)
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
                    builder.AppendLine(_sql.BatchCommandSeparator);
                }

                Transfer(operation.NewSchema, operation.Schema, name, builder);
            }
        }

        public override void Generate(
            [NotNull] RenameTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
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
                    builder.AppendLine(_sql.BatchCommandSeparator);
                }

                Transfer(operation.NewSchema, operation.Schema, name, builder);
            }
        }

        public override void Generate(CreateSchemaOperation operation, IModel model, SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (string.Equals(operation.Name, "DBO", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder
                .Append("IF SCHEMA_ID(N")
                .Append(_sql.GenerateLiteral(operation.Name))
                .Append(") IS NULL EXEC(N'CREATE SCHEMA ")
                .Append(_sql.DelimitIdentifier(operation.Name))
                .Append("')");
        }

        public virtual void Generate(
            [NotNull] CreateDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("CREATE DATABASE ")
                .Append(_sql.DelimitIdentifier(operation.Name))
                .EndBatch()
                .Append("IF SERVERPROPERTY('EngineEdition') <> 5 EXEC(N'ALTER DATABASE ")
                .Append(_sql.DelimitIdentifier(operation.Name))
                .Append(" SET READ_COMMITTED_SNAPSHOT ON')");
        }

        public virtual void Generate(
            [NotNull] DropDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("IF SERVERPROPERTY('EngineEdition') <> 5 EXEC(N'ALTER DATABASE ")
                .Append(_sql.DelimitIdentifier(operation.Name))
                .Append(" SET SINGLE_USER WITH ROLLBACK IMMEDIATE')")
                .EndBatch()
                .Append("DROP DATABASE ")
                .Append(_sql.DelimitIdentifier(operation.Name));
        }

        public override void Generate(
            [NotNull] DropIndexOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP INDEX ")
                .Append(_sql.DelimitIdentifier(operation.Name))
                .Append(" ON ")
                .Append(_sql.DelimitIdentifier(operation.Table, operation.Schema));
        }

        public override void Generate(
            [NotNull] DropColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);
            base.Generate(operation, model, builder);
        }

        public override void Generate(
            [NotNull] RenameColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
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

        public override void ColumnDefinition(
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
            SqlBatchBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NotNull(builder, nameof(builder));

            if (computedColumnSql != null)
            {
                builder
                    .Append(_sql.DelimitIdentifier(name))
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

            var valueGenerationStrategy = annotatable[
                SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGenerationStrategy] as SqlServerIdentityStrategy?;
            if (valueGenerationStrategy == SqlServerIdentityStrategy.IdentityColumn)
            {
                builder.Append(" IDENTITY");
            }
        }

        public virtual void Rename(
            [NotNull] string name,
            [NotNull] string newName,
            [NotNull] SqlBatchBuilder builder) => Rename(name, newName, /*type:*/ null, builder);

        public virtual void Rename(
            [NotNull] string name,
            [NotNull] string newName,
            [CanBeNull] string type,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(newName, nameof(newName));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("EXEC sp_rename N")
                .Append(_sql.GenerateLiteral(name))
                .Append(", N")
                .Append(_sql.GenerateLiteral(newName));

            if (type != null)
            {
                builder
                    .Append(", ")
                    .Append(_sql.GenerateLiteral(type));
            }
        }

        public virtual void Transfer(
            [NotNull] string newSchema,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotEmpty(newSchema, nameof(newSchema));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER SCHEMA ")
                .Append(_sql.DelimitIdentifier(newSchema))
                .Append(" TRANSFER ")
                .Append(_sql.DelimitIdentifier(name, schema));
        }

        public override void IndexTraits(MigrationOperation operation, IModel model, SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var clustered = operation[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered] as bool?;
            if (clustered.HasValue)
            {
                builder.Append(clustered.Value ? "CLUSTERED " : "NONCLUSTERED ");
            }
        }

        public override void ForeignKeyAction(ReferentialAction referentialAction, SqlBatchBuilder builder)
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
            [NotNull] SqlBatchBuilder builder)
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
                    .Append(_sql.EscapeLiteral(schema))
                    .Append(".");
            }

            builder
                .Append(_sql.EscapeLiteral(tableName))
                .Append("') AND [c].[name] = N'")
                .Append(_sql.EscapeLiteral(columnName))
                .AppendLine("');")
                .Append("IF ")
                .Append(variable)
                .Append(" IS NOT NULL EXEC(N'ALTER TABLE ")
                .Append(_sql.DelimitIdentifier(tableName, schema))
                .Append(" DROP CONSTRAINT [' + ")
                .Append(variable)
                .AppendLine(" + ']');");
        }
    }
}
