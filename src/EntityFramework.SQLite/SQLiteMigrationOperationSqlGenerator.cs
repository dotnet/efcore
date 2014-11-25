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
            IndentedStringBuilder stringBuilder)
        {
            throw new NotSupportedException(Strings.MigrationOperationNotSupported(
                GetType(), createDatabaseOperation.GetType()));
        }

        public override void Generate(
            DropDatabaseOperation dropDatabaseOperation,
            IndentedStringBuilder stringBuilder)
        {
            throw new NotSupportedException(Strings.MigrationOperationNotSupported(
                GetType(), dropDatabaseOperation.GetType()));
        }

        public override void Generate(
            CreateSequenceOperation createSequenceOperation,
            IndentedStringBuilder stringBuilder)
        {
            throw new NotSupportedException(Strings.MigrationOperationNotSupported(
                GetType(), createSequenceOperation.GetType()));
        }

        public override void Generate(
            DropSequenceOperation dropSequenceOperation,
            IndentedStringBuilder stringBuilder)
        {
            throw new NotSupportedException(Strings.MigrationOperationNotSupported(
                GetType(), dropSequenceOperation.GetType()));
        }

        public override void Generate(
            MoveSequenceOperation moveSequenceOperation,
            IndentedStringBuilder stringBuilder)
        {
            throw new NotSupportedException(Strings.MigrationOperationNotSupported(
                GetType(), moveSequenceOperation.GetType()));
        }

        public override void Generate(
            RenameSequenceOperation renameSequenceOperation,
            IndentedStringBuilder stringBuilder)
        {
            throw new NotSupportedException(Strings.MigrationOperationNotSupported(
                GetType(), renameSequenceOperation.GetType()));
        }

        public override void Generate(
            AlterSequenceOperation alterSequenceOperation,
            IndentedStringBuilder stringBuilder)
        {
            throw new NotSupportedException(Strings.MigrationOperationNotSupported(
                GetType(), alterSequenceOperation.GetType()));
        }

        protected override void GenerateTableConstraints(
            CreateTableOperation createTableOperation,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(createTableOperation, "createTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            base.GenerateTableConstraints(createTableOperation, stringBuilder);

            foreach (var addForeignKeyOperation in createTableOperation.ForeignKeys)
            {
                stringBuilder.AppendLine(",");
                GenerateForeignKey(addForeignKeyOperation, stringBuilder);
            }
        }

        public override void Generate(RenameTableOperation renameTableOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(renameTableOperation, "renameTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            GenerateRenameTable(
                renameTableOperation.TableName,
                new SchemaQualifiedName(renameTableOperation.NewTableName, renameTableOperation.TableName.Schema),
                stringBuilder);
        }

        public override void Generate(MoveTableOperation moveTableOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(moveTableOperation, "moveTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            GenerateRenameTable(
                moveTableOperation.TableName,
                new SchemaQualifiedName(moveTableOperation.TableName.Name, moveTableOperation.NewSchema),
                stringBuilder);
        }

        protected virtual void GenerateRenameTable(
            [NotNull] SchemaQualifiedName tableName,
            [NotNull] SchemaQualifiedName newTableName,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(tableName, "tableName");
            Check.NotNull(newTableName, "newTableName");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(tableName))
                .Append(" RENAME TO ")
                .Append(DelimitIdentifier(newTableName));
        }

        public override void Generate(DropColumnOperation dropColumnOperation, IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(AlterColumnOperation alterColumnOperation, IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            AddDefaultConstraintOperation addDefaultConstraintOperation,
            IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            DropDefaultConstraintOperation dropDefaultConstraintOperation,
            IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(RenameColumnOperation renameColumnOperation, IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            AddPrimaryKeyOperation addPrimaryKeyOperation,
            IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            DropPrimaryKeyOperation dropPrimaryKeyOperation,
            IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            AddForeignKeyOperation addForeignKeyOperation,
            IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            DropForeignKeyOperation dropForeignKeyOperation,
            IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(RenameIndexOperation renameIndexOperation, IndentedStringBuilder stringBuilder)
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
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(uniqueConstraintOperation, "uniqueConstraintOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("UNIQUE (")
                .Append(uniqueConstraintOperation.ColumnNames.Select(DelimitIdentifier).Join())
                .Append(")");
        }
    }
}
