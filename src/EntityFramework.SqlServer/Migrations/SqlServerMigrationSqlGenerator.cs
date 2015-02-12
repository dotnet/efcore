// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Migrations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerMigrationSqlGenerator : MigrationSqlGenerator
    {
        public virtual void Generate(
            [NotNull] CreateDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("CREATE DATABASE ")
                .Append(DelimitIdentifier(operation.Name))
                .EndBatch()
                .Append("IF SERVERPROPERTY('EngineEdition') <> 5 EXECUTE sp_executesql N'ALTER DATABASE ")
                .Append(DelimitIdentifier(operation.Name))
                .Append(" SET READ_COMMITTED_SNAPSHOT ON';");
        }

        public virtual void Generate(
            [NotNull] DropDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("IF SERVERPROPERTY('EngineEdition') <> 5 EXECUTE sp_executesql N'ALTER DATABASE ")
                .Append(DelimitIdentifier(operation.Name))
                .Append(" SET SINGLE_USER WITH ROLLBACK IMMEDIATE'")
                .EndBatch()
                .Append("DROP DATABASE ")
                .Append(DelimitIdentifier(operation.Name))
                .Append(";");
        }

        protected override void Generate(RenameSequenceOperation operation, IModel model, SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            GenerateRename(operation.Name, operation.Schema, operation.NewName, "OBJECT", builder);
        }

        protected override void Generate(MoveSequenceOperation operation, IModel model, SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            GenerateMove(operation.Name, operation.Schema, operation.NewSchema, builder);
        }

        protected override void Generate(RenameTableOperation operation, IModel model, SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            GenerateRename(operation.Name, operation.Schema, operation.NewName, "OBJECT", builder);
        }

        protected override void Generate(MoveTableOperation operation, IModel model, SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            GenerateMove(operation.Name, operation.Schema, operation.NewSchema, builder);
        }

        protected override void GenerateColumn([NotNull] ColumnModel column, [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(column, nameof(column));
            Check.NotNull(builder, nameof(builder));

            var computedSql = column[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ColumnComputedExpression];
            if (computedSql == null)
            {
                base.GenerateColumn(column, builder);

                return;
            }

            builder
                .Append(DelimitIdentifier(column.Name))
                .Append(" ")
                .Append("AS ")
                .Append(computedSql);
        }

        protected override void Generate(RenameColumnOperation operation, IModel model, SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            GenerateRename(
                EscapeLiteral(operation.Table) + "." + EscapeLiteral(operation.Name),
                operation.Schema,
                operation.NewName,
                "COLUMN",
                builder);
        }

        protected override void GenerateIndexTraits(CreateIndexOperation operation, SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered] == bool.TrueString)
            {
                builder.Append("CLUSTERED ");
            }
        }

        protected override void Generate(RenameIndexOperation operation, IModel model, SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            GenerateRename(
                EscapeLiteral(operation.Table) + "." + EscapeLiteral(operation.Name),
                operation.Schema,
                operation.NewName,
                "INDEX",
                builder);
        }

        protected override string DelimitIdentifier(string identifier) => "[" + EscapeIdentifier(identifier) + "]";
        protected override string EscapeIdentifier(string identifier) => identifier.Replace("]", "]]");

        protected override void GenerateColumnTraits(ColumnModel column, SqlBatchBuilder builder)
        {
            Check.NotNull(column, nameof(column));
            Check.NotNull(builder, nameof(builder));

            if (column[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration] ==
                SqlServerValueGenerationStrategy.Identity.ToString())
            {
                builder.Append(" IDENTITY");
            }
        }

        protected override void GeneratePrimaryKeyTraits(AddPrimaryKeyOperation operation, SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered] != bool.TrueString)
            {
                builder.Append(" NONCLUSTERED");
            }
        }

        private void GenerateRename(
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] string newName,
            [NotNull] string objectType,
            [NotNull] SqlBatchBuilder builder)
        {
            builder.Append("EXECUTE sp_rename @objname = N");

            if (!string.IsNullOrWhiteSpace(schema))
            {
                builder
                    .Append(Literal(schema))
                    .Append(".");
            }

            builder
                .Append(Literal(name))
                .Append(", @newname = N")
                .Append(Literal(newName))
                .Append(", @objtype = N")
                .Append(Literal(objectType))
                .Append(";");
        }

        private void GenerateMove(
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] string newSchema,
            [NotNull] SqlBatchBuilder builder) =>
                builder
                    .Append("ALTER SCHEMA ")
                    .Append(DelimitIdentifier(newSchema))
                    .Append(" TRANSFER ")
                    .Append(DelimitIdentifier(name, schema))
                    .Append(";");
    }
}
