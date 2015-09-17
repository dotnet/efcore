// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Sqlite.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Update.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations
{
    public class SqliteMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        public SqliteMigrationsSqlGenerator(
            [NotNull] SqliteUpdateSqlGenerator sql,
            [NotNull] SqliteTypeMapper typeMapper,
            [NotNull] SqliteAnnotationProvider annotations)
            : base(sql, typeMapper, annotations)
        {
        }

        public override IReadOnlyList<RelationalCommand> Generate(IReadOnlyList<MigrationOperation> operations, IModel model = null)
            => base.Generate(LiftForeignKeyOperations(operations), model);

        private IReadOnlyList<MigrationOperation> LiftForeignKeyOperations(IReadOnlyList<MigrationOperation> migrationOperations)
        {
            var operations = new List<MigrationOperation>();
            foreach (var operation in migrationOperations)
            {
                var foreignKeyOperation = operation as AddForeignKeyOperation;
                if (foreignKeyOperation != null)
                {
                    var table = migrationOperations
                        .OfType<CreateTableOperation>()
                        .FirstOrDefault(o => o.Name == foreignKeyOperation.Table);

                    if (table != null)
                    {
                        table.ForeignKeys.Add(foreignKeyOperation);
                        //do not add to fk operation migration
                        continue;
                    }
                }

                operations.Add(operation);
            }
            return operations.AsReadOnly();
        }

        protected override void Generate(DropIndexOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP INDEX ")
                .Append(Sql.DelimitIdentifier(operation.Name));
        }

        protected override void Generate(RenameTableOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.NewName != null)
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(Sql.DelimitIdentifier(operation.Name))
                    .Append(" RENAME TO ")
                    .Append(Sql.DelimitIdentifier(operation.NewName));
            }
        }

        protected override void Generate(CreateTableOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            // Lifts a primary key definition into the typename.
            // This handles the quirks of creating integer primary keys using autoincrement, not default rowid behavior.
            if (operation.PrimaryKey?.Columns.Length == 1)
            {
                var columnOp = operation.Columns.FirstOrDefault(o => o.Name == operation.PrimaryKey.Columns[0]);
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
            base.ColumnDefinition(
                schema, table, name, clrType, type, nullable,
                defaultValue, defaultValueSql, computedColumnSql, annotatable, model, builder);

            var inlinePk = annotatable[SqliteAnnotationNames.Prefix + SqliteAnnotationNames.InlinePrimaryKey] as bool?;
            if (inlinePk == true)
            {
                var inlinePkName = annotatable[
                    SqliteAnnotationNames.Prefix + SqliteAnnotationNames.InlinePrimaryKeyName] as string;
                if (!string.IsNullOrEmpty(inlinePkName))
                {
                    builder
                        .Append(" CONSTRAINT ")
                        .Append(Sql.DelimitIdentifier(inlinePkName));
                }
                builder.Append(" PRIMARY KEY");
                var autoincrement = annotatable[SqliteAnnotationNames.Prefix + SqliteAnnotationNames.Autoincrement] as bool?;
                if (autoincrement == true)
                {
                    builder.Append(" AUTOINCREMENT");
                }
            }
        }

        #region Invalid migration operations

        // These operations can be accomplished instead with a table-rebuild
        protected override void Generate(AddForeignKeyOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        protected override void Generate(AddPrimaryKeyOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        protected override void Generate(AddUniqueConstraintOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        protected override void Generate(DropColumnOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        protected override void Generate(DropForeignKeyOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        protected override void Generate(DropPrimaryKeyOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        protected override void Generate(DropUniqueConstraintOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        protected override void Generate(RenameColumnOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        protected override void Generate(RenameIndexOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        protected override void Generate(AlterColumnOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            throw new NotSupportedException(Strings.InvalidMigrationOperation);
        }

        #endregion

        #region Invalid schema operations

        protected override void Generate(EnsureSchemaOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            throw new NotSupportedException(Strings.SchemasNotSupported);
        }

        protected override void Generate(DropSchemaOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            throw new NotSupportedException(Strings.SchemasNotSupported);
        }

        #endregion

        #region Sequences not supported

        // SQLite does not have sequences
        protected override void Generate(RestartSequenceOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            throw new NotSupportedException(Strings.SequencesNotSupported);
        }

        protected override void Generate(CreateSequenceOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            throw new NotSupportedException(Strings.SequencesNotSupported);
        }

        protected override void Generate(RenameSequenceOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            throw new NotSupportedException(Strings.SequencesNotSupported);
        }

        protected override void Generate(AlterSequenceOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            throw new NotSupportedException(Strings.SequencesNotSupported);
        }

        protected override void Generate(DropSequenceOperation operation, IModel model, RelationalCommandListBuilder builder)
        {
            throw new NotSupportedException(Strings.SequencesNotSupported);
        }

        #endregion
    }
}
