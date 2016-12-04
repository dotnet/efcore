// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SqliteMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        public SqliteMigrationsSqlGenerator(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IRelationalAnnotationProvider annotations)
            : base(commandBuilderFactory, sqlGenerationHelper, typeMapper, annotations)
        {
        }

        public override IReadOnlyList<MigrationCommand> Generate(IReadOnlyList<MigrationOperation> operations, IModel model = null)
            => base.Generate(LiftForeignKeyOperations(operations), model);

        private static IReadOnlyList<MigrationOperation> LiftForeignKeyOperations(IReadOnlyList<MigrationOperation> migrationOperations)
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

        protected override void Generate(DropIndexOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP INDEX ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .EndCommand();
        }

        protected override void Generate(RenameTableOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.NewName != null)
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" RENAME TO ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                    .AppendLine(SqlGenerationHelper.StatementTerminator)
                    .EndCommand();
            }
        }

        protected override void Generate(CreateTableOperation operation, IModel model, MigrationCommandListBuilder builder)
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
                    columnOp.AddAnnotation(SqliteFullAnnotationNames.Instance.InlinePrimaryKey, true);
                    if (!string.IsNullOrEmpty(operation.PrimaryKey.Name))
                    {
                        columnOp.AddAnnotation(SqliteFullAnnotationNames.Instance.InlinePrimaryKeyName, operation.PrimaryKey.Name);
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
            base.ColumnDefinition(
                schema, table, name, clrType, type, unicode, maxLength, rowVersion, nullable,
                defaultValue, defaultValueSql, computedColumnSql, annotatable, model, builder);

            var inlinePk = annotatable[SqliteFullAnnotationNames.Instance.InlinePrimaryKey] as bool?;
            if (inlinePk == true)
            {
                var inlinePkName = annotatable[
                    SqliteFullAnnotationNames.Instance.InlinePrimaryKeyName] as string;
                if (!string.IsNullOrEmpty(inlinePkName))
                {
                    builder
                        .Append(" CONSTRAINT ")
                        .Append(SqlGenerationHelper.DelimitIdentifier(inlinePkName));
                }
                builder.Append(" PRIMARY KEY");
                var autoincrement = annotatable[SqliteFullAnnotationNames.Instance.Autoincrement] as bool?
                    // NB: Migrations scaffolded with version 1.0.0 don't have the prefix. See #6461
                    ?? annotatable[SqliteAnnotationNames.Autoincrement] as bool?;
                if (autoincrement == true)
                {
                    builder.Append(" AUTOINCREMENT");
                }
            }
        }

        #region Invalid migration operations

        // These operations can be accomplished instead with a table-rebuild
        protected override void Generate(AddForeignKeyOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            throw new NotSupportedException(SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));
        }

        protected override void Generate(AddPrimaryKeyOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            throw new NotSupportedException(SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));
        }

        protected override void Generate(AddUniqueConstraintOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            throw new NotSupportedException(SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));
        }

        protected override void Generate(DropColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            throw new NotSupportedException(SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));
        }

        protected override void Generate(DropForeignKeyOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            throw new NotSupportedException(SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));
        }

        protected override void Generate(DropPrimaryKeyOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            throw new NotSupportedException(SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));
        }

        protected override void Generate(DropUniqueConstraintOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            throw new NotSupportedException(SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));
        }

        protected override void Generate(RenameColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            throw new NotSupportedException(SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));
        }

        protected override void Generate(RenameIndexOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            throw new NotSupportedException(SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));
        }

        protected override void Generate(AlterColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            throw new NotSupportedException(SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));
        }

        #endregion

        #region Invalid schema operations

        protected override void Generate(EnsureSchemaOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            throw new NotSupportedException(SqliteStrings.SchemasNotSupported);
        }

        protected override void Generate(DropSchemaOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            throw new NotSupportedException(SqliteStrings.SchemasNotSupported);
        }

        #endregion

        #region Sequences not supported

        // SQLite does not have sequences
        protected override void Generate(RestartSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            throw new NotSupportedException(SqliteStrings.SequencesNotSupported);
        }

        protected override void Generate(CreateSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            throw new NotSupportedException(SqliteStrings.SequencesNotSupported);
        }

        protected override void Generate(RenameSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            throw new NotSupportedException(SqliteStrings.SequencesNotSupported);
        }

        protected override void Generate(AlterSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            throw new NotSupportedException(SqliteStrings.SequencesNotSupported);
        }

        protected override void Generate(DropSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            throw new NotSupportedException(SqliteStrings.SequencesNotSupported);
        }

        #endregion
    }
}
