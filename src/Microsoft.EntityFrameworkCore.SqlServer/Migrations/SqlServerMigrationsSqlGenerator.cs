// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class SqlServerMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        private int _variableCounter;

        public SqlServerMigrationsSqlGenerator(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IRelationalAnnotationProvider annotations)
            : base(commandBuilderFactory, sqlGenerationHelper, typeMapper, annotations)
        {
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
            AlterColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var property = FindProperty(model, operation.Schema, operation.Table, operation.Name);
            var mightNarrowColumn = property != null
                && (property.IsKeyOrForeignKey()
                    || operation.ColumnType != null
                    || operation.IsUnicode == false
                    || operation.MaxLength.HasValue
                    || !operation.IsNullable);
            var indexesToRebuild = mightNarrowColumn
                ? property.GetContainingIndexes()
                : Enumerable.Empty<IIndex>();

            foreach (var index in indexesToRebuild)
            {
                Generate(
                    new DropIndexOperation
                    {
                        Schema = operation.Schema,
                        Table = operation.Table,
                        Name = index.SqlServer().Name
                    },
                    model,
                    builder);
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

            if (indexesToRebuild.Any())
            {
                foreach (var index in indexesToRebuild)
                {
                    Generate(
                        new CreateIndexOperation
                        {
                            IsUnique = index.IsUnique,
                            Name = index.SqlServer().Name,
                            Schema = operation.Schema,
                            Table = operation.Table,
                            Columns = index.Properties.Select(p => p.SqlServer().ColumnName).ToArray(),
                            [SqlServerFullAnnotationNames.Instance.Clustered] = index.SqlServer().IsClustered
                        },
                        model,
                        builder);
                }
            }

            EndStatement(builder);
        }

        protected override void Generate(
            RenameIndexOperation operation,
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

            Rename(qualifiedName.ToString(), operation.NewName, "INDEX", builder);
            EndStatement(builder);
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

            EndStatement(builder);
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

            EndStatement(builder);
        }

        protected override void Generate(CreateIndexOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            base.Generate(operation, model, builder, terminate: false);

            var clustered = operation[SqlServerFullAnnotationNames.Instance.Clustered] as bool?;
            if (operation.IsUnique
                && (clustered != true))
            {
                builder.Append(" WHERE ");
                for (var i = 0; i < operation.Columns.Length; i++)
                {
                    if (i != 0)
                    {
                        builder.Append(" AND ");
                    }

                    builder
                        .Append(SqlGenerationHelper.DelimitIdentifier(operation.Columns[i]))
                        .Append(" IS NOT NULL");
                }
            }

            builder.AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
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
                .Append("')")
                .AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected virtual void Generate(
            [NotNull] SqlServerCreateDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .EndCommand()
                .Append("CREATE DATABASE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: true)
                .Append("IF SERVERPROPERTY('EngineEdition') <> 5 EXEC(N'ALTER DATABASE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" SET READ_COMMITTED_SNAPSHOT ON')")
                .AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder, suppressTransaction: true);
        }

        protected virtual void Generate(
            [NotNull] SqlServerDropDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .EndCommand()
                .Append("IF SERVERPROPERTY('EngineEdition') <> 5 EXEC(N'ALTER DATABASE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" SET SINGLE_USER WITH ROLLBACK IMMEDIATE')")
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: true)
                .Append("DROP DATABASE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder, suppressTransaction: true);
        }

        protected override void Generate(
            DropIndexOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP INDEX ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" ON ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected override void Generate(
            DropColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);
            base.Generate(operation, model, builder);
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
            EndStatement(builder);
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
                .AppendLine(" + ']');");
        }
    }
}
