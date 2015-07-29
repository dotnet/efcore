// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Migrations.Sql;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteMigrationSqlGenerator : MigrationSqlGenerator
    {
        private readonly IUpdateSqlGenerator _sql;

        public SqliteMigrationSqlGenerator(
            [NotNull] SqliteUpdateSqlGenerator sqlGenerator,
            [NotNull] SqliteTypeMapper typeMapper,
            [NotNull] SqliteMetadataExtensionProvider annotations)
            : base(sqlGenerator, typeMapper, annotations)
        {
            _sql = sqlGenerator;
        }

        public override void Generate(DropIndexOperation operation, IModel model, SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP INDEX ")
                .Append(_sql.DelimitIdentifier(operation.Name));
        }

        public override void Generate(RenameTableOperation operation, IModel model, SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.NewName != null)
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(_sql.DelimitIdentifier(operation.Name))
                    .Append(" RENAME TO ")
                    .Append(_sql.DelimitIdentifier(operation.NewName));
            }
        }

        public override void Generate(CreateTableOperation operation, IModel model, SqlBatchBuilder builder)
        {
            // Lifts a primary key definition into the typename.
            // This handles the quirks of creating integer primary keys using autoincrement, not default rowid behavior.
            if (operation.PrimaryKey?.Columns.Length == 1)
            {
                var columnOp = operation.Columns?.FirstOrDefault(o => o.Name == operation.PrimaryKey.Columns[0]);
                if (columnOp != null)
                {
                    columnOp.AddAnnotation(SqliteAnnotationNames.Prefix + SqliteAnnotationNames.InlinePrimaryKey, true);
                    if (!string.IsNullOrEmpty(operation.PrimaryKey.Name))
                    {
                        columnOp.AddAnnotation(SqliteAnnotationNames.Prefix + SqliteAnnotationNames.InlinePrimaryKeyName, operation.PrimaryKey.Name);
                    }
                    operation.PrimaryKey = null;
                }
            }

            base.Generate(operation, model, builder);
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
            base.ColumnDefinition(
                schema, table, name, clrType, type, nullable,
                defaultValue, defaultValueSql, computedColumnSql, annotatable, model, builder);

            var columnAnnotation = annotatable as Annotatable;
            var inlinePk = columnAnnotation?.FindAnnotation(SqliteAnnotationNames.Prefix + SqliteAnnotationNames.InlinePrimaryKey);

            if (inlinePk != null
                && (bool)inlinePk.Value)
            {
                var inlinePkName = columnAnnotation.FindAnnotation(SqliteAnnotationNames.Prefix + SqliteAnnotationNames.InlinePrimaryKeyName);
                if (!string.IsNullOrEmpty(inlinePkName?.Value as string))
                {
                    builder
                        .Append(" CONSTRAINT ")
                        .Append(_sql.DelimitIdentifier((string)inlinePkName.Value));
                }
                builder.Append(" PRIMARY KEY");
                var autoincrement = columnAnnotation.FindAnnotation(SqliteAnnotationNames.Prefix + SqliteAnnotationNames.Autoincrement);
                if (autoincrement != null
                    && (bool)autoincrement.Value)
                {
                    builder.Append(" AUTOINCREMENT");
                }
            }
        }

        #region Invalid migration operations

        // These operations can be accomplished instead with a table-rebuild
        public override void Generate(AddForeignKeyOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        public override void Generate(AddPrimaryKeyOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        public override void Generate(AddUniqueConstraintOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        public override void Generate(DropColumnOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        public override void Generate(DropForeignKeyOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        public override void Generate(DropPrimaryKeyOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        public override void Generate(DropUniqueConstraintOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        public override void Generate(RenameColumnOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        public override void Generate(RenameIndexOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        public override void Generate(AlterColumnOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        #endregion

        #region Invalid schema operations

        public override void Generate(CreateSchemaOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotSupportedException(Strings.SchemasNotSupported);
        }

        public override void Generate(DropSchemaOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotSupportedException(Strings.SchemasNotSupported);
        }

        #endregion

        #region Sequences not supported

        // SQLite does not have sequences
        public override void Generate(RestartSequenceOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotSupportedException(Strings.SequencesNotSupported);
        }

        public override void Generate(CreateSequenceOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotSupportedException(Strings.SequencesNotSupported);
        }

        public override void Generate(RenameSequenceOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotSupportedException(Strings.SequencesNotSupported);
        }

        public override void Generate(AlterSequenceOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotSupportedException(Strings.SequencesNotSupported);
        }

        public override void Generate(DropSequenceOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotSupportedException(Strings.SequencesNotSupported);
        }

        #endregion
    }
}
