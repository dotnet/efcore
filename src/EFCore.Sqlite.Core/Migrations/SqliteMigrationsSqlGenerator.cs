// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     SQLite-specific implementation of <see cref="MigrationsSqlGenerator" />.
    /// </summary>
    public class SqliteMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        private readonly IMigrationsAnnotationProvider _migrationsAnnotations;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        /// <param name="migrationsAnnotations"> Provider-specific Migrations annotations to use. </param>
        public SqliteMigrationsSqlGenerator(
            [NotNull] MigrationsSqlGeneratorDependencies dependencies,
            [NotNull] IMigrationsAnnotationProvider migrationsAnnotations)
            : base(dependencies)
            => _migrationsAnnotations = migrationsAnnotations;

        /// <summary>
        ///     Generates commands from a list of operations.
        /// </summary>
        /// <param name="operations"> The operations. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <returns> The list of commands to be executed or scripted. </returns>
        public override IReadOnlyList<MigrationCommand> Generate(IReadOnlyList<MigrationOperation> operations, IModel model = null)
            => base.Generate(RewriteOperations(operations, model), model);

        private bool IsSpatialiteColumn(AddColumnOperation operation, IModel model)
            => SqliteTypeMappingSource.IsSpatialiteType(
                operation.ColumnType
                    ?? GetColumnType(
                        operation.Schema,
                        operation.Table,
                        operation.Name,
                        operation.ClrType,
                        operation.IsUnicode,
                        operation.MaxLength,
                        operation.IsFixedLength,
                        operation.IsRowVersion,
                        model));

        private IReadOnlyList<MigrationOperation> RewriteOperations(
            IReadOnlyList<MigrationOperation> migrationOperations,
            IModel model)
        {
            var operations = new List<MigrationOperation>();
            foreach (var operation in migrationOperations)
            {
                if (operation is AddForeignKeyOperation foreignKeyOperation)
                {
                    var table = migrationOperations
                        .OfType<CreateTableOperation>()
                        .FirstOrDefault(o => o.Name == foreignKeyOperation.Table);

                    if (table != null)
                    {
                        table.ForeignKeys.Add(foreignKeyOperation);
                    }
                    else
                    {
                        operations.Add(operation);
                    }
                }
                else if (operation is CreateTableOperation createTableOperation)
                {
                    var spatialiteColumns = new Stack<AddColumnOperation>();
                    for (var i = createTableOperation.Columns.Count - 1; i >= 0; i--)
                    {
                        var addColumnOperation = createTableOperation.Columns[i];

                        if (IsSpatialiteColumn(addColumnOperation, model))
                        {
                            spatialiteColumns.Push(addColumnOperation);
                            createTableOperation.Columns.RemoveAt(i);
                        }
                    }

                    operations.Add(operation);
                    operations.AddRange(spatialiteColumns);
                }
                else
                {
                    operations.Add(operation);
                }
            }

            return operations;
        }

        /// <summary>
        ///     Builds commands for the given <see cref="AlterDatabaseOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(AlterDatabaseOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            if (operation[SqliteAnnotationNames.InitSpatialMetaData] as bool? != true
                || operation.OldDatabase[SqliteAnnotationNames.InitSpatialMetaData] as bool? == true)
            {
                return;
            }

            builder
                .Append("SELECT InitSpatialMetaData()")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }

        /// <summary>
        ///     Builds commands for the given <see cref="AddColumnOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(AddColumnOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate)
        {
            if (!IsSpatialiteColumn(operation, model))
            {
                base.Generate(operation, model, builder, terminate);

                return;
            }

            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));
            var longTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(long));

            var srid = operation[SqliteAnnotationNames.Srid] as int? ?? 0;
            var dimension = operation[SqliteAnnotationNames.Dimension] as string;

            var geometryType = operation.ColumnType
                ?? GetColumnType(
                    operation.Schema,
                    operation.Table,
                    operation.Name,
                    operation.ClrType,
                    operation.IsUnicode,
                    operation.MaxLength,
                    operation.IsFixedLength,
                    operation.IsRowVersion,
                    model);
            if (!string.IsNullOrEmpty(dimension))
            {
                geometryType += dimension;
            }

            builder
                .Append("SELECT AddGeometryColumn(")
                .Append(stringTypeMapping.GenerateSqlLiteral(operation.Table))
                .Append(", ")
                .Append(stringTypeMapping.GenerateSqlLiteral(operation.Name))
                .Append(", ")
                .Append(longTypeMapping.GenerateSqlLiteral(srid))
                .Append(", ")
                .Append(stringTypeMapping.GenerateSqlLiteral(geometryType))
                .Append(", -1, ")
                .Append(operation.IsNullable ? "0" : "1")
                .Append(")");

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
            else
            {
                Debug.Fail("I have a bad feeling about this. Geometry columns don't compose well.");
            }
        }

        /// <summary>
        ///     Builds commands for the given <see cref="DropIndexOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(DropIndexOperation operation, IModel model, MigrationCommandListBuilder builder)
            => Generate(operation, model, builder, terminate: true);

        /// <summary>
        ///     Builds commands for the given <see cref="DropIndexOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected virtual void Generate(
            [NotNull] DropIndexOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP INDEX ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (terminate)
            {
                builder
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                    .EndCommand();
            }
        }

        /// <summary>
        ///     Builds commands for the given <see cref="RenameIndexOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(RenameIndexOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            var index = FindEntityTypes(model, operation.Schema, operation.Table)
                ?.SelectMany(t => t.GetDeclaredIndexes()).Where(i => i.Relational().Name == operation.NewName)
                .FirstOrDefault();
            if (index == null)
            {
                throw new NotSupportedException(
                    SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));
            }

            var dropOperation = new DropIndexOperation
            {
                Schema = operation.Schema,
                Table = operation.Table,
                Name = operation.Name
            };
            dropOperation.AddAnnotations(_migrationsAnnotations.ForRemove(index));

            var createOperation = new CreateIndexOperation
            {
                IsUnique = index.IsUnique,
                Name = operation.NewName,
                Schema = operation.Schema,
                Table = operation.Table,
                Columns = index.Properties.Select(p => p.Relational().ColumnName).ToArray(),
                Filter = index.Relational().Filter
            };
            createOperation.AddAnnotations(_migrationsAnnotations.For(index));

            Generate(dropOperation, model, builder, terminate: false);
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            Generate(createOperation, model, builder);
        }

        /// <summary>
        ///     Builds commands for the given <see cref="RenameTableOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(RenameTableOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.NewName != null
                && operation.NewName != operation.Name)
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" RENAME TO ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                    .EndCommand();
            }
        }

        /// <summary>
        ///     Builds commands for the given <see cref="RenameTableOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(RenameColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table))
                .Append(" RENAME COLUMN ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" TO ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand();
        }

        /// <summary>
        ///     Builds commands for the given <see cref="CreateTableOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
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
                    columnOp.AddAnnotation(SqliteAnnotationNames.InlinePrimaryKey, true);
                    if (!string.IsNullOrEmpty(operation.PrimaryKey.Name))
                    {
                        columnOp.AddAnnotation(SqliteAnnotationNames.InlinePrimaryKeyName, operation.PrimaryKey.Name);
                    }

                    operation.PrimaryKey = null;
                }
            }

            base.Generate(operation, model, builder);
        }

        /// <summary>
        ///     Generates a SQL fragment for a column definition in an <see cref="AddColumnOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void ColumnDefinition(AddColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
            => ColumnDefinition(
                operation.Schema,
                operation.Table,
                operation.Name,
                operation.ClrType,
                operation.ColumnType,
                operation.IsUnicode,
                operation.MaxLength,
                operation.IsFixedLength,
                operation.IsRowVersion,
                operation.IsNullable,
                operation.DefaultValue,
                operation.DefaultValueSql,
                operation.ComputedColumnSql,
                operation,
                model,
                builder);

        /// <summary>
        ///     Generates a SQL fragment for a column definition for the given column metadata.
        /// </summary>
        /// <param name="schema"> The schema that contains the table, or <c>null</c> to use the default schema. </param>
        /// <param name="table"> The table that contains the column. </param>
        /// <param name="name"> The column name. </param>
        /// <param name="clrType"> The CLR <see cref="Type" /> that the column is mapped to. </param>
        /// <param name="type"> The database/store type for the column, or <c>null</c> if none has been specified. </param>
        /// <param name="unicode">
        ///     Indicates whether or not the column can contain Unicode data, or <c>null</c> if this is not applicable or not specified.
        /// </param>
        /// <param name="maxLength">
        ///     The maximum amount of data that the column can contain, or <c>null</c> if this is not applicable or not specified.
        /// </param>
        /// <param name="fixedLength"> Indicates whether or not the column is constrained to fixed-length data. </param>
        /// <param name="rowVersion">
        ///     Indicates whether or not this column is an automatic concurrency token, such as a SQL Server timestamp/rowversion.
        /// </param>
        /// <param name="nullable"> Indicates whether or not the column can store <c>NULL</c> values. </param>
        /// <param name="defaultValue"> The default value for the column. </param>
        /// <param name="defaultValueSql"> The SQL expression to use for the column's default constraint. </param>
        /// <param name="computedColumnSql"> The SQL expression to use to compute the column value. </param>
        /// <param name="annotatable"> The <see cref="MigrationOperation" /> to use to find any custom annotations. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void ColumnDefinition(
            string schema,
            string table,
            string name,
            Type clrType,
            string type,
            bool? unicode,
            int? maxLength,
            bool? fixedLength,
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
                schema, table, name, clrType, type, unicode, maxLength, fixedLength, rowVersion, nullable,
                defaultValue, defaultValueSql, computedColumnSql, annotatable, model, builder);

            var inlinePk = annotatable[SqliteAnnotationNames.InlinePrimaryKey] as bool?;
            if (inlinePk == true)
            {
                var inlinePkName = annotatable[
                    SqliteAnnotationNames.InlinePrimaryKeyName] as string;
                if (!string.IsNullOrEmpty(inlinePkName))
                {
                    builder
                        .Append(" CONSTRAINT ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(inlinePkName));
                }

                builder.Append(" PRIMARY KEY");
                var autoincrement = annotatable[SqliteAnnotationNames.Autoincrement] as bool?
                                    // NB: Migrations scaffolded with version 1.0.0 don't have the prefix. See #6461
                                    ?? annotatable[SqliteAnnotationNames.LegacyAutoincrement] as bool?;
                if (autoincrement == true)
                {
                    builder.Append(" AUTOINCREMENT");
                }
            }
        }

        #region Invalid migration operations

        /// <summary>
        ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
        ///     are not yet supported.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(AddForeignKeyOperation operation, IModel model, MigrationCommandListBuilder builder)
            => throw new NotSupportedException(
                SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

        /// <summary>
        ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
        ///     are not yet supported.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(AddPrimaryKeyOperation operation, IModel model, MigrationCommandListBuilder builder)
            => throw new NotSupportedException(
                SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

        /// <summary>
        ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
        ///     are not yet supported.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(AddUniqueConstraintOperation operation, IModel model, MigrationCommandListBuilder builder)
            => throw new NotSupportedException(
                SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

        /// <summary>
        ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
        ///     are not yet supported.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(DropColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
            => throw new NotSupportedException(
                SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

        /// <summary>
        ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
        ///     are not yet supported.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(DropForeignKeyOperation operation, IModel model, MigrationCommandListBuilder builder)
            => throw new NotSupportedException(
                SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

        /// <summary>
        ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
        ///     are not yet supported.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(DropPrimaryKeyOperation operation, IModel model, MigrationCommandListBuilder builder)
            => throw new NotSupportedException(
                SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

        /// <summary>
        ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
        ///     are not yet supported.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(DropUniqueConstraintOperation operation, IModel model, MigrationCommandListBuilder builder)
            => throw new NotSupportedException(
                SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

        /// <summary>
        ///     Throws <see cref="NotSupportedException" /> since this operation requires table rebuilds, which
        ///     are not yet supported.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(AlterColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
            => throw new NotSupportedException(
                SqliteStrings.InvalidMigrationOperation(operation.GetType().ShortDisplayName()));

        #endregion

        #region Ignored schema operations

        /// <summary>
        ///     Ignored, since schemas are not supported by SQLite and are silently ignored to improve testing compatibility.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(EnsureSchemaOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
        }

        /// <summary>
        ///     Ignored, since schemas are not supported by SQLite and are silently ignored to improve testing compatibility.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(DropSchemaOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
        }

        #endregion

        #region Sequences not supported

        /// <summary>
        ///     Throws <see cref="NotSupportedException" /> since SQLite does not support sequences.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(RestartSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
            => throw new NotSupportedException(SqliteStrings.SequencesNotSupported);

        /// <summary>
        ///     Throws <see cref="NotSupportedException" /> since SQLite does not support sequences.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(CreateSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
            => throw new NotSupportedException(SqliteStrings.SequencesNotSupported);

        /// <summary>
        ///     Throws <see cref="NotSupportedException" /> since SQLite does not support sequences.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(RenameSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
            => throw new NotSupportedException(SqliteStrings.SequencesNotSupported);

        /// <summary>
        ///     Throws <see cref="NotSupportedException" /> since SQLite does not support sequences.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(AlterSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
            => throw new NotSupportedException(SqliteStrings.SequencesNotSupported);

        /// <summary>
        ///     Throws <see cref="NotSupportedException" /> since SQLite does not support sequences.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <c>null</c> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(DropSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
            => throw new NotSupportedException(SqliteStrings.SequencesNotSupported);

        #endregion
    }
}
