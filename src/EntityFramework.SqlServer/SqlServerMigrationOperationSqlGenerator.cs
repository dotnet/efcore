// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerMigrationOperationSqlGenerator : MigrationOperationSqlGenerator
    {
        private int _variableCount;
        private readonly HashSet<string> _generatedSchemas = new HashSet<string>();
 
        public SqlServerMigrationOperationSqlGenerator(
            [NotNull] SqlServerMetadataExtensionProvider extensionProvider,
            [NotNull] SqlServerTypeMapper typeMapper)
            : base(extensionProvider, typeMapper)
        {
        }

        protected HashSet<string> GeneratedSchemas
        {
            get { return _generatedSchemas; }
        }

        public override void Generate(CreateDatabaseOperation createDatabaseOperation, SqlBatchBuilder batchBuilder)
        {
            base.Generate(createDatabaseOperation, batchBuilder);

            batchBuilder.EndBatch();

            batchBuilder
                .Append("IF SERVERPROPERTY('EngineEdition') <> 5 EXECUTE sp_executesql N")
                .Append(
                    GenerateLiteral(
                        string.Concat(
                            "ALTER DATABASE ",
                            DelimitIdentifier(createDatabaseOperation.DatabaseName),
                            " SET READ_COMMITTED_SNAPSHOT ON")));

            batchBuilder.EndBatch();
        }

        public override void Generate(DropDatabaseOperation dropDatabaseOperation, SqlBatchBuilder batchBuilder)
        {
            batchBuilder
                .Append("IF SERVERPROPERTY('EngineEdition') <> 5 EXECUTE sp_executesql N")
                .Append(
                    GenerateLiteral(
                        string.Concat(
                            "ALTER DATABASE ",
                            DelimitIdentifier(dropDatabaseOperation.DatabaseName),
                            " SET SINGLE_USER WITH ROLLBACK IMMEDIATE")));

            batchBuilder.EndBatch();

            base.Generate(dropDatabaseOperation, batchBuilder);

            batchBuilder.EndBatch();
        }

        public override void Generate(RenameSequenceOperation renameSequenceOperation, SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(renameSequenceOperation, "renameSequenceOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            GenerateRename(
                renameSequenceOperation.SequenceName,
                renameSequenceOperation.NewSequenceName,
                "OBJECT",
                batchBuilder);
        }

        public override void Generate(MoveSequenceOperation moveSequenceOperation, SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(moveSequenceOperation, "moveSequenceOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            GenerateMove(
                moveSequenceOperation.SequenceName,
                moveSequenceOperation.NewSchema,
                batchBuilder);
        }

        public override void Generate(RenameTableOperation renameTableOperation, SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(renameTableOperation, "renameTableOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            GenerateRename(
                renameTableOperation.TableName,
                renameTableOperation.NewTableName,
                "OBJECT",
                batchBuilder);
        }

        public override void Generate(MoveTableOperation moveTableOperation, SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(moveTableOperation, "moveTableOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            GenerateMove(
                moveTableOperation.TableName,
                moveTableOperation.NewSchema,
                batchBuilder);
        }

        public override void Generate(AddDefaultConstraintOperation addDefaultConstraintOperation, SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(addDefaultConstraintOperation, "addDefaultConstraintOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            var tableName = addDefaultConstraintOperation.TableName;
            var columnName = addDefaultConstraintOperation.ColumnName;

            batchBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(tableName))
                .Append(" ADD CONSTRAINT ")
                .Append(DelimitIdentifier("DF_" + tableName + "_" + columnName))
                .Append(" DEFAULT ");

            batchBuilder.Append(addDefaultConstraintOperation.DefaultSql ?? GenerateLiteral((dynamic)addDefaultConstraintOperation.DefaultValue));

            batchBuilder
                .Append(" FOR ")
                .Append(DelimitIdentifier(columnName));
        }

        public override void Generate(DropDefaultConstraintOperation dropDefaultConstraintOperation, SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(dropDefaultConstraintOperation, "dropDefaultConstraintOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            var constraintNameVariable = "@var" + _variableCount++;

            batchBuilder
                .Append("DECLARE ")
                .Append(constraintNameVariable)
                .AppendLine(" nvarchar(128)");

            batchBuilder
                .Append("SELECT ")
                .Append(constraintNameVariable)
                .Append(" = name FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N")
                .Append(GenerateLiteral(dropDefaultConstraintOperation.TableName))
                .Append(") AND COL_NAME(parent_object_id, parent_column_id) = N")
                .AppendLine(GenerateLiteral(dropDefaultConstraintOperation.ColumnName));

            batchBuilder
                .Append("EXECUTE('ALTER TABLE ")
                .Append(DelimitIdentifier(dropDefaultConstraintOperation.TableName))
                .Append(" DROP CONSTRAINT \"' + ")
                .Append(constraintNameVariable)
                .Append(" + '\"')");
        }

        public override void Generate(RenameColumnOperation renameColumnOperation, SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(renameColumnOperation, "renameColumnOperation");

            GenerateRename(
                string.Concat(
                    EscapeLiteral(renameColumnOperation.TableName),
                    ".",
                    EscapeLiteral(renameColumnOperation.ColumnName)),
                renameColumnOperation.NewColumnName,
                "COLUMN",
                batchBuilder);
        }

        public override void Generate(RenameIndexOperation renameIndexOperation, SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(renameIndexOperation, "renameIndexOperation");

            batchBuilder
                .Append("EXECUTE sp_rename @objname = N'")
                .Append(EscapeLiteral(renameIndexOperation.TableName))
                .Append(".")
                .Append(EscapeLiteral(renameIndexOperation.IndexName))
                .Append("', @newname = N")
                .Append(GenerateLiteral(renameIndexOperation.NewIndexName))
                .Append(", @objtype = N'INDEX'");
        }

        public override string DelimitIdentifier(string identifier)
        {
            Check.NotEmpty(identifier, "identifier");

            return "[" + EscapeIdentifier(identifier) + "]";
        }

        public override string EscapeIdentifier(string identifier)
        {
            Check.NotEmpty(identifier, "identifier");

            return identifier.Replace("]", "]]");
        }

        protected override void GenerateComputedColumn(SchemaQualifiedName tableName,
            Column column, SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(column, "column");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append(DelimitIdentifier(column.Name))
                .Append(" AS ")
                .Append(column.DefaultSql);

            var entityType = TargetModel.EntityTypes.Single(t => NameBuilder.SchemaQualifiedTableName(t) == tableName);
            var property = entityType.Properties.Single(p => NameBuilder.ColumnName(p) == column.Name);

            if (!property.IsNullable
                || property.IsForeignKey())
            {
                // TODO: Consider adding support for PERSISTED to SqlServer metadata extensions.
                batchBuilder.Append(" PERSISTED");
            }

            GenerateNullConstraint(tableName, column, batchBuilder);
        }

        protected override void GenerateColumnTraits(
            SchemaQualifiedName tableName, 
            Column column, 
            SqlBatchBuilder batchBuilder)
       {
            if (column.IsIdentity)
            {
                batchBuilder.Append(" IDENTITY");
            }
        }

        protected override void GeneratePrimaryKeyTraits(
            AddPrimaryKeyOperation primaryKeyOperation,
            SqlBatchBuilder batchBuilder)
        {
            if (!primaryKeyOperation.IsClustered)
            {
                batchBuilder.Append(" NONCLUSTERED");
            }
        }

        public override void Generate(DropIndexOperation dropIndexOperation, SqlBatchBuilder batchBuilder)
        {
            base.Generate(dropIndexOperation, batchBuilder);

            batchBuilder
                .Append(" ON ")
                .Append(DelimitIdentifier(dropIndexOperation.TableName));
        }

        protected override void EnsureSchema(string schema, SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(batchBuilder, "batchBuilder");

            if (string.IsNullOrEmpty(schema)
                || schema.Equals("dbo", StringComparison.OrdinalIgnoreCase)
                || GeneratedSchemas.Contains(schema))
            {
                return;
            }

            batchBuilder
                .Append("IF schema_id(")
                .Append(GenerateLiteral(schema))
                .AppendLine(") IS NULL");

            using (batchBuilder.Indent())
            {
                CreateSchema(schema, batchBuilder);
            }

            batchBuilder.AppendLine();

            GeneratedSchemas.Add(schema);
        }

        protected virtual void GenerateRename(
            [NotNull] string name, 
            [NotNull] string newName,
            [NotNull] string objectType,
            [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(newName, "newName");
            Check.NotEmpty(objectType, "objectType");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("EXECUTE sp_rename @objname = N")
                .Append(GenerateLiteral(name))
                .Append(", @newname = N")
                .Append(GenerateLiteral(newName))
                .Append(", @objtype = N")
                .Append(GenerateLiteral(objectType));
        }

        protected virtual void GenerateMove(
            SchemaQualifiedName name, 
            [NotNull] string newSchema,
            [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotEmpty(newSchema, "newSchema");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("ALTER SCHEMA ")
                .Append(DelimitIdentifier(newSchema))
                .Append(" TRANSFER ")
                .Append(DelimitIdentifier(name));
        }
    }
}
