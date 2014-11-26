// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Sqlite.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteMigrationOperationSqlGenerator : MigrationOperationSqlGenerator
    {
        public SqliteMigrationOperationSqlGenerator(
            [NotNull] SqliteMetadataExtensionProvider extensionProvider,
            [NotNull] SqliteTypeMapper typeMapper)
            : base(extensionProvider, typeMapper)
        {
        }

        public virtual new SqliteMetadataExtensionProvider ExtensionProvider
        {
            get { return (SqliteMetadataExtensionProvider)base.ExtensionProvider; }
        }

        public override void Generate(
            CreateDatabaseOperation createDatabaseOperation,
            SqlBatchBuilder batchBuilder)
        {
            throw new NotSupportedException(Strings.MigrationOperationNotSupported(
                GetType(), createDatabaseOperation.GetType()));
        }

        public override void Generate(
            DropDatabaseOperation dropDatabaseOperation,
            SqlBatchBuilder batchBuilder)
        {
            throw new NotSupportedException(Strings.MigrationOperationNotSupported(
                GetType(), dropDatabaseOperation.GetType()));
        }

        public override void Generate(
            CreateSequenceOperation createSequenceOperation,
            SqlBatchBuilder batchBuilder)
        {
            throw new NotSupportedException(Strings.MigrationOperationNotSupported(
                GetType(), createSequenceOperation.GetType()));
        }

        public override void Generate(
            DropSequenceOperation dropSequenceOperation,
            SqlBatchBuilder batchBuilder)
        {
            throw new NotSupportedException(Strings.MigrationOperationNotSupported(
                GetType(), dropSequenceOperation.GetType()));
        }

        public override void Generate(
            MoveSequenceOperation moveSequenceOperation,
            SqlBatchBuilder batchBuilder)
        {
            throw new NotSupportedException(Strings.MigrationOperationNotSupported(
                GetType(), moveSequenceOperation.GetType()));
        }

        public override void Generate(
            RenameSequenceOperation renameSequenceOperation,
            SqlBatchBuilder batchBuilder)
        {
            throw new NotSupportedException(Strings.MigrationOperationNotSupported(
                GetType(), renameSequenceOperation.GetType()));
        }

        public override void Generate(
            AlterSequenceOperation alterSequenceOperation,
            SqlBatchBuilder batchBuilder)
        {
            throw new NotSupportedException(Strings.MigrationOperationNotSupported(
                GetType(), alterSequenceOperation.GetType()));
        }

        protected override void GenerateTableConstraints(
            CreateTableOperation createTableOperation,
            SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(createTableOperation, "createTableOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            base.GenerateTableConstraints(createTableOperation, batchBuilder);

            foreach (var addForeignKeyOperation in createTableOperation.ForeignKeys)
            {
                batchBuilder.AppendLine(",");
                GenerateForeignKey(addForeignKeyOperation, batchBuilder);
            }
        }

        public override void Generate(RenameTableOperation renameTableOperation, SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(renameTableOperation, "renameTableOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            GenerateRenameTable(
                renameTableOperation.TableName,
                new SchemaQualifiedName(renameTableOperation.NewTableName, renameTableOperation.TableName.Schema),
                batchBuilder);
        }

        public override void Generate(MoveTableOperation moveTableOperation, SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(moveTableOperation, "moveTableOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            GenerateRenameTable(
                moveTableOperation.TableName,
                new SchemaQualifiedName(moveTableOperation.TableName.Name, moveTableOperation.NewSchema),
                batchBuilder);
        }

        protected virtual void GenerateRenameTable(
            [NotNull] SchemaQualifiedName tableName,
            [NotNull] SchemaQualifiedName newTableName,
            [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(tableName, "tableName");
            Check.NotNull(newTableName, "newTableName");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(tableName))
                .Append(" RENAME TO ")
                .Append(DelimitIdentifier(newTableName));
        }

        public override void Generate(DropColumnOperation dropColumnOperation, SqlBatchBuilder batchBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(AlterColumnOperation alterColumnOperation, SqlBatchBuilder batchBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            AddDefaultConstraintOperation addDefaultConstraintOperation,
            SqlBatchBuilder batchBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            DropDefaultConstraintOperation dropDefaultConstraintOperation,
            SqlBatchBuilder batchBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            RenameColumnOperation renameColumnOperation, 
            SqlBatchBuilder batchBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            AddPrimaryKeyOperation addPrimaryKeyOperation,
            SqlBatchBuilder batchBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            DropPrimaryKeyOperation dropPrimaryKeyOperation,
            SqlBatchBuilder batchBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            AddForeignKeyOperation addForeignKeyOperation,
            SqlBatchBuilder batchBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            DropForeignKeyOperation dropForeignKeyOperation,
            SqlBatchBuilder batchBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            RenameIndexOperation renameIndexOperation, 
            SqlBatchBuilder batchBuilder)
        {
            // TODO: Rebuild index
            throw new NotImplementedException();
        }

        public override string GenerateLiteral(byte[] value)
        {
            Check.NotNull(value, "value");

            var stringBuilder = new StringBuilder("X'");

            foreach (var @byte in value)
            {
                stringBuilder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
            }

            stringBuilder.Append("'");

            return stringBuilder.ToString();
        }

        public override string DelimitIdentifier(SchemaQualifiedName schemaQualifiedName)
        {
            return DelimitIdentifier(
                (schemaQualifiedName.IsSchemaQualified
                    ? schemaQualifiedName.Schema + "."
                    : string.Empty)
                + schemaQualifiedName.Name);
        }

        protected override void GenerateUniqueConstraint(
            AddUniqueConstraintOperation uniqueConstraintOperation,
            SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(uniqueConstraintOperation, "uniqueConstraintOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("UNIQUE (")
                .Append(uniqueConstraintOperation.ColumnNames.Select(DelimitIdentifier).Join())
                .Append(")");
        }
    }
}
