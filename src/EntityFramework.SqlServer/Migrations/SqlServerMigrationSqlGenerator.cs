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
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerMigrationSqlGenerator : MigrationSqlGenerator
    {
        private readonly ISqlServerUpdateSqlGenerator _sql;

        public SqlServerMigrationSqlGenerator([NotNull] ISqlServerUpdateSqlGenerator sqlGenerator)
            : base(Check.NotNull(sqlGenerator, nameof(sqlGenerator)))
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

            // TODO: Test default value/expression
            builder
                .Append("ALTER TABLE ")
                .Append(_sql.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ALTER COLUMN ");
            ColumnDefinition(operation, model, builder);
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
            string type,
            bool nullable,
            object defaultValue,
            string defaultExpression,
            IAnnotatable annotatable,
            IModel model,
            SqlBatchBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(type, nameof(type));
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NotNull(builder, nameof(builder));

            var computedExpression = annotatable[SqlServerAnnotationNames.Prefix
                                                 + SqlServerAnnotationNames.ColumnComputedExpression];
            if (computedExpression != null)
            {
                builder
                    .Append(_sql.DelimitIdentifier(name))
                    .Append(" AS ")
                    .Append(computedExpression);

                return;
            }

            base.ColumnDefinition(
                schema,
                table,
                name,
                type,
                nullable,
                defaultValue,
                defaultExpression,
                annotatable,
                model,
                builder);

            var valueGeneration = (string)annotatable[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ItentityStrategy];
            if (valueGeneration == SqlServerIdentityStrategy.IdentityColumn.ToString())
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

        public virtual void ColumnDefinition(
            [NotNull] AlterColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder) =>
                ColumnDefinition(
                    operation.Schema,
                    operation.Table,
                    operation.Name,
                    operation.Type,
                    operation.IsNullable,
                    operation.DefaultValue,
                    operation.DefaultValueSql,
                    operation,
                    model,
                    builder);

        public override void IndexTraits(MigrationOperation operation, IModel model, SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var clustered = (string)operation[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered];
            if (clustered != null)
            {
                builder.Append(clustered == "True" ? "CLUSTERED " : "NONCLUSTERED ");
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
    }
}
